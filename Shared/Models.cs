namespace Shared.Models
{
    public class FrutaMessage
    {
        public string NomeFruta { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataHoraSistema { get; set; }
    }

    public class UsuarioMessage
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string RG { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateTime DataHoraRegistro { get; set; }
    }

    public class ValidationMessage<T>
    {
        public T? Dados { get; set; }
        public bool Validado { get; set; }
        public string MensagemValidacao { get; set; } = string.Empty;
    }
}
