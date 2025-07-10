using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationValidationBaseOnlyTest : BaseJobDeclarationValidationTest<BaseJobDeclaration>
	{
		public void TestCheckJE_AddInfoIsWesternEuropean()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declarationAU = Factory.New<BaseJobDeclaration>();
				declarationAU.JE_AddInfo = "String=123456*NString=一二三四五六";
				AssertHasErrorContaining(declarationAU.JE_AddInfoInfo, EnglishCharactersValidation.GetNotificationMessage(declarationAU.JE_AddInfoInfo));
			}

			var declaration = Factory.New<BaseJobDeclaration>();
			var addInfo = (declaration as IAddInfoManager)?.AddInfo as BaseAddInfo;
			if (addInfo != null && addInfo.ZPropertyInfoHash.Count > 0)
			{
				foreach (var properyInfos in addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => declaration.FindPropertyInfo(x.Name) != null).Batch(20))
				{
					bool hasNAddInfo = false;
					foreach (var propertyInfo in properyInfos)
					{
						if (propertyInfo is ZPropertyInfoString)
						{
							propertyInfo.Value = new ZString("你");
							if (propertyInfo.IsNAddInfoField())
							{
								hasNAddInfo = true;
							}
						}
					}
					declaration.JE_AddInfo = string.Join("*", properyInfos.Select(x => x.Name.Remove(0, 3) + "=" + x.Value).ToArray());
					if (hasNAddInfo)
					{
						AssertNoErrorContaining(declaration.JE_AddInfoInfo, EnglishCharactersValidation.GetNotificationMessage(declaration.JE_AddInfoInfo));
					}
					else
					{
						AssertNoErrorContaining(declaration.JE_AddInfoInfo, EnglishCharactersValidation.GetNotificationMessage(declaration.JE_AddInfoInfo));
					}
				}
			}
			Assert(true);
		}
	}

	public abstract class BaseJobDeclarationValidationTest<TJobDeclaration> : BusinessObjectValidationTestCase
		where TJobDeclaration : BaseJobDeclaration
	{
		public virtual void TestCheckJE_OH_ImporterUsingCustomsRule()
		{
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
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateOfFirstArrival = today;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.IsBrokerToPay);
			AssertEquals(false, declaration.IsImporterToPay);
			AssertEquals(0m, declaration.DisbursementAmount);
			AssertEquals(0m, declaration.TotalDutyAmount);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = declaration.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			declaration.CustomsEntryInstructions?.Load();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.GetFormalEntries().FirstOrDefault();
			var entryLine = entry.MergedLines.FirstOrDefault();
			entryLine.CL_CustomsValue = 10001m;
			entry.ResetIsCustomsValueCalculated();
			var validation = declaration.Validation;
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Value (10001.00) exceeds maximum amount defined by Custom Rules.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			customsRule.CPH_PermitDescription = "TEST Rule";
			validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, "Total Customs Value (10001.00) exceeds maximum amount defined by Custom Rule: TEST Rule.");
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);

			entryLine.CL_CustomsValue = 10000m;
			entry.ResetIsCustomsValueCalculated();
			validation.ValidateJE_OH_Importer();
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, messageError);
		}

		public void TestCheckJE_VoyageFlightNo_ForBase()
		{
			declaration.JE_TransportMode = "AIR";
			var warning = "The airline code is not recognized.";

			declaration.JE_VoyageFlightNo = "";
			AssertNoWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_VoyageFlightNo = "B";
			AssertHasWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_VoyageFlightNo = "XX";
			AssertHasWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_VoyageFlightNo = "XX123";
			AssertHasWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_VoyageFlightNo = "BR";
			AssertNoWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_VoyageFlightNo = "BR123";
			AssertNoWarning(declaration.JE_VoyageFlightNoInfo, warning);

			declaration.JE_TransportMode = "SEA";
			declaration.JE_VoyageFlightNo = "B";
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoWarning(declaration.JE_VoyageFlightNoInfo, warning);
		}

		public void TestCheckCurrentCompanyJE_AddInfoIsWesternEuropean()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var addInfoInfo = declaration.JE_AddInfoInfo;
			declaration.JE_AddInfo = "String=123456*NString=一二三四五六";
			if (declaration is INAddInfoSupporter)
			{
				AssertNoErrors(addInfoInfo);
				declaration.JE_AddInfo = "String=123456";
				AssertNoErrors(addInfoInfo);
			}
			else
			{
				AssertHasErrorContaining(addInfoInfo, EnglishCharactersValidation.GetNotificationMessage(addInfoInfo));
				declaration.JE_AddInfo = "String=123456";
				AssertNoErrorContaining(addInfoInfo, EnglishCharactersValidation.GetNotificationMessage(addInfoInfo));
			}
		}

		public void TestJE_OH_SupplierValidationWithMiscOrg()
		{
			declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			AssertEquals("Declaration.JE_OH_SupplierInfo.GetErrors().ContainsNotificationContaining(JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration)", false, declaration.JE_OH_SupplierInfo.GetErrors().ContainsNotificationContaining(BaseJobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration));

			declaration.JE_OH_Supplier = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			if (declaration.JE_SupplierMiscFields.IsEmpty)
			{
				AssertHasErrorContaining(declaration.JE_OH_SupplierInfo, BaseJobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			}
		}

		public virtual void TestJE_OH_ImporterValidationWithMiscOrg()
		{
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			AssertEquals("Declaration.JE_OH_ImporterInfo.GetErrors().ContainsNotificationContaining(JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration)", false, declaration.JE_OH_ImporterInfo.GetErrors().ContainsNotificationContaining(BaseJobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration));

			declaration.JE_OH_Importer = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			if (declaration.JE_ImporterMiscFields.IsEmpty)
			{
				AssertHasErrorContaining(declaration.JE_OH_ImporterInfo, BaseJobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration);
			}
		}

		public void TestJE_OH_SupplierValidationWithNoMiscOrg()
		{
			SystemDefinedOrganisation.ClearOrgPKCacheForTest();
			var miscOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MISC");
			miscOrg.Delete();
			Factory.Save();
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals("Declaration.JE_OH_SupplierInfo.GetErrors().ContainsNotificationContaining(JobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration)", false, declaration.JE_OH_SupplierInfo.GetErrors().ContainsNotificationContaining(BaseJobDeclarationValidation.ErrorCannotUseMiscOrgOnThisTypeOfDeclaration));
			SystemDefinedOrganisation.ClearOrgPKCacheForTest();
		}

		public virtual void TestJE_OH_SupplierValidationWithOrgOnCreditHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;

			Factory.Save();

			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = org.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.IsExport());
			AssertHasWarningContaining(declaration.JE_OH_SupplierInfo, "TestOrg is on Credit Hold.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, declaration.IsExport());
			AssertNoWarningContaining(declaration.JE_OH_SupplierInfo, "TestOrg is on Credit Hold.");
		}

		public virtual void TestJE_OH_ImporterValidationWithOrgOnCreditHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;

			Factory.Save();

			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.IsImport());
			AssertHasWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(false, declaration.IsImport());
			AssertNoWarningContaining(declaration.JE_OH_ImporterInfo, "TestOrg is on Credit Hold.");
		}

		public void TestJE_OH_SupplierValidationNoException_WhenOrgDeleted()
		{
			var org = Factory.New<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = org.PK;
			org.Delete();
			AssertNoExceptionThrown(() => declaration.Validation.ValidateJE_OH_Supplier());
		}

		public void TestJE_OH_ImporterValidationNoException_WhenOrgDeleted()
		{
			var org = Factory.New<OrgHeader>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = org.PK;
			org.Delete();
			AssertNoExceptionThrown(() => declaration.Validation.ValidateJE_OH_Importer());
		}

		public void TestCheckJS_RX_NKInsuranceCurrency()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RX_NKInsuranceCurrency = "XXX";
			AssertHasError(declaration.JE_RX_NKInsuranceCurrencyInfo, "Enter a valid Insurance Value Currency.");

			declaration.JE_RX_NKInsuranceCurrency = "USD";
			AssertNoErrors(declaration.JE_RX_NKInsuranceCurrencyInfo);
		}

		public void TestJE_MasterBillPrefixMatchesAirline()
		{
			var warningMessage = "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_VoyageFlightNo = "";
			declaration.JE_MasterBill = "08298739457";
			AssertNoWarning(declaration.JE_MasterBillInfo, warningMessage);

			declaration.JE_VoyageFlightNo = "HW652";
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoWarning(declaration.JE_MasterBillInfo, warningMessage);

			declaration.JE_VoyageFlightNo = "QF112";
			declaration.Validation.ValidateJE_MasterBill();
			AssertHasWarning(declaration.JE_MasterBillInfo, warningMessage);

			declaration.JE_MasterBill = "08149837497";
			AssertNoWarning(declaration.JE_MasterBillInfo, warningMessage);

			declaration.JE_MasterBill = "5555555LOL5";
			AssertHasWarning(declaration.JE_MasterBillInfo, warningMessage);
		}

		public void TestCheckJE_ScreeningStatus()
		{
			OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.No);
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

			OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Exp);
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.Validation.ValidateAll();
			AssertNoMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

			declaration.JE_MessageType = DefaultExportMessageType;
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

			OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertHasMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
				declaration.JE_JS = shipment.PK;
				declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertNoMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);
				AssertHasRowMessageError(declaration, BaseJobDeclarationValidation.ComplianceStatusErrorMessage);

				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
				declaration.Validation.ValidateAll();
				AssertNoMessageError(declaration.JE_ScreeningStatusInfo, BaseJobDeclarationValidation.ScreeningErrorMessage);
				AssertNoRowMessageError(declaration, BaseJobDeclarationValidation.ComplianceStatusErrorMessage);
			}
		}

		public void TestCheckJE_ScreeningStatus_ValidStatus()
		{
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = string.Empty;
			AssertHasError(declaration.JE_ScreeningStatusInfo, "Please enter a Screening Status.");

			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(declaration.JE_ScreeningStatusInfo);

			declaration.JE_ScreeningStatus = "ZZZ";
			AssertHasError(declaration.JE_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		public void TestCheckJE_TotalVolumeUnit()
		{
			declaration.JE_TotalVolume = 20m;
			declaration.JE_TotalVolumeUnit = "FO";
			AssertHasMessageError(declaration.JE_TotalVolumeUnitInfo, "The code you have selected is not in the list.");
			declaration.JE_TotalVolumeUnit = Core.Constants.Volume.CubicMetres;
			AssertNoMessageError(declaration.JE_TotalVolumeUnitInfo, "The code you have selected is not in the list.");
		}

		public void TestSettingBranchOfOtherCompanyIsAnError()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_GB = ZGuid.Empty;
			AssertHasErrorContaining(testDec.JE_GBInfo, MandatoryValidation.MustBeEntered);

			testDec.JE_GB = branch.PK;
			AssertNoErrorContaining(testDec.JE_GBInfo, MandatoryValidation.MustBeEntered);
			AssertEquals("TestDec.JE_GB is not valid as it belongs to a different company", true, testDec.JE_GBInfo.HasErrors());
		}

		public void TestCheckJE_GC_JE_GB_BranchAndCompanyShouldMtatch()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var dec = Factory.New<BaseJobDeclaration>();
			var currentBranch = GlbBranch.CurrentBranch;
			AssertEquals("set as default", currentBranch.PK, dec.JE_GB);
			AssertEquals("set as default", currentBranch.GB_GC, dec.JE_GC);
			AssertEquals("No error", false, dec.JE_GCInfo.HasErrors());

			dec.JE_GC = company.PK;
			AssertEquals("JE_GB is set in JE_GC setter to ensure match", branch.PK, dec.JE_GB);
			AssertHasError(dec.JE_GBInfo, "You cannot transfer this job to a branch that belongs to a different company. Please log into the branch and create a job there.");
			AssertEquals("No error", false, dec.JE_GCInfo.HasErrors());

			dec.JE_GB = currentBranch.PK;
			AssertEquals("JE_GC is set in JE_GB setter to ensure match", currentBranch.GB_GC, dec.JE_GC);
			AssertEquals("No error", false, dec.JE_GBInfo.HasErrors());
			AssertEquals("No error", false, dec.JE_GCInfo.HasErrors());
		}

		public virtual void TestPackagesActualPackageCount_Validation()
		{
			var mockDeclaration = Factory.NewMoq<TJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TotalNoOfPacks = 100;
			Assert("Should Have Message Error", declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			var packGroup = declaration.PackingGroups.AddNew();
			var package = packGroup.Packages.AddNew();
			Assert("Should Have Message Error", declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			package.CW_PackQty = 100;
			Assert("Should Have No Message Error", !declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			declaration.JE_TotalNoOfPacks = 10;
			Assert("Should Have Message Error", declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			package.CW_PackQty = 10;
			Assert("Should Have No Message Error", !declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
			declaration.Packages.RemoveAndDeleteAll();
			Assert("Should Have Message Error", declaration.PackagesActualPackageCountInfo.HasMessageError(BaseJobDeclarationValidation.TotalPackageCountDoesNotMatchErrorMessage));
		}

		public void TestJE_TotalNoOfPacksActualHeaderNoOfPacks()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoWarningContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);

			declaration.JE_TotalNoOfPacks = 100;
			AssertNoWarningContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);

			declaration.Invoices.AddNew().JZ_NoOfPacks = 12.5m;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertHasWarningContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);
			declaration.Invoices.AddNew().JZ_NoOfPacks = 87.5m;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertNoWarningContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);
		}

		public void TestJE_TotalNoOfPacks_OM_IMBalanceInvoicePackage()
		{
			declaration.JE_TotalNoOfPacks = 100;
			declaration.Invoices.AddNew().JZ_NoOfPacks = 12.5m;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MiscServ = Factory.New<OrgMiscServ>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = true;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertHasMessageErrorContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);
			AssertNoWarning(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);

			declaration.Importer.MiscServ.OM_IMBalanceInvoicePackage = false;
			declaration.Validation.ValidateJE_TotalNoOfPacks();
			AssertHasWarningContaining(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);
			AssertNoMessageError(declaration.JE_TotalNoOfPacksInfo, BaseJobDeclarationValidation.DeclNoOfPacksNotEqualInvoicesNoOfPacks);
		}

		public virtual void TestJE_ShipmentIncoTerm()
		{
			Assert("Precondition: No error", !declaration.JE_ShipmentIncoTermInfo.HasErrors());
			declaration.JE_ShipmentIncoTerm = "XYZ";
			AssertHasMessageErrors(declaration.JE_ShipmentIncoTermInfo);
			declaration.JE_ShipmentIncoTerm = declaration.Lookups.IncoTermList[0].Code;
			Assert("No error after selecting valid incoterm", !declaration.JE_ShipmentIncoTermInfo.HasErrors());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ShipmentIncoTerm = "DAF";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DES";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DEQ";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DDU";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DDP";
			AssertNoMessageErrors(declaration.JE_ShipmentIncoTermInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ShipmentIncoTerm = "DAF";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DES";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DEQ";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DDU";
			AssertHasMessageError(declaration.JE_ShipmentIncoTermInfo, "The code you have selected is not in the list.");
			declaration.JE_ShipmentIncoTerm = "DDP";
			AssertNoMessageErrors(declaration.JE_ShipmentIncoTermInfo);
		}

		public void TestNegativeWeight()
		{
			declaration.JE_TotalWeight = -1m;
			AssertHasErrors(declaration.JE_TotalWeightInfo);
		}

		public void TestNegativeVolume()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalVolume = -1m;
			AssertHasErrors(declaration.JE_TotalVolumeInfo);
		}

		public void TestNegativeTotalNumberOfPieces()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalNoOfPieces = -1;
			AssertHasErrors(declaration.JE_TotalNoOfPiecesInfo);
		}

		public void TestNegativeTotalNumberOfPacks()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TotalNoOfPacks = -1;
			AssertHasErrors(declaration.JE_TotalNoOfPacksInfo);
		}

		public virtual void TestEmptyWeightUnit()
		{
			declaration.JE_TotalWeight = 10m;
			declaration.JE_TotalWeightUnit = "";
			AssertHasWarning(declaration.JE_TotalWeightUnitInfo, "Weight unit must be entered.");
		}

		public void TestInvalidWeightUnit()
		{
			declaration.JE_TotalWeight = 0m;
			declaration.JE_TotalWeightUnit = "XY";
			AssertHasMessageError(declaration.JE_TotalWeightUnitInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckJE_EstimatedDeliveryOrPickup()
		{
			ZDateTime currentTime = ZDateTime.Now;
			declaration.JE_MessageType = DefaultExportMessageType;
			declaration.JE_DateAtOrigin = currentTime;
			declaration.JE_EstimatedDeliveryOrPickup = currentTime.AddDays(1);
			AssertEquals("Expecting an error on Estimated Pickup.  Can't be after ETD of declaration", true, declaration.JE_EstimatedDeliveryOrPickupInfo.HasErrors());
			declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			AssertEquals("Not Expecting any errors. Date empty", false, declaration.JE_EstimatedDeliveryOrPickupInfo.HasErrors());
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_DateAtFinalDestination = currentTime;
			declaration.JE_EstimatedDeliveryOrPickup = currentTime.AddDays(-1);
			AssertEquals("Expecting an error. Delivery Date can not be befre ETA", true, declaration.JE_EstimatedDeliveryOrPickupInfo.HasErrors());
		}

		protected virtual string DefaultExportMessageType
		{
			get { return Customs.Business.JobMessageTypeList.Codes.Export; }
		}

		protected virtual string DefaultImportMessageType
		{
			get { return Customs.Business.JobMessageTypeList.Codes.Import; }
		}

		public void TestLandedCostingDefaultPercentages()
		{
			declaration.JE_LandedCostByWeight = 2;
			AssertEquals("Should have errors as the percentage doesn't add up", true, declaration.JE_LandedCostByWeightInfo.HasErrors());
			declaration.JE_LandedCostByVolume = 2;
			AssertEquals("Should have errors as the percentage doesn't add up", true, declaration.JE_LandedCostByVolumeInfo.HasErrors());
			declaration.JE_LandedCostByUnits = 2;
			AssertEquals("Should have errors as the percentage doesn't add up", true, declaration.JE_LandedCostByUnitsInfo.HasErrors());
			declaration.JE_LandedCostByCost = 2;
			AssertEquals("Should have errors as the percentage doesn't add up", true, declaration.JE_LandedCostByCostInfo.HasErrors());

			declaration.JE_LandedCostByCost = 94;
			AssertEquals("Percentage adds up now, should be no error", false, declaration.JE_LandedCostByWeightInfo.HasErrors());
			AssertEquals("Percentage adds up now, should be no error", false, declaration.JE_LandedCostByVolumeInfo.HasErrors());
			AssertEquals("Percentage adds up now, should be no error", false, declaration.JE_LandedCostByUnitsInfo.HasErrors());
			AssertEquals("Percentage adds up now, should be no error", false, declaration.JE_LandedCostByCostInfo.HasErrors());
		}

		public void TestServiceLevelForBrokerageValidationWhenPluggedIn()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = shipment.PK;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_RS_NKServiceLevel = "SDS";
			Assert("Precondition: Must not be stand alone", !declaration.IsStandAlone);
			Assert("Should have no error", !declaration.JE_RS_NKServiceLevelInfo.HasWarnings());
		}

		public virtual void TestCheckJE_DateOfArrival()
		{
			declaration.JE_DateOfFirstArrival = new ZDateTime(2010, 10, 10, 10, 10, 10);
			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10);
			AssertNoMessageErrors(declaration.JE_DateOfArrivalInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 10, 23, 10, 10);
			AssertHasWarning(declaration.JE_DateOfArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");

			declaration.JE_ExportDate = new ZDateTime(2010, 10, 11, 00, 00, 10);
			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 11, 23, 10, 10);
			AssertNoWarning(declaration.JE_DateOfArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
			AssertNoMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");

			declaration.JE_DateOfArrival = new ZDateTime(2010, 10, 09, 23, 10, 10);
			AssertNoWarning(declaration.JE_DateOfArrivalInfo, "Please confirm that Date of Arrival should be one day earlier than the Export Date");
			AssertHasMessageErrorContaining(declaration.JE_DateOfArrivalInfo, "Date of Arrival can not be before the Export Date");
		}

		public void TestMarksAndNumbers()
		{
			var validation = (BaseJobDeclarationValidation)declaration.Validation;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			validation.ValidateJE_MarksAndNumbersShort();
			Assert("No error on Marks and Numbers", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());

			declaration.JE_MarksAndNumbersShort = "Marks and Numbers";
			validation.ValidateJE_MarksAndNumbersShort();
			Assert("No error on Marks and Numbers", !declaration.JE_MarksAndNumbersShortInfo.HasMessageErrors());
		}

		class BaseTestJobDeclaration : BaseJobDeclaration
		{
			public BaseTestJobDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override JobDeclarationValidation GetNewValidation()
			{
				return new ATestJobDeclarationValidation(this);
			}
		}

		class ATestJobDeclarationValidation : BaseJobDeclarationValidation
		{
			public ATestJobDeclarationValidation(AutoJobDeclaration parent)
				: base(parent)
			{
			}

			public static Licences TestLicenceUsed;

			protected override LicenceCheckpoint ImportBrokerLicence
			{
				get { return TestLicenceUsed.ImportBroker; }
			}

			protected override LicenceCheckpoint ExportBrokerLicence
			{
				get { return TestLicenceUsed.ExportBroker; }
			}

			protected override LicenceCheckpoint DrawbackLicence
			{
				get { return TestLicenceUsed.Drawback; }
			}
		}

		public virtual void TestMergeByForExport()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = "ZZZ";
			AssertHasErrors(declaration.JE_MergeByInfo);

			declaration.JE_MergeBy = ZString.Empty;
			AssertHasErrors(declaration.JE_MergeByInfo);
		}

		public void TestMergeByForImport()
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = "ZZZ";
			AssertHasErrors(declaration.JE_MergeByInfo);

			declaration.JE_MessageType = "IMP";
			declaration.JE_MergeBy = ZString.Empty;
			AssertHasErrors(declaration.JE_MergeByInfo);
		}

		public void TestMessageTypeChangesWillUseImportAndExportLicences()
		{
			var newStaffA = Factory.New<IGlbStaff>();
			newStaffA.GS_Code = "INT";
			newStaffA.GS_LoginName = "Ian Test";
			newStaffA.GS_FullName = "Ian Chen Test";
			newStaffA.Factory.Save();

			var aLicences = new Licences();
			ATestJobDeclarationValidation.TestLicenceUsed = aLicences;

			try
			{
				var declaration = Factory.New<BaseTestJobDeclaration>();
				declaration.LicenceLogin += new LicenceLoginEventHandler(Validation_LicenceLogin);

				//import allowed, export not allowed, drawback not allowed
				ATestJobDeclarationValidation.TestLicenceUsed.ExportBroker.AllowUsageForTest = false;
				ATestJobDeclarationValidation.TestLicenceUsed.Drawback.AllowUsageForTest = false;

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				AssertEquals("HasErrors", false, declaration.JE_MessageTypeInfo.HasErrors());

				Env.Security.ExportEdit.IsAllowed = false;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("HasErrors", false, declaration.JE_MessageTypeInfo.HasErrors());

				Env.Security.ExportEdit.IsAllowed = true;
				declaration.Validation.ValidateJE_MessageType();
				Assert("Licence error has some text", ATestJobDeclarationValidation.TestLicenceUsed.ExportBroker.LastReasonForNotAllowing.Length > 0);
				AssertEquals("Message", ATestJobDeclarationValidation.TestLicenceUsed.ExportBroker.LastReasonForNotAllowing, declaration.JE_MessageTypeInfo.GetErrors().GetFirstMessage());

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
				AssertHasErrorContaining(declaration.JE_MessageTypeInfo, aLicences.Drawback.LastReasonForNotAllowing);
				Assert("Licence error has some text", ATestJobDeclarationValidation.TestLicenceUsed.Drawback.LastReasonForNotAllowing.Length > 0);
				AssertEquals("Message", ATestJobDeclarationValidation.TestLicenceUsed.Drawback.LastReasonForNotAllowing, declaration.JE_MessageTypeInfo.GetErrors().GetFirstMessage());

				ATestJobDeclarationValidation.TestLicenceUsed.ImportBroker.ForceLogout();
				ATestJobDeclarationValidation.TestLicenceUsed.ExportBroker.ForceLogout();

				//import not allowed, export allowed, drawback allowed
				ATestJobDeclarationValidation.TestLicenceUsed.ImportBroker.AllowUsageForTest = false;
				ATestJobDeclarationValidation.TestLicenceUsed.ExportBroker.AllowUsageForTest = true;
				ATestJobDeclarationValidation.TestLicenceUsed.Drawback.AllowUsageForTest = true;

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				AssertEquals("HasErrors", false, declaration.JE_MessageTypeInfo.HasErrors());

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
				AssertEquals("HasErrors", false, declaration.JE_MessageTypeInfo.HasErrors());

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Assert("Licence error has some text", ATestJobDeclarationValidation.TestLicenceUsed.ImportBroker.LastReasonForNotAllowing.Length > 0);
				AssertEquals("Message", ATestJobDeclarationValidation.TestLicenceUsed.ImportBroker.LastReasonForNotAllowing, declaration.JE_MessageTypeInfo.GetErrors().GetFirstMessage());
			}
			finally
			{
				ATestJobDeclarationValidation.TestLicenceUsed = null;
			}
		}

		void Validation_LicenceLogin(object sender, LicenceLoginEventArgs e)
		{
			e.LoginHasBeenAttempted = true;
			e.LicenceCheckPoint.Login(new TestLicensedComponent());
		}

		#region Implementation

		protected BaseJobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
		}

		#endregion
	}

	public class TestLicensedComponent : ILicensedComponent
	{
		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get
			{
				if (fLicensedComponentManager == null)
				{
					fLicensedComponentManager = new LicensedComponentManager(this);
				}
				return fLicensedComponentManager;
			}
		}
		LicensedComponentManager fLicensedComponentManager;
	}
}
