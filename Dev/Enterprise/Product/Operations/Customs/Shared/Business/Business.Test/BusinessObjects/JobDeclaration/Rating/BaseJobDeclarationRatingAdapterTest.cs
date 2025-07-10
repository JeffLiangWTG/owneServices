using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	class BaseJobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		public void TestPaymentTerm()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ShipmentIncoTerm = IncoTerms.ExWorks;
			var autoRating = declaration.RatingAdapter;

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertContainsExactElementsInAnyOrder
				(
					"Registry disabled: should contain both revenue and cost for IncoTerm",
					new[] { "EXW-Cost", "EXW-Revenue" },
					autoRating.PaymentTerm.PaymentTermInfoCollection.Select(x => $"{x.Value}-{x.CostOrSell}")
				);
			}

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContainsExactElementsInAnyOrder
				(
					"Registry enabled: should contain both revenue and cost for IncoTerm",
					new[] { "EXW-Cost", "EXW-Revenue" },
					autoRating.PaymentTerm.PaymentTermInfoCollection.Select(x => $"{x.Value}-{x.CostOrSell}")
				);
			}
		}

		public void TestRateTypeToUse()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var autoRating = declaration.RatingAdapter;

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Registry disabled", RateType.Forwarding, autoRating.RateTypeToUse);
			}

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Registry enabled", RateType.Customs, autoRating.RateTypeToUse);
			}
		}

		public void TestAutoRatingServiceProvider()
		{
			var externalBroker = Factory.New<OrgHeader>();
			var shippingLine = Factory.New<OrgHeader>();
			var forwarder = Factory.New<OrgHeader>();

			var docsAndCartageHeader = Factory.New<OrgHeader>();
			var docsAndCartage = Factory.New<OrgAddress>();
			docsAndCartage.OA_OH = docsAndCartageHeader.PK;

			var containerTerminalHeader = Factory.New<OrgHeader>();
			var containerTerminal = Factory.New<OrgAddress>();
			containerTerminal.OA_OH = containerTerminalHeader.PK;

			var containerYardHeader = Factory.New<OrgHeader>();
			var containerYard = Factory.New<OrgAddress>();
			containerYard.OA_OH = containerYardHeader.PK;

			var depotDocAddressHeader = Factory.New<OrgHeader>();
			var depotDocAddress = Factory.New<OrgAddress>();
			depotDocAddress.OA_OH = depotDocAddressHeader.PK;

			var declaration = Factory.New<BaseJobDeclaration>();
			var autoRating = declaration.RatingAdapter;
			Assert(!autoRating.Creditors.AllOrgs.Contains(externalBroker));
			Assert(!autoRating.Creditors.AllOrgs.Contains(shippingLine));
			Assert(!autoRating.Creditors.AllOrgs.Contains(forwarder));
			Assert(!autoRating.Creditors.AllOrgs.Contains(docsAndCartage.Header));
			Assert(!autoRating.Creditors.AllOrgs.Contains(containerTerminal.Header));
			Assert(!autoRating.Creditors.AllOrgs.Contains(containerYard.Header));
			Assert(!autoRating.Creditors.AllOrgs.Contains(depotDocAddress.Header));

			declaration.JE_OH_ExternalBroker = externalBroker.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(externalBroker));

			declaration.JE_OH_ShippingLine = shippingLine.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(shippingLine));

			declaration.JE_OH_Forwarder = forwarder.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(forwarder));

			declaration.DocsAndCartage.JP_OA_PickupCartageCoAddr = docsAndCartage.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(docsAndCartage.Header));

			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = containerTerminal.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(containerTerminal.Header));

			declaration.ContainerYardDocAddress.E2_OA_Address = containerYard.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(containerYard.Header));

			declaration.DepotDocAddress.E2_OA_Address = depotDocAddress.PK;
			Assert(autoRating.Creditors.AllOrgs.Contains(depotDocAddress.Header));
		}

		public void TestGetStatusInformationCoreMergesAutomatically()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var initiator = new SendsMessagesToCustomsShutterUpperer();
			initiator.ThrowExceptionOnInvalidOperation = false;
			declaration.MessageInitiator = initiator;
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();
			declaration.DoMerge();
			Assert("Pre-Requisite", !declaration.MergeManager.RequiresMerge);
			Assert("can execute", declaration.RatingAdapter.StatusInformation.CanExecute);
			declaration.JE_MergeBy = ZString.Empty;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("merge required", declaration.MergeManager.RequiresMerge);
			Assert("can execute", declaration.RatingAdapter.StatusInformation.CanExecute);
			Assert("merge has been done", !declaration.MergeManager.RequiresMerge);
			header = declaration.Invoices.AddNew();
			declaration.JE_MergeBy = ZString.Empty;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("merge required", declaration.MergeManager.RequiresMerge);
			Assert("can NOT execute", !declaration.RatingAdapter.StatusInformation.CanExecute);
			Assert(declaration.MergeManager.InvalidOperationText.Contains("You can't merge this entry because there is an invoice header with no invoice lines."));
		}

		public void TestSetMessageInitiator()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var header = declaration.Invoices.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MergeBy = ZString.Empty;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("Pre-Requisite", declaration.MergeManager.RequiresMerge);
			bool canExecute = false;
			AssertNoExceptionThrown("can execute", () => canExecute = declaration.RatingAdapter.StatusInformation.CanExecute);
			Assert("can NOT execute", !canExecute);
			var messageInitiator = declaration.MessageInitiator as SendsMessagesToCustomsReturningResultsAsProperties;
			AssertNotNull("MessageInitiator should be set", messageInitiator);
			AssertEquals("Invalid Operation: You can't merge this entry because there is an invoice header with no invoice lines.", messageInitiator.MergeResult);
		}

		public void TestStatusInformation()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var header = declaration.Invoices.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MergeBy = ZString.Empty;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("Pre-Requisite", declaration.MergeManager.RequiresMerge);
			AssertEquals("Invalid Operation: You can't merge this entry because there is an invoice header with no invoice lines.", declaration.RatingAdapter.StatusInformation.Message);
		}

		#region IAutoRating

		public void TestImportDeclarationConsignorConsignee()
		{
			var miscOrg = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			var deliveryConsignee = Factory.New<OrgHeader>();
			deliveryConsignee.OH_IsConsignee = true;
			var controller = Factory.New<OrgHeader>();

			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_ControllingCustomer = controller.PK;
			TestDec.JE_MessageType = DefaultImportMessageType;

			var adapter = TestDec.RatingAdapter;
			AssertEquals(importer.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);
			AssertEquals(supplier.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(controller.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS].PK);

			TestDec.JE_OH_Supplier = ZGuid.Empty;
			AssertNull("no fall backs for cnr on import", adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);

			TestDec.JE_OH_ControllingCustomer = ZGuid.Empty;
			AssertNull("no fall backs for CCUS on import", adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);

			TestDec.JE_OH_Importer = deliveryConsignee.PK;
			AssertEquals("Delivery consignee is prefered", deliveryConsignee.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);

			TestDec.ImporterDeliveryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(deliveryConsignee.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);

			TestDec.JE_OH_Importer = miscOrg;
			AssertEquals("Misc org cannot be consignee", null, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);

			TestDec.JE_OH_Importer = ZGuid.Empty;
			AssertNull("no fall backs", adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
		}

		public void TestAdapterTypeAndID()
		{
			AssertEquals(AdapterType.CustomsDeclaration, TestDec.RatingAdapter.AdapterType);
			AssertEquals(TestDec.JE_DeclarationReference, TestDec.RatingAdapter.OperationalJobCode);
		}

		public void TestExportDeclarationConsignorConsignee()
		{
			var miscOrg = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation;
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_IsConsignor = true;
			var importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			var pickupConsignor = Factory.New<OrgHeader>();
			pickupConsignor.OH_IsConsignor = true;
			var controller = Factory.New<OrgHeader>();

			TestDec.JE_OH_Supplier = supplier.PK;
			TestDec.JE_OH_Importer = importer.PK;
			TestDec.JE_OH_ControllingCustomer = controller.PK;
			TestDec.JE_MessageType = DefaultExportMessageType;

			var adapter = TestDec.RatingAdapter;
			AssertEquals(importer.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE].PK);
			AssertEquals(supplier.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);
			AssertEquals(controller.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS].PK);

			TestDec.JE_OH_Importer = ZGuid.Empty;
			AssertNull("no fall backs for cne on export", adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);

			TestDec.JE_OH_ControllingCustomer = ZGuid.Empty;
			AssertNull("no fall backs for CCUS on import", adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);

			TestDec.JE_OH_Supplier = pickupConsignor.PK;
			AssertEquals("Pickup consignor is prefered", pickupConsignor.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);

			TestDec.SupplierPickupAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(pickupConsignor.PK, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR].PK);

			TestDec.JE_OH_Supplier = miscOrg;
			AssertEquals("Misc org cannot be consignor", null, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);

			TestDec.JE_OH_Supplier = ZGuid.Empty;
			AssertNull("no fall backs", adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
		}

		public void TestChargeCodeGroups()
		{
			Env.Registry.Rating.SetFreightRatedCodes("OBR,ORG,LOD,UNL,DST,BRK,INS,FRT");
			Env.Registry.Rating.SetBrokerageRatedCodes("BRK,BON,CDS,DST");
			Env.Registry.Rating.SetOriginBrokerageRatedCodes("OBR,OBO,ORG");

			var declaration = BaseJobDeclaration.New(Factory);
			var autoRating = declaration.RatingAdapter;

			declaration.JE_MessageType = DefaultImportMessageType;
			Assert(autoRating.IsImport());
			AssertEquals(4, autoRating.ChargeCodeGroups.Count);

			declaration.JE_MessageType = DefaultExportMessageType;
			Assert(!autoRating.IsImport());
			AssertEquals(3, autoRating.ChargeCodeGroups.Count);

			declaration = GetJobDeclaration();
			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_JS = Factory.New(typeof(ForwardingShipment)).PK;
			autoRating = declaration.RatingAdapter;
			declaration.JE_MessageType = DefaultImportMessageType;

			Assert(autoRating.IsImport());
			AssertEquals(2, autoRating.ChargeCodeGroups.Count);
			Assert(autoRating.ChargeCodeGroups.Contains("BRK"));
			Assert(autoRating.ChargeCodeGroups.Contains("CDS"));

			declaration.JE_MessageType = DefaultExportMessageType;
			Assert(!autoRating.IsImport());
			AssertEquals(1, autoRating.ChargeCodeGroups.Count);
			Assert(autoRating.ChargeCodeGroups.Contains("OBR"));
		}

		public void TestAutoRatingTime()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "###";

			var autoRating = TestDec.RatingAdapter;

			TestDec.JE_PickupOrDeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 1, 0, 0);
			TestDec.JE_DeliveryOrPickupLabourTime = new ZDateTime(2006, 1, 1, 2, 0, 0);

			AssertEquals("Time for empty subgroup", 0d, autoRating.JobServices.Time(chargeCode)?.Span.TotalHours ?? 0);
			chargeCode.AC_ChargeSubGroup = "DME";
			AssertEquals("Detention (hours)", 1d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
			chargeCode.AC_ChargeSubGroup = "LBR";
			AssertEquals("Labor (hours)", 2d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestAutoRatingJobServicesFromChildContainers()
		{
			var autoRating = TestDec.RatingAdapter;
			TestDec.JE_MessageType = ImportMessageTypeForTest;
			var chargeCodeGroup = "BON";
			var cusContainerWithOwnService = TestDec.CusContainers.AddNew();
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			var quarantine = cusContainerWithOwnService.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			quarantine.ES_Completed = ZDateTime.Today;
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));
		}

		public void TestAutoRatingJobServicesWithShipment()
		{
			var autoRating = TestDec.RatingAdapter;
			var shipment = Factory.New<CommonShipment>();
			TestDec.JE_JS = shipment.PK;
			shipment.JS_E_DEP = new ZDateTime(2001, 1, 1);
			var chargeCodeGroup = "OBR";
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			var quarantine = TestDec.DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			quarantine.ES_Completed = new ZDateTime(2000, 12, 1);
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			chargeCodeGroup = "BRK";
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.Tailgate));

			var tailgate = TestDec.DocsAndCartage.Services.AddNew();
			tailgate.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.Tailgate));

			tailgate.ES_Completed = new ZDateTime(2001, 2, 1);
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.Tailgate));
		}

		public void TestAutoRatingJobServicesTime()
		{
			var chargeCodeFilter = new ZQuery(
			new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK),
			new ZQuery(AccChargeCodeSchema.AC_Code, "OQUAR"));

			var chargeCode = Factory.Load<AccChargeCode>(chargeCodeFilter).FirstOrDefault();

			var autoRating = TestDec.RatingAdapter;
			var container = TestDec.CusContainers.AddNew();
			var service = container.Services.AddNew();
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			service.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);
			service.ES_Completed = ZDateTime.Today;

			AssertEquals(2d, autoRating.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestAutoRatingJobServices()
		{
			var autoRating = TestDec.RatingAdapter;
			TestDec.JE_MessageType = ImportMessageTypeForTest;
			var chargeCodeGroup = "BON";
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageDemurrageTotal));

			TestDec.JE_PickupOrDeliveryTruckWaitTime = new ZDateTime(2006, 1, 1, 1, 0, 0);
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageDemurrageTotal));

			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.Labor));
			TestDec.JE_DeliveryOrPickupLabourTime = new ZDateTime(2006, 1, 1, 2, 0, 0);
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.Labor));

			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));
			var quarantine = TestDec.DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			quarantine.ES_Completed = ZDateTime.Today;
			Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, Core.Constants.FreightServiceType.Codes.QuarantineInspection));

			var oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");

				var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
				var rateTransportZoneHelper = new Mock<IRateTransportZoneHelper>(MockBehavior.Strict);

				using (var ctx = ObjectFactory.Substitute(rateTransportZoneHelper.Object))
				{
					var sydney = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
					TestDec.JE_RL_NKOrigin = "AUSYD";
					TestDec.SupplierPickupAddress.E2_Postcode = "2000";
					TestDec.SupplierPickupAddress.E2_City = "Sydney";
					TestDec.SupplierPickupAddress.E2_RN_NKCountryCode = "AU";

					var auckland = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
					TestDec.JE_RL_NKFinalDestination = "NZAKL";
					TestDec.ImporterDeliveryAddress.E2_Postcode = "32567";
					TestDec.ImporterDeliveryAddress.E2_RN_NKCountryCode = "NZ";

					TestDec.JE_MessageType = DefaultExportMessageType;
					chargeCodeGroup = "OBO";
					TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;

					rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory, transportProvider, sydney, "AU", "2000", "Sydney")).Returns(false);
					Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));
					rateTransportZoneHelper.VerifyAll();

					var alexandria = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
					TestDec.JE_RL_NKOrigin = "AUSYD";
					TestDec.SupplierPickupAddress.E2_Postcode = "2000";
					TestDec.SupplierPickupAddress.E2_City = "Alexandria";

					rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory, transportProvider, alexandria, "AU", "2000", "Alexandria")).Returns(true);
					Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));
					rateTransportZoneHelper.VerifyAll();

					TestDec.JE_MessageType = DefaultImportMessageType;
					chargeCodeGroup = "BON";
					TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;
					rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory, transportProvider, auckland, "NZ", "32567", "")).Returns(false);
					Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));
					rateTransportZoneHelper.VerifyAll();

					rateTransportZoneHelper.Setup(m => m.IsBeyond(Factory, transportProvider, auckland, "NZ", "32567", "")).Returns(true);
					Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));
					rateTransportZoneHelper.VerifyAll();
				}

				GlbCompany.CurrentCompany.SetCountry("US");

				TestDec.JE_RL_NKOrigin = "USCHI";
				TestDec.JE_RL_NKFinalDestination = "CATOR";

				TestDec.JE_MessageType = DefaultExportMessageType;
				TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				var aCIConsignorOriginZone = Factory.New<RefDomesticCartageZone>();
				aCIConsignorOriginZone.F1_RL_NKLoco = "USCHI";
				aCIConsignorOriginZone.F1_IsBeyond = true;
				aCIConsignorOriginZone.F1_CityTownPostCode = "2000";
				aCIConsignorOriginZone.F1_CityTown = "Sydney";
				aCIConsignorOriginZone.F1_PortCode = aCIConsignorOriginZone.Loco.RL_IATA;

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				var aCIConsignorOriginZone2 = Factory.New<RefDomesticCartageZone>();
				aCIConsignorOriginZone2.F1_RL_NKLoco = "USCHI";
				aCIConsignorOriginZone2.F1_IsBeyond = true;
				aCIConsignorOriginZone2.F1_CityTownPostCode = "2000";
				aCIConsignorOriginZone2.F1_CityTown = "Alexandria";
				aCIConsignorOriginZone2.F1_PortCode = aCIConsignorOriginZone2.Loco.RL_IATA;

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone2.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				TestDec.JE_MessageType = DefaultImportMessageType;
				TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				var aCIConsigneeOriginZone = Factory.New<RefDomesticCartageZone>();
				aCIConsigneeOriginZone.F1_RL_NKLoco = "CATOR";
				aCIConsigneeOriginZone.F1_IsBeyond = true;
				aCIConsigneeOriginZone.F1_CityTownPostCode = "32567";
				aCIConsigneeOriginZone.F1_PortCode = "YTO";

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsigneeOriginZone.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone.Delete();
				aCIConsignorOriginZone2.Delete();
				aCIConsigneeOriginZone.Delete();
				GlbCompany.CurrentCompany.SetCountry("PR");

				TestDec.JE_RL_NKOrigin = "USCHI";
				TestDec.JE_RL_NKFinalDestination = "CATOR";

				TestDec.JE_MessageType = DefaultExportMessageType;
				TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone = Factory.New<RefDomesticCartageZone>();
				aCIConsignorOriginZone.F1_RL_NKLoco = "USCHI";
				aCIConsignorOriginZone.F1_IsBeyond = true;
				aCIConsignorOriginZone.F1_CityTownPostCode = "2000";
				aCIConsignorOriginZone.F1_CityTown = "Sydney";
				aCIConsignorOriginZone.F1_PortCode = aCIConsignorOriginZone.Loco.RL_IATA;

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone2 = Factory.New<RefDomesticCartageZone>();
				aCIConsignorOriginZone2.F1_RL_NKLoco = "USCHI";
				aCIConsignorOriginZone2.F1_IsBeyond = true;
				aCIConsignorOriginZone2.F1_CityTownPostCode = "2000";
				aCIConsignorOriginZone2.F1_CityTown = "Alexandria";
				aCIConsignorOriginZone2.F1_PortCode = aCIConsignorOriginZone2.Loco.RL_IATA;

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsignorOriginZone2.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				TestDec.JE_MessageType = DefaultImportMessageType;
				TestDec.JE_OA_DeliveryOrPickupCartageCoAddr = transportProvider.MainAddress.PK;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsigneeOriginZone = Factory.New<RefDomesticCartageZone>();
				aCIConsigneeOriginZone.F1_RL_NKLoco = "CATOR";
				aCIConsigneeOriginZone.F1_IsBeyond = true;
				aCIConsigneeOriginZone.F1_CityTownPostCode = "32567";
				aCIConsigneeOriginZone.F1_PortCode = "YTO";

				Assert(autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));

				aCIConsigneeOriginZone.F1_IsBeyond = false;
				Assert(!autoRating.JobServices.IsEnabled(chargeCodeGroup, ChargeCodeSubGroupList.CartageBeyondPostcode));
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public virtual void TestFreightModeForPortDeliveryTime()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeAirCodeForTesting;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Air, TestDec.FreightModeForPortDeliveryTimes);

			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, TestDec.FreightModeForPortDeliveryTimes);

			TestDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, TestDec.FreightModeForPortDeliveryTimes);

			TestDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Sea, TestDec.FreightModeForPortDeliveryTimes);

			TestDec.JE_TransportMode = TestDec.TransportModeRoadCodeForTesting;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Road, TestDec.FreightModeForPortDeliveryTimes);

			TestDec.JE_TransportMode = TestDec.TransportModeRailCodeForTesting;
			AssertEquals(Enterprise.Core.Constants.TransportModes.Rail, TestDec.FreightModeForPortDeliveryTimes);
		}

		public void TestAutoRating_SingleContainer_Type_Commodity()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var autoRating = TestDec.RatingAdapter;
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;

			var container = SetupFCLContainerForRating(gp20.PK);
			container.CO_Weight = 2000m;

			CombineAssertions(() =>
			{
				var containerTareWeight = gp20.RC_TareWeight;
				AssertEquals("Freight Mode", FreightMode.FCL, autoRating.FreightMode);
				AssertGreaterThan("Tare Weight", containerTareWeight, 0);
				var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
				AssertEquals("Container Weight", 2000m + containerTareWeight, rateableMeasures.GetAllContainers().Single().ContainerWeightInKG);
				AssertEquals("Container Type PK", gp20.PK, rateableMeasures.GetContainerTypePKs().Single());
			});
		}

		public void TestGetPartList_NotNull()
		{
			var autoRating = TestDec.RatingAdapter;
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			AssertNotNull(rateableMeasures.GetPartList(MeasureType.Unit));
		}

		public void TestAutoRating_MultipleContainers_SingleTypeAndCommodity()
		{
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;

			SetupFCLContainerForRating(gp20.PK);
			SetupFCLContainerForRating(gp20.PK);

			var rateableMeasures = (RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures;
			var containerList = rateableMeasures.GetContainerTypeAndCommodityList().ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Container count", 2, containerList.Sum(x => x.ContainerCount));
				AssertEquals("1 type of containers", gp20.PK, containerList.Select(x => x.ContainerTypePk).Distinct().Single());
				AssertEquals("1 type of commodity", 1, containerList.Select(x => x.CommodityCode).Distinct().Count());
			});
		}

		public void TestAutoRating_MultipleContainersAndTypes_SingleCommodity()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;

			SetupFCLContainerForRating(gP20.PK);
			SetupFCLContainerForRating(gP20.PK);
			SetupFCLContainerForRating(gP40.PK);

			var rateableMeasures = (RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures;
			var containerList = rateableMeasures.GetContainerTypeAndCommodityList().ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Container count", 3, containerList.Sum(x => x.ContainerCount));
				AssertEquals("2 types of containers", 2, containerList.Select(x => x.ContainerTypePk).Distinct().Count());
				AssertEquals("1 type of commodity", 1, containerList.Select(x => x.CommodityCode).Distinct().Count());
			});
		}

		public void TestAutoRating_MultipleContainers_TypesAndCommodities()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gP40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;

			SetupFCLContainerForRating(gP20.PK);
			var container2 = SetupFCLContainerForRating(gP20.PK);
			var container3 = SetupFCLContainerForRating(gP40.PK);

			container2.JobContainer.JC_RH_NKContainerCommodityCode = "XYZ";
			container3.JobContainer.JC_RH_NKContainerCommodityCode = "ZAY";

			var rateableMeasures = (RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures;
			var containerList = rateableMeasures.GetContainerTypeAndCommodityList().ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Container count", 3, containerList.Sum(x => x.ContainerCount));
				AssertEquals("2 types of containers", 2, containerList.Select(x => x.ContainerTypePk).Distinct().Count());
				AssertEquals("3 types of commodity - blank, XYZ and ZAY", 3, containerList.Select(x => x.CommodityCode).Distinct().Count());
			});
		}

		public void TestGetWeightUniqueCommodities()
		{
			CombineAssertions(() =>
			{
				var autoRating = TestDec.RatingAdapter;
				var container1 = TestDec.CusContainers.AddNew();
				AssertEquals("No Container Commodity", ZString.Empty, ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());

				container1.JobContainer.JC_RH_NKContainerCommodityCode = "XYZ";
				AssertEquals("Single Container Commodity", "XYZ", ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());

				var container2 = TestDec.CusContainers.AddNew();
				container2.JobContainer.JC_RH_NKContainerCommodityCode = "ABC";
				AssertEquals("Multiple Container Commodities", ZString.Empty, ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());

				container2.JobContainer.JC_RH_NKContainerCommodityCode = "XYZ";
				AssertEquals("Single Commodity, Multiple Containers", "XYZ", ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());
			});
		}

		public void TestAutoRatingMeasures_Weight_Volume_Package_DeclarationLinkedShipment()
		{
			TestDec.JE_TransportMode = TestDec.TransportModeSeaCodeForTesting;
			TestDec.JE_TotalWeight = 100m;
			TestDec.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
			TestDec.JE_TotalVolume = 333m;
			TestDec.JE_TotalVolumeUnit = Core.Constants.Volume.MegaLitre;
			TestDec.JE_TotalNoOfPacks = 222;

			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = "HAZ";
			TestDec.JE_JS = shipment.PK;

			var rateableMeasures = (RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("Weight", 100m, rateableMeasures.GetActual(MeasureType.Weight));
				AssertEquals("Weight Unique Commodities", "HAZ", rateableMeasures.GetWeightUniqueCommodities().Single());
				AssertEquals("Volume", 333m, rateableMeasures.GetActual(MeasureType.Volume));
				AssertEquals("Volume Unique Commodities", "HAZ", rateableMeasures.GetVolumeUniqueCommodities().Single());
				AssertEquals("Packages", 222m, rateableMeasures.GetActual(MeasureType.Package));
				AssertEquals("Package Unique Commodities", "HAZ", rateableMeasures.GetPackageUniqueCommodities().Single());
			});
		}

		public void TestGetPackageUniqueCommodities_DeclarationLinkedShipment_UniqueContainerCommodity_WithMultipleOuterPackLineCommodities()
		{
			var container1 = TestDec.CusContainers.AddNew();
			container1.JobContainer.JC_RH_NKContainerCommodityCode = "XYZ";

			var shipment = Factory.New<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_RH_NKCommodityCode = "HAZ";
			var packLine2 = shipment.OuterPackLines.AddNew();
			TestDec.JE_JS = shipment.PK;

			var autoRating = TestDec.RatingAdapter;
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("Weight Unique Commodities", "XYZ", rateableMeasures.GetWeightUniqueCommodities().Single());
				AssertEquals("Volumne Unique Commodities", "XYZ", rateableMeasures.GetVolumeUniqueCommodities().Single());
				AssertEquals("Package Unique Commodities", "XYZ", rateableMeasures.GetPackageUniqueCommodities().Single());
				packLine2.JL_RH_NKCommodityCode = "GEN";
				AssertEquals("Different commodity codes", "XYZ", ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());
				packLine2.JL_RH_NKCommodityCode = "HAZ";
				AssertEquals("Identical commodity codes", "HAZ", ((RateableMeasureSet)autoRating.RateableMeasures).GetWeightUniqueCommodities().Single());
			});
		}

		public void TestMeasures_WeightVolumeUnitsAreEmpty_DontAddThemToMeasures()
		{
			TestDec.JE_TotalWeight = 100m;
			TestDec.JE_TotalWeightUnit = ZString.Empty;
			TestDec.JE_TotalVolume = 333m;
			TestDec.JE_TotalVolumeUnit = ZString.Empty;

			var rateableMeasures = (RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures;
			CombineAssertions(() =>
			{
				AssertEquals("Has weight measures", false, rateableMeasures.HasMeasureType(MeasureType.Weight));
				AssertEquals("Has volume measures", false, rateableMeasures.HasMeasureType(MeasureType.Volume));
			});
		}

		public void TestMeasures_ContainerCount_ShouldReturnContainersWithActualGrossWeight()
		{
			var gP20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var container1 = TestDec.CusContainers.AddNew();
			container1.JobContainer.JC_RC = gP20.PK;
			container1.JobContainer.JC_GrossWeightUQ = "KG";
			container1.JobContainer.JC_ContainerNum = "McLaren";
			container1.GoodsWeightForBinding = 300;
			container1.DunnageWeight = 100;
			container1.TareWeight = 2000;

			var autoRating = TestDec.RatingAdapter;
			var rateableMeasures = (RateableMeasureSet)autoRating.RateableMeasures;
			var containers = rateableMeasures
				.GetPartList(MeasureType.ContainerCount)
				.Cast<IRateableContainer>()
				.ToList();

			var actualContainers = containers
				.Select(c => new { c.Reference, c.ContainerGrossWeight })
				.ToList();

			actualContainers.Should().BeEquivalentTo(
				new[]
				{
					new { Reference = "MCLAREN", ContainerGrossWeight = new Quantity(2400m, "KG") }
				},
				because: "Should use the values specified on container rather than calculated from pack lines, i.e. 2000 + 300 + 100");

			Assert(true);
		}

		public void TestRateableMeasures_NoBills()
		{
			AssertEquals("No bills so Dec used", 1m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestRateableMeasures_BillNoType()
		{
			TestDec.Bills.AddNew();
			TestDec.Bills.AddNew();
			AssertEquals("Bill of no type so dec used", 1m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestRateableMeasures_MasterBills()
		{
			var masterBill1 = TestDec.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			var masterBill2 = TestDec.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("Master Bills ignored", 1m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestRateableMeasures_HouseBills()
		{
			var houseBill1 = TestDec.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			var houseBill2 = TestDec.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("Housebills counted", 2m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestRateableMeasures_SubHouseBills()
		{
			var subHouseBill1 = TestDec.Bills.AddNew();
			subHouseBill1.CU_BillType = BillTypeList.Codes.SubHouseBill;
			var subHouseBill2 = TestDec.Bills.AddNew();
			subHouseBill2.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertEquals("Sub-housebill counted", 2m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestRateableMeasures_ForwardingShipment()
		{
			var houseBill = TestDec.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			var subHouseBill = TestDec.Bills.AddNew();
			subHouseBill.CU_BillType = BillTypeList.Codes.SubHouseBill;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HAWB123";
			TestDec.JE_JS = shipment.PK;
			AssertEquals("Even if attached to shipment, house bills on a dec is autorated separately", 2m, ((RateableMeasureSet)TestDec.RatingAdapter.RateableMeasures).Shipments);
		}

		public void TestContainerAndFreightMode()
		{
			CombineAssertions(() =>
			{
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.FCL, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.FCL, ContainerModes.FCL, expectedContainerMode: ContainerModes.FCL, expectedFreightMode: FreightMode.FCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.FCL, ContainerModes.LCL, expectedContainerMode: ContainerModes.LCL, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.LCL, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.Liquid, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.NonContainerised, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);

				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.BreakBulk, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.BreakBulk, ContainerModes.FCL, expectedContainerMode: ContainerModes.FCL, expectedFreightMode: FreightMode.FCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.BreakBulk, ContainerModes.LCL, expectedContainerMode: ContainerModes.LCL, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.Bulk, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.RollOnRollOff, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Sea, ContainerModes.FCLMixedShipper, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: false, TransportModes.Air, string.Empty, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LSE);

				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.FCL, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.FCL, ContainerModes.FCL, expectedContainerMode: ContainerModes.FCL, expectedFreightMode: FreightMode.FCL);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.FCL, ContainerModes.LCL, expectedContainerMode: ContainerModes.LCL, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.LCL, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.Liquid, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.NonContainerised, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LCL);

				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.BreakBulk, string.Empty, expectedContainerMode: ContainerModes.BreakBulk, expectedFreightMode: FreightMode.BBK);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.BreakBulk, ContainerModes.FCL, expectedContainerMode: ContainerModes.BreakBulk, expectedFreightMode: FreightMode.BBK);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.BreakBulk, ContainerModes.LCL, expectedContainerMode: ContainerModes.BreakBulk, expectedFreightMode: FreightMode.BBK);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.Bulk, string.Empty, expectedContainerMode: ContainerModes.Bulk, expectedFreightMode: FreightMode.BLK);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.RollOnRollOff, string.Empty, expectedContainerMode: ContainerModes.RollOnRollOff, expectedFreightMode: FreightMode.ROR);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Sea, ContainerModes.FCLMixedShipper, string.Empty, expectedContainerMode: ContainerModes.FCLMixedShipper, expectedFreightMode: FreightMode.BCN);
				AssertContainerAndFreightMode(isRegistryEnabled: true, TransportModes.Air, string.Empty, string.Empty, expectedContainerMode: string.Empty, expectedFreightMode: FreightMode.LSE);
			});

			void AssertContainerAndFreightMode(bool isRegistryEnabled, string transportMode, string customContainerMode, string freightContainerMode, string expectedContainerMode, FreightMode expectedFreightMode)
			{
				var declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_TransportMode = transportMode;

				if (!string.IsNullOrEmpty(freightContainerMode))
				{
					var container = declaration.CusContainers.AddNew();
					container.CO_FCL_LCL_AIR = freightContainerMode;
				}

				// Set JE_ContainerMode after adding CusContainer to avoid being reset to 'CNT' (in BaseCusContainerCollection.OnAdded)
				declaration.JE_ContainerMode = customContainerMode;

				using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
				{
					var autoRating = declaration.RatingAdapter;
					AssertEquals
					(
						$"Registry:{isRegistryEnabled}, {transportMode}-{customContainerMode}-{freightContainerMode}",
						$"{expectedContainerMode}-{expectedFreightMode}",
						$"{autoRating.ContainerMode}-{autoRating.FreightMode}"
					);
				}
			}
		}

		public void TestIAutoRatingCustomsInfoMembers()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var autoRating = (IAutoRatingCustomsInfo)declaration.RatingAdapter;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "87654321";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "00102321";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "87654321";
			var invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "00102321";
			var invoiceLine5 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "00102006";

			AssertEquals(3, autoRating.TariffsPerShipment[0].InvoiceLines);

			AssertEquals(2, autoRating.TariffsPerInvoice.Count);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].InvoiceLines);

			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].EntryLines);

			var invoiceLine6 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "00102006";
			AssertEquals(2, autoRating.TariffsPerInvoice.Count);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].InvoiceLines);

			AssertEquals(2, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].EntryLines);

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "87654321";
			AssertEquals(2, autoRating.TariffsPerInvoice.Count);
			AssertEquals(2, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].InvoiceLines);

			AssertEquals(3, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].EntryLines);

			invoiceLine7.JI_Tariff = "87600001";
			AssertEquals(3, autoRating.TariffsPerInvoice[0].InvoiceLines);
			AssertEquals(3, autoRating.TariffsPerInvoice[1].InvoiceLines);
			AssertEquals(4, autoRating.TariffsPerShipment[0].InvoiceLines);

			AssertEquals(3, autoRating.TariffsPerInvoice[0].EntryLines);
			AssertEquals(4, autoRating.TariffsPerInvoice[1].EntryLines);
		}

		#endregion

		#region Client Contract Number

		public void TestUpdateClientContractNumber()
		{
			var declaration = TestDec;

			// blank + BBB = BBB
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + BBB = AAA
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "AAA",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + AAA = AAA
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "AAA",
				newNumbers: new[] { "AAA" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + empty => AAA
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "AAA",
				newNumbers: Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + blank => AAA
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "AAA",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + many = AAA
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "AAA",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// blank + BBB x 3 = BBB
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "",
				newNumbers: new[] { "BBB", "BBB", "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// blank + multiple = error
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "",
				newNumbers: Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				declaration,
				existingNumber: "",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");
		}

		static void AssertUpdateClientContractNumber(
			BaseJobDeclaration declaration,
			string existingNumber,
			IEnumerable<string> newNumbers,
			DataUpdateResult expectedResult,
			string expectedNumber)
		{
			var jobHeader = new JobHeader.Loader(declaration).TryLoadOrCreate();
			if (existingNumber != null)
			{
				jobHeader.JH_ClientContractNumber = existingNumber;
			}
			var jobDataUpdater = (IJobDataUpdater)declaration.RatingAdapter;

			ErrorReporter.Clear();
			var result = jobDataUpdater.UpdateClientContractNumber(newNumbers);

			var actualHasError = ErrorReporter.TotalErrorCount > 0;

			CombineAssertions(() =>
			{
				AssertEquals("Should update successfully", expectedResult, result);
				AssertEquals("Number", expectedNumber, jobHeader.JH_ClientContractNumber);
			});
		}

		public void TestGetContractNumberConfiguration()
		{
			var ratingAdapter = TestDec.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(configuration.ShouldAddContractNumberQueryFilter);
			Assert(!configuration.ShouldApplySpecificAdapterContractNumberFilter);
			Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			Assert(!configuration.ShouldMatchJobBlankContractNumber);
			Assert(!configuration.ShouldUseCarrierContractDateFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(configuration.ShouldAddContractNumberQueryFilter);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
			Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
			Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
			Assert(!configuration.ShouldMatchJobBlankContractNumber);
			Assert(!configuration.ShouldUseCarrierContractDateFilter);
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable

		public void TestConditionsSupporter()
		{
			var jobDeclaration = Factory.New<BaseJobDeclaration>();

			var adapter = new BaseJobDeclarationRatingAdapter<BaseJobDeclaration>(jobDeclaration);
			AssertType<DeclarationRateLineConditionsSupporter>(((IAutoRatingFreightConditionsSupportable)adapter).ConditionsSupporter);
		}

		#endregion

		#region Test Properties

		protected virtual string DefaultImportMessageType
		{
			get { return JobMessageTypeList.Codes.Import; }
		}

		protected virtual string DefaultExportMessageType
		{
			get { return JobMessageTypeList.Codes.Export; }
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			var dec = BaseJobDeclaration.New(Factory);
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		#region TestDec

		BaseJobDeclaration TestDec
		{
			get { return testDec ?? (testDec = GetJobDeclaration()); }
		}

		BaseJobDeclaration testDec;

		#endregion

		protected virtual ZString ImportMessageTypeForTest
		{
			get { return "IMP"; }
		}

		protected virtual ZString ExportMessageTypeForTest
		{
			get { return "EXP"; }
		}

		#endregion

		BaseCusContainer SetupFCLContainerForRating(ZGuid refContainerPK)
		{
			var container = TestDec.CusContainers.AddNew();
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			container.CO_RC = refContainerPK;
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			return container;
		}
	}
}
