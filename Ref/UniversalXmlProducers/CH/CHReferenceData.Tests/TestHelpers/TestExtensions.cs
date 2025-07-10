using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Tests.CodeLists;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
	static class TestExtensions
	{
		internal static string ReadManifestResourceContent(this string resourcePath)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		internal static byte[] GetTestArray(this Type type, string name)
		{
			using (var input = GetTestStream(type, name))
			using (var output = new MemoryStream())
			{
				input.CopyTo(output);
				return output.ToArray();
			}
		}

		internal static Stream GetZippedTestStream(this Type type, string name)
		{
			return new ZippedMemoryStream(name, GetTestStream(type, name));
		}

		internal static Stream GetUnzippedTestStream(this Type type, string name)
		{
			return new UnzippedMemoryStream(GetTestStream(type, name));
		}

		internal static Stream GetTestStream(this Type type, string name)
		{
			var path = type.Namespace + "." + name;
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(path) ?? throw new InvalidOperationException($"Missing resource: {path}");
		}

		internal static MockedRequest WithUserAgent(this MockedRequest request)
		{
			return request.WithHeaders("User-Agent", Constants.HttpUserAgent);
		}
	}
}
