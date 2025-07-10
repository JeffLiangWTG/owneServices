using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	sealed class OrgSupplierPartTest : Customs.Business.Testing.OrgSupplierPartTest
	{
		public void TestCustomsCountryCodeIsCorrect()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var part = Factory.New<OrgSupplierPart>();
				var pivot = part.PivotsForBinding.AddNew();
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, pivot.CI_RN_NKCountry);
			}
		}

		public void TestGetUSPivots()
		{
			Customs.Business.OrgSupplierPart part;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var factory = new BusinessObjectFactory();
				part = factory.New<Customs.Business.OrgSupplierPart>();
				part.OP_PartNum = "partnum";
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = "10";
				AssertEquals(Core.Constants.CountryCodes.Australia, pivot.CI_RN_NKCountry);
				pivot = part.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = "20";
				factory.Save();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var factory = new BusinessObjectFactory();
				part = factory.Load<Customs.Business.OrgSupplierPart>(part.PK);
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = "30";
				AssertEquals(Core.Constants.CountryCodes.NewZealand, pivot.CI_RN_NKCountry);
				pivot = part.PivotsForBinding.AddNew();
				pivot.CI_TariffNum = "40";
				factory.Save();
			}
			var usPart = Factory.Load<OrgSupplierPart>(part.PK);
			AssertEquals("usPart.PivotsForBinding.Count", 0, usPart.PivotsForBinding.Count);
			AssertEquals("usPart.GetUSPivots().Length", 0, usPart.GetUSPivots().Length);
			var usPivot1 = usPart.PivotsForBinding.AddNew();
			usPivot1.CI_TariffNum = "50";
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, usPivot1.CI_RN_NKCountry);
			var usPivot2 = usPart.PivotsForBinding.AddNew();
			usPivot2.CI_TariffNum = "60";
			var pivots = usPart.GetUSPivots();
			AssertEquals("pivots.Length", 2, pivots.Length);
			AssertCollectionContains(usPivot1, pivots);
			AssertCollectionContains(usPivot2, pivots);
		}

		public void TestBusinessObjectsWithRelatedEventsForPivot()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TST01";
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals("Related Logs has CusClassPartPivot", true, ((IList)part.BusinessObjectsWithRelatedEvents).Contains(pivot));
		}

		public void TestIHaveRequiredDocuments()
		{
			var part = Factory.New<OrgSupplierPart>();
			IDocManagerSupport iDocManagerSupport = part;

			AssertNotNull("DocManagerInfo", iDocManagerSupport.DocManagerInfo);
			AssertEquals(Core.Constants.DocManagerCodes.Product, iDocManagerSupport.DocManagerInfo.DocManagerCode);

			IHaveRequiredDocuments iHaveRequiredDocuments = part;
			part.OP_PartNum = "123456";
			AssertEquals("123456", iHaveRequiredDocuments.UniqueConsignRef);

			AssertEquals(ZString.Empty, iHaveRequiredDocuments.HouseBill);
			AssertEquals(ZString.Empty, iHaveRequiredDocuments.MasterBill);
			AssertNull(iHaveRequiredDocuments.ExportBroker);
			AssertEquals(iHaveRequiredDocuments.TableCode, OrgSupplierPartSchema.Constants.Prefix);
			AssertEquals(1, iHaveRequiredDocuments.AdditionalRefTypes.Count);
			AssertEquals(Core.Constants.ReferenceTypes.SupplyChainLogistics, iHaveRequiredDocuments.AdditionalRefTypes[0]);
			AssertEquals("RequiredDocuments", typeof(JobRequiredDocumentDependentCollection), iHaveRequiredDocuments.RequiredDocuments.GetType());
			AssertEquals(part, iHaveRequiredDocuments.UltimateDocumentParent);

			var requiredDocument = iHaveRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Nafta);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, requiredDocument.EQ_DocPeriod);

			var requiredDocument2 = iHaveRequiredDocuments.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.Cotton);
			AssertEquals(Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic, requiredDocument2.EQ_DocPeriod);
		}

		public void TestGetNewPivotsReturnsParentPivotsOnly()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_CI_Parent = pivot1.PK;
			part.PivotsForBinding.AddNew();

			AssertEquals(2, part.PivotsForBinding.Count);

			pivot2.CI_CI_Parent = pivot2.PK;
			AssertEquals("CusClassPartPivot.CI_CI_Parent is pointing to itself.", CargoWise.Common.ErrorReporter.LastMessageReported);
			CargoWise.Common.ErrorReporter.Clear();
		}

		public void TestTariffNumbersIncludingComponents()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "9101000010";
			AssertEquals("HTI:9101000010", product.Tariffs);

			pivot.CI_SupplementalTariff = "9802000050";
			AssertEquals("HTI:9101000010(9802000050)", product.Tariffs);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_TariffNum = "9201000010";

			AssertEquals("HTI:9101000010(9802000050), HTI:9201000010", product.Tariffs);
		}

		public void TestPivotsType()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			AssertEquals(typeof(CusClassPartPivotCollection), part.PivotsForBinding.GetType());
		}

		public void TestDeleteProduct()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PartNum";
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			Factory.Save();

			part.Delete();
			Assert("Part is successfully deleted", part.IsDeleted);
			Assert("Pivot is deleted", pivot.IsDeleted);
		}

		public void TestEndToEndTemplateCopy()
		{
			OrgHeader impOrg1 = Factory.New<OrgHeader>();
			impOrg1.OH_Code = "!IMP1";
			impOrg1.OH_IsConsignee = true;
			OrgHeader impOrg2 = Factory.New<OrgHeader>();
			impOrg2.OH_Code = "!IMP2";
			impOrg2.OH_IsConsignee = true;
			impOrg2.OH_IsConsignor = true;
			OrgHeader supOrg1 = Factory.New<OrgHeader>();
			supOrg1.OH_Code = "!SUP1";
			supOrg1.OH_IsConsignor = true;
			OrgHeader supOrg2 = Factory.New<OrgHeader>();
			supOrg2.OH_Code = "!SUP2";
			supOrg2.OH_IsConsignee = true;
			supOrg2.OH_IsConsignor = true;

			CusClassification impClass = Factory.New<CusClassification>();
			impClass.CC_LookupCode = "IMPLookup";
			impClass.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			impClass.CC_TariffNum = "3030303030";
			CusClassification expClass = Factory.New<CusClassification>();
			expClass.CC_LookupCode = "EXPLookup";
			expClass.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			expClass.CC_TariffNum = "4040404040";

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "!PZS!";
			part.OP_Desc = "PART DESC";
			OrgPartRelation impOrgRel1 = part.RelatedOrganisations.AddOrganisationIfNotExist(impOrg1.PK, OrgPartRelation.RelationshipTypes.Owner);
			impOrgRel1.OU_UsePartAttrib1 = true;
			OrgPartRelation impOrgRel2 = part.RelatedOrganisations.AddOrganisationIfNotExist(impOrg2.PK, OrgPartRelation.RelationshipTypes.Both);
			impOrgRel2.OU_UsePartAttrib1 = true;
			OrgPartRelation supOrgRel1 = part.RelatedOrganisations.AddOrganisationIfNotExist(supOrg1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			supOrgRel1.OU_UsePartAttrib1 = true;
			OrgPartRelation supOrgRel2 = part.RelatedOrganisations.AddOrganisationIfNotExist(supOrg2.PK, OrgPartRelation.RelationshipTypes.Both);
			supOrgRel2.OU_UsePartAttrib1 = true;
			CusClassPartPivot pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_UsageComment = "U1";
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_OH = impOrgRel1.OU_OH;
			pivot1.CI_CC = impClass.PK;
			CusAttributeFilter attrib1 = pivot1.Attributes1.AddNew();
			attrib1.BG_AttributeValue1 = "1";
			CusAttributeFilter attrib2 = pivot1.Attributes1.AddNew();
			attrib2.BG_AttributeValue1 = "2";

			CusClassPartPivot pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_OH = impOrgRel2.OU_OH;
			pivot2.CI_TariffNum = "1010101010";
			pivot2.CI_SupplementalTariff = "2020202020";
			CusAttributeFilter attrib3 = pivot2.Attributes1.AddNew();
			attrib3.BG_AttributeValue1 = "3";

			CusClassPartPivot pivot2Child1 = pivot2.Children.AddNew();
			pivot2Child1.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			pivot2Child1.CI_TariffNum = "1010101011";
			PGA pivot2Child1PGA = pivot2Child1.PGAs.AddNew();
			pivot2Child1PGA.US_PGALineValue = 1.1m;

			CusClassPartPivot pivot2Child2 = pivot2.Children.AddNew();
			pivot2Child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			pivot2Child2.CI_TariffNum = "1010101012";

			CusClassPartPivot pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot3.CI_OH = ZGuid.Empty;
			pivot3.CI_TariffNum = "1020102010";

			PGA pga1 = pivot1.PGAs.AddNew();
			pga1.US_PGALineValue = 1m;

			CusClassPartPivot pivot4 = part.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot4.CI_OH = supOrgRel1.OU_OH;
			pivot4.CD_ECCN = "E1";
			pivot4.CI_CC = expClass.PK;

			CusClassPartPivot pivot5 = part.PivotsForBinding.AddNew();
			pivot5.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot5.CI_OH = supOrgRel2.OU_OH;
			pivot5.CD_ECCN = "E2";
			pivot5.CI_TariffNum = "5050505050";

			CusClassPartPivot pivot6 = part.PivotsForBinding.AddNew();
			pivot6.CI_ChildType = ClassificationTypeList.Codes.SHB;
			pivot6.CD_ECCN = "E3";
			pivot6.CI_TariffNum = "6060606060";
			pivot6.CI_SupplementalTariff = "7070707070";
			Factory.Save();

			OrgSupplierPart partCloned = (OrgSupplierPart)((ITemplateCopyable)part).TemplateCopy();
			AssertEquals("PART DESC", partCloned.OP_Desc);
			AssertEquals(4, partCloned.RelatedOrganisations.Count);
			OrgPartRelation impOrgRel1Cloned = partCloned.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(impOrg1.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation impOrgRel2Cloned = partCloned.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(impOrg2.PK, OrgPartRelation.RelationshipTypes.Both);
			OrgPartRelation supOrgRel1Cloned = partCloned.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supOrg1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			OrgPartRelation supOrgRel2Cloned = partCloned.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supOrg2.PK, OrgPartRelation.RelationshipTypes.Both);
			AssertEquals(6, partCloned.PivotsForBinding.Count);
			CusClassPartPivot pivot1Cloned = partCloned.PivotsForBinding.GetImportMatch(impOrg1.PK, ZGuid.Empty, new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT1, Value = "1" });
			CusClassPartPivot pivot2Cloned = partCloned.PivotsForBinding.GetImportMatch(impOrg2.PK, ZGuid.Empty, new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT1, Value = "3" });
			CusClassPartPivot pivot3Cloned = partCloned.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty);
			CusClassPartPivot pivot4Cloned = partCloned.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supOrg1.PK);
			CusClassPartPivot pivot5Cloned = partCloned.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supOrg2.PK);
			CusClassPartPivot pivot6Cloned = partCloned.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty);

			AssertEquals("U1", pivot1Cloned.CI_UsageComment);
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot1Cloned.CI_ChildType);
			AssertEquals(impOrgRel1Cloned.OU_OH, pivot1Cloned.CI_OH);
			AssertEquals(impClass.PK, pivot1Cloned.CI_CC);
			AssertEquals("", pivot1Cloned.CI_TariffNum);
			AssertEquals("", pivot1Cloned.CI_SupplementalTariff);
			AssertEquals(2, pivot1Cloned.Attributes1.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1", "2" }, pivot1Cloned.Attributes1.GetFieldValues(CusAttributeFilter.Schema.BG_AttributeValue1));
			AssertEquals(1, pivot1Cloned.PGAs.Count);
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { 1m }, pivot1Cloned.PGAs.GetFieldValues(PGA.Schema.US_PGALineValue));

			AssertEquals(0, pivot1Cloned.Children.Count);

			AssertEquals(ClassificationTypeList.Codes.HTI, pivot2Cloned.CI_ChildType);
			AssertEquals(impOrgRel2Cloned.OU_OH, pivot2Cloned.CI_OH);
			AssertEquals(ZGuid.Empty, pivot2Cloned.CI_CC);
			AssertEquals("1010101010", pivot2Cloned.CI_TariffNum);
			AssertEquals("2020202020", pivot2Cloned.CI_SupplementalTariff);
			AssertEquals(1, pivot2Cloned.Attributes1.Count);
			AssertEquals("3", pivot2Cloned.Attributes1[0].BG_AttributeValue1);
			AssertEquals(0, pivot2Cloned.PGAs.Count);

			AssertEquals(2, pivot2Cloned.Children.Count);
			CusClassPartPivot pivot2Child1Cloned = pivot2Cloned.Children[0];
			CusClassPartPivot pivot2Child2Cloned = pivot2Cloned.Children[1];
			if (pivot2Child1Cloned.CI_TariffNum == "1010101012")
			{
				pivot2Child1Cloned = pivot2Cloned.Children[1];
				pivot2Child2Cloned = pivot2Cloned.Children[0];
			}
			AssertEquals(ClassificationChildTypeList.Codes.Related, pivot2Child1Cloned.CI_ChildType);
			AssertEquals("1010101011", pivot2Child1Cloned.CI_TariffNum);
			AssertEquals(0, pivot2Child1Cloned.Attributes1.Count);
			AssertEquals(1, pivot2Child1Cloned.PGAs.Count);
			AssertEquals(1m, pivot2Child1Cloned.PGAs[0].US_PGALineValue);

			AssertEquals(ClassificationChildTypeList.Codes.COMPONENT, pivot2Child2Cloned.CI_ChildType);
			AssertEquals("1010101012", pivot2Child2Cloned.CI_TariffNum);
			AssertEquals(0, pivot2Child2Cloned.Attributes1.Count);
			AssertEquals(0, pivot2Child2Cloned.PGAs.Count);

			AssertEquals(ClassificationTypeList.Codes.HTI, pivot3Cloned.CI_ChildType);
			AssertEquals(ZGuid.Empty, pivot3Cloned.CI_OH);
			AssertEquals(ZGuid.Empty, pivot3Cloned.CI_CC);
			AssertEquals("1020102010", pivot3Cloned.CI_TariffNum);
			AssertEquals("", pivot3Cloned.CI_SupplementalTariff);
			AssertEquals(0, pivot3Cloned.Attributes1.Count);
			AssertEquals(0, pivot3Cloned.PGAs.Count);

			AssertEquals(0, pivot3Cloned.Children.Count);

			AssertEquals(ClassificationTypeList.Codes.HTE, pivot4Cloned.CI_ChildType);
			AssertEquals(supOrgRel1Cloned.OU_OH, pivot4Cloned.CI_OH);
			AssertEquals("E1", pivot4Cloned.CD_ECCN);
			AssertEquals(expClass.PK, pivot4Cloned.CI_CC);
			AssertEquals("", pivot4Cloned.CI_TariffNum);
			AssertEquals("", pivot4Cloned.CI_SupplementalTariff);
			AssertEquals(0, pivot4Cloned.Attributes1.Count);
			AssertEquals(0, pivot4Cloned.PGAs.Count);

			AssertEquals(0, pivot4Cloned.Children.Count);

			AssertEquals(ClassificationTypeList.Codes.HTE, pivot5Cloned.CI_ChildType);
			AssertEquals(supOrgRel2Cloned.OU_OH, pivot5Cloned.CI_OH);
			AssertEquals("E2", pivot5Cloned.CD_ECCN);
			AssertEquals(ZGuid.Empty, pivot5Cloned.CI_CC);
			AssertEquals("5050505050", pivot5Cloned.CI_TariffNum);
			AssertEquals("", pivot5Cloned.CI_SupplementalTariff);
			AssertEquals(0, pivot5Cloned.Attributes1.Count);
			AssertEquals(0, pivot5Cloned.PGAs.Count);

			AssertEquals(0, pivot5Cloned.Children.Count);

			AssertEquals(ClassificationTypeList.Codes.SHB, pivot6Cloned.CI_ChildType);
			AssertEquals("E3", pivot6Cloned.CD_ECCN);
			AssertEquals(ZGuid.Empty, pivot6Cloned.CI_CC);
			AssertEquals("6060606060", pivot6Cloned.CI_TariffNum);
			AssertEquals("7070707070", pivot6Cloned.CI_SupplementalTariff);
			AssertEquals(0, pivot6Cloned.Attributes1.Count);
			AssertEquals(0, pivot6Cloned.PGAs.Count);

			AssertEquals(0, pivot6Cloned.Children.Count);
		}

		public void TestACEFDAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_ACEFDAIndicator = "D";
			AssertEquals("ACEFDAIndicator", "D", product.ImportACEFDAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_ACEFDAIndicator = "D";
			AssertEquals("ACEFDAIndicatorr", "D", product.ImportACEFDAIndicator);

			pivot2.CD_ACEFDAIndicator = "C";
			AssertEquals("ACEFDAIndicatorr", "MULTI", product.ImportACEFDAIndicator);
		}

		public void TestATFIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_ATFIndicator = "D";
			AssertEquals("DOT Indicator", "D", product.ImportATFIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_ATFIndicator = "C";
			AssertEquals("DOT Indicator", "MULTI", product.ImportATFIndicator);
		}

		public void TestAMSNOPIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_NOPIndicator = "D";
			AssertEquals("AMS NOP Indicator", "D", product.ImportNOPIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_NOPIndicator = "C";
			AssertEquals("AMS NOP Indicator", "MULTI", product.ImportNOPIndicator);
		}

		public void TestAMSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_AMSIndicator = "D";
			AssertEquals("AMS Indicator", "D", product.ImportAMSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_AMSIndicator = "C";
			AssertEquals("AMS Indicator", "MULTI", product.ImportAMSIndicator);
		}

		public void TestTTBIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_TTBIndicator = "D";
			AssertEquals("TTB Indicator should be D", "D", product.ImportTTBIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_TTBIndicator = "C";
			AssertEquals("TTB Indicator should be C", "MULTI", product.ImportTTBIndicator);
		}

		public void TestFWSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_FWSIndicator = "D";
			AssertEquals("FWS Indicator should be D", "D", product.ImportFWSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_FWSIndicator = "C";
			AssertEquals("FWS Indicator should be C", "MULTI", product.ImportFWSIndicator);
		}

		public void TestNMFS370Indicators()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_NMFS370Indicator = "D";
			AssertEquals("370 Indicator should be D", "D", product.ImportNMFS370Indicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_NMFS370Indicator = "C";
			AssertEquals("370 Indicator should be C", "MULTI", product.ImportNMFS370Indicator);
		}

		public void TestNMFSAMRIndicators()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_NMFSAMRIndicator = "D";
			AssertEquals("AMR Indicator should be D", "D", product.ImportNMFSAMRIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_NMFSAMRIndicator = "C";
			AssertEquals("AMR Indicator should be C", "MULTI", product.ImportNMFSAMRIndicator);
		}

		public void TestNMFSHMSIndicators()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_NMFSHMSIndicator = "D";
			AssertEquals("HMS Indicator should be D", "D", product.ImportNMFSHMSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_NMFSHMSIndicator = "C";
			AssertEquals("HMS Indicator should be C", "MULTI", product.ImportNMFSHMSIndicator);
		}

		public void TestNMFSSIMPIndicators()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_NMFSSIMPIndicator = "D";
			AssertEquals("SIMP Indicator should be D", "D", product.ImportNMFSSIMPIndicator);
		}

		public void TestDDTCIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.Details.CD_DDTCIndicator = "D";
			AssertEquals("LaceyAct Indicator", "D", product.ImportDDTCIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.Details.CD_DDTCIndicator = "D";
			AssertEquals("DDTC Indicator", "D", product.ImportDDTCIndicator);

			pivot2.Details.CD_DDTCIndicator = "C";
			AssertEquals("DDTC Multi Indicator", "MULTI", product.ImportDDTCIndicator);
		}

		public void TestLaceyActIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_LaceyActIndicator = "D";
			AssertEquals("LaceyAct Indicator", "D", product.ImportLaceyActPGAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_LaceyActIndicator = "D";
			AssertEquals("LaceyAct Indicator", "D", product.ImportLaceyActPGAIndicator);

			pivot2.CD_LaceyActIndicator = "C";
			AssertEquals("LaceyAct Indicator", "MULTI", product.ImportLaceyActPGAIndicator);
		}

		public void TestCountryOfOrigin()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_UC_NKCountryOfOrigin = "US";
			AssertEquals("Country of Origin", "US", product.CountryOfOrigin);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_UC_NKCountryOfOrigin = "UK";
			AssertEquals("Country of Origin", "MULTI", product.CountryOfOrigin);
		}

		public void TestCountryOfExport()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_UC_NKCountryOfExport = "US";
			AssertEquals("Country of Export", "US", product.CountryOfExport);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_UC_NKCountryOfExport = "UK";
			AssertEquals("Country of Export", "MULTI", product.CountryOfExport);
		}

		public void TestSPIIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_SPI = "MX";
			AssertEquals("SPI Indicator", "MX", product.SPIIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_SPI = "MY";
			AssertEquals("SPI Indicator", "MULTI", product.SPIIndicator);
		}

		public void TestProductClaim()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_ProductClaim = "X";
			AssertEquals("Product Claim", "X", product.ProductClaim);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_ProductClaim = "Y";
			AssertEquals("Product Claim", "MULTI", product.ProductClaim);
		}

		public void TestRulingType()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_RulingType = "R";
			AssertEquals("Ruling Type", "R", product.RulingType);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_RulingType = "P";
			AssertEquals("Ruling Type", "MULTI", product.RulingType);
		}

		public void TestRulingNum()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_RulingNumber = "123456";
			AssertEquals("Ruling Number", "123456", product.RulingNum);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_RulingNumber = "654321";
			AssertEquals("Ruling Number", "MULTI", product.RulingNum);
		}

		public void TestTSCAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_TSCAIndicator = "T";
			AssertEquals("TSCA Indicator", "T", product.TSCAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_TSCAIndicator = "Y";
			AssertEquals("TSCA Indicator", "MULTI", product.TSCAIndicator);
		}

		public void TestCottonFeeExempt()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_CottonFeeExempt = "C";
			AssertEquals("Cotton Fee Exemption", "C", product.CottonFeeExempt);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_CottonFeeExempt = "W";
			AssertEquals("Cotton Fee Exemption", "MULTI", product.CottonFeeExempt);
		}

		public void TestADDCaseNum()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_ADDCaseNo = "A428000000";
			AssertEquals("ADD Case Number", "A428000000", product.ADDCaseNum);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_ADDCaseNo = "A428000001";
			AssertEquals("ADD Case Number", "MULTI", product.ADDCaseNum);
		}

		public void TestCVDCaseNum()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_CVDCaseNo = "C582000000";
			AssertEquals("CVD Case Number", "C582000000", product.CVDCaseNum);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_CVDCaseNo = "C582000001";
			AssertEquals("CVD Case Number", "MULTI", product.CVDCaseNum);
		}

		public void TestTaxApplicability()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_TaxApplicability = "U";
			AssertEquals("Tax Applicability", "U", product.TaxApplicability);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_TaxApplicability = "K";
			AssertEquals("Tax Applicability", "MULTI", product.TaxApplicability);
		}

		public void TestExportCode()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTE";

			pivot.CD_ExportCode = "E";
			AssertEquals("Export Code", "E", product.ExportCode);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";

			pivot2.CD_ExportCode = "K";
			AssertEquals("Export Code", "MULTI", product.ExportCode);
		}

		public void TestOriginIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTE";

			pivot.CD_OriginIndicator = "O";
			AssertEquals("Origin Indicator", "O", product.OriginIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";

			pivot2.CD_OriginIndicator = "P";
			AssertEquals("Origin Indicator", "MULTI", product.OriginIndicator);
		}

		public void TestLicenseType()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTE";

			pivot.CD_LicenceType = "L";
			AssertEquals("License Type", "L", product.LicenseType);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";

			pivot2.CD_LicenceType = "K";
			AssertEquals("License Type", "MULTI", product.LicenseType);
		}

		public void TestECCN()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTE";

			pivot.CD_ECCN = "1234";
			AssertEquals("ECCN", "1234", product.ECCN);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";

			pivot2.CD_ECCN = "5678";
			AssertEquals("ECCN", "MULTI", product.ECCN);
		}

		public void TestITARExemptionNum()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTE";

			pivot.CD_ITARExemptionNo = "123";
			AssertEquals("ITAR", "123", product.ITARExemptionNum);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTE";

			pivot2.CD_ITARExemptionNo = "456";
			AssertEquals("ITAR", "MULTI", product.ITARExemptionNum);
		}

		public void TestMultiTariffIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			AssertEquals("Multi Tariff", false, product.MultiTariffIndicator);

			pivot.Children.AddNew();
			AssertEquals("Multi Tariff", true, product.MultiTariffIndicator);
		}

		public void TestTariffDescription()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals("Tariff Description", "", product.TariffDescription);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "1234567890";
			pivot.CI_ChildType = "HTI";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_ShortDescription = "Test Description";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			AssertEquals("Tariff Description", "Test Description", product.TariffDescription);

			product.PivotsForBinding.AddNew();
			AssertEquals("Tariff Description", "MULTI", product.TariffDescription);
		}

		public void TestTariffType()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals("Tariff Type", "", product.TariffType);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			AssertEquals("Tariff Type", "HTI", product.TariffType);

			product.PivotsForBinding.AddNew();
			AssertEquals("Tariff Type", "MULTI", product.TariffType);
		}

		public void TestProvProgTariff()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			AssertEquals("Prov Prog", "", product.ProvProgTariff);

			pivot.CI_SupplementalTariff = "1234567890";

			AssertEquals("Prov Prog", "1234567890", product.ProvProgTariff);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_SupplementalTariff = "0987654321";
			AssertEquals("Prov Prog", "MULTI", product.ProvProgTariff);
		}

		public void TestHasLaceyData()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertEquals("Has Lacey", false, product.HasLaceyData);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			var child = pivot.Children.AddNew();
			AssertEquals("Has Lacey", false, product.HasLaceyData);

			child.PGAs.AddNew();
			AssertEquals("Has Lacey", true, product.HasLaceyData);

			child.PGAs.RemoveAndDeleteAll();
			AssertEquals("Has Lacey", false, product.HasLaceyData);

			pivot.PGAs.AddNew();
			AssertEquals("Has Lacey", true, product.HasLaceyData);
		}

		public void TestManufacturer()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TEST";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = header.PK;
			pivot.CD_OA_Manufacturer = address.PK;

			AssertEquals("Manufacturer", "TEST", product.Manufacturer);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			var header2 = Factory.New<OrgHeader>();
			header2.OH_Code = "TEST2";
			var address2 = Factory.New<OrgAddress>();
			address2.OA_OH = header.PK;
			pivot2.CD_OA_Manufacturer = address2.PK;

			AssertEquals("Manufacturer", "MULTI", product.Manufacturer);
		}

		public void TestAttributeValues()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			AssertEquals("Attribute Values", "", product.AttributeValues);

			var att1 = pivot.Attributes1.AddNew();
			att1.BG_AttributeValue1 = "TEST";
			AssertEquals("Attribute Values", "TEST", product.AttributeValues);

			var att2 = pivot.Attributes2.AddNew();
			att2.BG_AttributeValue1 = "TEST";
			AssertEquals("Attribute Values", "MULTI", product.AttributeValues);

			pivot.Attributes2.DeleteAll();
			AssertEquals("Attribute Values", "TEST", product.AttributeValues);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			var att3 = pivot2.Attributes1.AddNew();
			att3.BG_AttributeValue1 = "TEST";
			AssertEquals("Attribute Values", "MULTI", product.AttributeValues);
		}

		public void TestNHTSAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_NHTSAIndicator = "D";
			AssertEquals("NHTSA Indicator", "D", product.ImportNHTSAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_NHTSAIndicator = "C";
			AssertEquals("NHTSA Indicator", "MULTI", product.ImportNHTSAIndicator);
		}

		public void TestODSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_ODSIndicator = "D";
			AssertEquals("ODS Indicator", "D", product.ImportODSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_ODSIndicator = "C";
			AssertEquals("ODS Indicator", "MULTI", product.ImportODSIndicator);
		}

		public void TestOMCIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_OMCIndicator = "D";
			AssertEquals("OMC Indicator", "D", product.ImportOMCIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_OMCIndicator = "C";
			AssertEquals("OMC Indicator", "MULTI", product.ImportOMCIndicator);
		}

		public void TestTSCAClaimIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_TSCAClaimIndicator = "D";
			AssertEquals("TSCA Indicator", "D", product.ImportTSCAClaimIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_TSCAClaimIndicator = "C";
			AssertEquals("TSCA Indicator", "MULTI", product.ImportTSCAClaimIndicator);
		}

		public void TestPSTIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_PSTIndicator = "D";
			AssertEquals("PST Indicator", "D", product.ImportPSTIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_PSTIndicator = "C";
			AssertEquals("PST Indicator", "MULTI", product.ImportPSTIndicator);
		}

		public void TestCPSCIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_CPSCIndicator = "D";
			AssertEquals("CPSC Indicator", "D", product.ImportCPSCIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_CPSCIndicator = "C";
			AssertEquals("CPSC Indicator", "MULTI", product.ImportCPSCIndicator);
		}

		public void TestVNEIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_VNEIndicator = "D";
			AssertEquals("VNE Indicator", "D", product.ImportVNEIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_VNEIndicator = "C";
			AssertEquals("VNE Indicator", "MULTI", product.ImportVNEIndicator);
		}

		public void TestDEAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_DEAIndicator = "D";
			AssertEquals("DEA Indicator", "D", product.ImportDEAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_DEAIndicator = "C";
			AssertEquals("DEA Indicator", "MULTI", product.ImportDEAIndicator);
		}

		public void TestAPHISIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "HTI";

			pivot.CD_APHISIndicator = "D";
			AssertEquals("APHIS Indicator should be D", "D", product.ImportAPHISIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = "HTI";

			pivot2.CD_APHISIndicator = "C";
			AssertEquals("APHIS Indicator should be C", "MULTI", product.ImportAPHISIndicator);
		}

		public void TestExportATFIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_ATFIndicator = "D";
			AssertEquals("Export ATF Indicator", "D", product.ExportATFIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_ATFIndicator = "D";
			AssertEquals("Export ATF Indicator", "D", product.ExportATFIndicator);
			pivot2.Details.CD_ATFIndicator = "X";
			AssertEquals("Export ATF Indicator", "MULTI", product.ExportATFIndicator);
		}

		public void TestExportAMSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_AMSIndicator = "D";
			AssertEquals("Export AMS Indicator", "D", product.ExportAMSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_AMSIndicator = "D";
			AssertEquals("Export AMS Indicator", "D", product.ExportAMSIndicator);
			pivot2.Details.CD_AMSIndicator = "X";
			AssertEquals("Export AMS Indicator", "MULTI", product.ExportAMSIndicator);
		}

		public void TestExportDEAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_DEAIndicator = "D";
			AssertEquals("Export DEA Indicator", "D", product.ExportDEAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_DEAIndicator = "D";
			AssertEquals("Export DEA Indicator", "D", product.ExportDEAIndicator);
			pivot2.Details.CD_DEAIndicator = "X";
			AssertEquals("Export DEA Indicator", "MULTI", product.ExportDEAIndicator);
		}

		public void TestExportTTBIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_TTBIndicator = "D";
			AssertEquals("Export TTB Indicator", "D", product.ExportTTBIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_TTBIndicator = "D";
			AssertEquals("Export TTB Indicator", "D", product.ExportTTBIndicator);
			pivot2.Details.CD_TTBIndicator = "X";
			AssertEquals("Export TTB Indicator", "MULTI", product.ExportTTBIndicator);
		}

		public void TestExportEPAIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_PSTIndicator = "D";
			AssertEquals("Export EPA Indicator", "D", product.ExportEPAIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_PSTIndicator = "D";
			AssertEquals("Export EPA Indicator", "D", product.ExportEPAIndicator);
			pivot2.Details.CD_PSTIndicator = "X";
			AssertEquals("Export EPA Indicator", "MULTI", product.ExportEPAIndicator);
		}

		public void TestExportFWSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_FWSIndicator = "D";
			AssertEquals("Export FWS Indicator", "D", product.ExportFWSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_FWSIndicator = "D";
			AssertEquals("Export FWS Indicator", "D", product.ExportFWSIndicator);
			pivot2.Details.CD_FWSIndicator = "X";
			AssertEquals("Export FWS Indicator", "MULTI", product.ExportFWSIndicator);
		}

		public void TestExportNMFSIndicator()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.Details.CD_NMFSHMSIndicator = "D";
			AssertEquals("Export NMFS Indicator", "D", product.ExportNMFSIndicator);

			var pivot2 = product.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot2.Details.CD_NMFSHMSIndicator = "D";
			AssertEquals("Export NMFS Indicator", "D", product.ExportNMFSIndicator);
			pivot2.Details.CD_NMFSHMSIndicator = "X";
			AssertEquals("Export NMFS Indicator", "MULTI", product.ExportNMFSIndicator);
		}

		protected override Type ExpectedClassificationCollectionType => typeof(ClassificationCollection<CusClassification>);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var part = factory.NewWithValidTestData<OrgSupplierPart>();
			var supplier = OrgHeader.New(factory);
			supplier.OH_FullName = "Delete test Supplier";
			supplier.MainAddress.OA_Address1 = "Delete Address 1";
			supplier.OH_RL_NKClosestPort = "AUSYD";
			var partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_OH = supplier.PK;
			return part;
		}

		protected override BusinessObject GetNewBusinessObject() => OrgSupplierPart.New(Factory);
	}
}
