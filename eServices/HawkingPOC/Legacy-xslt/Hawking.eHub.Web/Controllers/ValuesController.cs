using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Hawking.eHub.Web.Controllers
{
    public class ValuesController : ApiController
    {
        Dictionary<int, string> values = new Dictionary<int, string>();

        // GET api/values
        public IEnumerable<string> Get()
        {
            return values.Values;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return values[id];
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
            values[values.Count] = value;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
            values[5] = value;
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
            values.Remove(id);
        }
    }
}
