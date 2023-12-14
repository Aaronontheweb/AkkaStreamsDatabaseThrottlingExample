// -----------------------------------------------------------------------
//  <copyright file="IDbOperation.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace MyAkkaApp;

/// <summary>
/// Use a message to describe what type of database operation we want to perform.
/// </summary>
public interface IDbOperation
{
    CancellationToken CancellationToken { get; }
}

public sealed record ReadProductById(string ProductId, 
    CancellationToken CancellationToken) : IDbOperation;