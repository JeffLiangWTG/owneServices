using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Common
{
	sealed class ConvertApiImplTest : TestCase
	{
		public void TestConvertAsync()
		{
			// arrange
			var input = new MemoryStream(Encoding.ASCII.GetBytes("hello world"));
			var convertApi = new TestConvertApiImpl();

			// act
			_ = convertApi.ConvertAsync("DOCX", "PDF", input).Result;

			// assert
			AssertContainsExactElementsInAnyOrder(new List<string>() { "hello world" }, convertApi.Converted);
		}
	}
}
