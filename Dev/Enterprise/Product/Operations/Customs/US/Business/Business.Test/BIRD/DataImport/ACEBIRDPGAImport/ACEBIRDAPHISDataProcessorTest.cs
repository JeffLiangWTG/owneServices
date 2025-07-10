using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using APHISArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using APHISCommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDAPHISDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public void TestProcessUSDAAPHISGrower()
		{
			var message = GetEDIMessageForEndToEndTestWithPropagativeMaterialType();
			ImportMessageBlocks(message);
			AssertEquals(USDAGrower.PK, DeclarationImported.InvoiceLines[0].APHISHeaders[0].US_OA_USDAAPHISGrowerAddress);
		}

		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_APHISDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST APHIS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_APHISInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_APHISDisclaimReason);
			AssertEquals("TEST APHIS DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.JE_OH_Importer = Importer.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_FDAContactName = "IAN CHEN";
			declaration.US_FDAContactPhoneNo = "6301023498";
			declaration.US_FDAContactEmail = "IAN.CHEN@TEST.COM";

			invoiceLine.JI_Description = "IAN TEST APHIS";
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;

			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForCommercialSale;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.LiveAnimals;
			header.US_CategoryCode = APHISArticleCategory.LiveAnimalsList.Codes.CanidaeDogs;
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_Qty1 = 2m;
			header.US_UQ1 = "NO";
			header.US_CommoditySpecificName = "MALINOIS";
			header.US_ScientificGenusName = "CANUS";
			header.US_ScientificSpeciesName = "LUPUS";
			header.US_ScientificSubSpeciesName = "MALINOIS";
			header.US_OA_USDAAPHISGrowerAddress = USDAGrower.PK;

			var source = header.Sources.Count > 0 ? header.Sources[0] : header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.HongKong;

			var product1 = header.Products.AddNew();
			product1.US_ShowBreed = "DOG";
			product1.US_BreedVariety = "DGBM";
			product1.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months;
			product1.US_Gender = "M";

			var identity1 = product1.Identities.AddNew();
			identity1.CY_Code = APHISItemIdentityNumberQualifierList.Codes.CHP;
			identity1.CY_Data = "00001";

			var product2 = header.Products.AddNew();
			product2.US_ShowBreed = "DOG";
			product2.US_BreedVariety = "DGBM";
			product2.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months;
			product2.US_Gender = "M";

			var identity2 = product2.Identities.AddNew();
			identity2.CY_Code = APHISItemIdentityNumberQualifierList.Codes.CHP;
			identity2.CY_Data = "00002";

			var license0 = header.Licenses.AddNew();
			license0.US_RN_CountryCode = Core.Constants.CountryCodes.HongKong;
			license0.US_Quantity = 1m;
			license0.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license0.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license0.US_Number = "0000001";
			license0.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license0.US_Date = new ZDateTime(2016, 10, 20);

			var license1 = header.Licenses.AddNew();
			license1.US_RN_CountryCode = Core.Constants.CountryCodes.HongKong;
			license1.US_Quantity = 1m;
			license1.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license1.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license1.US_Number = "0000002";
			license1.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license1.US_Date = new ZDateTime(2016, 10, 20);
			var inspection = header.Inspections.Count > 0 ? header.Inspections[0] : header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection;
			inspection.US_Location = "1101";
			inspection.US_Date = new ZDateTime(2016, 10, 24);

			var routing = header.Routings.Count > 0 ? header.Routings[0] : header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.HongKong;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals("IAN CHEN", DeclarationImported.US_FDAContactName);
			AssertEquals("6301023498", DeclarationImported.US_FDAContactPhoneNo);
			AssertEquals("IAN.CHEN@TEST.COM", DeclarationImported.US_FDAContactEmail);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_APHISInd);
			AssertEquals("IAN TEST APHIS", invoiceLineImported.JI_Description);
			AssertEquals(UltimateConsignee.PK, invoiceLineImported.JI_OA_ConsigneeAddress);
			AssertEquals(1, invoiceLineImported.APHISHeaders.Count);

			var headerImported = invoiceLineImported.APHISHeaders[0];
			CombineAssertions(() =>
			{
				AssertEquals("header.US_ProgramType", APHISProgramCodeList.Codes.AVS, headerImported.US_ProgramType);
				AssertEquals("header.US_ProcessingCode", APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian, headerImported.US_ProcessingCode);
				AssertEquals("header.US_IntendedUseCode", IntendedUseCodesList.Codes.AnimalOrPlantForCommercialSale, headerImported.US_IntendedUseCode);
				AssertEquals("header.US_CategoryType", APHISCategoryTypeCodeList.Codes.LiveAnimals, headerImported.US_CategoryType);
				AssertEquals("header.US_CategoryCode", APHISArticleCategory.LiveAnimalsList.Codes.CanidaeDogs, headerImported.US_CategoryCode);
				AssertEquals("header.US_OA_ApplicantAddress", PermitHolder.PK, headerImported.US_OA_ApplicantAddress);
				AssertEquals("header.US_OA_USDAAPHISGrowerAddress", ZGuid.Empty, headerImported.US_OA_USDAAPHISGrowerAddress);
				AssertEquals("header.US_Qty1", 2m, headerImported.US_Qty1);
				AssertEquals("header.US_UQ1", "NO", headerImported.US_UQ1);
				AssertEquals("header.US_CommoditySpecificName", "MALINOIS", headerImported.US_CommoditySpecificName);
				AssertEquals("header.US_ScientificGenusName", "CANUS", headerImported.US_ScientificGenusName);
				AssertEquals("header.US_ScientificSpeciesName", "LUPUS", headerImported.US_ScientificSpeciesName);
				AssertEquals("header.US_ScientificSubSpeciesName", "MALINOIS", headerImported.US_ScientificSubSpeciesName);
				AssertEquals("header.Sources.Count", 1, headerImported.Sources.Count);
				AssertEquals("header.Products.Count", 2, headerImported.Products.Count);
				AssertEquals("header.Licenses.Count", 2, headerImported.Licenses.Count);
				AssertEquals("header.Inspections.Count", 1, headerImported.Inspections.Count);
				AssertEquals("header.Routings.Count", 1, headerImported.Routings.Count);

				var sourceImported = headerImported.Sources[0];
				AssertEquals("source.US_SourceTypeCode", SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, sourceImported.US_SourceTypeCode);
				AssertEquals("source.US_CountryCode", Core.Constants.CountryCodes.HongKong, sourceImported.US_CountryCode);

				var productImported1 = headerImported.Products[0];
				AssertEquals("product1.US_BreedVariety", "DGBM", productImported1.US_BreedVariety);
				AssertEquals("product1.US_Age", APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months, productImported1.US_Age);
				AssertEquals("product1.US_Gender", "M", productImported1.US_Gender);
				AssertEquals("product1.Identities.Count", 1, productImported1.Identities.Count);
				var identityImported1 = productImported1.Identities[0];
				AssertEquals("identity1.CY_Code", APHISItemIdentityNumberQualifierList.Codes.CHP, identityImported1.CY_Code);
				AssertEquals("identity1.CY_Data", "00001", identityImported1.CY_Data);

				var productImported2 = headerImported.Products[1];
				AssertEquals("product2.US_BreedVariety", "DGBM", productImported2.US_BreedVariety);
				AssertEquals("product2.US_Age", APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months, productImported2.US_Age);
				AssertEquals("product2.US_Gender", "M", productImported2.US_Gender);
				AssertEquals("product2.Identities.Count", 1, productImported2.Identities.Count);
				var identityImported2 = productImported2.Identities[0];
				AssertEquals("identity2.CY_Code", APHISItemIdentityNumberQualifierList.Codes.CHP, identityImported2.CY_Code);
				AssertEquals("identity2.CY_Data", "00002", identityImported2.CY_Data);

				var license0Imported = headerImported.Licenses[0];
				AssertEquals("license0.US_RN_CountryCode", Core.Constants.CountryCodes.HongKong, license0Imported.US_RN_CountryCode);
				AssertEquals("license0.US_Quantity", 1m, license0Imported.US_Quantity);
				AssertEquals("license0.US_UnitOfMeasure", ABIUnitOfMeasureList.Codes.Number, license0Imported.US_UnitOfMeasure);
				AssertEquals("license0.US_Type", APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate, license0Imported.US_Type);
				AssertEquals("license0.US_Number", "0000001", license0Imported.US_Number);
				AssertEquals("license0.US_DateQualifier", LPCODateQualifierList.Codes.DateIssuedOrSigned, license0Imported.US_DateQualifier);
				AssertEquals("license0.US_Date", new ZDateTime(2016, 10, 20), license0Imported.US_Date);

				var license1Imported = headerImported.Licenses[1];
				AssertEquals("license1.US_RN_CountryCode", Core.Constants.CountryCodes.HongKong, license1Imported.US_RN_CountryCode);
				AssertEquals("license1.US_Quantity", 1m, license1Imported.US_Quantity);
				AssertEquals("license1.US_UnitOfMeasure", ABIUnitOfMeasureList.Codes.Number, license1Imported.US_UnitOfMeasure);
				AssertEquals("license1.US_Type", APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate, license1Imported.US_Type);
				AssertEquals("license1.US_Number", "0000002", license1Imported.US_Number);
				AssertEquals("license1.US_DateQualifier", LPCODateQualifierList.Codes.DateIssuedOrSigned, license1Imported.US_DateQualifier);
				AssertEquals("license1.US_Date", new ZDateTime(2016, 10, 20), license1Imported.US_Date);

				var inspectionImported = headerImported.Inspections[0];
				AssertEquals("inspection.US_TestingStatus", InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, inspectionImported.US_TestingStatus);
				AssertEquals("inspection.US_Location", "1101", inspectionImported.US_Location);
				AssertEquals("inspection.US_Date", new ZDateTime(2016, 10, 24), inspectionImported.US_Date);

				var routingImported = headerImported.Routings[0];
				AssertEquals("routing.US_Type", RoutingTypeList.Codes.OriginalLocation, routingImported.US_Type);
				AssertEquals("routing.US_Country", Core.Constants.CountryCodes.HongKong, routingImported.US_Country);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_Code = "TESTIOR";
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");
			invoiceLine.JI_OA_ExporterAddress = ior.MainAddress.PK;
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
		}

		MQEDIMessage GetEDIMessageForEndToEndTestWithPropagativeMaterialType()
		{
			declaration.JE_OH_Importer = Importer.OA_OH;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_FDAContactName = "IAN CHEN";
			declaration.US_FDAContactPhoneNo = "6301023498";
			declaration.US_FDAContactEmail = "IAN.CHEN@TEST.COM";

			invoiceLine.JI_Description = "IAN TEST APHIS";
			invoiceLine.JI_OA_ConsigneeAddress = UltimateConsignee.PK;

			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			header.US_ProcessingCode = APHISGovernmentAgencyProcessingCodeList.Codes.APHISVSPortVeterinarian;
			header.US_IntendedUseCode = IntendedUseCodesList.Codes.AnimalOrPlantForCommercialSale;
			header.US_CategoryType = APHISCategoryTypeCodeList.Codes.PropagativeMaterial;
			header.US_CategoryCode = APHISArticleCategory.LiveAnimalsList.Codes.CanidaeDogs;
			header.US_OA_ApplicantAddress = PermitHolder.PK;
			header.US_Qty1 = 2m;
			header.US_UQ1 = "NO";
			header.US_CommoditySpecificName = "MALINOIS";
			header.US_ScientificGenusName = "CANUS";
			header.US_ScientificSpeciesName = "LUPUS";
			header.US_ScientificSubSpeciesName = "MALINOIS";
			header.US_OA_USDAAPHISGrowerAddress = USDAGrower.PK;

			var source = header.Sources.Count > 0 ? header.Sources[0] : header.Sources.AddNew();
			source.US_SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSpeciesOrigin;
			source.US_CountryCode = Core.Constants.CountryCodes.HongKong;

			var product1 = header.Products.AddNew();
			product1.US_ShowBreed = "DOG";
			product1.US_BreedVariety = "DGBM";
			product1.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months;
			product1.US_Gender = "M";

			var identity1 = product1.Identities.AddNew();
			identity1.CY_Code = APHISItemIdentityNumberQualifierList.Codes.CHP;
			identity1.CY_Data = "00001";

			var product2 = header.Products.AddNew();
			product2.US_ShowBreed = "DOG";
			product2.US_BreedVariety = "DGBM";
			product2.US_Age = APHISCommodityCharacteristicQualifier.LiveAnimalsAgeList.Codes._7To12Months;
			product2.US_Gender = "M";

			var identity2 = product2.Identities.AddNew();
			identity2.CY_Code = APHISItemIdentityNumberQualifierList.Codes.CHP;
			identity2.CY_Data = "00002";

			var license0 = header.Licenses.AddNew();
			license0.US_RN_CountryCode = Core.Constants.CountryCodes.HongKong;
			license0.US_Quantity = 1m;
			license0.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license0.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license0.US_Number = "0000001";
			license0.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license0.US_Date = new ZDateTime(2016, 10, 20);

			var license1 = header.Licenses.AddNew();
			license1.US_RN_CountryCode = Core.Constants.CountryCodes.HongKong;
			license1.US_Quantity = 1m;
			license1.US_UnitOfMeasure = ABIUnitOfMeasureList.Codes.Number;
			license1.US_Type = APHISLicenseTypeList.Codes.LiveAnimalHealthCertificate;
			license1.US_Number = "0000002";
			license1.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			license1.US_Date = new ZDateTime(2016, 10, 20);
			var inspection = header.Inspections.Count > 0 ? header.Inspections[0] : header.Inspections.AddNew();
			inspection.US_TestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection;
			inspection.US_Location = "1101";
			inspection.US_Date = new ZDateTime(2016, 10, 24);

			var routing = header.Routings.Count > 0 ? header.Routings[0] : header.Routings.AddNew();
			routing.US_Type = RoutingTypeList.Codes.OriginalLocation;
			routing.US_Country = Core.Constants.CountryCodes.HongKong;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = ZDateTime.Today;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		OrgAddress Importer
		{
			get
			{
				if (importer == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTIMP";
					org.OH_FullName = "IMPORTER";
					org.OH_RL_NKClosestPort = "USLAX";
					importer = org.MainAddress;
					importer.OA_Address1 = "IM ADDRESS 1";
					importer.OA_Address2 = "IM ADDRESS 2";
					importer.OA_RL_NKRelatedPortCode = "USLAX";
					importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "32-843933", Core.Constants.CountryCodes.UnitedStates);
				}
				return importer;
			}
		}
		OrgAddress importer;

		OrgAddress PermitHolder
		{
			get
			{
				if (permitHolder == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTPERMIT";
					org.OH_FullName = "PERMIT HOLDER";
					org.OH_RL_NKClosestPort = "USNYC";
					permitHolder = org.MainAddress;
					permitHolder.OA_Address1 = "PH ADDRESS 1";
					permitHolder.OA_Address2 = "PH ADDRESS 2";
					permitHolder.OA_RL_NKRelatedPortCode = "USNYC";
					permitHolder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD443", Core.Constants.CountryCodes.UnitedStates);
				}
				return permitHolder;
			}
		}
		OrgAddress permitHolder;

		OrgAddress UltimateConsignee
		{
			get
			{
				if (ultimateConsignee == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTCNE";
					org.OH_FullName = "ULTIMATE CONSIGNEE";
					org.OH_RL_NKClosestPort = "USCHI";
					ultimateConsignee = org.MainAddress;
					ultimateConsignee.OA_Address1 = "UC ADDRESS 1";
					ultimateConsignee.OA_Address2 = "UC ADDRESS 2";
					ultimateConsignee.OA_RL_NKRelatedPortCode = "USCHI";
					ultimateConsignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "32-23-234232", Core.Constants.CountryCodes.UnitedStates);
				}
				return ultimateConsignee;
			}
		}
		OrgAddress ultimateConsignee;

		OrgAddress USDAGrower
		{
			get
			{
				if (usdaGrower == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_Code = "USDAGROW";
					org.OH_FullName = "USDA Grower";
					org.OH_RL_NKClosestPort = "USCHI";
					usdaGrower = org.MainAddress;
					usdaGrower.OA_Address1 = "GR ADDRESS 1";
					usdaGrower.OA_Address2 = "GR ADDRESS 2";
					usdaGrower.OA_RL_NKRelatedPortCode = "USCHI";
					usdaGrower.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.APHISAssignedNumber, "32KD445", Core.Constants.CountryCodes.UnitedStates);
				}
				return usdaGrower;
			}
		}
		OrgAddress usdaGrower;
	}
}
