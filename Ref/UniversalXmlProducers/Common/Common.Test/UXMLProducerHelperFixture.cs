using CargoWise.RefDbRepo.Common.Customization.Fody;
using System.IO;
using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.XmlProducer.Common.Test
{
	[TestFixture]
	public class UXMLProducerHelperFixture
	{
		[Test]
		public void GetFlagAppException()
		{
			var provider = new ReplaceMethodProvider();
			try
			{
				using (var stringWriter = new StringWriter())
				{
					Console.SetError(stringWriter);
					provider.TextWriterErrorWriteObjectMethodReplaceAddIn(new ArgumentException());
					Assert.IsTrue(stringWriter.ToString().StartsWith(FlagHelper.GetFlag(UXMLProducerHelper.AppException)));
				}
			}
			finally
			{
				Console.SetError(Console.Error);
			}
		}
	}
}
