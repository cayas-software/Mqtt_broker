using System.Text.Json;
using MQTTnet;
using MQTTnet.Server;
using System.Text;
using MQTTnet.Internal;
using Newtonsoft.Json;

var option = new MqttServerOptionsBuilder().WithDefaultEndpoint().WithDefaultEndpointPort(1884);

Console.WriteLine("START BUILD BROKER");

var mqttServer = new MqttFactory().CreateMqttServer(option.Build());

await mqttServer.StartAsync();

Console.WriteLine("BROKER STARTED");

Console.WriteLine("PRESS ANY KEY TO STOP BROKER");
Console.ReadLine();

await mqttServer.StopAsync();