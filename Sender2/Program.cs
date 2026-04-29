using RabbitMQ.Client;
using Newtonsoft.Json;
using Shared;
using Shared.Models;
using System.Text;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║    SENDER 2 - Dados de Usuário (Producer)║");
Console.WriteLine("╚══════════════════════════════════════════╝");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

// Dados fictícios de usuários do Sistema de Gestão Hortifruti
var usuarios = new List<UsuarioMessage>
{
    new()
    {
        NomeCompleto = "Ana Paula Ferreira",
        Endereco = "Rua das Flores, 123 - Jardim Primavera, São Paulo - SP, 01234-567",
        RG = "12.345.678-9",
        CPF = "123.456.789-00",
        DataHoraRegistro = new DateTime(2025, 3, 10, 9, 30, 0)
    },
    new()
    {
        NomeCompleto = "Carlos Eduardo Mendes",
        Endereco = "Av. Brasil, 456 - Centro, Campinas - SP, 13001-001",
        RG = "98.765.432-1",
        CPF = "987.654.321-00",
        DataHoraRegistro = new DateTime(2025, 4, 22, 14, 15, 0)
    },
    new()
    {
        NomeCompleto = "Mariana Costa Oliveira",
        Endereco = "Rua do Comércio, 789 - Vila Nova, Guarulhos - SP, 07110-000",
        RG = "55.123.456-7",
        CPF = "555.123.456-78",
        DataHoraRegistro = new DateTime(2026, 1, 5, 8, 0, 0)
    }
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(
    exchange: RabbitMQConstants.ExchangeUsuarios,
    type: ExchangeType.Topic,
    durable: true,
    autoDelete: false);

channel.QueueDeclare(
    queue: RabbitMQConstants.QueueUsuariosValidation,
    durable: true,
    exclusive: false,
    autoDelete: false);

channel.QueueBind(
    queue: RabbitMQConstants.QueueUsuariosValidation,
    exchange: RabbitMQConstants.ExchangeUsuarios,
    routingKey: RabbitMQConstants.RoutingKeyUsuariosSend);

var props = channel.CreateBasicProperties();
props.Persistent = true;
props.ContentType = "application/json";

Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Iniciando envio de usuários do Sistema de Gestão Hortifruti...\n");

foreach (var usuario in usuarios)
{
    var json = JsonConvert.SerializeObject(usuario, Formatting.Indented);
    var body = Encoding.UTF8.GetBytes(json);

    channel.BasicPublish(
        exchange: RabbitMQConstants.ExchangeUsuarios,
        routingKey: RabbitMQConstants.RoutingKeyUsuariosSend,
        basicProperties: props,
        body: body);

    Console.WriteLine($"✅ Mensagem enviada → Exchange: [{RabbitMQConstants.ExchangeUsuarios}]");
    Console.WriteLine($"   Routing Key     : {RabbitMQConstants.RoutingKeyUsuariosSend}");
    Console.WriteLine($"   Nome            : {usuario.NomeCompleto}");
    Console.WriteLine($"   CPF             : {usuario.CPF}");
    Console.WriteLine($"   RG              : {usuario.RG}");
    Console.WriteLine($"   Endereço        : {usuario.Endereco}");
    Console.WriteLine($"   Data Registro   : {usuario.DataHoraRegistro:dd/MM/yyyy HH:mm:ss}");
    Console.WriteLine(new string('-', 60));

    Thread.Sleep(500);
}

Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Todos os {usuarios.Count} usuários foram enviados para validação.");
Console.WriteLine("Pressione qualquer tecla para encerrar...");
Console.ReadKey();
