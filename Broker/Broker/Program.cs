using System.Text.Json;
using MQTTnet;
using MQTTnet.Server;
using System.Text;
using MQTTnet.Internal;
using Newtonsoft.Json;

const string LogFilename = "YOUR_PATH";

var option = new MqttServerOptionsBuilder().WithDefaultEndpoint().WithDefaultEndpointPort(1884);

Console.WriteLine("START BUILD BROKER");

var mqttServer = new MqttFactory().CreateMqttServer(option.Build());
mqttServer.InterceptingPublishAsync += context =>
{
    var message = Encoding.UTF8.GetString(context.ApplicationMessage.Payload);

    if (File.Exists(LogFilename) == false)
        File.CreateText(LogFilename);

    File.AppendAllText(LogFilename, $"Client: {context.ClientId}, sent time: {DateTime.Now}, message: {JsonConvert.SerializeObject(message)} \r\n");

    return CompletedTask.Instance;
};

await mqttServer.StartAsync();

Console.WriteLine("BROKER STARTED");

Console.WriteLine("PRESS ANY KEY TO STOP BROKER");
Console.ReadLine();

await mqttServer.StopAsync();