using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using NamespageMessage;

Console.WriteLine("Starting Consumer Avro");


var conf = new ConsumerConfig
{
    GroupId = "ConsumerAvro",
    BootstrapServers = "localhost:9092",
    AutoOffsetReset = AutoOffsetReset.Earliest
};
var schemaRegistryConfig = new SchemaRegistryConfig{Url="http://localhost:8081"};
var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

using(var c = new ConsumerBuilder<string, AvroMessage>(conf)
.SetValueDeserializer(new AvroDeserializer<AvroMessage>(schemaRegistry).AsSyncOverAsync())
.SetErrorHandler((_,e)=> Console.WriteLine($"Error: {e.Reason}"))
.Build()
)
{
    c.Subscribe("MessageTopicAvro");
    CancellationTokenSource cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>{
        e.Cancel = true;
        cts.Cancel();
    };
    
    try
    {
        while(true)
        {
            try
            {
                var messageConsumed = c.Consume(cts.Token);
                var messageJson = JsonSerializer.Serialize(messageConsumed.Message.Value);
                Console.WriteLine($"Message: {messageJson}");
            }
            catch(ConsumeException ex)
            {
                Console.WriteLine($"Error occurred: {ex.Error.Reason}");
            }
        }
    }
    catch(OperationCanceledException){ c.Close();}
}

