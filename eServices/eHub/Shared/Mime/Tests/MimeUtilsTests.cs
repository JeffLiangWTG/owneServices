using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Shared.Mime.Tests
{
	[TestClass]
	public class MimeUtilsTests
	{
		[TestMethod]
		public void ParseHeaders()
		{
			string headers = @"MIME-Version: 1.0
Content-Transfer-Encoding: 8bit
Content-ID: {8609DDBA-B424-43BE-B1DC-56F0A7652E39}
Content-Description: body

";
			var headersStream = new MemoryStream(Encoding.Default.GetBytes(headers));

			var expectedResult = new Dictionary<string, string>
			{
				{ "MIME-Version", "1.0" },
				{ "Content-Transfer-Encoding", "8bit" },
				{ "Content-ID", "{8609DDBA-B424-43BE-B1DC-56F0A7652E39}" },
				{ "Content-Description", "body" }
			};

			var stringResult = MimeUtils.ParseHeaders(headers);
			var streamResult = MimeUtils.ParseHeaders(headersStream);

			CollectionAssert.AreEqual(expectedResult, stringResult);
			CollectionAssert.AreEqual(expectedResult, streamResult);
		}

		[TestMethod]
		public void FormatHeaders()
		{
			var headers = new Dictionary<string, string>
			{
				{ "MIME-Version", "1.0" },
				{ "Content-Transfer-Encoding", "8bit" },
				{ "Content-ID", "{8609DDBA-B424-43BE-B1DC-56F0A7652E39}" },
				{ "Content-Description", "body" }
			};

			string expected = @"MIME-Version: 1.0
Content-Transfer-Encoding: 8bit
Content-ID: {8609DDBA-B424-43BE-B1DC-56F0A7652E39}
Content-Description: body

";

			var stringResult = MimeUtils.FormatHeaders(headers, Encoding.Default);
			var streamResult = MimeUtils.FormatHeaders(headers);

			Assert.AreEqual(expected, stringResult);
			Assert.AreEqual(expected, new StreamReader(streamResult).ReadToEnd());
		}

		[TestMethod]
		public void ExtractHeaders()
		{
			var message = TestFileHelpers.GetResourceStream("TestFiles.MessageAsBody.txt");

			var headers = MimeUtils.ExtractHeaders(message);

			Assert.AreEqual(@"MIME-Version: 1.0
Content-Transfer-Encoding: 8bit
Content-ID: {8609DDBA-B424-43BE-B1DC-56F0A7652E39}
Content-Description: body

", new StreamReader(headers).ReadToEnd());

			Assert.AreEqual(@"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate.", new StreamReader(message).ReadToEnd());
		}
	}
}
