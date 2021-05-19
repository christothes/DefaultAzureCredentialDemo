using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using DACDemo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DACDemo.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BlobsController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<BlobsController> _logger;
        private readonly BlobContainerClient _blobClient;

        public BlobsController(ILogger<BlobsController> logger, BlobContainerClient blobClient)
        {
            _logger = logger;
            _blobClient = blobClient;
        }

        [HttpGet]
        public async Task<IActionResult> ListBlobs()
        {
            var result = new List<BlobDetails>();
            try
            {
                await foreach (var blob in _blobClient.GetBlobsAsync())
                {
                    result.Add(new BlobDetails { Name = blob.Name, LastModified = DateTime.Parse(blob.Metadata["LastModified"]) });
                }
            }
            catch (Exception ex)
            {
                result.Add(new BlobDetails { LastModified = DateTime.Now, Name = ex.Message });
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult CreateBlob()
        {
            return Ok();
        }
    }
}
