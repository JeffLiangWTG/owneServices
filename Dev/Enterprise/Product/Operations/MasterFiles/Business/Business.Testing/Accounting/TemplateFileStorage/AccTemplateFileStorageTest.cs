using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTemplateFileStorage))]
	sealed class AccTemplateFileStorageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultsCorrectly()
		{
			var templateFile = Factory.New<AccTemplateFileStorage>();
			templateFile.TFS_ExternalReference = ZGuid.NewZGuid();

			AssertEquals(Env.CurrentCompanyPK, templateFile.TFS_GC);
			AssertEquals("AR", templateFile.TFS_Ledger);
		}

		public void TestDeterminesDuplicateCorrectly()
		{
			var fakeCompanyGuid = ZGuid.NewZGuid();

			var templateFile = (AccTemplateFileStorage)GetNewBusinessObject();
			var templateFileDup = (AccTemplateFileStorage)GetNewBusinessObject();

			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.TFS_GC = v, b => b.TFS_GC, templateFile, templateFileDup, ZGuid.NewZGuid());
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.TFS_Ledger = v, b => b.TFS_Ledger, templateFile, templateFileDup, new ZString("AP"));
			TestPropertyIsConsideredInCheckForDuplication((b, v) => b.TFS_Code = v, b => b.TFS_Code, templateFile, templateFileDup, new ZString("SI1"));
		}

		void TestPropertyIsConsideredInCheckForDuplication<PropType>(Action<AccTemplateFileStorage, PropType> setter, Func<AccTemplateFileStorage, PropType> getter, AccTemplateFileStorage origBizo, AccTemplateFileStorage dupBizo, PropType randomValue)
		{
			Assert(origBizo.IsDuplicateOf(dupBizo));
			AssertEquals(getter(origBizo), getter(dupBizo));
			setter(dupBizo, randomValue);
			Assert(!origBizo.IsDuplicateOf(dupBizo));
			setter(dupBizo, getter(origBizo));
		}

		public void TestDeletingFileNameDeletesFileData()
		{
			var exampleTemplateFile = (AccTemplateFileStorage)GetNewBusinessObject();

			exampleTemplateFile.TFS_Code = "XYZ";
			exampleTemplateFile.TFS_FileData = ZBlob.FromUTF8("0x505AECBBD7AE24E97626762F40EFB0591CA149E4A90E9FA60E7B466132BCC9F01931180CC265789361320");
			exampleTemplateFile.TFS_FileName = "Test File";

			exampleTemplateFile.TFS_FileName = string.Empty;
			Assert(exampleTemplateFile.TFS_FileData.IsEmpty);
		}
	}
}
