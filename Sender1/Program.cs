using RabbitMQ.Client;
using Newtonsoft.Json;
using Shared;
using Shared.Models;
using System.Text;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     SENDER 1 - Frutas de Época (Producer)║");
Console.WriteLine("╚══════════════════════════════════════════╝");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

var frutas = new List<FrutaMessage>
{
    new() { NomeFruta = "Manga",    Descricao = "Fruta tropical doce e suculenta, rica em vitamina A e C. Muito consumida no verão brasileiro." },
    new() { NomeFruta = "Abacaxi",  Descricao = "Fruta tropical com sabor ácido-doce, fonte de bromelaína e vitamina C. Ideal para sucos e sobremesas." },
    new() { NomeFruta = "Melancia", Descricao = "Fruta de verão com alto teor de água (92%), rica em licopeno e vitaminas A e C." },
    new() { NomeFruta = "Goiaba",   Descricao = "Fruta brasileira muito nutritiva, com alto teor de vitamina C, fibras e antioxidantes." },
    new() { NomeFruta = "Jabuticaba", Descricao = "Fruta nativa do Brasil, cresce diretamente no tronco, rica em antocianinas e vitamina C." }
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(
    exchange: RabbitMQConstants.ExchangeFrutas,
    type: ExchangeType.Topic,
    durable: true,
    autoDelete: false);

channel.QueueDeclare(
    queue: RabbitMQConstants.QueueFrutasValidation,
    durable: true,
    exclusive: false,
    autoDelete: false);

channel.QueueBind(
    queue: RabbitMQConstants.QueueFrutasValidation,
    exchange: RabbitMQConstants.ExchangeFrutas,
    routingKey: RabbitMQConstants.RoutingKeyFrutasSend);

var props = channel.CreateBasicProperties();
props.Persistent = true;
props.ContentType = "application/json";

Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Iniciando envio de frutas de época...\n");

foreach (var fruta in frutas)
{
    fruta.DataHoraSistema = DateTime.Now;

    var json = JsonConvert.SerializeObject(fruta, Formatting.Indented);
    var body = Encoding.UTF8.GetBytes(json);

    channel.BasicPublish(
        exchange: RabbitMQConstants.ExchangeFrutas,
        routingKey: RabbitMQConstants.RoutingKeyFrutasSend,
        basicProperties: props,
        body: body);

    Console.WriteLine($" Mensagem enviada → Exchange: [{RabbitMQConstants.ExchangeFrutas}]");
    Console.WriteLine($"   Routing Key : {RabbitMQConstants.RoutingKeyFrutasSend}");
    Console.WriteLine($"   Fruta       : {fruta.NomeFruta}");
    Console.WriteLine($"   Data/Hora   : {fruta.DataHoraSistema:dd/MM/yyyy HH:mm:ss}");
    Console.WriteLine($"   Descrição   : {fruta.Descricao}");
    Console.WriteLine(new string('-', 60));

    Thread.Sleep(500);
}

Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Todas as {frutas.Count} frutas foram enviadas para validação.");
Console.WriteLine("Pressione qualquer tecla para encerrar...");
Console.ReadKey();
