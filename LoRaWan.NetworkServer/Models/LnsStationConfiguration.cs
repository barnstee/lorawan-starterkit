// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace LoRaWANContainer.LoRaWan.NetworkServer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.Json;
    using global::LoRaWan;
    using LoRaTools.Regions;
    using Newtonsoft.Json;

    public static class LnsStationConfiguration
    {
        public enum Radio { Zero, One }

        public static T Map<T>(this Radio radio, T zero, T one) => radio switch
        {
            Radio.Zero => zero,
            Radio.One => one,
            _ => throw new ArgumentException(null, nameof(radio))
        };

        [DataContract]
        public class ChannelConfig(bool enable, Radio radio, int @if)
        {
            [DataMember(Name = "enable")]
            public bool Enable { get; } = enable;

            [DataMember(Name = "radio")]
            public Radio Radio { get; } = radio;

            [DataMember(Name = "if")]
            public int If { get; } = @if;
        }

        [DataContract]
        public class StandardConfig(bool enable, Radio radio, int @if, Bandwidth bandwidth, SpreadingFactor spreadingFactor)
        {
            [DataMember(Name = "enable")]
            public bool Enable { get; } = enable;

            [DataMember(Name = "radio")]
            public Radio Radio { get; } = radio;

            [DataMember(Name = "if")]
            public int If { get; } = @if;

            [DataMember(Name = "bandwidth")]
            public Bandwidth Bandwidth { get; } = bandwidth;

            [DataMember(Name = "spread_factor")]
            public SpreadingFactor SpreadingFactor { get; } = spreadingFactor;
        }

        [DataContract]
        public class RadioConfig(bool enable, Hertz freq)
        {
            [DataMember(Name = "enable")]
            public bool Enable { get; } = enable;

            [DataMember(Name = "freq")]
            public Hertz Freq { get; } = freq;
        }

        [DataContract]
        public class Sx1301Config(RadioConfig radio0,
                                  RadioConfig radio1,
                                  StandardConfig channelLoraStd,
                                  ChannelConfig channelFsk,
                                  ChannelConfig channelMultiSf0,
                                  ChannelConfig channelMultiSf1,
                                  ChannelConfig channelMultiSf2,
                                  ChannelConfig channelMultiSf3,
                                  ChannelConfig channelMultiSf4,
                                  ChannelConfig channelMultiSf5,
                                  ChannelConfig channelMultiSf6,
                                  ChannelConfig channelMultiSf7)
        {
            [DataMember(Name = "radio_0")]
            public RadioConfig Radio0 { get; } = radio0;

            [DataMember(Name = "radio_1")]
            public RadioConfig Radio1 { get; } = radio1;

            [DataMember(Name = "chan_Lora_std")]
            public StandardConfig ChannelLoraStd { get; } = channelLoraStd;

            [DataMember(Name = "chan_FSK")]
            public ChannelConfig ChannelFsk { get; } = channelFsk;

            [DataMember(Name = "chan_multiSF_0")]
            public ChannelConfig ChannelMultiSf0 { get; } = channelMultiSf0;

            [DataMember(Name = "chan_multiSF_1")]
            public ChannelConfig ChannelMultiSf1 { get; } = channelMultiSf1;

            [DataMember(Name = "chan_multiSF_2")]
            public ChannelConfig ChannelMultiSf2 { get; } = channelMultiSf2;

            [DataMember(Name = "chan_multiSF_3")]
            public ChannelConfig ChannelMultiSf3 { get; } = channelMultiSf3;

            [DataMember(Name = "chan_multiSF_4")]
            public ChannelConfig ChannelMultiSf4 { get; } = channelMultiSf4;

            [DataMember(Name = "chan_multiSF_5")]
            public ChannelConfig ChannelMultiSf5 { get; } = channelMultiSf5;

            [DataMember(Name = "chan_multiSF_6")]
            public ChannelConfig ChannelMultiSf6 { get; } = channelMultiSf6;

            [DataMember(Name = "chan_multiSF_7")]
            public ChannelConfig ChannelMultiSf7 { get; } = channelMultiSf7;
        }

        [DataContract]
        public class RouterConfig
        {
            [DataMember(Name = "msgtype")]
            public string MsgType { get; set; } = "router_config";

            [DataMember(Name = "NetID")]
            public List<int> NetID { get; set; } = [];

            [DataMember(Name = "JoinEui")]
            public List<(ulong Begin, ulong End)> JoinEui { get; set; } = []; // ranges: beg, end inclusive

            [DataMember(Name = "region")]
            public string Region { get; set; } = string.Empty; // e.g., "EU863", "US902", etc.

            [DataMember(Name = "hwspec")]
            public string HwSpec { get; set; } = string.Empty;

            [DataMember(Name = "freq_range")]
            public (ulong Min, ulong Max) FreqRange { get; set; } // min, max (hz)

            [DataMember(Name = "DRs")]
            public List<(int SpreadingFactor, int Bandwidth, bool DownloadOnly)> DRs { get; set; } = []; // sf, bw, dnonly

            [DataMember(Name = "sx1301_conf")]
            public List<Sx1301Config> Sx1301Conf { get; set; } = [];

            [DataMember(Name = "nocca")]
            public bool NoCca { get; set; }

            [DataMember(Name = "nodc")]
            public bool NoDc { get; set; }

            [DataMember(Name = "nodwell")]
            public bool NoDwell { get; set; }

            [DataMember(Name = "bcning")]
            public Beaconing Bcning { get; set; }
        }

        [DataContract]
        public class Beaconing
        {
            [DataMember(Name = "DR")]
            public uint DR { get; set; }

            [DataMember(Name = "layout")]
            public uint[] Layout { get; set; }

            [DataMember(Name = "freqs")]
            public uint[] Freqs { get; set; }
        }

        public static RouterConfig GetConfiguration(string jsonInput) => JsonConvert.DeserializeObject<RouterConfig>(jsonInput);

        public static Region GetRegion(string jsonInput) => JsonConvert.DeserializeObject<Region>(jsonInput);

        private static string WriteRouterConfig(IEnumerable<NetId> allowedNetIds,
                                                IEnumerable<(JoinEui Min, JoinEui Max)> joinEuiRanges,
                                                string region,
                                                string hwspec,
                                                (Hertz Min, Hertz Max) freqRange,
                                                IEnumerable<(SpreadingFactor SpreadingFactor, Bandwidth Bandwidth, bool DnOnly)> dataRates,
                                                Sx1301Config[] sx1301Config,
                                                bool nocca, bool nodc, bool nodwell, Beaconing bcning)
        {
            if (string.IsNullOrEmpty(region)) throw new Newtonsoft.Json.JsonException("Region must not be null.");
            if (string.IsNullOrEmpty(hwspec)) throw new Newtonsoft.Json.JsonException("hwspec must not be null.");
            if (freqRange is var (minFreq, maxFreq) && minFreq == maxFreq) throw new Newtonsoft.Json.JsonException("Minimum and maximum frequencies must differ.");
            if (!dataRates.Any()) throw new Newtonsoft.Json.JsonException("Datarates list must not be empty.");
            if (sx1301Config.Length == 0) throw new Newtonsoft.Json.JsonException("sx1301_conf must not be empty.");

            using var ms = new MemoryStream();

            using var writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            writer.WriteString("msgtype", LnsMessageType.RouterConfig.ToBasicStationString());
            writer.WritePropertyName("NetID");
            writer.WriteStartArray();

            if (allowedNetIds is not null)
            {
                foreach (var netId in allowedNetIds)
                {
                    writer.WriteNumberValue(netId.NetworkId);
                }
            }
            writer.WriteEndArray();

            writer.WritePropertyName("JoinEui");
            writer.WriteStartArray();
            if (joinEuiRanges is not null)
            {
                foreach (var (minJoinEui, maxJoinEui) in joinEuiRanges)
                {
                    writer.WriteStartArray();
                    writer.WriteNumberValue(minJoinEui.AsUInt64);
                    writer.WriteNumberValue(maxJoinEui.AsUInt64);
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndArray();

            writer.WriteString("region", region);
            writer.WriteString("hwspec", hwspec);

            writer.WritePropertyName("freq_range");
            writer.WriteStartArray();
            writer.WriteNumberValue(freqRange.Min.AsUInt64);
            writer.WriteNumberValue(freqRange.Max.AsUInt64);
            writer.WriteEndArray();

            writer.WritePropertyName("DRs");
            writer.WriteStartArray();
            foreach (var (sf, bw, dnOnly) in dataRates)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue((int)sf);
                writer.WriteNumberValue((int)bw);
                writer.WriteNumberValue(dnOnly ? 1 : 0);
                writer.WriteEndArray();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("sx1301_conf");
            writer.WriteStartArray();

            foreach (var config in sx1301Config)
            {
                writer.WriteStartObject();
                WriteRadioConfig("radio_0", config.Radio0);
                WriteRadioConfig("radio_1", config.Radio1);
                WriteChannelConfig("chan_FSK", config.ChannelFsk);
                WriteStandardConfig("chan_Lora_std", config.ChannelLoraStd);
                WriteChannelConfig("chan_multiSF_0", config.ChannelMultiSf0);
                WriteChannelConfig("chan_multiSF_1", config.ChannelMultiSf1);
                WriteChannelConfig("chan_multiSF_2", config.ChannelMultiSf2);
                WriteChannelConfig("chan_multiSF_3", config.ChannelMultiSf3);
                WriteChannelConfig("chan_multiSF_4", config.ChannelMultiSf4);
                WriteChannelConfig("chan_multiSF_5", config.ChannelMultiSf5);
                WriteChannelConfig("chan_multiSF_6", config.ChannelMultiSf6);
                WriteChannelConfig("chan_multiSF_7", config.ChannelMultiSf7);
                writer.WriteEndObject();
            }

            writer.WriteEndArray(); // sx1301_conf: [...]

            writer.WriteBoolean("nocca", nocca);
            writer.WriteBoolean("nodc", nodc);
            writer.WriteBoolean("nodwell", nodwell);

            if (bcning != null)
            {
                writer.WritePropertyName("bcning"); // start beaconing

                writer.WriteStartObject();
                writer.WriteNumber("DR", bcning.DR);
                writer.WritePropertyName("layout");
                writer.WriteStartArray();
                foreach (var layout in bcning.Layout)
                {
                    writer.WriteNumberValue(layout);
                }
                writer.WriteEndArray();
                writer.WritePropertyName("freqs");
                writer.WriteStartArray();
                foreach (var freq in bcning.Freqs)
                {
                    writer.WriteNumberValue(freq);
                }
                writer.WriteEndArray();
                writer.WriteEndObject(); // end beaconing
            }
            writer.WriteEndObject();

            writer.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());

            void WriteRadioConfig(string property, RadioConfig radioConf)
            {
                writer.WriteStartObject(property);
                writer.WriteBoolean("enable", radioConf.Enable);
                writer.WriteNumber("freq", radioConf.Freq.AsUInt64);
                writer.WriteEndObject();
            }

            void WriteChannelConfig(string property, ChannelConfig sxConf)
            {
                writer.WriteStartObject(property);
                writer.WriteBoolean("enable", sxConf.Enable);
                writer.WriteNumber("radio", (int)sxConf.Radio);
                writer.WriteNumber("if", sxConf.If);
                writer.WriteEndObject();
            }

            void WriteStandardConfig(string property, StandardConfig chanConf)
            {
                writer.WriteStartObject(property);
                writer.WriteBoolean("enable", chanConf.Enable);
                writer.WriteNumber("radio", (int)chanConf.Radio);
                writer.WriteNumber("if", chanConf.If);
                if (Enum.IsDefined(chanConf.Bandwidth))
                    writer.WriteNumber("bandwidth", chanConf.Bandwidth.ToHertz().AsUInt64);
                if (Enum.IsDefined(chanConf.SpreadingFactor))
                    writer.WriteNumber("spread_factor", (int)chanConf.SpreadingFactor);
                writer.WriteEndObject();
            }
        }
    }
}
