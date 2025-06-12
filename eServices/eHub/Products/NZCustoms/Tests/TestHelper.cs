using System;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Products.NZCustoms.Tests
{
	class TestHelper
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

		public static byte[] GetEmbeddedResourceAsByteArray(string resourceName)
		{
			byte[] buffer = new byte[16 * 1024];

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
	}
}
