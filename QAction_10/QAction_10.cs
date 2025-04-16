using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

/// <summary>
/// DataMiner QAction Class: Force Poll Transport Streams.
/// </summary>
public static class QAction
{
    private const string TransportStreamsJson = @"{
    ""transport_streams"": [{
            ""ts_id"": 1,
            ""ts_name"": ""RTL HD"",
            ""multicast"": ""232.101.1.1"",
            ""sourceIp"": ""10.15.1.1"",
            ""network_id"": 1,
            ""services"": [{
                    ""service_id"": 52006,
                    ""service_name"": ""Service 1"",
                    ""service_type"": ""digital_television"",
                    ""service_provider"": ""Provider A""
                }, {
                    ""service_id"": 52007,
                    ""service_name"": ""Service 2"",
                    ""service_type"": ""digital_television"",
                    ""service_provider"": ""Provider A""
                }
            ]
        }, {
            ""ts_id"": 2,
            ""ts_name"": ""Das Erste SD"",
            ""multicast"": ""232.101.1.2"",
            ""sourceIp"": ""10.15.1.2"",
            ""network_id"": 1,
            ""services"": [{
                    ""service_id"": 101,
                    ""service_name"": ""Service 3"",
                    ""service_type"": ""digital_television"",
                    ""service_provider"": ""Provider B""
                }, {
                    ""service_id"": 102,
                    ""service_name"": ""Service 4"",
                    ""service_type"": ""digital_radio"",
                    ""service_provider"": ""Provider B""
                }
            ]
        }, {
            ""ts_id"": 3,
            ""ts_name"": ""Comedy Central HD"",
            ""multicast"": ""232.101.1.3"",
            ""sourceIp"": ""10.15.1.3"",
            ""network_id"": 2,
            ""services"": [{
                    ""service_id"": 2003,
                    ""service_name"": ""Service 5"",
                    ""service_type"": ""digital_television"",
                    ""service_provider"": ""Provider C""
                }, {
                    ""service_id"": 2004,
                    ""service_name"": ""Service 6"",
                    ""service_type"": ""digital_radio"",
                    ""service_provider"": ""Provider C""
                }
            ]
        }, {
            ""ts_id"": 4,
            ""ts_name"": ""Comedy Central QHD"",
            ""multicast"": ""232.101.1.3"",
            ""sourceIp"": ""10.15.1.3"",
            ""network_id"": 2,
            ""services"": [{
                    ""service_id"": 2007,
                    ""service_name"": ""Service 5"",
                    ""service_type"": ""digital_television"",
                    ""service_provider"": ""Provider C""
                }, {
                    ""service_id"": 2009,
                    ""service_name"": ""Service 6"",
                    ""service_type"": ""digital_radio"",
                    ""service_provider"": ""Provider C""
                }
            ]
        }
    ]
}
";

    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            var transportStreams = SecureNewtonsoftDeserialization.DeserializeObject<Data>(TransportStreamsJson);

            var streamRows = transportStreams.TransportStreams.Select(ToQActionRow).ToArray();
            protocol.FillArray(protocol.transportstreams.TableId, protocol.transportstreams.QActionRowsToObjectFillArray(streamRows));

            var serviceRows = transportStreams.TransportStreams
                .SelectMany(stream => stream.Services?.Select(service => ToQActionRow(service, stream)))
                .ToArray();

            protocol.FillArray(protocol.services.TableId, protocol.services.QActionRowsToObjectFillArray(serviceRows));
        }
        catch (Exception ex)
        {
            protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
        }
    }

    private static TransportstreamsQActionRow ToQActionRow(Data.TransportStream transportStream)
    {
        return new TransportstreamsQActionRow()
        {
            Transportstreamsid_1001 = transportStream.Id,
            Transportstreamsname_1002 = transportStream.Name,
            Transportstreamsmulticast_1003 = transportStream.Multicast,
            Transportstreamssourceip_1004 = transportStream.SourceIp,
            Transportstreamsnetworkid_1005 = transportStream.NetworkId,
            Transportstreamslastpolledtimestamp_1007 = DateTime.Now.ToOADate(),
        };
    }

    private static ServicesQActionRow ToQActionRow(Data.Service service, Data.TransportStream parent)
    {
        return new ServicesQActionRow()
        {
            Servicesserviceid_2001 = service.Id.ToString(),
            Servicesservicename_2002 = service.Name,
            Servicesservicetype_2003 = service.Type,
            Servicesserviceprovider_2004 = service.Provider,
            Serviceslastpolledtimestamp_2005 = DateTime.Now.ToOADate(),
            Servicesparenttransportstreamid_2006 = parent.Id,
        };
    }

    private struct Data
    {
        [JsonProperty("transport_streams")]
        public List<TransportStream> TransportStreams;

        public struct TransportStream
        {
            [JsonProperty("ts_id", Required = Required.Always)]
            public int Id;

            [JsonProperty("ts_name")]
            public string Name;

            [JsonProperty("multicast")]
            public string Multicast;

            [JsonProperty("sourceIp")]
            public string SourceIp;

            [JsonProperty("network_id")]
            public int? NetworkId;

            public List<Service> Services;
        }

        public struct Service
        {
            [JsonProperty("service_id", Required = Required.Always)]
            public int Id;

            [JsonProperty("service_name", Required = Required.Always)]
            public string Name;

            [JsonProperty("service_type")]
            public string Type;

            [JsonProperty("service_provider")]
            public string Provider;
        }
    }
}