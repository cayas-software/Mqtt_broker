using System.Text.Json;
using MQTTnet;
using MQTTnet.Server;
using System.Text;
using MQTTnet.Internal;
using Newtonsoft.Json;

const string LogFilename = "YOUR_PATH";
const string RetainedMessagesFilename = "YOUR_PATH";

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
// Make sure that the server will load the retained messages.
mqttServer.LoadingRetainedMessageAsync += async eventArgs =>
{
    try
    {
        var json = await File.ReadAllTextAsync(RetainedMessagesFilename);
        eventArgs.LoadedRetainedMessages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);

        Console.WriteLine("Retained messages loaded.");
    }
    catch (FileNotFoundException)
    {
        // Ignore because nothing is stored yet.
        Console.WriteLine("No retained messages stored yet.");
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception);
    }
};

// Make sure to persist the changed retained messages.
mqttServer.RetainedMessageChangedAsync += async eventArgs =>
{
    try
    {
        // This sample uses the property _StoredRetainedMessages_ which will contain all(!) retained messages.
        // The event args also contain the affected retained message (property ChangedRetainedMessage). This can be
        // used to write all retained messages to dedicated files etc. Then all files must be loaded and a full list
        // of retained messages must be provided in the loaded event.

        //var buffer = JsonSerializer.SerializeToUtf8Bytes(eventArgs.StoredRetainedMessages);
        //await File.WriteAllBytesAsync(RetainedMessagesFilename, buffer);

        if (File.Exists(RetainedMessagesFilename) == false)
            File.CreateText(RetainedMessagesFilename);

        File.WriteAllText(RetainedMessagesFilename, JsonConvert.SerializeObject(eventArgs.StoredRetainedMessages));

        Console.WriteLine("Retained messages saved.");
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception);
    }
};

// Make sure to clear the retained messages when they are all deleted via API.
mqttServer.RetainedMessagesClearedAsync += _ =>
{
    File.Delete(RetainedMessagesFilename);
    return Task.CompletedTask;
};

await mqttServer.StartAsync();

Console.WriteLine("BROKER STARTED");

Console.WriteLine("PRESS ANY KEY TO STOP BROKER");
Console.ReadLine();

await mqttServer.StopAsync();