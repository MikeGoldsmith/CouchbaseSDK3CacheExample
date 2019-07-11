using System;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KeyNotFoundException = Couchbase.KeyNotFoundException;

namespace CouchbaseSDK3CacheExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ICollection _collection;

        public ValuesController(ILogger logger, ICollection collection)
        {
            _logger = logger;
            _collection = collection;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            string value;
            try
            {
                // get cache entry
                var result = await _collection.Get($"key:{id}");

                // cache hit
                value = result.ContentAs<string>();
            }
            catch (KeyNotFoundException)
            {
                // cache miss - get value from permanent storage
                value = "some value";

                // repopulate cache so subsequent calls get cache hit
                // set document TTL (expiration) to so cache ejects it after some time
                await _collection.Insert(
                    $"key:{id}",
                    value,
                    options => options.WithExpiration(TimeSpan.FromSeconds(10))
                );
            }
            catch (CouchbaseException)
            {
                // error performing get
                throw;
            }

            return value;
        }

        // POST api/values
        [HttpPost]
        public async Task Post(int id, [FromBody] string value)
        {
            try
            {
                // insert cache entry
                await _collection.Insert(
                    $"key:{id}",
                    value,
                    options => options.WithExpiration(TimeSpan.FromSeconds(10))
                );
            }
            catch (KeyExistsException)
            {
                // cache key already exists, use PUT instead
            }
            catch (CouchbaseException)
            {
                // error performing insert
                throw;
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] string value)
        {
            try
            {
                // add / update cache entry
                await _collection.Upsert(
                    $"key:{id}",
                    value,
                    options => options.WithExpiration(TimeSpan.FromSeconds(10))
                );
            }
            catch (CouchbaseException)
            {
                // error performing upsert
                throw;
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            try
            {
                // remove cache entry
            }
            catch (KeyNotFoundException)
            {
                // cache key doesn't exist
                throw;
            }
        }
    }
}
