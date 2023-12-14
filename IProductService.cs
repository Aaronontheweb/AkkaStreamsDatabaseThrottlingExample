// -----------------------------------------------------------------------
//  <copyright file="IProductService.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Hosting;
using MyAkkaApp.Actors;

namespace MyAkkaApp;

public interface IProductService{
    Task<ProductIndexResponse?> GetProductIndexOrNullAsync(string id, 
        CancellationToken cancellationToken = default);

    // i.e. other CRUD methods pertaining to Products
}

public sealed record ProductIndexResponse(string Id, string PublisherId);

public sealed class ThrottledImplementation : IProductService
{
    private readonly IActorRef _throttler;

    public ThrottledImplementation(IRequiredActor<ThrottlerActor> throttler)
    {
        _throttler = throttler.ActorRef;
    }

    public Task<ProductIndexResponse?> GetProductIndexOrNullAsync(string id, CancellationToken cancellationToken = default)
    {
        return _throttler.Ask<ProductIndexResponse?>(new ReadProductById(id, cancellationToken), cancellationToken: cancellationToken);
    }
}

public sealed class ConcreteDatabaseImplementation : IProductService{
    
    // i.e. implementations of interface methods using EF et al
    public async Task<ProductIndexResponse?> GetProductIndexOrNullAsync(string id, CancellationToken cancellationToken = default)
    {
        // i.e. EF query
        Console.WriteLine("Did the thing");

        return new ProductIndexResponse("foo", "bar");
    }
}