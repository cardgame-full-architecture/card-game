using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;

namespace Assets.Scripts.Consul
{
    public class ServiceTaggedAddress
    {
        public string Address;

        public int Port;
    }

    public class AgentService
    {
        public string ID;

        public string Service;

        public string[] Tags;

        public int Port;

        public string Address;

        public Dictionary<string, ServiceTaggedAddress> TaggedAddresses;

        public bool EnableTagOverride;

        public Dictionary<string, string> Meta;
    }
        
    public class Node
    {
        [JsonProperty(PropertyName = "Node")]
        public string Name { get; set; }

        public string Address { get; set; }

        public Dictionary<string, string> TaggedAddresses { get; set; }
    }

    public class HealthStatusConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((HealthStatus)value).Status);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (string)serializer.Deserialize(reader, typeof(string)) switch
            {
                "passing" => HealthStatus.Passing,
                "warning" => HealthStatus.Warning,
                "critical" => HealthStatus.Critical,
                _ => throw new ArgumentException("Invalid Check status value during deserialization"),
            };
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(HealthStatus))
            {
                return true;
            }

            return false;
        }
    }

    public class HealthStatus : IEquatable<HealthStatus>
    {
        public const string NodeMaintenance = "_node_maintenance";

        public const string ServiceMaintenancePrefix = "_service_maintenance:";

        public string Status { get; private set; }

        public static HealthStatus Passing { get; } = new HealthStatus
        {
            Status = "passing"
        };


        public static HealthStatus Warning { get; } = new HealthStatus
        {
            Status = "warning"
        };


        public static HealthStatus Critical { get; } = new HealthStatus
        {
            Status = "critical"
        };


        public static HealthStatus Maintenance { get; } = new HealthStatus
        {
            Status = "maintenance"
        };


        public static HealthStatus Any { get; } = new HealthStatus
        {
            Status = "any"
        };


        public bool Equals(HealthStatus other)
        {
            if (other != null)
            {
                return this == other;
            }

            return false;
        }

        public override bool Equals(object other)
        {
            if (other != null && GetType() == other.GetType())
            {
                return Equals((HealthStatus)other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Status.GetHashCode();
        }
    }

    public class HealthCheck
    {
        public string Node { get; set; }

        public string CheckID { get; set; }

        public string Name { get; set; }

        [JsonConverter(typeof(HealthStatusConverter))]
        public HealthStatus Status { get; set; }

        public string Notes { get; set; }

        public string Output { get; set; }

        public string ServiceID { get; set; }

        public string ServiceName { get; set; }

        public string Type { get; set; }
    }

    public class ServiceEntry
    {
        public Node Node { get; set; }

        public AgentService Service { get; set; }

        public HealthCheck[] Checks { get; set; }
    }

    public class KVPairConverter : JsonConverter
    {
        private static readonly Lazy<string[]> ObjProps = new Lazy<string[]>(() => (from p in typeof(KVPair).GetRuntimeProperties()
                                                                                    select p.Name).ToArray());

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            KVPair kVPair = new KVPair();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    continue;
                }

                if (reader.TokenType == JsonToken.EndObject)
                {
                    return kVPair;
                }

                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                string jsonPropName = reader.Value!.ToString();
                string text = ObjProps.Value.FirstOrDefault((string p) => p.Equals(jsonPropName, StringComparison.OrdinalIgnoreCase));
                if (text == null)
                {
                    continue;
                }

                PropertyInfo runtimeProperty = kVPair.GetType().GetRuntimeProperty(text);
                if (jsonPropName.Equals("Flags", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(reader.ReadAsString()))
                    {
                        ulong num = Convert.ToUInt64(reader.Value);
                        runtimeProperty.SetValue(kVPair, num, null);
                    }
                }
                else if (jsonPropName.Equals("Value", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(reader.ReadAsString()))
                    {
                        byte[] value = Convert.FromBase64String(reader.Value!.ToString());
                        runtimeProperty.SetValue(kVPair, value, null);
                    }
                }
                else if (reader.Read())
                {
                    object value2 = Convert.ChangeType(reader.Value, runtimeProperty.PropertyType);
                    runtimeProperty.SetValue(kVPair, value2, null);
                }
            }

            return kVPair;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(KVPair))
            {
                return true;
            }

            return false;
        }
    }

    public class InvalidKeyPairException : Exception
    {
        public InvalidKeyPairException()
        {
        }

        public InvalidKeyPairException(string message)
            : base(message)
        {
        }

        public InvalidKeyPairException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    [JsonConverter(typeof(KVPairConverter))]
    public class KVPair
    {
        public string Key { get; set; }

        public ulong CreateIndex { get; set; }

        public ulong ModifyIndex { get; set; }

        public ulong LockIndex { get; set; }

        public ulong Flags { get; set; }

        public byte[] Value { get; set; }

        public string Session { get; set; }

        public KVPair(string key)
        {
            Key = key;
        }

        internal KVPair()
        {
        }

        internal void Validate()
        {
            ValidatePath(Key);
        }

        internal static void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidKeyPairException("Invalid key. Key path is empty.");
            }

            if (path[0] == '/')
            {
                throw new InvalidKeyPairException($"Invalid key. Key must not begin with a '/': {path}");
            }
        }
    }


    public class ConsulClient
    {
        [Serializable]
        struct AgentServiceCheck
        {
            public string CheckID;
            public string TCP;
            public string Interval;
        }

        [Serializable]
        struct AgentServiceRegistration
        {
            public string Name;
            public string ID;
            public string Address;
            public AgentServiceCheck Check;
            public int Port;
        }

        private static List<ConsulClient> _startedClients = new List<ConsulClient>();

        private string _consulAPIURL = "http://127.0.0.1:8500/v1";
        private bool _isAlive;
        private TcpListener _tcpListener;
        private Task _aliveTask;

        /// <summary>
        /// Создает экземпляр класса ConsulClient для управления локальным агентом
        /// </summary>
        /// <param name="name">Имя сервиса</param>
        /// <param name="instanceID">ID экземпляра сервиса</param>
        /// <param name="serviceAddress">Адрес экземпляра сервиса (IPAddress.None - устанновит на первый попавшийся IPv4 адрес)</param>
        /// <param name="servicePort">Порт экземпляра сервиса</param>
        /// <param name="checkPort">Порт для проверки состояния консулом (0 - выбирается любой свободный)</param>
        public ConsulClient(string name, string instanceID, IPAddress serviceAddress, int servicePort = 0, int checkPort = 0)
        {
            Name = name;
            ServiceId = instanceID;
            ServiceAddress = serviceAddress == IPAddress.None ? GetCurrentIPV4Address() : serviceAddress;
            ServicePort = servicePort;
            CheckPort = checkPort == 0 ? GetUnusedPort() : checkPort;
        }

        public ConsulClient()
        {
            
        }

        public string ServiceId { get; }
        public int ServicePort { get; }
        public IPAddress ServiceAddress { get; }
        public string Name { get; }
        public int CheckPort { get; }

        private IPAddress GetCurrentIPV4Address()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).First(addr => addr.AddressFamily == AddressFamily.InterNetwork);
        }

        private int GetUnusedPort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static ConsulClient[] GetStartedClients()
        {
            return _startedClients.ToArray();
        }

        private UnityWebRequest SendRequest(UnityWebRequest request)
        {
            UnityWebRequestAsyncOperation responsOperation = request.SendWebRequest();
            while (!responsOperation.isDone) ;
            return request;
        }

        public async Task<bool> RegistrationAsync()
        {
            UnityWebRequest request = SendRequest(
                UnityWebRequest.Put(
                    _consulAPIURL + "/agent/service/register",
                    JsonUtility.ToJson(new AgentServiceRegistration
                    {
                        ID = ServiceId,
                        Name = Name,
                        Address = ServiceAddress.ToString(),
                        Port = ServicePort,
                        Check = new AgentServiceCheck
                        {
                            CheckID = $"service:{Name}",
                            TCP = $"{ServiceAddress}:{CheckPort}",
                            Interval = "10s"
                        }
                    })
                )
            );
            return request.responseCode == 200;
        }

        public Task StartPingTask()
        {
            return _aliveTask ??= Task.Run(() =>
            {
                _isAlive = true;

                _tcpListener = new TcpListener(ServiceAddress, CheckPort);
                try
                {
                    _tcpListener.Start();
                    _startedClients.Add(this);
                    while (_isAlive)
                    {
                        TcpClient r = _tcpListener.AcceptTcpClient();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    _isAlive = false;
                    _tcpListener.Stop();
                }
            });
        }

        public void StopPingTask()
        {
            _isAlive = false;
            _tcpListener.Stop();
            _startedClients.Remove(this);
        }

        public async Task<Dictionary<string, string[]>> GetServices()
        {
            UnityWebRequest request = SendRequest(UnityWebRequest.Get(_consulAPIURL + "/catalog/services"));

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string[]>>(request.downloadHandler.text);
            }
            else
            {
                return null;
            }
        }

        public async Task<ServiceEntry[]> GetAliveServiceEntries(string serviceName)
        {
            UnityWebRequest request = SendRequest(UnityWebRequest.Get(_consulAPIURL + "/health/service/" + serviceName));

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonConvert.DeserializeObject<ServiceEntry[]>(request.downloadHandler.text)
                    .Where(entry => entry.Checks.FirstOrDefault(check => check.CheckID == $"service:{serviceName}")?.Status.Status == "passing")
                    .ToArray();
            }
            else
            {
                return null;
            }
        }

        public async Task<KVPair> GetKV(string key)
        {
            UnityWebRequest request = SendRequest(UnityWebRequest.Get(_consulAPIURL + "/kv/" + key));

            if (request.result == UnityWebRequest.Result.Success)
            {
                return JsonConvert.DeserializeObject<KVPair[]>(request.downloadHandler.text).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> SetKV(string key, byte[] value)
        {
            UnityWebRequest request = SendRequest(UnityWebRequest.Put(_consulAPIURL + "/kv/" + key, value));
            return request.result == UnityWebRequest.Result.Success;
        }

        public async Task<bool> DeleteKV(string key)
        {
            UnityWebRequest request = SendRequest(UnityWebRequest.Delete(_consulAPIURL + "/kv/" + key));
            return request.result == UnityWebRequest.Result.Success;
        }
    }
}