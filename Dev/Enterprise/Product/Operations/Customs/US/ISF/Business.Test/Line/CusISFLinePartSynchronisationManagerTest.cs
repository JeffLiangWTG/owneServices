using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFLinePartSynchronisationManagerTest : TestCaseWithFactory
	{
		public void TestUpdateDetailsOnPartChange()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			var product = Factory.New<US.Business.OrgSupplierPart>();
			product.OP_PartNum = "DWG Test Product";
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_FormattedSupplementalTariff = "9817.00.5000";
			pivot1.CD_UC_NKCountryOfOrigin = "US";
			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_FormattedTariffNum = "8481.80.5080";
			child1.CI_FormattedSupplementalTariff = "9889.80.5090";
			child1.CD_UC_NKCountryOfOrigin = "CA";
			var child2 = pivot1.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			child2.CI_FormattedTariffNum = "8481.80.5090";
			child2.CI_FormattedSupplementalTariff = "9903.88.03";
			child2.CD_UC_NKCountryOfOrigin = "DE";

			var invLine = CreateLine(Factory, "DWG Test Product", importer.PK);
			var lines = invLine.Header.Lines;

			AssertEquals(2, lines.Count);
			AssertEquals("lines[0].BL_TextProductCode", "DWG TEST PRODUCT", lines[0].BL_TextProductCode);
			AssertEquals("lines[0].BL_RN_NKGoodsOrigin", "CA", lines[0].BL_RN_NKGoodsOrigin);
			AssertEquals("lines[0].BL_FormattedHarmonisedNum", "8481.80.5080", lines[0].BL_FormattedHarmonisedNum);
			AssertEquals("lines[0].BL_LineType", ZString.Empty, lines[0].BL_LineType);

			AssertEquals("lines[1].BL_TextProductCode", "DWG TEST PRODUCT", lines[1].BL_TextProductCode);
			AssertEquals("lines[1].BL_RN_NKGoodsOrigin", "DE", lines[1].BL_RN_NKGoodsOrigin);
			AssertEquals("lines[1].BL_FormattedHarmonisedNum", "8481.80.5090", lines[1].BL_FormattedHarmonisedNum);
			AssertEquals("lines[1].BL_LineType", ISFLineTypeList.Codes.Related, lines[1].BL_LineType);

			pivot1.CI_FormattedTariffNum = "8481.80.1111";
			invLine = CreateLine(Factory, "DWG Test Product", importer.PK);
			AssertEquals(3, invLine.Header.Lines.Count);
		}

		public void TestCalculatePart()
		{
			CusISFLine invLine = CreateLine(Factory, "PARTNUM", ImporterPK);
			CusISFLinePartSynchronisationManager syncManager = invLine.PartSyncManager;
			AssertNull(syncManager.Part);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			SaveNewPart(factory2, "PARTNUM", ImporterPK);
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestDataRefreshIsWorking()
		{
			Factory.RefreshEnabled = true;
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			CusISFLinePartSynchronisationManager syncManager = line.PartSyncManager;
			AssertNull(syncManager.Part);
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			SaveNewPart(secondFactory, "PARTNUM", ImporterPK);
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfTheImporterChanges()
		{
			SaveNewPart(Factory, "PARTNUM", ImporterPK);
			CusISFLine line = CreateLine(Factory, "PARTNUM", ZGuid.Empty);
			CusISFLinePartSynchronisationManager syncManager = line.PartSyncManager;
			AssertNull(syncManager.Part);
			line.Header.BF_OH_Importer = ImporterPK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfThePartNumberChanges()
		{
			SaveNewPart(Factory, "PARTNUM", ImporterPK);
			CusISFLine line = CreateLine(Factory, ZString.Empty, ImporterPK);
			CusISFLinePartSynchronisationManager syncManager = line.PartSyncManager;
			AssertNull(syncManager.Part);
			line.BL_TextProductCode = "PARTNUM";
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartReloadsIfTheHeaderChanges()
		{
			SaveNewPart(Factory, "PARTNUM", ImporterPK);
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			CusISFLinePartSynchronisationManager syncManager = line.PartSyncManager;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
			CusISFHeader header = line.Header;
			line.BL_BF = ZGuid.Empty;
			AssertNull(syncManager.Part);
			line.BL_BF = header.PK;
			AssertEquals("PARTNUM", syncManager.Part.OP_PartNum);
		}

		public void TestPartSynchronisesIfChangedInAnotherFactory()
		{
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			CusISFLinePartSynchronisationManager syncManager = line.PartSyncManager;
			AssertNull(syncManager.Part);
			ZGuid partPK = SaveNewPart(new BusinessObjectFactory(), "PARTNUM", ImporterPK);
			var pivot = syncManager.Part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Part Origin", Core.Constants.CountryCodes.Australia, pivot.CD_UC_NKCountryOfOrigin);
			ChangePartOrigin(partPK, Core.Constants.CountryCodes.NewZealand);
			pivot = syncManager.Part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Part Origin", Core.Constants.CountryCodes.NewZealand, pivot.CD_UC_NKCountryOfOrigin);
		}

		public void TestSynchroniseWorksForLoadedLine()
		{
			ZGuid partPK = SaveNewPart(Factory, "PARTNUM", ImporterPK);
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			CusISFHeader secondHeader = secondFactory.Load<CusISFHeader>(line.Header.PK);
			CusISFLine secondFactoryLine = secondHeader.Lines[0];
			AssertEquals("Line Origin", Core.Constants.CountryCodes.Australia, line.BL_RN_NKGoodsOrigin);
			ChangePartOrigin(partPK, Core.Constants.CountryCodes.NewZealand);
			AssertEquals("Line Origin", Core.Constants.CountryCodes.NewZealand, secondFactoryLine.BL_RN_NKGoodsOrigin);
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfHeaderDeleted()
		{
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			line.Header.Delete();
			line.PartSyncManager.Refresh();
		}

		[ExpectNoExceptions]
		public void TestRefreshDoesntErrorIfLineDeleted()
		{
			CusISFLine line = CreateLine(Factory, "PARTNUM", ImporterPK);
			line.Delete();
			line.PartSyncManager.Refresh();
		}

		[TestDate(2016, 4, 6)]
		public void TestRefreshWithChildLines()
		{
			var importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			US.Business.OrgSupplierPart part = Factory.New<US.Business.OrgSupplierPart>();
			part.OP_PartNum = "PART123ZZ";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = part.RelatedOrganisations[0].OU_OH;
			pivot1.CI_TariffNum = "1010231030";
			pivot1.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pivot1.CI_DateStart = new ZDateTime(2016, 4, 1);
			pivot1.CI_DateEnd = new ZDateTime(2016, 4, 30);
			pivot1.Attributes1.AddNew().BG_AttributeValue1 = "1";
			pivot1.Attributes2.AddNew().BG_AttributeValue1 = "1";
			pivot1.Attributes3.AddNew().BG_AttributeValue1 = "1";
			var pivot1Child1 = pivot1.Children.AddNew();
			pivot1Child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot1Child1.CI_ChildListOrder = 1;
			pivot1Child1.CI_TariffNum = "1010231031";
			pivot1Child1.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Cambodia;
			var pivot1Child2 = pivot1.Children.AddNew();
			pivot1Child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot1Child2.CI_TariffNum = "1010231032";
			pivot1Child2.CI_ChildListOrder = 1;
			pivot1Child2.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Albania;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = part.RelatedOrganisations[0].OU_OH;
			pivot2.CI_TariffNum = "1010231040";
			pivot2.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			part.OP_PartNum = "PART123ZZ";
			pivot2.CI_DateStart = new ZDateTime(2016, 4, 1);
			pivot2.CI_DateEnd = new ZDateTime(2016, 4, 30);
			pivot2.Attributes1.AddNew().BG_AttributeValue1 = "2";
			pivot2.Attributes2.AddNew().BG_AttributeValue1 = "2";
			pivot2.Attributes3.AddNew().BG_AttributeValue1 = "2";
			var pivot2Child1 = pivot2.Children.AddNew();
			pivot2Child1.CI_ChildListOrder = 1;
			pivot2Child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2Child1.CI_TariffNum = "1010231041";
			pivot2Child1.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			Factory.Save();
			var header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = importer.PK;
			var line = header.Lines.AddNew();
			line.BL_TextProductCode = "PART123ZZ";
			line.BL_PartAttrib1 = "1";
			line.BL_PartAttrib2 = "1";
			line.BL_PartAttrib3 = "1";
			AssertEquals("BL_HarmonisedNum", "1010231030", line.BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "AU", line.BL_RN_NKGoodsOrigin);
			AssertEquals(1, line.ChildLines.Count());
			AssertEquals("BL_HarmonisedNum", "1010231031", line.ChildLines.First().BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "KH", line.ChildLines.First().BL_RN_NKGoodsOrigin);
			AssertEquals(1, line.ProductRelatedLines.Count());
			AssertEquals("BL_HarmonisedNum", "1010231032", line.ProductRelatedLines.First().BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "AL", line.ProductRelatedLines.First().BL_RN_NKGoodsOrigin);
			line.BL_PartAttrib1 = "2";
			line.BL_PartAttrib2 = "2";
			line.BL_PartAttrib3 = "2";
			AssertEquals("BL_HarmonisedNum", "1010231040", line.BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "US", line.BL_RN_NKGoodsOrigin);
			AssertEquals(1, line.ChildLines.Count());
			AssertEquals("BL_HarmonisedNum", "1010231041", line.ChildLines.First().BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "CA", line.ChildLines.First().BL_RN_NKGoodsOrigin);
			AssertEquals(0, line.ProductRelatedLines.Count());
			var pivot2Child2 = pivot2.Children.AddNew();
			pivot2Child2.CI_ChildListOrder = 2;
			pivot2Child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2Child2.CI_TariffNum = "1010231042";
			pivot2Child2.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Colombia;
			((IDataRefreshBusSubscriber)line.PartSyncManager).UpdatedByDataRefresh(new List<IPartProvider> { part });
			AssertEquals(1, line.ChildLines.Count());
			AssertEquals("BL_HarmonisedNum", "1010231041", line.ChildLines.First().BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "CA", line.ChildLines.First().BL_RN_NKGoodsOrigin);
			AssertEquals(1, line.ProductRelatedLines.Count());
			AssertEquals("BL_HarmonisedNum", "1010231042", line.ProductRelatedLines.First().BL_HarmonisedNum);
			AssertEquals("BL_RN_NKGoodsOrigin", "CO", line.ProductRelatedLines.First().BL_RN_NKGoodsOrigin);
		}

		ZGuid ImporterPK => Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS").PK;

		CusISFLine CreateLine(BusinessObjectFactory factory, ZString partNum, ZGuid importerPK)
		{
			var header = factory.New<CusISFHeader>();
			header.BF_OH_Importer = importerPK;
			var result = header.Lines.AddNew();
			result.BL_TextProductCode = partNum;
			return result;
		}

		ZGuid SaveNewPart(BusinessObjectFactory factory, ZString partNum, ZGuid importerPK)
		{
			var newPart = factory.New<US.Business.OrgSupplierPart>();
			if (!importerPK.IsEmpty)
			{
				newPart.RelatedOrganisations.AddOrganisationIfNotExist(importerPK, OrgPartRelation.RelationshipTypes.Owner);
				var classification = factory.New<CusClassification>();
				classification.CC_LookupCode = "TestLookup";
				classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				classification.CC_ClassificationType = Customs.US.Business.CusClassification.ClassificationType.IMP;
				classification.CC_TariffNum = "1010231032";
				var importPivot = newPart.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				importPivot.CI_CC = classification.PK;
				importPivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			}

			newPart.OP_PartNum = partNum;
			factory.Save();
			return newPart.PK;
		}

		void ChangePartOrigin(ZGuid partPK, ZString origin)
		{
			var factory = new BusinessObjectFactory();
			var part = factory.Load<US.Business.OrgSupplierPart>(partPK);
			var importPivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
			importPivot.CD_UC_NKCountryOfOrigin = origin;
			importPivot.CI_TariffNum = "2343";
			factory.Save();
		}
	}
}
