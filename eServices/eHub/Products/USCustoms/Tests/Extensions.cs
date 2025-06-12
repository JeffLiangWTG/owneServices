using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CargoWise.eServices.USCustoms.Tests
{
	public static class Extensions
	{
		public static Stream ToStream(this string input)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(input);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static string ContentToString(this Stream stream)
		{
			stream.Position = 0;
			using (var streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}
	}
}
