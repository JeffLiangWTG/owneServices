using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public class JsonActionResult : IHttpActionResult
	{
		HttpStatusCode statusCode { get; set; }
		object data { get; set; }
		string jsonData;
		string JsonData
		{
			get
			{
				if (string.IsNullOrEmpty(jsonData))
				{
					jsonData = Json(data);
				}
				return jsonData;
			}
		}

		public JsonActionResult(HttpStatusCode statusCode, object data)
		{
			this.statusCode = statusCode;
			this.data = data;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			HttpResponseMessage response = new HttpResponseMessage(statusCode);
			response.Content = new CompressedStringContent(JsonData);
			return Task.FromResult(response);
		}

		string Json(object data)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}
	}

	public class CompressedStringContent : HttpContent
	{
		readonly HttpContent originalContent;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Content-Encoding")]
		const string EncodingType = "gzip";
		const string AppJsonMediaType = "application/json"; // Content-Type

		public CompressedStringContent(string content)
		{
			originalContent = new StringContent(content, Encoding.UTF8, AppJsonMediaType);
			foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
			{
				this.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			this.Headers.ContentEncoding.Add(EncodingType);
		}

		protected override bool TryComputeLength(out long length)
		{
			length = -1;

			return false;
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
		{
			Stream compressedStream = null;

			compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);

			return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
			{
				if (compressedStream != null)
				{
					compressedStream.Dispose();
				}
			});
		}
	}
}
