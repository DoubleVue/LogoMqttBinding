﻿using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LogoMqttBinding.MqttAdapter
{
  internal class MqttFormat
  {
    public MqttFormat(ILogger logger) => this.logger = logger;



    public bool ToValue(byte[] payload, out byte result)
    {
      var s = Encoding.UTF8.GetString(payload);
      var succeeded = byte.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
      if (!succeeded) logger.LogWarning($"Cannot parse '{s}'");
      return succeeded;
    }

    public bool ToValue(byte[] payload, out short result)
    {
      var s = Encoding.UTF8.GetString(payload);
      var succeeded = short.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
      if (!succeeded) logger.LogWarning($"Cannot parse '{s}'");
      return succeeded;
    }

    public bool ToValue(byte[] payload, out float result)
    {
      var s = Encoding.UTF8.GetString(payload);
      var succeeded = float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
      if (!succeeded) logger.LogWarning($"Cannot parse '{s}'");
      return succeeded;
    }



    public static byte[] ToPayload(byte value)
    {
      var s = value.ToString(CultureInfo.InvariantCulture);
      return Encoding.UTF8.GetBytes(s);
    }

    public static byte[] ToPayload(short value)
    {
      var s = value.ToString(CultureInfo.InvariantCulture);
      return Encoding.UTF8.GetBytes(s);
    }

    public static byte[] ToPayload(float value)
    {
      var s = value.ToString(CultureInfo.InvariantCulture);
      return Encoding.UTF8.GetBytes(s);
    }



    private readonly ILogger logger;
  }
}