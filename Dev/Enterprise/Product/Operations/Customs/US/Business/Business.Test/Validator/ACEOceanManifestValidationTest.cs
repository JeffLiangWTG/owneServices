using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEOceanManifestValidationTest : TestCaseWithFactory
	{
		public void TestWarnIllegalCharactersWereReplaced()
		{
			var bizObj = Factory.New<DummyOceanManifest>();
			bizObj.Z0_Description = "A/*AA";
			AssertHasWarningContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
			bizObj.Z0_Description = "A/ÎAA";
			AssertHasWarningContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
			bizObj.Z0_Description = "AA" + System.Environment.NewLine + "AA";
			AssertHasWarningContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
			bizObj.Z0_Description = "AAAA";
			AssertNoWarningContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
		}

		public void TestMessageErrorIfThereAreIllegalCharacters()
		{
			var bizObj = Factory.New<DummyOceanManifest>();
			bizObj.Z0_Description = "AA";
			AssertNoMessageErrorContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
			bizObj.Z0_Description = "ÎÎÎÎ";
			AssertHasMessageErrorContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
			bizObj.Z0_Description = "AA" + System.Environment.NewLine + "AA";
			AssertNoMessageErrorContaining(bizObj.Z0_DescriptionInfo, "Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'");
		}

		public void TestReplaceIllegalCharacters()
		{
			AssertEquals("HELLO WORLD HOW  ARE YOU TODAY?", ACEOceanManifestIllegalCharacters.ReplaceIllegalCharacters("HELLO\rWORLD\nHow\r\nARE*YOU TODAY?"));
		}

		sealed class DummyOceanManifest : DummyBusinessObject
		{
			public DummyOceanManifest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation() => new DummyOceanManifestValidation(this);
		}

		sealed class DummyOceanManifestValidation : DummyBizoValidation
		{
			public DummyOceanManifestValidation(DummyOceanManifest parent) : base(parent)
			{
			}

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();
				ACEOceanManifestIllegalCharacters.WarnIllegalCharactersWereReplaced(Parent.Z0_DescriptionInfo);
				ACEOceanManifestIllegalCharacters.MessageErrorIfThereAreIllegalCharacters(Parent.Z0_DescriptionInfo);
			}
		}
	}
}
