﻿using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LogoMqttBinding.Configuration;
using Xunit;

namespace LogoMqttBinding.Tests
{
  public class ConfigTests
  {
    [Fact]
    public void Validate_Uninitialized_ShouldPass()
    {
      var config = new Config();
      config.Validate();
    }

    [Fact]
    public void Validate_InvalidBrokerUri_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some\\where"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 100,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(Config.MqttBrokerIpAddress));
      ex.ActualValue.Should().Be("some\\where");
      ex.Message.Should().Contain("'some\\where' should be a valid URI");
    }

    [Fact]
    public void Validate_BrokerPortBelowRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""-1"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 100,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(Config.MqttBrokerPort));
      ex.ActualValue.Should().Be(-1);
      ex.Message.Should().Contain("'-1' should be a valid port");
    }

    [Fact]
    public void Validate_BrokerPortAboveRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""65536"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 100,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(Config.MqttBrokerPort));
      ex.ActualValue.Should().Be(65536);
      ex.Message.Should().Contain("'65536' should be a valid port");
    }

    [Fact]
    public void Validate_InvalidLogoIpAddress_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4.5"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 100,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(LogoConfig.IpAddress));
      ex.ActualValue.Should().Be("1.2.3.4.5");
      ex.Message.Should().Contain("'1.2.3.4.5' should be a valid IP address");
    }

    [Fact]
    public void Validate_MemoryRangeStartBelowRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 10000,
          ""LocalVariableMemoryStart"": -1,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MemoryRangeConfig.LocalVariableMemoryStart));
      ex.ActualValue.Should().Be(-1);
      ex.Message.Should().Contain("0..850");
    }

    [Fact]
    public void Validate_MemoryRangeEndAboveRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 10000,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 851
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MemoryRangeConfig.LocalVariableMemoryEnd));
      ex.ActualValue.Should().Be(851);
      ex.Message.Should().Contain("0..850");
    }

    [Fact]
    public void Validate_MemoryRangeSizeSmallerOne_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 10000,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 0
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MemoryRangeConfig.LocalVariableMemorySize));
      ex.ActualValue.Should().Be(0);
      ex.Message.Should().Contain("1..850");
    }

    [Fact]
    public void Validate_MemoryRangePollingCycleSmaller100_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 99,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 850
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MemoryRangeConfig.LocalVariableMemoryPollingCycleMilliseconds));
      ex.ActualValue.Should().Be(99);
      ex.Message.Should().Contain("Polling cycle should be greater than 100");
    }

    [Fact]
    public void Validate_PublishedLogoAddressAboveRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""Mqtt"": [
        {
          ""ClientId"": ""mqtt-client-id"",
          ""Subscribed"": [
            {
              ""Topic"": ""map/20/30/set"",
              ""LogoAddress"": 0,
              ""Type"": ""byte""
            }
          ],
          ""Published"": [
            {
              ""Topic"": ""map/20/30/get"",
              ""LogoAddress"": 851,
              ""Type"": ""byte""
            }
          ]
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MqttChannel.LogoAddress));
      ex.ActualValue.Should().Be(851);
      ex.Message.Should().Contain("should be 0..850");
    }

    [Fact]
    public void Validate_PublishedLogoAddressBelowRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""Mqtt"": [
        {
          ""ClientId"": ""mqtt-client-id"",
          ""Subscribed"": [
            {
              ""Topic"": ""map/20/30/set"",
              ""LogoAddress"": 0,
              ""Type"": ""byte""
            }
          ],
          ""Published"": [
            {
              ""Topic"": ""map/20/30/get"",
              ""LogoAddress"": -1,
              ""Type"": ""byte""
            }
          ]
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MqttChannel.LogoAddress));
      ex.ActualValue.Should().Be(-1);
      ex.Message.Should().Contain("should be 0..850");
    }

    [Fact]
    public void Validate_SubscribedLogoAddressAboveRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""Mqtt"": [
        {
          ""ClientId"": ""mqtt-client-id"",
          ""Subscribed"": [
            {
              ""Topic"": ""map/20/30/set"",
              ""LogoAddress"": 851,
              ""Type"": ""byte""
            }
          ],
          ""Published"": [
            {
              ""Topic"": ""map/20/30/get"",
              ""LogoAddress"": 42,
              ""Type"": ""byte""
            }
          ]
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MqttChannel.LogoAddress));
      ex.ActualValue.Should().Be(851);
      ex.Message.Should().Contain("should be 0..850");
    }

    [Fact]
    public void Validate_SubscribedLogoAddressBelowRange_ShouldThrow()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""some.where"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""Mqtt"": [
        {
          ""ClientId"": ""mqtt-client-id"",
          ""Subscribed"": [
            {
              ""Topic"": ""map/20/30/set"",
              ""LogoAddress"": -1,
              ""Type"": ""byte""
            }
          ],
          ""Published"": [
            {
              ""Topic"": ""map/20/30/get"",
              ""LogoAddress"": 42,
              ""Type"": ""byte""
            }
          ]
        }
      ]
    }
  ]
}");
      var config = new Config();
      config.Read(configFile.Path);

      var ex = Assert.Throws<ArgumentOutOfRangeException>(() => config.Validate());
      ex.ParamName.Should().Be(nameof(MqttChannel.LogoAddress));
      ex.ActualValue.Should().Be(-1);
      ex.Message.Should().Contain("should be 0..850");
    }



    [Fact]
    public void Read_ValidContent_Succeeds()
    {
      using var configFile = new TempFile(@"
{
  ""MqttBrokerIpAddress"": ""5.6.7.8"",
  ""MqttBrokerPort"": ""6667"",
  ""Logos"": [
    {
      ""IpAddress"": ""1.2.3.4"",
      ""MemoryRanges"": [
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 10000,
          ""LocalVariableMemoryStart"": 0,
          ""LocalVariableMemoryEnd"": 128
        },
        {
          ""LocalVariableMemoryPollingCycleMilliseconds"": 666,
          ""LocalVariableMemoryStart"": 12,
          ""LocalVariableMemoryEnd"": 42
        }
      ],
      ""Mqtt"": [
        {
          ""ClientId"": ""mqtt-client-id"",
          ""Subscribed"": [
            {
              ""Topic"": ""map/21/31/set"",
              ""LogoAddress"": 21,
              ""Type"": ""byte""
            },
            {
              ""Topic"": ""map/22/32/set"",
              ""LogoAddress"": 22,
              ""Type"": ""integer""
            },
            {
              ""Topic"": ""map/26/36/set"",
              ""LogoAddress"": 26,
              ""Type"": ""float""
            }
          ],
          ""Published"": [
            {
              ""Topic"": ""map/120/130/get"",
              ""LogoAddress"": 120,
              ""Type"": ""bit""
            },
            {
              ""Topic"": ""map/121/131/get"",
              ""LogoAddress"": 121,
              ""Type"": ""byte""
            },
            {
              ""Topic"": ""map/122/132/get"",
              ""LogoAddress"": 122,
              ""Type"": ""integer""
            },
            {
              ""Topic"": ""map/126/136/get"",
              ""LogoAddress"": 126,
              ""Type"": ""float""
            }
          ]
        }
      ]
    }
  ]
}");

      var config = new Config();
      config.Read(configFile.Path);

      config.MqttBrokerIpAddress.Should().Be("5.6.7.8");
      config.MqttBrokerPort.Should().Be(6667);

      config.Logos.Length.Should().Be(1);
      var logo = config.Logos.First();

      logo.IpAddress.Should().Be("1.2.3.4");

      logo.MemoryRanges[0].LocalVariableMemoryStart.Should().Be(0);
      logo.MemoryRanges[0].LocalVariableMemoryEnd.Should().Be(128);
      logo.MemoryRanges[0].LocalVariableMemoryPollingCycleMilliseconds.Should().Be(10000);

      logo.MemoryRanges[1].LocalVariableMemoryStart.Should().Be(12);
      logo.MemoryRanges[1].LocalVariableMemoryEnd.Should().Be(42);
      logo.MemoryRanges[1].LocalVariableMemoryPollingCycleMilliseconds.Should().Be(666);

      logo.Mqtt[0].ClientId.Should().Be("mqtt-client-id");

      logo.Mqtt[0].Subscribed[0].Topic.Should().Be("map/21/31/set");
      logo.Mqtt[0].Subscribed[0].LogoAddress.Should().Be(21);
      logo.Mqtt[0].Subscribed[0].Type.Should().Be("byte");

      logo.Mqtt[0].Subscribed[1].Topic.Should().Be("map/22/32/set");
      logo.Mqtt[0].Subscribed[1].LogoAddress.Should().Be(22);
      logo.Mqtt[0].Subscribed[1].Type.Should().Be("integer");

      logo.Mqtt[0].Subscribed[2].Topic.Should().Be("map/26/36/set");
      logo.Mqtt[0].Subscribed[2].LogoAddress.Should().Be(26);
      logo.Mqtt[0].Subscribed[2].Type.Should().Be("float");


      logo.Mqtt[0].Published[0].Topic.Should().Be("map/120/130/get");
      logo.Mqtt[0].Published[0].LogoAddress.Should().Be(120);
      logo.Mqtt[0].Published[0].Type.Should().Be("bit");

      logo.Mqtt[0].Published[1].Topic.Should().Be("map/121/131/get");
      logo.Mqtt[0].Published[1].LogoAddress.Should().Be(121);
      logo.Mqtt[0].Published[1].Type.Should().Be("byte");

      logo.Mqtt[0].Published[2].Topic.Should().Be("map/122/132/get");
      logo.Mqtt[0].Published[2].LogoAddress.Should().Be(122);
      logo.Mqtt[0].Published[2].Type.Should().Be("integer");

      logo.Mqtt[0].Published[3].Topic.Should().Be("map/126/136/get");
      logo.Mqtt[0].Published[3].LogoAddress.Should().Be(126);
      logo.Mqtt[0].Published[3].Type.Should().Be("float");
    }
  }

  public class TempFile : IDisposable
  {
    public TempFile(string content)
    {
      Path = System.IO.Path.GetTempFileName();
      File.WriteAllText(Path, content);
    }

    public void Dispose() => File.Delete(Path);

    public string Path { get; }
  }
}