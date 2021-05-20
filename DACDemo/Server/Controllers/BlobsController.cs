using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DACDemo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace DACDemo.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BlobsController : ControllerBase
    {
        private const string UploadedBy = "UploadedBy";
        private readonly ILogger<BlobsController> _logger;
        private readonly BlobContainerClient _blobClient;
        private readonly GraphServiceClient _graphServiceClient;

        public BlobsController(ILogger<BlobsController> logger, BlobContainerClient blobClient, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _blobClient = blobClient;
            _graphServiceClient = graphServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> ListBlobs()
        {
            var result = new List<BlobDetails>();
            try
            {
                await foreach (var blob in _blobClient.GetBlobsAsync(BlobTraits.Metadata))
                {
                    result.Add(new BlobDetails { Name = blob.Name, LastModified = blob.Properties.LastModified.Value.DateTime, UploadedBy = blob.Metadata[UploadedBy] });
                }
            }
            catch (Exception ex)
            {
                result.Add(new BlobDetails { LastModified = DateTime.Now, Name = ex.Message });
            }
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> CreateBlob()
        {
            string name = "dac-demo-web";
            try
            {
                var user = await _graphServiceClient.Me.Request().GetAsync();
                name = user.DisplayName;
            }
            catch
            { }
            var bc = _blobClient.GetBlobClient(Guid.NewGuid().ToString());
            await bc.UploadAsync(new BinaryData("").ToStream(), true);
            await bc.SetMetadataAsync(new Dictionary<string, string> { { UploadedBy, name } });

            return Ok("Created Successfully.");
        }
    }
}
