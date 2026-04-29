# 🥦 HortifrutiMQ — API com Microservices e Mensageria

> **FIAP – Faculdade de Informática e Administração Paulista**
> Curso de Tecnologia em Análise e Desenvolvimento de Sistemas (TDS)
> Professor: Dr. Marcel Stefan Wagner — Checkpoint 5

---

## 👥 Integrantes do Grupo

| Nome Completo | RM |
|---|---|
| _(Integrante 1)_ | RM000000 |
| _(Integrante 2)_ | RM000000 |
| _(Integrante 3)_ | RM000000 |

> ⚠️ Substitua com os nomes e RMs reais do seu grupo.

---

## 📋 Descrição do Projeto

Sistema de mensageria assíncrona para um **Sistema de Gestão Hortifruti**, composto por dois fluxos independentes:

- **Fluxo 1 – Frutas de Época:** Sender 1 → RabbitMQ → Validation → RabbitMQ → Receiver 1
- **Fluxo 2 – Dados de Usuário:** Sender 2 → RabbitMQ → Validation → RabbitMQ → Receiver 2

Tecnologias utilizadas: **.NET 8**, **RabbitMQ 3**, **Docker Desktop**, **Newtonsoft.Json**.

---

## 🏗️ Arquitetura

```
SENDER 1 (Producer)                               RECEIVER 1 (Consumer)
  - Data/Hora Sistema         ──► RabbitMQ ──►  VALIDATION  ──► RabbitMQ ──►  - Data/Hora Sistema
  - Info Frutas de Época         (Broker)       (Consumer +      (Broker)       - Informação Validada?
                                                  Producer)                     - Informação enviada

SENDER 2 (Producer)                               RECEIVER 2 (Consumer)
  - Data/Hora de Registro     ──► RabbitMQ ──►  VALIDATION  ──► RabbitMQ ──►  - Data/Hora Registro
  - Dados do Usuário             (Broker)       (Consumer +      (Broker)       - Informação Validada?
                                                  Producer)                     - Dados do Usuário
```

---

## 📁 Estrutura de Projetos

```
HortifrutiMQ/
├── HortifrutiMQ.sln
├── docker-compose.yml
├── README.md
├── Shared/                  ← Modelos e constantes compartilhados
│   ├── Models.cs
│   ├── RabbitMQConstants.cs
│   └── Shared.csproj
├── Sender1/                 ← Producer: envia frutas de época
│   ├── Program.cs
│   └── Sender1.csproj
├── Sender2/                 ← Producer: envia dados de usuário
│   ├── Program.cs
│   └── Sender2.csproj
├── Validation/              ← Consumer + Producer: valida e repassa
│   ├── Program.cs
│   └── Validation.csproj
├── Receiver1/               ← Consumer: recebe frutas validadas
│   ├── Program.cs
│   └── Receiver1.csproj
└── Receiver2/               ← Consumer: recebe usuários validados
    ├── Program.cs
    └── Receiver2.csproj
```

---

## 🐇 Exchanges, Queues e Routing Keys

### Exchanges

| Exchange | Tipo | Finalidade |
|---|---|---|
| `hortifruti.frutas.exchange` | Topic | Recebe mensagens do Sender 1 |
| `hortifruti.usuarios.exchange` | Topic | Recebe mensagens do Sender 2 |
| `hortifruti.validation.exchange` | Topic | Distribui mensagens validadas para os Receivers |

### Queues (Filas)

| Fila | Ligada à Exchange | Finalidade |
|---|---|---|
| `queue.frutas.validation` | `hortifruti.frutas.exchange` | Aguarda frutas para validar |
| `queue.usuarios.validation` | `hortifruti.usuarios.exchange` | Aguarda usuários para validar |
| `queue.frutas.receiver` | `hortifruti.validation.exchange` | Entrega frutas validadas ao Receiver 1 |
| `queue.usuarios.receiver` | `hortifruti.validation.exchange` | Entrega usuários validados ao Receiver 2 |

### Routing Keys

| Routing Key | Origem → Destino |
|---|---|
| `frutas.send` | Sender 1 → Validation |
| `frutas.validated` | Validation → Receiver 1 |
| `usuarios.send` | Sender 2 → Validation |
| `usuarios.validated` | Validation → Receiver 2 |

---

## 🐳 Configuração do Docker

### Pré-requisito
Ter o **Docker Desktop** instalado e em execução.

### Subir o container RabbitMQ

```bash
docker-compose up -d
```

Isso cria um container chamado `hortifruti-rabbitmq` com:
- **Porta 5672** → protocolo AMQP (conexão das aplicações)
- **Porta 15672** → painel de administração HTTP

### Verificar o container

```bash
docker ps
```

### Acessar o painel de administração

Abra o navegador em: **http://localhost:15672**
- Usuário: `guest`
- Senha: `guest`

No painel você pode visualizar: Connections, Channels, Exchanges, Queues e mensagens em tempo real.

---

## ▶️ Como executar

> Execute cada projeto em um terminal separado, **na ordem abaixo**.

### 1. Subir o RabbitMQ

```bash
docker-compose up -d
```

### 2. Iniciar o Validation (deve estar rodando antes dos Senders)

```bash
cd Validation
dotnet run
```

### 3. Iniciar o Receiver 1 (Frutas)

```bash
cd Receiver1
dotnet run
```

### 4. Iniciar o Receiver 2 (Usuários)

