using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	public class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_OH_ImporterUsingDTYCustomsRule()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var today = new ZDate(2023, 01, 30);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_OH_PermitHolder = importer.PK;
			customsRule.CPH_StartDate = today.AddMonths(-1);
			var rule01 = customsRule.Rules.AddNew();
			rule01.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalDuty;
			rule01.CPR_ValueTo = "10000";
			Factory.Save();

			var messageError = "exceeds maximum amount";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateOfFirstArrival = today;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.IsBrokerToPay);
			AssertEquals(false, declaration.IsImporterToPay);
			AssertEquals(0m, declaration.DisbursementAmount);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			declaration.CustomsEntryInstructions?.Load();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			invoiceLine1.US_Duty = 5000m;
			invoiceLine1.US_SupDuty = 0.5m;
			invoiceLine2.US_Duty = 5000m;
			invoiceLine2.US_SupDuty = 0.5m;
			var validation = declaration.Validation;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Duty Amount (10001.00) exceeds maximum amount defined by Custom Rules.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			customsRule.CPH_PermitDescription = "TEST Rule";
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Duty Amount (10001.00) exceeds maximum amount defined by Custom Rule: TEST Rule.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			invoiceLine1.US_SupDuty = 0m;
			invoiceLine2.US_SupDuty = 0m;
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);
		}

		public override void TestCheckJE_OH_ImporterUsingCustomsRule()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var today = new ZDate(2023, 01, 30);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_OH_PermitHolder = importer.PK;
			customsRule.CPH_StartDate = today.AddMonths(-1);
			var rule01 = customsRule.Rules.AddNew();
			rule01.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsValue;
			rule01.CPR_ValueTo = "10000";
			var rule02 = customsRule.Rules.AddNew();
			rule02.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			rule02.CPR_ValueTo = "0";
			Factory.Save();

			var messageError = "exceeds maximum amount";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateOfFirstArrival = today;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.IsBrokerToPay);
			AssertEquals(false, declaration.IsImporterToPay);
			AssertEquals(0m, declaration.DisbursementAmount);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.CustomsEntryInstructions?.Load();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.FormalEntry;
			var entryLine = entry.MergedLines[0];
			entryLine.CL_CustomsValue = 10001m;
			entry.ResetIsCustomsValueCalculated();
			var validation = declaration.Validation;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Value (10001.00) exceeds maximum amount defined by Custom Rules.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			entryLine.CL_CustomsValue = 10000m;
			entry.ResetIsCustomsValueCalculated();
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);
		}

		public void TestCheckJE_OH_ImporterUsingCSDCustomsRule()
		{
			var today = new ZDate(2023, 01, 30);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_OH_PermitHolder = importer.PK;
			customsRule.CPH_StartDate = today.AddMonths(-1);
			var rule01 = customsRule.Rules.AddNew();
			rule01.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
			rule01.CPR_ValueTo = "10000";
			var rule02 = customsRule.Rules.AddNew();
			rule02.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.PaymentType;
			rule02.CPR_ValueFrom = CustomsRuleRulePaymentTypeValueFromCodeList.Codes.Broker;
			Factory.Save();

			var messageError01 = "exceeds maximum amount";
			var messageError02 = "Total Customs Disbursement Amount";
			var messageError03 = "Total Customs Disbursement Amount (10001.00) exceeds maximum amount defined by Custom Rules.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);

			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.US_DutyCalcDate = today;
			ensEntry.CH_TotalPaid = 10001m;
			var validation = declaration.Validation;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError03);

			ensEntry.CH_TotalPaid = 10000m;
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);

			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.No;
			ensEntry.CH_TotalPaid = 10001m;
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);

			rule02.CPR_ValueFrom = CustomsRuleRulePaymentTypeValueFromCodeList.Codes.Import;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError03);

			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			customsRule.Rules.Delete(rule02);
			validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError01);
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError02);
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError03);
		}

		public void TestAddressShouldValidateForeignCharacters()
		{
			string addressDescriptionWarning = "Address Description : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";
			string addressCodeWarning = "Address Code : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with an asterisk '*'.";

			var factory = new BusinessObjectFactory();

			var party = factory.New<OrgHeader>();
			var orgAddress = party.MainAddress;
			orgAddress.OA_City = "KYIV";
			orgAddress.OA_Address1 = "éééÄöß";
			orgAddress.OA_Address2 = "Address2Äöß";
			orgAddress.OA_Code = "öß";

			Assembly assembly = Assembly.Load("Enterprise.Customs.US.Business");
			Type[] allTypes = assembly.GetTypes();
			var exemptTypes = new HashSet<string>()
			{
				"CusUnderbond",
				"FWSHeader",
				"TTBLine",
				"CusClassPartPivot",
				"AllocationQuantityPerFPI",
				"FishingInformation"
			};
			var exemptProperties = new HashSet<string>()
			{
				//below properties are not used in US
				"JZ_OA_BuyerAddressInfo",
				"JZ_OA_ConsigneeAddressInfo",
				"JZ_OA_IntermediateConsigneeAddressInfo",
				"JZ_OA_InvoicerAddressInfo",
				"JZ_OA_SupplierAddressInfo",
				"JI_OA_ConsigneeAddressInfo",
				"JI_OA_GrowerAddressInfo",
				"JI_OA_ProducerAddressInfo",
				"JI_OA_LocalFacilityInfo",
				"JI_OH_TreatmentProviderInfo",
				"JE_OA_BuyingAgentAddressInfo",
				"JE_OA_DeliveryOrPickupCartageCoAddrInfo",
				"JE_OA_ConsigneeAddressInfo",
				"JE_OA_DeclarantAddressInfo",
				"JE_OA_ExporterAddressInfo",
				"JE_OA_RepresentativeInfo",
				"JE_OA_ImporterAddressInfo",
				"JE_OA_SupplierAddressInfo",
				"JE_OA_DistributorAddressInfo",
				"JE_OA_PackagerAddressInfo",
				"JE_OA_ShipperAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_ExporterAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_ManufacturerAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_SellerAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_ShipToPartyAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_SoldToPartyAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_DistributorAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_PackagerAddressInfo",
				"JobComInvoiceGroupHeader/JZ_OA_ShipperAddressInfo",
				"JobComInvoiceLine/US_OA_DRWExporterOrDestroyerInfo",

				//below properties are tested in there own classes
				"APHISHeader/US_OA_ApplicantAddressInfo",
				"APHISHeader/US_OA_CropGrowerAddressInfo",
				"APHISHeader/US_OA_PermittedAddressInfo",
				"APHISHeader/US_OA_ShipperAddressInfo",
				"APHISHeader/US_OA_USDAAPHISGrowerAddressInfo",

				"CPSCHeader/US_OA_ManufacturerAddressInfo",
				"CPSCHeader/US_OA_CertifyingEntityAddressInfo",
				"CPSCHeader/US_OA_ContactPointAddressInfo",

				"Pesticide/US_OA_ExaminationLocationInfo",
				"Pesticide/US_OA_ShipperAddressInfo",

				"OMCHeader/US_OA_ExporterInfo",
				"OMCHeader/US_OA_ResponsibleGovernmentOfficialInfo",
				"OMCHeader/US_OA_AquacultureFacilityInfo",

				"ACEFDA/US_OA_ShipperAddressInfo",

				"NMFSHarvestingDetail/US_OA_ContactPartyInfo",

				"Vehicle/US_OA_OwnerInfo",
				"Vehicle/US_OA_StorageLocationInfo",
			};

			foreach (var objType in allTypes)
			{
				if (exemptTypes.Contains(objType.Name))
				{
					continue;
				}

				if (typeof(BusinessObject).IsAssignableFrom(objType))
				{
					BusinessObject obj = null;
					try
					{
						obj = factory.New(objType);
					}
					catch { }

					if (obj == null)
					{
						continue;
					}

					bool isContainAddress = false;

					foreach (PropertyInfo property in objType.GetProperties())
					{
						if (property.Name.Contains("_OA_"))
						{
							if (typeof(ZGuid).IsAssignableFrom(property.PropertyType))
							{
								property.SetValue(obj, orgAddress.PK);
								isContainAddress = true;
							}
						}
					}

					if (!isContainAddress)
					{
						continue;
					}

					var propInfo = objType.GetProperty("Validation", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
					ZValidation validation = propInfo?.GetValue(obj, null) as ZValidation;
					if (validation != null)
					{
						validation.ValidateAll();
					}

					ZValidation addInfoValidation = objType.GetProperty("AddInfoValidation")?.GetValue(obj, null) as ZValidation;
	
					if (addInfoValidation != null)
					{
						addInfoValidation.ValidateAll();
					}

					if (validation == null && addInfoValidation == null)
					{
						continue;
					}

					foreach (var property in objType.GetProperties())
					{
						if (property.Name.Contains("_OA_") && typeof(ZPropertyInfo).IsAssignableFrom(property.PropertyType))
						{
							var zPropertyValue = ((ZPropertyInfo)property.GetValue(obj, Array.Empty<object>()));
							if (!((ZGuid)zPropertyValue.Value).IsEmpty && !exemptProperties.Contains(property.Name) && !exemptProperties.Contains(objType.Name + "/" + property.Name) && !objType.Namespace.EndsWith("Testing"))
							{
								AssertHasWarning(objType.Name + " should call OrganisationValidation.ValidateCharactorsForAddressDescription on " + property.Name, ((ZPropertyInfo)property.GetValue(obj, Array.Empty<object>())), addressDescriptionWarning);
								AssertHasWarning(objType.Name + " should call OrganisationValidation.ValidateCharactorsForAddressDescription on " + property.Name, ((ZPropertyInfo)property.GetValue(obj, Array.Empty<object>())), addressCodeWarning);
							}
						}
					}
				}
			}
		}

		public void TestCheckJE_MessageType_ErrorIfChangedAfterFTZTransactions()
		{
			AssertErrorsForFreeTradeZoneAfterChangingJE_MessageType(false, false, true);
		}

		public void TestCheckJE_MessageType_MessageErrorIfControllerChangedAfterFTZTransactions()
		{
			AssertErrorsForFreeTradeZoneAfterChangingJE_MessageType(true, false, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterFTZTransactions()
		{
			AssertErrorsForFreeTradeZoneAfterChangingJE_MessageType(false, true, false);
		}

		public void TestCheckJE_MessageType_ErrorIfChangedAfterTransactions()
		{
			AssertErrorsAfterChangingJE_MessageType(false, false, true);
		}

		public void TestCheckJE_MessageType_MessageErrorIfControllerChangedAfterHasTransactions()
		{
			AssertErrorsAfterChangingJE_MessageType(true, false, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterHasTransactions()
		{
			AssertErrorsAfterChangingJE_MessageType(false, true, false);
		}

		public void TestCheckJE_OH_FDASubmitter()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(5);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_FDAForcePN = true;

			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.JE_OH_FDASubmitterInfo, ACEImportJobDeclarationValidation.SubmitterRequireForPriorNotice);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "SUBMIT";
			declaration.JE_OH_FDASubmitter = orgHeader.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_FDASubmitterInfo, ACEImportJobDeclarationValidation.SubmitterRequireForPriorNotice);
		}

		public void TestTIBMotorVehiclesWith98130075Line()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.AUTO);
			Factory.Save();

			var tariff35 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98130035", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff35.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition35 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff35.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition35.PK, EntryTypeList.Codes.TemporaryImportationBond);

			var tariff75 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98130075", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff75.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition75 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff75.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition75.PK, EntryTypeList.Codes.TemporaryImportationBond);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TIBMVNonConforming = false;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(false, declaration.HasAutoCondition);
			invoiceLine.US_SupTariff = "98130075";
			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_LinePrice = 243055m;
			AssertEquals(true, declaration.HasAutoCondition);
			declaration.AddInfoValidation.ValidateUS_TIBMotorVehicles();
			AssertHasMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);
			declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			declaration.AddInfoValidation.ValidateUS_TIBMotorVehicles();
			AssertNoMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);
			declaration.US_TIBMotorVehicles = YesNoList.Codes.No;
			declaration.AddInfoValidation.ValidateUS_TIBMotorVehicles();
			AssertNoMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);

			invoiceLine.US_SupTariff = "98130035";
			declaration.US_TIBMotorVehicles = "";
			AssertEquals(true, declaration.HasAutoCondition);
			declaration.AddInfoValidation.ValidateUS_TIBMotorVehicles();
			AssertHasMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);
			declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
			AssertNoMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);
			declaration.US_TIBMotorVehicles = YesNoList.Codes.No;
			AssertNoMessageError(declaration.US_TIBMotorVehiclesInfo, ACEImportAddInfoJobDeclarationValidation.TIBMotorVehicles);
		}

		public void TestJE_OH_ExternalBroker()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ExternalBroker = organisation.PK;
			//precondition
			AssertHasWarning(declaration.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.NoValidCommunicationMethodIsDefinedForBIRD);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			decLoaded.RunPreSaveValidation();
			AssertHasWarning(decLoaded.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.NoValidCommunicationMethodIsDefinedForBIRD);
		}

		public void TestCheckJE_ContainerMode()
		{
			const string ContainerModeShouldBeEntered = "Container type is required for sea shipment.";
			const string ContainerModeShouldBeInList = "Please enter a valid Container Type. The code you have selected is not in the Container Types List.";

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.CusContainers.AddNew();
			AssertNoWarningContaining(declaration.JE_ContainerModeInfo, "container(s) for a mode that doesn't require them");
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertHasWarningContaining(declaration.JE_ContainerModeInfo, "container(s) for a mode that doesn't require them");
			AssertHasWarning(declaration.JE_ContainerModeInfo, "You have entered 1 container(s) for a mode that doesn't require them. These containers will not be used in any message or document for this reason.");
			AssertNoMessageError(declaration.JE_ContainerModeInfo, ContainerModeShouldBeInList);

			declaration.JE_ContainerMode = ZString.Empty;
			AssertHasMessageError(declaration.JE_ContainerModeInfo, ContainerModeShouldBeEntered);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications(declaration.JE_ContainerModeInfo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ContainerMode = "~";
			AssertHasMessageError(declaration.JE_ContainerModeInfo, ContainerModeShouldBeInList);

			// Air mode with duff container mode should not give an error:
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = "XXX";
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications(declaration.JE_ContainerModeInfo);
		}

		public override void TestPackagesActualPackageCount_Validation()
		{
			declaration.JE_TotalNoOfPacks = 100;
			AssertNoMessageErrors("No Message Error", declaration.PackagesActualPackageCountInfo);
		}

		public void TestCheckJE_TotalNoOfPacksPackType()
		{
			declaration.JE_TotalNoOfPacksPackType = "Z!";
			AssertHasWarningContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ValidationConstants.InvalidPackTypeMessage);
			foreach (ICodeDescription pair in new ShippingOrPackingingUnitList())
			{
				declaration.JE_TotalNoOfPacksPackType = pair.Code;
				AssertNoWarningContaining(declaration.JE_TotalNoOfPacksPackTypeInfo, ValidationConstants.InvalidPackTypeMessage);
			}
		}

		public void TestVesselVoyageNotRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestJE_ContainerMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = false;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertNoMessageError(declaration.JE_ContainerModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertHasMessageError(declaration.JE_ContainerModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);

			declaration.CusContainers.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertNoMessageError(declaration.JE_TransportModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);

			declaration.CusContainers.RemoveAndDeleteAll();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertNoWarning(declaration.JE_ContainerModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasWarning(declaration.JE_ContainerModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);

			declaration.CusContainers.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertNoWarning(declaration.JE_ContainerModeInfo, JobDeclarationValidation.NoContainerEnteredWhenContainerModeIsSelected);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ValidationModes = ValidationModes.CargoRelease;
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declaration.JE_GS_NKCusAgent = "";
			AssertNoErrors("Validation should not occur when not in ENS", declaration.JE_GS_NKCusAgentInfo);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_GS_NKCusAgent = "";
			AssertNoErrors("When declarant is not defaulted to broker, this field can be blank as declarant defaults to current user", declaration.JE_GS_NKCusAgentInfo);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declaration.JE_GS_NKCusAgent = "";
			AssertHasError("When registry defaults declarant to be broker, no broker entered should error", declaration.JE_GS_NKCusAgentInfo, JobDeclarationValidation.BrokerRequired);
			declaration.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(declaration.JE_GS_NKCusAgentInfo);

			declaration.JE_GS_NKCusAgent = "XCV";
			AssertHasErrors("List Validation", declaration.JE_GS_NKCusAgentInfo);
			AssertNoErrorContaining("Changed Message Not Required", declaration.JE_GS_NKCusAgentInfo, JobDeclarationValidation.BrokerRequired);
		}

		public virtual void TestJE_VesselName()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			Assert("Precondition: IsSea", declaration.IsSea);

			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TestTestTest";
			declaration.JE_VesselName = "TestTestTest";
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			Assert("Precondition: not IsSea", !declaration.IsSea);
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageError(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			Assert("Precondition: IsSea", declaration.IsSea);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.JE_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, ValidationConstants.Declaration.VesselRequired);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("Vessel Name length - 20", JobDeclaration.Schema.VesselNameLength, declaration.Validation.VesselNameLength);

			var errorText = string.Format(ValidationConstants.Declaration.VesselNameLength, 20);
			AssertNoWarningContaining(declaration.JE_VesselNameInfo, errorText);

			declaration.JE_VesselName = "WELLTHEYCALLHIMYOSEMITESAM";
			AssertHasWarningContaining(declaration.JE_VesselNameInfo, errorText);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.Validation.ValidateJE_VesselName();
			Assert("Should not be any notifications for Rail Transport Mode", !declaration.JE_VesselNameInfo.HasNotifications());
		}

		public void TestVoyageFlightNo()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, declaration.IsSea);
			AssertEquals("IsAir", false, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_VoyageFlightNo = "V234";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);
			AssertEquals("IsAir", true, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertHasMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_VoyageFlightNo = "V234";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);
			AssertEquals("IsAir", false, declaration.IsAir);
			declaration.JE_VoyageFlightNo = "";
			AssertNoMessageError(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestUS_SchDArrivalForSea()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, declaration.IsSea);

			declaration.US_SchDArrival = "";
			AssertHasMessageErrorContaining(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);

			declaration.US_SchDArrival = "2809";
			AssertNoMessageError(declaration.JE_RL_NKPortOfArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("IsSea", false, declaration.IsSea);

			declaration.US_SchDArrival = "2809";
			AssertNoMessageError(declaration.US_SchDArrivalInfo, FormalImportAddInfoJobDeclarationValidation.PortOfDischargeRequired);
		}

		// CS00078424
		public void TestContainerModeForNonContainerType()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_ContainerMode = "!Z~";
			AssertNoNotifications(declaration.JE_ContainerModeInfo);
		}

		public void TestCheckJE_HouseBill()
		{
			declaration.JE_TransportMode = ZString.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_HouseBillIssuerSCAC = "RT";
			AssertHasMessageErrorContaining(declaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_HouseBillIssuerSCAC = "!!";
			AssertHasMessageErrorContaining(declaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_HouseBill = "14856321";
			AssertNoMessageErrorContaining(declaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);

			BillValidatorTest.AssertValidate(declaration.JE_HouseBillInfo, "House Bill");
		}

		public virtual void TestCheckJE_MasterBill()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
				usCarrier.UI_AirwayBillPrefix = "AMF";
			}

			declaration.JE_MasterBillIssuerSCAC = usCarrier.UI_Code;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			var standardWarningMessage = "The MAWB should contain 11 digits.";
			var usImportAirSpecificWarning = USAirWayBillValidatorTest.MAWBLengthWarningMessageForTest;

			declaration.JE_MasterBill = "AMF12345";

			AssertNoWarning("Declaration Master Bill Number should not contains warning about invalid length (11 digits) for AIR jobs",
							declaration.JE_MasterBillInfo, standardWarningMessage);

			AssertHasWarning("Declaration Master Bill Number should contains US specific warning about invalid length (11 digits) for AIR jobs",
							declaration.JE_MasterBillInfo, usImportAirSpecificWarning);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_MasterBill();

			AssertHasWarning("Declaration Master Bill Number should contains standard warning about invalid length (11 digits) for AIR jobs",
				declaration.JE_MasterBillInfo, standardWarningMessage);

			AssertNoWarning("Declaration Master Bill Number should not contains US specific warning about invalid length (11 digits) for AIR jobs",
							declaration.JE_MasterBillInfo, usImportAirSpecificWarning);
		}

		public void TestJE_RL_Origin()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKOrigin = "USCHI";
			AssertNoWarnings(declaration.JE_RL_NKOriginInfo);

			declaration.US_SchDLoading = "4901";
			declaration.JE_RL_NKOrigin = "USLAX";
			AssertHasWarnings("'Export' from Puerto Rico - code must be Puerto Rican port", declaration.JE_RL_NKOriginInfo);

			declaration.JE_RL_NKOrigin = "PRSJU";
			AssertNoWarnings(declaration.JE_RL_NKOriginInfo);
		}

		public virtual void TestCheckJE_RL_NKPortOfArrival()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertNoWarnings(declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.US_SchDLoading = "4901";
			declaration.JE_RL_NKPortOfArrival = "AUBNE";
			AssertNoWarnings("'Export' from Puerto Rico to Foreign Port", declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "USNYC";
			AssertNoWarnings("'Export' from Puerto Rico to US Domestic Port", declaration.JE_RL_NKPortOfArrivalInfo);

			declaration.JE_RL_NKPortOfArrival = "XXZZZ";
			AssertHasWarnings("'Export' from Puerto Rico to Invalid Port", declaration.JE_RL_NKPortOfArrivalInfo);
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			AssertNoWarnings(declaration.JE_RL_NKPortOfLoadingInfo);

			declaration.US_SchDLoading = "4901";
			declaration.JE_RL_NKPortOfLoading = "USCHI";
			AssertHasWarnings("'Export' from Puerto Rico", declaration.JE_RL_NKPortOfLoadingInfo);
		}

		public virtual void TestCheckBrokerToPayIndicator()
		{
			declaration.BrokerToPayIndicator = ZString.Empty;
			AssertNoMessageError("No message error, because Payment Type is empty", declaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);

			declaration.BrokerToPayIndicator = ZString.Empty;
			AssertHasMessageError(declaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.BrokerToPayIndicator = ZString.Empty;
			AssertNoMessageError("No message error, because Payment Type is empty", reconDeclaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);

			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			reconDeclaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(reconDeclaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);

			reconDeclaration.BrokerToPayIndicator = ZString.Empty;
			AssertHasMessageError(reconDeclaration.BrokerToPayIndicatorInfo, FormalImportJobDeclarationValidation.BrokerToPayRequired);
		}

		public void TestCheckJE_OH_NotifyParty()
		{
			var formalDec = Factory.New<JobDeclaration>();
			formalDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			formalDec.US_EnableENS = true;

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			foreach (var dec in new[] { formalDec, drawback })
			{
				var notifyParty = Factory.New<OrgHeader>();
				dec.JE_OH_NotifyParty = notifyParty.PK;
				AssertHasMessageError(dec.JE_OH_NotifyPartyInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Notify Party"));
				notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "EIN", Core.Constants.CountryCodes.UnitedStates);
				dec.JE_OH_NotifyParty = ZGuid.Empty;
				dec.JE_OH_NotifyParty = notifyParty.PK;
				AssertNoMessageError(dec.JE_OH_NotifyPartyInfo, string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Notify Party"));
			}
		}

		public void TestCheckIOROrgPK()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.IOROrgPK = ZGuid.Invalid;
			dec.Validation.ValidateAll();
			AssertHasErrors("Enter a valid Organization.", dec.IOROrgPKInfo);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			dec.IOROrgPK = org.PK;
			dec.Validation.ValidateAll();
			AssertNoErrors("Enter a valid Organization.", dec.IOROrgPKInfo);
		}

		#region Implementation

		new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
		}

		#endregion

		void AssertErrorsForFreeTradeZoneAfterChangingJE_MessageType(bool setUserIsController, bool messageErrorOverride, bool shouldBeError)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(setUserIsController))
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, declaration.GetDefaultDataGroupingCode(), ZDateTime.Today, messageErrorOverride))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
					AssertEquals("No expected Shipment Type notifications", true, declaration.JE_MessageTypeInfo.Notifications == null || !declaration.JE_MessageTypeInfo.Notifications.ContainsNotificationContaining(ShipmentTypeCannotBeChangedMessage));

					var addOn = Factory.NewWithValidTestData<MasterFiles.Business.CustomValues.GenAddOnColumn>();
					addOn.XA_ParentID = declaration.PK;
					addOn.XA_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
					addOn.XA_Name = "US_FTZAdmissionStatus";
					addOn.XA_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;
					addOn.XA_Data = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
					Factory.Save();

					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					if (shouldBeError)
					{
						AssertHasError("Error for MessageType change", declaration.JE_MessageTypeInfo, ShipmentTypeCannotBeChangedMessage);
					}
					else
					{
						AssertHasMessageError("Message Error for MessageType change", declaration.JE_MessageTypeInfo, ShipmentTypeCannotBeChangedMessage);
					}
				});
			}
		}

		void AssertErrorsAfterChangingJE_MessageType(bool setUserIsController, bool messageErrorOverride, bool shouldBeError)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(setUserIsController))
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, declaration.GetDefaultDataGroupingCode(), ZDateTime.Today, messageErrorOverride))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("No expected Shipment Type notifications", true, declaration.JE_MessageTypeInfo.Notifications == null || !declaration.JE_MessageTypeInfo.Notifications.ContainsNotificationContaining(ShipmentTypeCannotBeChangedMessage));

					declaration.Invoices.AddNew();
					declaration.InvoiceLines.AddNew();
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
					var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
					ensEntry.CH_Status = Common.US.ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
					Factory.Save();

					AssertEquals("PreCondition: Entry Summary has transactions with customs", true, declaration.ActiveEntryHeaders.EntrySummaryEntry.HasTransactionsWithCustoms);
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					if (shouldBeError)
					{
						AssertHasError("Error for MessageType change", declaration.JE_MessageTypeInfo, ShipmentTypeCannotBeChangedMessage);
					}
					else
					{
						AssertHasMessageError("Message Error for MessageType change", declaration.JE_MessageTypeInfo, ShipmentTypeCannotBeChangedMessage);
					}
				});
			}
		}

		const string ShipmentTypeCannotBeChangedMessage = "Shipment Type cannot be changed because customs transactions exist.";
	}
}
