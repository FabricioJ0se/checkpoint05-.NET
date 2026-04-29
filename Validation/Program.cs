using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Shared;
using Shared.Models;
using System.Text;

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║  VALIDATION - Consumer + Producer (Middleware)    ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Aguardando mensagens para validação...\n");

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// ── Exchanges ──────────────────────────────────────────────
// Frutas: entrada e saída
channel.ExchangeDeclare(RabbitMQConstants.ExchangeFrutas,    ExchangeType.Topic, durable: true);
channel.ExchangeDeclare(RabbitMQConstants.ExchangeValidation, ExchangeType.Topic, durable: true);

// Usuários: entrada e saída
channel.ExchangeDeclare(RabbitMQConstants.ExchangeUsuarios,   ExchangeType.Topic, durable: true);

// ── Filas de entrada (vindas dos Senders) ──────────────────
channel.QueueDeclare(RabbitMQConstants.QueueFrutasValidation,   durable: true, exclusive: false, autoDelete: false);
channel.QueueDeclare(RabbitMQConstants.QueueUsuariosValidation, durable: true, exclusive: false, autoDelete: false);

channel.QueueBind(RabbitMQConstants.QueueFrutasValidation,   RabbitMQConstants.ExchangeFrutas,   RabbitMQConstants.RoutingKeyFrutasSend);
channel.QueueBind(RabbitMQConstants.QueueUsuariosValidation, RabbitMQConstants.ExchangeUsuarios, RabbitMQConstants.RoutingKeyUsuariosSend);

// ── Filas de saída (para os Receivers) ────────────────────
channel.QueueDeclare(RabbitMQConstants.QueueFrutasReceiver,   durable: true, exclusive: false, autoDelete: false);
channel.QueueDeclare(RabbitMQConstants.QueueUsuariosReceiver, durable: true, exclusive: false, autoDelete: false);

channel.QueueBind(RabbitMQConstants.QueueFrutasReceiver,   RabbitMQConstants.ExchangeValidation, RabbitMQConstants.RoutingKeyFrutasValidated);
channel.QueueBind(RabbitMQConstants.QueueUsuariosReceiver, RabbitMQConstants.ExchangeValidation, RabbitMQConstants.RoutingKeyUsuariosValidated);

channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

// ── Consumer de Frutas ─────────────────────────────────────
var frutasConsumer = new EventingBasicConsumer(channel);
frutasConsumer.Received += (sender, ea) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] 🍓 Mensagem de FRUTA recebida para validação:");
    Console.WriteLine(json);

    try
    {
        var fruta = JsonConvert.DeserializeObject<FrutaMessage>(json)!;

        var resultado = new ValidationMessage<FrutaMessage>
        {
            Dados = fruta,
            Validado = ValidarFruta(fruta, out string motivo),
            MensagemValidacao = motivo
        };

        if (resultado.Validado)
            Console.WriteLine($"   ✅ Validação OK: {resultado.MensagemValidacao}");
        else
            Console.WriteLine($"   ❌ Validação FALHOU: {resultado.MensagemValidacao}");

        var resultJson = JsonConvert.SerializeObject(resultado, Formatting.Indented);
        var resultBody = Encoding.UTF8.GetBytes(resultJson);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        channel.BasicPublish(
            exchange: RabbitMQConstants.ExchangeValidation,
            routingKey: RabbitMQConstants.RoutingKeyFrutasValidated,
            basicProperties: props,
            body: resultBody);

        Console.WriteLine($"   📤 Resultado enviado → Exchange: [{RabbitMQConstants.ExchangeValidation}] | RK: [{RabbitMQConstants.RoutingKeyFrutasValidated}]");

        channel.BasicAck(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Erro ao processar fruta: {ex.Message}");
        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
    }
};

// ── Consumer de Usuários ───────────────────────────────────
var usuariosConsumer = new EventingBasicConsumer(channel);
usuariosConsumer.Received += (sender, ea) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);

    Console.WriteLine($"\n[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] 👤 Mensagem de USUÁRIO recebida para validação:");
    Console.WriteLine(json);

    try
    {
        var usuario = JsonConvert.DeserializeObject<UsuarioMessage>(json)!;

        var resultado = new ValidationMessage<UsuarioMessage>
        {
            Dados = usuario,
            Validado = ValidarUsuario(usuario, out string motivo),
            MensagemValidacao = motivo
        };

        if (resultado.Validado)
            Console.WriteLine($"   ✅ Validação OK: {resultado.MensagemValidacao}");
        else
            Console.WriteLine($"   ❌ Validação FALHOU: {resultado.MensagemValidacao}");

        var resultJson = JsonConvert.SerializeObject(resultado, Formatting.Indented);
        var resultBody = Encoding.UTF8.GetBytes(resultJson);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        channel.BasicPublish(
            exchange: RabbitMQConstants.ExchangeValidation,
            routingKey: RabbitMQConstants.RoutingKeyUsuariosValidated,
            basicProperties: props,
            body: resultBody);

        Console.WriteLine($"   📤 Resultado enviado → Exchange: [{RabbitMQConstants.ExchangeValidation}] | RK: [{RabbitMQConstants.RoutingKeyUsuariosValidated}]");

        channel.BasicAck(ea.DeliveryTag, multiple: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  Erro ao processar usuário: {ex.Message}");
        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
    }
};

// Inicia os consumers
channel.BasicConsume(queue: RabbitMQConstants.QueueFrutasValidation,   autoAck: false, consumer: frutasConsumer);
channel.BasicConsume(queue: RabbitMQConstants.QueueUsuariosValidation, autoAck: false, consumer: usuariosConsumer);

Console.WriteLine("Pressione [ENTER] para encerrar o Validation...");
Console.ReadLine();

// ── Funções de Validação ───────────────────────────────────

static bool ValidarFruta(FrutaMessage fruta, out string motivo)
{
    if (string.IsNullOrWhiteSpace(fruta.NomeFruta))
    {
        motivo = "Nome da fruta não pode ser vazio.";
        return false;
    }
    if (string.IsNullOrWhiteSpace(fruta.Descricao))
    {
        motivo = "Descrição da fruta não pode ser vazia.";
        return false;
    }
    if (fruta.Descricao.Length < 10)
    {
        motivo = "Descrição da fruta muito curta (mínimo 10 caracteres).";
        return false;
    }
    if (fruta.DataHoraSistema == default)
    {
        motivo = "Data/hora do sistema inválida.";
        return false;
    }
    motivo = $"Fruta '{fruta.NomeFruta}' validada com sucesso em {DateTime.Now:dd/MM/yyyy HH:mm:ss}.";
    return true;
}

static bool ValidarUsuario(UsuarioMessage usuario, out string motivo)
{
    if (string.IsNullOrWhiteSpace(usuario.NomeCompleto))
    {
        motivo = "Nome completo não pode ser vazio.";
        return false;
    }
    if (string.IsNullOrWhiteSpace(usuario.CPF) || usuario.CPF.Length < 11)
    {
        motivo = "CPF inválido.";
        return false;
    }
    if (string.IsNullOrWhiteSpace(usuario.RG))
    {
        motivo = "RG não pode ser vazio.";
        return false;
    }
    if (string.IsNullOrWhiteSpace(usuario.Endereco))
    {
        motivo = "Endereço não pode ser vazio.";
        return false;
    }
    if (usuario.DataHoraRegistro == default)
    {
        motivo = "Data/hora de registro inválida.";
        return false;
    }
    motivo = $"Usuário '{usuario.NomeCompleto}' validado com sucesso em {DateTime.Now:dd/MM/yyyy HH:mm:ss}.";
    return true;
}
