// -----------------------------------------------------------------------
//  <copyright file="ThrottlerActor.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace MyAkkaApp.Actors;

public sealed class ThrottlerActor : ReceiveActor
{
    private readonly IServiceProvider _sp;
    private ISourceQueueWithComplete<(IDbOperation, IActorRef)> _dbOperationQueue;

    public ThrottlerActor(IServiceProvider sp)
    {
        _sp = sp;
        
        ReceiveAsync<IDbOperation>(async dbOp =>
        {
            // will block and buffer messages inside actor's mailbox once queue reaches capacity
            await _dbOperationQueue!.OfferAsync((dbOp, Sender));
        });
    }

    protected override void PreStart()
    {
        // create an Akka.Streams graph that will accept messages of type (IDbOperation, IActorRef) using a Source.Queue
        // and then process them using a Flow that will dispatch them to the correct ConcreteDatabaseImplementation method using 
        // a RunForEachAsync sink
        _dbOperationQueue = Source.Queue<(IDbOperation, IActorRef)>(100, OverflowStrategy.Backpressure)
            .ToMaterialized(Sink.ForEachAsync<(IDbOperation, IActorRef)>(10, async tuple =>
            {
                var (dbOperation, replyTo) = tuple;
                if (dbOperation.CancellationToken.IsCancellationRequested)
                {
                    // if the cancellation token is already cancelled, we can't do anything
                    Sender.Tell("request failed");
                }
                
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ConcreteDatabaseImplementation>();
                
                switch (dbOperation)
                {
                    case ReadProductById readProductById:
                        var result = await 
                            db.GetProductIndexOrNullAsync(readProductById.ProductId, readProductById.CancellationToken);
                        replyTo.Tell(result);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported IDbOperation type [{dbOperation.GetType()}]");
                }
            }), Keep.Left)
            .Run(Context.Materializer());
    }
    
    protected override void PostStop()
    {
        _dbOperationQueue?.Complete();
    }
}