// -----------------------------------------------------------------------
//  <copyright file="ProductController.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

namespace MyAkkaApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : Controller
{
    private readonly IProductService _products;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService products, ILogger<ProductController> logger)
    {
        _products = products;
        _logger = logger;;
    }

    [HttpGet("index.json")]
    public async Task<ActionResult<ProductIndexResponse>> ProductIndexAsync(string id, CancellationToken cancellationToken)
    {
        var index = await _products.GetProductIndexOrNullAsync(id, cancellationToken);
        if (index == null)
        {
            return NotFound();
        }

        return index;
    }
}