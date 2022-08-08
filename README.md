[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![Gitpod live code](https://img.shields.io/badge/Gitpod-live--code-blue?logo=gitpod&style=flat-square&color=ff69b4)](https://gitpod.io/#https://github.com/meysamhadeli/booking-modular-monolith)

![](./assets/booking-modular-monolith.png)

# Booking Modular Monolith

The main idea of creating this project is implementing an infrastructure for up and running Modular Monolith with the latest technology and architecture like Vertical Slice Architecture, CQRS, DDD, gRpc, RabbitMq, Cap in .Net, and we will not deal mainly with business.

# Table of Contents

- [The Goals of This Project](#the-goals-of-this-project)
- [Plan](#plan)
- [Technologies - Libraries](#technologies---libraries)
- [The Domain and Bounded Context - Service Boundary](#the-domain-and-bounded-context---service-boundary)
- [Structure of Project](#structure-of-project)
- [Prerequisites](#prerequisites)
- [How to Run](#how-to-run)
  - [Docker Compose](#docker-compose)
  - [Kubernetes](#kubernetes)
- [Support](#support)
- [Contribution](#contribution)

## The Goals of This Project

- Modular Monolith based on `Domain Driven Design (DDD)` implementation.
- Correct `separation of bounded contexts` for each module.
- Communication between bounded contexts (modules) through asynchronous `MessageBus` and `events`.
- Communication internally between our modules with `gRPC` synchronously.
- Simple `CQRS` implementation and Event Driven Architecture.
- Using `event store` to store all historical state of aggregate. (Bookin Module)
- Using `Best Practice` and `New Technologies` and `Design Patterns`.
- Using `Docker-Compose` and `Kubernetes` for our deployment mechanism.

## Plan

> 🌀This project is a work in progress, new features will be added over time.🌀

I will try to register future goals and additions in the [Issues](https://github.com/meysamhadeli/booking-modular-monolith/issues) section of this repository.

High-level plan is represented in the table

| Feature           | Status         |
| ----------------- | -------------- |
| API       | Completed ✔️   |
| Identity Module  | Completed ✔️   |
| Flight Module    | Completed ✔️   |
| Passenger Module | Completed ✔️   |
| Booking Module   | Completed ✔️   |
| Building Blocks   | Completed ✔️ |

## Technologies - Libraries

- ✔️ **[`.NET 6`](https://dotnet.microsoft.com/download)** - .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
- ✔️ **[`MVC Versioning API`](https://github.com/microsoft/aspnet-api-versioning)** - Set of libraries which add service API versioning to ASP.NET Web API, OData with ASP.NET Web API, and ASP.NET Core
- ✔️ **[`EF Core`](https://github.com/dotnet/efcore)** - Modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations
- ✔️ **[`Cap`](https://github.com/dotnetcore/CAP)** - An EventBus with local persistent message functionality for system integration in SOA or Microservice architecture.
- ✔️ **[`MediatR`](https://github.com/jbogard/MediatR)** - Simple, unambitious mediator implementation in .NET.
- ✔️ **[`FluentValidation`](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library for building strongly-typed validation rules
- ✔️ **[`Swagger & Swagger UI`](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** - Swagger tools for documenting API's built on ASP.NET Core
- ✔️ **[`Serilog`](https://github.com/serilog/serilog)** - Simple .NET logging with fully-structured events
- ✔️ **[`Polly`](https://github.com/App-vNext/Polly)** - Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner
- ✔️ **[`Scrutor`](https://github.com/khellang/Scrutor)** - Assembly scanning and decoration extensions for Microsoft.Extensions.DependencyInjection
- ✔️ **[`Opentelemetry-dotnet`](https://github.com/open-telemetry/opentelemetry-dotnet)** - The OpenTelemetry .NET Client
- ✔️ **[`DuendeSoftware IdentityServer`](https://github.com/DuendeSoftware/IdentityServer)** - The most flexible and standards-compliant OpenID Connect and OAuth 2.x framework for ASP.NET Core
- ✔️ **[`EasyCaching`](https://github.com/dotnetcore/EasyCaching)** - Open source caching library that contains basic usages and some advanced usages of caching which can help us to handle caching more easier.
- ✔️ **[`Mapster`](https://github.com/MapsterMapper/Mapster)** - Convention-based object-object mapper in .NET.
- ✔️ **[`Hellang.Middleware.ProblemDetails`](https://github.com/khellang/Middleware/tree/master/src/ProblemDetails)** - A middleware for handling exception in .Net Core
- ✔️ **[`IdGen`](https://github.com/RobThree/IdGen)** - Twitter Snowflake-alike ID generator for .Net
- ✔️ **[`MagicOnion`](https://github.com/Cysharp/MagicOnion)** - gRPC based HTTP/2 RPC Streaming Framework for .NET, .NET Core and Unity.
- ✔️ **[`EventStore`](https://github.com/EventStore/EventStore)** - The open-source, functional database with Complex Event Processing.

## The Domain And Bounded Context - Service Boundary

- `Identity Module`: The Identity Service is a bounded context for the authentication and authorization of users using [Identity Server](https://github.com/DuendeSoftware/IdentityServer). This service is responsible for creating new users and their corresponding roles and permissions using [.Net Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) and Jwt authentication and authorization.

- `Flight Module`: The Flight Service is a bounded context `CRUD` service to handle flight related operations.

- `Passenger Module`: The Passenger Service is a bounded context for managing passenger information, tracking activities and subscribing to get notification for out of stock products.

- `Booking Module`: The Booking Service is a bounded context for managing all operation related to booking ticket.

- `Api`: The Api Project use for hosting all modules in one place.

> Note: We don't have separated API project for each module because they are not microervice and shouldn't host separately, so for hosting all modules, we just use one Api project.

![](./assets/modular-monolith-diagram.jpg)

## Structure of Project

In this project I used a mix of [clean architecture](https://jasontaylor.dev/clean-architecture-getting-started/), [vertical slice architecture](https://jimmybogard.com/vertical-slice-architecture/) and I used [feature folder structure](http://www.kamilgrzybek.com/design/feature-folders/) to structure my files.

We have a separate module ([IdentityServer](https://github.com/DuendeSoftware/IdentityServer)) for authentication and authorization of each request. Once signed-in users are issued a JWT token. This token is used by other module to validate the user, read claims and allow access to authorized/role specific endpoints.

I used [MagicOnion](https://github.com/Cysharp/MagicOnion) for `sync` communication between our modules. This framework is based on `gRPC`, which is a fast and compact binary network transport for HTTP/2. However, unlike plain gRPC, it treats C# interfaces as a protocol schema, enabling seamless code sharing between C# projects without .proto (Protocol Buffers IDL).

I used [In-Memory Queue](https://github.com/yang-xiaodong/Savorboard.CAP.InMemoryMessageQueue) as my MessageBroker for `async` communication between modules using the eventual consistency mechanism. Each modules uses [Cap](https://github.com/dotnetcore/CAP) to interface with [In-Memory Queue](https://github.com/yang-xiaodong/Savorboard.CAP.InMemoryMessageQueue) for easy use messaging, availability, reliability, etc.

modules are `event based` which means they can publish and/or subscribe to any events occurring in the setup. By using this approach for communicating between modules, each module does not need to know about the other module or handle errors occurred in other modules.

I treat each request as a distinct use case or slice, encapsulating and grouping all concerns from front-end to back.
When adding or changing a feature in an application in n-tire architecture, we are typically touching many "layers" in an application. We are changing the user interface, adding fields to models, modifying validation, and so on. Instead of coupling across a layer, we couple vertically along a slice. We `minimize coupling` `between slices`, and `maximize coupling` `in a slice`.

With this approach, each of our vertical slices can decide for itself how to best fulfill the request. New features only add code, we're not changing shared code and worrying about side effects.

![](./assets/Vertical-Slice-Architecture.jpg)

Instead of grouping related action methods in one controller, as found in traditional ASP.net controllers, I used the [REPR pattern](https://deviq.com/design-patterns/repr-design-pattern). Each action gets its own small endpoint, consisting of a route, the action, and an `IMediator` instance (see [MediatR](https://github.com/jbogard/MediatR)). The request is passed to the `IMediator` instance, routed through a [`Mediatr pipeline`](https://lostechies.com/jimmybogard/2014/09/09/tackling-cross-cutting-concerns-with-a-mediator-pipeline/) where custom [middleware](https://github.com/jbogard/MediatR/wiki/Behaviors) can log, validate and intercept requests. The request is then handled by a request specific `IRequestHandler` which performs business logic before returning the result.

The use of the [mediator pattern](https://dotnetcoretutorials.com/2019/04/30/the-mediator-pattern-in-net-core-part-1-whats-a-mediator/) in my controllers creates clean and [thin controllers](https://codeopinion.com/thin-controllers-cqrs-mediatr/). By separating action logic into individual handlers we support the [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle) and [Don't Repeat Yourself principles](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself), this is because traditional controllers tend to become bloated with large action methods and several injected `Services` only being used by a few methods.

I used CQRS to decompose my features into small parts that makes our application:

- Maximize performance, scalability and simplicity.
- Easy to maintain and add features to. Changes only affect one command or query, avoiding breaking changes or creating side effects.
- It gives us better separation of concerns and cross-cutting concern (with help of mediatr behavior pipelines), instead of bloated service classes doing many things.

Using the CQRS pattern, we cut each business functionality into vertical slices, for each of these slices we group classes (see [technical folders structure](http://www.kamilgrzybek.com/design/feature-folders)) specific to that feature together (command, handlers, infrastructure, repository, controllers, etc). In our CQRS pattern each command/query handler is a separate slice. This is where you can reduce coupling between layers. Each handler can be a separated code unit, even copy/pasted. Thanks to that, we can tune down the specific method to not follow general conventions (e.g. use custom SQL query or even different storage). In a traditional layered architecture, when we change the core generic mechanism in one layer, it can impact all methods.

## How to Run

### Config Certificate

Run the following commands for [Config SSL](https://docs.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-6.0) in your system

```bash
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p {password here}
dotnet dev-certs https --trust
```

> Note: for running this command in `powershell` use `$env:USERPROFILE` instead of `%USERPROFILE%`

### Docker Compose

TODO 👷‍♂️
Deployment App in Docker-Compose

We have a seprate docker file for up and running [infrastracture.yaml](./deployments/docker-compose/infrastracture.yaml) independently:

```bash
docker-compose -f ./deployments/docker-compose/infrastracture.yaml up -d
```

### Documentation Apis

All modules uses swagger open api, navigate to /swagger for a list of every endpoint.
For testing apis I used the [REST Client](https://github.com/Huachao/vscode-restclient) plugin for VSCode running this file [booking.rest](./booking.rest).

# Support

If you like my work, feel free to:

- ⭐ this repository. And we will be happy together :)

Thanks a bunch for supporting me!

## Contribution

Thanks to all [contributors](https://github.com/meysamhadeli/booking-modular-monolith/graphs/contributors), you're awesome and this wouldn't be possible without you! The goal is to build a categorized community-driven collection of very well-known resources.

## Project References & Credits

- [https://github.com/jbogard/ContosoUniversityDotNetCore-Pages](https://github.com/jbogard/ContosoUniversityDotNetCore-Pages)
- [https://github.com/kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [https://github.com/oskardudycz/EventSourcing.NetCore](https://github.com/oskardudycz/EventSourcing.NetCore)
- [https://github.com/thangchung/clean-architecture-dotnet](https://github.com/thangchung/clean-architecture-dotnet)
- [https://github.com/jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)

## License
This project is made available under the MIT license. See [LICENSE](https://github.com/meysamhadeli/booking-modular-monolith/blob/main/LICENSE) for details.
