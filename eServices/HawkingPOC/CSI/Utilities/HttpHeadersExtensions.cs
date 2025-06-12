using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Utilities
{
    public static class HttpHeadersExtensions
    {
        public static IDictionary<string, StringValues> GetCustomHeaders(this HttpRequestMessage request)
        {
            return request.Headers.Where(h => h.Key.StartsWith("X-"))
                .ToDictionary(h => h.Key, h => new StringValues(h.Value?.ToArray()));
        }

        public static IDictionary<string, StringValues> GetCustomHeaders(this HttpResponseMessage response)
        {
            return response.Headers.Where(h => h.Key.StartsWith("X-"))
                .ToDictionary(h => h.Key, h => new StringValues(h.Value?.ToArray()));
        }

        public static IDictionary<string, StringValues> GetCustomHeaders(this HttpRequest request)
        {
            return request.Headers.Where(h => h.Key.StartsWith("X-"))
                .ToDictionary(h => h.Key, h => h.Value);
        }

        public static IDictionary<string, StringValues> GetCustomHeaders(this HttpResponse response)
        {
            return response.Headers.Where(h => h.Key.StartsWith("X-"))
                .ToDictionary(h => h.Key, h => h.Value);
        }

        public static void CopyTo(this IDictionary<string, IEnumerable<string>> source, HttpHeaders target)
        {
            foreach (var item in source)
                target.Add(item.Key, item.Value);
        }

        public static void CopyTo(this IDictionary<string, StringValues> source, HttpHeaders target)
        {
            foreach (var item in source)
                target.Add(item.Key, (IEnumerable<string>)item.Value);
        }

        public static void CopyTo(this IDictionary<string, IEnumerable<string>> source, IHeaderDictionary target)
        {
            foreach (var item in source)
                target.Add(item.Key, item.Value.ToArray());
        }

        public static void CopyTo(this IDictionary<string, StringValues> source, IHeaderDictionary target)
        {
            foreach (var item in source)
                target.Add(item.Key, item.Value);
        }
    }
}