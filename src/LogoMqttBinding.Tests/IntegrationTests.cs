﻿using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LogoMqttBinding.Configuration;
using LogoMqttBindingTests.Infrastructure;
using MQTTnet;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Unsubscribing;
using Xunit;

namespace LogoMqttBindingTests
{
  [Collection(nameof(IntegrationTestEnvironment))]
  public class IntegrationTests
  {
    private readonly IntegrationTestEnvironment testEnvironment;
    public IntegrationTests(IntegrationTestEnvironment testEnvironment) => this.testEnvironment = testEnvironment;



    [Theory]
    [InlineData(0, "get/integer/at/0", 7)]
    [InlineData(0, "get/integer/at/0", 42)]
    [InlineData(17, "get/integer/at/17", 1337)]
    public async Task ChangedValueInLogo_Integer_TriggersMqttWithCorrectValue(int logoAddress, string mqttTopic, short value)
    {
      MqttApplicationMessageReceivedEventArgs? receivedMessage = null;
      testEnvironment.MqttMessageReceived += (s, e) => receivedMessage = e;

      await testEnvironment.MqttClient!.SubscribeAsync(new MqttClientSubscribeOptionsBuilder().WithTopicFilter(mqttTopic).Build(), CancellationToken.None);

      testEnvironment.LogoHardwareMock!.WriteInteger(logoAddress, value);
      await Task.Delay(250).ConfigureAwait(false); // let cache update, detect change and publish

      await testEnvironment.MqttClient.UnsubscribeAsync(new MqttClientUnsubscribeOptionsBuilder().WithTopicFilter(mqttTopic).Build(), CancellationToken.None);

      receivedMessage.Should().NotBeNull();
      var receivedString = Encoding.UTF8.GetString(receivedMessage!.ApplicationMessage.Payload);
      var actualValue = short.Parse(receivedString);
      actualValue.Should().Be(value);
    }

    [Theory]
    [InlineData(5, "set/integer/at/5", 7)]
    [InlineData(5, "set/integer/at/5", 42)]
    [InlineData(25, "set/integer/at/25", 1337)]
    public async Task SetValueFromMqtt_Integer_UpdatesLogoWithCorrectValue(int logoAddress, string mqttTopic, short value)
    {
      var payload = Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture));

      await testEnvironment.MqttClient!.PublishAsync(
        new MqttApplicationMessageBuilder()
          .WithTopic(mqttTopic)
          .WithPayload(payload)
          .Build(),
        CancellationToken.None);

      await Task.Delay(100).ConfigureAwait(false);

      var actualValue = testEnvironment.LogoHardwareMock!.ReadInteger(logoAddress);

      actualValue.Should().Be(value);
    }

    //TODO: test: subscribe / connect / enforce reconnect -> change value should only notify once

    internal static Config GetConfig(string brokerUri, int brokerPort)
    {
      return new Config
      {
        MqttBrokerUri = brokerUri,
        MqttBrokerPort = brokerPort,
        Logos = new[]
        {
          new LogoConfig
          {
            IpAddress = IPAddress.Loopback.ToString(),
            MemoryRanges = new[]
            {
              new MemoryRangeConfig
              {
                LocalVariableMemoryStart = 0,
                LocalVariableMemoryEnd = 850,
                LocalVariableMemoryPollingCycleMilliseconds = 100,
              },
            },
            Mqtt = new[]
            {
              new MqttDevice
              {
                ClientId = "mqttClient",

                Subscribed = new[]
                {
                  new MqttChannel
                  {
                    Topic = "set/integer/at/5",
                    LogoAddress = 5,
                    Type = "integer",
                  },
                  new MqttChannel
                  {
                    Topic = "set/integer/at/25",
                    LogoAddress = 25,
                    Type = "integer",
                  },
                },

                Published = new[]
                {
                  new MqttChannel
                  {
                    Topic = "get/integer/at/0",
                    LogoAddress = 0,
                    Type = "integer",
                  },
                  new MqttChannel
                  {
                    Topic = "get/integer/at/17",
                    LogoAddress = 17,
                    Type = "integer",
                  },
                },
              },
            },
          },
        },
      };
    }
  }
}