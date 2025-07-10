using CargoWise.RefDbRepo.FRReferenceData.Services;
using System.IO;
using System;
using NUnit.Framework;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class BaseUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestMailSentIfNoData()
		{
			UniversalDataHelper.SentEmails.Clear();

			var generator = new GeneratorForTest();
			var error = new Errors();
			Assert.DoesNotThrow(() => generator.GenerateFiles(new DateTime(2022, 03, 01), ref error));
			Assert.That(UniversalDataHelper.SentEmails.Count == 1);
			Assert.That(UniversalDataHelper.SentEmails.Contains(new Email
			{
				From = "donotreply_refservice@wisetechglobal.com",
				To = ApplicationConfig.Instance.EmailRecipients,
				Subject = "No FR - Some Type information was found.",
				Body = "No information could be retrieved from File name 1, File name 2. It's likely that this or one of those files is empty or its structure has changed."
			}));
		}

		class GeneratorForTest : BaseUniversalReferenceDataFileGenerator<RefCusCodeList>
		{
			public override string OutputFile => "Some File";

			public override string DataSource => "FR - Some Type";

			protected override List<RefCusCodeList> GetDataCollection(DateTime publicationDate, ref Errors error) => new List<RefCusCodeList>();

			protected override XmlWriterConfiguration GetXmlWriterConfiguration() => new XmlWriterConfiguration();

			public override IEnumerable<string> InputFiles
			{
				get
				{
					yield return "File name 1";
					yield return "File name 2";
				}
			}
		}
	}
}
