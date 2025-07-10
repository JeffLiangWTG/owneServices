using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DocumentWatermarkHelperTests : TestCaseWithFactory
	{
		public void TestCustomWatermarkForSADDocumentPack()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("2");
			Factory.Save();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "BR2";
			Factory.Save();
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration1.JE_GB = branch1.PK;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = "1";
			var entryHeader2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration2.JE_GB = branch2.PK;
			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_EntryStatus = "1";
			var entryHeader4 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_EntryStatus = "";
			Factory.Save();
			var context = new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack);
			var docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_MenuName = "Customs Declaration Response";
			var docDataProvider1 = declaration1.DocumentSupporter.GetBODocDataProviders(context, docCommand)[0];
			var docDataProvider2 = declaration1.DocumentSupporter.GetBODocDataProviders(context, docCommand)[1];
			var docDataProvider3 = declaration2.DocumentSupporter.GetBODocDataProviders(context, docCommand)[0];
			var docDataProvider4 = declaration2.DocumentSupporter.GetBODocDataProviders(context, docCommand)[1];
			AssertNull(DocumentWatermarkHelper.GetWatermarkText("Customs Declaration Response", docDataProvider1));
			AssertNull(DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider1));
			AssertNull(DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider2));
			AssertNull(DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider3));
			AssertNull(DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider4));
			var collection = new SADDocumentWatermarkCollection();
			var watermark = collection.AddNew();
			watermark.EntryStatusCode = "1";
			ZACustomsRegistry.Instance.SADDocumentPackWatermarks.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, collection);
			Factory.Save();
			AssertEquals("Release", DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider1));
			AssertNull(DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider3));
			watermark.WatermarkText = "Watermark1";
			ZACustomsRegistry.Instance.SADDocumentPackWatermarks.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, collection);
			Factory.Save();
			AssertEquals("Watermark1", DocumentWatermarkHelper.GetWatermarkText(DocumentWatermarkHelper.DocumentCommands.SADDocument, docDataProvider1));
		}
	}
}
