using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.AWB;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	class NewConsolExportAWBHeaderTest : NewExportAWBHeaderTest<ConsolExportAWBHeader>
	{
		public void TestRateLineRateChargeOrDiscount_AlwaysReturnsConsolChargeable_WhenNotDirectConsol()
		{
			var nonDirectConsol = Factory.New<ForwardingConsol>();
			nonDirectConsol.JK_TransportMode = TransportModes.Air;
			nonDirectConsol.JK_AgentType = AgentType.CoLoad;
			nonDirectConsol.JK_PrepaidCollect = PaymentType.Collect;
			nonDirectConsol.JK_ConsolChargeableRate = 5m;

			var shipment = nonDirectConsol.Shipments.AddNew();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			CreateJobHeader(shipment.PK, client, debtor);

			CreateShipmentCharge(shipment, Env.Registry.FreightChargeCode, 300m, 0m);

			Factory.Save();

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.Both))
			{
				SetConsolAndAssertRateLine("Should use ConsolChargeableRate", nonDirectConsol, expected: 5m);
			}
		}

		public void TestRateLineRateChargeOrDiscount_RespectsMAWBBillingSellRate_WhenDirectConsol()
		{
			#region Setup

			var collectConsol = Factory.New<ForwardingConsol>();
			collectConsol.JK_TransportMode = TransportModes.Air;
			collectConsol.JK_AgentType = AgentType.Direct;
			collectConsol.JK_PrepaidCollect = PaymentType.Collect;
			collectConsol.JK_ConsolChargeableRate = 5m;

			var prepaidConsol = Factory.New<ForwardingConsol>();
			prepaidConsol.JK_TransportMode = TransportModes.Air;
			prepaidConsol.JK_AgentType = AgentType.Direct;
			prepaidConsol.JK_PrepaidCollect = PaymentType.Prepaid;
			prepaidConsol.JK_ConsolChargeableRate = 5m;

			var shipment1 = collectConsol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 1.0;
			shipment1.JS_ActualVolume = 1.8;

			var shipment2 = prepaidConsol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 1.0;
			shipment2.JS_ActualVolume = 1.8;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader1 = CreateJobHeader(shipment1.PK, client, debtor);
			var jobHeader2 = CreateJobHeader(shipment2.PK, client, debtor);

			CreateShipmentCharge(shipment1, Env.Registry.FreightChargeCode, 300, 0);
			CreateShipmentCharge(shipment2, Env.Registry.FreightChargeCode, 150, 0);

			Factory.Save();

			#endregion

			CombineAssertions("Pre-conditions", () =>
			{
				Assert(shipment1.IsDirectShipment);
				Assert(shipment2.IsDirectShipment);
				AssertEquals(shipment1.PK, collectConsol.DirectShipment.PK);
				AssertEquals(shipment2.PK, prepaidConsol.DirectShipment.PK);
				AssertEquals(300m, shipment1.JS_ActualChargeable);
				AssertEquals(300m, shipment2.JS_ActualChargeable);
			});

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.None))
			{
				AssertEquals(FreightDataRegistry.Instance.MAWBBillingSellRate.Value, "NON");
				SetConsolAndAssertRateLine("Should use ConsolChargeableRate", collectConsol, expected: 5m);
				SetConsolAndAssertRateLine("Should use ConsolChargeableRate", prepaidConsol, expected: 5m);
			}

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.CollectOnly))
			{
				SetConsolAndAssertRateLine("Should use Billing Sell Rate from Direct Shipment (300kg at $300/kg)", collectConsol, expected: 1m);
				SetConsolAndAssertRateLine("Should use ConsolChargeableRate", prepaidConsol, expected: 5m);
			}

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.PrepaidOnly))
			{
				SetConsolAndAssertRateLine("Should use ConsolChargeableRate", collectConsol, expected: 5m);
				SetConsolAndAssertRateLine("Should use Billing Sell Rate from Direct Shipment (300kg at $150/kg)", prepaidConsol, expected: 0.5m);
			}

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.Both))
			{
				SetConsolAndAssertRateLine("Should use Billing Sell Rate from Direct Shipment (300kg at $300/kg)", collectConsol, expected: 1m);
				SetConsolAndAssertRateLine("Should use Billing Sell Rate from Direct Shipment (300kg at $150/kg)", prepaidConsol, expected: 0.5m);
			}
		}

		public void TestRateLineRateChargeOrDiscount_CombinesCharges()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Direct;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_ConsolChargeableRate = 5m;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 1.0;
			shipment.JS_ActualVolume = 1.8;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			CreateJobHeader(shipment.PK, client, debtor);
			CreateShipmentCharge(shipment, Env.Registry.FreightChargeCode, 300m, 0m);
			CreateShipmentCharge(shipment, Env.Registry.FreightChargeCode, 150m, 0m);

			using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.Both))
			{
				SetConsolAndAssertRateLine("Should use combined Billing Sell Rate from Direct Shipment (300kg at $450/kg)", consol, expected: 1.5m);
			}
		}

		public void TestRateLineTotal_UsesLocalCurrency()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var debtor = Factory.NewWithValidTestData<OrgHeader>();
				var creditor = Factory.NewWithValidTestData<OrgHeader>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_AgentType = AgentType.Direct;
				consol.JK_PrepaidCollect = PaymentType.Collect;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualWeight = 1.0;
				shipment.JS_ActualVolume = 1.8;

				var jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeader.JH_ParentID = shipment.PK;
				jobHeader.JH_JobNum = "Phony number";
				jobHeader.JH_OA_AgentCollectAddr = debtor.MainAddress.PK;
				jobHeader.JH_OA_LocalChargesAddr = creditor.MainAddress.PK;
				jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

				CreateExchangeRate("USD", 2.5, "DEB", debtor.PK, jobHeader.PK);

				var charge = CreateShipmentCharge(shipment, Env.Registry.FreightChargeCode, 200, 445, sellAccount: debtor);
				charge.JR_RX_NKSellCurrency = "AUD";

				Factory.Save();

				using (FreightDataRegistry.Instance.MAWBBillingSellRate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MAWBBillingSellRateModes.Both))
				{
					var header = Factory.New<ConsolExportAWBHeaderForTest>();
					header.SetConsol(consol);
					header.Populate();

					AssertEquals("No currency change as AU company with AUD charge", 200m, header.RateLineTotal);

					charge.JR_RX_NKSellCurrency = "USD";
					charge.JR_OSSellAmt = 200m;
					charge.JR_LocalSellAmt = 80m;

					AssertEquals("Should use local currency as AU company with US charge", 80m, header.RateLineTotal);
				}
			}
		}

		public void TestShippersSignatureLengthForConsolExportAWBHeader()
		{
			var oldName = GlbStaff.CurrentUser.GS_FullName;

			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
				{
					var shipmentAWBHeader = Factory.New<ShipmentExportAWBHeader>();
					shipmentAWBHeader.EH_ParentID = AWBHeader.Consol.Shipments.FirstOrDefault().PK;

					var certificate = Factory.New<GenRegCertAccredMaintList>();
					certificate.XZ_Type = "DGN";
					certificate.XZ_Comment = "Test DGN Number";
					certificate.XZ_ParentID = GlbStaff.CurrentUser.PK;
					certificate.XZ_ParentTableCode = "GS";
					certificate.XZ_RefNumber = "AU1234567890";

					Factory.Save();

					GlbStaff.CurrentUser.GS_FullName = "Sally Gomez Louise Jones";

					AWBHeader.Populate();
					shipmentAWBHeader.Populate();

					AssertEquals("20 characters", 20, AWBHeader.ShippersSignatureMaxLength);
					AssertEquals("Signature length is 20 in ConsolExportAWBHeader.", "AU1234567890 S Jones", AWBHeader.EH_ShippersSignature);

					AssertEquals("35 characters", 35, shipmentAWBHeader.ShippersSignatureMaxLength);
					AssertEquals("Signature length is 35 in ExportAWBHeader.", "AU1234567890 Sally Jones", shipmentAWBHeader.EH_ShippersSignature);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
				{
					AWBHeader.Populate();
					AssertEquals("35 characters allowed outside AU", 35, AWBHeader.ShippersSignatureMaxLength);
					AssertEquals("Registration number is truncated outside AU", "Sally Gomez Louise Jones AU12345678", AWBHeader.EH_ShippersSignature);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_FullName = oldName;
			}
		}

		public void TestItalyCodes()
		{
			var accountingInformations = AWBHeader.AWBAccountingInformations.Cast<ExportAWBAccountingInformation>();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "IT";
			AWBHeader.Consol.JK_MasterBillNum = "08112345678";
			AWBHeader.Populate();

			Assert(AWBHeader.AWBAccountingInformations.Count == 0);

			// sending forwarder
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();

			var ivaCC = sendingForwarder.CustomsCodes.AddNew();
			ivaCC.OK_RN_NKCodeCountry = "IT";
			ivaCC.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			ivaCC.OK_CustomsRegNo = "10987654321";

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AWBHeader.Populate();

			Assert(AWBHeader.AWBAccountingInformations.Count == 2);

			var infoToDelete = accountingInformations.
				FirstOrDefault(info =>
							   info.IsItalianRegistrationCode &&
							   info.EA_InformationID == ExportAWBAccountingInformationLookups.IssuedByIVA &&
							   info.EA_Information == "10987654321");

			AssertNotNull(infoToDelete);

			AssertNotNull(accountingInformations.
							FirstOrDefault(info =>
										   info.IsItalianRegistrationCode &&
										   info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA &&
										   info.EA_Information == "10987654321"));

			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			OrgHeader consignor = Factory.New<OrgHeader>();
			AWBHeader.Consol.Shipments[0].ConsignorPK = consignor.PK;

			var consignorSIV = consignor.CustomsCodes.AddNew();
			consignorSIV.OK_RN_NKCodeCountry = "IT";
			consignorSIV.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			consignorSIV.OK_CustomsRegNo = "11223344551";

			AWBHeader.Populate();

			Assert(AWBHeader.AWBAccountingInformations.Count == 1);

			Assert(infoToDelete.IsDeleted);

			AssertNotNull(accountingInformations.
							FirstOrDefault(info =>
										   info.IsItalianRegistrationCode &&
										   info.EA_InformationID == ExportAWBAccountingInformationLookups.ShipperCodiceFiscaleOrIVA &&
										   info.EA_Information == "11223344551"));
		}

		public void TestThrowReplaceMacrosException()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			using (FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"{First}\" == \"1\").First()>"))
			{
				AssertExceptionThrown<ExportAWBHeaderReplaceMacrosException>("Exception throw", $@"MAWB cannot be generated.
Please correct macro format in Registry -> {FreightDataRegistry.Instance.MAWBHandlingInformationExtraText.HumanReadableRegistryPath()}.", () => consol.AWBHeader.Populate());
			}
		}

		#region OtherCharges

		protected override BooleanRegistryItem GroupOtherChargesByIataCodeRegistry
		{
			get { return ExportAWBRegistry.Instance.MAWBGroupOtherChargesByIATACode; }
		}

		public override void TestIncludeCharges()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			BusinessObject consolCost1 = CreateConsolCost(null, 1);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals("ignores charge with no AccChargeCode", 0, AWBHeader.AWBOtherCharges.Count);

			BusinessObject consolCost2 = CreateConsolCost(Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode), 5);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost2 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals("ignores charge that is FreightChargeCode", 0, AWBHeader.AWBOtherCharges.Count);

			BusinessObject consolCost3 = CreateConsolCost(Factory.New<AccChargeCode>(), 3);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost2, consolCost3 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals("ignores unmapped charge", 0, AWBHeader.AWBOtherCharges.Count);

			BusinessObject consolCost4 = CreateConsolCost(ChargeCodeAS, 10);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost2, consolCost3, consolCost4 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals("processes correct charge", 1, AWBHeader.AWBOtherCharges.Count);
			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals("ignores charge set to hide", 0, AWBHeader.AWBOtherCharges.Count);

			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Show);
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("processes correct charge", 1, AWBHeader.AWBOtherCharges.Count);
			AWBHeader.AWBOtherCharges.RemoveAndDeleteAll();

			consolCost4[JobConsolCostSchema.E6_LocalCostAmount] = 0m;

			AWBHeader.Populate();
			AssertEquals("ignores charge with 0 amount", 0, AWBHeader.AWBOtherCharges.Count);
		}

		public override void TestSplitCharges()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertEquals("prerequisite", 1, AWBHeader.Consol.Shipments.Count);

			ForwardingShipment shipment1 = AWBHeader.Consol.Shipments[0];
			CreateShipmentHeader(shipment1);

			ForwardingShipment shipment2 = AWBHeader.Consol.Shipments.AddNew();
			CreateShipmentHeader(shipment2);

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			BusinessObject consolCost1 = CreateConsolCost(ChargeCodeAS, 1000);
			JobCharge apportionedConsolCost1ToShipment1 = CreateShipmentCharge(shipment1, consolCost1, ChargeCodeAS, 500, 600);
			JobCharge apportionedConsolCost1ToShipment2 = CreateShipmentCharge(shipment2, consolCost1, ChargeCodeAS, 500, 400);
			JobCharge additionalShipment1Charge = CreateShipmentCharge(shipment1, null, ChargeCodeAC, 100, 95);

			AssertContainsExactElementsInAnyOrder(new[] { apportionedConsolCost1ToShipment1, additionalShipment1Charge },
				GetShipmentCharges(shipment1));

			AssertContainsExactElementsInAnyOrder(new[] { apportionedConsolCost1ToShipment2 },
				GetShipmentCharges(shipment2));

			AWBHeader.Populate();
			AssertEquals("due carrier", 1, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000"
			},
			GetChargesDescriptions());

			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Split;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertEquals("should not split because made no profit", 1, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000"
			},
			GetChargesDescriptions());

			apportionedConsolCost1ToShipment1.JR_LocalSellAmt = 800;
			apportionedConsolCost1ToShipment2.JR_LocalSellAmt = 550;
			AssertEquals("profit on shipment 1 apportioned charge", 200m, apportionedConsolCost1ToShipment1.JR_LocalSellAmt - apportionedConsolCost1ToShipment1.JR_LocalCostAmt);
			AssertEquals("profit on shipment 2 apportioned charge", 150m, apportionedConsolCost1ToShipment2.JR_LocalSellAmt - apportionedConsolCost1ToShipment2.JR_LocalCostAmt);

			AWBHeader.Populate();
			AssertEquals("should show agent profit in addition to consol cost", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AS|A|PPD|350"
			},
			GetChargesDescriptions());

			apportionedConsolCost1ToShipment1.JR_LocalSellAmt = 800;
			apportionedConsolCost1ToShipment2.JR_LocalSellAmt = 150;
			AssertEquals("profit on shipment 1 apportioned charge", 200m, apportionedConsolCost1ToShipment1.JR_LocalSellAmt - apportionedConsolCost1ToShipment1.JR_LocalCostAmt);
			AssertEquals("loss on shipment 2 apportioned charge", -250m, apportionedConsolCost1ToShipment2.JR_LocalSellAmt - apportionedConsolCost1ToShipment2.JR_LocalCostAmt);

			AWBHeader.Populate();
			AssertEquals("should show agent loss in addition to consol cost", 2, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AS|A|PPD|-50"
			},
			GetChargesDescriptions());
		}

		public override void TestGroupExcessiveCharges()
		{
			GroupOtherChargesByIataCodeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertEquals("prerequisite", 15, AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB);

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);

			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Agent;
			collection[Core.Constants.AWB.ChargeCodes.AS].Visibility = nameof(AWBDisplayOptionVisibility.Show);

			collection[Core.Constants.AWB.ChargeCodes.AC].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			collection[Core.Constants.AWB.ChargeCodes.AC].Visibility = nameof(AWBDisplayOptionVisibility.Show);

			ExportAWBRegistry.Instance.MAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertGroupExcessiveChargesNotEnoughtToGroup();

			ResetHeaderAndParent();

			AssertGroupExcessiveChargesExactlyMaxAllowed();

			ResetHeaderAndParent();

			AssertGroupExcessiveChargesTwoEquallySizedGroups();

			ResetHeaderAndParent();

			AssertGroupExcessiveChargesShouldGroupStartingFromTheLargestGroupFirst();
		}

		public override void TestMaxOtherChargesOnGroupExcessiveCharges()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;

			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.AC), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.AS), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.AT), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.AW), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.BF), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.BI), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.BM), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.BR), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CA), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CB), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CC), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CD), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CF), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CG), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CH), 10);
			CreateConsolCost(CreateChargeCode(Core.Constants.AWB.ChargeCodes.CI), 10);

			GroupOtherChargesByIataCodeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.Populate();
			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);

			CreateConsolCost(CreateChargeCode(string.Empty), 10);

			AWBHeader.Populate();
			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);
		}

		void AssertGroupExcessiveChargesNotEnoughtToGroup()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;

			for (int i = 0; i < 4; i++)
			{
				CreateConsolCost(ChargeCodeAS, 10);
			}

			AWBHeader.Populate();

			AssertEquals(4, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|A|COL|10", "AS|A|COL|10", "AS|A|COL|10", "AS|A|COL|10"
			},
			GetChargesDescriptions());
		}

		void AssertGroupExcessiveChargesExactlyMaxAllowed()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			for (int i = 0; i < 15; i++)
			{
				CreateConsolCost(ChargeCodeAC, 10);
			}

			AWBHeader.Populate();

			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10",
				"AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10"
			},
			GetChargesDescriptions());
		}

		void AssertGroupExcessiveChargesTwoEquallySizedGroups()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			for (int i = 0; i < 16; i++)
			{
				CreateConsolCost(ChargeCodeAS, 10);
				CreateConsolCost(ChargeCodeAC, 10);
			}

			AWBHeader.Populate();

			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10",
				"AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "AC|C|PPD|10", "MB|C|PPD|30", "MB|A|PPD|160"
			},
			GetChargesDescriptions());
		}

		void AssertGroupExcessiveChargesShouldGroupStartingFromTheLargestGroupFirst()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			CreateConsolCost(ChargeCodeAC, 10);

			for (int i = 0; i < 16; i++)
			{
				CreateConsolCost(ChargeCodeAS, 10);
			}

			AWBHeader.Populate();

			AssertEquals(15, AWBHeader.AWBOtherCharges.Count);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AC|C|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10",
				"AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "AS|A|PPD|10", "MB|A|PPD|30"
			},
			GetChargesDescriptions());
		}

		public override void TestMergeWithExistingOtherCharges()
		{
			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			ForwardingShipment shipment = AWBHeader.Consol.Shipments[0];
			CreateShipmentHeader(shipment);

			BusinessObject consolCost1 = CreateConsolCost(ChargeCodeAS, 1000);
			BusinessObject consolCost2 = CreateConsolCost(ChargeCodeAC, 550);
			JobCharge apportionedConsolCost1 = CreateShipmentCharge(shipment, consolCost1, ChargeCodeAS, 1000, 1000);
			JobCharge apportionedConsolCost2 = CreateShipmentCharge(shipment, consolCost2, ChargeCodeAC, 550, 550);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { apportionedConsolCost1, apportionedConsolCost2 }, GetShipmentCharges(shipment));
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost2 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|C|PPD|550"
			},
			GetChargesDescriptions());

			Factory.Save();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);

			AWBHeader.Populate();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|C|PPD|550"
			},
			GetChargesDescriptions());

			BusinessObject consolCost3 = CreateConsolCost(ChargeCodeAS, 350);
			JobCharge apportionedConsolCost3 = CreateShipmentCharge(shipment, consolCost3, ChargeCodeAS, 350, 350);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { apportionedConsolCost1, apportionedConsolCost2, apportionedConsolCost3 }, GetShipmentCharges(shipment));
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost2, consolCost3 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|C|PPD|550", "AS|C|PPD|350"
			},
			GetChargesDescriptions());

			AssertEquals(false, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "C" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 1000).HasChanges);

			AssertEquals(false, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == "AC" && charge.EO_EntitlementCode == "C" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 550).HasChanges);

			AssertEquals(false, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "C" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 350).HasChanges);

			Factory.Save();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);

			AWBHeader.Populate();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AC|C|PPD|550", "AS|C|PPD|350"
			},
			GetChargesDescriptions());

			apportionedConsolCost2.Delete();
			consolCost2.Delete();
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { apportionedConsolCost1, apportionedConsolCost3 }, GetShipmentCharges(shipment));
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { consolCost1, consolCost3 }, GetConsolCosts(AWBHeader.Consol));

			AWBHeader.Populate();
			AssertEquals(false, AWBHeader.AWBOtherCharges.HasChanges);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|1000", "AS|C|PPD|350"
			},
			GetChargesDescriptions());

			AssertEquals(false, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "C" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 1000).HasChanges);

			AssertEquals(false, AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.First(charge => charge.EO_ChargeCode == "AS" && charge.EO_EntitlementCode == "C" && charge.EO_PPDCLT == "PPD" && charge.EO_Amount == 350).HasChanges);
		}

		public void TestGenerateChargesDifferentCurriency()
		{
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			auCountry.RN_RX_NKAirWaybillCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Factory.Save();

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals("prerequisite", 1, AWBHeader.Consol.Shipments.Count);

			ForwardingShipment shipment1 = AWBHeader.Consol.Shipments[0];
			CreateShipmentHeader(shipment1);

			AssertNotNull("prerequisite", AWBHeader.Consol.Transports.DepartureTransport);

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();

			JobSailing sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			AWBHeader.Consol.Transports.DepartureTransport.JW_JX = sailing.PK;

			VoyageExRate rate = AWBHeader.Consol.Transports.DepartureTransport.Sailing.Voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			rate.E8_VoyageExchangeRate = 2m;

			BusinessObject consolCost1 = CreateConsolCost(ChargeCodeAS, 1000);
			JobCharge apportionedConsolCost1ToShipment1 = CreateShipmentCharge(shipment1, consolCost1, ChargeCodeAS, 1200, 1000);
			apportionedConsolCost1ToShipment1.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.Australia;
			apportionedConsolCost1ToShipment1.JR_OSSellAmt = 1200;
			apportionedConsolCost1ToShipment1.JR_OSCostAmt = 1000;

			AWBDisplayOptionCollection collection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.MAWB);
			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Carrier;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|2000"
			},
			GetChargesDescriptions());

			collection[Core.Constants.AWB.ChargeCodes.AS].Entitlement = Core.Constants.AWB.EntitlementCode.Split;
			ExportAWBRegistry.Instance.MAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AWBHeader.Populate();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AS|C|PPD|2000", "AS|A|PPD|400"
			},
			GetChargesDescriptions());
		}

		public void TestPopulateFreightChargesForTax()
		{
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var consolCost = CreateConsolCost(freightChargeCode, 50m);
			consolCost[JobConsolCostSchema.E6_IsTaxAmountOverridden] = true;
			consolCost["E6_OSGSTAmount_Calc"] = 100m;

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AWBHeader.Populate();

			AssertEquals(100m, AWBHeader.EH_TaxesPPD);
			AssertEquals(0m, AWBHeader.EH_TaxesCOL);

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			AWBHeader.Populate();

			AssertEquals(0m, AWBHeader.EH_TaxesPPD);
			AssertEquals(100m, AWBHeader.EH_TaxesCOL);

			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AWBHeader.Populate();
			AssertEquals(0m, AWBHeader.EH_TaxesPPD);
			AssertEquals(0m, AWBHeader.EH_TaxesCOL);
		}

		public void TestPopulateFreightChargesForTax_UseAWBCurrency()
		{
			var auCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
			auCountry.RN_RX_NKAirWaybillCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Factory.Save();

			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			var consolCost = CreateConsolCost(freightChargeCode, 50m);
			consolCost[JobConsolCostSchema.E6_IsTaxAmountOverridden] = true;
			consolCost["E6_OSGSTAmount_Calc"] = 100m;
			consolCost[JobConsolCostSchema.E6_RX_NKCurrency] = "AUD";

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AWBHeader.Consol.JK_RL_NKLoadPort = "AUSYD";
			AWBHeader.Consol.JK_RL_NKDischargePort = "NZAKL";

			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			var sailing = voyage.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			AWBHeader.Consol.Transports.DepartureTransport.JW_JX = sailing.PK;

			var rate = AWBHeader.Consol.Transports.DepartureTransport.Sailing.Voyage.ExRates.AddNew();
			rate.E8_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			rate.E8_VoyageExchangeRate = 2m;

			AWBHeader.Consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AWBHeader.Populate();

			AWBHeader.Populate();
			AssertEquals(200m, AWBHeader.EH_TaxesPPD);
		}

		IJobConsolCost[] GetConsolCosts(ForwardingConsol consol)
		{
			return Factory.Load<IJobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
		}

		BusinessObject CreateConsolCost(AccChargeCode chargeCode, ZDecimal amount)
		{
			BusinessObject consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = AWBHeader.Consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCode != null ? chargeCode.PK : ZGuid.Empty;
			consolCost[JobConsolCostSchema.E6_LocalCostAmount] = amount;
			consolCost[JobConsolCostSchema.E6_OSCostAmount] = amount;

			return consolCost;
		}

		#endregion

		protected override BusinessObject GetNewParent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			return consol;
		}

		#region Implementation

		void SetConsolAndAssertRateLine(string message, ForwardingConsol consol, ZDecimal expected)
		{
			var header = Factory.New<ConsolExportAWBHeaderForTest>();
			header.SetConsol(consol);
			header.Populate();

			var rateLine = header.AWBRateLines[ExportAWBRateLine.Schema.ER_LineCount, (ZByte)1];

			AssertEquals(message, expected, rateLine.ER_RateChargeOrDiscount);
		}

		JobHeader CreateJobHeader(ZGuid shipmentPK, OrgHeader client, OrgHeader debtor)
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipmentPK;
			job.JH_JobNum = "Phony number";
			job.JH_OA_AgentCollectAddr = debtor != null ? debtor.MainAddress.PK : ZGuid.Empty;
			job.JH_OA_LocalChargesAddr = client != null ? client.MainAddress.PK : ZGuid.Empty;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return job;
		}

		void CreateExchangeRate(string currencyCode, ZDecimal baseRate, string orgType, ZGuid orgHeaderPK, ZGuid jobHeaderPK)
		{
			var exRate = (BusinessObject)Factory.New<IExchangeRate>();
			exRate["JF_RX_NKRateCurrency"] = currencyCode;
			exRate["JF_BaseRate"] = baseRate;
			exRate["JF_OH_Org"] = orgHeaderPK;
			exRate["JF_OrgType"] = orgType;
			exRate["JF_JH"] = jobHeaderPK;
		}

		JobCharge CreateShipmentCharge(
			ForwardingShipment shipment,
			ZGuid chargeCode,
			ZDecimal sell,
			ZDecimal cost,
			OrgHeader sellAccount = null,
			AccTaxRate rate = null
		)
		{
			var loadedChargeCode = Factory.Load<AccChargeCode>(chargeCode);
			AssertNotNull(loadedChargeCode);

			return CreateShipmentCharge(shipment, null, loadedChargeCode, sell, cost, sellAccount, rate);
		}

		class JobHeaderWithFakeCurrencyConverter : JobHeader
		{
			public JobHeaderWithFakeCurrencyConverter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override CurrencyConverter CurrencyConverter
			{
				get { return CurrencyConverter.New(Factory, ZDateTime.Now, ExchangeRateType.Sell, 0); }
			}

			protected override bool IsChargesCollectionLoaded => throw new NotImplementedException();
		}

		#endregion
	}
}
