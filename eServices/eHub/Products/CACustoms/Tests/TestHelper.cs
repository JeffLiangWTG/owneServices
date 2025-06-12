using System;
using System.Linq;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Common.Extensions;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics;
using Microsoft.BizTalk.TestTools.Schema;
using CargoWise.BizTalk.UnitTestFX;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace CargoWise.eHub.Products.CACustoms.Tests
{
	internal class TestHelper
	{
		public static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		public static string GetEmbeddedResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		public static byte[] GetEmbeddedResourceAsByteArray(string resourceName)
		{
			var buffer = new byte[16*1024];

			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var memoryStream = new MemoryStream())
				{
					int read;
					while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						memoryStream.Write(buffer, 0, read);
					}
					return memoryStream.ToArray();
				}
			}
		}

		public static bool CompareStreams(Stream expectedStream, MemoryStream actualStream)
		{
			expectedStream.Position = 0;
			actualStream.Position = 0;

			if (expectedStream.Length != actualStream.Length)
			{
				return false;
			}

			while (true)
			{
				int expectedByte = expectedStream.ReadByte();
				int actualByte = actualStream.ReadByte();

				if (expectedByte != actualByte)
				{
					return false;
				}

				if (expectedByte == -1 && actualByte == -1)
				{
					return true;
				}
			}
		}

		public static string RequestMessageToString(HttpRequestMessage requestMessage)
		{
			var result = new StringBuilder();
			var headers = requestMessage.Headers.Concat(requestMessage.Content.Headers);
			foreach (var header in headers)
			{
				if (header.Value != null)
				{
					var builtHeader = string.Format("{0}: {1}", header.Key, string.Join(" ", header.Value));
					result.AppendLine(builtHeader);
				}
				
			}

			result.AppendLine(string.Format("Content-Length: {0}", requestMessage.Content.Headers.ContentLength));
			result.Append(requestMessage.Content.ReadAsStringAsync().Result);

			return result.ToString();
		}
	}
}