```bash
cd Receiver2
dotnet run
```

### 5. Executar o Sender 1 (Frutas de Época)

```bash
cd Sender1
dotnet run
```

### 6. Executar o Sender 2 (Dados de Usuário)

```bash
cd Sender2
dotnet run
```

---

## 🧪 Exemplos de Execução e Testes

### Par Sender 1 / Receiver 1 — Frutas de Época

**Sender 1 — saída esperada no terminal:**
```
╔══════════════════════════════════════════╗
║     SENDER 1 - Frutas de Época (Producer) ║
╚══════════════════════════════════════════╝

[28/04/2026 10:00:00] Iniciando envio de frutas de época...

✅ Mensagem enviada → Exchange: [hortifruti.frutas.exchange]
   Routing Key : frutas.send
   Fruta       : Manga
   Data/Hora   : 28/04/2026 10:00:00
   Descrição   : Fruta tropical doce e suculenta...
------------------------------------------------------------
```

**Validation — saída esperada ao receber fruta:**
```
[28/04/2026 10:00:00] 🍓 Mensagem de FRUTA recebida para validação:
{
  "NomeFruta": "Manga",
  "Descricao": "Fruta tropical doce e suculenta...",
  "DataHoraSistema": "2026-04-28T10:00:00"
}
   ✅ Validação OK: Fruta 'Manga' validada com sucesso em 28/04/2026 10:00:01.
   📤 Resultado enviado → Exchange: [hortifruti.validation.exchange] | RK: [frutas.validated]
```

**Receiver 1 — saída esperada:**
```
[28/04/2026 10:00:01] 🍓 Fruta recebida:
   Nome       : Manga
   Descrição  : Fruta tropical doce e suculenta...
   Data/Hora  : 28/04/2026 10:00:00
   Validado?  : ✅ SIM
   Resultado  : Fruta 'Manga' validada com sucesso em 28/04/2026 10:00:01.
------------------------------------------------------------
```

---

### Par Sender 2 / Receiver 2 — Dados de Usuário

**Sender 2 — saída esperada:**
```
╔══════════════════════════════════════════╗
║    SENDER 2 - Dados de Usuário (Producer) ║
╚══════════════════════════════════════════╝

[28/04/2026 10:01:00] Iniciando envio de usuários...

✅ Mensagem enviada → Exchange: [hortifruti.usuarios.exchange]
   Routing Key     : usuarios.send
   Nome            : Ana Paula Ferreira
   CPF             : 123.456.789-00
   RG              : 12.345.678-9
   Endereço        : Rua das Flores, 123 - Jardim Primavera...
   Data Registro   : 10/03/2025 09:30:00
```

**Receiver 2 — saída esperada:**
```
[28/04/2026 10:01:01] 👤 Usuário recebido:
   Nome            : Ana Paula Ferreira
   CPF             : 123.456.789-00
   RG              : 12.345.678-9
   Endereço        : Rua das Flores, 123 - Jardim Primavera, São Paulo - SP
   Data Registro   : 10/03/2025 09:30:00
   Validado?       : ✅ SIM
   Resultado       : Usuário 'Ana Paula Ferreira' validado com sucesso em 28/04/2026 10:01:01.
```

---

## ✅ Processo de Validação

### Fluxo 1 — Frutas de Época

O **Validation** consome da fila `queue.frutas.validation` e aplica as seguintes regras:

| Regra | Campo | Critério |
|---|---|---|
| 1 | `NomeFruta` | Não pode ser vazio |
| 2 | `Descricao` | Não pode ser vazia |
| 3 | `Descricao` | Mínimo 10 caracteres |
| 4 | `DataHoraSistema` | Deve ser uma data válida (não `default`) |

Se todas as regras passarem → `Validado = true` e a mensagem é publicada na `queue.frutas.receiver`.
Se qualquer regra falhar → `Validado = false` com o motivo descrito em `MensagemValidacao`.

### Fluxo 2 — Dados de Usuário

O **Validation** consome da fila `queue.usuarios.validation` e aplica:

| Regra | Campo | Critério |
|---|---|---|
| 1 | `NomeCompleto` | Não pode ser vazio |
| 2 | `CPF` | Não pode ser vazio e deve ter ao menos 11 caracteres |
| 3 | `RG` | Não pode ser vazio |
| 4 | `Endereco` | Não pode ser vazio |
| 5 | `DataHoraRegistro` | Deve ser uma data válida (não `default`) |

Se todas as regras passarem → `Validado = true` e a mensagem é publicada na `queue.usuarios.receiver`.

---

## 🔍 Visualizando no RabbitMQ Management

Após executar os projetos, acesse **http://localhost:15672** e verifique:

- **Connections:** conexões ativas de cada projeto (Sender1, Sender2, Validation, Receiver1, Receiver2)
- **Channels:** canais abertos por cada conexão
- **Exchanges:** as 3 exchanges criadas (`frutas`, `usuarios`, `validation`)
- **Queues:** as 4 filas com contagem de mensagens em tempo real
- **Bindings:** vinculações de routing key entre exchanges e filas

---

## 📦 Dependências NuGet

| Pacote | Versão | Uso |
|---|---|---|
| `RabbitMQ.Client` | 6.8.1 | Conexão e comunicação com RabbitMQ |
| `Newtonsoft.Json` | 13.0.3 | Serialização/desserialização de mensagens JSON |
