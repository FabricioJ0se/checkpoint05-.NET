namespace Shared
{
    public static class RabbitMQConstants
    {
        // Exchanges
        public const string ExchangeFrutas = "hortifruti.frutas.exchange";
        public const string ExchangeUsuarios = "hortifruti.usuarios.exchange";
        public const string ExchangeValidation = "hortifruti.validation.exchange";

        // Queues
        public const string QueueFrutasSender = "queue.frutas.sender";
        public const string QueueFrutasValidation = "queue.frutas.validation";
        public const string QueueFrutasReceiver = "queue.frutas.receiver";

        public const string QueueUsuariosSender = "queue.usuarios.sender";
        public const string QueueUsuariosValidation = "queue.usuarios.validation";
        public const string QueueUsuariosReceiver = "queue.usuarios.receiver";

        // Routing Keys
        public const string RoutingKeyFrutasSend = "frutas.send";
        public const string RoutingKeyFrutasValidated = "frutas.validated";

        public const string RoutingKeyUsuariosSend = "usuarios.send";
        public const string RoutingKeyUsuariosValidated = "usuarios.validated";
    }
}
