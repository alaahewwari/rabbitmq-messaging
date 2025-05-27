# 📦 RabbitMQ Messaging System

This project is a demonstration of distributed messaging using RabbitMQ. It features three microservices:

- A **Sender** service that sends messages to a queue.
- An **Email Receiver** that consumes email-related messages.
- An **SMS Receiver** that consumes SMS-related messages.

Each service is implemented using **.NET 8** and communicates via **RabbitMQ**.

---

## 🧾 Project Structure

```
RabbitMq/
│
├── RabbitMqSender/               # Service to send messages
│   ├── Program.cs
│   └── Dockerfile
│
├── RabbitMqEmailReceiver/       # Email message consumer
│   ├── Program.cs
│   └── Dockerfile
│
└── RabbitMqSmsReceiver/         # SMS message consumer
    ├── Program.cs
    └── Dockerfile
```

---

## 🚀 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/)
- [RabbitMQ](https://www.rabbitmq.com/) (can be run locally or via Docker)

---

## 🐳 Running the Services with Docker Compose

To spin up the entire system including RabbitMQ and all services, simply run:

```bash
docker-compose up -d
```

This will:
- Start a RabbitMQ broker with the management UI at [http://localhost:15672](http://localhost:15672)
- Start the Sender, Email Receiver, and SMS Receiver services

---

## 🧪 Testing

Once the containers are running:
- The Sender service will automatically start sending test messages.
- The Email and SMS Receivers will consume and process messages from their respective queues.
- Check the logs or console output to verify the message flow.
