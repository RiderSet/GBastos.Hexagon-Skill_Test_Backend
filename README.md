Projeto: GBastos.Hexagon-Skill_Test_Backend


Descrição

A GBastos.Hexagon-Skill_Test_Backend é uma API "minimal", RESTful, construída com .NET 9, seguindo padrões 
de arquitetura Hexagonal/Clean Architecture.

Foi desenvolvida para gerenciar dados de usuarios. Com login, é um CRUD completo, integrado por mensageria 
RabbitMQ (sob Outbox Pattern), preparada para, em DEV, trabalhar com banco de dados SQLite e, em PROD, com SQL Server no Docker.


Tecnologias e Métodos Utilizados

- .NET 9 – framework principal da API minimal
- C# 12 – linguagem de desenvolvimento
- Entity Framework Core 9 – ORM para persistência de dados
- SQLite / SQL Server – banco de dados
- Docker – para garantir funcionamento e versão do BD em qualquer ambiente
- RabbitMQ – broker de mensagens para integração e eventos assíncronos
- Outbox Pattern – garante consistência de mensagens mesmo em falhas de transações
- JWT (JSON Web Tokens) – autenticação e autorização
- Swagger / OpenAPI – documentação interativa da API


Padrões

- Outbox Pattern
- RabbitMQ Publisher
- RequireAuthorization
- Guid como Id
- Paginação no GET
- Minimal API


Pré-requisitos

Antes de rodar a API, certifique-se de:

- .NET 9 SDK
- RabbitMQ (local ou em container Docker)
- Um editor de código como [Visual Studio 2022+] ou [VSCode]


- Em PROD: para o SQL Server, execute o Docker
- Em DEV: para o SQLite, não é necessário servidor adicional.
  

Obs.:
  Em ambos os casos, não se esqueça de rodar o Docker (perdi uma ótima vaga de emprego por isso)!
