using System;
using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Customization.Fody.Test
{
	[TestFixture]
	public class ReplaceMethodProviderTest
	{
		[Test]
 		public void TextWriterErrorWriteObjectMethodReplaceAddIn()
		{
			var provider = new ReplaceMethodProvider();
			try
			{
				using (var stringWriter = new StringWriter())
				{
					Console.SetError(stringWriter);
					provider.TextWriterErrorWriteObjectMethodReplaceAddIn(null);
					Assert.AreEqual("$$AppException$$:null", stringWriter.ToString());

					provider.TextWriterErrorWriteObjectMethodReplaceAddIn(new ArgumentException("no argument error test."));
					Assert.AreEqual("$$AppException$$:null$$AppException$$:{\"ClassName\":\"System.ArgumentException\",\"Message\":\"no argument error test.\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2147024809,\"Source\":null,\"WatsonBuckets\":null,\"ParamName\":null}", stringWriter.ToString());
				}
			}
			finally
			{
				Console.SetError(Console.Error);
			}
		}

		[Test]
		public void TextWriterErrorWriteLineObjectMethodReplaceAddIn()
		{
			var provider = new ReplaceMethodProvider();
			try
			{
				using (var stringWriter = new StringWriter())
				{
					Console.SetError(stringWriter);
					provider.TextWriterErrorWriteLineObjectMethodReplaceAddIn(null);
					Assert.AreEqual("$$AppException$$:null\r\n", stringWriter.ToString());

					provider.TextWriterErrorWriteLineObjectMethodReplaceAddIn(new ArgumentException("no argument error test."));
					Assert.AreEqual("$$AppException$$:null\r\n$$AppException$$:{\"ClassName\":\"System.ArgumentException\",\"Message\":\"no argument error test.\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2147024809,\"Source\":null,\"WatsonBuckets\":null,\"ParamName\":null}\r\n", stringWriter.ToString());
				}
			}
			finally
			{
				Console.SetError(Console.Error);
			}
		}
	}
}
