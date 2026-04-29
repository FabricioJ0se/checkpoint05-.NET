using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Shared;
using Shared.Models;
using System.Text;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║  RECEIVER 2 - Usuários Validados (Consumer)  ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Aguardando usuários validados...\n");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(RabbitMQConstants.ExchangeValidation, ExchangeType.Topic, durable: true);
channel.QueueDeclare(RabbitMQConstants.QueueUsuariosReceiver, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(
    queue: RabbitMQConstants.QueueUsuariosReceiver,
    exchange: RabbitMQConstants.ExchangeValidation,
    routingKey: RabbitMQConstants.RoutingKeyUsuariosValidated);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, ea) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);

    try
    {
        var msg = JsonConvert.DeserializeObject<ValidationMessage<UsuarioMessage>>(json)!;

        Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}]  Usuário recebido:");
        Console.WriteLine($"   Nome            : {msg.Dados?.NomeCompleto}");
        Console.WriteLine($"   CPF             : {msg.Dados?.CPF}");
        Console.WriteLine($"   RG              : {msg.Dados?.RG}");
        Console.WriteLine($"   Endereço        : {msg.Dados?.Endereco}");
        Console.WriteLine($"   Data Registro   : {msg.Dados?.DataHoraRegistro:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"   Validado?       : {(msg.Validado ? " SIM" : " NÃO")}");
        Console.WriteLine($"   Resultado       : {msg.MensagemValidacao}");
        Console.WriteLine(new string('-', 60));

        channel.BasicAck(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Erro ao processar mensagem: {ex.Message}");
        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
    }
};

channel.BasicConsume(
    queue: RabbitMQConstants.QueueUsuariosReceiver,
    autoAck: false,
    consumer: consumer);

Console.WriteLine("Pressione [ENTER] para encerrar o Receiver 2...");
Console.ReadLine();
