using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseJobDeclaration;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EUExitControl;
using Constants = Enterprise.Core.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestNumberFountainUniqueIndexFailureHandlerForNormalMessageType()
		{
			AssertNumberFountainUniqueIndexFailureHandler(JobMessageTypeList.Codes.Import, "CustomsJobNo");
		}

		public void TestNumberFountainUniqueIndexFailureHandlerForWarehousedByExternalAgent()
		{
			AssertNumberFountainUniqueIndexFailureHandler(JobMessageTypeList.Codes.WarehousedByExternalAgent, "CustomsJobNoByExternalAgent");
		}

		void AssertNumberFountainUniqueIndexFailureHandler(string messageType, string numberFountain)
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_MessageType = messageType;
			Factory.Save();

			Assert(!declaration1.JE_DeclarationReference.IsEmpty);
			var numberPart = int.Parse(declaration1.JE_DeclarationReference.Substring(1)) + 1;
			var nextDeclarationReference = declaration1.JE_DeclarationReference.Substring(0, 1) + numberPart.ToString("D8");
			declaration1.JE_DeclarationReference = nextDeclarationReference; // it make confilict with next fountain value
			Factory.Save();
			AssertEquals("It should not populate again and change the manual setting", nextDeclarationReference, declaration1.JE_DeclarationReference);

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_MessageType = messageType;
			var saveFailed = false;
			try
			{
				Factory.Save();
			}
			catch (Exception e)
			{
				saveFailed = true;
				AssertEquals("Declaration Reference should not be populated.", string.Empty, declaration2.JE_DeclarationReference);
				ZExceptionReporting.HandleSaveException(e);
				ErrorReporter.Clear();
			}
			AssertStartsWith("User should be notified about the unique index conflict.",
				@$"While you were working, the automatically assigned record number was used by another user.
Saving again should automatically resolve this issue.
Number Fountain: {numberFountain}
Index: NR_UX__JE_DeclarationReference_JE_GC
Value: Declaration", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Save should have failed", saveFailed);
			UnitTestUserNotification.Instance.ClearMessages();

			Factory.Save();
			var newDeclarationReference = declaration1.JE_DeclarationReference.Substring(0, 1) + (numberPart + 1).ToString("D8");
			AssertEquals("Next Declaration Reference should be given out.", newDeclarationReference, declaration2.JE_DeclarationReference);
			AssertEquals("There should be no error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestUniqueIndexFailureHandlers()
		{
			var declaration = (IBusinessObjectInternals)Factory.New<BaseJobDeclaration>();

			AssertEquals(1, declaration.UniqueIndexFailureHandlers.Count());
			AssertEquals("Enterprise.Customs.Business.BaseJobDeclaration+DeclarationNumberFountainUniqueIndexFailureHandler", declaration.UniqueIndexFailureHandlers.First().GetType().ToString());
		}

		public void TestCorrectLoadingOfCharges()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var nzCompany = factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			var declaration = (BaseJobDeclaration)factory.New<NZ.IJobDeclaration>();
			declaration.JE_GB = nzBranch.PK;
			var currencyCode = declaration.LocalCurrencyCode;
			var groupCharge = declaration.TopGroupInvoice.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			groupCharge.J7_Amount = 100m;
			groupCharge.J7_RX_NKCurrency = currencyCode;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = currencyCode;
			var invoice1Charge = invoice1.Charges.AddNew();
			invoice1Charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			invoice1Charge.J7_Amount = 100m;
			invoice1Charge.J7_RX_NKCurrency = currencyCode;
			var invoice1ApportionedCharge = invoice1.GroupCharges.AddNew();
			invoice1ApportionedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			invoice1ApportionedCharge.J7_Amount = 100m;
			invoice1ApportionedCharge.J7_RX_NKCurrency = currencyCode;
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_LinePrice = 50m;
			var inv1Line1Charge = invoice1Line1.Charges.AddNew();
			inv1Line1Charge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			inv1Line1Charge.J7_Amount = 100m;
			inv1Line1Charge.J7_RX_NKCurrency = currencyCode;
			var inv1Line1ApportionedCharge = invoice1Line1.ApportionedCharges.AddNew();
			inv1Line1ApportionedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			inv1Line1ApportionedCharge.J7_Amount = 100m;
			inv1Line1ApportionedCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_LinePrice = 50m;
			var inv1Line2Charge = invoice1Line2.Charges.AddNew();
			inv1Line2Charge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			inv1Line2Charge.J7_Amount = 100m;
			inv1Line2Charge.J7_RX_NKCurrency = currencyCode;
			var inv1Line2ApportionedCharge = invoice1Line2.ApportionedCharges.AddNew();
			inv1Line2ApportionedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			inv1Line2ApportionedCharge.J7_Amount = 100m;
			inv1Line2ApportionedCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_InvoiceAmount = 100m;
			invoice2.JZ_RX_NKInvoice_Currency = currencyCode;
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line.JI_LinePrice = 100m;
			var invoice2Charge = invoice2.Charges.AddNew();
			invoice2Charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			invoice2Charge.J7_Amount = 100m;
			invoice2Charge.J7_RX_NKCurrency = currencyCode;
			var invoice2ApportionedCharge = invoice2.GroupCharges.AddNew();
			invoice2ApportionedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			invoice2ApportionedCharge.J7_Amount = 100m;
			invoice2ApportionedCharge.J7_RX_NKCurrency = currencyCode;
			var inv2LineCharge = invoice2Line.Charges.AddNew();
			inv2LineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			inv2LineCharge.J7_Amount = 100m;
			inv2LineCharge.J7_RX_NKCurrency = currencyCode;
			var inv2LineApportionedCharge = invoice2Line.ApportionedCharges.AddNew();
			inv2LineApportionedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			inv2LineApportionedCharge.J7_Amount = 100m;
			inv2LineApportionedCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			declaration.ApportionmentDirty = false;
			factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var factory2 = new BusinessObjectFactory();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
				{
					var dec = factory2.Load<BaseJobDeclaration>(declaration.PK);
					dec.JE_ExportDate = ZDateTime.Today.AddDays(-1);
				}
				AssertNoExceptionThrown(factory2.Save);
			}
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				var declaration = (BaseJobDeclaration)Factory.New<NZ.IJobDeclaration>();
				declaration.JE_GB = ZGuid.Empty;
				AssertEquals("Branch is null", "ER", (declaration as ITypeDeciderContext).Country);

				declaration.JE_GB = nzBranch.PK;
				AssertEquals("Branch isn't null", "NZ", (declaration as ITypeDeciderContext).Country);
			});
		}

		public void TestSourceIdentifier()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();

			Assert(declaration is ISourceIdentifierProvider);
			AssertEquals(declaration.PK, (declaration as ISourceIdentifierProvider).SourceIdentifier);

			declaration.JE_JS = shipment.PK;

			AssertEquals(shipment.PK, (declaration as ISourceIdentifierProvider).SourceIdentifier);
		}

		public void TestBrokerageCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.FrenchGuyana))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				AssertEquals("FR", dec.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				AssertEquals("FR", dec.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				AssertEquals("DE", dec.BrokerageCountryCode);
			}
		}

		public void TestWorkflowInformationProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var koreaCompany = Factory.New<GlbCompany>();
			koreaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			koreaCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var koreaBranch = koreaCompany.Branches.AddNew();
			koreaBranch.GB_RL_NKHomePort = "NZAKL";

			declaration.JE_GB = koreaBranch.PK;

			declaration.JE_RL_NKOrigin = "DEFRA";
			declaration.JE_RL_NKFinalDestination = "AUSYD";

			var info = ((IWorkflowProvider)declaration).GetWorkflowInformationProvider();
			AssertEquals(koreaCompany.PK, info.Companies.First());
			AssertEquals("Frankfurt am Main", info.Origin);
			AssertEquals("Sydney", info.Destination);
			AssertEquals(TrackingConstants.BusinessContext.Declaration, info.BusinessContext);
		}

		public void TestISupportsPostingOverseasAgentCharge()
		{
			var declaration = BaseJobDeclaration.New(new BusinessObjectFactory());
			AssertNotNull(@"BaseJobDeclaration is made to implement the empty interface ISupportsPostingOverseasAgentCharge so that action trigger POA can be made
					applicable specifically to this type"
				, declaration);
		}

		public void TestWipingJeOhPartiesWipesCorrespondingJobDocAddress()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeMainAddress = consignee.Addresses.MainAddress;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorMainAddress = consignor.Addresses.MainAddress;
			declaration.JE_OH_Importer = consignee.PK;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			AssertEquals(consigneeMainAddress.PK, importerDocumentaryAddress.E2_OA_Address);
			declaration.JE_OH_Supplier = consignor.PK;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			AssertEquals(consignorMainAddress.PK, supplierDocumentaryAddress.E2_OA_Address);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(false, importerDocumentaryAddress.IsDeleted);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(false, supplierDocumentaryAddress.IsDeleted);
			declaration.JE_JS = ZGuid.NewZGuid();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(true, importerDocumentaryAddress.IsDeleted);
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(true, supplierDocumentaryAddress.IsDeleted);
		}

		public void TestOrganisationsForCreditChecks()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true));
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.CompanyData.OB_IsDebtor = true;
			var localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.CompanyData.OB_IsDebtor = true;
			var debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			debtor1.CompanyData.OB_IsDebtor = true;
			var debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			debtor2.CompanyData.OB_IsDebtor = true;

			var shipment = Factory.New<CommonShipment>();
			var job1 = new JobHeader.Loader(shipment).TryCreate();

			var charge1 = Factory.New<JobCharge>();
			charge1.JR_JH = job1.PK;
			charge1.FillWithValidTestData();

			var declaration = BaseJobDeclaration.New(Factory);
			var job2 = new JobHeader.Loader(declaration).TryCreate();
			declaration.JE_OverrideFreightDefaults = true;

			var charge2 = Factory.New<JobCharge>();
			charge2.JR_JH = job2.PK;
			charge2.FillWithValidTestData();

			Factory.Save();

			declaration.JE_JS = shipment.PK;

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(1, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals(1, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			job1.LocalChargesPK = localClient.PK;
			AssertEquals(1, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			job1.LocalChargesPK = ZGuid.Empty;
			AssertEquals(2, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			job1.LocalChargesPK = localClient.PK;
			charge1.JR_OH_SellAccount = debtor1.PK;
			job2.LocalChargesPK = localClient2.PK;
			charge2.JR_OH_SellAccount = debtor2.PK;

			setupOrganizationsEvaluatedForCreditControlRegistry("", declaration, Factory);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(5, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(localClient));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(debtor1));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			declaration.JE_JS = ZGuid.Empty;
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(3, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(debtor2));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			declaration.JE_JS = shipment.PK;
			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.LocalClient, declaration, Factory);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(debtor1));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.Consignee, declaration, Factory);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(localClient));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(debtor1));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.Consignor, declaration, Factory);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(localClient));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(debtor1));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);

			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.AllDebtors, declaration, Factory);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(3, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Supplier));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.Importer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(localClient));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestOrganisationsForCreditChecks_ControllingCustomersAndControllingAgents()
		{
			var shipment = Factory.New<CommonShipment>();
			var job1 = new JobHeader.Loader(shipment).TryCreate();

			var charge1 = Factory.New<JobCharge>();
			charge1.JR_JH = job1.PK;
			charge1.FillWithValidTestData();

			var declaration = BaseJobDeclaration.New(Factory);
			var job2 = new JobHeader.Loader(declaration).TryCreate();
			declaration.JE_OverrideFreightDefaults = true;

			var charge2 = Factory.New<JobCharge>();
			charge2.JR_JH = job2.PK;
			charge2.FillWithValidTestData();

			Factory.Save();

			declaration.JE_JS = shipment.PK;
			var controllingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var controllingCustomer = Factory.LoadTop1<OrgHeader>(new ZQuery().AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, controllingAgent.PK));
			declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;
			declaration.JE_OH_ControllingAgent = controllingAgent.PK;

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(0, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);

			AccountingMasterFilesRegistry.Instance.EnableCreditControlEvaluationForControllingCustomersAndControllingAgents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(2, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingCustomer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingAgent));

			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.ControllingCustomer, declaration, Factory);
			AssertEquals(1, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(!((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingCustomer));
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingAgent));

			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.ControllingAgent, declaration, Factory);
			AssertEquals(1, ((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingCustomer));
			Assert(!((ICreditControlledDocumentDelivery)declaration).OrganisationsForCreditChecks.Contains(declaration.ControllingAgent));
		}

		readonly Action<string, BaseJobDeclaration, BusinessObjectFactory> setupOrganizationsEvaluatedForCreditControlRegistry = (organizationType, declaration, factory) =>
		{
			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
			if (!string.IsNullOrEmpty(organizationType))
			{
				var orgCreditControl = orgCreditControlCollection.AddNew();
				orgCreditControl.JobType = declaration.InvoicingSupporter.ConsumerType.Code;
				orgCreditControl.INCOTerm = AccountingMasterFilesConstants.INCOTermCodes.All;
				orgCreditControl.FreightPaymentTerm = AccountingMasterFilesConstants.FreightPaymentTermCodes.All;
				orgCreditControl.OrganizationType = organizationType;
			}
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
			factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
		};

		public void TestAutoAssignImporterRef_WhenMiscServ_IsDeleted()
		{
			var declaration = BaseJobDeclaration.New(new BusinessObjectFactory());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer1";

			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;

			AssertNotNull("Importer should't be null", declaration.Importer);

			AssertNoExceptionThrown(delegate
			{ var tmp = declaration.AutoAssignImporterRef; });
			AssertNoExceptionThrown(delegate
			{ var tmp = declaration.EstimateDutyAndTaxOnWHEntries; });

			var importer1 = declaration.Importer;
			importer1.MiscServ.Delete();

			AssertNoExceptionThrown(delegate
			{ var tmp = declaration.AutoAssignImporterRef; });
			AssertNoExceptionThrown(delegate
			{ var tmp = declaration.EstimateDutyAndTaxOnWHEntries; });
		}

		public void TestHasMessageInitiator()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			AssertEquals("HasMessageInitiator", false, dec.HasMessageInitiator);

			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("HasMessageInitiator", true, dec.HasMessageInitiator);
		}

		public void TestNoteContextsForRelatedNotes()
		{
			BaseJobDeclarationForTest declaration = Factory.New<BaseJobDeclarationForTest>();
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			Assert("is AIR", (declaration.NoteContextsForRelatedNotes.FreightMode & StmNoteContextFreightMode.I) != 0);
			Assert("Always Declaration", (declaration.NoteContextsForRelatedNotes.Module & StmNoteContextModule.D) != 0);
			Assert("Always 'Shipment and Declaration'", (declaration.NoteContextsForRelatedNotes.Module & StmNoteContextModule.E) != 0);
			Assert("Not yet Shipment", (declaration.NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) == 0);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("is Import", (declaration.NoteContextsForRelatedNotes.Direction & StmNoteContextDirection.I) != 0);
			Assert("not Export", (declaration.NoteContextsForRelatedNotes.Direction & StmNoteContextDirection.E) == 0);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			Assert("not Import", (declaration.NoteContextsForRelatedNotes.Direction & StmNoteContextDirection.I) == 0);
			Assert("is Export", (declaration.NoteContextsForRelatedNotes.Direction & StmNoteContextDirection.E) != 0);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Assert("Should be Shipment now", (declaration.NoteContextsForRelatedNotes.Module & StmNoteContextModule.F) != 0);
		}

		//Issue:00019038
		public void TestDeveloperExceptionOnApportionment()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = dec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, dec.LocalCurrencyCode);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration decLoaded = factory2.Load<BaseJobDeclaration>(dec.PK);
			AssertNotNull("PreCondition:DecLoaded should not be null", decLoaded);

			ErrorReporter.Clear();
			decLoaded.ApportionmentDirty = true;//simulate charges are not loaded, but apportionment gets dirty
			decLoaded.ResumeApportionment();
			AssertEquals("No developer exception expected", "", ErrorReporter.LastKeyReported);
		}

		public void TestDocumentNote()
		{
			NUnit.Framework.TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();

			StmSystemDefinedFieldCollection collection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField field1 = collection.AddNew();
			field1.S1_Name = "Field1";
			field1.S1_BusinessContext = nameof(CargoWise.Definitions.BusinessContext.Customs);

			StmSystemDefinedField field2 = collection.AddNew();
			field2.S1_Name = "Field2";
			field2.S1_BusinessContext = nameof(CargoWise.Definitions.BusinessContext.Customs);

			StmSystemDefinedField field3 = collection.AddNew();
			field3.S1_Name = "Field3";
			field3.S1_BusinessContext = nameof(CargoWise.Definitions.BusinessContext.Consol);

			Factory.Save();

			dec.DocNote.SetSystemDefinedFieldValue("Field1", "ValueField1");
			dec.DocNote.SetSystemDefinedFieldValue("Field2", "ValueField2");
			dec.DocNote.SetSystemDefinedFieldValue("Field3", "ValueField3");
			AssertEquals("Field1", "ValueField1", dec.DocNote.GetSystemDefinedFieldValue("Field1"));
			AssertEquals("Field1", "ValueField2", dec.DocNote.GetSystemDefinedFieldValue("Field2"));
			AssertEquals("Field1", null, dec.DocNote.GetSystemDefinedFieldValue("Field3"));
		}

		public void TestOnApportioningIsCalledBeforeApportionmentIsDone()
		{
			TestDeclaration testDec = Factory.New<TestDeclaration>();
			AssertEquals("OnApportioning not called yet", false, testDec.OnApportioningCalled);

			testDec.ResumeApportionment();
			AssertEquals("OnApportioning should have called now", true, testDec.OnApportioningCalled);
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool OnApportioningCalled;
			protected override void OnApportioning()
			{
				OnApportioningCalled = true;
				base.OnApportioning();
			}
		}

		public void TestSBREventLog_ShouldCreateWhenParentTypeIsDeclaration()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = ZGuid.Empty;
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_MasterBill = "08187443521";

				var transport = declaration.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "A1234A";
				transport.JW_RL_NKLoadPort = "AUMEL";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2015, 01, 01);
				transport.JW_ETA = ZDateTime.Now.AddDays(1);

				Factory.Save();

				var logs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Precondition: Should not create SBR event", 0, logs.Count);

				declaration.JE_MasterBill = "46197135463";
				Factory.Save();

				logs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should have created a new SBR event", 1, logs.Count);
			}
		}

		public void TestSBREventLog_DoesNotCreateWhenRegistryDisabled()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = ZGuid.Empty;
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_MasterBill = "08187443521";

				var transport = declaration.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "A1234A";
				transport.JW_RL_NKLoadPort = "AUMEL";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2015, 01, 01);
				transport.JW_ETA = new ZDateTime(2015, 01, 01);

				Factory.Save();

				var logs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should not create SBR event", 0, logs.Count);

				declaration.JE_MasterBill = "46197135463";
				Factory.Save();

				logs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertEquals("Should not create SBR event as registry is disabled", 0, logs.Count);
			}
		}

		public void TestSBREventLog_NoLinkedTransport()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = ZGuid.Empty;
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_MasterBill = "08187443521";

				var transport = declaration.Transports.AddNew();
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "A1234A";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2015, 01, 01);
				transport.JW_ETA = ZDateTime.Now.AddDays(1);

				Factory.Save();

				var logs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertFlightTrackingEvent("New unlinked transport: Create new SBR event", declaration, 1, "|RFN=08187443521|TYP=AWB Automation");

				declaration.JE_MasterBill = "46197135463";
				Factory.Save();
				AssertFlightTrackingEvent("MAWB changed: Create new SBR event", declaration, 2, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_VoyageFlight = "A1234B";
				Factory.Save();
				AssertFlightTrackingEvent("Flight NO. changed: Create new SBR event", declaration, 3, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_RL_NKLoadPort = "AUMEL";
				Factory.Save();
				AssertFlightTrackingEvent("Load Port changed: Create new SBR event", declaration, 4, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_RL_NKDiscPort = "NZAKL";
				Factory.Save();
				AssertFlightTrackingEvent("Discharge Port changed: Create new SBR event", declaration, 5, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_ETD = new ZDateTime(2017, 01, 03);
				Factory.Save();
				AssertFlightTrackingEvent("ETD changed: Create new SBR event", declaration, 6, "|RFN=46197135463|TYP=AWB Automation");

				transport.JW_ETA = ZDateTime.Now.AddDays(-1);
				Factory.Save();
				AssertFlightTrackingEvent("ETA changed: Create new SBR event", declaration, 7, "|RFN=46197135463|TYP=AWB Automation");
			}
		}

		public void TestSBREventLog_NonAirTransport()
		{
			using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = ZGuid.Empty;
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_MasterBill = "08187443521";

				var transport = declaration.Transports.AddNew();
				transport.JW_IsLinked = false;
				transport.JW_VoyageFlight = "A1234A";
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_ETD = new ZDateTime(2015, 01, 01);
				transport.JW_ETA = new ZDateTime(2015, 01, 01);

				Factory.Save();

				var logs = transport.Logs.GetAllLogs().Cast<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).ToList();
				AssertFlightTrackingEvent("No events: not AIR transport", declaration, 0, "|FDT=2015-01-01|RFN=08187443521|TYP=AWB Automation|VFL=A1234A");

				declaration.JE_MasterBill = "46197135463";
				Factory.Save();
				AssertFlightTrackingEvent("No events: not AIR transport", declaration, 0, "|FDT=2015-01-01|RFN=46197135463|TYP=AWB Automation|VFL=A1234A");
			}
		}

		void AssertFlightTrackingEvent(ZString message, BaseJobDeclaration declaration, int expectedLogCount, ZString expectedEventReference, bool expectedIsCancelled = false)
		{
			var logs = declaration.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code).OrderByDescending(x => x.SL_EventTime).ToList();

			AssertNotNull(logs);
			AssertEquals(expectedLogCount, logs.Count);

			if (expectedLogCount > 0)
			{
				AssertEquals("Event Reference", expectedEventReference, logs[0].SL_Reference);
				AssertEquals("Event IsCancelled", expectedIsCancelled, logs[0].SL_IsCancelled);
			}

			for (int i = 1; i < logs.Count; i++)
			{
				var log = logs[i];
				AssertEquals("Event should be cancelled", true, log.SL_IsCancelled);
			}
		}

		public void TestUpdateJZ_InvoiceCurrExRateWhenApportionmentIsDone()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, testDec.LocalCurrencyCode));
			var rateOn1Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn1Jan.RE_ExRateType = "CUS";
			rateOn1Jan.RE_StartDate = new ZDateTime(1999, 1, 1);
			rateOn1Jan.RE_ExpiryDate = new ZDateTime(1999, 1, 1);
			rateOn1Jan.RE_SellRate = 0.7m;

			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = new ZDateTime(1999, 1, 2);
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			AssertEquals("Exchange rate defaulted", 0.7m, invoice.JZ_InvoiceCurrExRate);

			var charge = invoice.Charges.AddNew();
			charge.J7_RX_NKCurrency = foreignCurrency.RX_Code;
			AssertEquals("Exchange rate defaulted", 0.7m, charge.J7_ExchangeRate);

			var rateOn2Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn2Jan.RE_ExRateType = "CUS";
			rateOn2Jan.RE_StartDate = new ZDateTime(1999, 1, 2);
			rateOn2Jan.RE_ExpiryDate = new ZDateTime(1999, 1, 2);
			rateOn2Jan.RE_SellRate = 0.8m;

			AssertEquals("Exchange rates are imported and the latest ex-rate is now one for Jan/2", 0.8m, invoice.EffectiveExchangeRateForInvoiceCurr);
			AssertEquals("JZ_InvoiceCurrExRate is still 0.7m", 0.7m, invoice.JZ_InvoiceCurrExRate);

			testDec.ResumeApportionment();
			AssertEquals("PreCondition:JZ_InvoiceCurrExRate is not user-enterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertEquals("JZ_InvoiceCurrExRate should have been updated", 0.8m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals("Exchange rate updated", 0.8m, charge.J7_ExchangeRate);
		}

		public void TestJE_AutoWeightApportion()
		{
			CustomsDataRegistry.Instance.EnableAutoApportionWeight.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.JE_AutoWeightApportion);
			declaration.JE_AutoWeightApportion = false;
			AssertEquals(false, declaration.JE_AutoWeightApportion);

			CustomsDataRegistry.Instance.EnableAutoApportionWeight.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.JE_AutoWeightApportion);
			declaration.JE_AutoWeightApportion = true;
			AssertEquals(true, declaration.JE_AutoWeightApportion);
		}

		public void TestShouldRunWeightApportionment()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.IsImportingData = true;
			Assert(!declaration.ShouldRunWeightApportionment);
			declaration.IsImportingData = false;
			Assert(declaration.ShouldRunWeightApportionment);
		}

		public void TestWeightApportionment()
		{
			RefCurrency usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			usd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.5m);

			RefCurrency nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.NewZealand);
			nzd.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8m);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_AutoWeightApportion = true;
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;

			BaseJobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = usd.RX_Code;
			invoice1.JZ_InvoiceAmount = 1500m;
			AssertEquals("Invoice1 Weight", 1000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);

			BaseJobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = nzd.RX_Code;
			AssertEquals("Invoice1 Weight", 1000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", ZDecimal.Zero, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Kilograms, invoice2.JZ_WeightUQ);

			invoice2.JZ_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Invoice1 Weight", 1000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", ZDecimal.Zero, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			invoice2.JZ_InvoiceAmount = 1600m;
			AssertEquals("Invoice1 Weight", 600m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 0.4m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals("Invoice1 Weight", 600000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Kilograms, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			invoice1.JZ_WeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Invoice1 Weight", 600000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Tonnes, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			declaration.JE_TotalWeight = ZDecimal.Zero;
			AssertEquals("Invoice1 Weight", 600000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Tonnes, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			declaration.JE_TotalWeightUnit = ZString.Empty;
			declaration.JE_TotalWeight = 1000m;
			AssertEquals("Invoice1 Weight", 600000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Tonnes, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			declaration.JE_AutoWeightApportion = false;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			AssertEquals("Invoice1 Weight", 600000m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Tonnes, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);

			declaration.JE_AutoWeightApportion = true;
			AssertEquals("Invoice1 Weight", 600m, invoice1.JZ_Weight);
			AssertEquals("Invoice1 WeightUQ", Core.Constants.Weight.Tonnes, invoice1.JZ_WeightUQ);
			AssertEquals("Invoice2 Weight", 400m, invoice2.JZ_Weight);
			AssertEquals("Invoice2 WeightUQ", Core.Constants.Weight.Tonnes, invoice2.JZ_WeightUQ);
		}

		public void TestIApportionInvoiceHolder()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			AssertEquals("There should be two invoices", 2, testDec.Invoices.Count);
			AssertEquals("ChargeHolders", 4, ((IApportionInvoiceHolder)testDec).ChargeHolders.Length);

			ArrayList chargeHolders = new ArrayList(((IApportionInvoiceHolder)testDec).ChargeHolders);
			AssertEquals("Invoic1 is there", true, chargeHolders.Contains(invoice1));
			AssertEquals("Invoic2 is there", true, chargeHolders.Contains(invoice2));
			AssertEquals("TopGroup is there", true, chargeHolders.Contains(topGroup));
			AssertEquals("SubGroup is there", true, chargeHolders.Contains(subGroup));

			ArrayList invoices = new ArrayList(((IApportionInvoiceHolder)testDec).Invoices);
			AssertEquals("There should be two invoices", 2, invoices.Count);
			AssertEquals("There should be Inovice1", true, invoices.Contains(invoice1));
			AssertEquals("There should be Inovice2", true, invoices.Contains(invoice2));
		}

		public void TestJE_RS_NKServiceLevelNotDefaultedToJE_RS_NKServiceLevelOfImporterWhenImporterSetAndJE_RS_NKServiceLevelIsEmpty()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = ZString.Empty;

			BaseJobDeclaration jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_RS_NKServiceLevel = "BLH";
			jobDeclaration.JE_OH_Importer = organisation.PK;
			AssertEquals("BLH", jobDeclaration.JE_RS_NKServiceLevel);
		}

		public void TestJE_RS_NKServiceLevelChangedWhenImporterIsChanged()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = "D2D";

			var defaultServiceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR");
			Env.Registry.ServiceLevel = defaultServiceLevel.PK.ToGuid();

			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("DIR", declaration.JE_RS_NKServiceLevel);

			declaration.JE_OH_Importer = organisation.PK;
			AssertEquals("D2D", declaration.JE_RS_NKServiceLevel);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_RS_NKServiceLevel = "PER";

			declaration.JE_OH_Importer = organisation.PK;
			AssertEquals("PER", declaration.JE_RS_NKServiceLevel);
		}

		public void TestJE_RS_NKServiceLevelDefaultedToJE_RS_NKServiceLevelOfImporterWhenImporterSet()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = "D2D";

			BaseJobDeclaration jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_OH_Importer = organisation.PK;
			AssertEquals("D2D", jobDeclaration.JE_RS_NKServiceLevel);
		}

		public void TestDoNotDefaultServiceLevelFromOrganisationWhenShipmentIsThere()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.MiscServ.OM_RS_NKIMDefaultServiceLevel = "D2D";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RS_NKServiceLevel = "STD";

			BaseJobDeclaration jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = shipment.PK;
			new JobDeclarationSynchroniser(jobDeclaration).Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("STD", shipment.JS_RS_NKServiceLevel);

			jobDeclaration.JE_OH_Importer = organisation.PK;
			AssertEquals("Should not default from importer when plugged into shipment. Service level on brokerage disappears if plugged into shipment", "STD", jobDeclaration.JE_RS_NKServiceLevel);
		}

		public void TestBillSynchronisationWillReuseBillObject()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_MasterBillNum = "MB234223";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HB324325";

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
				var primaryMasterBill = declaration.PrimaryMasterBill;
				var primaryHouseBill = declaration.PrimaryHouseBill;
				AssertCollectionContains(primaryMasterBill, declaration.Bills);
				AssertCollectionContains(primaryHouseBill, declaration.Bills);

				declaration.JE_OverrideFreightDefaults = ZBool.True;
				AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
				AssertCollectionContains(primaryMasterBill, declaration.Bills);
				AssertCollectionContains(primaryHouseBill, declaration.Bills);

				declaration.JE_OverrideFreightDefaults = ZBool.False;
				AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
				AssertCollectionContains(primaryMasterBill, declaration.Bills);
				AssertCollectionContains(primaryHouseBill, declaration.Bills);

				declaration.ShipmentSynchroniser.Synchronise(true);
				AssertEquals("declaration.Bills.Count", 2, declaration.Bills.Count);
				AssertCollectionContains(primaryMasterBill, declaration.Bills);
				AssertCollectionContains(primaryHouseBill, declaration.Bills);
			}
		}

		public void TestICurrencyConverterDataProvider()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);

			ICurrencyConverterDataProvider decAsProvider = testDec;
			AssertEquals("DateOfValuation", testDec.JE_ExportDate, decAsProvider.DateOfValuation);
			AssertEquals("Rate Type", ZArchitecture.Core.ExchangeRateType.Customs, decAsProvider.RateType);
			AssertEquals("Fall back days", BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack, decAsProvider.MaximumDaysToFallback);
		}

		public void TestDateOfValuation()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_ValuationDate = new ZDate(2021, 1, 1);
			CombineAssertions(() =>
			{
				AssertEquals("DateOfValuation from JE_ValuationDate", testDec.JE_ValuationDate, testDec.DateOfValuation.Date);
				testDec.JE_ValuationDate = ZDate.Empty;
				testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDec.JE_ExportDate = new ZDateTime(2021, 1, 2);
				AssertEquals("DateOfValuation from JE_ExportDate", testDec.JE_ExportDate, testDec.DateOfValuation);

				testDec.JE_ExportDate = ZDate.Empty;
				AssertEquals("DateOfValuation from CachedTodaysDate when JE_ExportDate empty", testDec.CachedTodaysDate, testDec.DateOfValuation);
				testDec.JE_ExportDate = testDec.CachedTodaysDate.AddDays(2);
				testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("DateOfValuation from CachedTodaysDate when export and JE_ExportDate > CachedTodaysDate", testDec.CachedTodaysDate, testDec.DateOfValuation);
			});
		}

		public void TestMarkApportionmentDirtyWhenExportDateChanged()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

			testDec.JE_ExportDate = new ZDateTime(2005, 1, 1);
			AssertEquals("not marked as there are no invoices yet", false, testDec.ApportionmentDirty);

			testDec.Invoices.AddNew();
			testDec.JE_ExportDate = new ZDateTime(2005, 1, 2);
			AssertEquals("not marked as there are no invoices yet", true, testDec.ApportionmentDirty);
		}

		public void TestHaveAmendmentsBeenMadeAndNotYetClearedByCustoms()
		{
			var mock1 = Factory.NewMoq<CusEntryHeader>();
			var entryHeader1 = mock1.Object;
			var mock2 = Factory.NewMoq<CusEntryHeader>();
			var entryHeader2 = mock2.Object;

			var declaration = BaseJobDeclaration.New(Factory);
			Assert("No entry headers", !declaration.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);

			declaration.CustomsEntryHeaders.AddRange(entryHeader1, entryHeader2);
			Assert("Entry headers - no amendments", !declaration.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);

			mock2.Setup(m => m.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms).Returns(true);
			Assert("Entry headers - one has amendments", declaration.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms);
		}

		public void TestTopLevelObjectForJobToReference()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			AssertEquals("Declaration", declaration, ((ICustomsJobInfo)declaration).TopLevelObjectForJobToReference);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals("Shipment", shipment, ((ICustomsJobInfo)declaration).TopLevelObjectForJobToReference);
		}

		public void TestJobDocsAndCartageIsRegisteredEditableChild()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			Assert(declaration.IsRegisteredEditableChildObject(declaration.DocsAndCartage));
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationInSecondFactory = factory2.Load<BaseJobDeclaration>(declaration.PK);
			Assert(declarationInSecondFactory.IsRegisteredEditableChildObject(declarationInSecondFactory.DocsAndCartage));
		}

		public void TestShouldLogEventIfJE_EntryStatusChanged()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = mockDec.Object;
			Factory.Save();
			declaration.Reload();

			declaration.JE_EntryStatus = "34";
			Factory.Save();
			declaration.Reload();
			AssertEquals("Event Created", 1, declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Parent == declaration.PK).Count());

			mockDec.Setup(m => m.ShouldLogEventIfJE_EntryStatusChanged).Returns(false);
			declaration.JE_EntryStatus = "56";
			Factory.Save();
			declaration.Reload();
			AssertEquals("No more Event Created", 1, declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Parent == declaration.PK).Count());
		}

		public void TestLogEventIfJE_EntryStatusChanged()
		{
			var cesEvent = Events.CustomsEntryStatus;
			var cesEventCode = cesEvent.Code;
			var queryForAssertion = new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code);
			CombineAssertions("StandAlone Declaration", () =>
			{
				BaseJobDeclaration declaration1 = BaseJobDeclaration.New(Factory);
				declaration1.Logs.AddNew(cesEvent, "Test1");
				Factory.Save();

				var declarationPK = declaration1.PK;
				var reloadedDeclaration = (new BusinessObjectFactory()).Load<BaseJobDeclaration>(declarationPK);
				var reloadedLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_1.Count", 1, reloadedLogs.Length);
				AssertEquals("Test_1.Description", "Test1", reloadedLogs[0].SL_Reference);

				declaration1.JE_EntryStatus = "XX";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_2.Count", 2, reloadedLogs.Length);
				AssertEquals("Test_2.Description", "XX", reloadedLogs[1].SL_Reference);

				declaration1.JE_EntryStatus = "YY";
				declaration1.JE_EntryStatus = "XX";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_3.Count", 2, reloadedLogs.Length);
				AssertEquals("Test_3.Description", "XX", reloadedLogs[1].SL_Reference);

				declaration1.Logs.AddNew(cesEvent, "YY");
				declaration1.JE_EntryStatus = "YY";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_4.Count", 3, reloadedLogs.Length);
				AssertEquals("Test_4.Description", "YY", reloadedLogs[2].SL_Reference);

				declaration1.Logs.AddNew(cesEvent, "Test2");
				declaration1.JE_EntryStatus = "ZZ";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_5.Count", 5, reloadedLogs.Length);
				AssertEquals("Test_5.Description_1", "Test2", reloadedLogs[3].SL_Reference);
				AssertEquals("Test_5.Description_2", "ZZ", reloadedLogs[4].SL_Reference);
			});
			CombineAssertions("Declaration With Shipment", () =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_JS = shipment.PK;
				shipment.Logs.AddNew(cesEvent, "Test1");
				Factory.Save();

				var shipmentPK = shipment.PK;
				var declarationPK = declaration.PK;
				var newFactory = new BusinessObjectFactory();
				var reloadedDeclaration = newFactory.Load<BaseJobDeclaration>(declarationPK);
				var reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipmentPK);
				var reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_1.Shipment.Count", 1, reloadedShipmentLogs.Length);
				AssertEquals("Test_1.Shipment.Description", "Test1", reloadedShipmentLogs[0].SL_Reference);
				AssertEquals("Test_1.Count", 0, reloadedDeclarationLogs.Length);

				declaration.JE_EntryStatus = "XX";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				reloadedShipment.Reload();
				reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_2.Shipment.Count", 1, reloadedShipmentLogs.Length);
				AssertEquals("Test_2.Shipment.Description", "Test1", reloadedShipmentLogs[0].SL_Reference);
				AssertEquals("Test_2.Count", 1, reloadedDeclarationLogs.Length);
				AssertEquals("Test_2.Description", "XX", reloadedDeclarationLogs[0].SL_Reference);

				declaration.JE_EntryStatus = "XX";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				reloadedShipment.Reload();
				reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_3.Shipment.Count", 1, reloadedShipmentLogs.Length);
				AssertEquals("Test_3.Shipment.Description", "Test1", reloadedShipmentLogs[0].SL_Reference);
				AssertEquals("Test_3.Count", 1, reloadedDeclarationLogs.Length);
				AssertEquals("Test_3.Description", "XX", reloadedDeclarationLogs[0].SL_Reference);

				declaration.Logs.AddNew(cesEvent, "YY");
				declaration.JE_EntryStatus = "YY";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				reloadedShipment.Reload();
				reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_4.Shipment.Count", 1, reloadedShipmentLogs.Length);
				AssertEquals("Test_4.Shipment.Description", "Test1", reloadedShipmentLogs[0].SL_Reference);
				AssertEquals("Test_4.Count", 2, reloadedDeclarationLogs.Length);
				AssertEquals("Test_4.Description", "YY", reloadedDeclarationLogs[1].SL_Reference);

				shipment.Logs.AddNew(cesEvent, "ZZ");
				declaration.JE_EntryStatus = "ZZ";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				reloadedShipment.Reload();
				reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_5.Shipment.Count", 2, reloadedShipmentLogs.Length);
				AssertEquals("Test_5.Shipment.Description", "ZZ", reloadedShipmentLogs[1].SL_Reference);
				AssertEquals("Test_5.Count", 2, reloadedDeclarationLogs.Length);
				AssertEquals("Test_5.Description", "YY", reloadedDeclarationLogs[1].SL_Reference);

				shipment.Logs.AddNew(cesEvent, "Test2");
				declaration.JE_EntryStatus = "OO";
				Factory.Save();
				reloadedDeclaration.Reload();
				reloadedDeclarationLogs = reloadedDeclaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				reloadedShipment.Reload();
				reloadedShipmentLogs = reloadedShipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, cesEvent.Code));
				AssertEquals("Test_6.Shipment.Count", 3, reloadedShipmentLogs.Length);
				AssertEquals("Test_6.Shipment.Description", "Test2", reloadedShipmentLogs[2].SL_Reference);
				AssertEquals("Test_6.Count", 3, reloadedDeclarationLogs.Length);
				AssertEquals("Test_6.Description", "OO", reloadedDeclarationLogs[2].SL_Reference);
			});
		}

		public void TestLogEventIfJE_EntryStatusChanged_CombinedEntryStatus()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			mockDec.Setup(m => m.ShouldLogEventIfJE_EntryStatusChanged).Returns(true);
			var declaration = mockDec.Object;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryStatus = "XX";
			entry2.CH_EntryStatus = "YY";

			Factory.Save();
			declaration.Reload();
			AssertEquals("no Event Created after create and save", 0, declaration.Logs.Find(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Parent == declaration.PK).Count());
		}

		public void TestLogCSHForHVLVStandAloneDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.LogCSHForHVLVStandAloneDeclaration();
			Assert("Should not create CSH log for regular declaration", !declaration.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode));

			declaration.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>("TYP", "HVL"));
			declaration.LogCSHForHVLVStandAloneDeclaration();
			Assert("Should create CSH log for HVLV standalone declaration", declaration.Logs.HasLogWith(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode));

			declaration.LogCSHForHVLVStandAloneDeclaration();
			AssertEquals("Should not create CSH log if another CSH log has been added but not saved", 1, declaration.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.ClearanceStatusChangedCode).Count());
		}

		[ExpectNoExceptions()]
		public void TestUpdateJobDeclarationReferenceOnWarehouseSide()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.JE_DeclarationReference = "DECREF1";
			declaration.UpdateJobDeclarationReferenceOnWarehouseSide();
		}

		public void TestJobDocsAndCartageIsRightType()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Assert("Declaration.DocsAndCartage is Forwarding.Business.ForwardingDocsAndCartage", declaration.DocsAndCartage is ForwardingDocsAndCartage);
		}

		public void TestIsWarehousedByExternalAgent()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert(dec.IsWarehousedByExternalAgent);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!dec.IsWarehousedByExternalAgent);
		}

		public void TestIsImportIncludesWarehousedByExternalAgent()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert(dec.IsImport);
		}

		public void TestIsImportIncludesImportByExternalBroker()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			Assert(dec.IsImport);
			Assert(dec.IsImportByExternalBroker);
			Assert(dec.IsDeclarationByExternalBroker);
			Assert(!dec.IsExport);
			Assert(!dec.IsExportByExternalBroker);
		}

		public void TestIsExportIncludesExportByExternalBroker()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			Assert(dec.IsExport);
			Assert(dec.IsExportByExternalBroker);
			Assert(dec.IsDeclarationByExternalBroker);
			Assert(!dec.IsImport);
			Assert(!dec.IsImportByExternalBroker);
		}

		public void TestSupportsSingleEntryBondedWarehousing()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.SupportMultipleWarehouseEntryCore).Returns(false);
			var declaration = declarationMock.Object;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			AssertEquals("SupportsSingleEntryBondedWarehousing", true, declaration.SupportsSingleEntryBondedWarehousing);
			declaration.SetSupportsBondedWarehousingForTesting(false);
			AssertEquals("SupportsSingleEntryBondedWarehousing", false, declaration.SupportsSingleEntryBondedWarehousing);
			declaration.SetSupportsBondedWarehousingForTesting(true);
			AssertEquals("SupportsSingleEntryBondedWarehousing", true, declaration.SupportsSingleEntryBondedWarehousing);
			declarationMock.Reset();
			declarationMock.Setup(m => m.SupportMultipleWarehouseEntryCore).Returns(true);
			AssertEquals("SupportsSingleEntryBondedWarehousing", false, declaration.SupportsSingleEntryBondedWarehousing);
		}

		public void TestHasManualWarehouseUpdate()
		{
			var dec = BaseJobDeclaration.New(Factory);
			var supporter = (IWarehouseIntegrationSupporter)dec;
			Assert("PreCondition: SingleWarehouseEntry", !supporter.HasManualWhsUpdate);
			Assert("PreCondition: SingleWarehouseEntry", !dec.SupportMultipleWarehouseEntry);

			var entry = dec.ActiveEntryHeaders.AddNew();
			entry.CH_HasManualWhsUpdate = true;
			Assert("set declaration HasManualWhsUpdate from SingleWarehouseEntry", supporter.HasManualWhsUpdate);

			supporter.HasManualWhsUpdate = false;
			Assert("set SingleWarehouseEntry from declartion", !entry.CH_HasManualWhsUpdate);

			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Setup(m => m.SupportMultipleWarehouseEntryCore).Returns(true);
			var dec2 = declarationMock.Object;
			var supporter2 = (IWarehouseIntegrationSupporter)dec2;
			Assert("PreCondition: MultipleWarehouseEntry", !supporter2.HasManualWhsUpdate);
			Assert("PreCondition: MultipleWarehouseEntry", dec2.SupportMultipleWarehouseEntry);

			var entry2 = dec2.ActiveEntryHeaders.AddNew();
			entry2.CH_HasManualWhsUpdate = true;
			Assert("declartion HasManualWhsUpdate return false if not SingleWarehouseEntry", !supporter2.HasManualWhsUpdate);

			supporter2.HasManualWhsUpdate = true;
			AssertEquals("setting exceptiion for SupportMultipleWarehouseEntry", "HasManualWhsUpdate should not be set when SupportMultipleWarehouseEntry is true.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestHasLineGoingIntoABondedWarehouse()
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			BaseJobComInvoiceLine line = dec.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(dec);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("HasLineGoingIntoABondedWarehouse", ZBool.True, dec.HasLineGoingIntoABondedWarehouse);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals("HasLineGoingIntoABondedWarehouse", ZBool.False, dec.HasLineGoingIntoABondedWarehouse);
		}

		public void TestHasLineGoingIntoAnAutomatedBondedWarehouse()
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.SupportsBondedWarehousingCoreExposed = true;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			BaseJobComInvoiceLine line = dec.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(dec);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("HasLineGoingIntoAnAutomatedBondedWarehouse", ZBool.True, dec.HasLineGoingIntoAnAutomatedBondedWarehouse);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals("HasLineGoingIntoAnAutomatedBondedWarehouse", ZBool.False, dec.HasLineGoingIntoAnAutomatedBondedWarehouse);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			dec.SupportsBondedWarehousingCoreExposed = false;
			AssertEquals("HasLineGoingIntoAnAutomatedBondedWarehouse", ZBool.False, dec.HasLineGoingIntoAnAutomatedBondedWarehouse);

			dec.SupportsBondedWarehousingCoreExposed = true;
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			AssertEquals("HasLineGoingIntoAnAutomatedBondedWarehouse", ZBool.True, dec.HasLineGoingIntoAnAutomatedBondedWarehouse);
		}

		public void TestHasLineGoingIntoBondAndLineNotGoingIntoBond()
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.SupportsBondedWarehousingCoreExposed = true;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			BaseJobComInvoiceLine line = dec.FilteredInvoiceLines.AddNew();
			line.SetDeclarationForTesting(dec);
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			BaseJobComInvoiceLine line2 = dec.FilteredInvoiceLines.AddNew();
			line2.SetDeclarationForTesting(dec);
			line2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", true, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			dec.SupportsBondedWarehousingCoreExposed = false;
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", true, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("TestHasLineGoingIntoBondAndLineNotGoingIntoBond", false, dec.HasLineGoingIntoBondAndLineNotGoingIntoBond);
		}

		public void TestSupportsBondedWarehousing()
		{
			var declarationMock = Factory.NewMoq<DummyDeclarationWithIntegrationSupport>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			var dec = declarationMock.Object;
			dec.JE_SystemCreateTimeUtc = ZDateTime.Today;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.SupportMultipleWarehouseEntryCoreExposed = false;
			var entry = dec.CustomsEntryHeaders.AddNew();

			dec.SupportsBondedWarehousingCoreExposed = false;
			AssertEquals("SupportsBondedWarehousing", false, dec.SupportsBondedWarehousing);

			dec.SupportsBondedWarehousingCoreExposed = true;
			AssertEquals("SupportsBondedWarehousing", true, dec.SupportsBondedWarehousing);

			dec.JE_OH_Importer = Guid.Empty;
			AssertEquals("SupportsBondedWarehousing", false, dec.SupportsBondedWarehousing);

			var org = OrgHeader.New(Factory);
			dec.JE_OH_Importer = org.PK;
			AssertEquals("SupportsBondedWarehousing", false, dec.SupportsBondedWarehousing);

			org.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("SupportsBondedWarehousing", true, dec.SupportsBondedWarehousing);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("SupportsBondedWarehousing", false, dec.SupportsBondedWarehousing);

			dec.WarehouseDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("SupportsBondedWarehousing", true, dec.SupportsBondedWarehousing);

			org.CompanyData.OB_IMUsedBondedWhs = false;
			AssertEquals("SupportsBondedWarehousing", false, dec.SupportsBondedWarehousing);

			dec.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			AssertEquals("SupportsBondedWarehousing", true, dec.SupportsBondedWarehousing);
		}

		public void TestIsBondedWarehousingDisabled()
		{
			var declarationMock = Factory.NewMoq<DummyDeclarationWithIntegrationSupport>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(true);
			var dec = declarationMock.Object;
			dec.SupportMultipleWarehouseEntryCoreExposed = false;
			dec.CustomsEntryHeaders.AddNew();
			AssertEquals("IsBondedWarehousingDisabled", false, dec.IsBondedWarehousingDisabled);
			dec.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			AssertEquals("IsBondedWarehousingDisabled", true, dec.IsBondedWarehousingDisabled);
		}

		public void TestSupportsBondedWarehousingCoreDefaultValue()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			AssertEquals("By default, BondedWarehouse support is turned off, it is an opt in.", false, dec.SupportsBondedWarehousingCore);
		}

		public void TestSupportsBondedWarehousingCore()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			var registryItem = DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory;

			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Registry setting enabled.", true, dec.SupportsBondedWarehousingCore);

			registryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Registry setting disabled.", false, dec.SupportsBondedWarehousingCore);
		}

		public void TestNotifyInvoiceLinkIfChangedToFromExWarehousingDoesNotCreateLink()
		{
			BaseJobDeclarationForTest declaration = BaseJobDeclarationForTest.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertNull("Link should not be created", declaration.fWarehouseInvoiceLink);
		}

		public void TestNotifyInvoiceLinkIfChangedToFromExWarehousing()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			((IInvoiceLinkProvider)declaration).Link.IsExWarehouseChanged += new IsExWarehouseChangedEvent(Link_OnEnabledChanged);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			notifyCount = 0;

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("NotifyCount", 1, notifyCount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("NotifyCount", 1, notifyCount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("NotifyCount", 2, notifyCount);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("NotifyCount", 2, notifyCount);
		}

		public void TestNoteContextsForRelatedNotesLookAtSDSNotes()
		{
			BaseJobDeclarationForTest declaration = BaseJobDeclarationForTest.New(Factory);
			AssertEquals("StmNoteContextModule.D a context", true, (declaration.NoteContextsForRelatedNotes.Module & StmNoteContextModule.D) > 0);
		}

		public void TestLoadFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Declaration", declaration, BaseJobDeclaration.Load(shipment));
		}

		public void TestLoadedFromJobDeclarationWithoutShipment_WhenLoadingJobHeader()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			var jobHeaderparent = declaration.Job.Parent;
			AssertNotNull("Job header parent should not be null", jobHeaderparent);
		}

		public void TestAuditSecurity()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			AssertEquals("AuditSecurity", Env.Security.CustomsDeclarationAudit, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.AuditSecurity);
		}

		public void TestOverriddenDepartmentPK()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			AssertEquals("Base Overridden Dept. PK is empty guid", ZGuid.Empty, ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.OverriddenDepartmentPK);
		}

		public void TestTransportMode()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Base TransportMode is JE_TransportMode", "AIR", ((IJobInvoicingPlugIn)declaration).InvoicingSupporter.TransportMode);
		}

		int notifyCount;

		void Link_OnEnabledChanged()
		{
			notifyCount++;
		}

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = BaseJobDeclaration.New(Factory);
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestShouldOverrideNotes()
		{
			var declaration = Factory.New<BaseJobDeclarationForTest>();
			declaration.SetShouldOverrideNotes(false);
			AssertEquals("shouldOverrideNotes flag", false, declaration.shouldOverrideNotes);
			declaration.SetShouldOverrideNotes(true);
			AssertEquals("shouldOverrideNotes flag", true, declaration.shouldOverrideNotes);
		}

		public void TestMarkContainersAsIsForInvoiceLine()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.CusContainers.AddNew();
			BaseJobComInvoiceHeader header = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine line = header.JobComInvoiceLines.AddNew();

			declaration.JE_ContainerMode = line.JI_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			line.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			Assert("ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog", declaration.ShouldShowInvoiceLinesAreNotLinkedToTheContainerDialog);

			line.ContainersForInvoiceLinesForBindingOnly.RemoveAll();
			AssertNoExceptionThrown(() => declaration.MarkContainersAsIsForInvoiceLine());
		}

		public void TestJobDirection()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Job direction = import", Directions.Import, dec.JobDirection);
			dec.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Job direction = import for MSC", Directions.Import, dec.JobDirection);
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Job direction = export", Directions.Export, dec.JobDirection);
		}

		public void TestCreateStmProcessQueueProcessor_ParentIsCusExitReport()
		{
			var exitReport = Factory.New<ICusExitReport>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var result = ((IBaseAutoSendingMessageSupporter)declaration).CreateStmProcessQueueProcessor((BusinessObject)exitReport, "TRA");
			CombineAssertions(() =>
			{
				AssertType<CustomsStmProcessQueueCreatorProcessor>("Type", result);

				var customsStmProcessQueueCreatorProcessor = result as CustomsStmProcessQueueCreatorProcessor;
				AssertEquals("Parent", customsStmProcessQueueCreatorProcessor.BizObj, exitReport);
				AssertEquals("ApplicationCode", customsStmProcessQueueCreatorProcessor.ApplicationCode, "ASC");
				AssertEquals("TriggerActionCode", customsStmProcessQueueCreatorProcessor.TriggerActionCode, "TRA");
			});
		}

		#region JE_MessageType

		BaseJobDeclaration GetTestDeclarationForMessageTypeChangeTest(bool isMessageTypeChangeAnError)
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			if (!isMessageTypeChangeAnError)
			{
				return declaration;
			}

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var message = declaration.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			Factory.Save();
			return declaration;
		}

		GlbStaff GetTestUserAccount(string code, bool isSystemAccount)
		{
			var testUser = Factory.New<GlbStaff>();
			testUser.GS_Code = code;
			testUser.GS_LoginName = code;
			testUser.GS_IsSystemAccount = isSystemAccount;
			testUser.GS_EmailAddress = $"{code}@example.com";
			Factory.Save();

			return testUser;
		}

		public void TestShouldLogEventJE_MessageTypeChanged()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			var declaration = declarationMock.Object;
			CombineAssertions(() =>
			{
				Assert("[PreCondition]: ShouldLogEventJE_MessageTypeChanged is false", !declaration.ShouldLogEventJE_MessageTypeChanged);

				declarationMock.Setup(d => d.IsMessageTypeChangeAnError).Returns(true);
				Assert("[Only IsMessageTypeChangeAnError]: ShouldLogEventJE_MessageTypeChanged should be false", !declaration.ShouldLogEventJE_MessageTypeChanged);
				declarationMock.Setup(d => d.IsMessageTypeChangeAnError).Returns(false);

				declarationMock.Setup(d => d.IsInDatabase).Returns(true);
				Assert("[Only IsInDatabase]: ShouldLogEventJE_MessageTypeChanged should be false", !declaration.ShouldLogEventJE_MessageTypeChanged);
				declarationMock.Setup(d => d.IsInDatabase).Returns(false);

				using (new User.IsBatchProcessorOverride(EnvProxy.Instance.CurrentUser))
				{
					Assert("[Only IsBatchProcessor]: ShouldLogEventJE_MessageTypeChanged should be false", !declaration.ShouldLogEventJE_MessageTypeChanged);

					declarationMock.Setup(d => d.IsInDatabase).Returns(true);
					declarationMock.Setup(d => d.IsMessageTypeChangeAnError).Returns(true);
					Assert("[IsInDatabase & IsBatchProcessor & IsMessageTypeChangeAnError]: ShouldLogEventJE_MessageTypeChanged should be true", declaration.ShouldLogEventJE_MessageTypeChanged);
				}
			});
		}

		public void TestShouldLogEvent_WhenJE_MessageTypeChanged()
		{
			var declaration = GetTestDeclarationForMessageTypeChangeTest(true);

			Assert("[PreCondition]: IsMessageTypeChangeAnError is true", declaration.IsMessageTypeChangeAnError);
			AssertEquals("[PreCondition]: No log", false, declaration.GetMessageTypeChangeLogs().Any());
			AssertEquals("[PreCondition]: No event report", string.Empty, ErrorReporter.LastMessageReported);

			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK,
					   Env.CurrentDepartment.PK))
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			}

			var logExists = declaration.GetMessageTypeChangeLogs().Any(l =>
				(ZString)l.OldValue == SharedJobMessageTypeList.Codes.Import
				&& (ZString)l.NewValue == SharedJobMessageTypeList.Codes.Export
				&& l.StackTrace != null);
			Assert("Log should exist before save", logExists);
			AssertEquals("Error report should **NOT** exist before save", string.Empty, ErrorReporter.LastMessageReported);

			Factory.Save();

			AssertEquals("Log should **NOT** exist after save", false, declaration.GetMessageTypeChangeLogs().Any());
			Assert("Error report should exist after save", ErrorReporter.LastMessageReported.StartsWith($"{nameof(declaration.JE_MessageType)} was changed from"));

			ErrorReporter.Clear();
		}

		public void TestShouldNotLogEvent_WhenJE_MessageTypeChangedAndNotIsMessageTypeChangeAnError()
		{
			var declaration = GetTestDeclarationForMessageTypeChangeTest(false);

			Assert("[PreCondition]: IsMessageTypeChangeAnError is false", !declaration.IsMessageTypeChangeAnError);
			AssertEquals("[PreCondition]: No log", false, declaration.GetMessageTypeChangeLogs().Any());
			AssertEquals("[PreCondition]: No event report", string.Empty, ErrorReporter.LastMessageReported);

			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK,
					   Env.CurrentDepartment.PK))
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			}

			AssertEquals("Log should **NOT** exist", false, declaration.GetMessageTypeChangeLogs().Any());

			Factory.Save();

			AssertEquals("Error report should **NOT** exist after save", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestShouldNotLogEvent_WhenJE_MessageTypeChangedByANonSystemUser()
		{
			var declaration = GetTestDeclarationForMessageTypeChangeTest(true);

			Assert("[PreCondition]: IsMessageTypeChangeAnError is true", declaration.IsMessageTypeChangeAnError);
			AssertEquals("[PreCondition]: No log", false, declaration.GetMessageTypeChangeLogs().Any());
			AssertEquals("[PreCondition]: No event report", string.Empty, ErrorReporter.LastMessageReported);

			var normalUser = GetTestUserAccount("NUA", isSystemAccount: false);

			using (EnvProxy.Instance.SetTemporaryUserContext(normalUser.GS_LoginName, Env.CurrentBranch.PK,
					   Env.CurrentDepartment.PK))
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			}

			AssertEquals("Log should **NOT** exist", false, declaration.GetMessageTypeChangeLogs().Any());

			Factory.Save();

			AssertEquals("Error report should **NOT** exist after save", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestShouldLogEvent_WhenJE_MessageTypeChangedAndReverted()
		{
			var declaration = GetTestDeclarationForMessageTypeChangeTest(true);

			Assert("[PreCondition]: IsMessageTypeChangeAnError is true", declaration.IsMessageTypeChangeAnError);
			AssertEquals("[PreCondition]: No log", false, declaration.GetMessageTypeChangeLogs().Any());
			AssertEquals("[PreCondition]: No event report", string.Empty, ErrorReporter.LastMessageReported);

			using (EnvProxy.Instance.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK,
					   Env.CurrentDepartment.PK))
			{
				Assert("[PreCondition]: ShouldLogEventJE_MessageTypeChanged is true", declaration.ShouldLogEventJE_MessageTypeChanged);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;

				var logExists = declaration.GetMessageTypeChangeLogs().Any(l =>
					(ZString)l.OldValue == SharedJobMessageTypeList.Codes.Import
					&& (ZString)l.NewValue == SharedJobMessageTypeList.Codes.Export
					&& l.StackTrace.Contains("set_JE_MessageType"));
				Assert("Log should exist before save", logExists);
				AssertEquals("Error report should **NOT** exist before save", string.Empty, ErrorReporter.LastMessageReported);

				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

				logExists = declaration.GetMessageTypeChangeLogs().Any(l =>
					(ZString)l.OldValue == SharedJobMessageTypeList.Codes.Import
					&& (ZString)l.NewValue == SharedJobMessageTypeList.Codes.Export
					&& l.StackTrace.Contains("set_JE_MessageType"));
				Assert("Log should exist before save after revert", logExists);
				AssertEquals("Error report should **NOT** exist before save", string.Empty, ErrorReporter.LastMessageReported);

				Factory.Save();
				AssertEquals("Error report should exist after save", string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		public void TestLogATCEvent_AttachedNewCommercialInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP01";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INVOICE01";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			Factory.Save();

			var expectedReference = "|REF=INVOICE01:SUP01|TYP=Commercial Invoice";
			var eventLogEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.ToString()));
			AssertEquals("ATC Event reference should contain invoice number and supplier org code and type", expectedReference, eventLogEntries.Single().SL_Reference);
		}

		public void TestNoLogATCEvent_ModifiedExistingCommercialInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUP02";
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			Factory.Save();

			var eventATCCountBeforeSave = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.ToString())).Length;
			AssertEquals(1, eventATCCountBeforeSave);

			invoiceHeader.JZ_Weight = 100;
			declaration.JE_GoodsDescription = "a";
			Factory.Save();

			var eventATCCountAfterSave = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Attached.ToString())).Length;
			AssertEquals("ATC event should not be logged for modifying the existing commercial invoice", 1, eventATCCountAfterSave);
		}

		#region IBaseJobDeclaration

		public void TestIBaseJobDeclaration_InvoiceLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var iFace = declaration as IBaseJobDeclaration;
			AssertSame(declaration.InvoiceLines, iFace.InvoiceLines);
		}

		public void TestIBaseJobDeclaration_IsInvoiceLinesLoaded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			Assert(((IBaseJobDeclaration)declaration).IsInvoiceLinesLoaded);

			declaration.ReleaseInvoiceLines();
			Assert(!((IBaseJobDeclaration)declaration).IsInvoiceLinesLoaded);
		}

		#endregion

		public void TestIsAllocatedQuantityRequiredForBondedWarehouse()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("IsAllocatedQuantityRequiredForBondedWarehouse should be false by default for all countries", false, declaration.IsAllocatedQuantityRequiredForBondedWarehouse);
		}

		#region Compliance

		public void TestBaseJobDeclaration_ComplianceRiskStatusObject()
		{
			TestComplianceRisk("OVR", "PSK", "CLR", "CLR");
			TestComplianceRisk("OVR", "CLR", "CLR", "PSK");
			TestComplianceRisk("PSK", "PSK", "PSK", "CLR");
			TestComplianceRisk("CLR", "CLR", "CLR", "CLR");

			void TestComplianceRisk(ZString overallRisk, ZString commodityRisk, ZString partyRisk, ZString locationRisk)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var declaration = Factory.New<BaseJobDeclaration>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = declaration.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = declaration.PK;

				instance[ComplianceRiskStatusSchema.COR_CommodityRisk] = commodityRisk;
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = partyRisk;
				instance[ComplianceRiskStatusSchema.COR_LocationRisk] = locationRisk;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRisk;
				
				AssertEquals(overallRisk, declaration.ComplianceRiskStatus.JobRisk);
				AssertEquals(partyRisk, declaration.ComplianceRiskStatus.PartyRisk);
				AssertEquals(commodityRisk, declaration.ComplianceRiskStatus.CommodityRisk);
				AssertEquals(locationRisk, declaration.ComplianceRiskStatus.LocationRisk);
			}
		}

		public void TestOverallComplianceRisk()
		{
			TestOverallRisk("Override Clear", "OVR");
			TestOverallRisk("Potential Risk", "PSK");
			TestOverallRisk("Clear", "CLR");
			TestOverallRisk("Held", "HLD");
			TestOverallRisk("Blocked", "BLK");

			void TestOverallRisk(ZString expectedOverallComplianceRisk, string overallRiskForSetup)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var declaration = Factory.New<BaseJobDeclaration>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = declaration.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = declaration.PK;
				instance[ComplianceRiskStatusSchema.COR_OverallRisk] = overallRiskForSetup;

				AssertEquals(expectedOverallComplianceRisk, declaration.OverallComplianceRisk);
			}
		}

		public void TestPartyComplianceRisk()
		{
			TestPartyRisk("Potential Risk", "PSK");
			TestPartyRisk("Clear", "CLR");
			TestPartyRisk("High Risk", "HSK");
			TestPartyRisk("Blocked", "BLK");

			void TestPartyRisk(ZString expectedPartyComplianceRisk, string partyRiskForSetup)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var declaration = Factory.New<BaseJobDeclaration>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = declaration.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = declaration.PK;
				instance[ComplianceRiskStatusSchema.COR_PartyRisk] = partyRiskForSetup;

				AssertEquals(expectedPartyComplianceRisk, declaration.PartyComplianceRisk);
			}
		}

		public void TestLocationComplianceRisk()
		{
			TestLocationRisk("Potential Risk", "PSK");
			TestLocationRisk("Clear", "CLR");
			TestLocationRisk("Blocked", "BLK");

			void TestLocationRisk(ZString expectedLocationComplianceRisk, string locationRiskForSetup)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var declaration = Factory.New<BaseJobDeclaration>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = declaration.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = declaration.PK;
				instance[ComplianceRiskStatusSchema.COR_LocationRisk] = locationRiskForSetup;

				AssertEquals(expectedLocationComplianceRisk, declaration.LocationComplianceRisk);
			}
		}

		public void TestCommodityComplianceRisk()
		{
			TestCommodityRisk("Clear", "CLR");
			TestCommodityRisk("Blocked", "BLK");
			TestCommodityRisk("High Risk", "HSK");
			TestCommodityRisk("Incomplete", "INC");
			TestCommodityRisk("Not Applicable", "NAP");
			TestCommodityRisk("Possible Risk", "PRS");
			TestCommodityRisk("Unknown", "UNK");
			TestCommodityRisk("Not Assessed", "NAS");

			void TestCommodityRisk(ZString expectedCommodityComplianceRisk, string commodityRiskForSetup)
			{
				var type = ObjectFactory.GetType("ComplianceRiskStatus");
				var declaration = Factory.New<BaseJobDeclaration>();
				var instance = Factory.New(type);
				instance[ComplianceRiskStatusSchema.COR_ParentTableCode] = declaration.TablePrefix;
				instance[ComplianceRiskStatusSchema.COR_ParentID] = declaration.PK;
				instance[ComplianceRiskStatusSchema.COR_CommodityRisk] = commodityRiskForSetup;

				AssertEquals(expectedCommodityComplianceRisk, declaration.CommodityComplianceRisk);
			}
		}

		#endregion
	}

	public abstract class BaseJobDeclarationTest<T> : BaseJobDeclarationAbstractTest
		where T : BaseJobDeclaration
	{
		public virtual void TestGetCreditCheckMessage()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ScreeningStatus = "MAT";

				var declaration = (T)GetNewBusinessObject();
				declaration.JE_ScreeningStatus = "CLR";
				declaration.JE_JS = shipment.PK;
				Factory.Save();
				if (declaration is IValidateForCustomsMessagingSupporter supporter && supporter.SupportValidateCustomsMessaging)
				{
					var dec = declaration as IBaseJobDeclaration;
					AssertContains("Screening Status is not Clear.", dec.GetCreditCheckMessage());
				}
				Assert(true);
			}
		}

		public void TestJE_MessageType_SetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (declaration.SetterSuspender.SuspendSetting(BaseJobDeclaration.Schema.JE_MessageType))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertNotEquals("Should not be set", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			}
		}

		public void TestSupportInvoiceLineRefs()
		{
			AssertEquals("SupportInvoiceLineRefs should be expected", ExpectedSupportInvoiceLineRefs, Factory.GetNull<BaseJobDeclaration>().SupportInvoiceLineRefs);
		}

		public void TestSupportsJobComInvoiceLineTax()
		{
			AssertEquals("SupportsJobComInvoiceLineTax should be expected", ExpectedSupportsJobComInvoiceLineTax, Factory.GetNull<BaseJobDeclaration>().SupportsJobComInvoiceLineTax);
		}

		public virtual void TestEnableCopyCommercialInvoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(declaration.EnableCopyCommercialInvoice);
		}

		public virtual void TestEnableCommercialInvoiceMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(declaration.EnableCommercialInvoiceMenuItem);
		}

		public virtual void TestIsAutoUpdateBondedWarehouseEnabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(!declaration.IsAutoUpdateBondedWarehouseEnabled);
		}

		public virtual void TestIsWarehouseOrderFunctionActivated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert(!declaration.IsWarehouseOrderFunctionActivated);
		}

		public virtual void TestShouldCopyProcedureFromPreviousInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("ShouldCopyProcedureFromPreviousInvoiceLine should be false by default for all countries", false, declaration.ShouldCopyProcedureFromPreviousInvoiceLine);
		}

		public void TestSupportsBondedWarehouse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				Assert(!declaration.SupportsBondedWarehouse());
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					Assert(declaration.SupportsBondedWarehouse());
				}
			}
		}

		public virtual void TestMutexForDoMergeCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.Invoices.AddNew();
			declaration.FilteredInvoiceLines.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
			}
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;

			if (!declaration.IsDeclarationIntegrated)
			{
				using (var mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + declaration.PK.ToString()))
				{
					Assert("Locked OK", mutex.Lock());
					try
					{
						Assert(!declaration.DoMerge());
					}
					catch (ApplicationException ex)
					{
						AssertContains("is in the process of merging this job; system cannot merge this data as it will result in a different entry details.\r\nPlease retry merging when the other user has finished.", ex.Message);
					}
				}

				Assert(declaration.DoMerge());
				Assert(declaration.DoMergeMutex.IsLocked);
				Factory.Save();
				Assert(!declaration.DoMergeMutex.IsLocked);

				var entryHeaderPK = ZGuid.NewZGuid();
				var sql = $@"
					INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
					VALUES('{entryHeaderPK}', '{declaration.CountryCode}', '{declaration.PK}', 'BGM Reference 1', 'ENS', {declaration.JE_ClusterKey}, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				TestConnection.Command(sql).ExecuteNonQuery();

				try
				{
					Assert(!declaration.DoMerge());
				}
				catch (ApplicationException ex)
				{
					AssertContains("Another user has created new entry details.\r\nPlease reload the data before trying merging again.", ex.Message);
				}
				Factory.Save();
				Assert(!declaration.DoMergeMutex.IsLocked);

				declaration.CustomsEntryHeaders.Reload(false);
				var entryHeader = Factory.Load<CusEntryHeader>(entryHeaderPK);
				entryHeader.Delete();
				AssertEquals(1, declaration.DeletedEntryHeaderPKsInDatabase.Count);
				AssertEquals(entryHeader.PK, declaration.DeletedEntryHeaderPKsInDatabase[0]);
				Assert(declaration.DoMerge());
				Factory.Save();
				Assert(!declaration.DoMergeMutex.IsLocked);
			}

			declaration.LockDoMergeMutex();
			Assert(declaration.DoMergeMutex.IsLocked);
			declaration.Delete();
			Assert(!declaration.DoMergeMutex.IsLocked);
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;
		}

		public void TestMutexLockInfoForMerge_NullUser()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			using (var mutex = new ZGlobalMutex(MutexIDs.DataProcessing, "DoMerge" + declaration.PK.ToString()))
			{
				Assert(mutex.Lock());
				try
				{
					var lockInfo = "Mutex:" + MutexIDs.DataProcessing.Name + ":DoMerge" + declaration.PK.ToString();
					var emptyGuid = Guid.Empty;
					var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
								SET SV_ParentId = '{emptyGuid}',
									SV_SystemLastEditTimeUtc = GetUtcDate(),
									SV_SystemLastEditUser = 'USR'
								FROM dbo.StmServiceSemaphore
								INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
								WHERE SS_LockInfo LIKE '%{lockInfo}%';";
					TestConnection.Command(sql).ExecuteNonQuery();
					declaration.DoMerge();
				}
				catch (ApplicationException ex)
				{
					AssertContains("Unknown", ex.Message);
				}
			}
		}

		public void TestJE_CopyLastInvoiceLineDetailsToNewLines()
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(true, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);

				var filteredInvoiceLines = declaration.FilteredInvoiceLines;
				AssertEquals(true, filteredInvoiceLines.CopyLastLineDetailsToNewLines);
				AssertEquals(filteredInvoiceLines.CopyLastLineDetailsToNewLines, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);

				filteredInvoiceLines.CopyLastLineDetailsToNewLines = false;
				AssertEquals(false, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
			}

			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				AssertEquals(false, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);

				var filteredInvoiceLines = declaration.FilteredInvoiceLines;
				AssertEquals(false, filteredInvoiceLines.CopyLastLineDetailsToNewLines);
				AssertEquals(filteredInvoiceLines.CopyLastLineDetailsToNewLines, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);

				filteredInvoiceLines.CopyLastLineDetailsToNewLines = true;
				AssertEquals(true, declaration.JE_CopyLastInvoiceLineDetailsToNewLines);
			}
		}

		public virtual void TestDefaultDataGroupingCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? GlbCompany.CurrentCompany.Country.Code;
			var declaration = Factory.New<BaseJobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals(expectedCountry, declaration.GetDefaultDataGroupingCode());

				declaration.JE_GB = branch.PK;
				expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? Core.Constants.CountryCodes.Argentina;
				AssertEquals(expectedCountry, declaration.GetDefaultDataGroupingCode());
			});
		}

		protected virtual ZString? NoVariableDefaultDataGroupingCodeCountry => null;

		public virtual void TestDefaultDataGroupingCodeForTariffs()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			AssertEquals(GlbCompany.CurrentCompany.Country.Code, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			declaration.JE_GB = branch.PK;
			AssertEquals(Core.Constants.CountryCodes.Argentina, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));

			declaration.TariffDataGrouping = "XYZ";

			AssertEquals("XYZ", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
		}

		public virtual void TestDefaultDataGroupingCodeForDutyRateCodes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			AssertEquals(GlbCompany.CurrentCompany.Country.Code, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			declaration.JE_GB = branch.PK;
			AssertEquals(Core.Constants.CountryCodes.Argentina, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));

			declaration.TariffDataGrouping = "XYZ";

			AssertEquals("XYZ", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
		}

		public virtual void TestDefaultDataGroupingCodeForCusProcedure()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			AssertEquals(GlbCompany.CurrentCompany.Country.Code, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));

			declaration.JE_GB = branch.PK;
			AssertEquals(Core.Constants.CountryCodes.Argentina, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));

			declaration.TariffDataGrouping = "XYZ";

			AssertEquals(Core.Constants.CountryCodes.Argentina, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
		}

		public virtual void TestDefaultDataGroupingCodeForAdditionalDocumentCodes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<TariffDataGroupingDeclaration>();
			AssertEquals(GlbCompany.CurrentCompany.Country.Code, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

			declaration.JE_GB = branch.PK;
			AssertEquals(Core.Constants.CountryCodes.Argentina, declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

			declaration.TariffDataGrouping = "XYZ";

			AssertEquals("XYZ", declaration.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));
		}

		public void TestUniversalCopyAddInfo()
		{
			AssertNotNull(typeof(BaseJobDeclaration).GetCustomAttribute<UniversalCopyAddInfoAttribute>());
		}

		public void TestContainerModeListIsMappedForOrgSupBuyLinkTrnMode()
		{
			var transportModeContainerModeDictionary = new Dictionary<ZString, List<ZString>>();
			var declaration = Factory.New<BaseJobDeclaration>();
			foreach (var applicationCode in Declaration.Lookups.ApplicationCodeList.GetAllCodes().Union(new[] { string.Empty }))
			{
				declaration.JE_ApplicationCode = applicationCode;
				foreach (ICodeDescription messageTypePair in declaration.Lookups.MessageTypeList)
				{
					declaration.JE_MessageType = messageTypePair.Code;
					foreach (ICodeDescription transportModePair in declaration.Lookups.TransportTypeList)
					{
						declaration.JE_TransportMode = transportModePair.Code;
						if (!transportModeContainerModeDictionary.TryGetValue(transportModePair.Code, out var list))
						{
							list = new List<ZString>();
							transportModeContainerModeDictionary.Add(transportModePair.Code, list);
						}
						list.AddRange(declaration.Lookups.CargoIdTypeList.GetAllCodes().Where(x => !list.Contains(x)).Select(x => new ZString(x)));
					}
				}
			}
			var missingTransportModeContainerModeDictionary = new Dictionary<ZString, List<ZString>>();
			var linkTrnMode = Factory.New<OrgSupBuyLinkTrnMode>();
			foreach (var pair in transportModeContainerModeDictionary)
			{
				linkTrnMode.PF_TransportMode = pair.Key;
				var list = linkTrnMode.Lookups.ContainerModeList;
				var missingList = pair.Value.Where(x => !list.ContainsCode(x)).ToList();
				if (missingList.Count > 0)
				{
					if (!missingTransportModeContainerModeDictionary.TryGetValue(pair.Key, out var newList))
					{
						newList = new List<ZString>();
						missingTransportModeContainerModeDictionary.Add(pair.Key, newList);
					}
					newList.AddRange(missingList.Where(x => !newList.Contains(x)));
				}
			}
			if (missingTransportModeContainerModeDictionary.Count == 0)
			{
				Assert("All is good", true);
			}
			else
			{
				var messageBuilder = new ZStringBuilder();
				missingTransportModeContainerModeDictionary.ForEach(x => messageBuilder.AppendLine($@"{x.Key}: {string.Join(", ", x.Value)}"));
				messageBuilder.Prepend($"The following Transport Mode is missing a mapping for container mode; please ensure that it is added to {ObjectFactory.GetType<ICustomsCodePairListProvider>().FullName}:");
				Fail(messageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		public void TestEntryHasBeenSubmitted()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = DefaultExportMessageType;
			AssertEquals("IsExport", true, dec.IsExport);

			dec.Logs.AddNew(Events.CustomsCommenced, "Import");
			AssertEquals("!IsSubmitted", false, dec.EntryHasBeenSubmitted);
			dec.Logs.AddNew(Events.ExportCustomsCommenced, "Export");
			AssertEquals("IsSubmitted", true, dec.EntryHasBeenSubmitted);

			dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = DefaultImportMessageType;
			AssertEquals("IsImport", true, dec.IsImport);

			dec.Logs.AddNew(Events.ExportCustomsCommenced, "Export");
			AssertEquals("!IsSubmitted", false, dec.EntryHasBeenSubmitted);
			dec.Logs.AddNew(Events.CustomsCommenced, "Import");
			AssertEquals("IsSubmitted", true, dec.EntryHasBeenSubmitted);
		}

		public void TestILandedCostHeader_SupportsNoCostApportionmentItem()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, ((ILandedCostHeader)dec).SupportsNoCostApportionmentItem);
		}

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.BaseJobDeclaration);
			}
		}

		public virtual void TestGetSupportingDocSendingObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var sendingObject = declaration.GetSupportingDocSendingObject();
			AssertType(typeof(JobDeclarationSupportingDocSendingObject), sendingObject);
		}

		public virtual void TestAreMultipleEntryInstructionsAllowed()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, dec.AreMultipleEntryInstructionsAllowed);
		}

		public void TestInvoicesContainProductWithMissingUnitOfMeasure_WithMissingUnitOfMeasure_ReturnsTrue()
		{
			var invoiceHeader = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			for (var i = 1; i <= 3; i++)
			{
				var line = invoiceHeader.JobComInvoiceLines.AddNew();
				line.JI_PartNo = "part " + i;
				line.JI_Description = "description " + i;
				line.JI_Tariff = "T2";
				line.JI_InvoiceUQ = "PKG";
				line.JI_CC = ZGuid.NewZGuid();
			}

			var line1 = invoiceHeader.JobComInvoiceLines[0];
			line1.JI_InvoiceUQ = string.Empty;

			Assert("InvoicesContainProductWithMissingUnitOfMeasure returns true since some line is missing JI_InvoiceUQ", TestDec.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ);

			line1.JI_InvoiceUQ = "NMB";
			Assert("InvoicesContainProductWithMissingUnitOfMeasure returns false since all the lines have got JI_InvoiceUQ", !TestDec.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ);
		}

		public void TestInvoicesContainProductWithMissingUnitOfMeasure_NoMissingUnitOfMeasure_ReturnsFalse()
		{
			var invoiceHeader = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			for (var i = 1; i <= 3; i++)
			{
				var line = invoiceHeader.JobComInvoiceLines.AddNew();
				line.JI_PartNo = "part " + i;
				line.JI_Description = "description " + i;
				line.JI_Tariff = "T2";
				line.JI_InvoiceUQ = "PKG";
				line.JI_CC = ZGuid.NewZGuid();
			}

			Assert("InvoicesContainProductWithMissingUnitOfMeasure returns false since none of the lines is missing JI_InvoiceUQ", !TestDec.InvoicesContainNotPersistentActiveProductWithMissingInvoiceUQ);
		}

		public void TestBillingBranchBillingDepartmetnBillingOperator()
		{
			var branchPK = GlbBranch.CurrentBranch.PK;
			var companyPK = GlbCompany.CurrentCompany.PK;
			var departmentPK = GlbDepartment.CurrentDepartment.PK;

			var branch = GlbBranch.CurrentBranch;
			branch.GB_GC = companyPK;
			var department = GlbDepartment.CurrentDepartment;

			var newStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			newStaff1.GS_Code = "ST1";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_GB = branchPK;
			declaration.JE_DeclarationReference = "B01234567";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			job.JH_ParentTableCode = "JE";
			job.JH_GB = branchPK;
			job.JH_GC = companyPK;
			job.JH_GE = departmentPK;
			job.JH_GS_NKRepOps = newStaff1.GS_Code;

			Factory.Save();

			declaration.Job.JH_GB = branchPK;
			AssertEquals("JH_GB PK", branch.GB_Code, declaration.BillingBranch);

			declaration.Job.JH_GE = departmentPK;
			AssertEquals("JH_GB PK", department.GE_Code, declaration.BillingDepartment);

			AssertEquals("JH_GS_NKRepOps PK", declaration.Job.JH_GS_NKRepOps, declaration.BillingOperator);
		}

		public void TestIsConsigneeConsignerMatch()
		{
			var declaration = PrepareDataForIsConsigneeConsignerMatch();
			var agentsInvoiceDocuments = declaration.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = declaration.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);

			AssertEquals("declaration should have 3 AgentsInvoice required documents added.", 3, agentsInvoiceDocuments.Count());
			AssertEquals("declaration should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("3 AgentsInvoice required documents added were from supplier/importer relationship.", "0004,0005,0006", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from importer.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		public void TestIsConsigneeConsignerMatchWhenClone()
		{
			var declaration = PrepareDataForIsConsigneeConsignerMatch();
			var newDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Factory.Save();
			var agentsInvoiceDocuments = newDeclaration.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.AgentsInvoice).OrderBy(doc => doc.EQ_DocNumber);
			var deliveryOrderDocuments = newDeclaration.DocsAndCartage.RequiredDocuments.OfType<JobRequiredDocument>().Where(doc => doc.EQ_DocType == Constants.RefDocTypes.DeliveryOrder).OrderBy(doc => doc.EQ_DocNumber);
			AssertEquals("newDeclaration should have 3 AgentsInvoice required documents added.", 3, agentsInvoiceDocuments.Count());
			AssertEquals("newDeclaration should have 1 DeliveryOrder required documents added.", 1, deliveryOrderDocuments.Count());
			AssertEquals("3 AgentsInvoice required documents added were from supplier/importer relationship.", "0004,0005,0006", string.Join(",", agentsInvoiceDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
			AssertEquals("1 DeliveryOrder required documents added was from importer.", "0003", string.Join(",", deliveryOrderDocuments.Select(doc => doc.EQ_DocNumber).ToArray()));
		}

		BaseJobDeclaration PrepareDataForIsConsigneeConsignerMatch()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DateAtOrigin = ZDateTime.Today;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_OH_Supplier = consignor.PK;

			var validToDate = ZDate.Today.AddDays(5);
			var requiredDocument1 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0001", ZGuid.Empty);
			var requiredDocument2 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0002", consignor.PK);
			var requiredDocument3 = AddNewJobRequiredDoc(consignee, Constants.RefDocTypes.DeliveryOrder, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0003", orgHeader.PK);

			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;
			var requiredDocument4 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0004", ZGuid.Empty);
			var requiredDocument5 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0005", consignor.PK);
			var requiredDocument6 = AddNewJobRequiredDoc(supplierLink, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), validToDate, "0006", orgHeader.PK);
			Factory.Save();

			return declaration;
		}

		public void TestIsCustomsLineAmendmentATotalReplacementForIntegratedDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Thailand))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				declaration.ActiveEntryHeaders.AddNew();
				Assert("Integrate declaration IsCustomsLineAmendmentATotalReplacement should return false without exception.", declaration.ShouldKeepDeletedLinesOnAmendment);
			}
		}

		JobRequiredDocument AddNewJobRequiredDoc(IHaveRequiredDocuments org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDate date, ZString docNumber, ZGuid documentOwner)
		{
			JobRequiredDocument result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			result.EQ_OH_DocumentOwner = documentOwner;
			return result;
		}

		public void TestOrdersLimit()
		{
			const string notification = @"The number of Orders on a Declaration is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			using (CustomsDataRegistry.Instance.OrdersPerDeclarationLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (CustomsDataRegistry.Instance.OrdersPerDeclarationLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var declaration = Factory.New<BaseJobDeclaration>();

				var order1 = declaration.AttachedOrders.AddNew();
				AssertNoRowWarnings(order1);
				AssertNoRowErrors(order1);

				var order2 = declaration.AttachedOrders.AddNew();
				AssertHasRowWarning(order2, notification);
				AssertNoRowErrors(order2);

				var order3 = declaration.AttachedOrders.AddNew();
				AssertNoRowWarnings(order3);
				AssertHasRowError(order3, notification);
			}
		}

		[TestDate(2017, 08, 14)]
		public virtual void TestResetInvoiceDateForGroupingInvoiceInTemplateCopy()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var groupinvoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2007, 08, 14);
			var normalinvoice = declaration.Invoices.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2006, 08, 14);
			Factory.Save();

			var declarationCopy = (BaseJobDeclaration)declaration.TemplateCopy();
			var groupinvoiceCopy = (BaseJobComInvoiceGroupHeader)declarationCopy.AllGroupHeaders.First();
			AssertEquals(new ZDateTime(2017, 08, 14), groupinvoiceCopy.JZ_InvoiceDate);

			var normalinvoiceCopy = declarationCopy.Invoices.First(x => !x.JZ_GroupInvoice);
			AssertEquals(new ZDateTime(2006, 08, 14), normalinvoiceCopy.JZ_InvoiceDate);
		}

		public virtual void TestLocalCurrencyCoreOverride()
		{
			var dec = GetJobDeclaration();
			if (dec.GetType() != typeof(BaseJobDeclaration)
				&& dec.GetType() != ObjectFactory.GetType<EU.IJobDeclaration>()
				&& dec.GetType() != ObjectFactory.GetType<AsycudaCustoms.IJobDeclaration>())
			{
				Assert("Please override LocalCurrencyCodeCore and this test in your sub-class and assert correct local currency is set", !((BaseJobDeclarationForTesting)dec).LocalCurrencyCodeCoreExposed.IsEmpty);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestReCalculateRelatedPortsAfterMessageTypeChanged()
		{
			GlbCompany.CurrentCompany.SetCountry("US");

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = orgHeader.PK;
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("AUSYD", declaration.JE_RL_NKOrigin);

			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals(JobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			AssertEquals(ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals(ZString.Empty, declaration.JE_RL_NKOrigin);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfArrival);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("AUSYD", declaration.JE_RL_NKOrigin);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, declaration.JE_RL_NKPortOfLoading);
			AssertEquals(ZString.Empty, declaration.JE_RL_NKOrigin);
			AssertEquals("AUSYD", declaration.JE_RL_NKPortOfArrival);
		}

		public void TestIsDPSFreightMovementRestrictedCore_AllJobs_EnableComplianceRisk()
		{
			AssertIsDPSFreightMovementRestrictedCore_AllJobs(true);
		}

		public void TestIsDPSFreightMovementRestrictedCore_AllJobs_DisableComplianceRisk()
		{
			AssertIsDPSFreightMovementRestrictedCore_AllJobs(false);
		}

		public void TestIsDPSFreightMovementRestrictedCore_InternationalJobs_EnableComplianceRisk()
		{
			AssertIsDPSFreightMovementRestrictedCore_InternationalJobs(true);
		}

		public void TestIsDPSFreightMovementRestrictedCore_InternationalJobs_DisableComplianceRisk()
		{
			AssertIsDPSFreightMovementRestrictedCore_InternationalJobs(false);
		}

		public void TestDefaultOriginFromSupplierAndDefaultFinalDestinationPortFromImporterNotWhenReadOnly()
		{
			GlbCompany.CurrentCompany.SetCountry("ER");
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "ERMIK";
			var dec = Factory.New<BaseJobDeclarationForTesting>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.PortsAreReadOnly = false;
			AssertEquals("PreReq", false, dec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals("PreReq", false, dec.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			dec.PortsAreReadOnly = true;
			AssertEquals("PreReq", true, dec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals("PreReq", true, dec.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			dec.PortsAreReadOnly = true;
			dec.JE_OH_Importer = org.PK;
			AssertEquals("", dec.JE_RL_NKFinalDestination);
			dec.JE_OH_Supplier = org.PK;
			AssertEquals("", dec.JE_RL_NKPortOfLoading);
			dec.PortsAreReadOnly = false;
			dec.JE_OH_Importer = ZGuid.Empty;
			dec.JE_OH_Importer = org.PK;
			AssertEquals("ERMIK", dec.JE_RL_NKFinalDestination);
			dec.JE_OH_Supplier = ZGuid.Empty;
			dec.JE_OH_Supplier = org.PK;
			AssertEquals("ERMIK", dec.JE_RL_NKPortOfLoading);
		}

		public virtual void TestDoNotDefaultPortsFromImporterWhenImporterCountryDifferentFromEnv()
		{
			var countryCode = GetForeignCountryCodeForTestDefaultPorts();
			GlbDepartment.CurrentDepartment.GE_Import = true;
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = countryCode + "MIK";
			port.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

			var importer = Factory.New<OrgHeader>();
			importer.OH_RL_NKClosestPort = countryCode + "MIK";

			var dec = GetJobDeclaration();
			var tuple = GetMessageTypeForTestDefaultPorts();
			var importCode = tuple.Item1;
			var otherCode = tuple.Item2;
			AssertEquals("Message Type", importCode, dec.JE_MessageType);
			dec.JE_OH_Supplier = importer.PK;
			dec.JE_OH_Importer = importer.PK;

			if (WillDefaultMessageTypeDueToImporter)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Message Type", importCode, dec.JE_MessageType);
					Assert("JE_RL_NKPortOfArrival", dec.JE_RL_NKPortOfArrival.IsEmpty);
					Assert("JE_RL_NKPortOfFirstArrival", dec.JE_RL_NKPortOfFirstArrival.IsEmpty);
					Assert("JE_RL_NKFinalDestination", dec.JE_RL_NKFinalDestination.IsEmpty);
				});
			}
			else
			{
				AssertEquals("Message Type", otherCode, dec.JE_MessageType);
			}
		}

		protected virtual ZString GetForeignCountryCodeForTestDefaultPorts()
		{
			return Constants.CountryCodes.Italy;
		}

		protected virtual Tuple<ZString, ZString> GetMessageTypeForTestDefaultPorts()
		{
			return new Tuple<ZString, ZString>(JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Export);
		}

		protected virtual bool WillDefaultMessageTypeDueToImporter => true;

		public void TestIsDataChangeSuspendedByFakeDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			using (declaration.SuspendDataChangeByFakeDeclaration())
			{
				Assert(declaration.IsDataChangeSuspendedByFakeDeclaration);
			}
			Assert(!declaration.IsDataChangeSuspendedByFakeDeclaration);
		}

		public void TestMessageCollectionApplicationCodeListCore()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = "ABC";
			var mockMessageABC = Factory.NewMoq<EDIMessage>();
			mockMessageABC.Protected().Setup<string>("GetMessageReferenceNumber").Returns("1");
			var messageABC = mockMessageABC.Object;
			messageABC.EM_ApplicationCode = "ABC";
			messageABC.EM_LinkedObject = declaration;
			var mockMessageGood = Factory.NewMoq<EDIMessage>();
			mockMessageGood.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2");
			var messageGood = mockMessageGood.Object;
			messageGood.EM_ApplicationCode = "XXX";
			messageGood.EM_LinkedObject = declaration;
			var mockMessageCrap = Factory.NewMoq<EDIMessage>();
			mockMessageCrap.Protected().Setup<string>("GetMessageReferenceNumber").Returns("2");
			var messageCrap = mockMessageCrap.Object;
			messageCrap.EM_ApplicationCode = "POO";
			messageCrap.EM_LinkedObject = declaration;
			AssertEquals(2, declaration.Messages.Count);
		}

		public void TestJE_AddInfo()
		{
			var declaration = (BaseJobDeclaration)GetNewBusinessObject();
			var addInfo = (declaration as IAddInfoManager)?.AddInfo as BaseAddInfo;
			if (addInfo != null && addInfo.ZPropertyInfoHash.Count > 0)
			{
				foreach (var properyInfos in addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => declaration.FindPropertyInfo(x.Name) != null).Batch(20))
				{
					declaration.JE_AddInfo = string.Join("*", properyInfos.Select(x => x.Name.Remove(0, 3) + "=").ToArray());
					foreach (var propertyInfo in properyInfos)
					{
						if (propertyInfo.IsNAddInfoField())
						{
							Assert(propertyInfo.Name + " should be stored in JE_NAddInfo", declaration.JE_NAddInfo.Contains(propertyInfo.Name.Remove(0, 3) + "="));
						}
						else
						{
							Assert(propertyInfo.Name + " should NOT be stored in JE_NAddInfo", !declaration.JE_NAddInfo.Contains(propertyInfo.Name.Remove(0, 3) + "="));
						}
					}
				}
			}
			Assert(true);
		}

		public void TestAllTranportModesAreTranslatedForJE_TransportModeGeneric()
		{
			CodeDescriptionPairList transportTypes = TestDec.Lookups.TransportTypeList;
			TransportTypeGenericList genericList = new TransportTypeGenericList();
			CombineAssertions(delegate
			{
				foreach (ICodeDescription transportType in transportTypes)
				{
					TestDec.JE_TransportMode = transportType.Code;
					ZString transportModeGeneric = TestDec.TransportModeGeneric;

					if (!TransportModesNotToTestForGenericTranslation.Contains(transportType.Code))
					{
						AssertEquals("JE_TransportModeGeneric.IsEmpty when JE_TransportMode = " + transportType.Code, false, transportModeGeneric.IsEmpty);
					}

					Assert("genericList.ContainsCode(JE_TransportModeGeneric) when JE_TransportMode = " + transportType.Code, genericList.ContainsCode(transportModeGeneric));
				}
			});
		}

		protected virtual ZString[] TransportModesNotToTestForGenericTranslation => Array.Empty<ZString>();

		public void TestAllTranportModesAreTranslatedToGenericForIHaveInternalCartage()
		{
			CodeDescriptionPairList transportTypes = TestDec.Lookups.TransportTypeList;
			TransportTypeGenericList genericList = new TransportTypeGenericList();
			CombineAssertions(delegate
			{
				foreach (ICodeDescription transportType in transportTypes)
				{
					TestDec.JE_TransportMode = transportType.Code;
					ZString transportModeGeneric = TestDec.TransportModeGeneric;
					ZString transportModeForIHaveInternalCartage = ((IHaveInternalCartage)TestDec).TransportMode;
					AssertEquals("IHaveInternalCartage.TransportMode should be generic for " + transportType.Code, transportModeGeneric, transportModeForIHaveInternalCartage);
				}
			});
		}

		#region OnSaving

		public void TestDeleteDuplicatedBillsOnSavingIfPackingInformationIsNotRelevant()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			declaration = mockDeclaration.Object;
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			declaration.JE_MasterBill = "MBL";
			var bill1 = declaration.PrimaryMasterBill;

			declaration.JE_HouseBill = "HBL";
			var bill2 = declaration.PrimaryHouseBill;

			var bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill3.CU_BillNum = "SHBL1";
			bill3.CU_CU_ParentBill = bill2.PK;

			var bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill4.CU_CU_ParentBill = bill2.PK;
			bill4.CU_BillNum = "SHBL1";

			var bill5 = declaration.Bills.AddNew();
			bill5.CU_BillType = BillTypeList.Codes.MasterBill;
			bill5.CU_BillNum = "MBL";

			var bill6 = declaration.Bills.AddNew();
			bill6.CU_BillType = BillTypeList.Codes.HouseBill;
			bill6.CU_BillNum = "HBL";
			bill6.CU_CU_ParentBill = bill5.PK;
			bill6.CU_GUIPresentationRecord = true;

			declaration.OnSaving();

			AssertEquals("Duplicated bills should NOT be deleted because packing information is relevant", 6, declaration.Bills.Count);

			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			declaration.OnSaving();

			AssertEquals("Only primary bills should remain because packing information is NOT relevant", 2, declaration.Bills.Count);
			Assert("Not primary MBL is deleted", bill5.IsDeleted);
			Assert("All SHBLs are deleted", bill3.IsDeleted && bill4.IsDeleted);
			Assert("One of HBLs is deleted", bill2.IsDeleted || bill6.IsDeleted);
		}

		public void TestDuplicatedDeclaration()
		{
			ZGuid shipmentPk = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipmentPk;
			declaration.JE_GB = Env.CurrentBranch.PK;
			Factory.Save();
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipmentPk;
			Assert(ErrorReporter.HasBeenReported("DeclarationAlreadyExistsForShipment"));
			ErrorReporter.Clear();
			try
			{
				Factory.Save();
				Assert("Duplicated Declaration Exception should be thrown", false);
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("Duplicated Declaration Exception should be thrown but was" + ex.Message, "DuplicatedDeclaration", ex.Heading);
			}
		}

		public void TestOnSavingCountryRequiredDocuments()
		{
			PrepareDataForDefaultRequiredDocuments_Country();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = organisation.PK;
			declaration.JE_OH_Importer = organisation2.PK;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			Factory.Save();

			AssertNull("BOE, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BillOfEntry));
			AssertNull("CAD, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNull("CHG, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull("DAL, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNull("ARN, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ArrivalNotice));
			AssertNull("INV, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Invoice));
			AssertNull("SAD, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShippingAdvice));
			AssertNull("EFT, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EFTRequest));

			AssertEquals(5, declaration.DocsAndCartage.RequiredDocuments.Count);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.AgentsInvoice);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.AgentsInstruction);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.BankDraft);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.DangerousGoodsForm);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.HouseBill);

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_OH_Supplier = organisation.PK;
			declaration2.JE_OH_Importer = organisation2.PK;
			declaration2.JE_MessageType = DefaultExportMessageType;
			declaration2.JE_RL_NKOrigin = "AUSYD";
			declaration2.JE_RL_NKFinalDestination = "NZAKL";
			declaration2.JE_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			AssertNull("BOE, shouldn't be in the list", declaration.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.BillOfEntry));
			AssertNull("CAD, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.CartageAdvice));
			AssertNull("CHG, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ChargeSheet));
			AssertNull("DAL, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.DelayAlert));
			AssertNull("INV, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.Invoice));
			AssertNull("SAD, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.ShippingAdvice));
			AssertNull("EFT, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.EFTRequest));
			AssertNull("AIN, shouldn't be in the list", declaration2.DocsAndCartage.RequiredDocuments.GetDocByType(Constants.RefDocTypes.AgentsInstruction));

			AssertEquals(5, declaration2.DocsAndCartage.RequiredDocuments.Count);
			AssertRequiredDocument(declaration2, Constants.RefDocTypes.AgentsInvoice, "EXP");
			AssertRequiredDocument(declaration2, Constants.RefDocTypes.ArrivalNotice, "EXP");
			AssertRequiredDocument(declaration2, Constants.RefDocTypes.BankDraft, "EXP");
			AssertRequiredDocument(declaration2, Constants.RefDocTypes.DangerousGoodsForm, "EXP");
			AssertRequiredDocument(declaration2, Constants.RefDocTypes.HouseBill, "EXP");
		}

		public void TestTestOnCloneCountryRequiredDocuments()
		{
			PrepareDataForDefaultRequiredDocuments_Country();

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = organisation.PK;
			declaration.JE_OH_Importer = organisation2.PK;
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			Factory.Save();
			AssertEquals(5, declaration.DocsAndCartage.RequiredDocuments.Count);
			var newDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Factory.Save();
			AssertEquals(5, newDeclaration.DocsAndCartage.RequiredDocuments.Count);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.AgentsInvoice);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.AgentsInstruction);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.BankDraft);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.DangerousGoodsForm);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.HouseBill);

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_OH_Supplier = organisation.PK;
			declaration2.JE_OH_Importer = organisation2.PK;
			declaration2.JE_MessageType = DefaultExportMessageType;
			declaration2.JE_RL_NKOrigin = "AUSYD";
			declaration2.JE_RL_NKFinalDestination = "NZAKL";
			declaration2.JE_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();
			AssertEquals(5, declaration2.DocsAndCartage.RequiredDocuments.Count);
			var newDeclaration2 = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration2, CloneType.TemplateCopy).Clone();
			Factory.Save();
			AssertEquals(5, newDeclaration2.DocsAndCartage.RequiredDocuments.Count);
			AssertRequiredDocument(newDeclaration2, Constants.RefDocTypes.AgentsInvoice, "EXP");
			AssertRequiredDocument(newDeclaration2, Constants.RefDocTypes.ArrivalNotice, "EXP");
			AssertRequiredDocument(newDeclaration2, Constants.RefDocTypes.BankDraft, "EXP");
			AssertRequiredDocument(newDeclaration2, Constants.RefDocTypes.DangerousGoodsForm, "EXP");
			AssertRequiredDocument(newDeclaration2, Constants.RefDocTypes.HouseBill, "EXP");
		}

		void AssertRequiredDocument(BaseJobDeclaration declaration, ZString docType, string docUsage = "IMP", string docPeriod = "SHP")
		{
			var document = declaration.DocsAndCartage.RequiredDocuments.GetDocByType(docType);
			AssertNotNull($"{docType} should be in the list", document);
			AssertEquals($"DocUsage should be {docUsage}", docUsage, document.EQ_DocUsage);
			AssertEquals($"DocPeriod should be {docPeriod}", docPeriod, document.EQ_DocPeriod);
		}

		void PrepareDataForDefaultRequiredDocuments_Country()
		{
			var australia = Constants.CountryCodes.Australia;
			var countryAU = RefCountry.LoadFromCountryCode(Factory, australia);

			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, australia, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInstruction, "", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ArrivalNotice, australia, "", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BankDraft, "", "", JobRequiredDocument.DocUsage.Both, Constants.TransportModes.All, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BillOfEntry, australia, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.Rail, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.CartageAdvice, australia, "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, false);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ChargeSheet, "US", "NZ", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DelayAlert, australia, "US", JobRequiredDocument.DocUsage.Import, Constants.TransportModes.All, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, australia, "NZ", JobRequiredDocument.DocUsage.Export, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.DangerousGoodsForm, australia, "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.HouseBill, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.Invoice, australia, australia, JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.ShippingAdvice, "", "", JobRequiredDocument.DocUsage.Domestic, Constants.TransportModes.Sea, true);
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.EFTRequest, australia, australia, JobRequiredDocument.DocUsage.All, Constants.TransportModes.Sea, true);
		}

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingOrganisationRequiredDocuments()
		{
			var declaration = PrepareDataForDefaultRequiredDocuments_Organisation();
			AssertRequiredDocument(declaration, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both);
		}

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnCloneOrganisationRequiredDocuments()
		{
			var declaration = PrepareDataForDefaultRequiredDocuments_Organisation();
			var newDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Factory.Save();
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both);
		}

		BaseJobDeclaration PrepareDataForDefaultRequiredDocuments_Organisation()
		{
			var australia = Constants.CountryCodes.Australia;
			var countryAU = RefCountry.LoadFromCountryCode(Factory, australia);

			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, australia, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.VetinaryCertificate, "", "", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_RL_NKClosestPort = "NZAKL";

			GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");
			GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			var requiredDoc6 = GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			return declaration;
		}

		public void TestCurrentUserHasSecurityAccess()
		{
			Env.Security.ImportEdit.IsAllowed = true;
			Env.Security.ExportEdit.IsAllowed = true;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals(true, declaration.CurrentUserHasSecurityAccess);

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals(true, declaration.CurrentUserHasSecurityAccess);

			Env.Security.ImportEdit.IsAllowed = false;
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals(false, declaration.CurrentUserHasSecurityAccess);

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals(true, declaration.CurrentUserHasSecurityAccess);

			Env.Security.ImportEdit.IsAllowed = true;
			Env.Security.ExportEdit.IsAllowed = false;

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals(false, declaration.CurrentUserHasSecurityAccess);

			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals(true, declaration.CurrentUserHasSecurityAccess);

			Env.Security.ImportEdit.IsAllowed = false;

			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals(false, declaration.CurrentUserHasSecurityAccess);

			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals(false, declaration.CurrentUserHasSecurityAccess);
		}

		public void TestSupplierImporterLink()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			RefCountry otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));

			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			OrgHeader consignor = OrgHeader.New(Factory);

			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			AssertNull(declaration.SupplierImporterLink);

			OrgSupplierBuyerLink link1 = consignee.SupplierLinks.AddNew(consignor);
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			OrgSupplierBuyerLink link2 = consignee.SupplierLinks.AddNew(consignor);
			link2.OL_RN_NKImporterCountry = otherCountry2.RN_Code;

			AssertEquals(link1, declaration.SupplierImporterLink);

			declaration.JE_RL_NKFinalDestination = otherUnloco2.RL_Code;
			AssertEquals(link2, declaration.SupplierImporterLink);

			declaration.JE_RL_NKFinalDestination = "XXXXX";
			link1.OL_RN_NKImporterCountry = "BF";
			var oldCompanyCountry = declaration.Branch.Company.GC_RN_NKCountryCode;
			declaration.Branch.Company.GC_RN_NKCountryCode = otherCountry2.RN_Code;
			AssertEquals(link2, declaration.SupplierImporterLink);
			declaration.Branch.Company.GC_RN_NKCountryCode = oldCompanyCountry;
		}

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnSavingBuyerSupplierRequiredDocuments()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			PrepareDataForDefaultRequiredDocuments_BuyerSupplier(consignor, consignee);
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "SGSIN";
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertRequiredDocument(declaration, Constants.RefDocTypes.VetinaryCertificate, ZDateTimeOffset.Today.AddDays(-2));
			AssertRequiredDocument(declaration, Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Today.AddDays(-2));
			AssertRequiredDocument(declaration, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(declaration, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			AssertRequiredDocument(declaration, Constants.RefDocTypes.VetinaryCertificate, ZDateTimeOffset.Today.AddDays(-3));
		}

		[TestDate(2007, 3, 15, 10, 15, 21)]
		public void TestOnCloneBuyerSupplierRequiredDocuments()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			PrepareDataForDefaultRequiredDocuments_BuyerSupplier(consignor, consignee);
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "SGSIN";
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_MessageType = "IMP";
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var newDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Factory.Save();
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.VetinaryCertificate, ZDateTimeOffset.Today.AddDays(-2));
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.AgentsInvoice, ZDateTimeOffset.Today.AddDays(-2));
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);

			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "USLAX";
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_DateAtOrigin = ZDateTime.Today.AddDays(1);
			Factory.Save();
			newDeclaration = (BaseJobDeclaration)new CustomsBusinessObjectCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Factory.Save();
			AssertRequiredDocument(newDeclaration, Constants.RefDocTypes.VetinaryCertificate, ZDateTimeOffset.Today.AddDays(-3));
		}

		void AssertRequiredDocument(BaseJobDeclaration declaration, ZString docType, ZDateTimeOffset dateReceived, string docUsage = "BTH", string docPeriod = "PER")
		{
			var document = declaration.DocsAndCartage.RequiredDocuments.GetDocByType(docType);
			AssertNotNull($"{docType} should be in the list", document);
			AssertEquals($"DocUsage should be {docUsage}", docUsage, document.EQ_DocUsage);
			AssertEquals($"DocPeriod should be {docPeriod}", docPeriod, document.EQ_DocPeriod);
			AssertEquals($"DateReceived should be {dateReceived}", dateReceived, document.EQ_DateReceived);
		}

		void PrepareDataForDefaultRequiredDocuments_BuyerSupplier(OrgHeader consignor, OrgHeader consignee)
		{
			var australia = Constants.CountryCodes.Australia;
			var countryAU = RefCountry.LoadFromCountryCode(Factory, australia);

			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.AgentsInvoice, australia, "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass
			GetNewRequiredDoc(countryAU, Constants.RefDocTypes.BeneficiaryCertificate, "", "NZ", JobRequiredDocument.DocUsage.All, Constants.TransportModes.All, true);//pass

			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			GetNewJobRequiredDoc(consignor, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0001");

			var buyerSupplierLink1 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink1.OL_RN_NKImporterCountry = "NZ";

			GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.DangerousGoodsForm, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0002");
			GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.BeneficiaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0003");
			var requiredDoc6 = GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.AgentsInvoice, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0004");
			requiredDoc6.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-2);
			GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.SanitaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0005");
			GetNewBuyerSupplierRequiredDoc(buyerSupplierLink1, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-2), ZDateTime.Today.Date.AddDays(2), "0006");

			var buyerSupplierLink2 = consignor.BuyerLinks.AddNew(consignee);
			buyerSupplierLink2.OL_RN_NKImporterCountry = "US";
			GetNewBuyerSupplierRequiredDoc(buyerSupplierLink2, Constants.RefDocTypes.VetinaryCertificate, JobRequiredDocument.DocUsage.Both, Constants.JobRequiredDocuments.DocumentPeriods.Periodic, ZDate.Today.AddDays(-3), ZDateTime.Today.Date.AddDays(3), "0007");
		}

		JobRequiredDocument GetNewJobRequiredDoc(OrgHeader org, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			var result = org.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		JobRequiredDocument GetNewBuyerSupplierRequiredDoc(OrgSupplierBuyerLink buyerSupplierLink, ZString docType, ZString docUsage, ZString period, ZDate recvDate, ZDateTime date, ZString docNumber)
		{
			var result = buyerSupplierLink.RequiredDocuments.AddNew();
			result.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			result.EQ_DocType = docType;
			result.EQ_DocUsage = docUsage;
			result.EQ_DocPeriod = period;
			result.EQ_DateReceived = recvDate.ToZDateTime().ToDateTimeOffset(null);
			result.EQ_ValidToDate = date;
			result.EQ_DocNumber = docNumber;
			return result;
		}

		RefCountryRequiredDocument GetNewRequiredDoc(RefCountry country, ZString docType, ZString orig, ZString dest, ZString usage, ZString transport, ZBool isBrokerage)
		{
			var result = country.RequiredDocuments.AddNew();
			result.RD_DocType = docType;
			result.RD_RN_NKOrigin = orig;
			result.RD_RN_NKDestination = dest;
			result.RD_DocUsage = usage;
			result.RD_TransportMode = transport;
			result.RD_OnBrokerage = isBrokerage;
			return result;
		}

		#endregion

		public void TestGetImporterEquipment()
		{
			var dec = Factory.New<BaseJobDeclarationForTesting>();
			AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
			var orgHeader = Factory.New<OrgHeader>();
			dec.JE_OH_Importer = orgHeader.PK;
			AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());

			dec.JE_TransportMode = Constants.TransportModes.Air;
			dec.ImporterDeliveryAddress.Address.OA_AIREquipmentNeeded = "";
			AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
			dec.ImporterDeliveryAddress.Address.OA_AIREquipmentNeeded = "A01";
			AssertEquals("ImporterEquipment", "A01", dec.GetImporterEquipment_Exposed());
			dec.JE_TransportMode = Constants.TransportModes.Sea;
			dec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			dec.ImporterDeliveryAddress.Address.OA_LCLEquipmentNeeded = "";
			AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
			dec.ImporterDeliveryAddress.Address.OA_LCLEquipmentNeeded = "L01";
			AssertEquals("ImporterEquipment", "L01", dec.GetImporterEquipment_Exposed());
			dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			dec.ImporterDeliveryAddress.Address.OA_FCLEquipmentNeeded = "";
			AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
			dec.ImporterDeliveryAddress.Address.OA_FCLEquipmentNeeded = "F01";
			AssertEquals("ImporterEquipment", "F01", dec.GetImporterEquipment_Exposed());

			dec.ImporterDeliveryAddress.E2_AddressOverride = true;
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.DataType.SuspendValidation())
			{
				dec.JE_TransportMode = Constants.TransportModes.Air;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "A02");
				AssertEquals("ImporterEquipment", "A02", dec.GetImporterEquipment_Exposed());
			}
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.DataType.SuspendValidation())
			{
				dec.JE_TransportMode = Constants.TransportModes.Sea;
				dec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "L02");
				AssertEquals("ImporterEquipment", "L02", dec.GetImporterEquipment_Exposed());
			}
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.DataType.SuspendValidation())
			{
				dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("ImporterEquipment", dec.GetImporterEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "F02");
				AssertEquals("ImporterEquipment", "F02", dec.GetImporterEquipment_Exposed());
			}
		}

		public void TestGetSupplierEquipment()
		{
			var dec = Factory.New<BaseJobDeclarationForTesting>();
			AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
			var orgHeader = Factory.New<OrgHeader>();
			dec.JE_OH_Supplier = orgHeader.PK;
			AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());

			dec.JE_TransportMode = Constants.TransportModes.Air;
			dec.SupplierPickupAddress.Address.OA_AIREquipmentNeeded = "";
			AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
			dec.SupplierPickupAddress.Address.OA_AIREquipmentNeeded = "A03";
			AssertEquals("SupplierEquipment", "A03", dec.GetSupplierEquipment_Exposed());
			dec.JE_TransportMode = Constants.TransportModes.Sea;
			dec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			dec.SupplierPickupAddress.Address.OA_LCLEquipmentNeeded = "";
			AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
			dec.SupplierPickupAddress.Address.OA_LCLEquipmentNeeded = "L03";
			AssertEquals("SupplierEquipment", "L03", dec.GetSupplierEquipment_Exposed());
			dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			dec.SupplierPickupAddress.Address.OA_FCLEquipmentNeeded = "";
			AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
			dec.SupplierPickupAddress.Address.OA_FCLEquipmentNeeded = "F03";
			AssertEquals("SupplierEquipment", "F03", dec.GetSupplierEquipment_Exposed());

			dec.SupplierPickupAddress.E2_AddressOverride = true;
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.DataType.SuspendValidation())
			{
				dec.JE_TransportMode = Constants.TransportModes.Air;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "A04");
				AssertEquals("SupplierEquipment", "A04", dec.GetSupplierEquipment_Exposed());
			}
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.DataType.SuspendValidation())
			{
				dec.JE_TransportMode = Constants.TransportModes.Sea;
				dec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "L04");
				AssertEquals("SupplierEquipment", "L04", dec.GetSupplierEquipment_Exposed());
			}
			using (Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.DataType.SuspendValidation())
			{
				dec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				AssertNull("SupplierEquipment", dec.GetSupplierEquipment_Exposed());
				Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "F04");
				AssertEquals("SupplierEquipment", "F04", dec.GetSupplierEquipment_Exposed());
			}
		}

		public void TestDefaultCartageEquipment()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();
			var mainAddress = orgHeader.Addresses.AddNewMainAddress();
			mainAddress.OA_AIREquipmentNeeded = "ABC";
			mainAddress.OA_FCLEquipmentNeeded = "DEF";
			mainAddress.OA_LCLEquipmentNeeded = "GHI";

			var secondAddress = orgHeader.Addresses.AddNew();
			secondAddress.OA_AIREquipmentNeeded = "JKL";
			secondAddress.OA_FCLEquipmentNeeded = "MNO";
			secondAddress.OA_LCLEquipmentNeeded = "PQR";

			// Import air
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = ImportMessageTypeForTest;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeader2.Addresses.AddNewMainAddress().PK;
			declaration.SupplierPickupAddress.E2_OA_Address = declaration.SupplierDocumentaryAddress.E2_OA_Address;
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			declaration.ImporterDeliveryAddress.E2_OA_Address = mainAddress.PK;
			declaration.JE_MessageType = ImportMessageTypeForTest;
			AssertEquals("Pre-req - this needs to be an import dec. IsImport=true", true, declaration.IsImport);
			AssertEquals("ABC", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			mainAddress.OA_AIREquipmentNeeded = "";
			declaration.DefaultCartageEquipment();
			AssertEquals("XXX", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			//Export air
			declaration.JE_MessageType = ExportMessageTypeForTest;
			declaration.JE_TransportMode = "AIR";
			AssertEquals("Pre-req - this needs to be an export dec. IsExport=true", true, declaration.IsExport);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			declaration.SupplierPickupAddress.E2_OA_Address = secondAddress.PK;
			AssertEquals("JKL", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			secondAddress.OA_AIREquipmentNeeded = "";
			declaration.DefaultCartageEquipment();
			AssertEquals("XXX", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			// Export FCL
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			declaration.JE_TransportMode = "SEA";
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			declaration.SupplierPickupAddress.E2_OA_Address = mainAddress.PK;
			AssertEquals("DEF", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			mainAddress.OA_FCLEquipmentNeeded = "";
			declaration.DefaultCartageEquipment();
			AssertEquals("XXX", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			// Export LCL
			shipment.JS_PackingMode = "LCL";
			container.CO_FCL_LCL_AIR = "LCL";
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			declaration.SupplierPickupAddress.E2_OA_Address = secondAddress.PK;
			AssertEquals("PQR", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "XXX";
			secondAddress.OA_LCLEquipmentNeeded = "";
			declaration.DefaultCartageEquipment();
			AssertEquals("XXX", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);

			// Test that changing the mode of a container forces a recalculation of drop mode without needing to change address
			container.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("MNO", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
		}

		public virtual void TestDoNotDefaultCartageEquipmentWhenObjectIsInitialised()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = orgHeader.Addresses.AddNewMainAddress();
			mainAddress.OA_AIREquipmentNeeded = "ABC";
			mainAddress.OA_FCLEquipmentNeeded = "DEF";
			mainAddress.OA_LCLEquipmentNeeded = "GHI";

			// Import air
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = ImportMessageTypeForTest;
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MessageType = ImportMessageTypeForTest;
			AssertEquals("Pre-req - this needs to be an import dec. IsImport=true", true, declaration.IsImport);
			declaration.JE_OH_Supplier = orgHeader2.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeader2.Addresses.AddNewMainAddress().PK;
			declaration.SupplierPickupAddress.E2_OA_Address = declaration.SupplierDocumentaryAddress.E2_OA_Address;
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;
			declaration.ImporterDeliveryAddress.E2_OA_Address = mainAddress.PK;
			AssertEquals("ABC", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "DEF";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			var initialised = declarationLoaded.Supplier;
			var initialised2 = declarationLoaded.SupplierDocumentaryAddress;
			AssertEquals("Should not attempt to recalculate", "DEF", declarationLoaded.JE_FCLDeliveryOrPickupEquipmentNeeded);
		}

		protected virtual ZString ImportMessageTypeForTest { get { return "IMP"; } }
		protected virtual ZString ExportMessageTypeForTest { get { return "EXP"; } }

		public void TestHumanReadableName()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "S666666";
			declaration.JE_MasterBill = "MB666666";
			declaration.JE_HouseBill = "HB666666";
			AssertEquals("Declaration S666666", declaration.HumanReadableName);

			declaration.JE_DeclarationReference = "";
			AssertEquals("Declaration (Master Bill='MB666666' House Bill='HB666666')", declaration.HumanReadableName);

			declaration.JE_MasterBill = "";
			AssertEquals("Declaration (House Bill='HB666666')", declaration.HumanReadableName);

			declaration.JE_MasterBill = "MB666666";
			declaration.JE_HouseBill = "";
			AssertEquals("Declaration (Master Bill='MB666666')", declaration.HumanReadableName);
		}

		public void TestRecoverFromUnsuccessfulSave()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.OH_Code = "FRED";
			importer.MiscServ.OM_IMAutoImpJobRefered = true;
			TestDec.JE_OH_Importer = importer.PK;
			CargoWise.Data.Db.Connection.BeginTransaction();
			TestDec.PopulateJE_DeclarationReferenceIfNeeded();
			TestDec.PopulateJE_OwnerRefIfNeeded();
			CargoWise.Data.Db.Connection.CommitTransaction();
			TestDec.JE_MessageStatus = "XXX";
			TestDec.JE_ConsolidatedCargoStatus = "YYY";
			Assert(!TestDec.JE_DeclarationReference.IsEmpty);
			Assert(!TestDec.JE_OwnerRef.IsEmpty);
			Assert(!TestDec.JE_MessageStatus.IsEmpty);
			Assert(!TestDec.JE_ConsolidatedCargoStatus.IsEmpty);
			TestDec.RecoverFromUnsuccessfulSave();
			Assert(TestDec.JE_DeclarationReference.IsEmpty);
			Assert(TestDec.JE_OwnerRef.IsEmpty);
			Assert(TestDec.JE_MessageStatus.IsEmpty);
			Assert(TestDec.JE_ConsolidatedCargoStatus.IsEmpty);
		}

		public void TestDeleteAnyNewMessages()
		{
			var testDec = Factory.New<CanNotBeSavedDeclaration>();
			testDec.CanNotBeSaved = false;
			var message1 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message1);
			message1.EM_ReceiveTransmit = "RCV";
			Factory.Save();
			var message2 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message2);
			message2.EM_ReceiveTransmit = "TRX";
			var message3 = Factory.New<EDIMessage>();
			testDec.Messages.Add(message3);
			message3.EM_ReceiveTransmit = "RCV";
			AssertEquals(3, testDec.Messages.Count);
			Assert(!message1.IsDeleted);
			Assert(!message2.IsDeleted);
			Assert(!message3.IsDeleted);
			testDec.JE_TransportMode = "ABC";
			testDec.CanNotBeSaved = true;
			AssertExceptionThrown<Exception>(() =>
			{
				Factory.Save();
			});
			AssertEquals(2, testDec.Messages.Count);
			Assert("Only outgoing message will be deleted", !message1.IsDeleted);
			Assert("Only outgoing message will be deleted", message2.IsDeleted);
			Assert("Only outgoing message will be deleted", !message3.IsDeleted);
		}

		public void Test_ShouldDeclarationReferenceBePopulatedFromXml()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.ShouldDeclarationReferenceBePopulatedFromXml = true;
			declaration.JE_DeclarationReference = "AgentReference";
			Factory.Save();
			AssertEquals("AgentReference", declaration.JE_DeclarationReference);

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.ShouldDeclarationReferenceBePopulatedFromXml = false;
			declaration2.JE_DeclarationReference = "";
			Factory.Save();
			Assert(!declaration.JE_DeclarationReference.IsEmpty);
		}

		public virtual void TestBrokerName()
		{
			GlbStaff staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "JS";
			staffMember1.GS_FullName = "John Smith";
			GlbStaff staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "JC";
			staffMember2.GS_FullName = "Jinlee Chow";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Broker Name", ZString.Empty, declaration.BrokerName);

			declaration.JE_GS_NKCusAgent = staffMember1.GS_Code;
			AssertEquals("Broker Name", "John Smith", declaration.BrokerName);

			declaration.JE_GS_NKCusAgent = staffMember2.GS_Code;
			AssertEquals("Broker Name", "Jinlee Chow", declaration.BrokerName);
		}

		public virtual void TestOrganisationNames()
		{
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_Code = "consignee";
			consignee.OH_FullName = "Importer-Consignee Co. Pty. Ltd.";
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_Code = "consignor";
			consignor.OH_FullName = "Supplier-Consignor Co. Pte.";
			OrgHeader forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_IsForwarder = ZBool.True;
			forwarder.OH_Code = "forwarder";
			forwarder.OH_FullName = "Lets Move Freight P/L.";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Importer Name", ZString.Empty, declaration.ImporterName);
			AssertEquals("Supplier Name", ZString.Empty, declaration.SupplierName);
			AssertEquals("Forwarder Name", ZString.Empty, declaration.ForwarderName);

			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("Importer Name", "Importer-Consignee Co. Pty. Ltd.", declaration.ImporterName);
			AssertEquals("Supplier Name", "Supplier-Consignor Co. Pte.", declaration.SupplierName);
			AssertEquals("Forwarder Name", "Lets Move Freight P/L.", declaration.ForwarderName);
		}

		public virtual void TestJE_DateOfArrivalCaption()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DateOfArrivalInfo);
				AssertEquals("Caption", "Arrival", resourceStringData.Caption);
				AssertEquals("ShortCaption", "Arr.", resourceStringData.ShortCaption);
			});
		}

		public virtual void TestMasterBillLabel()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var dataBoundBO = new DataBoundBusinessObject(declaration);
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, dataBoundBO);
			AssertEquals("Air Master Bill Label", "Master Bill", resourceStringData.Caption);
			AssertEquals("Air Master Full", "Master Bill of the consignment.", resourceStringData.FullDescription);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, dataBoundBO);
			AssertEquals("Sea Master Bill Label", "Ocean Bill", resourceStringData.Caption);
			AssertEquals("Sea Master Full", "Ocean Bill of the consignment.", resourceStringData.FullDescription);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, dataBoundBO);
			AssertEquals("Other Master Bill Label", "Master Bill", resourceStringData.Caption);
		}

		public virtual void TestHouseBillLabel()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_HouseBillInfo);
			AssertEquals("Air House Bill Label", "House Bill", resourceStringData.Caption);
		}

		[ExpectNoExceptions]
		public void TestSupplierDocumentaryAddressAndSupplierPickupAddressNotThrowExceptionIfDeleted()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			AssertNotNull(declaration.SupplierDocumentaryAddress);
			AssertNotNull(declaration.SupplierPickupAddress);

			Factory.Save();
			declaration.SupplierDocumentaryAddress.E2_City = "Hello City";
			declaration.SupplierPickupAddress.E2_City = "Hello City";
		}

		public void TestGetExportStatement_ReturnEmpty()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("IsExport", true, declaration.IsExport);
			AssertEquals("GetExportStatement for null paramenter", "", declaration.GetExportStatement(null));
			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = "ZZ";
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "ST1";
			statementSetting.Statement = "TESTING STATEMENT";
			statementSetting.Field1 = "FL1";
			statementSetting.Field2 = "FL2";
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("IsExport", false, declaration.IsExport);
			AssertEquals("GetExportStatement for Import Message", "", declaration.GetExportStatement(statementSetting));
		}

		public virtual void TestGetExportStatement()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = DefaultExportMessageType;
			CountryExportStatementSetting countrySetting = new CountryExportStatementSetting();
			countrySetting.CountryCode = "ZZ";
			ExportStatementSetting statementSetting = countrySetting.Statements.AddNew();
			statementSetting.Code = "ST1";
			statementSetting.Statement = "TESTING STATEMENT";
			statementSetting.Field1 = "FL1";
			statementSetting.Field2 = "FL2";
			AssertMultilineASCIIEquals("GetExportStatement", new ExportStatementCreator(statementSetting).ExportStatement, declaration.GetExportStatement(statementSetting));
		}

		public virtual void TestCreateConsolBillDetails()
		{
			const string port1 = "USNYC";
			const string port2 = "DEAAA";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				AssertEquals("It should be null", null, declaration.RelevantConsol);
				var billDetails = declaration.CreateConsolBillDetails(declaration.RelevantConsol);
				AssertEquals("It should be null", null, billDetails);

				var consol = shipment.Consols.AddNew();
				consol.JK_RL_NKLoadPort = port1;
				consol.JK_RL_NKDischargePort = port2;
				AssertEquals("US Import Relevant Consol: DEAAA - USNYC", consol, declaration.RelevantConsol);
				billDetails = declaration.CreateConsolBillDetails(declaration.RelevantConsol);
				AssertEquals("It should return consol", consol, billDetails);
			}
		}

		public void TestLightValidationForDifferentMessageTypesAndTransportModes()
		{
			new LightValidationForDifferentMessageTypesAndTransportModesTester(
				delegate
				{
					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					return (BaseJobDeclaration)GetNewBusinessObjectForDeleteTest(newFactory);
				},
				tester => TestLightValidation(tester.Declaration),
				declaration => AfterInitialise(declaration)
			).Test();
		}

		protected virtual void AfterInitialise(BaseJobDeclaration declaration)
		{
		}

		public virtual void TestTurningOverrideFreightDefaultOffRemovesHouseBillsThatAreNotMatched()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_HouseBill = "HBL1";

			AssertEquals("There should be no house bill", 0, declaration.Bills.Count);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("There should be one house bill", 1, declaration.Bills.Count);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("There should be one house bill", 1, declaration.Bills.Count);
			var bill = declaration.Bills[0];
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "HBL2";

			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals("synchronised house bill 1 should not be deleted", false, bill.IsDeleted);
			AssertEquals("synchronised house bill 2 should be deleted", true, bill2.IsDeleted);
			AssertEquals("There should be one house bill", 1, declaration.Bills.Count);
			AssertEquals(bill, declaration.Bills[0]);
		}

		public void TestChangingOverrideFreightDefaultRefreshBindingOfHouseBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			bool houseBillListChangedCalled = false;
			declaration.Bills.ListChanged += new System.ComponentModel.ListChangedEventHandler(delegate
			{ houseBillListChangedCalled = true; });

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, houseBillListChangedCalled);
		}

		public void TestBranchPK()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			dec.JE_GB = ZGuid.Empty;
			AssertEquals("IHaveInternalCartage.BranchPK.IsEmpty", true, ((IHaveInternalCartage)dec).BranchPK.IsEmpty);
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch = company.Branches.AddNew();
			dec.JE_GB = branch.PK;
			AssertEquals("IHaveInternalCartage.BranchPK.IsEmpty", branch.PK, ((IHaveInternalCartage)dec).BranchPK);
			dec.JE_GB = ZGuid.Empty;
			AssertEquals("IHaveInternalCartage.BranchPK.IsEmpty", true, ((IHaveInternalCartage)dec).BranchPK.IsEmpty);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			IDocAddresses iDec = dec;
			SecurityCheckpoint checkpoint1 = iDec.GetCanOverrideCheckpoint(dec.SupplierDocumentaryAddress);
			SecurityCheckpoint checkpoint2 = iDec.GetCanOverrideCheckpoint(dec.SupplierPickupAddress);
			SecurityCheckpoint checkpoint3 = iDec.GetCanOverrideCheckpoint(dec.ImporterDeliveryAddress);
			SecurityCheckpoint checkpoint4 = iDec.GetCanOverrideCheckpoint(dec.ImporterDocumentaryAddress);
			SecurityCheckpoint checkpoint5 = iDec.GetCanOverrideCheckpoint(dec.ClientPickupDeliveryAddress);

			AssertEquals(Env.Security.None, checkpoint1);
			AssertEquals(Env.Security.None, checkpoint2);
			AssertEquals(Env.Security.None, checkpoint3);
			AssertEquals(Env.Security.None, checkpoint4);
			AssertEquals(Env.Security.None, checkpoint5);
		}

		public virtual void TestSetSynchroniserFieldsReadOnly()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			Assert(!declaration.Packages.ReadOnly);
			declaration.SetSynchroniserFieldsReadOnly(true);
			Assert(declaration.Packages.ReadOnly);
			declaration.SetSynchroniserFieldsReadOnly(false);
			Assert(!declaration.Packages.ReadOnly);
		}

		public void TestSelectAndUnselectContainerForAllInvoiceLines()
		{
			var container1 = TestDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OLCU0000001";
			container1.CO_Seal = "CUCKOO";

			var container2 = TestDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OLCU0000002";
			container2.CO_Seal = "CUCKOO";

			var line1 = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var line2 = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			var line3 = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();

			var containersForInvoiceLines = line1.ContainersForInvoiceLinesForBindingOnly;
			var nonPersistentContainer1ForLine1 = containersForInvoiceLines[0];
			var nonPersistentContainer2ForLine1 = containersForInvoiceLines[1];

			containersForInvoiceLines = line2.ContainersForInvoiceLinesForBindingOnly;
			var nonPersistentContainer1ForLine2 = containersForInvoiceLines[0];
			var nonPersistentContainer2ForLine2 = containersForInvoiceLines[1];

			containersForInvoiceLines = line3.ContainersForInvoiceLinesForBindingOnly;
			var nonPersistentContainer1ForLine3 = containersForInvoiceLines[0];
			var nonPersistentContainer2ForLine3 = containersForInvoiceLines[1];

			//Container 1
			AssertEquals("Should not be selected", false, nonPersistentContainer1ForLine1.IsForInvoiceLine);
			AssertEquals("Should not be selected", false, nonPersistentContainer1ForLine2.IsForInvoiceLine);
			AssertEquals("Should not be selected", false, nonPersistentContainer1ForLine3.IsForInvoiceLine);
			//Container 2
			AssertEquals("Should not be selected", false, nonPersistentContainer2ForLine1.IsForInvoiceLine);
			AssertEquals("Should not be selected", false, nonPersistentContainer2ForLine2.IsForInvoiceLine);
			AssertEquals("Should not be selected", false, nonPersistentContainer2ForLine3.IsForInvoiceLine);

			TestDec.SelectContainerForAllInvoiceLines(nonPersistentContainer1ForLine1);
			containersForInvoiceLines = line1.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should be selected", true, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);

			containersForInvoiceLines = line2.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should be selected", true, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);

			containersForInvoiceLines = line3.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should be selected", true, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);

			TestDec.UnSelectContainerForAllInvoiceLines(nonPersistentContainer1ForLine2);
			containersForInvoiceLines = line1.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should not be selected", false, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);

			containersForInvoiceLines = line2.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should not be selected", false, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);

			containersForInvoiceLines = line3.ContainersForInvoiceLinesForBindingOnly;
			AssertEquals("Container1 should not be selected", false, containersForInvoiceLines.FindByContainer(container1).IsForInvoiceLine);
			AssertEquals("Container2 should not be selected", false, containersForInvoiceLines.FindByContainer(container2).IsForInvoiceLine);
		}

		public void TestCountry()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, dec.Country.Code);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, dec.CountryCode);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrowIsThereAtLeastOneInvoiceHEaderAndOneInvoiceLinePerInvoiceHeaderWhenNotHeaders()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			AssertNotNull("JobComInvoiceGroupHeaders is not null", topGroup);
			testDec.ActiveGroupHeader.RemoveAndDeleteAll();
			AssertEquals("NoExceptionThrown when JobComInvoiceGroupHeaders has no invoiceHeader", false, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);
		}

		public void TestIsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();

			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			BaseJobComInvoiceHeader inv1 = subGroup1.JobComInvoiceHeaders.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			inv1.JobComInvoiceLines.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", true, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			BaseJobComInvoiceHeader inv2 = subGroup1.JobComInvoiceHeaders.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			inv2.JobComInvoiceLines.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", true, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			BaseJobComInvoiceHeader inv3 = subGroup2.JobComInvoiceHeaders.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", false, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);

			inv3.JobComInvoiceLines.AddNew();
			AssertEquals("IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader", true, testDec.IsThereAtLeastOneInvoiceHeaderAndOneInvoiceLinePerInvoiceHeader);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_DeclarationReference = "B00001111";

			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_IsDebtor = ZBool.True;
			debtor.OH_Code = "DEBTOR";

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_OH = debtor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_ConsolidatedInvoiceRef = jobDec.JE_DeclarationReference;

			// shouldn't be included in the related objects
			AccTransactionHeader nonMatchingInvoice = Factory.New<AccTransactionHeader>();
			nonMatchingInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			nonMatchingInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			nonMatchingInvoice.AH_OH = debtor.PK;
			nonMatchingInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			Assert("Should be something in the related logs", jobDec.BusinessObjectsWithRelatedEvents.Length > 0);
			Assert("Matching invoice should appear in the list of related objects", ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(invoice));
			Assert("Non Matching invoice shouldn't appear in the list of related objects", !((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(nonMatchingInvoice));

			var invoiceHeader1 = jobDec.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			Assert("InvoiceHeader1 should be in BusinessObjectWithRelatedLogs", ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(invoiceHeader1));
			Assert("InvoiceLine1 should be in BusinessObjectWithRelatedLogs", ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(invoiceLine1));
		}

		public void TestBusinessObjectsWithRelatedEventsForCusEntryHeader()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_DeclarationReference = "B00001111";

			CusEntryHeader entryHeader = jobDec.CustomsEntryHeaders.AddNew();
			AssertEquals("Related Logs has CusEntryHeader", true, ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(entryHeader));
		}

		public void TestBusinessObjectsWithRelatedEventsForJobContainer()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_DeclarationReference = "B00001111";
			var container = jobDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "WITHEVENTS";

			AssertEquals("Related Logs has JobContainer", true, ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(container.JobContainer));
		}

		public void TestBusinessObjectsWithRelatedEventsForJobHeader()
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_DeclarationReference = "B00001111";

			var jobInvoicing = Factory.NewJobForTesting<JobHeader>();
			jobInvoicing[JobHeaderSchema.JH_GC.Name] = GlbCompany.CurrentCompany.PK;
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = jobDec.PK;

			AssertEquals("Related Logs has JobHeader", true, ((IList)jobDec.BusinessObjectsWithRelatedEvents).Contains(jobDec.Job));
		}

		public void TestAddCargoAvailableEventToDeclarationIfAvailabilityDateHasChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MergeBy = JobMessageTypeList.Codes.Import;
			var msCAV = declaration.WorkflowItems.Milestones.AddNew();
			msCAV.TriggerConditions.TriggerEventCode = Events.CargoAvailableCode;
			AssertEquals("Precondition: CAV Milestone Actual Date", ZDateTime.Empty, msCAV.P9_ActualDate);
			Factory.Save();

			declaration.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2016, 1, 1);
			Factory.Save();
			AssertEquals("CAV Event Time", new ZDateTime(2016, 1, 1), declaration.Logs.MostRecentLogByEventTime(Events.CargoAvailable).SL_EventTime);
			AssertEquals("CAV Milestone Actual Date", new ZDateTime(2016, 1, 1), msCAV.P9_ActualDate);

			declaration.DocsAndCartage.JP_LCLAvailable = new ZDateTime(2016, 1, 2);
			Factory.Save();
			AssertEquals("CAV Event Time", new ZDateTime(2016, 1, 2), declaration.Logs.MostRecentLogByEventTime(Events.CargoAvailable).SL_EventTime);
			AssertEquals("CAV Milestone Actual Date didn't change because milestones should only trigger once.", new ZDateTime(2016, 1, 1), msCAV.P9_ActualDate);
		}

		public void TestNoAutoAddOfInvoiceHeaderForExWarehouseWhenCopying()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			((IBusinessObjectInternals)declaration).IsCopying = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0, declaration.Invoices.Count);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(0, declaration.Invoices.Count);
		}

		public virtual void TestAutoAddOfInvoiceHeaderWhenImportThenChangedToExWarehouse()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(0, declaration.Invoices.Count);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(GetAutoCreatedExBondInvoiceNumber(), declaration.Invoices[0].JZ_InvoiceNumber);
		}

		public virtual void TestAutoAddOfInvoiceHeaderWhenExWarehouse()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(1, declaration.Invoices.Count);
			declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(GetAutoCreatedExBondInvoiceNumber(), declaration.Invoices[0].JZ_InvoiceNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Doesn't get deleted", 1, declaration.Invoices.Count);
			AssertEquals(GetAutoCreatedExBondInvoiceNumber(), declaration.Invoices[0].JZ_InvoiceNumber);
		}

		protected virtual string GetAutoCreatedExBondInvoiceNumber()
		{
			return "EX-BOND";
		}

		public virtual void TestNoStackOverflowWhenAddingToExWarehouseInvoiceLinesWithAutoAddOfHeader()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, declaration.FilteredInvoiceLines.Count);
			AssertEquals(1, declaration.Invoices.Count);
			AssertEquals(GetAutoCreatedExBondInvoiceNumber(), declaration.FilteredInvoiceLines[0].InvoiceHeader.JZ_InvoiceNumber);
		}

		public void TestAllEntriesCleared()
		{
			var declaration = JobDeclarationForAllEntriesClearedTest;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("Empty collection", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals(false, declaration.HaveAllEntriesCleared);

			var mock = Factory.NewMoq<CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 2, 1));
			declaration.CustomsEntryHeaders.Add(mock.Object);
			AssertEquals(true, declaration.HaveAllEntriesCleared);

			mock = Factory.NewMoq<CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			declaration.CustomsEntryHeaders.Add(mock.Object);
			AssertEquals(false, declaration.HaveAllEntriesCleared);
		}

		protected virtual T JobDeclarationForAllEntriesClearedTest => (T)GetNewBusinessObject();

		public virtual void TestBGMReferences()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("", declaration.BGMReferences);

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "BGM456";

			AssertEquals("BGM456", declaration.BGMReferences);

			CusEntryHeader entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BGM123";

			declaration.ActiveEntryHeaders.Sort(CusEntryHeader.Schema.CH_BGMReference);
			AssertEquals("BGM123, BGM456", declaration.BGMReferences);
		}

		public virtual void TestSwitchFromSeaToAirWithContainersDereferencesAndDeletesContainers()
		{
			BaseJobDeclaration jobDec = GetJobDeclaration();
			jobDec.FillWithValidTestData();
			jobDec.JE_TransportMode = Constants.TransportModes.Sea;

			BaseCusContainer container = jobDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123";

			Bill houseBill = jobDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HB123";

			BasePackingGroup pack = (jobDec.PackingGroups.Count > 0) ? jobDec.PackingGroups[0] : jobDec.PackingGroups.AddNew();
			pack.CR_CO_Container = container.PK;
			pack.CR_CU_HouseBill = houseBill.PK;

			Factory.Save();

			jobDec.JE_TransportMode = jobDec.TransportModeAirCodeForTesting;

			Factory.Save();

			if (jobDec.ContainersRequired)
			{
				AssertEquals("Containers were deleted or rereferenced on change to Air", 1, jobDec.CusContainers.Count);
			}
			else
			{
				Assert(pack.IsDeleted);
				AssertEquals("Containers were not deleted or rereferenced on change to Air", 0, jobDec.CusContainers.Count);
			}
		}

		#region Sailing Schedule

		public void TestVoyage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Vessel";
			declaration.JE_VoyageFlightNo = "Voyage";
			AssertEquals("Voyage should match correctly", Voyage, declaration.Voyage);
		}

		public void TestVoyage_Sea_ShippingLineIsUsedForMatching()
		{
			var shippingLine1 = Factory.New<OrgHeader>();
			var shippingLine2 = Factory.New<OrgHeader>();

			var helper = new VoyageTestHelper(Factory);
			var voyage1 = helper.CreateSeaVoyage("Visund", "123", shippingLine1.PK);
			var voyage2 = helper.CreateSeaVoyage("Visund", "123", shippingLine2.PK);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = "Visund";
			declaration.JE_VoyageFlightNo = "123";
			declaration.JE_OH_ShippingLine = shippingLine1.PK;
			AssertEquals(voyage1, declaration.Voyage);

			declaration.JE_OH_ShippingLine = shippingLine2.PK;
			AssertEquals(voyage2, declaration.Voyage);
		}

		public void TestVoyage_ForAir()
		{
			JobVoyage decoyVoyage = Voyage2;
			decoyVoyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			decoyVoyage.JV_FlightDate = ZDateTime.Now.AddDays(2);

			Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			Voyage.JV_FlightDate = ZDateTime.Now;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = Voyage.JV_VoyageFlight;
			declaration.JE_VesselName = Voyage.JV_RV_NKVessel;
			declaration.JE_ExportDate = ZDateTime.Now;
			AssertEquals("The flight with the closest departure date should be used", Voyage, declaration.Voyage);
		}

		public void TestLoadingVoyageOrigin()
		{
			VoyageOrigin decoyOrigin = Voyage.Origins.AddNew();
			decoyOrigin.JA_RL_NKPortOfLoading = "USLAX";
			VoyageOrigin origin = Voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "MYPKG";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = Voyage.JV_RV_NKVessel;
			declaration.JE_VoyageFlightNo = Voyage.JV_VoyageFlight;
			declaration.JE_RL_NKPortOfLoading = "MYPKG";
			AssertEquals(origin, declaration.LoadingVoyageOrigin);
		}

		public void TestDischargeVoyageDestination()
		{
			VoyageDestination decoyDestination = Voyage.Destinations.AddNew();
			decoyDestination.JB_RL_NKPortOfDischarge = "AUPER";
			VoyageDestination destination = Voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = Voyage.JV_RV_NKVessel;
			declaration.JE_VoyageFlightNo = Voyage.JV_VoyageFlight;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUPER";
			AssertEquals(destination, declaration.DischargeVoyageDestination);
		}

		public virtual void TestFirstArrivalVoyageDestination()
		{
			VoyageDestination decoyDestination = Voyage.Destinations.AddNew();
			decoyDestination.JB_RL_NKPortOfDischarge = "AUPER";
			VoyageDestination destination = Voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = Voyage.JV_RV_NKVessel;
			declaration.JE_VoyageFlightNo = Voyage.JV_VoyageFlight;
			declaration.JE_RL_NKPortOfArrival = "AUPER";
			declaration.JE_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals(destination, declaration.FirstArrivalVoyageDestination);
		}

		public void TestFirstArrivalVoyageDestination_WhenPortOfFirstArrivalNotSet()
		{
			VoyageDestination decoyDestination = Voyage.Destinations.AddNew();
			decoyDestination.JB_RL_NKPortOfDischarge = "AUPER";
			VoyageDestination destination = Voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = Voyage.JV_RV_NKVessel;
			declaration.JE_VoyageFlightNo = Voyage.JV_VoyageFlight;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "";
			AssertEquals(destination, declaration.FirstArrivalVoyageDestination);
		}

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel";

					voyage = Factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "Voyage";
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		JobVoyage Voyage2
		{
			get
			{
				if (voyage2 == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel";

					voyage2 = Factory.New<JobVoyage>();
					voyage2.JV_RV_NKVessel = vessel.RV_FK;
					voyage2.JV_VoyageFlight = "Voyage";
				}
				return voyage2;
			}
		}
		JobVoyage voyage2;

		#endregion

		public void TestIDeclarationProvider()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			AssertEquals(dec, ((IDeclarationProvider)dec).Declaration);
		}

		public void TestJE_MasterBill()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_MasterBill = "1234 56789";
			AssertEquals("Master bill", "1234 56789", testDec.JE_MasterBill);

			testDec.JE_MasterBill = "1234-56789";
			AssertEquals("Master bill", "1234-56789", testDec.JE_MasterBill);

			var testDecCopy = Factory.New<BaseJobDeclaration>();
			testDecCopy.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
			testDecCopy.CopyPersistentValuesFrom(testDec);
			AssertEquals("Master bill remains unchanged when copying", "1234-56789", testDecCopy.JE_MasterBill);

			testDec.JE_TransportMode = testDec.TransportModeAirCodeForTesting;
			testDec.JE_MasterBill = "1234 56789";
			AssertEquals("Master bill updated when setting and transport is Air", "123456789", testDec.JE_MasterBill);

			testDec.JE_MasterBill = "1234-56789";
			AssertEquals("Master bill", "123456789", testDec.JE_MasterBill);
		}

		#region Property Overrides

		public void TestJE_DateAtOrigin_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.CO_ContainerNumber = "Container1";
				Container2.CO_ContainerNumber = "Container2";

				Declaration.JE_DateAtOrigin = ZDateTime.Now;
				AssertEquals("Both containers should be notified of the ETD change", 2, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container1.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container2.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyETDChangedCalledForContainers[1].PK);
			}
		}

		public void TestJE_DateAtFinalDestination_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.CO_ContainerNumber = "Container1";
				Container2.CO_ContainerNumber = "Container2";

				Declaration.JE_DateAtFinalDestination = ZDateTime.Now;
				AssertEquals("Both containers should be notified of the ETA change", 2, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container1.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETA change for each container", Container2.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyETAChangedCalledForContainers[1].PK);
			}
		}

		public void TestJE_VesselName_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.CO_ContainerNumber = "Container1";
				Container2.CO_ContainerNumber = "Container2";

				Declaration.JE_VesselName = "DIRECT CONDOR";
				AssertEquals("Both containers should be notified of the ETD change", 2, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container1.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container2.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyVesselChangedCalledForContainers[1].PK);
			}
		}

		public void TestJE_VoyageFlightNo_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.CO_ContainerNumber = "Container1";
				Container2.CO_ContainerNumber = "Container2";

				Declaration.JE_VoyageFlightNo = "123";
				AssertEquals("Both containers should be notified of the ETD change", 2, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container1.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container2.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyVoyageChangedCalledForContainers[1].PK);
			}
		}

		public void TestJE_RL_NKPortOfArrival_ContainerEventDataVendorNotified_ForEachContainer()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container1.CO_ContainerNumber = "Container1";
				Container2.CO_ContainerNumber = "Container2";

				Declaration.JE_RL_NKPortOfArrival = "AUSYD";
				AssertEquals("Both containers should be notified of the ETD change", 2, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers.Count);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container1.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers[0].PK);
				AssertEquals("ContainerEventDataVendor must be notified of the ETD change for each container", Container2.JobContainer.PK, MockContainerEventDataVendor.Instance.NotifyDischargePortChangedCalledForContainers[1].PK);
			}
		}

		BaseJobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration declaration;

		BaseCusContainer Container1
		{
			get { return container1 ?? (container1 = Declaration.CusContainers.AddNew()); }
		}
		BaseCusContainer container1;

		BaseCusContainer Container2
		{
			get { return container2 ?? (container2 = Declaration.CusContainers.AddNew()); }
		}
		BaseCusContainer container2;

		#endregion

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration result = (BaseJobDeclaration)base.GetNewBusinessObjectForDeleteTest(factory);
			Bill bill = result.Bills.AddNew();
			bill.PackingGroups.AddNew();
			result.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return result;
		}

		#region TestMergeByDefaultsFromClientWhenClientChanges

		public virtual void TestMergeByDefaultsFromClientWhenClientChanges()
		{
			TestDec.DisableMessageTypeChangeOnSupplierChangeForTesting = true;
			TestDec.JE_MessageType = DefaultExportMessageType;
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "Importer";
			importer.OH_RL_NKClosestPort = TestDec.CountryCode + "XXX";
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = MergeType1;

			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_FullName = "Supplier";
			supplier.OH_RL_NKClosestPort = TestDec.CountryCode + "XXX";
			supplier.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "DEF";

			AssertEquals("Declaration.JE_MergeBy", DefaultMergeType, TestDec.JE_MergeBy);

			Env.Registry.SetCommercialInvoiceLineMergeMethod(GlbCompany.CurrentCompany.PK.ToGuid(), MergeType2);

			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_OH_Importer = importer.PK;

			//export so from supplier
			AssertEquals("Declaration.JE_MergeBy", MergeType2, TestDec.JE_MergeBy);

			TestDec.JE_OH_Supplier = ZGuid.Empty;
			TestDec.JE_OH_Importer = ZGuid.Empty;

			TestDec.JE_MessageType = DefaultImportMessageType;

			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_OH_Importer = importer.PK;

			AssertEquals("Declaration.JE_MergeBy", MergeType1, TestDec.JE_MergeBy);
		}

		protected virtual string DefaultMergeType
		{
			get { return "TRF"; }
		}

		protected virtual string MergeType1
		{
			get { return OrgConstants.MergeInvoiceLines.PartNumber; }
		}

		protected virtual string MergeType2
		{
			get { return OrgConstants.MergeInvoiceLines.TariffAndDescription; }
		}

		#endregion

		public void TestIInvoiceLinkProvider()
		{
			IInvoiceLinkProvider call1 = TestDec;
			IInvoiceLinkProvider call2 = TestDec;

			AssertEquals("Same value returned each call", call1, call2);
			AssertNotNull(call1);
		}

		public void TestMessageSubTypeDescription()
		{
			TestBaseJobDeclaration dec = Factory.New<TestBaseJobDeclaration>();
			dec.JE_MessageSubType = dec.Lookups.MessageSubTypeList[0].Code;
			AssertEquals(dec.Lookups.MessageSubTypeList[0].Description, dec.MessageSubTypeDescription);
		}

		public virtual void TestMessageTypeDescription()
		{
			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals(JobMessageTypeList.Descriptions.Import, TestDec.MessageTypeDescription);
		}

		#region TestDescription
		public virtual void TestDescription()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("CA");
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var descriptionImportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid());
			var importCustomization = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization;
			AssertDescription(importCustomization, descriptionImportCustomizationCollection, declaration);

			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var descriptionExportCustomizationCollection = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid());
			var exportCustomization = CustomsDataRegistry.Instance.DeclarationImportDescriptionCustomization;
			AssertDescription(exportCustomization, descriptionExportCustomizationCollection, declaration);
			GlbCompany.CurrentCompany.SetCountry(country);
		}

		void AssertDescription(CodeDescriptionBoolDisallowNewRegistryItem customization, CodeDescriptionBoolCollection descriptionCustomizationCollection, BaseJobDeclaration jobDeclaration)
		{
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("EXP")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ORF")).Bool = ZBool.False;

			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);

			jobDeclaration.JE_DeclarationReference = "ABC";
			AssertEquals("Default Description ", "ABC", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("JNO")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("MBL")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC", jobDeclaration.Description);
			jobDeclaration.JE_MasterBill = "MBL";
			AssertEquals("ABC - MBL: MBL", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("HBL")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL", jobDeclaration.Description);
			jobDeclaration.JE_HouseBill = "HBL";
			AssertEquals("ABC - MBL: MBL, HBL: HBL", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ENT")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("IMP")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ", jobDeclaration.Description);
			var importer = OrgHeader.New(Factory);
			importer.OH_Code = "Import";
			jobDeclaration.JE_OH_Importer = importer.PK;
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: , IMP: Import", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("EXP")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: , IMP: Import", jobDeclaration.Description);
			var exporter = OrgHeader.New(Factory);
			exporter.OH_Code = "Export";
			jobDeclaration.JE_OH_Supplier = exporter.PK;
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: , IMP: Import, EXP: Export", jobDeclaration.Description);

			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ORF")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: , IMP: Import, EXP: Export", jobDeclaration.Description);
		}
		#endregion

		#region GetAirline
		public void TestSettingJE_HouseBillChangesPresentationRecord()
		{
			Bill bill1 = TestDec.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			Assert("Precondition", bill1.CU_GUIPresentationRecord);
			Bill bill2 = TestDec.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "2";
			AssertEquals("", TestDec.JE_HouseBill);
			TestDec.JE_HouseBill = "2";
			Assert(!bill1.CU_GUIPresentationRecord);
			Assert(bill2.CU_GUIPresentationRecord);
		}

		public void TestChangingFrontHouseBillChangesPresentationRecord()
		{
			Bill bill1 = TestDec.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_HouseBill = "1";
			Bill bill2 = TestDec.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "2";
			Bill bill3 = TestDec.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_HouseBill = "3";
			AssertEquals("1", TestDec.JE_HouseBill);

			TestDec.JE_HouseBill = "2";
			AssertEquals(false, bill3.CU_GUIPresentationRecord);
			AssertEquals(true, bill2.CU_GUIPresentationRecord);
		}

		RefAirline GetAirline()
		{
			RefAirline airline1 = Factory.New<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "9XX";
			airline1.RM_AirlineCountry = "AU";
			airline1.RM_AirlineCity = "Sydney";
			airline1.RM_AirlineName1 = "New Airline";
			airline1.RM_AddressLine1 = "addr";
			airline1.RM_TwoCharacterCode = "XX";

			Factory.Save();
			return airline1;
		}
		#endregion

		public void TestAirLineChangesWhenFlightChanges()
		{
			RefAirline airline = GetAirline();
			ZString airlinePrefix = airline.RM_TwoCharacterCode;

			TestDec.IsImportingData = true;
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertNull(TestDec.Airline);
			TestDec.JE_VoyageFlightNo = airlinePrefix + " 556";
			AssertNotNull(TestDec.Airline);
			TestDec.JE_VoyageFlightNo = "";
			AssertNull(TestDec.Airline);
		}

		public void TestCarrierId()
		{
			Assert(TestDec.CarrierId.IsEmpty);
			TestDec.JE_VoyageFlightNo = "EK 556";
			AssertEquals("EK", TestDec.CarrierId);
		}

		public void TestAirline()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertNull(TestDec.Airline);
			TestDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			TestDec.JE_VoyageFlightNo = GetAirline().RM_TwoCharacterCode + " 123";
			AssertNotNull(TestDec.Airline);
		}

		public void TestAirLineName()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			TestDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Assert(TestDec.AirlineName.IsEmpty);

			RefAirline airline = GetAirline();
			ZString airlineName = airline.RM_AirlineName1;
			ZString airlinePrefix = airline.RM_TwoCharacterCode;

			TestDec.JE_VoyageFlightNo = airlinePrefix + " 789";
			AssertNotNull(TestDec.Airline);
			AssertEquals("New Airline", TestDec.AirlineName);
		}

		public virtual void TestTotalCustomsWeight()
		{
			ZWeight result = new ZWeight(0.0m, "KG");
			AssertEquals(true, result == TestDec.TotalCustomsWeight);

			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_CustomsUnitQty = "KG";
			line1.JI_CustomsQuantity = 100;
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_CustomsUnitQty = "BOX";
			line2.JI_CustomsQuantity = 20;
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_CustomsUnitQty = "T";
			line3.JI_CustomsQuantity = 2;

			result = new ZWeight(2100.0m, "KG");
			AssertEquals(true, result == TestDec.TotalCustomsWeight);
		}

		#region TestOnTransportModeChanged
		public void TestOnTransportModeChanged()
		{
			TestDec.OnTransportModeChanged += new TransportModeChangedEventHandler(Declaration_OnTransportModeChanged);
			try
			{
				TestDec.JE_TransportMode = "ZZZ";
				Assert("Declaration.OnTransportModeChanged should be fired when JE_TransportMode is set", testHitOnTransportModeChanged);
			}
			finally
			{
				TestDec.OnTransportModeChanged -= new TransportModeChangedEventHandler(Declaration_OnTransportModeChanged);
			}
		}
		bool testHitOnTransportModeChanged;
		void Declaration_OnTransportModeChanged()
		{
			testHitOnTransportModeChanged = true;
		}
		#endregion

		public virtual void TestContainersRequiredOnSea()
		{
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			Assert("Precondition - Should not show", !TestDec.ContainersRequired);
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			Assert("Should show", TestDec.ContainersRequired);
		}
		public virtual void TestContainersRequiredOnAir()
		{
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			Assert("Precondition - Should not show", !TestDec.ContainersRequired);
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			Assert("Should not show", !TestDec.ContainersRequired);
		}

		#region TestContainersRequiredOnOverride

		public void TestContainersRequiredOnOverride()
		{
			var mock = Factory.NewMoq<T>();
			mock.Setup(m => m.ContainersAlwaysRequired).Returns(new ZBool(true));
			BaseJobDeclaration declaration = mock.Object;
			AssertEquals("Declaration.ContainersRequired should always be true when ContainersAlwaysRequired is Overridden", true, declaration.ContainersRequired);
			declaration.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertEquals("Declaration.ContainersRequired should always be true when ContainersAlwaysRequired is Overridden", true, declaration.ContainersRequired);
			declaration.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			AssertEquals("Declaration.ContainersRequired should always be true when ContainersAlwaysRequired is Overridden", true, declaration.ContainersRequired);
		}

		public virtual void TestContainersRequiredOnNonTransportDeclarationType()
		{
			var mock = Factory.NewMoq<T>();
			var declaration = mock.Object;
			declaration.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("Pre-condition", true, declaration.ContainersRequired);
			mock.Setup(m => m.IsNonTransportDeclarationType).Returns(true);
			Assert("Should not show", !declaration.ContainersRequired);
		}

		#endregion

		public void TestHasCusContainers()
		{
			AssertEquals("Precondition: No CusContainers Yet", 0, TestDec.CusContainers.Count);
			AssertEquals("HasCusContainers", false, TestDec.HasCusContainers);
			TestDec.CusContainers.AddNew();
			AssertEquals("HasCusContainers", true, TestDec.HasCusContainers);
		}

		public virtual void TestILandedCostHeader()
		{
			BaseJobComInvoiceGroupHeader groupHeader = TestDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = TestDec.FilteredInvoiceLines.AddNew();
			TestDec.CusContainers.AddNew();

			AssertEquals("3 Charge holders", 3, new List<ILandedCostChargeHolder>(((ILandedCostHeader)TestDec).ChargeHolders).Count);
			AssertEquals("Charge holder == GroupHeader", groupHeader, new List<ILandedCostChargeHolder>(((ILandedCostHeader)TestDec).ChargeHolders)[0]);
			AssertEquals("Charge holder == Invoice", invoice, new List<ILandedCostChargeHolder>(((ILandedCostHeader)TestDec).ChargeHolders)[1]);
			AssertEquals("Charge holder == InvoiceLine", invoiceLine, new List<ILandedCostChargeHolder>(((ILandedCostHeader)TestDec).ChargeHolders)[2]);

			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			AssertEquals("IsSea", true, TestDec.IsSea);
			AssertEquals("4 distributees", 4, new List<ILandedCostDistributeTo>(((ILandedCostHeader)TestDec).CandidatesToDistributeCostTo).Count);

			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertEquals("IsSea", false, TestDec.IsSea);
			AssertEquals("3 distributees", 3, new List<ILandedCostDistributeTo>(((ILandedCostHeader)TestDec).CandidatesToDistributeCostTo).Count);
		}

		public void TestJobInvoicing()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;

			var jobInvoicing = Factory.NewJobForTesting<JobHeader>();
			jobInvoicing[JobHeaderSchema.JH_GC.Name] = GlbCompany.CurrentCompany.PK;

			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = shipment.PK;
			AssertEquals("Job Invoicing found attached to shipment", jobInvoicing, testDec.JobInvoicing);

			testDec.JE_JS = ZGuid.Empty;
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = testDec.PK;
			AssertEquals("Job Invoicing found", jobInvoicing, testDec.JobInvoicing);
		}

		public void TestChargeHoldersWhenJobInvoicingForShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_JS = shipment.PK;

			var jobInvoicing = Factory.NewJobForTesting<JobHeader>();
			jobInvoicing[JobHeaderSchema.JH_GC.Name] = GlbCompany.CurrentCompany.PK;
			jobInvoicing[JobHeaderSchema.JH_ParentID.Name] = shipment.PK;

			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			groupHeader.JZ_InvoiceNumber = "1";

			AssertEquals("Three charge holders, two group headers and job invoicing", 3, new List<ILandedCostChargeHolder>(((ILandedCostHeader)testDec).ChargeHolders).Count);
		}

		public void TestPackagesRequiredPackageCount()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_TotalNoOfPacks = 10;
			AssertEquals("Declaration.PackagesRequiredPackageCount", 10, declaration.PackagesRequiredPackageCount);

			declaration.JE_TotalNoOfPacks = 23;
			AssertEquals("Declaration.PackagesRequiredPackageCount", 23, declaration.PackagesRequiredPackageCount);
		}

		public void TestInvoiceLinesGetLoaded()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			BaseJobComInvoiceHeader invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			invoice.JZ_JE = TestDec.PK;
			TestDec.Invoices.Add(invoice);

			AssertEquals("InvoiceLines", 3, TestDec.InvoiceLines.Count);
			AssertEquals("FilteredInvoiceLines", 3, TestDec.FilteredInvoiceLines.Count);
		}

		public void TestHouseBillsCommaSeparated()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			testDec.JE_HouseBill = "HBL1";
			AssertEquals("HBL1", testDec.HouseBillsCommaSeparated);

			Bill bill = testDec.Bills.AddNew();
			bill.CU_HouseBill = "HBL2";
			AssertEquals("HBL1,HBL2", testDec.HouseBillsCommaSeparated);

			testDec.JE_HouseBill = "HBL3";
			AssertEquals("HBL2,HBL3", testDec.HouseBillsCommaSeparated);
		}

		public virtual void TestMasterBillsCommaSeparated()
		{
			TestDec.JE_MasterBill = "MBL1";
			AssertEquals("MBL1", TestDec.MasterBillsCommaSeparated);

			Bill bill = TestDec.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "MBL2";
			AssertEquals("MBL1,MBL2", TestDec.MasterBillsCommaSeparated);

			TestDec.JE_MasterBill = "";
			AssertEquals("MBL2", TestDec.MasterBillsCommaSeparated);
		}

		#region PortDeliveryTime

		public void TestETADoesntGetUpdatedForInvalidATA()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_DateOfArrival = ZDateTime.Invalid;
			Assert("Date At Final Destination should be empty!", declaration.JE_DateAtFinalDestination.IsEmpty);
		}

		public void TestETADoesntDefaultToATAPlusDaysWhenNotImport()
		{
			ZDateTime currentDate = new ZDateTime(2005, 4, 14);
			GlbPortDeliveryTime defaultDelay3 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay3.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay3.G1_RL_NKDischargePort = "AUPER";
			defaultDelay3.G1_RL_NKDestinationPort = "AUSYD";
			defaultDelay3.G1_DaysDelayFromArrivalToDeliver = 10;

			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USSEA";
			declaration.JE_DateOfArrival = currentDate;

			AssertEquals("ETA should not be defaulted at all", ZDateTime.Empty, declaration.JE_DateAtFinalDestination);
		}

		public void TestEstPickupDeliveryOnlyDefaultsForImportsWhenNoPortDeliveryTime_WorkItemW00036899()
		{
			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = nameof(FreightMode.AIR);
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			Assert("Precondition - Estimated pickup or delivery is blank by default", TestDec.JE_EstimatedDeliveryOrPickup.IsEmpty);
			ZDateTime now = ZDateTime.Now;
			TestDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			TestDec.JE_RL_NKPortOfArrival = "USNYC";
			TestDec.JE_RL_NKFinalDestination = "USAAA";
			TestDec.JE_MessageType = DefaultExportMessageType;
			TestDec.JE_DateAtFinalDestination = now;
			Assert("Estimated pickup/delivery should not be defaulted for exports", TestDec.JE_EstimatedDeliveryOrPickup.IsEmpty);

			TestDec.JE_DateAtFinalDestination = TestDec.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			TestDec.JE_DateAtFinalDestination = now;
			Assert("Estimated pickup/delivery should be defaulted for imports", !TestDec.JE_EstimatedDeliveryOrPickup.IsEmpty);
		}

		public void TestETAOnShipmentWhenIsCustomsJob()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 5, 14);

			GlbPortDeliveryTime defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;
			defaultDelay.G1_JobMode = "FWD";

			GlbPortDeliveryTime defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay2.G1_RL_NKDischargePort = "USLAX";
			defaultDelay2.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay2.G1_DaysDelayFromArrivalToDeliver = 5;
			defaultDelay2.G1_JobMode = "CUS";

			BaseJobDeclaration dec = GetJobDeclaration();
			dec.JE_MessageType = DefaultImportMessageType;

			dec.JE_RL_NKOrigin = Enterprise.Core.Constants.CountryCodes.France;
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.NonContainerised;
			dec.JE_RL_NKPortOfArrival = "USLAX";
			dec.JE_RL_NKFinalDestination = "USAAA";
			dec.JE_DateOfArrival = dateOfArrival;

			AssertEquals("ETA should be delayed by 5 days", dateOfArrival.AddDays(5), dec.JE_DateAtFinalDestination);
		}

		#endregion

		#region Get Estimate Date From Transport

		public void TestGetEstimateDateFromDeclarationTransport()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = ZGuid.Empty;

			AssertDateFromTransport(declaration,
				declaration,
				BaseJobDeclaration.Schema.JE_ETAOfDischarge,
				JobDeclarationSchema.JE_RL_NKPortOfArrival,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_RL_NKDiscPort);

			AssertDateFromTransport(declaration,
				declaration,
				BaseJobDeclaration.Schema.JE_ETDOfLoading,
				JobDeclarationSchema.JE_RL_NKPortOfLoading,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_RL_NKLoadPort);
		}

		public void TestGetEstimateDateFromConsolTransport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			shipment.Transports.RemoveAndDeleteAll();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			AssertDateFromTransport(declaration,
				consol,
				BaseJobDeclaration.Schema.JE_ETAOfDischarge,
				JobDeclarationSchema.JE_RL_NKPortOfArrival,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_RL_NKDiscPort);

			var transportOnShipment = shipment.Transports.AddNew();
			transportOnShipment.JW_LegOrder = 1;

			transportOnShipment.JW_RL_NKDiscPort = "AU2UA";
			transportOnShipment.JW_ETA = new ZDateTime(2018, 1, 8);

			var expectedDateFromConsolTransport = new ZDateTime(2018, 1, 10);

			declaration.ResetEstimateDateForView();
			AssertEquals("Should get ETA from consol transport before shipment transport.", expectedDateFromConsolTransport, declaration.JE_ETAOfDischarge);

			AssertDateFromTransport(declaration,
				consol,
				BaseJobDeclaration.Schema.JE_ETDOfLoading,
				JobDeclarationSchema.JE_RL_NKPortOfLoading,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_RL_NKLoadPort);

			transportOnShipment.JW_RL_NKLoadPort = "AU3WS";
			transportOnShipment.JW_ETD = new ZDateTime(2018, 1, 9);

			declaration.ResetEstimateDateForView();
			AssertEquals("Should get ETD from consol transport before shipment transport.", expectedDateFromConsolTransport, declaration.JE_ETDOfLoading);
		}

		public void TestGetEstimateDateFromShipmentTransport()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			consol.Transports.RemoveAndDeleteAll();

			AssertDateFromTransport(declaration,
				shipment,
				BaseJobDeclaration.Schema.JE_ETAOfDischarge,
				JobDeclarationSchema.JE_RL_NKPortOfArrival,
				JobConsolTransportSchema.JW_ETA,
				JobConsolTransportSchema.JW_RL_NKDiscPort);

			AssertDateFromTransport(declaration,
				shipment,
				BaseJobDeclaration.Schema.JE_ETDOfLoading,
				JobDeclarationSchema.JE_RL_NKPortOfLoading,
				JobConsolTransportSchema.JW_ETD,
				JobConsolTransportSchema.JW_RL_NKLoadPort);
		}

		void AssertDateFromTransport(BaseJobDeclaration declaration, ITransportParent transportParent, string dateColumnOfDec, SchemaColumn portColumnOfDec, SchemaColumn dateColumnOfTransport, SchemaColumn portColumnOfTransport)
		{
			declaration[portColumnOfDec] = ZString.Empty;
			transportParent.Transports.RemoveAndDeleteAll();

			AssertEquals("Should default to empty.", ZDateTime.Empty, declaration[dateColumnOfDec]);

			var transport1 = transportParent.Transports.AddNew();
			transport1.FillWithValidTestData();

			transport1[portColumnOfTransport] = "CNSHA";
			transport1[dateColumnOfTransport] = new ZDateTime(2018, 1, 1);
			transport1.JW_LegOrder = 1;

			var transport2 = transportParent.Transports.AddNew();
			transport2.FillWithValidTestData();

			transport2[portColumnOfTransport] = "AUSYD";
			transport2[dateColumnOfTransport] = new ZDateTime(2018, 1, 5);
			transport2.JW_LegOrder = 2;

			var transport3 = transportParent.Transports.AddNew();
			transport3.FillWithValidTestData();

			transport3[portColumnOfTransport] = "AUYUK";
			transport3[dateColumnOfTransport] = new ZDateTime(2018, 1, 10);
			transport3.JW_LegOrder = 3;

			declaration[portColumnOfDec] = ZString.Empty;
			declaration.ResetEstimateDateForView();

			var actualDate = declaration[dateColumnOfDec];

			AssertEquals("Should is empty as the port code is empty.", ZDateTime.Empty, actualDate);

			declaration[portColumnOfDec] = "AUSYD";
			declaration.ResetEstimateDateForView();

			var expectedDate = new ZDateTime(2018, 1, 5);
			actualDate = (ZDateTime)declaration[dateColumnOfDec];

			var message = string.Format("Should get date from the transport leg with {0} that matches the declaration - {1}.",
				portColumnOfTransport.Name, portColumnOfDec.Name);

			AssertEquals(message, expectedDate, actualDate);

			declaration[portColumnOfDec] = "AUZNE";
			declaration.ResetEstimateDateForView();

			expectedDate = new ZDateTime(2018, 1, 10);
			actualDate = (ZDateTime)declaration[dateColumnOfDec];

			message = string.Format("Should get date from the last transport leg that its {0} in the same country as the {1}.",
				portColumnOfTransport.Name, portColumnOfDec.Name);

			AssertEquals(message, expectedDate, actualDate);
		}

		#endregion

		public void TestCartageContainerMode()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var internalCartage = declaration as IHaveInternalCartage;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_ContainerMode = BaseCusContainer.ContainerModes.BreakBulk;
			AssertEquals("Container mode For BBK", Core.Constants.ContainerModes.BreakBulk, internalCartage.ContainerMode);
			declaration.JE_TransportMode = "AIR";
			declaration.JE_ContainerMode = "";
			AssertEquals("Default container mode is blank for air", "", internalCartage.ContainerMode);
			declaration.JE_TransportMode = "SEA";
			AssertEquals("Default sea container mode is blank", "", internalCartage.ContainerMode);
			declaration.JE_ContainerMode = "FCL";
			AssertEquals("Setting JE_ContainerMode to FCL gives same for CartageContainerMode", "FCL", internalCartage.ContainerMode);
			declaration.JE_ContainerMode = "FCX";
			AssertEquals("Setting JE_ContainerMode to FCX gives FCL for CartageContainerMode", "FCL", internalCartage.ContainerMode);
			declaration.JE_ContainerMode = "LCL";
			AssertEquals("Setting JE_ContainerMode to LCL gives LCL for CartageContainerMode", "LCL", internalCartage.ContainerMode);
			declaration.JE_ContainerMode = "XXX";
			AssertEquals("Setting JE_ContainerMode to crap gives default for CartageContainerMode", "", internalCartage.ContainerMode);
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("If JE_containermode not set to LCL/FCL/FCX, then we look a CusContainers to work out CartageContainerMode", "FCL", internalCartage.ContainerMode);
			container.CO_FCL_LCL_AIR = "LCL";
			AssertEquals("If JE_containermode not set to LCL/FCL/FCX, then we look a CusContainers to work out CartageContainerMode", "LCL", internalCartage.ContainerMode);
			container.CO_FCL_LCL_AIR = "FCL";
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "FCL";
			AssertEquals("If JE_containermode not set to LCL/FCL/FCX, then we look a CusContainers to work out CartageContainerMode.  At least one FCL container in the collection means overall FCL", "FCL", internalCartage.ContainerMode);
			var shipment = Factory.New<CommonShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_PackingMode = "ABC";
			AssertEquals("If a shipment is on the dec, defer to the shipment's mode for CartageContainerMode", "ABC", internalCartage.ContainerMode);
		}

		public void TestFreightContainerMode()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = "FCL";
			container2.CO_FCL_LCL_AIR = "";
			AssertEquals("FCL", declaration.FreightContainerMode);
			container2.CO_FCL_LCL_AIR = "LCL";
			AssertEquals("FCL", declaration.FreightContainerMode);
			container1.CO_FCL_LCL_AIR = "FCX";
			AssertEquals("FCL", declaration.FreightContainerMode);
			container1.CO_FCL_LCL_AIR = "LCL";
			AssertEquals("LCL", declaration.FreightContainerMode);

			container1.CO_FCL_LCL_AIR = "BBK";
			AssertEquals("BBK", declaration.FreightContainerMode);

			container1.CO_FCL_LCL_AIR = "BLK";
			AssertEquals("BLK", declaration.FreightContainerMode);

			var shipment = Factory.New<CommonShipment>();
			declaration.JE_JS = shipment.PK;
			shipment.JS_PackingMode = "ABC";
			AssertEquals("ABC", declaration.FreightContainerMode);
		}

		#region Estimated Delivery Time

		public void TestETDeliveryDoesntGetUpdatedForInvalidATA()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_DateOfArrival = ZDateTime.Invalid;
			Assert("Est. Deliv. should be empty!", declaration.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);
		}

		public void TestETDeliveryDoesntDefaultToETAPlusDaysWhenNotImport()
		{
			ZDateTime currentDate = new ZDateTime(2005, 4, 14);
			GlbPortDeliveryTime defaultDelay3 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay3.G1_FreightMode = Core.Constants.TransportModes.Sea;
			defaultDelay3.G1_RL_NKDischargePort = "AUPER";
			defaultDelay3.G1_RL_NKDestinationPort = "AUSYD";
			defaultDelay3.G1_DaysDelayFromArrivalToDeliver = 10;
			defaultDelay3.G1_DaysFromDestinationArrivalToClientDelivery = 3;

			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USSEA";
			declaration.JE_DateOfArrival = currentDate;
			AssertEquals("Est. Delivery should be DateOfArrival", true, declaration.DocsAndCartage.JP_EstimatedDelivery.IsEmpty);
		}

		public void TestEstDeliveryOrPickupDefaultedFromETAAccordingToDefaultDeliveryWhenDelayIsNotSetRegistryItem()
		{
			var testDate = new ZDateTime(2010, 7, 22);
			var testDate2 = new ZDateTime(2005, 5, 14);
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = nameof(FreightMode.LCL);
			TestDec.JE_RL_NKPortOfArrival = defaultDelay.G1_RL_NKDischargePort = "USLAX";
			TestDec.JE_RL_NKFinalDestination = defaultDelay.G1_RL_NKDestinationPort = "USAAA";
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 0;
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;

			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Assert("Precondition - Estimated Delivery is blank by default", TestDec.JE_EstimatedDeliveryOrPickup.IsEmpty);

			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery is NOT defaulted when the registry is off and no delays", ZDateTime.Empty, TestDec.JE_EstimatedDeliveryOrPickup);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;
			TestDec.JE_DateAtFinalDestination = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery is defaulted when the registry is off and delay is set", testDate.AddDays(3), TestDec.JE_EstimatedDeliveryOrPickup);

			TestDec.JE_DateAtFinalDestination = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate2;
			AssertEquals("Estimated Delivery is defaulted when not empty", testDate2.AddDays(3), TestDec.JE_EstimatedDeliveryOrPickup);

			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestDec.JE_DateAtFinalDestination = TestDec.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery is defaulted when the registry is on", testDate.AddDays(3), TestDec.JE_EstimatedDeliveryOrPickup);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 0;
			TestDec.JE_DateAtFinalDestination = TestDec.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate2;
			AssertEquals("Estimated Delivery is defaulted when the registry is on and no delays", testDate2, TestDec.JE_EstimatedDeliveryOrPickup);

			TestDec.JE_DateAtFinalDestination = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery is defaulted when less than ETA", testDate, TestDec.JE_EstimatedDeliveryOrPickup);

			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultDeliveryWhenDelayIsNotSet.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 0;
			TestDec.JE_DateAtFinalDestination = TestDec.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Registry fall back to company level should cause no defaulting", ZDateTime.Empty, TestDec.JE_EstimatedDeliveryOrPickup);
		}

		public void TestDoNotDefaultWhenNoMatchFound()
		{
			var testDate = new ZDateTime(2010, 7, 22);

			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = nameof(FreightMode.LCL);
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";

			TestDec.JE_RL_NKPortOfArrival = "USLAX";
			TestDec.JE_RL_NKFinalDestination = "USAAA";
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery should not be defaulted when No match is found", ZDateTime.Empty, TestDec.JE_EstimatedDeliveryOrPickup);
		}

		public void TestDefaultForSEA()
		{
			var testDate = new ZDateTime(2010, 7, 22);

			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = nameof(FreightMode.SEA);
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;
			defaultDelay.G1_RL_NKDischargePort = "USLAX";
			defaultDelay.G1_RL_NKDestinationPort = "USAAA";

			TestDec.JE_RL_NKPortOfArrival = "USLAX";
			TestDec.JE_RL_NKFinalDestination = "USAAA";
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			TestDec.JE_DateAtFinalDestination = testDate;
			AssertEquals("Estimated Delivery should not be defaulted when No match is found", testDate.AddDays(3), TestDec.JE_EstimatedDeliveryOrPickup);
		}

		#endregion

		public virtual void TestUpdatingPortOfArrivalUpdatesEmptyFinalDestination()
		{
			TestDec.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Final Destination should be AUSYD", "AUSYD", TestDec.JE_RL_NKFinalDestination);
		}

		public void TestUpdatingPortOfArrivalDoesntUpdateNotEmptyFinalDestination()
		{
			TestDec.JE_RL_NKFinalDestination = "AUDBO";
			TestDec.JE_RL_NKPortOfArrival = "AUSYD";
			AssertEquals("Final Destination should be AUSYD", "AUDBO", TestDec.JE_RL_NKFinalDestination);
		}

		public virtual void TestUpdatingPortOfLoadingUpdatesEmptyOrigin()
		{
			TestDec.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Origin should be AUSDY", "AUSYD", TestDec.JE_RL_NKOrigin);
		}

		public void TestUpdatingPortOfLoadingDoesntUpdateNotEmptyOrigin()
		{
			TestDec.JE_RL_NKOrigin = "AUPER";
			TestDec.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Origin should be AUSDY", "AUPER", TestDec.JE_RL_NKOrigin);
		}

		public void TestUpdatingExportDateUpdatesEmptyDateAtOrigin()
		{
			ZDateTime currentDate = new ZDateTime(2005, 4, 6);
			TestDec.JE_ExportDate = currentDate;
			AssertEquals("Date At Origin", currentDate, TestDec.JE_DateAtOrigin);
			TestDec.JE_DateAtOrigin = ZDateTime.Empty;
			ZDateTime invalidPastDate = new ZDateTime(1006, 4, 6);
			TestDec.JE_ExportDate = invalidPastDate;
			AssertEquals("Date At Origin", ZDateTime.Empty, TestDec.JE_DateAtOrigin);
		}

		public void TestUpdatingDateAtOriginUpdatesEmptyExportDate()
		{
			ZDateTime currentDate = new ZDateTime(2005, 4, 6);
			TestDec.JE_DateAtOrigin = currentDate;
			AssertEquals("Date At Origin", currentDate, TestDec.JE_ExportDate);
			TestDec.JE_ExportDate = ZDateTime.Empty;
			ZDateTime invalidPastDate = new ZDateTime(1006, 4, 6);
			TestDec.JE_DateAtOrigin = invalidPastDate;
			AssertEquals("Date At Origin", ZDateTime.Empty, TestDec.JE_ExportDate);
		}

		public void TestUpdatingExportDateDoentUpdateNotEmptyDateAtOrigin()
		{
			ZDateTime currentDate = new ZDateTime(2005, 4, 6);
			TestDec.JE_DateAtOrigin = currentDate;
			TestDec.JE_ExportDate = currentDate.AddDays(5);
			AssertEquals("Date At Origin", currentDate, TestDec.JE_DateAtOrigin);
		}

		public void TestRunningPreSaveValidationRunsApportionment()
		{
			TestDec.MarkApportionmentDirty();

			AssertEquals("Apportionment is dirty", true, TestDec.ApportionmentDirty);
			TestDec.RunPreSaveValidation();
			AssertEquals("Apportionment is resumed", false, TestDec.ApportionmentDirty);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestJobDeclarationSynchroniserWithNoShipment()
		{
			TestDec.ShipmentSynchroniser.SetEnabled(false, false);
			AssertEquals("You can't synchronise with a shipment when you don't have a shipment.", ErrorReporter.LastExceptionReported);
		}

		public void TestJobDeclarationSynchroniserLoadsNewSynchroniser()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			TestDec.JE_JS = shipment.PK;
			TestDec.ShipmentSynchroniser.SetEnabled(true, false);
			AssertNotNull(TestDec.ShipmentSynchroniser);
		}

		public virtual void TestFactorySavingDoesNotCauseAMergeToBeRequired()
		{
			var declaration = new MergedDeclarationCreator<T>(Factory).Declaration;
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
			Factory.Save();
			AssertEquals("RequiresMerge", false, declaration.MergeManager.RequiresMerge);
		}

		public virtual void TestShouldSynchroniseWithShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			TestDec.JE_OverrideFreightDefaults = true;
			AssertEquals(false, TestDec.ShouldSynchroniseWithShipment());
			TestDec.JE_OverrideFreightDefaults = false;
			AssertEquals(true, TestDec.ShouldSynchroniseWithShipment());
			var message = TestDec.Messages.AddNew();
			AssertEquals(TestDec.IsDeclarationIntegrated, TestDec.ShouldSynchroniseWithShipment());
			message.EM_Status = EDIMessage.Status.Discarded;
			AssertEquals(true, TestDec.ShouldSynchroniseWithShipment());
			TestDec.JE_JS = ZGuid.Empty;
			AssertEquals(false, TestDec.ShouldSynchroniseWithShipment());
		}

		public virtual void TestSynchroniseWithShipmentWhenDeactivate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			Factory.Save();

			shipment.IsCancelled = true;
			Assert("Shipment has changes", shipment.HasChanges);
			Assert("Declaration has changes", TestDec.HasChanges);

			var iSynchonisation = (IJobDeclarationWithShipmentSynchonisation)TestDec;
			iSynchonisation.SynchroniseWithShipmentIfNeeded();

			Assert(shipment.JS_IsCancelled);
			Assert(TestDec.JE_IsCancelled);
			Factory.Save();

			var factoryForLoad = new BusinessObjectFactory();
			var shipmentLoaded = factoryForLoad.Load<ForwardingShipment>(shipment.PK);
			Assert(shipmentLoaded.JS_IsCancelled);

			var declarationLoaded = factoryForLoad.Load<BaseJobDeclaration>(TestDec.PK);
			Assert(declarationLoaded.JE_IsCancelled);
		}

		public virtual void TestShouldNotSynchroniseWithShipmentEndToEnd()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "123124";
			container1 = packLine1.Containers.AddNew();
			TestDec.JE_JS = shipment.PK;
			TestDec.JE_OverrideFreightDefaults = true;
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			TestHelper.MakeConsolRelevantToDeclaration(consol, TestDec);
			AssertEquals("Container should not be synched at this stage", 0, TestDec.CusContainers.Count);
			TestDec.JE_OverrideFreightDefaults = false;
			AssertEquals("Container should have been synched from shipment", 1, TestDec.CusContainers.Count);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "23423";
			packLine2.JL_JC = container2.PK;
			AssertEquals("Container should have been synched from shipment", 2, TestDec.CusContainers.Count);
			TestDec.JE_OverrideFreightDefaults = true;
			var packLine3 = shipment.OuterPackLines.AddNew();
			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "652334";
			packLine3.JL_JC = container3.PK;
			packLine3.JL_PackageCount = 1;
			AssertEquals("Container added to shipment should not have synched", 2, TestDec.CusContainers.Count);
		}

		public void TestIsStandAlone()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			AssertEquals(true, TestDec.IsStandAlone);
			TestDec.JE_JS = shipment.PK;
			AssertEquals(false, TestDec.IsStandAlone);
		}

		public virtual void TestMergeMethodThatTakesISendsMessageToCustoms()
		{
			if (!TestDec.IsDeclarationIntegrated)
			{
				SendsMessagesToCustomsShutterUpperer notificationCollector = new SendsMessagesToCustomsShutterUpperer(false);
				TestDec.DoMerge(notificationCollector);
				Assert(notificationCollector.InvalidOperationText.IndexOf("invoice header") != -1);
			}
			else
			{
				Assert(true);//no merge for customsware enabled
			}
		}

		#region TestMergedSuccessfullyEvent
		public virtual void TestMergedSuccessfullyEvent()
		{
			mergedSuccessfullyCallCount = 0;
			TestDec.MergedSuccessfully += new MergedSuccessfullyHandler(Declaration_MergedSuccessfully);
			PrepareDeclarationForMerge(TestDec);

			AssertEquals("No merge yet", 0, mergedSuccessfullyCallCount);

			TestDec.DoMerge();
			AssertEquals("MergeCount", 1, mergedSuccessfullyCallCount);
			TestDec.DoMerge();
			AssertEquals("MergeCount", 2, mergedSuccessfullyCallCount);

			TestDec.JE_OH_Importer = ZGuid.Invalid;
			TestDec.DoMerge();
			AssertEquals("MergeCount still works even if there are errors", 3, mergedSuccessfullyCallCount);
		}

		protected virtual void PrepareDeclarationForMerge(BaseJobDeclaration declaration)
		{
			var invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			if (declaration.IsEntryInstructionRequired)
			{
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
			}
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
		}

		int mergedSuccessfullyCallCount;
		void Declaration_MergedSuccessfully()
		{
			mergedSuccessfullyCallCount++;
		}
		#endregion

		protected CusEntryHeader CreateEntryHeaderWithCustomsValueOf(ZDecimal customsValue, BaseJobDeclaration declaration)
		{
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = customsValue;
			return entryHeader;
		}

		public void TestGrossWeight()
		{
			TestDec.JE_TotalWeight = 100m;
			TestDec.JE_TotalWeightUnit = "LB";
			AssertEquals(new ZWeight(100, "LB"), TestDec.GrossWeight);
			TestDec.GrossWeight = new ZWeight(200, "KG");
			AssertEquals(200m, TestDec.JE_TotalWeight);
			AssertEquals("KG", TestDec.JE_TotalWeightUnit);
		}

		public virtual void TestTotalCustomsValueInLocalCurreny()
		{
			CusEntryHeader entryHeader1 = CreateEntryHeaderWithCustomsValueOf(100m, TestDec);
			CusEntryHeader entryHeader2 = CreateEntryHeaderWithCustomsValueOf(200m, TestDec);
			CusEntryHeader entryHeader3 = CreateEntryHeaderWithCustomsValueOf(300m, TestDec);
			AssertEquals(600m, TestDec.TotalCustomsValueInLocalCurrency);
		}

		public void TestIsRoad()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeRoadCodeForTesting;
			AssertEquals(true, TestDec.IsRoad);
		}

		public void TestIsRail()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeRailCodeForTesting;
			AssertEquals(true, TestDec.IsRail);
		}

		public void TestIsAir()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertEquals(true, TestDec.IsAir);
		}

		public void TestIsFixed()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeFixedCodeForTesting;
			AssertEquals(true, TestDec.IsFixedInstallation);
		}

		public void TestFlightNoHasNumericCarrierCode()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			AssertEquals("Property relates to Air declaration only", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			TestDec.JE_VoyageFlightNo = "QF439";
			AssertEquals("Airline Carrier Code does not contain numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "5X215";
			AssertEquals("Airline Carrier Code for UPS (5X) does contain numeric carrier code", true, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "9W110";
			AssertEquals("Airline Carrier Code for Jet Airways India (9W) does contain numeric carrier code", true, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "SQ225";
			AssertEquals("Airline Carrier Code for Singapore Air does not contain numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "439";
			AssertEquals("Flight no without Airline Code does not have numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "QF1";
			AssertEquals("Airline Carrier Code does contain numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "001A";
			AssertEquals("Flight no without Airline Code does not have numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "01";
			AssertEquals("Flight no without Airline Code does not have numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "1";
			AssertEquals("Flight no without Airline Code does not have numeric carrier code", false, TestDec.FlightNoHasNumericAirlineCode);

			TestDec.JE_VoyageFlightNo = "X5300";
			AssertEquals("Airline Carrier Code for Afrique Airlines S.A. (X5) does contain numeric carrier code", true, TestDec.FlightNoHasNumericAirlineCode);
		}

		public void TestCancellation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			var header1 = Factory.New<US.InBond.ICusInBondHeader>();
			header1.BH_ParentID = TestDec.PK;
			header1.BH_ParentTableCode = TestDec.TablePrefix;
			var header2 = Factory.New<US.InBond.ICusInBondHeader>();
			header2.BH_ParentID = TestDec.PK;
			header2.BH_ParentTableCode = TestDec.TablePrefix;

			TestDec.IsCancelled = true;
			Assert(shipment.IsCancelled);
			Assert(header1.IsCancelled);
			Assert(header2.IsCancelled);

			TestDec.IsCancelled = false;
			Assert(!shipment.IsCancelled);
			Assert(!header1.IsCancelled);
			Assert(!header2.IsCancelled);
		}

		public void ShouldCloneContainersEvenNotLinked()
		{
			AssertEquals(false, TestDec.ShouldCloneContainersEvenNotLinked);
			TestDec.ShouldCloneContainersEvenNotLinked = true;
			AssertEquals(true, TestDec.ShouldCloneContainersEvenNotLinked);
		}

		public void TestIsSea()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			AssertEquals(true, TestDec.IsSea);
		}

		public virtual void TestIsBreakBulk()
		{
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(true, TestDec.IsBreakBulk);
			TestDec.JE_ContainerMode = ZString.Empty;
			AssertEquals(false, TestDec.IsBreakBulk);
		}

		public virtual void TestIsEmptyContainer()
		{
			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.Empty;
			AssertEquals(true, TestDec.IsEmptyContainer);
			TestDec.JE_ContainerMode = ZString.Empty;
			AssertEquals(false, TestDec.IsEmptyContainer);
		}

		public void TestAttachingOrderSetsHasChanges()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Factory.Save();
			AssertEquals("Initially no changes", false, declaration.HasChanges);
			declaration.AttachedOrders.Add(Factory.New<Order>());
			AssertEquals("Has changes", true, declaration.HasChanges);
		}

		public void TestUnRegisterEditableChildObjectsForMessageValidation_WithoutShipment()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.MainAddress.City = "Sydney";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			Factory.Save();
			AssertEquals("(pre-condition) declaration has no changes", false, declaration.HasChanges);

			declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
			AssertEquals("(pre-condition) orders are editable children", true, declaration.HasChanges);

			Factory.Save();
			AssertEquals("(pre-condition) declaration has no changes", false, declaration.HasChanges);

			using (declaration.UnRegisterEditableChildObjectsForMessageValidation())
			{
				declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
				AssertEquals("orders are not editable children for now", false, declaration.HasChanges);
			}

			declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
			AssertEquals("orders are editable children again", true, declaration.HasChanges);
		}

		public void TestUnRegisterEditableChildObjectsForMessageValidation_WithShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			AssertEquals("(pre-condition) declaration has no changes", false, declaration.HasChanges);

			declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
			AssertEquals("(pre-condition) orders are not editable children, because they belong to shipment", false, declaration.HasChanges);

			using (declaration.UnRegisterEditableChildObjectsForMessageValidation())
			{
				declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
				AssertEquals("orders did not become editable children", false, declaration.HasChanges);
			}

			declaration.AttachedOrders.Add(Factory.NewWithValidTestData<Order>());
			AssertEquals("orders did not become editable children", false, declaration.HasChanges);
		}

		public void TestAttachingOrderUpdatesRequiredDocuments()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Assert("Dec doesn't have an original bill", !declaration.DocsAndCartage.RequiredDocuments.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));

			Order order = Factory.NewWithValidTestData<Order>();
			JobRequiredDocument doc = order.RequiredDocuments.AddNew();
			doc.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;

			Factory.Save();
			declaration.AttachedOrders.Add(order);

			Assert("Dec should have original bill", declaration.DocsAndCartage.RequiredDocuments.IsDocRequired(Core.Constants.RefDocTypes.MasterBill));
		}

		public void TestOrderNumbersForModuleGrid()
		{
			const string TestOrderNumber1 = "2200192";
			const string TestOrderNumber2 = "4403200";
			BaseJobDeclaration declaration = GetJobDeclaration();
			Order newOrder1 = Factory.New<Order>();
			newOrder1.JD_OrderNumber = TestOrderNumber1;
			declaration.AttachedOrders.Add(newOrder1);
			AssertEquals("Order numbers", TestOrderNumber1, declaration.OrderNumbers);
			Order newOrder2 = declaration.AttachedOrders.AddNew();
			newOrder2.JD_OrderNumber = TestOrderNumber2;
			AssertEquals("Order numbers", TestOrderNumber1 + ", " + TestOrderNumber2, declaration.OrderNumbers);
		}
		public void TestCloningAndDeletingCloneDoesntRemoveShipmentOrders()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			shipment.AttachedOrders.AddNew();
			AssertEquals(shipment.PK, shipment.AttachedOrders[0].JD_JS);
			BaseJobDeclaration clonedDec = (BaseJobDeclaration)declaration.Clone();
			clonedDec.Delete();
			AssertEquals("Order Still Linked to Shipment", shipment.PK, shipment.AttachedOrders[0].JD_JS);
		}
		public void TestMakeNonPersistent()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.HasChanges = true;
			Assert("Should be saved", declaration.IsSavedByFactory);
			declaration.MakeNonPersistent();
			Assert("Should no longer be saved", !declaration.IsSavedByFactory);
		}
		public void TestIsPersistent()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Assert("Default is persistent", declaration.IsPersistent);
			declaration.MakeNonPersistent();
			Assert("Now non-persistent", !declaration.IsPersistent);
		}

		public void TestConfigOrg()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			invoiceHeader.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;
			AssertEquals("IMP", invoiceHeader.JZ_MessageType);
			AssertEquals("ConfigOrg is Declaration's Importer", declaration.JE_OH_Importer, declaration.ConfigOrg?.PK);

			declaration = BaseJobDeclaration.New(Factory);
			invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
			invoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
			AssertEquals("EXP", invoiceHeader.JZ_MessageType);
			AssertEquals("ConfigOrg is Declaration's Supplier", declaration.JE_OH_Supplier, declaration.ConfigOrg?.PK);

			declaration = BaseJobDeclaration.New(Factory);
			invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = declaration.PK;
			declaration.MakeNonPersistent();
			invoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
			invoiceHeader.JZ_MessageType = "EXP";
			declaration.JE_MessageType = "IMP";
			AssertEquals("IMP", declaration.JE_MessageType);
			AssertEquals("EXP", invoiceHeader.JZ_MessageType);
			AssertEquals("ConfigOrg is InvoiceHeader's Supplier", invoiceHeader.JZ_OH_Supplier, declaration.ConfigOrg?.PK);
		}

		public void TestDeclarationReferenceAssignment()
		{
			BaseJobDeclaration oneToOne = GetJobDeclaration();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			oneToOne.JE_JS = shipment.PK;
			Factory.Save();
			Assert("Reference number starts with S", oneToOne.JE_DeclarationReference.StartsWith("S"));

			oneToOne = GetJobDeclaration();
			AssertEquals("it is a standalone declaration", ZGuid.Empty, oneToOne.JE_JS);
			Factory.Save();
			Assert("Reference number starts with B", oneToOne.JE_DeclarationReference.StartsWith("B"));
		}

		public void TestDeclarationReferenceAssignmentWhenWEA()
		{
			BaseJobDeclaration oneToOne = GetJobDeclaration();
			oneToOne.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			Assert("Reference number starts with B", oneToOne.JE_DeclarationReference.StartsWith("B"));

			oneToOne = GetJobDeclaration();
			oneToOne.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Factory.Save();
			ZString declarationReference = oneToOne.JE_DeclarationReference;
			Assert("Reference number starts with G", declarationReference.StartsWith("G"));

			oneToOne.JE_VoyageFlightNo = "123";
			Factory.Save();
			AssertEquals("Declaration Reference should not be changed", declarationReference, oneToOne.JE_DeclarationReference);
		}

		public void TestIsRefund()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Refund;
			AssertEquals(true, TestDec.IsRefund);
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, TestDec.IsRefund);
		}

		public void TestIsMiscellaneous()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(true, TestDec.IsMiscellaneous);
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, TestDec.IsMiscellaneous);
		}

		public virtual void TestIsExWarehouse()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, TestDec.IsExWarehouse);
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals(false, TestDec.IsExWarehouse);
		}

		public virtual void TestIsDrawback()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("Declaration is of type Drawback", true, TestDec.IsDrawback);
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Declaration is not of type Drawback", false, TestDec.IsDrawback);
		}

		public void TestIsFCLorFCX()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = Constants.TransportModes.Sea;
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.FullContainerLoad;
			Assert("IsFCLorFCX", TestDec.IsFCLorFCX);
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.FCX;
			Assert("IsFCLorFCX", TestDec.IsFCLorFCX);
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.LessContainerLoad;
			Assert("IsFCLorFCX", !TestDec.IsFCLorFCX);
		}

		public void TestEntrySubmittedDateResetWhenMergeThrownAway()
		{
			BaseJobDeclaration testDec = GetJobDeclaration();
			testDec.JE_EntrySubmittedDate = new ZDateTime(2004, 10, 20);
			testDec.ThrowAwayMerge();
			AssertEquals("Entry submitted date", ZDateTime.Empty, testDec.JE_EntrySubmittedDate);
		}

		public void TestReferenceNumberAssignmentForPlugInWithJobInvoicing()
		{
			BaseJobDeclaration testDec = GetJobDeclaration();
			JobHeader invoicingJob = Factory.NewJobForTesting<JobHeader>();
			invoicingJob.JH_ParentID = testDec.PK;
			invoicingJob.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			invoicingJob.JH_JobNum = "Job1";
			invoicingJob.JH_GB = GlbBranch.CurrentBranch.PK;
			invoicingJob.JH_GC = GlbCompany.CurrentCompany.PK;
			invoicingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			testDec.JE_JS = shipment.PK;
			Factory.Save();

			AssertEquals("PlugIn declaration No", true, testDec.JE_DeclarationReference.StartsWith("S"));
		}

		public void TestReferenceNumberNotAssignedAfterMessageGeneratedOrJobInvoicingGenerated()
		{
			BaseJobDeclaration testDec = GetJobDeclaration();
			Factory.Save();
			AssertEquals("PreCondition: Stand-alone declaration No", true, testDec.JE_DeclarationReference.StartsWith("B"));

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			var invoicingJob = Factory.NewJobForTesting<JobHeader>();
			invoicingJob[JobHeaderSchema.JH_ParentID.Name] = shipment.PK;
			invoicingJob[JobHeaderSchema.JH_ParentTableCode.Name] = JobDeclarationSchema.Constants.Prefix;
			invoicingJob[JobHeaderSchema.JH_GB.Name] = GlbBranch.CurrentBranch.PK;
			invoicingJob[JobHeaderSchema.JH_GC.Name] = GlbCompany.CurrentCompany.PK;
			invoicingJob[JobHeaderSchema.JH_GE.Name] = GlbDepartment.CurrentDepartment.PK;
			invoicingJob[JobHeaderSchema.JH_JobNum.Name] = testDec.JE_DeclarationReference;

			testDec.JE_JS = shipment.PK;

			Factory.Save();
			AssertEquals("Declaration No cannot be re-assigned", true, testDec.JE_DeclarationReference.StartsWith("B"));
		}

		public void TestIsPluggedIntoShipment()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals("Now plugged into", true, declaration.IsPluggedIntoShipment);
		}

		public void TestGetInvoicesFromJobDeclaration()
		{
			BaseJobComInvoiceGroupHeader groupHeader = TestDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
			groupHeader.JobComInvoiceHeaders.AddNew();
			groupHeader1.JobComInvoiceHeaders.AddNew();
			groupHeader2.JobComInvoiceHeaders.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobDeclaration jobDecLoaded = newFactory.Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Three invoices", 3, jobDecLoaded.Invoices.Count);
		}

		public virtual void TestInvoiceLinesFromJobDeclaration()
		{
			BaseJobComInvoiceGroupHeader groupHeader = TestDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader groupHeader1 = groupHeader.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceGroupHeader groupHeader2 = groupHeader1.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = groupHeader1.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice3 = groupHeader2.JobComInvoiceHeaders.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice2.JobComInvoiceLines.AddNew();
			invoice3.JobComInvoiceLines.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobDeclaration jobDecLoaded = newFactory.Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Three invoice lines", 3, jobDecLoaded.InvoiceLines.Count);
			AssertEquals("Three invoice lines", 3, jobDecLoaded.FilteredInvoiceLines.Count);
		}

		#region TestSettingImporter

		public virtual void TestSettingImporter()
		{
			OrgHeader cartage = Factory.LoadTop1<OrgHeader>(new ZQuery());

			PartyInCurrentCountry.SetRelatedParty(cartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			PartyInCurrentCountry.MainAddress.OA_FCLEquipmentNeeded = "XYZ";
			PartyInCurrentCountry.MainAddress.OA_LCLEquipmentNeeded = "ABC";

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_ContainerMode = Constants.ContainerModes.Containerised;
			TestDec.JE_OH_Importer = PartyInCurrentCountry.PK;
			TestDec.JE_TransportMode = Constants.TransportModes.Sea;

			AssertEquals("Import as importer is in this country", DefaultImportMessageType, TestDec.JE_MessageType);
			AssertEquals("Default container mode is unset so equipment is also unset", "", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);

			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			TestDec.JE_ContainerMode = Constants.ContainerModes.FCL;
			TestDec.JE_OH_Importer = ZGuid.Empty;
			TestDec.JE_OH_Importer = PartyInCurrentCountry.PK;
			AssertEquals("FCL Equipment", "XYZ", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			TestDec.JE_OH_Importer = ZGuid.Empty;
			TestDec.JE_OH_Importer = PartyInCurrentCountry.PK;
			AssertEquals("FCX Equipment is the same as FCL", "XYZ", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
		}

		protected virtual string DefaultImportMessageType
		{
			get { return Customs.Business.JobMessageTypeList.Codes.Import; }
		}

		#endregion

		public virtual void TestWhenSettingImporterFinalDestinationWillBeSet()
		{
			OrgHeader cartage = Factory.LoadTop1<OrgHeader>(new ZQuery());
			PartyInCurrentCountry.SetRelatedParty(cartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			PartyInCurrentCountry.MainAddress.OA_FCLEquipmentNeeded = "XYZ";

			TestDec.JE_OH_Importer = PartyInCurrentCountry.PK;
			TestDec.JE_TransportMode = Constants.TransportModes.Sea;
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.FullContainerLoad;
			AssertEquals("Destination set", PartyInCurrentCountry.OH_RL_NKClosestPort, TestDec.JE_RL_NKFinalDestination);
		}

		public virtual void TestEquipmentUpdatedOnSettingTransportMode()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			// Export Job
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;

			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			TestDec.JE_TransportMode = "SEA";

			AssertEquals("Precondition: Export Job", JobMessageTypeList.Codes.Export, TestDec.JE_MessageType);
			AssertEquals("No Address Override - Delivery Equipment blank for export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("No Address Override - Delivery Equipment blank for export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("No Address Override - Delivery Equipment blank for export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment defaults to to same as FCL for FCX", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "AIR";
			AssertEquals("No Address Override - Delivery Equipment blank for export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			// Import Job
			fTestDec = null;
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;

			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";

			AssertEquals("Precondition: Import Job", JobMessageTypeList.Codes.Import, TestDec.JE_MessageType);
			AssertEquals("No Address Override - Delivery Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment blank for import", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			//TestDec.JE_TransportMode = "";
			AssertEquals("No Address Override - Delivery Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment blank for import", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			//TestDec.JE_TransportMode = "";
			//TestDec.JE_TransportMode = "SEA";
			AssertEquals("No Address Override - Delivery Equipment defaults to same as FCL for FCX", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment blank for import", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			//TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "AIR";
			AssertEquals("No Address Override - Delivery Equipment defaults", Enterprise.Registry.Business.OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("No Address Override - Pickup Equipment blank for import", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			OrgAddress importerAddress = importer.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True);
			importerAddress.OA_FCLEquipmentNeeded = "XY1";
			importerAddress.OA_AIREquipmentNeeded = "AB1";
			importerAddress.OA_LCLEquipmentNeeded = "ZU1";

			OrgAddress supplierAddress = supplier.Addresses.AddNew(OrgAddressType.Pickup, ZBool.True);
			supplierAddress.OA_FCLEquipmentNeeded = "XY2";
			supplierAddress.OA_AIREquipmentNeeded = "AB2";
			supplierAddress.OA_LCLEquipmentNeeded = "ZU2";

			// Export Job
			fTestDec = null;
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("Importer Overridden - Delivery Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment determined", "XY2", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("Importer Overridden - Delivery Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment determined", "ZU2", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("Importer Overridden - Delivery Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment determined to same as FCL for FCX", "XY2", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "AIR";
			AssertEquals("Importer Overridden - Delivery Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment determined", "AB2", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			// Import Job
			fTestDec = null;
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			TestDec.JE_TransportMode = "";
			TestDec.JE_TransportMode = "SEA";
			AssertEquals("Importer Overridden - Delivery Equipment determined", "XY1", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			AssertEquals("Importer Overridden - Delivery Equipment determined", "ZU1", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("Importer Overridden - Delivery Equipment determined to same as FCL for FCX", "XY1", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);

			TestDec.JE_TransportMode = "AIR";
			AssertEquals("Importer Overridden - Delivery Equipment determined", "AB1", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			AssertEquals("Supplier Overridden - Pickup Equipment blank for Export", ZString.Empty, TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
		}

		#region TestDefaultingPickupAndDeliveryDatesFromContainersAndBackwards

		ZDateTime testDate14th;
		ZDateTime testDate15th;
		ZDateTime testDate16th;

		ZDateTime testDate22nd;
		ZDateTime testDate23rd;
		ZDateTime testDate24th;

		ZDateTime testDate26th;
		ZDateTime testDate27th;
		ZDateTime testDate28th;

		void InitialiseContainerTestDates()
		{
			testDate14th = new ZDateTime(2010, 10, 14);
			testDate15th = new ZDateTime(2010, 10, 15);
			testDate16th = new ZDateTime(2010, 10, 16);

			testDate22nd = new ZDateTime(2010, 10, 22);
			testDate23rd = new ZDateTime(2010, 10, 23);
			testDate24th = new ZDateTime(2010, 10, 24);

			testDate26th = new ZDateTime(2010, 10, 26);
			testDate27th = new ZDateTime(2010, 10, 27);
			testDate28th = new ZDateTime(2010, 10, 28);
		}

		public void TestDefaultingDeliveryDatesFromContainersAndBackwards_NoShipment()
		{
			InitialiseContainerTestDates();

			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = TestDec.GetDefaultContainerisedContainerMode();
			var container1 = TestDec.CusContainers.AddNew();
			var container2 = TestDec.CusContainers.AddNew();
			var container3 = TestDec.CusContainers.AddNew();
			Factory.Save();
			AssertEquals("Precondition: when container updated, we do not expect Declaration has changes. Declaration should be updated regardless HasChanges, if it is allowed", false, TestDec.HasChanges);

			var newFactory = new BusinessObjectFactory();
			var container1Reloaded = newFactory.Load<BaseCusContainer>(container1.PK);
			SetArrivalContainerDates(container1Reloaded, testDate14th, testDate15th, testDate16th);
			SetDepartureContainerDates(container1Reloaded, testDate26th, testDate27th, testDate28th);

			var container2Reloaded = newFactory.Load<BaseCusContainer>(container2.PK);
			SetArrivalContainerDates(container2Reloaded, testDate22nd, testDate23rd, testDate24th);
			SetDepartureContainerDates(container2Reloaded, testDate26th, testDate27th, testDate28th);

			var container3Reloaded = newFactory.Load<BaseCusContainer>(container3.PK);
			SetDepartureContainerDates(container3Reloaded, testDate26th, testDate27th, testDate28th);
			newFactory.Save();
			AssertDeclarationDates("should not be defaulted, not all containers are delivered", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);

			SetArrivalContainerDates(container3Reloaded, testDate16th, testDate15th, testDate14th);

			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			newFactory.Save();
			AssertDeclarationDates("Don't default dates when a shipment is attached...", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}

		public void TestDefaultingDeliveryDatesFromContainersAndBackwards()
		{
			InitialiseContainerTestDates();

			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = TestDec.GetDefaultContainerisedContainerMode();
			var container1 = TestDec.CusContainers.AddNew();
			var container2 = TestDec.CusContainers.AddNew();
			var container3 = TestDec.CusContainers.AddNew();
			Factory.Save();
			AssertEquals("Precondition: when container updated, we do not expect Declaration has changes. Declaration should be updated regardless HasChanges, if it is allowed", false, TestDec.HasChanges);

			var newFactory = new BusinessObjectFactory();
			var container1Reloaded = newFactory.Load<BaseCusContainer>(container1.PK);
			SetArrivalContainerDates(container1Reloaded, testDate14th, testDate15th, testDate16th);
			SetDepartureContainerDates(container1Reloaded, testDate26th, testDate27th, testDate28th);

			var container2Reloaded = newFactory.Load<BaseCusContainer>(container2.PK);
			SetArrivalContainerDates(container2Reloaded, testDate22nd, testDate23rd, testDate24th);
			SetDepartureContainerDates(container2Reloaded, testDate26th, testDate27th, testDate28th);

			var container3Reloaded = newFactory.Load<BaseCusContainer>(container3.PK);
			SetDepartureContainerDates(container3Reloaded, testDate26th, testDate27th, testDate28th);
			newFactory.Save();
			AssertDeclarationDates("should not be defaulted, not all containers are delivered", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);

			SetArrivalContainerDates(container3Reloaded, testDate16th, testDate15th, testDate14th);
			newFactory.Save();
			AssertDeclarationDates("should be defaulted to last delivered container date on save if empty", testDate22nd, testDate23rd, testDate24th);

			SetArrivalContainerDates(TestDec.CusContainers.AddNew(), new ZDateTime(2010, 10, 31), new ZDateTime(2010, 10, 30), new ZDateTime(2010, 10, 29));
			newFactory.Save();
			AssertDeclarationDates("already set and should not be reset", testDate22nd, testDate23rd, testDate24th);

			//backwards
			TestDec.CusContainers.RemoveAndDeleteAll();
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers.AddNew();

			SetDeclarationDates(testDate14th, testDate15th, testDate16th);
			foreach (BaseCusContainer cusContainer in TestDec.CusContainers)
			{
				AssertArrivalContainerDates("should be defaulted from declaration if empty", cusContainer, testDate14th, testDate15th, testDate16th);
				AssertDepartureContainerDates("should not be defaulted if import", cusContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			SetDeclarationDates(testDate22nd, testDate23rd, testDate24th);
			foreach (BaseCusContainer cusContainer in TestDec.CusContainers)
			{
				AssertArrivalContainerDates("should be reset if value equals to original value on declaration", cusContainer, testDate22nd, testDate23rd, testDate24th);
				AssertDepartureContainerDates("should not be defaulted if import", cusContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			SetArrivalContainerDates(TestDec.CusContainers[0], testDate16th, testDate15th, testDate14th);
			SetDeclarationDates(testDate14th, testDate15th, testDate16th);

			AssertArrivalContainerDates("should not be reset if changed", TestDec.CusContainers[0], testDate16th, testDate15th, testDate14th);
			AssertArrivalContainerDates("should be reset if not changed", TestDec.CusContainers[1], testDate14th, testDate15th, testDate16th);

			SetDeclarationDates(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			const string message = "should (not) be cleared";
			AssertArrivalContainerDates(message, TestDec.CusContainers[0], testDate16th, testDate15th, testDate14th);
			AssertArrivalContainerDates(message, TestDec.CusContainers[1], testDate14th, testDate15th, ZDate.Empty);
		}

		public void TestDefaultingPickupDatesFromContainersAndBackwards()
		{
			var testDate1 = new ZDateTime(2010, 10, 14);
			var testDate2 = new ZDateTime(2010, 10, 15);
			var testDate3 = new ZDateTime(2010, 10, 16);

			var testDate4 = new ZDateTime(2010, 10, 22);
			var testDate5 = new ZDateTime(2010, 10, 23);
			var testDate6 = new ZDateTime(2010, 10, 24);

			var testDate7 = new ZDateTime(2010, 10, 26);
			var testDate8 = new ZDateTime(2010, 10, 27);
			var testDate9 = new ZDateTime(2010, 10, 28);

			TestDec.JE_MessageType = DefaultExportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = TestDec.GetDefaultContainerisedContainerMode();
			var container = TestDec.CusContainers.AddNew();
			SetDepartureContainerDates(container, testDate1, testDate2, testDate3);
			SetArrivalContainerDates(container, testDate7, testDate8, testDate9);

			container = TestDec.CusContainers.AddNew();
			SetDepartureContainerDates(container, testDate4, testDate5, testDate6);
			SetArrivalContainerDates(container, testDate7, testDate8, testDate9);

			container = TestDec.CusContainers.AddNew();
			SetArrivalContainerDates(container, testDate7, testDate8, testDate9);
			Factory.Save();
			AssertDeclarationDates("should not be defaulted, not all containers are picked up", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);

			SetDepartureContainerDates(container, testDate3, testDate2, testDate1);
			Factory.Save();
			AssertDeclarationDates("should be defaulted to last picked up container date on save if empty", testDate4, testDate5, testDate6);

			SetDepartureContainerDates(TestDec.CusContainers.AddNew(), new ZDateTime(2010, 10, 31), new ZDateTime(2010, 10, 30), new ZDateTime(2010, 10, 29));
			Factory.Save();
			AssertDeclarationDates("already set and should not be reset", testDate4, testDate5, testDate6);

			//backwards
			TestDec.CusContainers.RemoveAndDeleteAll();
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers.AddNew();

			SetDeclarationDates(testDate1, testDate2, testDate3);
			foreach (BaseCusContainer cusContainer in TestDec.CusContainers)
			{
				AssertDepartureContainerDates("should be defaulted from declaration if empty", cusContainer, testDate1, testDate2, testDate3);
				AssertArrivalContainerDates("should not be defaulted if export", cusContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			SetDeclarationDates(testDate4, testDate5, testDate6);
			foreach (BaseCusContainer cusContainer in TestDec.CusContainers)
			{
				AssertDepartureContainerDates("should be reset if value equals to original value on declaration", cusContainer, testDate4, testDate5, testDate6);
				AssertArrivalContainerDates("should not be defaulted if export", cusContainer, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			SetDepartureContainerDates(TestDec.CusContainers[0], testDate3, testDate2, testDate1);
			SetDeclarationDates(testDate1, testDate2, testDate3);

			AssertDepartureContainerDates("should not be reset if changed", TestDec.CusContainers[0], testDate3, testDate2, testDate1);
			AssertDepartureContainerDates("should be reset if not changed", TestDec.CusContainers[1], testDate1, testDate2, testDate3);

			SetDeclarationDates(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			const string message = "should (not) be cleared";
			AssertDepartureContainerDates(message, TestDec.CusContainers[0], testDate3, testDate2, testDate1);
			AssertDepartureContainerDates(message, TestDec.CusContainers[1], testDate1, testDate2, ZDateTime.Empty);
		}

		public void TestSetContainerDateOnDeclarationPropagatesToCusContainer_Arrival()
		{
			InitialiseContainerTestDates();
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = TestDec.GetDefaultContainerisedContainerMode();
			var container = TestDec.CusContainers.AddNew();
			SetDeclarationDates(testDate22nd, testDate23rd, testDate24th);
			AssertArrivalContainerDates("Container dates updated by setting declaration date", container, testDate22nd, testDate23rd, testDate24th);
			SetDeclarationDates(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals("JC_ArrivalEstimatedDelivery wiped by setting declaration date", ZDateTime.Empty, container.JobContainer.JC_ArrivalEstimatedDelivery);
			AssertEquals("JC_ArrivalCartageComplete wiped by setting declaration date", ZDateTime.Empty, container.JobContainer.JC_ArrivalCartageComplete);
		}

		public void TestSetContainerDateOnDeclarationPropagatesToCusContainer_Departure()
		{
			InitialiseContainerTestDates();
			TestDec.JE_MessageType = DefaultExportMessageType;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = TestDec.GetDefaultContainerisedContainerMode();
			var container = TestDec.CusContainers.AddNew();
			SetDeclarationDates(testDate22nd, testDate23rd, testDate24th);
			AssertDepartureContainerDates("Container dates updated by setting declaration date", container, testDate22nd, testDate23rd, testDate24th);
			SetDeclarationDates(ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals("JC_DepartureEstimatedPickup wiped by setting declaration date", ZDateTime.Empty, container.JobContainer.JC_DepartureEstimatedPickup);
			AssertEquals("JC_DepartureCartageComplete wiped by setting declaration date", ZDateTime.Empty, container.JobContainer.JC_DepartureCartageComplete);
		}

		void AssertDeclarationDates(string message, ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			AssertEquals("JE_EstimatedDeliveryOrPickup " + message, estimatedDelivery, TestDec.JE_EstimatedDeliveryOrPickup);
			AssertEquals("JP_Calc_CartageAdvised " + message, cartageAdvised, TestDec.JP_Calc_CartageAdvised);
			AssertEquals("JE_CartageCompleted " + message, cartageComplete, TestDec.JE_CartageCompleted);
		}

		static void AssertArrivalContainerDates(string message, BaseCusContainer container, ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			AssertEquals("JC_ArrivalEstimatedDelivery " + message, estimatedDelivery, container.JobContainer.JC_ArrivalEstimatedDelivery);
			AssertEquals("JC_ArrivalCartageAdvised " + message, cartageAdvised, container.JobContainer.JC_ArrivalCartageAdvised);
			AssertEquals("JC_ArrivalCartageComplete " + message, cartageComplete, container.JobContainer.JC_ArrivalCartageComplete);
		}

		static void AssertDepartureContainerDates(string message, BaseCusContainer container, ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			AssertEquals("JC_DepartureEstimatedPickup " + message, estimatedDelivery, container.JobContainer.JC_DepartureEstimatedPickup);
			AssertEquals("JC_DepartureCartageAdvised " + message, cartageAdvised, container.JobContainer.JC_DepartureCartageAdvised);
			AssertEquals("JC_DepartureCartageComplete " + message, cartageComplete, container.JobContainer.JC_DepartureCartageComplete);
		}

		void SetDeclarationDates(ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			TestDec.JE_EstimatedDeliveryOrPickup = estimatedDelivery;
			TestDec.JP_Calc_CartageAdvised = cartageAdvised;
			TestDec.JE_CartageCompleted = cartageComplete;
		}

		static void SetArrivalContainerDates(BaseCusContainer container, ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			container.JobContainer.JC_ArrivalEstimatedDelivery = estimatedDelivery;
			container.JobContainer.JC_ArrivalCartageAdvised = cartageAdvised;
			container.JobContainer.JC_ArrivalCartageComplete = cartageComplete;
		}

		static void SetDepartureContainerDates(BaseCusContainer container, ZDateTime estimatedDelivery, ZDateTime cartageAdvised, ZDateTime cartageComplete)
		{
			container.JobContainer.JC_DepartureEstimatedPickup = estimatedDelivery;
			container.JobContainer.JC_DepartureCartageAdvised = cartageAdvised;
			container.JobContainer.JC_DepartureCartageComplete = cartageComplete;
		}

		#endregion

		#region TestSettingSupplier

		public virtual void TestSettingSupplier()
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			TestDec.JE_TransportMode = Constants.TransportModes.Sea;
			TestDec.CusContainers.AddNew();
			TestDec.CusContainers[0].CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			PartyInCurrentCountry.MainAddress.OA_FCLEquipmentNeeded = "XYZ";
			TestDec.JE_OH_Supplier = PartyInCurrentCountry.PK;

			AssertEquals("Origin set", PartyInCurrentCountry.OH_RL_NKClosestPort, TestDec.JE_RL_NKOrigin);
			if (!TestDec.JE_MessageTypeInfo.ReadOnly)
			{
				AssertEquals("Export as supplier is in this country", DefaultExportMessageType, TestDec.JE_MessageType);
			}
			AssertEquals("FCL Equipment", "XYZ", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded);
		}

		protected virtual string DefaultExportMessageType
		{
			get { return Customs.Business.JobMessageTypeList.Codes.Export; }
		}

		#endregion

		public void TestISupportDataImporting()
		{
			TestDec.IsImportingData = true;
			AssertEquals("IsImportingData", true, TestDec.IsImportingData);
		}

		public void TestISupportUXMLDataImporting()
		{
			TestDec.IsUXMLImportingData = true;
			AssertEquals("IsUXMLImportingData", true, TestDec.IsUXMLImportingData);
		}

		public void TestCustomLabelsConfigOrg()
		{
			AssertNull("Without a config org should return null", TestDec.ConfigOrg);

			OrgHeader importer = OrgHeader.New(Factory);
			OrgHeader supplier = OrgHeader.New(Factory);
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_Supplier = supplier.PK;

			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("ConfigOrg is Supplier", supplier.PK, TestDec.ConfigOrg.PK);
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("ConfigOrg is Importer", importer.PK, TestDec.ConfigOrg.PK);
		}

		public void TestShipmentReferenceNumberAssignedOnSaving()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration testDec = GetJobDeclaration();
			testDec.JE_JS = shipment.PK;
			Factory.Save();

			AssertEquals("Shipment Number is assigned", shipment.JS_UniqueConsignRef, testDec.JE_DeclarationReference);
		}

		public void TestStand_AloneDeclarationReferenceAssignedOnSaving()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Factory.Save();
			Assert("Unique reference number is assigned", !declaration.JE_DeclarationReference.IsEmpty);
		}

		public void TestGetterShipment()
		{
			AssertEquals("PreCondition:Shipment is null", null, TestDec.Shipment);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment1.PK;
			AssertEquals("Shipment 1", shipment1, TestDec.Shipment);

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment2.PK;
			AssertEquals("Shipment 2", shipment2, TestDec.Shipment);

			TestDec.JE_JS = ZGuid.Empty;
			AssertEquals("Shipment is now null", null, TestDec.Shipment);
		}

		public void TestAirlinePrefix()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertEquals(ZString.Empty, TestDec.AirlinePrefix);
			TestDec.JE_VoyageFlightNo = "QF1";
			AssertEquals("QF", TestDec.AirlinePrefix);
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			AssertEquals(ZString.Empty, TestDec.AirlinePrefix);
		}

		public virtual void TestOverrideFreightDefaultsSetsReadOnlyFalse()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			TestDec.JE_JS = shipment.PK;
			TestDec.JE_OverrideFreightDefaults = true;

			TestDec.JE_OverrideFreightDefaults = false;
			AssertEquals("TestDec.JE_OH_SupplierInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults, because JE_OH_Supplier is blank", false, TestDec.JE_OH_SupplierInfo.ReadOnly);
			AssertEquals("TestDec.JE_OH_ImporterInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults, because JE_OH_Importer is blank", false, TestDec.JE_OH_ImporterInfo.ReadOnly);
			AssertEquals("TestDec.JE_RL_NKOriginInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals("TestDec.JE_RL_NKFinalDestinationInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateAtOriginInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_DateAtOriginInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateAtFinalDestinationInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_DateAtFinalDestinationInfo.ReadOnly);
			AssertEquals("TestDec.JE_HouseBillInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_HouseBillInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalWeightInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalWeightInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalWeightUnitInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalWeightUnitInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalVolumeInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalVolumeInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalVolumeUnitInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalVolumeUnitInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalNoOfPacksInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalNoOfPacksInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalNoOfPacksPackTypeInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
			AssertEquals("TestDec.JE_GoodsDescriptionInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_GoodsDescriptionInfo.ReadOnly);
			AssertEquals("TestDec.JE_ShipmentIncoTermInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_ShipmentIncoTermInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateOfArrivalInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_DateOfArrivalInfo.ReadOnly);
			AssertEquals("TestDec.JE_ExportDateInfo.ReadOnly should be true after setting JE_OverrideFreightDefaults", true, TestDec.JE_ExportDateInfo.ReadOnly);

			TestDec.JE_OverrideFreightDefaults = true;
			AssertEquals("TestDec.JE_OH_SupplierInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_OH_SupplierInfo.ReadOnly);
			AssertEquals("TestDec.JE_OH_ImporterInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_OH_ImporterInfo.ReadOnly);
			AssertEquals("TestDec.JE_RL_NKOriginInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_RL_NKOriginInfo.ReadOnly);
			AssertEquals("TestDec.JE_RL_NKFinalDestinationInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_RL_NKFinalDestinationInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateAtOriginInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_DateAtOriginInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateAtFinalDestinationInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_DateAtFinalDestinationInfo.ReadOnly);
			AssertEquals("TestDec.JE_HouseBillInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_HouseBillInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalWeightInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalWeightInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalWeightUnitInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalWeightUnitInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalVolumeInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalVolumeInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalVolumeUnitInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalVolumeUnitInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalNoOfPacksInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalNoOfPacksInfo.ReadOnly);
			AssertEquals("TestDec.JE_TotalNoOfPacksPackTypeInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_TotalNoOfPacksPackTypeInfo.ReadOnly);
			AssertEquals("TestDec.JE_GoodsDescriptionInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_GoodsDescriptionInfo.ReadOnly);
			AssertEquals("TestDec.JE_ShipmentIncoTermInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_ShipmentIncoTermInfo.ReadOnly);
			AssertEquals("TestDec.JE_DateOfArrivalInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_DateOfArrivalInfo.ReadOnly);
			AssertEquals("TestDec.JE_ExportDateInfo.ReadOnly should be false after setting JE_OverrideFreightDefaults", false, TestDec.JE_ExportDateInfo.ReadOnly);
		}

		#region Clone Tests

		public void TestClone()
		{
			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)TestDec.Clone();
			AssertNotNull("CLone should have a valid DocsAndCartage object", clonedDeclaration.DocsAndCartage);
			AssertEquals("Clone should have a different DocsAndCartage object", true, (TestDec.DocsAndCartage.PK != clonedDeclaration.DocsAndCartage.PK));
		}

		public virtual void TestCloneHasChanges()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "cnumber";
			BaseJobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "inumber";
			invoiceHeader.JobComInvoiceLines.AddNew();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)declaration.TemplateCopy();

			AssertEquals("Container Count", 0, clonedDeclaration.CusContainers.Count);
			AssertEquals("Invoice Group Header Count", declaration.JobComInvoiceGroupHeaders.Count, clonedDeclaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Invoice Count", declaration.Invoices.Count, clonedDeclaration.Invoices.Count);
			AssertEquals("Invoice Line Count", declaration.Invoices[0].JobComInvoiceLines.Count, clonedDeclaration.Invoices[0].JobComInvoiceLines.Count);

			AssertEquals("Declaration hasn't clones DocsAndCartage properly.", clonedDeclaration.PK, clonedDeclaration.DocsAndCartage.JP_ParentID);
		}

		public virtual void TestCloneHouseBills()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			Bill houseBill = declaration.Bills.AddNew();
			Bill houseBill2 = declaration.Bills.AddNew();
			AssertEquals("PreConditition: two house bills", 2, declaration.Bills.Count);

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)declaration.TemplateCopy();
			AssertEquals("House bills are cloned", 0, clonedDeclaration.Bills.Count);
		}

		public virtual void TestResetValuesOnTemplateCopyAfterClone()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageStatus = "MSG";
			declaration.JE_ConsolidatedCargoStatus = "HLD";
			declaration.JE_EntryDate = ZDate.Today;
			AssertEquals("Precondition: message status not empty", true, !declaration.JE_MessageStatus.IsEmpty);
			AssertEquals("Precondition: consolidated cargo status not empty", true, !declaration.JE_ConsolidatedCargoStatus.IsEmpty);
			AssertEquals("Precondition: entry date not empty", true, !declaration.JE_EntryDate.IsEmpty);

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)declaration.TemplateCopy();
			AssertEquals("Message status is empty on cloned dec", true, clonedDeclaration.JE_MessageStatus.IsEmpty);
			AssertEquals("Consolidated cargo status is empty on cloned dec", true, clonedDeclaration.JE_ConsolidatedCargoStatus.IsEmpty);
			AssertEquals("Entry date is empty on cloned dec", true, clonedDeclaration.JE_EntryDate.IsEmpty);
		}

		public virtual void TestResetValuesAfterCloningACancelledJob()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_IsCancelled = true;
			declaration.JE_AuditDateUtc = ZDateTime.UtcNow;
			declaration.JE_GS_NKAuditUser = staff.GS_Code;
			declaration.JE_AuditReference = "ABC123";

			var clonedDeclaration = (BaseJobDeclaration)Declaration.TemplateCopy();
			AssertEquals("Declaration should not be cancelled", false, clonedDeclaration.JE_IsCancelled);
			AssertEquals("Audit data should be cleared.", ZDateTime.Empty, clonedDeclaration.JE_AuditDateUtc);
			AssertEquals("Audit data should be cleared.", ZString.Empty, clonedDeclaration.JE_GS_NKAuditUser);
			AssertEquals("Audit data should be cleared.", ZString.Empty, clonedDeclaration.JE_AuditReference);
		}

		public void TestMessageStatusConcurrency()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_MessageStatus = "MSG";
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			BaseJobDeclaration testDecAgain = factory1.Load<BaseJobDeclaration>(testDec.PK);
			testDecAgain.JE_Folio = "123";

			BaseJobDeclaration testDecLoaded = factory2.Load<BaseJobDeclaration>(testDec.PK);
			testDecLoaded.JE_MessageStatus = "XXX";

			bool gotException = false;
			try
			{
				factory2.Save();
			}
			catch
			{
				gotException = true;
			}
			Assert("Should be able to save", !gotException);

			testDecAgain.JE_MessageStatus = "YYY";

			gotException = false;
			try
			{
				factory1.Save();
			}
			catch
			{
				gotException = true;
			}

			Assert("Should not be able to save", gotException);
			AssertEquals("User2 value not changed", "XXX", testDecLoaded.JE_MessageStatus);

			gotException = false;
			try
			{
				factory1.Save();
			}
			catch
			{
				gotException = true;
			}

			ErrorReporter.Clear(); // stop any developer notifications happening

			Assert("Should still not be able to save", gotException);
		}

		#endregion

		public void TestSetDefaultsOnOrder()
		{
			((IBusinessObjectInternals)TestDec).IsCopying = true;
			try
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var supplier = Factory.NewWithValidTestData<OrgHeader>();

				TestDec.JE_OH_Importer = importer.PK;
				TestDec.JE_OH_Supplier = supplier.PK;
				TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
				TestDec.JE_ContainerMode = "";
				TestDec.JE_GoodsDescription = new string('A', TestDec.JE_GoodsDescriptionInfo.MaxLength);
				TestDec.JE_RS_NKServiceLevel = "SVC";
				TestDec.JE_ShipmentIncoTerm = "FOB";
			}
			finally
			{
				((IBusinessObjectInternals)TestDec).IsCopying = false;
			}

			var order = TestDec.AttachedOrders.AddNew();
			AssertEquals(order.BuyerPK, TestDec.JE_OH_Importer);
			AssertEquals(order.SupplierPK, TestDec.JE_OH_Supplier);
			AssertEquals(order.JD_TransportMode, TestDec.JE_TransportMode);
			AssertEquals(order.JD_ContainerMode, "");
			AssertEquals(order.JD_OrderGoodsDescription, TestDec.JE_GoodsDescription.SubstringSafe(0, order.JD_OrderGoodsDescriptionInfo.MaxLength));
			AssertEquals(order.JD_RS_NKServiceLevel_NI, TestDec.JE_RS_NKServiceLevel);
			AssertEquals(order.JD_IncoTerm, TestDec.JE_ShipmentIncoTerm);

			var container = TestDec.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			order = TestDec.AttachedOrders.AddNew();
			AssertEquals(order.JD_ContainerMode, Core.Constants.ContainerModes.FCL);
		}

		public void TestSetDefaultsOnOrderWhileImporting()
		{
			((IBusinessObjectInternals)TestDec).IsCopying = true;
			try
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var supplier = Factory.NewWithValidTestData<OrgHeader>();

				TestDec.JE_OH_Importer = importer.PK;
				TestDec.JE_OH_Supplier = supplier.PK;
				TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
				TestDec.JE_ContainerMode = "";
				TestDec.JE_GoodsDescription = new string('A', TestDec.JE_GoodsDescriptionInfo.MaxLength);
				TestDec.JE_RS_NKServiceLevel = "SVC";
				TestDec.JE_ShipmentIncoTerm = "FOB";
			}
			finally
			{
				((IBusinessObjectInternals)TestDec).IsCopying = false;
			}

			var emptyOrder = Factory.New<Order>();

			using (new DisposableAction(() => TestDec.IsImportingData = true, () => TestDec.IsImportingData = false))
			{
				var order = TestDec.AttachedOrders.AddNew();
				AssertEquals(ZGuid.Empty, order.BuyerPK);
				AssertEquals(ZGuid.Empty, order.SupplierPK);
				AssertEquals(emptyOrder.JD_TransportMode, order.JD_TransportMode);
				AssertEquals(emptyOrder.JD_ContainerMode, order.JD_ContainerMode);
				AssertEquals(ZString.Empty, order.JD_OrderGoodsDescription);
				AssertEquals(ZString.Empty, order.JD_RS_NKServiceLevel_NI);
				AssertEquals(emptyOrder.JD_IncoTerm, order.JD_IncoTerm);

				var container = TestDec.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

				order = TestDec.AttachedOrders.AddNew();
				AssertEquals(emptyOrder.JD_ContainerMode, order.JD_ContainerMode);
			}
		}

		public void TestJobDocsAndCartageExportImportProxyPropertiesAndInfos()
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsLocalTransport = true;
			carrier.OH_FullName = "Carrier";

			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.DocsAndCartage.JP_DeliveryCartageAdvised = new ZDateTime(2004, 1, 1);
			TestDec.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2004, 2, 1);
			TestDec.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2004, 3, 1);
			TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "XXX";
			TestDec.DocsAndCartage.JP_DeliveryLabourTime = new ZDateTime(2006, 1, 1, 4, 0, 0);
			TestDec.DocsAndCartage.JP_DeliveryLabourCharge = 5;
			TestDec.DocsAndCartage.JP_DeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 6, 0, 0);
			TestDec.DocsAndCartage.JP_DeliveryTruckWaitCharge = 7;
			TestDec.DocsAndCartage.DeliveryCartageCoPK = carrier.PK;
			TestDec.DocsAndCartage.JP_DeliveryRequiredBy = new ZDateTime(2004, 9, 1);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryCartageAdvised, TestDec.JP_Calc_CartageAdvised);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryCartageAdvisedInfo, ((ZWrappedPropertyInfo)TestDec.JP_Calc_CartageAdvisedInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryCartageCompleted, TestDec.JE_CartageCompleted);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryCartageCompletedInfo, ((ZWrappedPropertyInfo)TestDec.JE_CartageCompletedInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_EstimatedDelivery, TestDec.JE_EstimatedDeliveryOrPickup);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryCartageCompletedInfo, ((ZWrappedPropertyInfo)TestDec.JE_CartageCompletedInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded, TestDec.JE_FCLDeliveryOrPickupEquipmentNeeded);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeededInfo, ((ZWrappedPropertyInfo)TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryLabourTime, TestDec.JE_DeliveryOrPickupLabourTime);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryLabourTimeInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupLabourTimeInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryLabourCharge, TestDec.JE_DeliveryOrPickupLabourCharge);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryLabourChargeInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupLabourChargeInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryTruckWaitTime, TestDec.JE_PickupOrDeliveryTruckWaitTime);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryTruckWaitTimeInfo, ((ZWrappedPropertyInfo)TestDec.JE_PickupOrDeliveryTruckWaitTimeInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryTruckWaitCharge, TestDec.JE_PickupOrDeliveryTruckWaitCharge);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryTruckWaitChargeInfo, ((ZWrappedPropertyInfo)TestDec.JE_PickupOrDeliveryTruckWaitChargeInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_OA_DeliveryCartageCoAddr, TestDec.JE_OA_DeliveryOrPickupCartageCoAddr);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo, ((ZWrappedPropertyInfo)TestDec.JE_OA_DeliveryOrPickupCartageCoAddrInfo).InnerInfo);

			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryRequiredBy, TestDec.JE_DeliveryOrPickupRequiredBy);
			AssertEquals("For import", TestDec.DocsAndCartage.JP_DeliveryRequiredByInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupRequiredByInfo).InnerInfo);

			AssertHasErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");
			TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "";
			AssertNoErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");

			//having an invalid value in Pickup, should not cause error when Delivery is expected...
			TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "YYY";
			AssertNoErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");

			TestDec.JE_MessageType = DefaultExportMessageType;

			TestDec.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2004, 1, 2);
			TestDec.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2004, 2, 2);
			TestDec.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2004, 3, 2);
			TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "YYY";
			TestDec.DocsAndCartage.JP_PickupLabourTime = new ZDateTime(2006, 1, 1, 14, 0, 0);
			TestDec.DocsAndCartage.JP_PickupLabourCharge = 15;
			TestDec.DocsAndCartage.JP_PickupTruckWaitTime = new ZDateTime(2006, 1, 1, 16, 0, 0);
			TestDec.DocsAndCartage.JP_PickupTruckWaitCharge = 17;
			TestDec.DocsAndCartage.PickupCartageCoPK = carrier.PK;
			TestDec.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2004, 9, 2);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupCartageAdvised, TestDec.JP_Calc_CartageAdvised);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupCartageAdvisedInfo, ((ZWrappedPropertyInfo)TestDec.JP_Calc_CartageAdvisedInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupCartageCompleted, TestDec.JE_CartageCompleted);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupCartageCompletedInfo, ((ZWrappedPropertyInfo)TestDec.JE_CartageCompletedInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_EstimatedPickup, TestDec.JE_EstimatedDeliveryOrPickup);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_EstimatedPickupInfo, TestDec.JE_EstimatedDeliveryOrPickupInfo.InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded, TestDec.JE_FCLDeliveryOrPickupEquipmentNeeded);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeededInfo, ((ZWrappedPropertyInfo)TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupLabourTime, TestDec.JE_DeliveryOrPickupLabourTime);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupLabourTimeInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupLabourTimeInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupLabourCharge, TestDec.JE_DeliveryOrPickupLabourCharge);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupLabourChargeInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupLabourChargeInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupTruckWaitTime, TestDec.JE_PickupOrDeliveryTruckWaitTime);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupTruckWaitTimeInfo, ((ZWrappedPropertyInfo)TestDec.JE_PickupOrDeliveryTruckWaitTimeInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupTruckWaitCharge, TestDec.JE_PickupOrDeliveryTruckWaitCharge);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupTruckWaitChargeInfo, ((ZWrappedPropertyInfo)TestDec.JE_PickupOrDeliveryTruckWaitChargeInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_OA_PickupCartageCoAddr, TestDec.JE_OA_DeliveryOrPickupCartageCoAddr);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo, ((ZWrappedPropertyInfo)TestDec.JE_OA_DeliveryOrPickupCartageCoAddrInfo).InnerInfo);

			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupRequiredBy, TestDec.JE_DeliveryOrPickupRequiredBy);
			AssertEquals("For export", TestDec.DocsAndCartage.JP_PickupRequiredByInfo, ((ZWrappedPropertyInfo)TestDec.JE_DeliveryOrPickupRequiredByInfo).InnerInfo);

			AssertHasErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");
			TestDec.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "";
			AssertNoErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");

			//having an invalid value in Delivery, should not cause error when Pickup is expected...
			TestDec.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "YYY";
			AssertNoErrorContaining(TestDec.JE_FCLDeliveryOrPickupEquipmentNeededInfo, "Enter a valid");
		}

		public virtual void TestBondedWarehouseEditable()
		{
			AssertEquals("Can edit on export declaration", true, TestDec.BondedWarehouseEditable);

			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Can edit on import declaration", true, TestDec.BondedWarehouseEditable);

			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("Can't edit on drawback declaration", false, TestDec.BondedWarehouseEditable);
		}

		public virtual void TestCartageAdvisedCaption()
		{
			TestDec.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Docs to carrier on export", "Docs to Carrier", TestDec.CartageAdvisedCaption);

			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Port Transport Advised on import", "Port Trn. Advised", TestDec.CartageAdvisedCaption);
		}

		public void TestDeleteWithShipmentAttached()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			TestDec.Delete();
			AssertEquals("Shouldn't delete cartage as the shipment is attached has it", false, shipment.DocsAndCartage.IsDeleted);
		}

		public void TestMessagesIncludingInterchangeRejections()
		{
			AssertNotNull(TestDec.MessagesIncludingInterchangeRejections);
		}

		public void TestLogsOfDeclarationOrShipment()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			Logs logs1 = TestDec.LogsOfDeclarationOrShipment;
			TestDec.JE_JS = shipment.PK;
			Logs logs2 = TestDec.LogsOfDeclarationOrShipment;

			AssertNotNull("DecLogs", logs1);
			AssertNotNull("ShipmentLogs", logs2);
			Assert("DecLogs != ShipmentLogs", logs1 != logs2);
		}

		#region TestIsValidToSendToCustoms
		public void TestIsValidToSendToCustoms()
		{
			SetupInvoice(TestDec);
			Factory.Save();

			BaseJobDeclaration loadedDec = new BusinessObjectFactory().Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Children not registered editable yet", false, IsInvoiceHeaderCollectionRegistered(loadedDec));
			AssertEquals("IsValidToSendToCustoms", true, loadedDec.IsValidToSendToCustoms);
			AssertEquals("Children registered before checking for errors", true, IsInvoiceHeaderCollectionRegistered(loadedDec));
		}

		protected virtual void SetupInvoice(BaseJobDeclaration declartion)
		{
			BaseJobComInvoiceHeader invoice = declartion.Invoices.AddNew();
		}

		public void TestInvoiceHeadersNotAutomaticallyLoadedWithDeclaration()
		{
			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			Factory.Save();

			BaseJobDeclaration loadedDec = new BusinessObjectFactory().Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Invoice header not loaded yet", false, IsInvoiceHeaderCollectionRegistered(loadedDec));
		}

		bool IsInvoiceHeaderCollectionRegistered(BaseJobDeclaration declaration)
		{
			IBusiness[] children = ((IBusiness)declaration).Children;
			return Array.Exists(children, child => child is InvoiceHeaderActiveCollection);
		}
		#endregion

		#region TestLogCustomsCommencedIfNeeded
		public virtual void TestLogCustomsCommencedIfNeeded()
		{
			TestDec.JE_MessageType = DefaultImportMessageType;
			TestDec.JE_GS_NKCusAgent = "XZX";
			DoTestLogCustomsCommencedIfNeeded(Events.CustomsCommenced, TestDec);
		}

		public virtual void TestLogCustomsCommencedIfNeeded_ForExport()
		{
			TestDec.JE_MessageType = DefaultExportMessageType;
			TestDec.JE_GS_NKCusAgent = "XZX";
			DoTestLogCustomsCommencedIfNeeded(Events.ExportCustomsCommenced, TestDec);
		}

		void AddBrokerLicenceForCurrentUser()
		{
			GenRegCertAccredMaintList brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;
		}

		[TestDate(2022, 04, 27)]
		protected virtual void DoTestLogCustomsCommencedIfNeeded(Event customsCommencedEvent, BaseJobDeclaration testDec)
		{
			var saveCurrentUserIsSystemAccount = GlbStaff.CurrentUser.GS_IsSystemAccount;
			var saveCurrentUserCode = GlbStaff.CurrentUser.GS_Code;
			try
			{
				var today = ZDateTime.Today;
				GlbStaff.CurrentUser.GS_IsSystemAccount = false;
				GlbStaff.CurrentUser.GS_Code = "XZX";
				testDec.LogCustomsCommencedIfNeeded();
				var secondCommencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertEquals("TestDec.JE_GS_NKCusAgent", "XZX", testDec.JE_GS_NKCusAgent);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, secondCommencedLog.SL_Reference.Left(2));
				AssertEquals("TestDec.JE_CustomsCommencedDate", today, testDec.JE_CustomsCommencedDate.Date);
				AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", "XZX", testDec.JE_GS_NKCustomsCommencedUser);

				GlbStaff.CurrentUser.GS_Code = "ZXZ";
				testDec.CancelCustomsEvents();
				testDec.LogCustomsCommencedIfNeeded();
				var thirdCommencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertEquals("TestDec.JE_GS_NKCusAgent updated when message is first sent to customs", "ZXZ", testDec.JE_GS_NKCusAgent);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, thirdCommencedLog.SL_Reference.Left(2));
				AssertEquals("TestDec.JE_CustomsCommencedDate", today, testDec.JE_CustomsCommencedDate.Date);
				AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", "ZXZ", testDec.JE_GS_NKCustomsCommencedUser);

				using (thirdCommencedLog.LockForUpdatingKeyFieldsForTesting())
				{
					thirdCommencedLog.SL_IsEstimate = true;
				}
				testDec.LogCustomsCommencedIfNeeded();
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, customsCommencedEvent.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, testDec.PK);
				AssertEquals("A new log should have been added", 3, Factory.Load<StmALog>(query).Length);
				testDec.JE_GS_NKCusAgent = "";
				testDec.LogCustomsCommencedIfNeeded();
				AssertEquals("TestDec.JE_GS_NKCusAgent updated when message is sent to customs and Broker is not set", "ZXZ", testDec.JE_GS_NKCusAgent);
				AssertEquals("TestDec.JE_CustomsCommencedDate", today, testDec.JE_CustomsCommencedDate.Date);
				AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", "ZXZ", testDec.JE_GS_NKCustomsCommencedUser);

				AddBrokerLicenceForCurrentUser();
				testDec.LogCustomsCommencedIfNeeded();
				var commencedLog = testDec.Logs.MostRecentLogByEventTime(customsCommencedEvent, GetBranchQuery());
				AssertNotNull(commencedLog);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, commencedLog.SL_Reference.Left(2));
				AssertEquals("TestDec.JE_CustomsCommencedDate", today, testDec.JE_CustomsCommencedDate.Date);
				AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", "ZXZ", testDec.JE_GS_NKCustomsCommencedUser);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = saveCurrentUserIsSystemAccount;
				GlbStaff.CurrentUser.GS_Code = saveCurrentUserCode;
			}
		}
		#endregion

		#region TestCancelCustomsCommenced
		public void TestCancelCustomsCommenced()
		{
			TestDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			DoTestCancelCustomsCommenced(Events.CustomsCommenced);
		}

		public virtual void TestCancelCustomsCommenced_ForExport()
		{
			TestDec.JE_MessageType = DefaultExportMessageType;
			DoTestCancelCustomsCommenced(Events.ExportCustomsCommenced);
		}

		[TestDate(2022, 04, 27)]
		void DoTestCancelCustomsCommenced(Event customsCommencedEvent)
		{
			var today = ZDateTime.Today;
			GlbStaff.CurrentUser.GS_Code = "XXX";

			TestDec.CancelCustomsEvents();
			TestDec.LogsOfDeclarationOrShipment.AddNew(customsCommencedEvent, ZDateTimeOffset.Now.AddHours(1), ZBool.True);
			AssertNull(TestDec.Logs.MostRecentLogByEventTimeExcludingEstimated(customsCommencedEvent, GetBranchQuery()));
			AssertNull(TestDec.Logs.MostRecentLogByEventTime(Events.Cancelled));
			AssertEquals("TestDec.JE_CustomsCommencedDate", ZDateTime.Empty, TestDec.JE_CustomsCommencedDate);
			AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", ZString.Empty, TestDec.JE_GS_NKCustomsCommencedUser);

			TestDec.LogCustomsCommencedIfNeeded();
			AssertNotNull(TestDec.Logs.MostRecentLogByEventTimeExcludingEstimated(customsCommencedEvent, GetBranchQuery()));
			AssertEquals("TestDec.JE_CustomsCommencedDate", today, TestDec.JE_CustomsCommencedDate.Date);
			AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", "XXX", TestDec.JE_GS_NKCustomsCommencedUser);

			TestDec.CancelCustomsEvents();
			AssertNull("IsCancelled", TestDec.Logs.MostRecentLogByEventTimeExcludingEstimated(customsCommencedEvent, GetBranchQuery()));
			AssertNotNull("CancelledEvent", TestDec.Logs.MostRecentLogByEventTime(Events.Cancelled));
			AssertEquals("TestDec.JE_CustomsCommencedDate", ZDateTime.Empty, TestDec.JE_CustomsCommencedDate);
			AssertEquals("TestDec.JE_GS_NKCustomsCommencedUser", ZString.Empty, TestDec.JE_GS_NKCustomsCommencedUser);
		}

		public ZQuery GetBranchQuery()
		{
			return new ZQuery(StmALogSchema.SL_GB_NKBranch, GlbCompany.CurrentCompany.ActiveBranches.Cast<GlbBranch>().Select(x => x.GB_Code));
		}
		#endregion

		public void TestLogCustomsImpediment()
		{
			TestDec.LogCustomsImpediment();
			AssertNotNull(TestDec.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived));
		}

		#region TestLogCustomsClearedIfNeeded
		[TestDate(2024, 6, 30)]
		public void TestLogCustomsClearedIfNeeded()
		{
			var testDec = GetTestDecForTestLogCustomsCleared(Customs.Business.JobMessageTypeList.Codes.Import);
			DoTestLogCustomsClearedIfNeeded(testDec, Events.CustomsCleared);
		}

		[TestDate(2024, 6, 30)]
		public void TestLogCustomsClearedIfNeeded_ForExport()
		{
			var testDec = GetTestDecForTestLogCustomsCleared(DefaultExportMessageType);
			DoTestLogCustomsClearedIfNeeded(testDec, Events.ExportCustomsCleared);
		}

		void DoTestLogCustomsClearedIfNeeded(BaseJobDeclaration testDec, Event customsCleared)
		{
			testDec.LogCustomsClearedIfNeeded();
			var log = testDec.Logs.MostRecentLogByEventTime(customsCleared);
			AssertNotNull(log);
			AssertEquals(ExpectedReferenceForLogCustomsClearedIfNeeded, log.SL_Reference.Left(2));
			AssertEquals(GetExpectedEventTimeOffsetForLogCustomsClearedIfNeeded(testDec), log.EventTimeOffset);

			testDec.LogCustomsClearedIfNeeded();
			AssertEquals(log, testDec.Logs.MostRecentLogByEventTime(customsCleared));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}
			testDec.LogCustomsClearedIfNeeded();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, customsCleared.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, testDec.PK);
			AssertEquals("A new log should have been added", 2, Factory.Load<StmALog>(query).Length);
		}

		protected virtual ZString ExpectedReferenceForLogCustomsClearedIfNeeded => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected virtual ZDateTimeOffset GetExpectedEventTimeOffsetForLogCustomsClearedIfNeeded(BaseJobDeclaration declaration) => ZDateTimeOffset.Now;

		protected virtual BaseJobDeclaration GetTestDecForTestLogCustomsCleared(ZString messageType)
		{
			TestDec.JE_MessageType = messageType;
			return TestDec;
		}

		#endregion

		public virtual void TestDefaultINCOFromOrgLink()
		{
			var currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			var otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));

			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
			var consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";

			BaseJobDeclaration dec = GetJobDeclaration();

			var link1 = consignee.SupplierLinks.AddNew(consignor);
			var link1TrnModeSea = link1.OrgSupBuyLinkTrnModes[0];
			link1TrnModeSea.PF_IncoTerm = "123";
			link1TrnModeSea.PF_TransportMode = dec.TransportModeSeaCodeForTesting;
			link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;

			var link1TrnModeAir = link1.OrgSupBuyLinkTrnModes.AddNew();
			link1TrnModeAir.PF_IncoTerm = "789";
			link1TrnModeAir.PF_TransportMode = dec.TransportModeAirCodeForTesting;

			var link2 = consignee.SupplierLinks.AddNew(consignor);
			var link2TrnModSea = link2.OrgSupBuyLinkTrnModes[0];
			link2TrnModSea.PF_IncoTerm = "456";
			link2TrnModSea.PF_TransportMode = dec.TransportModeSeaCodeForTesting;
			link2.OL_RN_NKImporterCountry = otherCountry2.RN_Code;

			dec.JE_OH_Importer = consignee.PK;
			dec.JE_OH_Supplier = consignor.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			dec.JE_RL_NKFinalDestination = currentPort.RL_Code;
			AssertEquals("Defaulted Inco Term", "789", dec.JE_ShipmentIncoTerm);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals("Defaulted Inco Term", "123", dec.JE_ShipmentIncoTerm);

			dec.JE_RL_NKFinalDestination = otherUnloco2.RL_Code;
			AssertEquals("Defaulted Inco Term", "456", dec.JE_ShipmentIncoTerm);
		}

		public virtual void TestDefaultINCOFromSupplier()
		{
			OrgHeader consignor = OrgHeader.New(Factory);
			consignor.MiscServ.OM_EXDefaultIncoTerm = "321";
			TestDec.JE_OH_Supplier = consignor.PK;
			AssertEquals("Defaulted Inco Term", "321", TestDec.JE_ShipmentIncoTerm);
		}

		public virtual void TestDefaultINCOFromImporter()
		{
			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.MiscServ.OM_IMDefaultINCOTerm = "150";
			TestDec.JE_OH_Importer = consignee.PK;

			AssertEquals("Defaulted Inco Term", "150", TestDec.JE_ShipmentIncoTerm);
		}

		public virtual void TestDefaultLoadPortATDAndDischargePortATA()
		{
			TestDec.JE_RL_NKOrigin = "NZAKL";
			TestDec.JE_DateAtOrigin = new ZDateTime(2004, 10, 30);
			TestDec.JE_RL_NKFinalDestination = "AUSYD";
			TestDec.JE_DateAtFinalDestination = new ZDateTime(2004, 11, 1);

			AssertEquals("Loading", TestDec.JE_RL_NKOrigin, TestDec.JE_RL_NKPortOfLoading);
			AssertEquals("Discharge", TestDec.JE_RL_NKFinalDestination, TestDec.JE_RL_NKPortOfArrival);
			AssertEquals("ATD", TestDec.JE_DateAtOrigin, TestDec.JE_ExportDate);
			AssertEquals("ATA", TestDec.JE_DateAtFinalDestination, TestDec.JE_DateOfArrival);
		}

		public void TestPhysicalAddressForExporter()
		{
			OrgHeader exporter = OrgHeader.New(Factory);
			exporter.OH_Code = "EXPORT";
			exporter.OH_FullName = "Exporter";
			exporter.MainAddress.OA_Address1 = "Main address 1";
			exporter.MainAddress.OA_Address2 = "Main address 2";
			exporter.MainAddress.OA_City = "Main City";

			OrgAddress postalAddress = exporter.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal address 1";
			postalAddress.OA_Address2 = "Postal address 2";
			postalAddress.OA_City = "Post City";
			OrgAddress salesAddress = exporter.Addresses.AddNew();
			salesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Sales.Code);
			salesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Sales.Code);
			salesAddress.OA_Address1 = "Sales address 1";
			salesAddress.OA_Address2 = "Sales address 2";
			salesAddress.OA_City = "Sales City";
			TestDec.JE_OH_Supplier = exporter.PK;
			Factory.Save();
			AssertEquals("Should always have a physical address (OFC)", exporter.MainAddress, TestDec.PhysicalAddressForExporter);

			OrgAddress pickupAddress = exporter.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			pickupAddress.OA_Address1 = "Pickup address 1";
			pickupAddress.OA_Address2 = "Pickup address 2";
			pickupAddress.OA_City = "Pickup City";
			Factory.Save();

			OrgAddress result = TestDec.PhysicalAddressForExporter;
			AssertEquals("Should return OFC address (as first one entered)", typeof(OrgAddress), result.GetType());
			AssertEquals("Address 1", "Main address 1", result.OA_Address1);
			AssertEquals("Address 2", "Main address 2", result.OA_Address2);
			AssertEquals("City", "Main City", result.OA_City);
		}

		public void TestPhysicalAddressForImporter()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_Code = "Import";
			importer.OH_FullName = "Importer";
			importer.MainAddress.OA_Address1 = "Main address 1";
			importer.MainAddress.OA_Address2 = "Main address 2";
			importer.MainAddress.OA_City = "Main City";

			OrgAddress postalAddress = importer.Addresses.AddNew();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal address 1";
			postalAddress.OA_Address2 = "Postal address 2";
			postalAddress.OA_City = "Post City";
			OrgAddress salesAddress = importer.Addresses.AddNew();
			salesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Sales.Code);
			salesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Sales.Code);
			salesAddress.OA_Address1 = "Sales address 1";
			salesAddress.OA_Address2 = "Sales address 2";
			salesAddress.OA_City = "Sales City";
			TestDec.JE_OH_Importer = importer.PK;
			Factory.Save();
			AssertEquals("Should always have a physical address (OFC)", importer.MainAddress, TestDec.PhysicalAddressForImporter);

			OrgAddress pickupAddress = importer.Addresses.AddNew();
			pickupAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			pickupAddress.OA_Address1 = "Pickup address 1";
			pickupAddress.OA_Address2 = "Pickup address 2";
			pickupAddress.OA_City = "Pickup City";
			Factory.Save();

			OrgAddress result = TestDec.PhysicalAddressForImporter;
			AssertEquals("Should return OFC address (as first one entered)", typeof(OrgAddress), result.GetType());
			AssertEquals("Address 1", "Main address 1", result.OA_Address1);
			AssertEquals("Address 2", "Main address 2", result.OA_Address2);
			AssertEquals("City", "Main City", result.OA_City);
		}

		public void TestActiveFilter()
		{
			TestDec.JE_IsCancelled = true;
			BaseJobDeclaration[] retrievedDeclarations = (BaseJobDeclaration[])Factory.Load(typeof(BaseJobDeclaration), new ZQuery(JobDeclarationSchema.PK, TestDec.PK));
			AssertEquals("Should not be able to load inactive declaration", 0, retrievedDeclarations.Length);

			TestDec.JE_IsCancelled = false;
			retrievedDeclarations = (BaseJobDeclaration[])Factory.Load(typeof(BaseJobDeclaration), new ZQuery(JobDeclarationSchema.PK, TestDec.PK));
			AssertEquals("Should be able to load active declaration", 1, retrievedDeclarations.Length);
		}

		public void TestIsMultiSupplier()
		{
			BaseJobComInvoiceHeader invoiceHeader1 = TestDec.Invoices.AddNew();
			OrgHeader supplier1 = OrgHeader.New(Factory);
			supplier1.OH_Code = "1234";
			invoiceHeader1.JZ_OH_Supplier = supplier1.PK;

			BaseJobComInvoiceHeader invoiceHeader2 = TestDec.Invoices.AddNew();
			OrgHeader supplier2 = OrgHeader.New(Factory);
			supplier2.OH_Code = "1234";
			invoiceHeader2.JZ_OH_Supplier = supplier2.PK;

			AssertEquals(TestDec.IsMultiSupplier, true);
		}

		public void TestConsignmentValue()
		{
			var invoiceHeader1 = TestDec.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 1111;

			var invoiceHeader2 = TestDec.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 2222;

			AssertEquals(TestDec.ConsignmentValue, new ZDecimal(3333));
		}

		public void TestDontOverwriteJE_DateOfFirstArrivalOnCloning()
		{
			TestDec.JE_RL_NKPortOfFirstArrival = "AUSYD";
			TestDec.JE_RL_NKPortOfArrival = "AUSYD";
			TestDec.JE_DateOfArrival = ZDateTime.Today;
			TestDec.JE_DateOfFirstArrival = ZDateTime.Empty;

			BaseJobDeclaration clonedDec = (BaseJobDeclaration)TestDec.Clone();
			AssertEquals("JE_DateOfFirstArrival", ZDateTime.Empty, clonedDec.JE_DateOfFirstArrival);
		}

		public void TestTotalFOB()
		{
			TestDec.Invoices.DeleteAll();
			BaseJobComInvoiceHeader header1 = TestDec.Invoices.AddNew();
			BaseJobComInvoiceHeader header2 = TestDec.Invoices.AddNew();
			header1.JZ_InvoiceAmount = 100m;
			header1.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			header2.JZ_InvoiceAmount = 200m;
			header2.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			AssertEquals(TestDec.TotalFOB.Amount, 300m);
		}

		public void TestDisableApportionmentForInvoiceCharge()
		{
			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			BaseInvoiceCharge invCharge = invoice.Charges.AddNew();
			invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			invCharge.J7_Amount = 1000m;
			invCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 5000m;

			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 4000m;

			AssertEquals("Line1 shouldn;t have apportioned charge as it is disabled", 0, line1.ApportionedCharges.Count);
			AssertEquals("Line2 shouldn;t have apportioned charge as it is disabled", 0, line2.ApportionedCharges.Count);
		}

		public void TestResumeApportionmentForInvoiceCharge()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var invoice = TestDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000;
				invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";
				var invCharge = invoice.Charges.AddNew();
				PrepareCharge(invCharge);
				invCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				invCharge.J7_Amount = 1000m;
				invCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

				var line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 9000m;

				AssertEquals("PreCondition:Line shouldn;t have apportioned charge as it is disabled", 0, line.ApportionedCharges.Count);

				TestDec.ResumeApportionment();
				AssertEquals("Line should have an apportioned charge now", 1, line.ApportionedCharges.Count);
			}
		}

		public void TestDisableApportionmentForGroupCharge()
		{
			BaseGroupInvoiceCharge grpCharge = TestDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			grpCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			grpCharge.J7_Amount = 1000m;
			grpCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			AssertEquals("Invoice doesnt have an apportioned charge as the apoprtionment is disabled", 0, invoice.GroupCharges.Count);
			AssertEquals("Line doesnt have an apportioned charge as the apportionment is disabled", 0, line.ApportionedCharges.Count);
		}

		public void TestResumeApportionmentForGroupCharge()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				TestDec.AutoCreateChargesBasedOnIncoTerm = false;
				var grpCharge = TestDec.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				PrepareCharge(grpCharge);
				grpCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				grpCharge.J7_Amount = 1000m;
				grpCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

				var invoice = TestDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000;
				invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 10000m;

				AssertEquals("PreCondition:Invoice doesnt have an apportioned charge as the apoprtionment is disabled", 0, invoice.GroupCharges.Count);
				AssertEquals("PreCondition:Line doesnt have an apportioned charge as the apportionment is disabled", 0, line.ApportionedCharges.Count);

				TestDec.ResumeApportionment();
				AssertEquals("Invoice has an apportioned charge as the apportionment is resumed", 1, invoice.GroupCharges.Count);
				AssertEquals("Line has an apportioned charge as the apportionment is resumed", 1, line.ApportionedCharges.Count);
			}
		}

		public void TestDisableApportionmentForLineCharge()
		{
			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			lineCharge.J7_Amount = 1000m;
			lineCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

			AssertEquals("Invoice doesn't have an apportioned charge from line", 0, invoice.GroupCharges.Count);
		}

		public void TestResumeApportionmentForLineCharge()
		{
			BaseJobComInvoiceHeader invoice = TestDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";

			BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;
			BaseInvoiceLineCharge lineCharge = line.Charges.AddNew();

			lineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			lineCharge.J7_Amount = 1000m;
			lineCharge.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;

			AssertEquals("Invoice doesn't have an apportioned charge from line", 0, invoice.GroupCharges.Count);

			TestDec.ResumeApportionment();
			AssertEquals("Invoice has an apportioned charge from line", 1, invoice.GroupCharges.Count);
		}

		public virtual void TestDisableResultApportionmentWithRealInvoice2()
		{
			using (DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var invoice = TestDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 3581.21m;
				invoice.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";

				var invFIFT = invoice.Charges.AddNew();
				PrepareCharge(invFIFT);
				invFIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
				invFIFT.J7_Amount = 55.85m;
				invFIFT.J7_RX_NKCurrency = TestDec.LocalCurrencyCode;
				invFIFT.J7_IsIncludedInITOT = false;

				var invDIS = invoice.Charges.AddNew();
				PrepareCharge(invDIS);
				invDIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				invDIS.J7_Percentage = 0.75m;

				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.JI_LinePrice = 360;

				var line2 = invoice.JobComInvoiceLines.AddNew();
				line2.JI_LinePrice = 2880m;

				var line3 = invoice.JobComInvoiceLines.AddNew();
				line3.JI_LinePrice = 312m;
				var line3DIS = line3.Charges.AddNew();
				PrepareCharge(line3DIS);
				line3DIS.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				line3DIS.J7_Amount = 10.34m;

				TestDec.ResumeApportionment();

				AssertEquals("Line1 FIFT", 5.66m, line1.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);
				AssertEquals("Line2 FIFT", 45.28m, line2.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);
				AssertEquals("Line3 FIFT", 4.91m, line3.ApportionedCharges.GetCharge(invFIFT.ChargeKey).Amount);

				AssertEquals("Line1 Apportioned DIS", 2.70m, line1.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
				AssertEquals("Line2 Apportioned DIS", 21.60m, line2.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);
				AssertEquals("Line3 Apportioned DIS", 2.34m, line3.ApportionedCharges.GetCharge(invDIS.ChargeKey).Amount);

				AssertEquals("Invoice DIS", 26.64m, invDIS.J7_Amount);
			}
		}

		protected virtual void PrepareCharge(Common.JobComInvCharge charge)
		{
		}

		public virtual void TestEntryStatusChangedLogged()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			Factory.Save();

			declaration.JE_EntryStatus = Events.CustomsCommenced.Code;
			Factory.Save();

			ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			filter.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			AssertEquals("Event Created", 1, Factory.GetDatabaseCount(typeof(StmALog), filter));

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			Factory.Save();
			AssertEquals("Event Created", 1, Factory.GetDatabaseCount(typeof(StmALog), filter));
		}

		#region TestJE_OwnerRefTakenFromNumberFountain

		public void TestJE_OwnerRefTakenFromNumberFountainTrue()
		{
			TestJE_OwnerRefTakenFromNumberFountain(true);
		}

		public void TestJE_OwnerRefTakenFromNumberFountainFalse()
		{
			TestJE_OwnerRefTakenFromNumberFountain(false);
		}

		void TestJE_OwnerRefTakenFromNumberFountain(bool autoImpJobRefered)
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			importer.OH_Code = "FRED";
			importer.MiscServ.OM_IMAutoImpJobRefered = autoImpJobRefered;
			TestDec.JE_OH_Importer = importer.PK;
			AssertEquals("AutoAssignImporterRef", autoImpJobRefered, TestDec.AutoAssignImporterRef);
			AssertEquals("Readonly", autoImpJobRefered, TestDec.JE_OwnerRefInfo.ReadOnly);
			bool gotException = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				gotException = true;
			}
			Assert("Should be able to save", !gotException);
			AssertEquals("OwnerRef", autoImpJobRefered ? "1" : "", TestDec.JE_OwnerRef);
			OrgHeader importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "FRED";// force factory.save error by duplicated org
			TestDec.JE_OwnerRef = "";
			gotException = false;
			try
			{
				Factory.Save();
			}
			catch
			{
				gotException = true;
			}
			Assert("Should not be able to save", gotException);
			AssertEquals("OwnerRef should be blank", "", TestDec.JE_OwnerRef);
		}

		#endregion

		public void TestPackagesCollection()
		{
			BaseJobDeclaration testDec = GetJobDeclaration();
			using (testDec.SuspendSettingHasChanges())
			{
				Bill houseBill1 = testDec.Bills.AddNew();
				houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
				houseBill1.CU_HouseBill = "HOUSEBILL1";

				BasePackingGroup packingGroup1 = (houseBill1.PackingGroups.Count > 0) ? houseBill1.PackingGroups[0] : houseBill1.PackingGroups.AddNew();
				BasePackage package1 = (packingGroup1.Packages.Count > 0) ? packingGroup1.Packages[0] : packingGroup1.Packages.AddNew();
				package1.CW_PackQty = 1;
				package1.CW_PackType = "11";
			}

			var collection = testDec.Packages;

			AssertEquals("Collection.Count", 1, collection.Count);

			AssertEquals("Collection[0].CW_PackQty", 1, collection[0].CW_PackQty);
			AssertEquals("Collection[0].CW_PackType", "11", collection[0].CW_PackType);

			Factory.Save();
			AssertEquals("Precondition: Declaration.HasChanges", false, testDec.HasChanges);
			testDec.Packages[0].CW_PackQty = 123;
			AssertEquals("Declaration.HasChanges After Change to Child Collection", true, testDec.HasChanges);
		}

		#region TestMessageTypeForDocumentFilter
		public virtual void TestMessageTypeForDocumentFilter()
		{
			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, TestDec.MessageTypeForDocumentFilter);

			TestDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Import type", JobMessageTypeList.Codes.Import, TestDec.MessageTypeForDocumentFilter);

			TestDec.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Export type", JobMessageTypeList.Codes.Export, TestDec.MessageTypeForDocumentFilter);

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Export type", JobMessageTypeList.Codes.Drawback, TestDec.MessageTypeForDocumentFilter);
		}

		public virtual void TestMessageTypesForDocumentFilter()
		{
			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals("Import type", "," + JobMessageTypeList.Codes.Import + ",", TestDec.MessageTypesForDocumentFilter);

			TestDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("Import type", "," + JobMessageTypeList.Codes.Import + ",", TestDec.MessageTypesForDocumentFilter);

			TestDec.JE_MessageType = DefaultExportMessageType;
			AssertEquals("Export type", "," + JobMessageTypeList.Codes.Export + ",", TestDec.MessageTypesForDocumentFilter);

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("Export type", "," + JobMessageTypeList.Codes.Drawback + ",", TestDec.MessageTypesForDocumentFilter);

			TestDec.JE_MessageType = ZString.Empty;
			AssertEquals("Empty type", "", TestDec.MessageTypesForDocumentFilter);
		}
		#endregion

		public virtual void TestMessageTypeForHSAssist()
		{
			TestDec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("Import type", SharedJobMessageTypeList.Codes.Import, TestDec.MessageTypeForHSAssist);
			TestDec.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Export type", SharedJobMessageTypeList.Codes.Export, TestDec.MessageTypeForHSAssist);
			TestDec.JE_MessageType = "INV";
			AssertEquals("Invalid Type still returned", "INV", TestDec.MessageTypeForHSAssist);
		}

		public void TestShouldDefaultFCLCartageCo()
		{
			SetUpNewTestDec(DefaultImportMessageType, "", "");

			AssertEquals("Precondition: No containers should not use FCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);

			TestDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("Should use FCL cartage", true, TestDec.ShouldDefaultFCLCartageCo);
			TestDec.JE_ContainerMode = ZString.Empty;
			TestDec.JE_TransportMode = ZString.Empty;

			BaseCusContainer container = TestDec.CusContainers.AddNew();

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			AssertEquals("Import FCL containers should use FCL cartage", true, TestDec.ShouldDefaultFCLCartageCo);

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("Import FCX containers should use FCL cartage", true, TestDec.ShouldDefaultFCLCartageCo);

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			AssertEquals("Import LCL containers should not use FCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);

			SetUpNewTestDec(DefaultExportMessageType, "", "");

			AssertEquals("Precondition: No containers should not use FCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);

			TestDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			TestDec.JE_ContainerMode = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("Should use LCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);
			TestDec.JE_ContainerMode = ZString.Empty;
			TestDec.JE_TransportMode = ZString.Empty;

			container = TestDec.CusContainers.AddNew();

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FullContainerLoad;
			AssertEquals("Export FCL containers should use FCL cartage", true, TestDec.ShouldDefaultFCLCartageCo);

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.FCX;
			AssertEquals("Export FCX containers should not use FCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);

			container.CO_FCL_LCL_AIR = BaseCusContainer.ContainerModes.LessContainerLoad;
			AssertEquals("Export LCL containers should not use FCL cartage", false, TestDec.ShouldDefaultFCLCartageCo);
		}

		#region TestDefaultImportCartage

		public void TestDeliveryCartagePartyFromImporter()
		{
			var roadCartage = Factory.NewWithValidTestData<OrgHeader>();
			roadCartage.OH_IsShippingProvider = true;
			roadCartage.OH_IsLocalTransport = true;

			var importer = Factory.New<OrgHeader>();
			importer.SetRelatedParty(roadCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Road, ZString.Empty);

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = DefaultImportMessageType;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("DeliveryCartageCo not set", null, declaration.DocsAndCartage.DeliveryCartageCo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("DeliveryCartageCo is set", roadCartage, declaration.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestPickupCartagePartyFromSupplier()
		{
			var roadCartage = Factory.NewWithValidTestData<OrgHeader>();
			roadCartage.OH_IsShippingProvider = true;
			roadCartage.OH_IsLocalTransport = true;

			var supplier = Factory.New<OrgHeader>();
			supplier.SetRelatedParty(roadCartage, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Core.Constants.TransportModes.Road, ZString.Empty);

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = DefaultExportMessageType;
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("PickupCartageCo not set", null, declaration.DocsAndCartage.PickupCartageCo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("PickupCartageCo is set", roadCartage, declaration.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultImportCartageWithDeliveryAddressLoaded()
		{
			var currentBranchLocationCode = GlbCompany.CurrentCompany.Branches[0].GB_RL_NKHomePort;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.Addresses[0].OA_RL_NKRelatedPortCode = currentBranchLocationCode;

			OrgHeader orgFCLCartageCo = Factory.New<OrgHeader>();
			orgFCLCartageCo.Addresses[0].OA_RL_NKRelatedPortCode = currentBranchLocationCode;
			orgFCLCartageCo.OH_IsShippingProvider = true;
			orgFCLCartageCo.OH_IsLocalTransport = true;

			OrgHeader orgLCLCartageCo = Factory.New<OrgHeader>();
			orgLCLCartageCo.Addresses[0].OA_RL_NKRelatedPortCode = currentBranchLocationCode;
			orgLCLCartageCo.OH_IsShippingProvider = true;
			orgLCLCartageCo.OH_IsLocalTransport = true;

			consignee.SetRelatedParty(orgFCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL, currentBranchLocationCode);
			consignee.SetRelatedParty(orgLCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			var origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");
			var destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUPER");

			SetUpNewTestDec(JobMessageTypeList.Codes.Import, origin.RL_Code, destination.RL_Code);
			TestDec.JE_OH_Importer = consignee.PK;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;

			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("If importer is set, cartage should default from importer's default cartage", orgFCLCartageCo.PK, TestDec.DocsAndCartage.DeliveryCartageCoPK);

			TestDec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("If importer is set, cartage should default from importer's default cartage", orgLCLCartageCo.PK, TestDec.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultImportCartageWithoutRegistry_NoImporter()
		{
			const string destinationPort = "AUPER";

			var dec = CreateImportDeclaration(destinationPort);
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;

			AssertNull("Precondition - importer not set", dec.Importer);
			AssertNull("Precondition - no branch for current company exists at origin", GlbCompany.CurrentCompany.FirstBranchForUnLoco(dec.Origin));
			AssertNull("Precondition - no branch for current company exists at destination", GlbCompany.CurrentCompany.FirstBranchForUnLoco(dec.FinalDestination));

			AssertNull("If no registry value exists for current branch and importer is not set, cartage should not default to anything", dec.DocsAndCartage.DeliveryCartageCo);
		}

		public void TestDefaultImportCartageWithRegistry_NoImporter_Containers()
		{
			const string destinationPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);

			void AssertDeliveryCartageCoPK(string message, string co_fcl_lcl_air, ZGuid expectedPK)
			{
				var dec = CreateImportDeclaration(destinationPort);

				var container = dec.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = co_fcl_lcl_air;

				dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

				AssertEquals(message, expectedPK, dec.DocsAndCartage.DeliveryCartageCoPK);
			}

			AssertDeliveryCartageCoPK("If importer is not set, cartage should default from registry setting for destination branch based on the attached containers", Core.Constants.ContainerModes.FCLMixedShipper, regValue.BranchFCLCartagePK);
			AssertDeliveryCartageCoPK("If importer is not set, cartage should default from registry setting for destination branch based on the attached containers", Core.Constants.ContainerModes.FCL, regValue.BranchFCLCartagePK);
			AssertDeliveryCartageCoPK("If importer is not set, cartage should default from registry setting for destination branch based on the attached containers", Core.Constants.ContainerModes.LCL, regValue.BranchLCLCartagePK);
		}

		public void TestDefaultImportCartageWithRegistry_NoImporter_TransportMode()
		{
			const string destinationPort = "AUPER";

			var dec = CreateImportDeclaration(destinationPort);
			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);

			dec.JE_TransportMode = dec.TransportModeMailCodeForTesting;
			AssertEquals($"If transport mode is {dec.TransportModeMailCodeForTesting}, cartage stays blank", ZGuid.Empty, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals("If importer is not set and there are no containers on the declaration, cartage should default from registry default LCL cartage", regValue.BranchLCLCartagePK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change mode to {dec.TransportModeAirCodeForTesting} should change default to registry default AIR cartage", regValue.BranchAIRCartagePK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals($"Change mode back to {dec.TransportModeSeaCodeForTesting} should change default to registry default LCL cartage", regValue.BranchLCLCartagePK, dec.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultImportCartageWithRegistry_NoImporter_ContainerMode()
		{
			const string destinationPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);

			var dec = CreateImportDeclaration(destinationPort);
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

			void AssertDeliveryCartageCoPK(string message, string containerMode, ZGuid expectedPK)
			{
				dec.JE_ContainerMode = containerMode;
				AssertEquals(message, expectedPK, dec.DocsAndCartage.DeliveryCartageCoPK);
			}

			AssertDeliveryCartageCoPK("Change Container mode to FCL, with no containers, should change default to registry default FCL cartage", Core.Constants.ContainerModes.FCL, regValue.BranchFCLCartagePK);
			AssertDeliveryCartageCoPK("Change Container mode from FCL tp B/B, with no containers, should change default to registry default LCL cartage", Core.Constants.ContainerModes.BreakBulk, regValue.BranchLCLCartagePK);
		}

		public void TestDefaultImportCartageWithRegistry_NoImporter_ManuallyChange()
		{
			const string destinationPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);

			var otherCartageCo = Factory.New<OrgHeader>();
			otherCartageCo.OH_IsShippingProvider = true;
			otherCartageCo.OH_IsLocalTransport = true;

			var dec = CreateImportDeclaration(destinationPort);
			dec.DocsAndCartage.DeliveryCartageCoPK = otherCartageCo.PK;

			AssertEquals("Precondition - Cartage Co manually set to another cartage co", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change mode to {dec.TransportModeAirCodeForTesting}, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Change transport and container modes, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change to {dec.TransportModeAirCodeForTesting}, after clearing maual override, should now change to registry defailt AIR Cartage", regValue.BranchAIRCartagePK, dec.DocsAndCartage.DeliveryCartageCoPK);
		}

		public void TestDefaultImportCartageWithRegistryAndImporter_TransportMode()
		{
			const string destinationPort = "AUPER";

			SetupCartageRegistryValuesWithNewBranch(destinationPort);

			var dec = CreateImportDeclaration(destinationPort);

			var importerWithParties = CreateImporterWithRelatedParties();
			dec.JE_OH_Importer = importerWithParties.Importer.PK;

			void AssertDeliveryCartageCoPK(string message, string transportMode, ZGuid expectedPK)
			{
				dec.JE_TransportMode = transportMode;
				dec.JE_ContainerMode = Constants.ContainerModes.LCL;

				AssertEquals(message, expectedPK, dec.DocsAndCartage.DeliveryCartageCoPK);
			}

			AssertDeliveryCartageCoPK($"If transport mode is {dec.TransportModeMailCodeForTesting}, cartage stays blank", dec.TransportModeMailCodeForTesting, ZGuid.Empty);
			AssertDeliveryCartageCoPK($"Change mode to {dec.TransportModeAirCodeForTesting} should change default to importer's default AIR cartage", dec.TransportModeAirCodeForTesting, importerWithParties.OrgAIRCartageCoPK);
			AssertDeliveryCartageCoPK($"Change mode back to {dec.TransportModeSeaCodeForTesting} should change default to importer's default LCL cartage", dec.TransportModeSeaCodeForTesting, importerWithParties.OrgLCLCartageCoPK);
		}

		public void TestDefaultImportCartageWithRegistryAndImporter_ContainerMode()
		{
			const string destinationPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);
			var importerWithParties = CreateImporterWithRelatedParties();

			var dec = CreateImportDeclaration(destinationPort);
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_OH_Importer = importerWithParties.Importer.PK;

			void AssertDeliveryCartageCoPK(string message, string containerMode, ZGuid expectedPK)
			{
				dec.JE_ContainerMode = containerMode;
				AssertEquals(message, expectedPK, dec.DocsAndCartage.DeliveryCartageCoPK);
			}

			AssertDeliveryCartageCoPK("If importer is set and there are no containers on the declaration, cartage should default from importer's default LCL cartage", Constants.ContainerModes.LCL, importerWithParties.OrgLCLCartageCoPK);
			AssertDeliveryCartageCoPK("Change Container mode to FCL, with no containers, should change default to importer's default FCL cartage", Constants.ContainerModes.FCL, importerWithParties.OrgFCLCartageCoPK);
			AssertDeliveryCartageCoPK("Change Container mode from FCL to B/B, with no containers, as there is no default BBK cartage on importer, should use the org of LCLCartageCompany", Constants.ContainerModes.BreakBulk, regValue.BranchLCLCartagePK);
		}

		public void TestDefaultImportCartageWithRegistryAndImporter_Containers()
		{
			const string destinationPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(destinationPort);
			var importerWithParties = CreateImporterWithRelatedParties();

			void AssertDeliveryCartageCoPK(string message, string co_fcl_lcl_air, Func<BaseJobDeclaration, ZGuid> getExpectedPKFunc)
			{
				var dec = CreateImportDeclaration(destinationPort);
				dec.JE_OH_Importer = importerWithParties.Importer.PK;

				var container = dec.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = co_fcl_lcl_air;

				dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
				AssertEquals(message, getExpectedPKFunc(dec), dec.DocsAndCartage.DeliveryCartageCoPK);
			}

			AssertDeliveryCartageCoPK
			(
				"If importer is set, cartage should default from importer's default cartage based on the attached containers",
				Core.Constants.ContainerModes.FCL,
				(d) => importerWithParties.OrgFCLCartageCoPK
			);

			AssertDeliveryCartageCoPK
			(
				"If importer is set, cartage should default from importer's default cartage based on the attached containers",
				Core.Constants.ContainerModes.FCLMixedShipper,
				(d) => importerWithParties.OrgFCLCartageCoPK
			);

			AssertDeliveryCartageCoPK
			(
				"If importer is set, cartage should default from branch or importer's default cartage based on the attached containers",
				Core.Constants.ContainerModes.LCL,
				(d) => d.ContainerModeForCartage == Core.Constants.ContainerModes.LCL ? importerWithParties.OrgLCLCartageCoPK : regValue.BranchLCLCartagePK
			);
		}

		public void TestDefaultImportCartageWithRegistryAndImporter_ManuallyChange()
		{
			const string destinationPort = "AUPER";

			SetupCartageRegistryValuesWithNewBranch(destinationPort);

			var otherCartageCo = Factory.New<OrgHeader>();
			otherCartageCo.OH_IsShippingProvider = true;
			otherCartageCo.OH_IsLocalTransport = true;

			var importerWithParties = CreateImporterWithRelatedParties();

			var dec = CreateImportDeclaration(destinationPort);
			dec.JE_OH_Importer = importerWithParties.Importer.PK;

			dec.DocsAndCartage.DeliveryCartageCoPK = otherCartageCo.PK;
			AssertEquals("Precondition - Cartage Co manually set to another cartage co", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change mode to {dec.TransportModeAirCodeForTesting}, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals("Change transport and container modes, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.DeliveryCartageCoPK);

			dec.DocsAndCartage.DeliveryCartageCoPK = ZGuid.Empty;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change to {dec.TransportModeAirCodeForTesting}, after clearing maual override, should now change to importer's defailt AIR Cartage", importerWithParties.OrgAIRCartageCoPK, dec.DocsAndCartage.DeliveryCartageCoPK);
		}

		BaseJobDeclaration CreateImportDeclaration(string destinationPort)
		{
			var result = Factory.New<BaseJobDeclaration>();
			result.JE_MessageType = DefaultImportMessageType;
			result.JE_RL_NKOrigin = "INBOM";
			result.JE_RL_NKFinalDestination = destinationPort;

			return result;
		}

		(OrgHeader Importer, ZGuid OrgAIRCartageCoPK, ZGuid OrgFCLCartageCoPK, ZGuid OrgLCLCartageCoPK) CreateImporterWithRelatedParties()
		{
			var importer = Factory.New<OrgHeader>();

			var orgAIRCartageCo = Factory.New<OrgHeader>();
			orgAIRCartageCo.OH_IsShippingProvider = true;
			orgAIRCartageCo.OH_IsLocalTransport = true;

			var orgFCLCartageCo = Factory.New<OrgHeader>();
			orgFCLCartageCo.OH_IsShippingProvider = true;
			orgFCLCartageCo.OH_IsLocalTransport = true;

			var orgLCLCartageCo = Factory.New<OrgHeader>();
			orgLCLCartageCo.OH_IsShippingProvider = true;
			orgLCLCartageCo.OH_IsLocalTransport = true;

			importer.SetRelatedParty(orgAIRCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			importer.SetRelatedParty(orgFCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			importer.SetRelatedParty(orgLCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			return (importer, orgAIRCartageCo.PK, orgFCLCartageCo.PK, orgLCLCartageCo.PK);
		}

		(ZGuid BranchAIRCartagePK, ZGuid BranchLCLCartagePK, ZGuid BranchFCLCartagePK) SetupCartageRegistryValuesWithNewBranch(string homePort)
		{
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			branch.GB_RL_NKHomePort = homePort;

			var branchAIRCartage = Factory.New<OrgHeader>();
			branchAIRCartage.OH_IsShippingProvider = true;
			branchAIRCartage.OH_IsLocalTransport = true;

			var branchLCLCartage = Factory.New<OrgHeader>();
			branchLCLCartage.OH_IsShippingProvider = true;
			branchLCLCartage.OH_IsLocalTransport = true;

			var branchFCLCartage = Factory.New<OrgHeader>();
			branchFCLCartage.OH_IsShippingProvider = true;
			branchFCLCartage.OH_IsLocalTransport = true;

			FreightDataRegistry.Instance.AIRCartageCompany.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, branchAIRCartage.PK.ToGuid());
			FreightDataRegistry.Instance.LCLCartageCompany.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, branchLCLCartage.PK.ToGuid());
			FreightDataRegistry.Instance.FCLCartageCompany.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, branchFCLCartage.PK.ToGuid());

			return (branchAIRCartage.PK, branchLCLCartage.PK, branchFCLCartage.PK);
		}

		#endregion

		#region TestDefaultExportCartage

		public void TestDefaultExportCartageWithoutRegistry_NoSupplier()
		{
			const string originPort = "AUPER";

			var dec = CreateExportDeclaration(originPort);
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;

			AssertNull("Precondition - supplier not set", dec.Supplier);

			AssertNull("Precondition - no branch for current company exists at origin", GlbCompany.CurrentCompany.FirstBranchForUnLoco(dec.Origin));
			AssertNull("Precondition - no branch for current company exists at destination", GlbCompany.CurrentCompany.FirstBranchForUnLoco(dec.FinalDestination));

			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNull("If no registry value exists for current branch and supplier is not set, cartage should not default to anything", dec.DocsAndCartage.PickupCartageCo);
		}

		public void TestDefaultExportCartageWithRegistry_NoSupplier_TransportMode()
		{
			const string originPort = "AUPER";

			var dec = CreateExportDeclaration(originPort);
			var regValue = SetupCartageRegistryValuesWithNewBranch(originPort);

			AssertNull("Precondition - supplier not set", dec.Supplier);

			dec.JE_TransportMode = dec.TransportModeMailCodeForTesting;
			AssertEquals($"If transport mode is {dec.TransportModeMailCodeForTesting}, cartage stays blank", ZGuid.Empty, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"If supplier is not set and transport mode is {dec.TransportModeAirCodeForTesting}, cartage should default from AIR registry setting for origin branch", regValue.BranchAIRCartagePK, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			AssertEquals("If supplier is not set and there are no containers on the declaration, cartage should default from LCL registry setting for origin branch", regValue.BranchLCLCartagePK, dec.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultExportCartageWithRegistry_NoSupplier_Containers()
		{
			const string originPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(originPort);

			void AssertPickupCartageCoPK(string message, string co_fcl_lcl_air, Func<BaseJobDeclaration, ZGuid> getExpectedPKFunc)
			{
				var dec = CreateExportDeclaration(originPort);

				var container = dec.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = co_fcl_lcl_air;

				dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

				AssertEquals(message, getExpectedPKFunc(dec), dec.DocsAndCartage.PickupCartageCoPK);
			}

			AssertPickupCartageCoPK
			(
				"If supplier is not set, cartage should default from registry setting for origin branch based on the attached containers",
				Core.Constants.ContainerModes.FCL,
				(d) => regValue.BranchFCLCartagePK
			);

			AssertPickupCartageCoPK
			(
				"If supplier is not set, cartage should default from registry setting for origin branch based on the attached containers",
				Core.Constants.ContainerModes.LCL,
				(d) => regValue.BranchLCLCartagePK
			);
		}

		public void TestDefaultExportCartageWithRegistryAndSupplier_TransportMode()
		{
			const string originPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(originPort);
			var supplierWithParties = CreateSupplierWithRelatedParties();

			var dec = CreateExportDeclaration(originPort);
			dec.JE_OH_Supplier = supplierWithParties.Supplier.PK;

			dec.JE_TransportMode = dec.TransportModeMailCodeForTesting;
			AssertEquals($"If transport mode is {dec.TransportModeMailCodeForTesting}, cartage stays blank", ZGuid.Empty, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"If supplier is set and transport mode is {dec.TransportModeAirCodeForTesting}, cartage should default from supplier's default air cartage", supplierWithParties.OrgAIRCartageCoPK, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

			var expectedPk = dec.ContainerModeForCartage == Core.Constants.ContainerModes.LCL ? supplierWithParties.OrgLCLCartageCoPK : regValue.BranchLCLCartagePK;

			AssertEquals("If supplier is set and there are no containers on the declaration, cartage should default from branch or supplier's default LCL cartage", expectedPk, dec.DocsAndCartage.PickupCartageCoPK);
		}

		public void TestDefaultExportCartageWithRegistryAndSupplier_ContainerMode()
		{
			const string originPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(originPort);

			var supplierWithParties = CreateSupplierWithRelatedParties();

			var dec = CreateExportDeclaration(originPort);
			dec.JE_OH_Supplier = supplierWithParties.Supplier.PK;
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

			void AssertDeliveryCartageCoPK(string message, string containerMode, ZGuid expectedPK)
			{
				dec.JE_ContainerMode = containerMode;
				AssertEquals(message, expectedPK, dec.DocsAndCartage.PickupCartageCoPK);
			}

			AssertDeliveryCartageCoPK("If supplier is set and there are no containers on the declaration, cartage should default from supplier's default LCL cartage", Constants.ContainerModes.LCL, supplierWithParties.OrgLCLCartageCoPK);
			AssertDeliveryCartageCoPK("Change Container mode to FCL, with no containers, should change default to supplier's default FCL cartage", Constants.ContainerModes.FCL, supplierWithParties.OrgFCLCartageCoPK);
			AssertDeliveryCartageCoPK("Change Container mode from FCL to B/B, with no containers, as there is no default BBK cartage on supplier, should use the org of LCLCartageCompany", Constants.ContainerModes.BreakBulk, regValue.BranchLCLCartagePK);
		}

		public void TestDefaultExportCartageWithRegistryAndSupplier_Containers()
		{
			const string originPort = "AUPER";

			var regValue = SetupCartageRegistryValuesWithNewBranch(originPort);
			var supplierWithParties = CreateSupplierWithRelatedParties();

			void AssertPickupCartageCoPK(string message, string co_fcl_lcl_air, Func<BaseJobDeclaration, ZGuid> getExpectedPKFunc)
			{
				var dec = CreateExportDeclaration(originPort);
				dec.JE_OH_Supplier = supplierWithParties.Supplier.PK;

				var container = dec.CusContainers.AddNew();
				container.CO_FCL_LCL_AIR = co_fcl_lcl_air;

				dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;

				AssertEquals(message, getExpectedPKFunc(dec), dec.DocsAndCartage.PickupCartageCoPK);
			}

			AssertPickupCartageCoPK
			(
				"If supplier is set, cartage should default from supplier's default cartage based on the attached containers",
				Core.Constants.ContainerModes.FCL,
				(d) => supplierWithParties.OrgFCLCartageCoPK
			);

			AssertPickupCartageCoPK
			(
				"If supplier is set, cartage should default from branch or supplier's default cartage based on the attached containers",
				Core.Constants.ContainerModes.LCL,
				(d) => d.ContainerModeForCartage == Core.Constants.ContainerModes.LCL ? supplierWithParties.OrgLCLCartageCoPK : regValue.BranchLCLCartagePK
			);
		}

		public void TestDefaultExportCartageWithRegistryAndSupplier_ManuallyChange()
		{
			const string originPort = "AUPER";

			SetupCartageRegistryValuesWithNewBranch(originPort);

			var supplierWithParties = CreateSupplierWithRelatedParties();

			var otherCartageCo = Factory.New<OrgHeader>();
			otherCartageCo.OH_IsShippingProvider = true;
			otherCartageCo.OH_IsLocalTransport = true;

			var dec = CreateExportDeclaration(originPort);
			dec.JE_OH_Supplier = supplierWithParties.Supplier.PK;

			dec.DocsAndCartage.PickupCartageCoPK = otherCartageCo.PK;
			AssertEquals("Precondition - Cartage Co manually set to another cartage co", otherCartageCo.PK, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change mode to {dec.TransportModeAirCodeForTesting}, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.PickupCartageCoPK);

			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_ContainerMode = Core.Constants.ContainerModes.Combination;
			AssertEquals("Change transport and container modes, after manual change, should have no effect", otherCartageCo.PK, dec.DocsAndCartage.PickupCartageCoPK);

			dec.DocsAndCartage.PickupCartageCoPK = ZGuid.Empty;
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			AssertEquals($"Change to {dec.TransportModeAirCodeForTesting}, after clearing maual override, should now change to supplier's defailt {dec.TransportModeAirCodeForTesting} Cartage", supplierWithParties.OrgAIRCartageCoPK, dec.DocsAndCartage.PickupCartageCoPK);
		}

		BaseJobDeclaration CreateExportDeclaration(string originPort)
		{
			var result = Factory.New<BaseJobDeclaration>();
			result.JE_MessageType = DefaultExportMessageType;
			result.JE_RL_NKOrigin = originPort;
			result.JE_RL_NKFinalDestination = "INBOM";

			return result;
		}

		(OrgHeader Supplier, ZGuid OrgAIRCartageCoPK, ZGuid OrgFCLCartageCoPK, ZGuid OrgLCLCartageCoPK) CreateSupplierWithRelatedParties()
		{
			var supplier = Factory.New<OrgHeader>();

			var orgAIRCartageCo = Factory.New<OrgHeader>();
			orgAIRCartageCo.OH_IsShippingProvider = true;
			orgAIRCartageCo.OH_IsLocalTransport = true;

			var orgFCLCartageCo = Factory.New<OrgHeader>();
			orgFCLCartageCo.OH_IsShippingProvider = true;
			orgFCLCartageCo.OH_IsLocalTransport = true;

			var orgLCLCartageCo = Factory.New<OrgHeader>();
			orgLCLCartageCo.OH_IsShippingProvider = true;
			orgLCLCartageCo.OH_IsLocalTransport = true;

			supplier.SetRelatedParty(orgAIRCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			supplier.SetRelatedParty(orgFCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			supplier.SetRelatedParty(orgLCLCartageCo, RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			return (supplier, orgAIRCartageCo.PK, orgFCLCartageCo.PK, orgLCLCartageCo.PK);
		}

		void SetUpNewTestDec(string messageType, string origin, string destination)
		{
			fTestDec = BaseJobDeclaration.New(Factory);
			TestDec.JE_MessageType = messageType;
			TestDec.JE_RL_NKOrigin = origin;
			TestDec.JE_RL_NKFinalDestination = destination;
		}

		#endregion

		#region ConcurrencyPolicySet

		public void TestConcurrencyPolicySet()
		{
			var declaration = GetJobDeclaration();
			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedDec = (ConcurrencyBaseJobDeclaration)reloadFactory.Load(typeof(ConcurrencyBaseJobDeclaration), declaration.PK);
			reloadedDec.JE_AddInfo = "ABC";
			AssertEquals("Default strategy - not set yet", ConcurrencyPolicy.Default, reloadedDec.JE_EntryStatusInfo.ConcurrencyPolicy);
			reloadedDec.JE_EntryStatus = "XYZ";

			AssertEquals("Strict strategy - property updated", ConcurrencyPolicy.Strict, reloadedDec.JE_EntryStatusInfo.ConcurrencyPolicy);
			reloadedDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			reloadFactory.Save();
			Assert("On saving called, and test inside declaration subclass was run", reloadedDec.OnSavingCalled);
		}

		class ConcurrencyBaseJobDeclaration : BaseJobDeclaration
		{
			public ConcurrencyBaseJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public bool OnSavingCalled;
			public override void OnSaving()
			{
				OnSavingCalled = true;
				base.OnSaving();
				AssertEquals("Strategy: No merge", ConcurrencyPolicy.Strict, JE_EntryStatusInfo.ConcurrencyPolicy);
			}
		}

		public void TestConcurrencyPolicySaveAfterSending()
		{
			var strictProperties = new Func<BaseJobDeclaration, ZPropertyInfo>[]
			{
				d => d.JE_EntryStatusInfo,
				d => d.JE_MessageStatusInfo,
				d => d.JE_ConsolidatedCargoStatusInfo,
				d => d.JE_MessageTypeInfo,
				d => d.JE_ApplicationCodeInfo
			};

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Consignee";
			consignee.MiscServ.OM_IMImporterRequiresOrderNumbersOnDocs = true;
			Factory.Save();

			foreach (bool shouldFail in new bool[] { false, true })
			{
				foreach (var property in strictProperties)
				{
					var factory = NewFactory();

					var arrivalDate = ZDateTime.Today;
					var dec = factory.New<BaseJobDeclaration>();
					dec.JE_DateOfArrival = arrivalDate;
					dec.JE_MessageType = JobMessageTypeList.Codes.Import;
					dec.JE_OH_Importer = consignee.PK;
					// Insert record to database so that the next save will update property's concurrency policy to Strict
					factory.Save();

					var propertyInfo = property(dec);
					propertyInfo.Value = (ZString)"ORG";
					factory.Save();

					AssertEquals(propertyInfo.Name, ConcurrencyPolicy.Observe, propertyInfo.ConcurrencyPolicy);

					TestConnection.ExecuteNonQuery(FormattableString.Invariant($"UPDATE dbo.JobDeclaration SET {propertyInfo.Name} = 'DBC', JE_SystemLastEditTimeUtc = GetUtcDate(), JE_SystemLastEditUser = '~BP' WHERE JE_PK = '{dec.PK}'"));

					dec.JE_DateOfArrival = dec.JE_DateOfArrival.AddDays(10);
					if (shouldFail)
					{
						propertyInfo.Value = (ZString)"NEW";
						AssertExceptionThrown<ZSaveConcurrencyException>(propertyInfo.Name, () => factory.Save());
						ErrorReporter.Clear();
					}
					else
					{
						factory.Save();
					}
				}
			}
		}

		public void TestConcurrencyPolicyAfterUpdatingProperties()
		{
			var strictProperties = new Func<BaseJobDeclaration, ZPropertyInfo>[]
			{
				d => d.JE_EntryStatusInfo,
				d => d.JE_MessageStatusInfo,
				d => d.JE_ConsolidatedCargoStatusInfo,
				d => d.JE_MessageTypeInfo,
				d => d.JE_ApplicationCodeInfo
			};

			// Create a declaration with 'OLD' property value
			var declaration = GetJobDeclaration();
			foreach (var property in strictProperties)
			{
				property(declaration).Value = (ZString)"OLD";
			}
			Factory.Save();

			foreach (var property in strictProperties)
			{
				var reloadedFactory = NewFactory();
				var reloadedDec = reloadedFactory.Load<BaseJobDeclaration>(declaration.PK);
				var propertyInfo = property(reloadedDec);
				AssertEquals($"After loading from DB, ConcurrencyPolicy of {propertyInfo.Name} is Default", ConcurrencyPolicy.Default, propertyInfo.ConcurrencyPolicy);

				propertyInfo.Value = (ZString)"OLD";
				AssertEquals($"ConcurrencyPolicy of {propertyInfo.Name} is still Default when setter does not change value", ConcurrencyPolicy.Default, propertyInfo.ConcurrencyPolicy);

				propertyInfo.Value = (ZString)"NEW";
				AssertEquals($"ConcurrencyPolicy of {propertyInfo.Name} is Strict after change", ConcurrencyPolicy.Strict, propertyInfo.ConcurrencyPolicy);

				propertyInfo.Value = (ZString)"VL2";
				AssertEquals($"ConcurrencyPolicy of {propertyInfo.Name} is Strict after change", ConcurrencyPolicy.Strict, propertyInfo.ConcurrencyPolicy);
			}
		}
		#endregion

		public void TestConvertToLocalAmountWorksCorrectly()
		{
			AssertEquals(((ICurrencyConverterProvider)TestDec).CurrencyConverter.ConvertRounded(new Money(100m, TestDec.LocalCurrency), TestDec.LocalCurrency).Amount, TestDec.ConvertToLocalAmount(100m, TestDec.LocalCurrency).Amount);
		}

		#region IJobDocsAndCartageParent

		public void TestRequireOrderTrackLink_ForImporter()
		{
			string expectedError = "You cannot save without attaching any orders.";
			object x = TestDec.DocsAndCartage; // load me

			TestDec.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			TestDec.Consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			TestDec.RunPreSaveValidation();
			AssertHasError("No orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.Consignee.MiscServ.OM_IMJobRequireOrderTrackLink = false;
			TestDec.RunPreSaveValidation();
			AssertNoError("No orders attached and not requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.Consignee.MiscServ.OM_IMJobRequireOrderTrackLink = true;
			TestDec.MarkAsNeedingValidationIncludingChildren();
			TestDec.RunPreSaveValidation();
			AssertHasError("No orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.AttachedOrders.AddNew();
			TestDec.RunPreSaveValidation();
			AssertNoError("Orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);
		}

		public void TestRequireOrderTrackLink_ForSupplier()
		{
			string expectedError = "You cannot save without attaching any orders.";
			object x = TestDec.DocsAndCartage; // load me

			TestDec.JE_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			TestDec.Consignor.MiscServ.OM_EXJobRequireOrderTrackLink = true;
			TestDec.RunPreSaveValidation();
			AssertHasError("No orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.Consignor.MiscServ.OM_EXJobRequireOrderTrackLink = false;
			TestDec.RunPreSaveValidation();
			AssertNoError("No orders attached and not requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.Consignor.MiscServ.OM_EXJobRequireOrderTrackLink = true;
			TestDec.MarkAsNeedingValidationIncludingChildren();
			TestDec.RunPreSaveValidation();
			AssertHasError("No orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);

			TestDec.AttachedOrders.AddNew();
			TestDec.RunPreSaveValidation();
			AssertNoError("Orders attached and requires orders", TestDec.DocsAndCartage.JP_OrderItemsAsStringInfo, expectedError);
		}

		#endregion

		#region Related Business Objects

		public void TestJobHeader_OnDeclaration()
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = TestDec.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_GC = ZGuid.NewZGuid();

			AssertNull("Job NOT found", TestDec.Job);

			job.JH_GC = Env.CurrentCompany.PK;
			AssertEquals("Job found", job.PK, TestDec.Job.PK);
		}

		public void TestJobHeader_OnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			job.JH_GC = ZGuid.NewZGuid();

			AssertNull("Job NOT found", TestDec.Job);

			job.JH_GC = Env.CurrentCompany.PK;
			AssertEquals("Job found", job.PK, TestDec.Job.PK);
		}

		public void TestJobDocsAndCartage()
		{
			AssertNotNull("JobDocsAndCartage always not null", TestDec.DocsAndCartage);
			AssertEquals(
				"The authoritive parent of JobDocsAndCartage is the declaration when there is only a declaration",
				TestDec.PK, ((BusinessObject)TestDec.DocsAndCartage.Parent).PK);
		}

		public void TestJobDocsAndCartage_Deleting()
		{
			JobDocsAndCartage cartage = TestDec.DocsAndCartage;
			TestDec.Delete();
			AssertEquals("Cartage object got deleted", true, cartage.IsDeleted);
		}

		public void TestJobDocsAndCartage_SharedAcrossShipmentDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			AssertEquals("Should be sharing the same JobDocsAndCartage", TestDec.DocsAndCartage.PK, shipment.DocsAndCartage.PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment", shipment.PK, ((BusinessObject)shipment.DocsAndCartage.Parent).PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment", shipment.PK, ((BusinessObject)TestDec.DocsAndCartage.Parent).PK);
		}

		public void TestSettingJE_JSToEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();

			TestDec.JE_JS = shipment.PK;
			TestDec.JE_JS = ZGuid.Empty;
			TestDec.JE_JS = shipment.PK;
			AssertEquals("Declaration's DocsAndCartage should be that of its Shipment's.", shipment.DocsAndCartage.PK, TestDec.DocsAndCartage.PK);
		}

		public void TestChangingDeclarationShipmentLink()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();
			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment1.PK;
			Factory.Save();

			var savingFactory = new BusinessObjectFactory();
			var savingShipment2 = savingFactory.Load<ForwardingShipment>(shipment2.PK);
			var savingDeclaration = savingFactory.Load<BaseJobDeclaration>(declaration.PK);
			savingDeclaration.JE_JS = savingShipment2.PK;
			savingFactory.Save();

			AssertEquals("New declaration/shipment link should have consistent cartage objects.", savingDeclaration.DocsAndCartage.PK, savingShipment2.DocsAndCartage.PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment.", savingShipment2.PK, savingDeclaration.DocsAndCartage.Parent.PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment.", savingShipment2.PK, savingShipment2.DocsAndCartage.Parent.PK);
		}

		public void TestSetDocsAndCartageFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			AssertEquals("DocsAndCartage should be populated.", TestDec.DocsAndCartage.PK, shipment.DocsAndCartage.PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment.", shipment.PK, ((BusinessObject)TestDec.DocsAndCartage.Parent).PK);
			AssertEquals("The authoritive parent of JobDocsAndCartage is the shipment.", shipment.PK, ((BusinessObject)shipment.DocsAndCartage.Parent).PK);
		}

		#endregion

		#region NoteTypes

		public void TestNoteTypes()
		{
			Assert("PredefinedNoteTypes.Instance.CertificateOfOriginNote should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.CertificateOfOriginNote, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.ClientVisibleJobNotes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.ClientVisibleJobNotes, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DeliveryInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.DeliveryInstructionsNote, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.PickupInstructionsNote should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.PickupInstructionsNote, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.DetailedGoodsDescription should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.DetailedGoodsDescription, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.HandlingInstructions should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.HandlingInstructions, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.UnmatchedOrgDetails should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.UnmatchedOrgDetails, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.SpecialInstructions should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.SpecialInstructions, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.CartageHistoryNotes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.CartageHistoryNotes, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.InternalWorkNotes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.InternalWorkNotes, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.AutoRatingAuditLog should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.AutoRatingAuditLog, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.FaxEmailTransmissionLog should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.CustomsQuarantineMessagingRemarks, TestDec.NoteTypes));
			Assert("PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes should be in BO.NoteTypes", PredefinedNoteTypeExist(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes, TestDec.NoteTypes));
		}

		bool PredefinedNoteTypeExist(PredefinedNoteType noteTypeToCheck, NoteTypeCollection noteTypes)
		{
			bool result = false;

			foreach (PredefinedNoteType noteType in noteTypes)
			{
				if (noteTypeToCheck == noteType)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		public virtual void TestDeveloperErrorOnAddNewNotesToDeclarationAttachedToShipment()
		{
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			TestDec.Notes.AddNew();
			AssertEquals("Should be no developer errors adding notes to a declaration", 0, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);

			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			try
			{
				TestDec.Notes.AddNew();
				AssertEquals("Should be a developer error adding notes to a declaration with a shipment attached", 1, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestAttachedOrdersWithDeclaration()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			Assert("Declaration should be attached to the shipment for test", shipment.Declarations.Length > 0);

			Order orderOnShipment = shipment.AttachedOrders.AddNew();
			AssertEquals("Should have orders attached from shipment", 1, TestDec.AttachedOrders.Count);

			TestDec.JE_JS = ZGuid.Empty;
			Order orderOnDeclaration1 = ((IAttachOrders)TestDec).AttachedOrders.AddNew();
			Order orderOnDeclaration2 = ((IAttachOrders)TestDec).AttachedOrders.AddNew();
			Assert("Declaration should no longer be attached to the shipment for test", shipment.Declarations.Length == 0);
			AssertEquals("Should have orders from declaration now", 3, TestDec.AttachedOrders.Count);
		}

		public void TestNotesAndEventsVisible()
		{
			var shipment = Factory.New<ForwardingShipment>();

			AssertEquals("NotesAndEventsVisible true when no shipment", true, TestDec.NotesAndEventsVisible);
			TestDec.JE_JS = shipment.PK;
			AssertEquals("NotesAndEventsVisible false when no shipment", false, TestDec.NotesAndEventsVisible);
		}

		public void TestPopulateServiceLevelFromShipmentOnFactorySaving()
		{
			var shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			shipment.JS_RS_NKServiceLevel = "xxx";
			Factory.Save();
			BaseJobDeclaration retrievedDeclaration = new BusinessObjectFactory().Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Service level should propagate to the declaration", "xxx", retrievedDeclaration.JE_RS_NKServiceLevel);

			shipment.JS_RS_NKServiceLevel = "YYY";

			Assert("PreCondition:OnSaving won't be called", !TestDec.HasChanges);
			Factory.Save();
			retrievedDeclaration = new BusinessObjectFactory().Load<BaseJobDeclaration>(TestDec.PK);
			AssertEquals("Service level should propagate to the declaration", "YYY", retrievedDeclaration.JE_RS_NKServiceLevel);
		}

		public void TestLandedCostingPercentageDefaults_Air()
		{
			TestBaseJobDeclaration.DefaultDepartmentTransportMode = Enterprise.Core.Constants.TransportModes.Air;
			TestBaseJobDeclaration declaration = Factory.New<TestBaseJobDeclaration>();
			AssertEquals((ZByte)100, declaration.JE_LandedCostByWeight);
			AssertEquals((ZByte)0, declaration.JE_LandedCostByVolume);
			AssertEquals((ZByte)0, declaration.JE_LandedCostByUnits);
			AssertEquals((ZByte)0, declaration.JE_LandedCostByCost);
		}

		public void TestLandedCostingPercentageDefaults_Sea()
		{
			TestBaseJobDeclaration.DefaultDepartmentTransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			TestBaseJobDeclaration declaration = Factory.New<TestBaseJobDeclaration>();
			AssertEquals((ZByte)0, declaration.JE_LandedCostByWeight);
			AssertEquals((ZByte)100, declaration.JE_LandedCostByVolume);
			AssertEquals((ZByte)0, declaration.JE_LandedCostByUnits);
			AssertEquals((ZByte)0, declaration.JE_LandedCostByCost);
		}

		public void TestGetOperationsSignificantDateAndActualChargeable_One()
		{
			var declaration = GetJobDeclaration();
			var pluginData = declaration as IJobInvoicingPlugIn;

			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.Empty,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			//--------------------------------------------------------------------------------------------------
			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-7), false);
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-6), false);
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-6),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-7), false);
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-6), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-7),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-7), true);
			declaration.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-6), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-6),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
		}

		public void TestGetOperationsSignificantDateAndActualChargeable_Two()
		{
			var declaration = GetJobDeclaration();
			var pluginData = declaration as IJobInvoicingPlugIn;

			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.Empty,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			//--------------------------------------------------------------------------------------------------
			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-5), false);
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-4), false);
			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-4),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-5), false);
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-4), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-5),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			declaration.Logs.RemoveAndDeleteAll();
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-5), true);
			declaration.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-4), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-4),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
		}

		public void TestGetOperationsRevenueRecognitionDateAndActualChargeable_Three()
		{
			var declaration = GetJobDeclaration();
			var pluginData = declaration as IJobInvoicingPlugIn;

			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.Empty,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			//--------------------------------------------------------------------------------------------------
			var shipment = Factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();

			shipment.Logs.RemoveAndDeleteAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = DefaultImportMessageType;
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-2),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			AssertEquals(shipment, declaration.Shipment);
			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-3),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-2),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
		}

		public void TestGetOperationsRevenueRecognitionDateAndActualChargeable_Four()
		{
			var declaration = GetJobDeclaration();
			var pluginData = declaration as IJobInvoicingPlugIn;

			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.Empty,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(
							RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			//--------------------------------------------------------------------------------------------------
			var shipment = Factory.New<ForwardingShipment>();
			shipment.FillWithValidTestData();

			shipment.Logs.RemoveAndDeleteAll();
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), false);

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = DefaultExportMessageType;
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			shipment.Logs.RemoveAndDeleteAll();
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday.AddMonths(-1),
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			shipment.Logs.RemoveAndDeleteAll();
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			AssertEquals("OperationsRevenueRecognitionDate on IJobInvoicingPlugin", ZDateTime.BrettsBirthday,
						 pluginData.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));

			//--------------------------------------------------------------------------------------------------

			declaration.JE_TotalWeight = 69m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Ounces;
			AssertNotNull(pluginData.InvoicingSupporter);
			AssertEquals(69m, pluginData.InvoicingSupporter.ActualChargeable);
			AssertEquals(Core.Constants.Weight.Ounces, pluginData.InvoicingSupporter.ActualChargeableUnit);
		}

		#region TestHouseBillProxiedThroughHouseBillsFirstElement
		public virtual void TestHouseBillProxiedThroughHouseBillsFirstElement()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			var declaration = mockDeclaration.Object;

			AssertEquals("Precondition of JE_HouseBill", "", declaration.JE_HouseBill);
			AssertEquals("Precondition of HouseBills.Count", 0, declaration.Bills.Count);

			declaration.JE_HouseBill = "";
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);
			AssertEquals("HouseBills.Count", 0, declaration.Bills.Count);

			declaration.JE_HouseBill = "HOUSEBILL1";
			AssertEquals("JE_HouseBill", "HOUSEBILL1", declaration.JE_HouseBill);
			AssertEquals("HouseBills.Count", 1, declaration.Bills.Count);
			AssertEquals("Declaration.HouseBills[0].CU_HouseBill", "HOUSEBILL1", declaration.Bills[0].CU_HouseBill);

			declaration.JE_HouseBill = "HOUSEBILL2";
			AssertEquals("JE_HouseBill", "HOUSEBILL2", declaration.JE_HouseBill);
			AssertEquals("HouseBills.Count", 1, declaration.Bills.Count);
			AssertEquals("Declaration.HouseBills[0].CU_HouseBill", "HOUSEBILL2", declaration.Bills[0].CU_HouseBill);

			declaration.JE_HouseBill = "";
			AssertHouseBillProxiedThroughHouseBillsFirstElement(declaration);
		}

		protected virtual void AssertHouseBillProxiedThroughHouseBillsFirstElement(BaseJobDeclaration declaration)
		{
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);
			AssertEquals("HouseBills.Count", 1, declaration.Bills.Count);
			AssertEquals("Declaration.HouseBills[0].CU_HouseBill", "", declaration.Bills[0].CU_HouseBill);
		}

		#endregion

		#region ICancellable

		public virtual void TestCanCancel_JobInCurrentCompany()
		{
			var shipment = (CommonShipment)Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			Factory.Save();

			Assert("CanCancel", string.IsNullOrEmpty(TestDec.CanCancel()));

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = "JE";
			job.JH_ParentID = TestDec.PK;
			Factory.Save();
			Assert("CanCancel", string.IsNullOrEmpty(TestDec.CanCancel()));

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			Assert(charge.JR_OSCostAmt.IsEmpty);
			Assert(charge.JR_OSSellAmt.IsEmpty);
			Factory.Save();
			Assert("CanCancel", string.IsNullOrEmpty(TestDec.CanCancel()));

			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();

			var expectedMessage = $@"{TestDec.HumanReadableName.ToString()} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.";

			AssertEquals("CanCancel", expectedMessage, TestDec.CanCancel());

			charge.JR_OSCostAmt = 0m;
			Factory.Save();
			AssertEquals("CanCancel - Existing Accrual", expectedMessage, TestDec.CanCancel());

			expectedMessage = $@"{TestDec.HumanReadableName.ToString()} cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header ({job.JH_JobNum}) in the company EDI.";

			charge.Delete();
			Factory.Save();
			AssertEquals("CanCancel - Existing Accrual", expectedMessage, TestDec.CanCancel());
		}

		public virtual void TestCanCancel_JobInForeignCompany()
		{
			var shipment = (CommonShipment)Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			var foreignCompany = Factory.New<GlbCompany>();
			foreignCompany.GC_Code = "BLA";
			var foreignBranch = foreignCompany.Branches.AddNew();
			foreignBranch.GB_Code = "BC1";
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), foreignBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Assert("CanCancel", string.IsNullOrEmpty(TestDec.CanCancel()));
				var foreignJob = new JobHeader.Loader(TestDec).TryCreateWithMutex();
				foreignJob.JH_ParentTableCode = "JE";
				foreignJob.JH_ParentID = TestDec.PK;
				foreignJob.JH_GC = foreignCompany.PK;
				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = foreignJob.PK;
				Assert(charge.JR_OSCostAmt.IsEmpty);
				Assert(charge.JR_OSSellAmt.IsEmpty);
				Factory.Save();
				Assert("CanCancel", string.IsNullOrEmpty(TestDec.CanCancel()));

				charge.JR_OSCostAmt = 5m;
				charge.JR_OSCostExRate = 1m;
				Factory.Save();

				var expectedMessage = $@"{TestDec.HumanReadableName.ToString()} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header ({foreignJob.JH_JobNum}) in the company {foreignCompany.GC_Code}.";

				AssertEquals("CanCancel", expectedMessage, TestDec.CanCancel());

				charge.JR_OSCostAmt = 0m;
				Factory.Save();
				AssertEquals("CanCancel - Existing Accrual", expectedMessage, TestDec.CanCancel());

				expectedMessage = $@"{TestDec.HumanReadableName.ToString()} cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header ({foreignJob.JH_JobNum}) in the company {foreignCompany.GC_Code}.";

				charge.Delete();
				Factory.Save();
				AssertEquals("CanCancel - existing line", expectedMessage, TestDec.CanCancel());
			}
		}

		public virtual void TestCanCancel_ShipmentJob()
		{
			var shipment = (CommonShipment)Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;
			Factory.Save();

			var shipmentJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			shipmentJob.JH_ParentTableCode = "JS";
			shipmentJob.JH_ParentID = shipment.PK;
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = shipmentJob.PK;
			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();
			var expectedMsg = $@"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.
{shipment.HumanReadableName}: {shipment.HumanReadableName} cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.";
			AssertEquals("CanCancel", expectedMsg, TestDec.CanCancel());

			charge.JR_OSCostAmt = 0m;
			Factory.Save();
			AssertEquals("CanCancel - Existing Accrual", expectedMsg, TestDec.CanCancel());

			charge.Delete();
			Factory.Save();
			expectedMsg = $@"This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.
{shipment.HumanReadableName}: {shipment.HumanReadableName} cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.";
			AssertEquals("CanCancel - Existing Accrual", expectedMsg, TestDec.CanCancel());
		}

		public void TestCanReactivate_Default()
		{
			var declaration = GetJobDeclaration();
			AssertNull(declaration.CanReactivate());
		}

		public void TestCanReactivate_HasJDSEvent()
		{
			var declaration = GetJobDeclaration();
			declaration.Logs.AddNew(Events.JobDeactivatedBySystem, "Test");
			AssertEquals("Cannot reactivate if has JDS event logged", "This Job has been deactivated by WiseTech Global and cannot be reactivated.", declaration.CanReactivate());
		}

		public void TotalInvoiceAmount()
		{
			BaseJobComInvoiceHeader invoice1 = TestDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			BaseJobComInvoiceHeader invoice2 = TestDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 200m;
			invoice2.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			AssertEquals(new Money(300m, TestDec.LocalCurrency), TestDec.TotalInvoiceAmount);
		}

		public void TestTotalInvoiceAmountInLocalCurrency()
		{
			BaseJobComInvoiceHeader invoice1 = TestDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			BaseJobComInvoiceHeader invoice2 = TestDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 200m;
			invoice2.JZ_RX_NKInvoice_Currency = TestDec.LocalCurrencyCode;
			AssertEquals(300m, TestDec.TotalInvoiceAmountInLocalCurrency);
		}

		public virtual void TestDateForDutyRate()
		{
			AssertEquals(ZDate.Today, TestDec.DateForDutyRate);
		}

		public void TestReadOnlySetWhenCancelled()
		{
			TestDec.JE_IsCancelled = true;
			foreach (PropertyInfo property in TestDec.GetType().GetProperties())
			{
				if (typeof(ZPropertyInfo).IsAssignableFrom(property.PropertyType))
				{
					AssertEquals("All properties should be read-only, including " + property.Name, true, ((ZPropertyInfo)property.GetValue(TestDec, Array.Empty<object>())).ReadOnly);
				}
			}

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var dec2 = factory2.Load<BaseJobDeclaration>(TestDec.PK);
			dec2.JE_IsCancelled = false;
			AssertEquals(false, dec2.JE_DateAtOriginInfo.ReadOnly);
		}

		#endregion

		public void TestEarliestCustomsEntryIssueDate()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();

			CusEntryHeader header1 = declaration.CustomsEntryHeaders.AddNew();
			header1.EntryNumber = "ABC";
			header1.CusEntryNumber.CE_IssueDate = new ZDateTime(2008, 1, 1);
			CusEntryHeader header2 = declaration.CustomsEntryHeaders.AddNew();
			header2.EntryNumber = "DEF";
			header2.CusEntryNumber.CE_IssueDate = new ZDateTime(2007, 2, 3);
			AssertEquals(new ZDateTime(2007, 2, 3), declaration.EarliestCustomsEntryIssueDate);
		}

		public void TestDecHouseContainerMultiPackages()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			AssertEquals("No elements in collection", 0, declaration.PackingGroups.Count);

			declaration = GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "House Bill";

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONTAINERNUM";

			BasePackingGroup pivot = houseBill.PackingGroups.AddNew();
			pivot.CR_CO_Container = container.PK;
			AssertEquals("One element in the collection", 1, declaration.PackingGroups.Count);
		}

		public virtual void TestRelatedDeclarations()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			if (!declaration.UseGenPivotForRelatedDeclarations)
			{
				SetDeclarationForMatch(declaration);
				BaseJobDeclaration matchDeclaration = GetNewMatchingDec(declaration);
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));

				declaration = Factory.New<BaseJobDeclaration>();
				SetDeclarationForMatch(declaration);
				matchDeclaration = GetNewMatchingDec(declaration);

				matchDeclaration.JE_HouseBill = ZString.Empty;
				matchDeclaration.JE_MasterBill = ZString.Empty;
				Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration));

				declaration = Factory.New<BaseJobDeclaration>();
				SetDeclarationForMatch(declaration);
				matchDeclaration = GetNewMatchingDec(declaration);
				BaseJobDeclaration matchDeclaration1 = GetNewMatchingDec(declaration);
				matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(21);
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
				Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));

				declaration = Factory.New<BaseJobDeclaration>();
				SetDeclarationForMatch(declaration);
				matchDeclaration = GetNewMatchingDec(declaration);
				matchDeclaration1 = GetNewMatchingDec(declaration);
				matchDeclaration1.JE_HouseBill = "HB002";
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
				Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));

				declaration = Factory.New<BaseJobDeclaration>();
				SetDeclarationForMatch(declaration);
				declaration.JE_HouseBill = ZString.Empty;
				matchDeclaration = GetNewMatchingDec(declaration);
				matchDeclaration1 = GetNewMatchingDec(declaration);
				matchDeclaration1.JE_MasterBill = "MB002";
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
				Assert(!declaration.RelatedDeclarations.Contains(matchDeclaration1));

				declaration = Factory.New<BaseJobDeclaration>();
				SetDeclarationForMatch(declaration);
				matchDeclaration = GetNewMatchingDec(declaration);
				matchDeclaration1 = GetNewMatchingDec(declaration);
				matchDeclaration1.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-11);
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration));
				Assert(declaration.RelatedDeclarations.Contains(matchDeclaration1));
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ExpectedSupportInvoiceLineRefs => false;
		protected virtual bool ExpectedSupportsJobComInvoiceLineTax => false;

		void SetDeclarationForMatch(BaseJobDeclaration declaration)
		{
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_HouseBill = "HB001";
			declaration.JE_MasterBill = "MB001";
		}

		BaseJobDeclaration GetNewMatchingDec(BaseJobDeclaration dec)
		{
			BaseJobDeclaration newDeclaration = Factory.New<BaseJobDeclaration>();
			newDeclaration.JE_SystemCreateTimeUtc = dec.JE_SystemCreateTimeUtc.AddDays(4);
			newDeclaration.JE_HouseBill = dec.JE_HouseBill;
			newDeclaration.JE_MasterBill = dec.JE_MasterBill;
			return newDeclaration;
		}

		public void TestNoMergeWhenCustomsWareEnabledCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					var dec = Factory.New<BaseJobDeclaration>();
					dec.JE_GB = GlbBranch.CurrentBranch.PK;//ISendsMessagesToCustoms
					dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

					mergedSuccessfullyCallCount = 0;
					dec.MergedSuccessfully += new MergedSuccessfullyHandler(Declaration_MergedSuccessfully);
					AssertEquals(true, dec.DoMerge());
					AssertEquals("MergeCount", 1, mergedSuccessfullyCallCount);
					dec.DoMerge();
					AssertEquals("MergeCount", 2, mergedSuccessfullyCallCount);
				}
			}
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 0, declaration.BusinessObjectsWithRelatedNotes.Length);
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 1, declaration.BusinessObjectsWithRelatedNotes.Length);
			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 2, declaration.BusinessObjectsWithRelatedNotes.Length);
			Order order1 = declaration.AttachedOrders.AddNew();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 3, declaration.BusinessObjectsWithRelatedNotes.Length);
			Order order2 = declaration.AttachedOrders.AddNew();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 4, declaration.BusinessObjectsWithRelatedNotes.Length);
			declaration.CusContainers.AddNew();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 5, declaration.BusinessObjectsWithRelatedNotes.Length);
			declaration.Invoices.AddNew();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 6, declaration.BusinessObjectsWithRelatedNotes.Length);
			declaration.MakeNonPersistent();
			AssertEquals("declaration.BusinessObjectsWithRelatedNotes.Count", 0, declaration.BusinessObjectsWithRelatedNotes.Length);
		}

		#region ICDArchive

		public void TestCDJobNumber()
		{
			TestDec.JE_DeclarationReference = "ABC";

			AssertEquals("Job Number is declaration ref", "ABC", TestDec.CDArchiveInfo.JobNumber);
		}

		public void TestCDHouseBill()
		{
			TestDec.JE_HouseBill = "11111";
			AssertEquals("Housebill", "11111", TestDec.CDArchiveInfo.HouseBill);
		}

		public void TestCDMasterBill()
		{
			TestDec.JE_MasterBill = "22222";
			AssertEquals("Masterbill", "22222", TestDec.CDArchiveInfo.MasterBill);
		}

		public void TestCDOrderNumbers()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			TestDec.JE_JS = shipment.PK;

			TestDec.JE_OwnerRef = "own";
			shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "ord";
			AssertEquals("OrderNumbers", "ord, own", TestDec.CDArchiveInfo.OrderNumbers);

			Order order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "123";

			Factory.Save();

			shipment.AttachedOrders.Add(order1);
			AssertEquals("OrderNumbers", "123, ord, own", TestDec.CDArchiveInfo.OrderNumbers);

			Order order2 = shipment.AttachedOrders.AddNew();
			order2.JD_OrderNumber = "456";
			AssertEquals("OrderNumbers", "123, 456, ord, own", TestDec.CDArchiveInfo.OrderNumbers);
		}

		public void TestCDInvoiceNumbers()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			TestDec.JE_DeclarationReference = "B00001000";
			TestDec.JE_OH_Importer = importer.PK;

			AccTransactionHeader header1 = Factory.New<AccTransactionHeader>();
			header1.AH_TransactionNum = "BBBBB";
			header1.AH_OH = importer.PK;
			header1.AH_GB = GlbBranch.CurrentBranch.PK;
			header1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header1.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			header1.AH_ConsolidatedInvoiceRef = "AAAAA";

			AccTransactionHeader header2 = Factory.New<AccTransactionHeader>();
			header2.AH_OH = importer.PK;
			header2.AH_GB = GlbBranch.CurrentBranch.PK;
			header2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			header2.AH_ConsolidatedInvoiceRef = "22222";

			AccTransactionHeader header3 = Factory.New<AccTransactionHeader>();
			header3.AH_OH = importer.PK;
			header3.AH_GB = GlbBranch.CurrentBranch.PK;
			header3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			header3.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			header3.AH_ConsolidatedInvoiceRef = "33333";

			AssertEquals("No matching invoices", ZString.Empty, TestDec.CDArchiveInfo.InvoiceNumbers);

			header1.AH_ConsolidatedInvoiceRef = "B00001000";
			AssertEquals("Matching invoice found", "B00001000", TestDec.CDArchiveInfo.InvoiceNumbers);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.UseJobNumberBasedInvoiceNumbers).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Matching invoice found", "BBBBB", TestDec.CDArchiveInfo.InvoiceNumbers);
			}
		}

		public void TestCDContainerNumbers()
		{
			BaseCusContainer container = TestDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "12345";

			AssertEquals("Containers", "12345", TestDec.CDArchiveInfo.ContainerNumbers);

			BaseCusContainer container2 = TestDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "67890";
			AssertEquals("Containers", "12345, 67890", TestDec.CDArchiveInfo.ContainerNumbers);
		}

		public void TestCDETA()
		{
			TestDec.JE_DateOfArrival = new ZDateTime(2004, 11, 30);

			AssertEquals("ETA", new ZDateTime(2004, 11, 30), TestDec.CDArchiveInfo.ETA);
		}

		public void TestCDETD()
		{
			TestDec.JE_ExportDate = new ZDateTime(2004, 11, 30);

			AssertEquals("ETD", new ZDateTime(2004, 11, 30), TestDec.CDArchiveInfo.ETD);
		}

		public void TestCDVessel()
		{
			AssertEquals("vessel blank if no shipment", ZString.Empty, TestDec.CDArchiveInfo.Vessel);

			TestDec.JE_VesselName = "ABC Vessel";
			AssertEquals("Vessel", "ABC Vessel", TestDec.CDArchiveInfo.Vessel);
		}

		public void TestCDVoyageFlight()
		{
			AssertEquals("voyage blank default", ZString.Empty, TestDec.CDArchiveInfo.VoyageFlight);

			TestDec.JE_VoyageFlightNo = "12345";
			AssertEquals("Voyage number", "12345", TestDec.CDArchiveInfo.VoyageFlight);
		}

		public void TestCDEntryNumber()
		{
			AssertEquals("EntryNumber should be blank by default.", ZString.Empty, TestDec.CDArchiveInfo.EntryNumber);

			var mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Setup(m => m.DeclarationNumber).Returns("12345");
			AssertEquals("EntryNumber", "12345", mockDeclaration.Object.CDArchiveInfo.EntryNumber);

			mockDeclaration.Object.CustomsEntryHeaders.AddNew().EntryNumber = "abcdef";
			mockDeclaration.Object.CustomsEntryHeaders.AddNew().EntryNumber = "qwerty";

			AssertEquals("EntryNumber", "12345, abcdef, qwerty", mockDeclaration.Object.CDArchiveInfo.EntryNumber);
		}

		public void TestCDOrigin()
		{
			TestDec.JE_RL_NKOrigin = "AUSYD";
			AssertEquals("Declaration origin", "AUSYD", TestDec.CDArchiveInfo.Origin);
			TestDec.JE_RL_NKOrigin = "AUPER";
			AssertEquals("Declaration origin", "AUPER", TestDec.CDArchiveInfo.Origin);
		}

		public void TestCDDestination()
		{
			TestDec.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Declaration origin", "AUSYD", TestDec.CDArchiveInfo.Destination);
			TestDec.JE_RL_NKFinalDestination = "AUPER";
			AssertEquals("Declaration origin", "AUPER", TestDec.CDArchiveInfo.Destination);
		}

		public void TestCDConsigneeCode()
		{
			var importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			BaseJobDeclaration declaration = GetJobDeclaration();

			AssertEquals("Importer empty string default", ZString.Empty, declaration.CDArchiveInfo.ConsigneeCode);

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("Importer code", importer.OH_Code, declaration.CDArchiveInfo.ConsigneeCode);
		}

		public void TestCDConsignorCode()
		{
			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			BaseJobDeclaration declaration = GetJobDeclaration();

			AssertEquals("Consignor empty string default", ZString.Empty, declaration.CDArchiveInfo.ConsignorCode);

			declaration.JE_OH_Supplier = consignor.PK;
			AssertEquals("Consignor code", consignor.OH_Code, declaration.CDArchiveInfo.ConsignorCode);
		}

		#endregion

		public void TestLoadFirstMatchingInCurrentCompanyIncludingInActive()
		{
			BaseJobDeclaration decInThisCountryInRightBranch = BaseJobDeclaration.New(Factory);
			decInThisCountryInRightBranch.JE_GB = GlbBranch.CurrentBranch.PK;
			decInThisCountryInRightBranch.JE_DeclarationReference = "12345";

			BaseJobDeclaration decInOtherCountry = BaseJobDeclaration.New(Factory);
			decInOtherCountry.JE_GB = BranchInOtherCountry.PK;
			decInThisCountryInRightBranch.JE_DeclarationReference = "12345";

			BaseJobDeclaration decInCurrentCountryOnDifferentCompany = BaseJobDeclaration.New(Factory);
			decInCurrentCountryOnDifferentCompany.JE_GB = BranchInCurrentCountryOnDifferentCompany.PK;
			decInThisCountryInRightBranch.JE_DeclarationReference = "12345";

			BaseJobDeclaration loadedDec = BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, "12345");

			AssertEquals("Should get DecInThisCountryInRightBranch", decInThisCountryInRightBranch.PK, loadedDec.PK);

			decInThisCountryInRightBranch.JE_DeclarationReference = "54321";

			BaseJobDeclaration noDecToLoad = BaseJobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, "12345");

			AssertNull("Should get No Declaration when Decs with this Ref are in other companies.", noDecToLoad);
		}

		public void TestLocalParty()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			TestDec.JE_MessageType = DefaultExportMessageType;
			TestDec.JE_OH_Supplier = supplier.PK;
			AssertEquals(TestDec.Supplier.PK, TestDec.LocalParty.PK);

			OrgHeader importer = OrgHeader.New(Factory);
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_MessageType = DefaultImportMessageType;
			AssertEquals(TestDec.Importer.PK, TestDec.LocalParty.PK);
		}

		#region TestPopulateCommercialInvoice

		public void TestPopulateCommercialInvoice()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();
			var line3 = shipment.OuterPackLines.AddNew();

			line1.JL_LinePrice = 10m;
			line2.JL_LinePrice = 20m;
			line3.JL_LinePrice = 30m;

			line2.JL_HarmonisedCode = "2022.22";
			line3.JL_HarmonisedCode = "3033.33";

			TestDec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			CreatePartAndClassification("AB0035X", "2605.00", TestDec.Importer);

			line1.Products.PackProductManager.Value = "AB0035X";
			line3.Products.PackProductManager.Value = "H15-394885J9";

			((Freight.Integration.Forwarding.ICommercialInvoice)TestDec).PopulateCommercialInvoice(shipment.OuterPackLines);

			CombineAssertions(() =>
			{
				AssertEquals("Has new header", 1, TestDec.Invoices.Count);
				AssertEquals("Should have 3 lines", 3, TestDec.Invoices[0].JobComInvoiceLines.Count);
				AssertEquals(10m, TestDec.Invoices[0].JobComInvoiceLines[0].JI_LinePrice);
				AssertEquals(20m, TestDec.Invoices[0].JobComInvoiceLines[1].JI_LinePrice);
				AssertEquals(30m, TestDec.Invoices[0].JobComInvoiceLines[2].JI_LinePrice);
				AssertEquals("260500", TestDec.Invoices[0].JobComInvoiceLines[0].JI_Tariff.Replace(".", ""));
				AssertEquals("202222", TestDec.Invoices[0].JobComInvoiceLines[1].JI_Tariff.Replace(".", ""));
				AssertEquals("303333", TestDec.Invoices[0].JobComInvoiceLines[2].JI_Tariff.Replace(".", ""));
				AssertEquals("AB0035X", TestDec.Invoices[0].JobComInvoiceLines[0].JI_PartNo);
				AssertEquals("", TestDec.Invoices[0].JobComInvoiceLines[1].JI_PartNo);
				AssertEquals("H15-394885J9", TestDec.Invoices[0].JobComInvoiceLines[2].JI_PartNo);
			});
		}

		protected virtual void CreatePartAndClassification(string partNum, string tariff, OrgHeader importer)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNum;
			part.RelatedOrganisations.AddOwner(importer);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_TariffNum = tariff;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_OP = part.PK;
			pivot.CI_CC = classification.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
		}

		#endregion

		public virtual void TestPackingGroupCollectionRegistration()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(false);

			var declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;
			var pack = declaration.PackingGroups.AddNew();
			AssertEquals(false, declaration.HasMessageErrors);
			pack.CR_CO_Container = ZGuid.Invalid;
			AssertEquals(false, declaration.HasErrors);

			mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(true);
			declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;
			pack = declaration.PackingGroups.AddNew();
			pack.CR_CO_Container = ZGuid.Invalid;
			AssertEquals(true, declaration.HasErrors);

			mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(false);
			declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;
			pack = declaration.PackingGroups.AddNew();
			pack.CR_CO_Container = ZGuid.Invalid;
			AssertEquals(false, declaration.HasErrors);
		}

		public void TestMultipleRegisteringOrUnregisteringPackingGroupChildrenDoesNotCauseException()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(false);
			var declaration = mockDeclaration.Object;
			var pack = declaration.PackingGroups.AddNew();

			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(true);
			AssertEquals("The collection should have been registered", 1, declaration.PackingGroups.Count); //Causes the collection to be registered
			AssertEquals("The collection should have been registered for the second time without any problem", 1, declaration.PackingGroups.Count); //Do it again to make sure there is not exception

			mockDeclaration.Protected().Setup<bool>("IsPackingGroupCollectionRegisteredEditable").Returns(false);
			AssertEquals("The collection should have been unregistered", 1, declaration.PackingGroups.Count); //Causes the collection to be unregistered
			AssertEquals("The collection should have been unregistered for the second time without any problem", 1, declaration.PackingGroups.Count); //Do it again to make sure there is not exception
		}

		#region TestSupportedAddressTypes + TestGetDocAddress

		public void TestSupportedAddressTypes()
		{
			var addressTypes = ((IDocAddresses)TestDec).SupportedAddressTypes;

			AssertEquals("New address types might have been added, ensure they are tested.", ExpectedDocAddressTypes.Count, addressTypes.Count);
			foreach (DocAddressType addressType in ExpectedDocAddressTypes.Values)
			{
				AssertCollectionContains(addressType, addressTypes);
			}
		}

		protected virtual Hashtable ExpectedDocAddressTypes
		{
			get
			{
				if (fExpectedDocAddressTypes == null)
				{
					fExpectedDocAddressTypes = new Hashtable();
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierDocumentaryAddress, DocAddressType.SupplierDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SupplierPickupDeliveryAddress, DocAddressType.SupplierPickupDeliveryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.ImporterDocumentaryAddress, DocAddressType.ImporterDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.ImporterPickupDeliveryAddress, DocAddressType.ImporterPickupDeliveryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.NotifyParty, DocAddressType.NotifyParty);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.NotifyParty2, DocAddressType.NotifyParty2);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.NotifyParty3, DocAddressType.NotifyParty3);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.BuyerDocumentaryAddress, DocAddressType.BuyerDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.InsuredByDocumentaryAddress, DocAddressType.InsuredByDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.AssuredPartyDocumentaryAddress, DocAddressType.AssuredPartyDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.ClaimsPayableByDocumentaryAddress, DocAddressType.ClaimsPayableByDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.SurveyReportPartyDocumentaryAddress, DocAddressType.SurveyReportPartyDocumentaryAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.CustomsContainerYardAddress, DocAddressType.CustomsContainerYardAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.CustomsContainerTerminalOperatorAddress, DocAddressType.CustomsContainerTerminalOperatorAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.CustomsDepotAddress, DocAddressType.CustomsDepotAddress);
					fExpectedDocAddressTypes.Add(DocAddressTypes.Codes.CustomsWarehouseAddress, DocAddressType.CustomsWarehouseAddress);
				}
				return fExpectedDocAddressTypes;
			}
		}
		Hashtable fExpectedDocAddressTypes;

		#endregion

		public void TestIRelatedJobNumberMembers()
		{
			TestDec.JE_DeclarationReference = "B00000001";
			string[] jobNumbers = ((IRelatedJobNumber)TestDec).JobNumber;
			AssertEquals("Number of elements in JobNumber", 1, jobNumbers.Length);
			AssertEquals("JobNumber must be equal Declaration.JobNumber", TestDec.JobNumber, jobNumbers[0]);
		}

		public virtual void TestNewRelatedDeclaration()
		{
			TestDec.JE_HouseBill = "HB0001";
			TestDec.JE_MasterBill = "MB0002";
			TestDec.JE_DateOfArrival = ZDateTime.Today;
			TestDec.JE_ExportDate = ZDateTime.Today.AddDays(4);
			BaseJobDeclaration relatedDeclaration = TestDec.GetNewRelatedDeclaration(Factory);
			AssertEquals("HB0001", relatedDeclaration.JE_HouseBill);
			AssertEquals("MB0002", relatedDeclaration.JE_MasterBill);
			AssertEquals(ZDateTime.Today, relatedDeclaration.JE_DateOfArrival);
			AssertEquals(ZDateTime.Today.AddDays(4), relatedDeclaration.JE_ExportDate);
			AssertEquals(ZString.Empty, relatedDeclaration.JE_GS_NKCusAgent);
		}

		public virtual void TestNewRelatedDeclarationReference()
		{
			TestDec.JE_DeclarationReference = "TestRef";
			BaseJobDeclaration relatedDeclaration = TestDec.GetNewRelatedDeclaration(Factory);
			AssertEquals("TestRef", relatedDeclaration.ClonedFromDeclarationReference);
		}

		[TestDate(2021, 05, 08)]
		public void TestAuditDetails()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Test User";

			TestDec.JE_AuditDateUtc = ZDateTime.UtcNow;
			TestDec.JE_AuditReference = "Declaration Audit Completed.";
			TestDec.JE_GS_NKAuditUser = "AAA";

			AssertEquals(ZDateTime.UtcNow.ToLocalBranchTime(), TestDec.AuditDate);
			AssertEquals(ZDateTime.UtcNow, TestDec.AuditDateUtc);
			AssertEquals("Declaration Audit Completed.", TestDec.AuditReference);
			AssertEquals("AAA", TestDec.AuditLogUser);
			AssertEquals("Test User", TestDec.AuditLogUserName);
		}

		#region ICustomFieldProvider

		public void TestGetCustomFieldFromRegistry()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.DocsAndCartage.JP_CustomAttrib1 = "Custom 1";
			declaration.DocsAndCartage.JP_CustomAttrib2 = "Custom 2";

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Registry1", "Hint"));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Registry2", "Hint"));
			try
			{
				AssertEquals("((ICustomFieldProvider)declaration).GetCustomField(\"Custom1\")", "Custom 1", declaration.GetCustomField("Registry1", null));
				AssertEquals("((ICustomFieldProvider)declaration).GetCustomField(\"Custom2\")", "Custom 2", declaration.GetCustomField("Registry2", null));
			}
			finally
			{
				FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint());
				FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint());
			}
		}

		#endregion

		#region IContainerParent

		public void TestIContainerParent_DocsAndCartage()
		{
			var containerParent = (IContainerParent)Declaration;

			AssertCollectionContains(Declaration.DocsAndCartage, containerParent.DocsAndCartage(Container1.JobContainer));
			AssertEquals(1, containerParent.DocsAndCartage(Container1.JobContainer).Count());
		}

		public void TestIContainerParent_LoadPort()
		{
			var containerParent = (IContainerParent)Declaration;

			AssertNull("Precondition", containerParent.LoadPort);

			Declaration.JE_RL_NKOrigin = "AUSYD";
			AssertEquals("AUSYD", containerParent.LoadPort.Code);
		}

		public void TestIContainerParent_DischargePort()
		{
			var containerParent = (IContainerParent)Declaration;

			AssertNull("Precondition", containerParent.DischargePort);

			Declaration.JE_RL_NKFinalDestination = "DEHAM";
			AssertEquals("DEHAM", containerParent.DischargePort.Code);
		}

		#endregion

		public void TestSortedInvoiceLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			AssertEquals("SortedInvoiceList Count", 2, declaration.SortedInvoiceList.Count);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[0].Code);
			AssertEquals("Invoice number", "1", declaration.SortedInvoiceList[0].Description);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[1].Code);
			AssertEquals("Invoice number", "2", declaration.SortedInvoiceList[1].Description);
		}

		public virtual void TestReadOnlyIncludingChildren()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1111";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "LINE#TEST READ ONLY";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123456";

			declaration.JE_IsCancelled = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals(true, declaration2.ReadOnly);
			AssertEquals(true, declaration2.CusContainers[0].ReadOnly);
			AssertEquals(true, declaration2.Invoices[0].ReadOnly);
			AssertEquals(true, declaration2.InvoiceLines[0].ReadOnly);

			declaration2.JE_IsCancelled = false;
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			var declaration3 = factory3.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals(false, declaration3.ReadOnly);
			AssertEquals(false, declaration3.CusContainers[0].ReadOnly);
			AssertEquals(false, declaration3.Invoices[0].ReadOnly);
			AssertEquals(false, declaration3.InvoiceLines[0].ReadOnly);
		}

		public virtual void TestIsDeclarationWithEntryInstruction()
		{
			Assert("Please override this test if EntryInstruction is supposed to be supported in the country-specific Declaration", Declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestDeclarationEntryProviderIsNeverNull()
		{
			AssertNotNull(Declaration.CustomsEntryInstructionProvider);
		}

		public void TestRefreshIncotermAndChargeFactory()
		{
			CombineAssertions(() =>
			{
				var groupHeader = Declaration.JobComInvoiceGroupHeaders[0];
				var header = Declaration.Invoices.AddNew();
				var testHitter2 = groupHeader.IncoTermAndChargeFactory;
				var testHitter4 = header.IncoTermAndChargeFactory;
				AssertEquals("1-2", false, groupHeader.NeedToGetNewIncoTermAndChargeFactory);
				AssertEquals("1-4", false, header.NeedToGetNewIncoTermAndChargeFactory);

				Declaration.RefreshIncotermAndChargeFactory();
				AssertEquals("2-2", true, groupHeader.NeedToGetNewIncoTermAndChargeFactory);
				AssertEquals("2-4", true, header.NeedToGetNewIncoTermAndChargeFactory);
			});
		}

		public void TestWorstScreeningStatus()
		{
			AssertWorstScreeningStatus(true, ScreeningStatusesList.Codes.Matched);
			AssertWorstScreeningStatus(false, ScreeningStatusesList.Codes.Unknown);
		}

		void AssertWorstScreeningStatus(bool isDeclarationLinkedWithShipment, string expectedWorstScreeningStatus)
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = orgHeader1.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = orgHeader2.PK;

			if (isDeclarationLinkedWithShipment)
			{
				declaration.JE_JS = shipment.PK;
			}

			Factory.Save();

			AssertEquals(expectedWorstScreeningStatus, (declaration as IScreeningPartyProvider).GetWorstScreeningStatus());
		}

		[ExpectNoExceptions]
		public void TestParentShipmentShouldUpdateScreeningStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			(shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = false;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			(declaration as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = false;
			Factory.Save();

			AssertEquals(false, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			(declaration as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;
			AssertEquals(true, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDeclaration = newFactory.Load<BaseJobDeclaration>(declaration.PK);

			declaration.JE_JS = ZGuid.Empty;
			declaration.Delete();
			Factory.Save();

			(reloadedDeclaration as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus = true;
			AssertEquals(false, (shipment as IShouldUpdateScreeningStatus).ShouldUpdateScreeningStatus);
		}

		public virtual void TestSetDefaultPackagesType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("default value for JE_TotalNoOfPacksPackType", "PKG", declaration.JE_TotalNoOfPacksPackType);
		}

		public virtual void TestSetDefaultCusAgentOnSaving()
		{
			GlbStaff.CurrentUser.GS_Code = "XZX";
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			AssertEquals("do not default JE_GS_NKCusAgent if current user is not Broker", "", declaration.JE_GS_NKCusAgent);
		}

		public virtual void TestIsBillIssueDateVisible()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			var declaration = mockDeclaration.Object;
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			Assert("IsBillIssueDateVisible", declaration.IsBillIssueDateVisible);
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			Assert("IsBillIssueDateVisible", !declaration.IsBillIssueDateVisible);
		}

		public virtual void TestSupportScreeningPresentation()
		{
			var mockDeclaration = Factory.NewMoq<T>();
			var declaration = mockDeclaration.Object;
			mockDeclaration.Protected().Setup<ZBool>("SupportScreeningPresentationCore").Returns(true);
			Assert("SupportScreeningPresentation", declaration.SupportScreeningPresentation);
			mockDeclaration.Protected().Setup<ZBool>("SupportScreeningPresentationCore").Returns(false);
			Assert("SupportScreeningPresentation", !declaration.SupportScreeningPresentation);
		}

		public virtual void TestContainerNotLinkedSeverity()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("ContainerNotLinkedSeverity", CargoWise.ComponentModel.NotificationType.Warning, declaration.ContainerNotLinkedSeverity);
		}

		#region Test ICusEntryNumFilterProvider

		public virtual void TestICusEntryNumFilterProviderImplementation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusEntryNumFilterProvider = (ICusEntryNumFilterProvider)declaration;
			var entryNumbers = new CusEntryNumCollection(Factory);

			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertEquals("pre-condition", 0, entryNumbers.Count);

			var entryNum1 = Factory.New<CusEntryNumber>();
			entryNum1.CE_ParentID = declaration.PK;
			entryNum1.CE_EntryNum = "12345678";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12345678"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var entryHeader1 = CreateCancellableEntryHeader(declaration, "12121212");
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212",
				"12345678"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var entryHeader2 = CreateCancellableEntryHeader(declaration, "21212121");
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12121212",
				"12345678",
				"21212121"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			// test with cancelled entry
			CancelEntryHeader(entryHeader1);
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"12345678",
				"21212121"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var otherDeclaration = Factory.New<BaseJobDeclaration>();
			var otherEntryNum = Factory.New<CusEntryNumber>();
			otherEntryNum.CE_ParentID = otherDeclaration.PK;
			otherEntryNum.CE_EntryNum = "55555555";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"55555555"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());

			var otherEntryHeader = CreateCancellableEntryHeader(otherDeclaration, "66666667");
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertArrayEqualsByElements(new string[]
			{
				"55555555",
				"66666667"
			}, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());
		}

		public void TestICusEntryNumFilterProviderShouldNotUseOrInFilter()
		{
			var declaration = GetJobDeclaration();
			var cusEntryNumFilterProvider = (ICusEntryNumFilterProvider)declaration;

			var numberPks = new List<ZGuid>();

			for (int i = 0; i < 10; i++)
			{
				var header = declaration.CustomsEntryHeaders.AddNew();
				header.EntryNumber = i.ToString("00000000");

				numberPks.Add(header.PK);
			}

			var filterString = cusEntryNumFilterProvider.ValidCusEntryNumFilter.FilterPartsHashKey;

			var message = @"Should use 'In' to instead of 'Or' for better performance - https://blogs.msdn.microsoft.com/tess/2008/03/31/net-case-study-stackoverflow-exception-when-using-a-complex-rowfilter/";
			AssertNotContains(message, " OR ", filterString, true);
		}

		protected virtual CusEntryHeader CreateCancellableEntryHeader(BaseJobDeclaration declaration, string entryNumber)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}

		protected virtual void CancelEntryHeader(CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
		}

		#endregion

		public virtual void TestGetContainerModeForDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Liquid));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.AIR, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ShippersConsol));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.Bulk, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Bulk));
			AssertEquals(Core.Constants.ContainerModes.Liquid, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Liquid));
			AssertEquals(Core.Constants.ContainerModes.AIR, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.ShippersConsol));
		}

		#region ICustomsFileParent

		public void TestICustomsFileParent_GetDeclarationTypeFromInfo()
		{
			var declaration = (ICustomsFileParent)Factory.New<BaseJobDeclaration>();
			declaration.DeclarationTypeInfo.SetValueFromString("AAA");

			AssertEquals("Should get value from the DeclarationTypeInfo", "AAA", declaration.DeclarationType);
		}

		public void TestICustomsFileParent_BranchPk()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_GB = branch.PK;

			var customsFileParent = (ICustomsFileParent)declaration;

			AssertEquals("Should get value from JE_GB.", declaration.JE_GB, customsFileParent.BranchPk);
		}

		public void TestICustomsFileParent_IsLocked()
		{
			var declaration = (ICustomsFileParent)Factory.New<BaseJobDeclaration>();

			var log = declaration.Logs.AddNew(AutoEvents.UnlockForEdit);
			AssertEquals("Should not be locked as there is no active LCK log.", false, declaration.IsLocked);

			log = declaration.Logs.AddNew(AutoEvents.LockForEdit);
			AssertEquals("Should be locked as there is an active LCK log.", true, declaration.IsLocked);

			log.Cancel();
			AssertEquals("Should not be locked as there is no active LCK log.", false, declaration.IsLocked);
		}

		public void TestICustomsFileParent_LockFile()
		{
			var declaration = (ICustomsFileParent)Factory.New<BaseJobDeclaration>();

			var log1 = declaration.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = declaration.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = declaration.Logs.AddNew(AutoEvents.Attached);

			declaration.LockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = declaration.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
			AssertNotNull("Should add a new LCK event.", newLog);
		}

		public void TestICustomsFileParent_UnlockFile()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();

			var log1 = declaration.Logs.AddNew(AutoEvents.LockForEdit);
			var log2 = declaration.Logs.AddNew(AutoEvents.UnlockForEdit);
			var log3 = declaration.Logs.AddNew(AutoEvents.Attached);
			var log4 = entryHeader2.Logs.AddNew(AutoEvents.LockForEdit);

			var logOnEntryHeader = entryHeader1.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNull(logOnEntryHeader);

			logOnEntryHeader = entryHeader2.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNull(logOnEntryHeader);

			var fileParent = (ICustomsFileParent)declaration;
			fileParent.UnlockFile(string.Empty);

			Assert("Should only cancel all existing LCK or UCK events.", log1.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log2.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", log4.SL_IsCancelled);
			Assert("Should only cancel all existing LCK or UCK events.", !log3.SL_IsCancelled);

			var newLog = declaration.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNotNull("Should add a new UCK event.", newLog);

			logOnEntryHeader = entryHeader1.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNull(logOnEntryHeader);

			logOnEntryHeader = entryHeader2.Logs.MostRecentLogByEventTime(AutoEvents.UnlockForEdit);
			AssertNotNull("Should add a UCK event from declaration.", logOnEntryHeader);
		}

		#endregion

		public void TestDeleteCusPackingListWhenDeclarationIsDeleted()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var packingList = dec.LoadOrCreateCusPackingList(Factory);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var query = new ZQuery(CusPackingListSchema.PK, packingList.PK);
			var loadedPackingList = otherFactory.LoadTop1<CusPackingList>(query);
			AssertNotNull(loadedPackingList);

			dec.Delete();
			Factory.Save();

			otherFactory = new BusinessObjectFactory();
			loadedPackingList = otherFactory.LoadTop1<CusPackingList>(query);
			AssertNull(loadedPackingList);
		}

		public void TestDeleteEquipmentsWhenDeclarationIsDeleted()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var equipment = dec.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedEquipment = otherFactory.Load<CusEquipment>(equipment.PK);
			AssertNotNull(loadedEquipment);

			dec.Delete();
			Factory.Save();

			otherFactory = new BusinessObjectFactory();
			loadedEquipment = otherFactory.Load<CusEquipment>(equipment.PK);
			AssertNull(loadedEquipment);
		}

		public virtual void TestGetNewCusEquipmentCollection()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AssertType<CusEquipmentCollection<CusEquipment>>(dec.Equipments);
		}

		public void TestHasCusPackingList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			Assert("Packing List should not exist", !dec.HasCusPackingList(new BusinessObjectFactory()));

			Factory.Save();
			dec.LoadOrCreateCusPackingList(Factory);
			Assert("Packing List should not exist", !dec.HasCusPackingList(new BusinessObjectFactory()));

			Factory.Save();
			Assert("Declaration has a Packing List", dec.HasCusPackingList(new BusinessObjectFactory()));
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			fTestDec = null;
		}

		BaseJobDeclaration fTestDec;
		BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = GetJobDeclaration();
				}
				return fTestDec;
			}
		}

		OrgHeader fPartyInCurrentCountry;
		protected OrgHeader PartyInCurrentCountry
		{
			get
			{
				if (fPartyInCurrentCountry == null)
				{
					fPartyInCurrentCountry = OrgHeader.New(Factory);
					fPartyInCurrentCountry.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				}
				return fPartyInCurrentCountry;
			}
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		void AssertIsDPSFreightMovementRestrictedCore_AllJobs(bool enableComplianceRisk)
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
			{
				var registryItem = OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions;
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ScreeningStatus = "MAT";

				var dec = Factory.New<BaseJobDeclarationForTesting>();
				dec.JE_ScreeningStatus = "CLR";
				dec.JE_JS = shipment.PK;

				AssertEquals(true, dec.IsDPSFreightMovementRestrictedCore_Exposed());

				shipment.JS_ScreeningStatus = "CLR";
				dec.JE_ScreeningStatus = "MAT";
				if (enableComplianceRisk)
				{
					AssertEquals("Restricted Check from CPW", true, dec.IsDPSFreightMovementRestrictedCore_Exposed());

					var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
					complianceRiskStatus.COR_ParentID = shipment.PK;
					complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
					complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
					AssertEquals("Restricted Check from CPW", false, dec.IsDPSFreightMovementRestrictedCore_Exposed());

					complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.HighRisk;
					AssertEquals("Restricted Check from CPW", true, dec.IsDPSFreightMovementRestrictedCore_Exposed());
				}
				else
				{
					AssertEquals(false, dec.IsDPSFreightMovementRestrictedCore_Exposed());
				}
			}
		}

		void AssertIsDPSFreightMovementRestrictedCore_InternationalJobs(bool enableComplianceRisk)
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
			{
				var registryItem = OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions;
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.Int);

				var dec = Factory.New<BaseJobDeclarationForTesting>();
				dec.JE_ScreeningStatus = "MAT";
				dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

				CombineAssertions(() =>
				{
					AssertEquals(false, dec.IsExport);
					AssertEquals(false, dec.IsImport);
					AssertEquals(false, dec.IsDPSFreightMovementRestrictedCore_Exposed());
				});

				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				CombineAssertions(() =>
				{
					AssertEquals(false, dec.IsExport);
					AssertEquals(true, dec.IsImport);
					AssertEquals(true, dec.IsDPSFreightMovementRestrictedCore_Exposed());
				});

				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				CombineAssertions(() =>
				{
					AssertEquals(true, dec.IsExport);
					AssertEquals(false, dec.IsImport);
					AssertEquals(true, dec.IsDPSFreightMovementRestrictedCore_Exposed());
				});
			}
		}

		class CanNotBeSavedDeclaration : BaseJobDeclaration
		{
			public CanNotBeSavedDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				if (CanNotBeSaved)
				{
					throw new Exception("Do not save.");
				}
			}

			public bool CanNotBeSaved { get; set; }
		}

		protected class TestBaseJobDeclaration : BaseJobDeclaration
		{
			public TestBaseJobDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ThreadStatic]
			public static ZString DefaultDepartmentTransportMode;
			protected override string CurrentDepartmentTransportMode
			{
				get { return DefaultDepartmentTransportMode; }
			}

			public new StmNoteContexts NoteContextsForRelatedNotes
			{
				get { return base.NoteContextsForRelatedNotes; }
			}

			protected override JobDeclarationLookups GetNewLookups()
			{
				return new LookupsForTestingMessageSubType(this);
			}
		}

		class LookupsForTestingMessageSubType : JobDeclarationLookups
		{
			public LookupsForTestingMessageSubType(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			public override CodeDescriptionPairList MessageSubTypeList
			{
				get
				{
					if (fMessageSubTypeList == null)
					{
						fMessageSubTypeList = base.MessageSubTypeList;
						fMessageSubTypeList.AddPair("SUB", "Subtype description");
					}
					return fMessageSubTypeList;
				}
			}

			CodeDescriptionPairList fMessageSubTypeList;
		}

		protected GlbBranch BranchInCurrentCountryOnDifferentCompany
		{
			get
			{
				if (fBranchInCurrentCountryOnDifferentCompany == null)
				{
					GlbCompany otherCompany = Factory.New<GlbCompany>();
					otherCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fBranchInCurrentCountryOnDifferentCompany = otherCompany.Branches.AddNew();
					fBranchInCurrentCountryOnDifferentCompany.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return fBranchInCurrentCountryOnDifferentCompany;
			}
		}
		GlbBranch fBranchInCurrentCountryOnDifferentCompany;

		protected GlbBranch BranchInOtherCountry
		{
			get
			{
				if (fBranchInOtherCountry == null)
				{
					GlbCompany otherCompany = Factory.New<GlbCompany>();
					otherCompany.GC_RN_NKCountryCode = ZString.Empty;
					fBranchInOtherCountry = otherCompany.Branches.AddNew();
					fBranchInOtherCountry.GB_RL_NKHomePort = "ZZXXX";
				}
				return fBranchInOtherCountry;
			}
		}
		GlbBranch fBranchInOtherCountry;
		#endregion

		[ExpectNoExceptions]
		public void TestGetAddressBookSelection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var selection = ((ISendEmailSource)shipment).GetAddressBookSelection();
			AssertEquals("No declaration yet", 0, selection.Recipients.Count);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierContact = supplier.Contacts.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			AssertEquals("Customs Declaration attached", true, shipment.Declarations.Contains(declaration));
			selection = ((ISendEmailSource)shipment).GetAddressBookSelection();
			AssertEquals(1, selection.Recipients.Count);
		}

		public void TestNoUncessaryActiveHeadersAreCreated()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			for (int i = 0; i < 300; i++)
			{
				var activeHeaders = declaration.ActiveEntryHeaders;
			}
			var header = declaration.ActiveEntryHeaders.AddNew();
			var count = ((IBusinessObjectInternals)header).ParentCollections.Length;
			Assert($"CusEntryHeader has {count} parent collections", count < 100);
		}

		public virtual void TestNewOwner()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclarationWithEntryInstructions>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			owner1.OH_Code = "X1$1";
			owner1.OH_FullName = owner1.OH_Code;

			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			owner2.OH_Code = "X2$2";
			owner2.OH_FullName = owner2.OH_Code;

			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "G1";
			instruction1.CEI_Description = "AA";
			instruction1.CEI_OH_Owner = owner1.PK;

			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "D1";
			instruction2.CEI_Description = "BB";
			instruction2.CEI_OH_Owner = owner2.PK;

			AssertEquals(instruction1.Owner, declaration.NewOwner);

			instruction2.CEI_OH_Owner = ZGuid.Empty;
			AssertNotNull(declaration.NewOwner);
		}

		public virtual void TestProcedureProcessings()
		{
			var testItem = Factory.New<BaseJobDeclaration>();
			AssertEquals("SupportInwardProcessing should be false by default.", false, testItem.SupportInwardProcessing);
			AssertEquals("SupportOutwardProcessing should be false by default.", false, testItem.SupportOutwardProcessing);

			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				AssertEquals(true, declaration.SupportInwardProcessing);
				AssertEquals(false, declaration.SupportOutwardProcessing);
			}
		}

		public void TestLoadOrCreateCusPackingList()
		{
			var declaration = Factory.New<T>();
			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var loadedDeclaration = otherFactory.Load<T>(declaration.PK);
			var loadedPackingList = loadedDeclaration.LoadOrCreateCusPackingList(otherFactory);
			AssertEquals(packingList.PK, loadedPackingList.PK);
			AssertEquals(ExpectedLoadOrCreateCusPackingListType, loadedPackingList.GetType());
		}

		public void TestLoadCusPackingList()
		{
			var declaration = Factory.New<T>();
			var loadedPackingList = declaration.LoadCusPackingList(Factory);
			AssertNull(loadedPackingList);

			var packingList = Factory.New<CusPackingList>();
			packingList.CUL_JE = declaration.PK;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationOtherFactory = otherFactory.Load<T>(declaration.PK);
			loadedPackingList = declarationOtherFactory.LoadCusPackingList(Factory);
			AssertNotNull(loadedPackingList);
			AssertEquals(ExpectedLoadOrCreateCusPackingListType, loadedPackingList.GetType());
		}

		[TestDate(2022, 03, 20)]
		public virtual void TestCreateCusPackingList()
		{
			var declaration = Factory.New<T>();
			declaration.JE_GoodsDescription = "Goods Description";
			var createdPackingList = declaration.CreateCusPackingList(Factory);
			AssertEquals(ExpectedPackingListDescription, createdPackingList.CUL_Description);
			AssertEquals(new ZDate(2022, 03, 20), createdPackingList.CUL_PackingListDate);
			AssertEquals(ExpectedLoadOrCreateCusPackingListType, createdPackingList.GetType());
		}

		public virtual void TestSupportsCusPackingList()
		{
			var declaration = Factory.New<T>();
			Assert("The default value should be false", !declaration.SupportsCusPackingList);
		}

		protected virtual Type ExpectedLoadOrCreateCusPackingListType => typeof(CusPackingList);

		protected virtual string ExpectedPackingListDescription => "Goods Description";

		public void TestNoDocsAndCartageExistAfterCreatingDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, Factory.ExistsInDatabase(JobDocsAndCartageSchema.Constants.TableName, new ZQuery(JobDocsAndCartageSchema.JP_ParentID, declaration.PK)));
		}

		[TestDate(2021, 05, 18)]
		public void TestIAuditColumnProviderForBOLogger()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(declaration.JE_AuditReferenceInfo.MaxLength, declaration.AuditReferenceMaxLength);
		}

		public virtual void TestDeclarantCode()
		{
			Declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNullOrEmpty(Declaration.DeclarantCode);

			var declarantOrg = Factory.New<OrgHeader>();
			declarantOrg.OH_Code = "Org1";
			var declarantAddress = declarantOrg.Addresses.AddNew();
			Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			AssertEquals("Org1", Declaration.DeclarantCode);
		}

		public virtual void TestDeclarantName()
		{
			Declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNullOrEmpty(Declaration.DeclarantName);

			var declarantOrg = Factory.New<OrgHeader>();
			declarantOrg.OH_FullName = "Org1";
			var declarantAddress = declarantOrg.Addresses.AddNew();
			Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			AssertEquals("Org1", Declaration.DeclarantName);
		}

		public virtual void TestIsOverrideRecipientEmailEnabled()
		{
			var declarant1 = Factory.New<OrgHeader>();
			var declarant2 = Factory.New<OrgHeader>();

			var jobDeclaration1 = Factory.New<BaseJobDeclaration>();
			jobDeclaration1.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			var jobDeclaration2 = Factory.New<BaseJobDeclaration>();
			jobDeclaration2.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			var jobDeclaration3 = Factory.New<BaseJobDeclaration>();
			jobDeclaration3.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
			var jobDeclaration4 = Factory.New<BaseJobDeclaration>();
			jobDeclaration4.JE_OA_DeclarantAddress = ZGuid.Empty;

			var isOverrideRecipientEmailEnabled = Declaration.IsOverrideRecipientEmailEnabled(typeof(BaseJobDeclaration), new[] { jobDeclaration1.PK });
			Assert("One declaration", isOverrideRecipientEmailEnabled);

			isOverrideRecipientEmailEnabled = Declaration.IsOverrideRecipientEmailEnabled(typeof(BaseJobDeclaration), new[] { jobDeclaration1.PK, jobDeclaration2.PK });
			Assert("Declarants for both declarations are the same", isOverrideRecipientEmailEnabled);

			isOverrideRecipientEmailEnabled = Declaration.IsOverrideRecipientEmailEnabled(typeof(BaseJobDeclaration), new[] { jobDeclaration1.PK, jobDeclaration3.PK });
			Assert("Declarants are different", !isOverrideRecipientEmailEnabled);

			isOverrideRecipientEmailEnabled = Declaration.IsOverrideRecipientEmailEnabled(typeof(BaseJobDeclaration), new[] { jobDeclaration1.PK, jobDeclaration4.PK });
			Assert("One of the declarations doesn't have a declarant", !isOverrideRecipientEmailEnabled);
		}

		public void TestInvoices_IBaseJobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertSame("The implementation of IBaseJobDeclaration.Invoices should be the same as this.Invoices", declaration.Invoices, ((IBaseJobDeclaration)declaration).Invoices);
		}

		#region Product Refresh

		public void TestRefreshInvoiceLineProducts()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var partPK = SaveNewPart(Factory, "PARTNUM", Importer, null, "DESC").PK;
			var invLine = CreateInvoiceLine(Factory, dec, "PARTNUM", Importer.PK, Supplier.PK);

			var part2PK = SaveNewPart(Factory, "PARTNUM2", Importer, null, "DESC2").PK;
			var invLine2 = CreateInvoiceLine(Factory, dec, "PARTNUM2", Importer.PK, Supplier.PK);

			ChangePartDescription(partPK, "NEWDESC");
			ChangePartDescription(part2PK, "NEWDESC2");

			dec.RefreshInvoiceLineProducts();
			AssertEquals("Line Description", invLine.ShouldSetDescriptionWhenPartNoChanges ? "NEWDESC" : "", invLine.JI_Description);
			AssertEquals("Line Description", invLine.ShouldSetDescriptionWhenPartNoChanges ? "NEWDESC2" : "", invLine2.JI_Description);
		}

		public void TestGetOrgHeaderList()
		{
			var declaration = Factory.New<BaseJobDeclarationWithIsLooksCachedInBaseExposed>();
			if (declaration.IsLookupsCachedInBaseExposed)
			{
				var lookups = declaration.Lookups;
				var iDocAddresses = (IDocAddresses)declaration;
				AssertSame(lookups.ImportersList, iDocAddresses.GetOrgHeaderList(DocAddressType.ImporterDocumentaryAddress));
				AssertSame(lookups.SuppliersList, iDocAddresses.GetOrgHeaderList(DocAddressType.SupplierDocumentaryAddress));
				AssertSame(lookups.DepotCollection, iDocAddresses.GetOrgHeaderList(DocAddressType.CustomsDepotAddress));
				AssertSame(lookups.NotifyParties, iDocAddresses.GetOrgHeaderList(DocAddressType.NotifyParty));
				AssertSame(lookups.NotifyParties, iDocAddresses.GetOrgHeaderList(DocAddressType.NotifyParty2));
				AssertSame(lookups.NotifyParties, iDocAddresses.GetOrgHeaderList(DocAddressType.NotifyParty3));
				AssertSame(lookups.Buyers, iDocAddresses.GetOrgHeaderList(DocAddressType.BuyerDocumentaryAddress));
				AssertSame(lookups.ContainerYardCollection, iDocAddresses.GetOrgHeaderList(DocAddressType.CustomsContainerYardAddress));
				AssertSame(lookups.BondedWarehouseCollection, iDocAddresses.GetOrgHeaderList(DocAddressType.CustomsWarehouseAddress));
				AssertSame(lookups.CarrierOrganisations, iDocAddresses.GetOrgHeaderList(DocAddressType.Carrier));
				AssertSame(lookups.SellingAgents, iDocAddresses.GetOrgHeaderList(DocAddressType.SellingParty));
				AssertSame(lookups.Exporters, iDocAddresses.GetOrgHeaderList(DocAddressType.Exporter));
				AssertSame(lookups.ExternalBrokers, iDocAddresses.GetOrgHeaderList(DocAddressType.ExternalBroker));
			}
		}

		OrgHeader Importer
		{
			get { return importer ?? (importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS")); }
		}
		OrgHeader importer;

		OrgHeader Supplier
		{
			get { return supplier ?? (supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABABEU")); }
		}
		OrgHeader supplier;

		BaseJobComInvoiceLine CreateInvoiceLine(BusinessObjectFactory factory, BaseJobDeclaration declaration, ZString partNum, ZGuid importerPK, params ZGuid[] supplierPKs)
		{
			declaration.JE_OH_Importer = importerPK;

			BaseJobComInvoiceLine result = null;
			var firstHeader = true;
			foreach (ZGuid supplierPK in supplierPKs)
			{
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				if (firstHeader)
				{
					result = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				invoiceHeader.JZ_OH_Supplier = supplierPK;
				firstHeader = false;
			}
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(factory, result.PartSyncManagerActiveDeciderPK);
			result.JI_PartNo = partNum;
			return result;
		}

		OrgSupplierPart SaveNewPart(BusinessObjectFactory factory, ZString partNum, OrgHeader importer, OrgHeader supplier, ZString description)
		{
			var newPart = factory.New<OrgSupplierPart>();
			if (importer != null)
			{
				newPart.RelatedOrganisations.AddOwner(importer);
			}
			if (supplier != null)
			{
				newPart.RelatedOrganisations.AddSupplier(supplier);
			}
			newPart.OP_PartNum = partNum;
			newPart.OP_Desc = description;
			factory.Save();
			return newPart;
		}

		void ChangePartDescription(ZGuid partPK, ZString newDescription)
		{
			var factory = new BusinessObjectFactory();
			var part = factory.Load<OrgSupplierPart>(partPK);
			part.OP_Desc = newDescription;
			factory.Save();
		}

		class BaseJobDeclarationWithIsLooksCachedInBaseExposed : BaseJobDeclaration
		{
			public BaseJobDeclarationWithIsLooksCachedInBaseExposed(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsLookupsCachedInBaseExposed => IsLookupsCachedInBase;
		}

		#endregion

		#region JE_MessageType

		public virtual void TestIsMessageTypeChangeAnError()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (DataRegistry.Business.CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.NewWithValidTestData<T>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				AssertEquals("[PreCondition]: IsMessageTypeChangeAnError should be false", false, declaration.IsMessageTypeChangeAnError);

				var message = declaration.Messages.AddNew();
				message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
				Factory.Save();

				AssertEquals("IsMessageTypeChangeAnError when messages have been sent", true, declaration.IsMessageTypeChangeAnError);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("IsMessageTypeChangeAnError for ITF", false, declaration.IsMessageTypeChangeAnError);
			}
		}

		#endregion

		#region IPropertyChecker

		static string ExpectedPropertyUpdateErrorMessage => "The provided screening status value is invalid. Ensure it matches one of the accepted statuses.";

		public void TestIsPropertyUpdatable_WhenUpdatingNonScreeningStatusField_ShouldReturnTrue()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var referenceProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_DeclarationReferenceInfo));

			var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(referenceProp, "ABC123", out _);

			Assert(result);
		}

		public void TestIsPropertyUpdatable_WhenUpdatingUnknownScreeningStatus_ShouldReturnTrue()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "UNK", out _);

			Assert(result);
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalClearScreeningStatusAndLinkedDec_ShouldReturnFalse()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.NewZGuid();
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JCE", out var errorMessage);

			Assert(!result);
			Assert(errorMessage.Equals(ExpectedPropertyUpdateErrorMessage, StringComparison.InvariantCulture));
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalClearScreeningStatusAndRegistryNotSet_ShouldReturnFalse()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.Empty;
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JCE", out var errorMessage);

				Assert(!result);
				Assert(errorMessage.Equals(ExpectedPropertyUpdateErrorMessage, StringComparison.InvariantCulture));
			}
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalBlockScreeningStatusAndLinkedDec_ShouldReturnFalse()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.NewZGuid();
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JBE", out var errorMessage);

			Assert(!result);
			Assert(errorMessage.Equals(ExpectedPropertyUpdateErrorMessage, StringComparison.InvariantCulture));
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalBlockScreeningStatusAndRegistryNotSet_ShouldReturnFalse()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.Empty;
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JBE", out var errorMessage);

				Assert(!result);
				Assert(errorMessage.Equals(ExpectedPropertyUpdateErrorMessage, StringComparison.InvariantCulture));
			}
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalClearScreeningStatus_ShouldReturnTrue()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.Empty;
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JCE", out _);

				Assert(result);
			}
		}

		public void TestIsPropertyUpdatable_WhenUpdatingExternalBlockScreeningStatus_ShouldReturnTrue()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = ZGuid.Empty;
			var screeningStatusProp = jobDeclaration.GetType().GetProperty(nameof(jobDeclaration.JE_ScreeningStatus));

			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = jobDeclaration.IsPropertyUpdatableViaXueAdditionalFields(screeningStatusProp, "JBE", out _);

				Assert(result);
			}
		}

		#endregion

		public virtual void TestPhaseStatus_Caption() => AssertEquals("Caption", "Phase Status", Factory.New<BaseJobDeclaration>().PhaseStatusInfo.Description);

		public virtual void TestPhaseStatusDescription_Caption() => AssertEquals("Caption", "Phase Status Description", Factory.New<BaseJobDeclaration>().PhaseStatusDescriptionInfo.Description);

		public void TestPhaseStatus()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Precondition: No status yet", ZString.Empty, declaration.PhaseStatus);

			var header0 = declaration.ActiveEntryHeaders.AddNew();
			header0.CH_PhaseStatus = "FIN";

			CombineAssertions(() =>
			{
				AssertEquals("FIN", declaration.PhaseStatus);

				var header1 = declaration.ActiveEntryHeaders.AddNew();
				AssertEquals("MUL", declaration.PhaseStatus);

				header1.CH_PhaseStatus = ZString.Empty;
				AssertEquals("MUL", declaration.PhaseStatus);

				header1.CH_PhaseStatus = "FIN";
				AssertEquals("FIN", declaration.PhaseStatus);

				header1.CH_PhaseStatus = "REM";
				AssertEquals("MUL", declaration.PhaseStatus);
			});
		}

		public void TestPhaseStatusDescription()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Precondition: No status yet", ZString.Empty, declaration.PhaseStatusDescription);

			var header0 = declaration.ActiveEntryHeaders.AddNew();
			header0.CH_PhaseStatus = "FIN";

			CombineAssertions(() =>
			{
				AssertNotEquals("Multiple - See Entries", declaration.PhaseStatusDescription);

				var header1 = declaration.ActiveEntryHeaders.AddNew();
				AssertEquals("Multiple - See Entries", declaration.PhaseStatusDescription);

				header1.CH_PhaseStatus = ZString.Empty;
				AssertEquals("Multiple - See Entries", declaration.PhaseStatusDescription);

				header1.CH_PhaseStatus = "FIN";
				AssertNotEquals("Multiple - See Entries", declaration.PhaseStatusDescription);

				header1.CH_PhaseStatus = "REM";
				AssertEquals("Multiple - See Entries", declaration.PhaseStatusDescription);
			});
		}
	}
}
