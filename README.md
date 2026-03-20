Projeto: GBastos.Hexagon-Skill_Test_Backend



Descrição


A GBastos.Hexagon-Skill_Test_Backend é uma API RESTful construída com .NET 9 minimal API, seguindo padrões 
de arquitetura Hexagonal/Clean Architecture.


Foi desenvolvida para gerenciar dados de usuarios, inclui login, é um CRUD completo, integrado por mensageria 
RabbitMQ (sob Outbox Pattern). Preparada para, em DEV, utilizar banco de dados SQLite e, em PROD, utilizar SQL Server no Docker.


Tecnologias e Métodos Utilizados

- .NET 9 – framework principal da API minimal
- C# 12 – linguagem de desenvolvimento
- Entity Framework Core 9 – ORM para persistência de dados
- SQLite / SQL Server – banco de dados
- MediatR – implementação de CQRS e patterns de Commands / Queries
- RabbitMQ – broker de mensagens para integração e eventos assíncronos
- Outbox Pattern – garante consistência de mensagens mesmo em falhas de transações
- JWT (JSON Web Tokens) – autenticação e autorização
- Swagger / OpenAPI – documentação interativa da API

    
Pré-requisitos

Antes de rodar a API, certifique-se de ter instalado:

- .NET 9 SDK
- RabbitMQ (local ou em container Docker)

Um editor de código como [Visual Studio 2022+] ou [VSCode]


Opcional para SQLite: não é necessário servidor adicional.
