using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class PaymentBasisIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestConvertToJobPaymentBases_ChargeableReferenceExceedsChargeableDescriptionColumnMaxLength_TrimTheReference()
		{
			var sourceBases = new List<PaymentBasis>();
			var chargeable = new Quantity(10, QuantityUnit.KG, reference: new string('a', 2001));

			var flatBasis = new PaymentBasis(chargeable, RateInfo.CreateFLT(100m, "AUD"), AdapterType.Shipment, "SHP1");

			sourceBases.Add(flatBasis);
			CombineAssertions("WHEN set description to > 2000 characters THEN should not throw MaxLengthExceededException.", () =>
			{
				IEnumerable<JobPaymentBasis> jobPaymentBasis = null;

				AssertNoExceptionThrown(
					"should not throw Exception",
					() => jobPaymentBasis = sourceBases.ConvertToJobPaymentBases(true, () => Factory.NewWithValidTestData<JobPaymentBasis>()));
				AssertEquals(
					"Description should have '...'",
					new string('a', AutoJobPaymentBasis.Schema.PBS_ChargeableDescriptionMaxLength - 3) + "...",
					jobPaymentBasis.Single().PBS_ChargeableDescription);
			});
		}

		public void TestConsolCosts_HavePaymentBases()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;

			cost.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader(1).PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Helper.NewOrgHeader().PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 30;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = shipment.ConsigneeDocumentaryAddress.E2_OA_Address;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 30m,
					}
				};
				AutoCostAndAssert("", null, expectedCosts, consol, false);

				var consolCost = Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
				AssertNotNull("Should create a consol cost with no error when autorate.", consolCost);
				Assert("Should have payment bases", consolCost.PaymentBases.Count > 0);

				var paymentBases = ((IPaymentBasisViewCharge)consolCost).CostPaymentBasesView;

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"BAF|30|AUD|30|KG|1|KG||C00001000 Route 1|UNT"
					},
					paymentBases.Select(x => x.ToString()));
			}
		}

		public void TestApportionedCharges_HavePaymentBasesFromCost()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var costEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			costEntry40GP.TI_RX_NKCurrency = "AUD";
			costEntry40GP.RateLines.RemoveAndDeleteAll();
			costEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_ContainerNum = "CONT00001";
			var container40GP = consol.Containers.AddNew();
			container40GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container40GP.JC_ContainerNum = "CONT00002";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 5.2m;
			packline1_1.JL_ActualWeight = 15000m;
			packline1_1.JL_JC = container20GP.PK;

			var packline1_2 = shipment1.OuterPackLines.AddNew();
			packline1_2.JL_ActualVolume = 3.7m;
			packline1_2.JL_ActualWeight = 25000m;
			packline1_2.JL_JC = container40GP.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = TransportModes.Sea;
			shipment2.JS_PackingMode = ContainerModes.FCL;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_INCO = "CFR";
			shipment2.JS_OH_DeliveryAgent = consignee.PK;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;

			var packline2_1 = shipment2.OuterPackLines.AddNew();
			packline2_1.JL_ActualVolume = 7.8m;
			packline2_1.JL_ActualWeight = 6000m;
			packline2_1.JL_JC = container20GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			using (var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;
				job2.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1000m,
					},
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 2000m,
					},
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();
				var expectedPaymentBasisStrings1 = new[]
				{
					"FRT|1000|AUD|1|20GP|1000|CN|CONT00001|C00001000 Route 1|UNT"
				};
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings1, consolCosts[0].PaymentBases.Select(x => x.ToString()));
				AssertEquals(2, consolCosts[0].ApportionmentCharges.Count);
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings1, consolCosts[0].ApportionmentCharges[0].CostPaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings1, consolCosts[0].ApportionmentCharges[1].CostPaymentBases.Select(x => x.ToString()));
				AssertEquals(0, consolCosts[0].ApportionmentCharges[0].SellPaymentBases.Count);
				AssertEquals(0, consolCosts[0].ApportionmentCharges[1].SellPaymentBases.Count);

				var expectedPaymentBasisStrings2 = new[]
				{
					"FRT|2000|AUD|1|40GP|2000|CN|CONT00002|C00001000 Route 1|UNT"
				};
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings2, consolCosts[1].PaymentBases.Select(x => x.ToString()));
				AssertEquals(2, consolCosts[1].ApportionmentCharges.Count);
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings2, consolCosts[1].ApportionmentCharges[0].CostPaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasisStrings2, consolCosts[1].ApportionmentCharges[1].CostPaymentBases.Select(x => x.ToString()));
				AssertEquals(0, consolCosts[1].ApportionmentCharges[0].SellPaymentBases.Count);
				AssertEquals(0, consolCosts[1].ApportionmentCharges[1].SellPaymentBases.Count);
			}
		}

		public void TestPaymentBasis_CostAndSell()
		{
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;

			var cnr = Helper.NewOrgHeader(1);
			var cne = Helper.NewOrgHeader();
			cnr.OH_IsDebtor = true;
			Factory.Save();

			var ct = Helper.NewCompanyTariff();
			var ctEntry = ct.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			ctEntry.RateLines.RemoveAndDeleteAll();
			ctEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 5;
			ctEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 4;
			ctEntry.AddRateLine("FRT", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("BAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;
			costEntry.AddRateLine("CAF", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = .5;

			ct.Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = cnr.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = cne.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate())
			{
				job.JH_OA_LocalChargesAddr = cnr.MainAddress.PK;
				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						CostCalculationDescription = "BAF: 1500 Kilogram(s) @ AUD 1.00/KG",
						ChargeCode = "BAF",
						E6_OSCostAmount = 1500,
					},
					new AssertionCost
					{
						CostCalculationDescription = "CAF: 1500 Kilogram(s) @ AUD 0.50/KG",
						ChargeCode = "CAF",
						E6_OSCostAmount = 750,
					}
				};

				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment, new[]
						{
							new AssertionCharge
							{
								ChargeCode = "BAF",
								JR_OSSellAmt = 7500,
								JR_OSCostAmt = 1500
							},
							new AssertionCharge
							{
								ChargeCode = "CAF",
								JR_OSSellAmt = 6000,
								JR_OSCostAmt = 750
							},
							new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 100m
							}
						}
					}
				};

				AutoCostAndAssert("", expectedCharges, expectedCosts, consol);

				var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToList();
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"BAF|1500|AUD|1500|KG|1|KG||C00001000 Route 1|UNT"
					},
					consolCosts[0].PaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"CAF|750|AUD|1500|KG|0.5|KG||C00001000 Route 1|UNT"
					},
					consolCosts[1].PaymentBases.Select(x => x.ToString()));

				var shipmentCharges = (job as Job).Charges.Cast<BaseCharge>().ToList();

				AssertEquals(3, shipmentCharges.Count);

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"BAF|7500|AUD|1500|KG|5|KG||S00001000|UNT",
						"CAF|6000|AUD|1500|KG|4|KG||S00001000|UNT",
						"FRT|100|AUD|||100|||S00001000|FLT",
					},
					shipmentCharges.SelectMany(x => x.SellPaymentBases.Select(y => y.ToString())));

				AssertEquals(1m, shipmentCharges.First(x => x.ChargeCode.AC_Code == "FRT").SellPaymentBases.First(x => x.PBS_FlatRate == 100m).PBS_ChargeableAmount);

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"BAF|1500|AUD|1500|KG|1|KG||C00001000 Route 1|UNT",
						"CAF|750|AUD|1500|KG|0.5|KG||C00001000 Route 1|UNT",
					},
					shipmentCharges.SelectMany(x => x.CostPaymentBases.Select(y => y.ToString())));
			}
		}

		public void TestPaymentBasis_CostAndSellBasisDescription()
		{
			var chargeCode = Helper.ChargeCodes["DDOC"];
			var refContainerPK = Helper.Containers["40GP"].PK;
			var origin = "CNSHA";
			var destination = "AUSYD";

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, origin, destination);
			costEntry.TI_RC = refContainerPK;

			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 16m;

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var tariffEntry = tariff.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, origin, destination);
			tariffEntry.TI_RC = refContainerPK;

			var tariffLine1 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 5m;
			tariffLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().BaseRate = 3.3m;

			var tariffLine2 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine2.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			var tariffLine3 = tariffEntry.AddRateLine(chargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			tariffLine3.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0m;

			Factory.Save();

			var consignee = Helper.NewOrgHeader(1);
			var shipment = CreateForwardingShipment(TransportModes.Sea, ZGuid.Empty, consignee.PK, origin, destination, 85m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();

			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, TransportProvider1, shipment);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = refContainerPK;
			container1.JC_ContainerNum = "CN1";
			container1.AddPackLine(packLine1);

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainerPK;
			container2.JC_ContainerNum = "CN2";
			container2.AddPackLine(packLine2);

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = refContainerPK;
			container3.JC_ContainerCount = 5;
			container3.AddPackLine(packLine3);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalCostAmt = 112m,
					CostCalculationDescription = "DDOC: 1 40GP Container(s) @ AUD 16.00/Container + 1 40GP Container(s) @ AUD 16.00/Container + 5 40GP Container(s) @ AUD 16.00/Container",
					JR_LocalSellAmt = 344.90m,
					RevenueCalculationDescription =
@"DDOC: 1 40GP Container(s) @ AUD 16.00/Container + 1 40GP Container(s) @ AUD 16.00/Container + 5 40GP Container(s) @ AUD 16.00/Container
DDOC: 1 40GP Container(s) @ AUD 16.00/Container + 1 40GP Container(s) @ AUD 16.00/Container + 5 40GP Container(s) @ AUD 16.00/Container
DDOC: Base Rate AUD 3.30 + 1 40GP Container(s) @ AUD 16.80/Container + 1 40GP Container(s) @ AUD 16.80/Container + 5 40GP Container(s) @ AUD 16.80/Container",
				}
			};

			AutorateAndAssert("Rating should succeed", expected, shipment, consignee);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<IPaymentBasisViewCharge>().ToList();

			AssertEquals(1, shipmentCharges.Count);
			var actualSellBases = shipmentCharges[0].SellPaymentBasesView.Select(x => x.ToString()).ToArray();
			var actualCostBases = shipmentCharges[0].CostPaymentBasesView.Select(x => x.ToString()).ToArray();

			var expectedSellBases = new[]
			{
				"DDOC|16|AUD|1|40GP|16|CN|CN1|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16|AUD|1|40GP|16|CN|CN2|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|80|AUD|5|40GP|16|CN||EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16|AUD|1|40GP|16|CN|CN1|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16|AUD|1|40GP|16|CN|CN2|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|80|AUD|5|40GP|16|CN||EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16.8|AUD|1|40GP|16.8|CN|CN1|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16.8|AUD|1|40GP|16.8|CN|CN2|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|84|AUD|5|40GP|16.8|CN||EBM22Q33TU475BXH3P60|UNT",
				"DDOC|3.3|AUD|||3.3|||EBM22Q33TU475BXH3P60|FLT",
			};

			var expectedCostBases = new[]
			{
				"DDOC|16|AUD|1|40GP|16|CN|CN1|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|16|AUD|1|40GP|16|CN|CN2|EBM22Q33TU475BXH3P60|UNT",
				"DDOC|80|AUD|5|40GP|16|CN||EBM22Q33TU475BXH3P60|UNT",
			};

			CombineAssertions("", () =>
			{
				AssertContainsExactElementsInAnyOrder("sell bases", expectedSellBases, actualSellBases);
				AssertContainsExactElementsInAnyOrder("cost bases", expectedCostBases, actualCostBases);
			});
		}

		public void TestPaymentBasis_OldBasesUpdatedWithNew()
		{
			var client = Helper.NewOrgHeader();
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			consignor.OH_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "KG");
			((UnitCalculator)rateLine1.Calculator).PerUnit = 7m;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7000m,
					JR_OSCostAmt = 1000m,
				}
			};

			AutorateAndAssert(expected, shipment, client);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var shipmentCharges = job.Charges.Cast<BaseCharge>().ToList();

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"FRT|7000|AUD|1000|KG|7|KG||S00001000|UNT"
					},
					shipmentCharges[0].SellPaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"FRT|1000|AUD|1000|KG|1|KG||S00001000|UNT"
					},
					shipmentCharges[0].CostPaymentBases.Select(x => x.ToString()));

				((UnitCalculator)rateLine1.Calculator).PerUnit = 8m;
				costEntry.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 2m;
				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 7000m,
						JR_OSCostAmt = 2000m,
					}
				};

				AutorateAndAssert(expected, shipment, client, job: job, autorateRevenue: false);

				shipmentCharges = job.Charges.Cast<BaseCharge>().ToList();

				AssertContainsExactElementsInAnyOrder(
					"Sell payment basis should retain",
					new[]
					{
						"FRT|7000|AUD|1000|KG|7|KG||S00001000|UNT"
					},
					shipmentCharges[0].SellPaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(
					"Cost payment basis should change",
					new[]
					{
						"FRT|2000|AUD|1000|KG|2|KG||S00001000|UNT"
					},
					shipmentCharges[0].CostPaymentBases.Select(x => x.ToString()));

				expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 8000m,
						JR_OSCostAmt = 2000m,
					}
				};

				AutorateAndAssert(expected, shipment, client, job: job, autorateCosts: false);

				shipmentCharges = job.Charges.Cast<BaseCharge>().ToList();

				AssertContainsExactElementsInAnyOrder(
					"Sell payment basis should change",
					new[]
					{
						"FRT|8000|AUD|1000|KG|8|KG||S00001000|UNT"
					},
					shipmentCharges[0].SellPaymentBases.Select(x => x.ToString()));
				AssertContainsExactElementsInAnyOrder(
					"Cost payment basis should retain",
					new[]
					{
						"FRT|2000|AUD|1000|KG|2|KG||S00001000|UNT"
					},
					shipmentCharges[0].CostPaymentBases.Select(x => x.ToString()));
			}
		}

		public void TestMinimumOrPerUnitCalculation()
		{
			var transportCompany = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(transportCompany);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AUSYD");

			var costLine1 = costEntry.AddRateLine("DDOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.KG);
			var calc1 = costLine1.GetCalculator<MinimumOrPerUnitCalculator>();
			calc1.Minimum = 25m;
			calc1.PerUnit = 1m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "DESTR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "DESTR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 5m;
			shipment.JS_ActualVolume = 0.029m;

			shipment.JS_OH_ImportBroker = transportCompany.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 25m,
					ChargeCode = "DDOC"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<IPaymentBasisViewCharge>().ToList();

			AssertEquals(1, shipmentCharges.Count);

			var costPaymentBasesForDisplay = shipmentCharges[0].CostPaymentBasesView;
			var actual = costPaymentBasesForDisplay
				.Select(i => $"{i.PBS_AdapterID}|{i.PBS_ChargeableAmount}|{i.PBS_ChargeableUnit}|{i.PBS_RateUnit}|{i.PBS_PerUnitRate}|{i.PBS_MinRate}|{i.PBS_RateReference}")
				.ToArray();

			var expectedResult = new[]
			{
				"S00001000|5|KG|KG|1.0000|25.0000|MIN"
			};

			AssertContainsExactElementsInAnyOrder("The payment bases for the cost line must match the expected result.", expectedResult, actual);
		}

		public void TestCombinedCalculatorWithMinimum_IncludesMinimumInDescription()
		{
			var transportCompany = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(transportCompany);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AUSYD");

			var costLine = costEntry.AddRateLine("DDOC", CombinedCalculator.Code, Weight.Kilograms);
			var costLineCalc = costLine.GetCalculator<CombinedCalculator>();
			costLineCalc["-5"] = (ZDecimal)70m;
			costLineCalc["+5"] = (ZDecimal)80m;
			costLineCalc["+15"] = (ZDecimal)90m;
			costLineCalc["+45"] = (ZDecimal)90m;
			costLineCalc.Minimum = 20m;
			costLineCalc.Maximum = 500m;
			costLineCalc.BaseRate = 5m;
			costLine.RateLineItems[0].TM_BreakWeightVolume = QuantityUnit.KG;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "DESTR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "DESTR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 5m;
			shipment.JS_ActualVolume = 0.029m;

			shipment.JS_OH_ImportBroker = transportCompany.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 405m,
					ChargeCode = "DDOC",
					CostCalculationDescription = "DDOC: Lesser of (Max Rate AUD 500.00, Greater of (Min Rate AUD 20.00, Base Rate AUD 5.00 + 5 Kilogram(s) @ AUD 80.00/KG))"
				}
			};

			using (RatingDataRegistry.Instance.EnableLongChargeCalculationDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssert(expected, shipment, client);
			}
		}

		public void TestFlatOrPerUnitCalculation()
		{
			var transportCompany = Helper.NewOrgHeader();
			var client = Helper.NewOrgHeader();

			var costing = Helper.NewCosting(transportCompany);
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AUSYD");

			var costLine1 = costEntry.AddRateLine("DDOC", FlatPlusPerUnitCalculator.Code, QuantityUnit.KG);
			var calc1 = costLine1.GetCalculator<FlatPlusPerUnitCalculator>();
			calc1.BaseRate = 25m;
			calc1.PerUnit = 1m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "DESTR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "DESTR";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_ActualWeight = 5m;
			shipment.JS_ActualVolume = 0.029m;

			shipment.JS_OH_ImportBroker = transportCompany.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 30m,
					ChargeCode = "DDOC"
				}
			};

			AutorateAndAssert(expected, shipment, client);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			AssertEquals(1, shipmentCharges.Count);
			var shipmentCharge = shipmentCharges[0];
			var costPaymentBases = shipmentCharge.CostPaymentBases.ToArray();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"DDOC|5|AUD|5|KG|1|KG||S00001000|UNT",
					"DDOC|25|AUD|||25|||S00001000|FLT",
				},
				costPaymentBases.Select(x => x.ToString()));

			AssertEquals(1m, costPaymentBases[0].PBS_PerUnitRate);
			AssertEquals("KG", costPaymentBases[0].PBS_RateUnit);
			AssertEquals(5m, costPaymentBases[0].PBS_ChargeableAmount);

			AssertEquals(25m, costPaymentBases[1].PBS_FlatRate);
			AssertEquals("1", costPaymentBases[1].PBS_RateUnit);
			AssertEquals(1m, costPaymentBases[1].PBS_ChargeableAmount);

			var paymentBasesForDisplay = ((IPaymentBasisViewCharge)shipmentCharge).CostPaymentBasesView;
			AssertEquals("DDOC|5|AUD|5|KG|1|KG||S00001000|UNT", paymentBasesForDisplay[0].ToString());
			AssertEquals("DDOC|25|AUD|||25|||S00001000|FLT", paymentBasesForDisplay[1].ToString());
		}

		public void TestPercentageCalculation()
		{
			var client = Helper.NewOrgHeader();
			client.OH_IsDebtor = true;

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var bafChargeCode = Helper.ChargeCodes["BAF"];

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode.AC_Code, FlatCalculator.Code);
			frtRateLine.GetCalculator<FlatCalculator>().BaseRate = 1000m;

			var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
			bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
			bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10m;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_UniqueConsignRef = "EBM22Q33TU475BXH3P60";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 1000m,
				},
				new AssertionCharge
				{
					ChargeCode = bafChargeCode.AC_Code,
					JR_OSSellAmt = 100m,
					RevenueCalculationDescription = "BAF: 10.00% of (AUD 1000.00 (FRT))"
				}
			};

			AutorateAndAssert(expected, shipment, client, autorateCosts: false);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			AssertEquals(2, shipmentCharges.Count);
			var frtPaymentBases = shipmentCharges.First(x => x.ChargeCode.PK == frtChargeCode.PK).SellPaymentBases.ToArray();
			AssertEquals(1, frtPaymentBases.Length);
			AssertEquals(1000m, frtPaymentBases[0].PBS_FlatRate);
			AssertEquals("1", frtPaymentBases[0].PBS_RateUnit);
			AssertEquals(1m, frtPaymentBases[0].PBS_ChargeableAmount);

			var bafPaymentBases = shipmentCharges.First(x => x.ChargeCode.PK == bafChargeCode.PK).SellPaymentBases.ToArray();
			AssertEquals(1, bafPaymentBases.Length);
			AssertEquals(10m, bafPaymentBases[0].PBS_PerUnitRate);
			AssertEquals("100", bafPaymentBases[0].PBS_RateUnit);
			AssertEquals(1000m, bafPaymentBases[0].PBS_ChargeableAmount);
			AssertEquals("AUD", bafPaymentBases[0].PBS_ChargeableUnit);

			var paymentBasesForDisplayFRT = ((IPaymentBasisViewCharge)shipmentCharges.First(x => x.ChargeCode.PK == frtChargeCode.PK)).SellPaymentBasesView.Select(x => x.ToString()).ToArray();
			var paymentBasesForDisplayBAF = ((IPaymentBasisViewCharge)shipmentCharges.First(x => x.ChargeCode.PK == bafChargeCode.PK)).SellPaymentBasesView.Select(x => x.ToString()).ToArray();

			var expectedFRTBases = new[]
			{
				"FRT|1000|AUD|||1000|||EBM22Q33TU475BXH3P60|FLT",
			};

			var expectedBAFBases = new[]
			{
				"BAF|100|AUD|1000|AUD|10|%||EBM22Q33TU475BXH3P60|UNT",
			};

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("frt bases", expectedFRTBases, paymentBasesForDisplayFRT);
				AssertContainsExactElementsInAnyOrder("baf bases", expectedBAFBases, paymentBasesForDisplayBAF);
			});
		}

		public void TestPercentageCalculation_OnLargeChargeableAmount()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var client = Helper.NewOrgHeader();
				client.OH_IsDebtor = true;

				var frtChargeCode = Helper.ChargeCodes["FRT"];
				var bafChargeCode = Helper.ChargeCodes["BAF"];

				var clientRate = Helper.NewClientRate(client);
				var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
				clientRateEntry.RateLines.RemoveAndDeleteAll();

				var frtRateLine = clientRateEntry.AddRateLine(frtChargeCode.AC_Code, UnitCalculator.Code, Weight.Grams);
				frtRateLine.GetCalculator<UnitCalculator>().PerUnit = 6666.66m;

				var bafRateLine = clientRateEntry.AddRateLine(bafChargeCode.AC_Code, PercentageCalculator.Code);
				bafRateLine.GetCalculator<PercentageCalculator>().AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = frtChargeCode.PK;
				bafRateLine.GetCalculator<PercentageCalculator>().Percent = 10.1m;

				var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, client.PK, "CNSHA", "AUSYD", 654321, 10);
				Factory.Save();

				var expected = new[]
				{
				new AssertionCharge
				{
					ChargeCode = frtChargeCode.AC_Code,
					JR_OSSellAmt = 4362135637860m,
					RevenueCalculationDescription = "FRT: 654321000 Gram(s) @ AUD 6666.66/G"
				},
				new AssertionCharge
				{
					ChargeCode = bafChargeCode.AC_Code,
					JR_OSSellAmt = 440575699423.86m,
					RevenueCalculationDescription = "BAF: 10.10% of (AUD 4362135637860.00 (FRT))"
				}
			};

				AutorateAndAssert(expected, shipment, client, autorateCosts: false);
				AssertNoExceptionThrown("Don't care about the exact number rounding, only that big number can be saved", () => Factory.Save());

				var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();
				var frtPaymentBasis = shipmentCharges.Single(x => x.ChargeCode.PK == frtChargeCode.PK).SellPaymentBases.Select(x => x.ToString()).ToArray();
				var bafPaymentBasis = shipmentCharges.Single(x => x.ChargeCode.PK == bafChargeCode.PK).SellPaymentBases.Select(x => x.ToString()).ToArray();

				var expectedFRTBases = new[]
				{
				"FRT|4362135637860|AUD|654321000|G|6666.66|G||EBM22Q33TU475BXH3P60|UNT"
			};

				var expectedBAFBases = new[]
				{
				"BAF|440575699423.86|AUD|4362135637860|AUD|10.1|%||EBM22Q33TU475BXH3P60|UNT"
			};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("frt bases", expectedFRTBases, frtPaymentBasis);
					AssertContainsExactElementsInAnyOrder("baf bases", expectedBAFBases, bafPaymentBasis);
				});
			}
		}

		public void TestRateShipmentPackLinesByPackType()
		{
			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var line1 = rateEntry.AddRateLine("BAF", UnitCalculator.Code, PkgUnit.Box);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5m;
			line1.UseOnlyActualWeightMeasure = true;

			var line2 = rateEntry.AddRateLine("WAR", CombinedCalculator.Code, PkgUnit.Carton);
			var calculator = line2.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10m, 20m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 10m, 30m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 20m, 40m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 30m, 50m);

			var line3 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, PkgUnit.Pallet);
			line3.UseOnlyActualWeightMeasure = true;
			calculator = line3.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 15m, 50m, QuantityUnit.KG);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 15m, 70m);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 45m, 80m);

			var line4 = rateEntry.AddRateLine("CAF", UnitCalculator.Code, QuantityUnit.PK);
			line4.GetCalculator<UnitCalculator>().PerUnit = 10;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 20m;
			packLine1.JL_ActualWeightUQ = Weight.Kilograms;
			packLine1.JL_ActualVolume = 0.40m;
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = PkgUnit.Box;
			packLine1.JL_RH_NKCommodityCode = "GEN";
			packLine1.JL_RefNumber = "REF1";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 60m;
			packLine2.JL_ActualWeightUQ = Weight.Kilograms;
			packLine2.JL_ActualVolume = 3m;
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = PkgUnit.Carton;
			packLine2.JL_RH_NKCommodityCode = "GEN";
			packLine2.JL_RefNumber = "REF2";

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 50m;
			packLine3.JL_ActualWeightUQ = Weight.Kilograms;
			packLine3.JL_ActualVolume = 0.30m;
			packLine3.JL_PackageCount = 5;
			packLine3.JL_F3_NKPackType = PkgUnit.Pallet;
			packLine3.JL_RH_NKCommodityCode = "GEN";
			packLine3.JL_RefNumber = "REF3";

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_ActualWeight = 50m;
			packLine4.JL_ActualWeightUQ = Weight.Kilograms;
			packLine4.JL_ActualVolume = 0.30m;
			packLine4.JL_PackageCount = 5;
			packLine4.JL_F3_NKPackType = PkgUnit.Pallet;
			packLine4.JL_RH_NKCommodityCode = "GEN";
			packLine4.JL_RefNumber = "REF4";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.PackLines.Add(packLine1);
			container.PackLines.Add(packLine2);
			container.PackLines.Add(packLine3);
			container.JC_ContainerNum = "CONT1";

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = refContainer.PK;
			container2.JC_ContainerNum = "CONT2";
			container2.PackLines.Add(packLine4);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 50.00m,
				},

				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 160.00m,
				},

				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 500.00m,
				},

				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 280.00m,
				},
			};

			AutorateAndAssert(expected, shipment, NewClient);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();
			AssertEquals(4, shipmentCharges.Count);

			CombineAssertions(() =>
			{
				var paymentBasesForDisplay1 = ((IPaymentBasisViewCharge)shipmentCharges[0]).SellPaymentBasesView;
				AssertEquals("BAF|50|USD|10|BOX|5|BOX|CONT1/REF1|EBM22Q33TU475BXH3P60|UNT", paymentBasesForDisplay1[0].ToString());

				var paymentBasesForDisplay2 = ((IPaymentBasisViewCharge)shipmentCharges[1]).SellPaymentBasesView;
				AssertEquals("CAF|280|USD|28|PK|10|PK|CONT1/REF1,CONT1/REF2,CONT1/REF3,CONT2/REF4|EBM22Q33TU475BXH3P60|UNT", paymentBasesForDisplay2[0].ToString());

				var paymentBasesForDisplay3 = ((IPaymentBasisViewCharge)shipmentCharges[2]).SellPaymentBasesView;
				var actualPaymentBasesForDisplay3 = paymentBasesForDisplay3.Select(p => p.ToString()).ToArray();
				var expectedPaymentBasesForDisplay3 = new[]
				{
					"FRT|250|USD|5|PLT|50|PLT|CONT1/REF3|EBM22Q33TU475BXH3P60|UNT",
					"FRT|250|USD|5|PLT|50|PLT|CONT2/REF4|EBM22Q33TU475BXH3P60|UNT"
				};
				AssertContainsExactElementsInAnyOrder(expectedPaymentBasesForDisplay3, actualPaymentBasesForDisplay3);

				var paymentBasesForDisplay4 = ((IPaymentBasisViewCharge)shipmentCharges[3]).SellPaymentBasesView;
				AssertEquals("WAR|160|USD|8|CTN|20|CTN|CONT1/REF2|EBM22Q33TU475BXH3P60|UNT", paymentBasesForDisplay4[0].ToString());
			});
		}

		public void TestBreaksApplyPerIndividualPackLine()
		{
			var rate = Helper.NewClientRate(NewClient);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "USLAX", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(rateEntry.Currency);

			var rateLine = rateEntry.AddRateLine("BAF", CombinedCalculator.Code, PkgUnit.Bottle);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();
			calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 7m, 2m, QuantityUnit.PK);
			calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 7m, 1.5m);

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = PkgUnit.Bottle;
			packLine1.JL_RH_NKCommodityCode = "GEN";
			packLine1.JL_RefNumber = "REF1";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;
			packLine2.JL_F3_NKPackType = PkgUnit.Bottle;
			packLine2.JL_RH_NKCommodityCode = "GEN";
			packLine2.JL_RefNumber = "REF2";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;
			container.PackLines.Add(packLine1);
			container.PackLines.Add(packLine2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 25.00m,
					RevenueCalculationDescription = "BAF: 5 Bottle(s) @ USD 2.00/Bottle + 10 Bottle(s) @ USD 1.50/Bottle"
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			var paymentBasesForDisplay = ((IPaymentBasisViewCharge)shipmentCharges[0]).SellPaymentBasesView;

			var actual = paymentBasesForDisplay.Select(b => b.ToString()).ToArray();

			var expectedResult = new[]
			{
				"BAF|15|USD|10|BOT|1.5|BOT|REF1|EBM22Q33TU475BXH3P60|UNT",
				"BAF|10|USD|5|BOT|2|BOT|REF2|EBM22Q33TU475BXH3P60|UNT"
			};

			AssertContainsExactElementsInAnyOrder(expectedResult, actual);
		}

		public void TestPaymentBasisMergingChargesWithMaximumApplicableToBoth()
		{
			var org = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(org);

			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.PK);
			rateLine1.GetCalculator<CombinedCalculator>().Maximum = 700m;
			rateLine1.GetCalculator<CombinedCalculator>().PerUnit = 6m;

			var rateLine2 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.PK);
			rateLine2.GetCalculator<CombinedCalculator>().Maximum = 700m;
			rateLine2.GetCalculator<CombinedCalculator>().PerUnit = 8m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();

			line1.JL_RH_NKCommodityCode = "GEN";
			line2.JL_RH_NKCommodityCode = "HAZ";

			line1.JL_PackageCount = 100;
			line2.JL_PackageCount = 100;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1300m,
					ChargeCode = "FRT",
				}
			};

			AutorateAndAssert(expected, shipment, org);
			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			var paymentBasesForDisplay = ((IPaymentBasisViewCharge)shipmentCharges[0]).SellPaymentBasesView;
			AssertEquals("FRT|700|AUD|||700|||S00001000|MAX", paymentBasesForDisplay[0].ToString());
			AssertEquals("FRT|600|AUD|100|PK|6|PK||S00001000|UNT", paymentBasesForDisplay[1].ToString());
		}

		public void TestPaymentBasisMergingChargesWithMinimumApplicableToBoth()
		{
			var org = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(org);

			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.PK);
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 700m;
			rateLine1.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 6m;

			var rateLine2 = rateEntry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.PK);
			rateLine2.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 700m;
			rateLine2.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 8m;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_INCO = IncoTerms.CostAndFreight;

			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();

			line1.JL_RH_NKCommodityCode = "GEN";
			line2.JL_RH_NKCommodityCode = "HAZ";

			line1.JL_PackageCount = 10;
			line2.JL_PackageCount = 10;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 1400m,
					ChargeCode = "FRT",
				}
			};

			AutorateAndAssert(expected, shipment, org);
			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			var paymentBasesForDisplay = ((IPaymentBasisViewCharge)shipmentCharges[0]).SellPaymentBasesView;
			AssertEquals("FRT|700|AUD|||700|||S00001000|MIN", paymentBasesForDisplay[0].ToString());
			AssertEquals("FRT|700|AUD|||700|||S00001000|MIN", paymentBasesForDisplay[1].ToString());
		}

		public void TestPaymentBasisContainsServiceAsReference()
		{
			var clnChargeCode = Helper.ChargeCodes.New("OCLN", "Service Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Cleaning);
			var fumChargeCode = Helper.ChargeCodes.New("OFUM", "Service Charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin, FreightServiceType.Codes.Fumigation);

			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "UAIEV", "AUSYD");
			rateEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var rateLine1 = rateEntry.AddRateLine("OCLN", TimeCalculator.Code, QuantityUnit.SV);
			rateLine1.GetCalculator<TimeCalculator>().PerUnit = 10;

			var rateLine2 = rateEntry.AddRateLine("OFUM", TimeCalculator.Code, QuantityUnit.SV);
			rateLine2.Calculator.IsAccumulated = false;
			var minusItem = rateLine2.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 10, 6m);
			minusItem.TM_BreakWeightVolume = QuantityUnit.HR;
			rateLine2.Calculator["+10"] = (ZDecimal)5m;
			rateLine2.Calculator["+15"] = (ZDecimal)4m;
			rateLine2.Calculator["+20"] = (ZDecimal)3m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.CreditorPK = creditor.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AA123456";
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_ContainerCount = 1;
			container.JC_RC = GP20.PK;

			var service1 = container.Services.AddNew();
			service1.ES_ServiceCount = 3;
			service1.ES_ServiceCode = FreightServiceType.Codes.Cleaning;
			service1.ES_Duration = new TimeSpan(4, 0, 0);
			service1.ES_Completed = ZDateTime.Now;
			service1.ES_References = "Cleaning with Tide";

			var service2 = container.Services.AddNew();
			service2.ES_ServiceCount = 2;
			service2.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			service2.ES_Duration = new TimeSpan(40, 0, 0);
			service2.ES_Completed = ZDateTime.Now;
			service2.ES_References = "Fumigating the container";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "UAIEV";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = IncoTerms.ExWorks;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;
			packLine.JL_PackageCount = 10;
			packLine.JL_RH_NKCommodityCode = "GEN";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "OCLN",
					JR_OSCostAmt = 30.00m
				},
				new AssertionCharge
				{
					ChargeCode = "OFUM",
					JR_OSCostAmt = 240.00m
				},
			};

			AutorateAndAssert(expected, shipment, creditor);

			var shipmentCharges = (shipment.Job as Job).Charges.Cast<BaseCharge>().ToList();

			var paymentBasesForDisplay1 = ((IPaymentBasisViewCharge)shipmentCharges[0]).CostPaymentBasesView;
			var paymentBasesForDisplay2 = ((IPaymentBasisViewCharge)shipmentCharges[1]).CostPaymentBasesView;

			AssertEquals("OCLN|30|AUD|3|SV x DY|10|DY|Cleaning with Tide|S00001000|UNT", paymentBasesForDisplay1[0].ToString());
			AssertEquals("OFUM|240|AUD|80|SV x HR|3|HR|Fumigating the container|S00001000|UNT", paymentBasesForDisplay2[0].ToString());
		}

		public void TestPaymentBasesMerge_ShipmentPackedInto2ContainersOfSameTypeWith2RateEntries()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var rateEntry20GP = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			rateEntry20GP.TI_RX_NKCurrency = "AUD";
			rateEntry20GP.RateLines.RemoveAndDeleteAll();
			rateEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1111m;

			var rateEntry40GP = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "40GP");
			rateEntry40GP.TI_RX_NKCurrency = "AUD";
			rateEntry40GP.RateLines.RemoveAndDeleteAll();
			rateEntry40GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 2000m;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USLAX", TransportProvider1, shipment);

			var container20GP_1 = consol.Containers.AddNew();
			container20GP_1.JC_RC = GP20.PK;
			container20GP_1.JC_ContainerNum = "CONT00001";
			var container20GP_2 = consol.Containers.AddNew();
			container20GP_2.JC_RC = container20GP_1.JC_RC;
			container20GP_2.JC_ContainerNum = "CONT00002";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_ActualVolume = 5.7m;
			packline1.JL_ActualWeight = 12280m;
			packline1.JL_JC = container20GP_1.PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualVolume = 5.7m;
			packline2.JL_ActualWeight = 12280m;
			packline2.JL_JC = container20GP_2.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 2222m,
					ChargeCode = "FRT",
				}
			};

			AutorateAndAssert(expected, shipment, Consignor);
		}

		public void TestPaymentBasis_CreatesZeroRatedPaymentBasis_NoMeasuresOnJob()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];

			var cost = Helper.NewCosting(null);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Box);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 1m;

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, PkgUnit.Box);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 2m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUMEL", "NLRTM", 25);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 0m,
					JR_LocalSellAmt = 0m,
					JR_CostRated = true,
					JR_SellRated = true,
				}
			};

			AutorateAndAssert("All rates are per box but the shipment has no boxes", expected, shipment, NewClient);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var charge = job.Charges.Cast<BaseCharge>().Single();

				var costPaymentBasis = ((IPaymentBasisViewCharge)charge).CostPaymentBasesView.Cast<JobPaymentBasis>().Single();
				var sellPaymentBasis = ((IPaymentBasisViewCharge)charge).SellPaymentBasesView.Cast<JobPaymentBasis>().Single();

				AssertEquals(1m, costPaymentBasis.PBS_PerUnitRate);
				AssertEquals(2m, sellPaymentBasis.PBS_PerUnitRate);

				AssertEquals(0m, costPaymentBasis.PBS_ChargeableAmount);
				AssertEquals(0m, sellPaymentBasis.PBS_ChargeableAmount);
			}
		}

		public void TestPaymentBasis_CreatesZeroRatedPaymentBasis_NoRates()
		{
			var chargeCode = Helper.ChargeCodes["ODOC"];

			var cost = Helper.NewCosting(null);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 0m;

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 0m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, NewClient.PK, "AUMEL", "NLRTM", 25);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_LocalCostAmt = 0m,
					JR_LocalSellAmt = 0m,
					JR_CostRated = true,
					JR_SellRated = true,
				}
			};

			AutorateAndAssert("0 * 25KG = 0", expected, shipment, NewClient);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var charge = job.Charges.Cast<BaseCharge>().Single();

				var costPaymentBasis = ((IPaymentBasisViewCharge)charge).CostPaymentBasesView.Cast<JobPaymentBasis>().Single();
				var sellPaymentBasis = ((IPaymentBasisViewCharge)charge).SellPaymentBasesView.Cast<JobPaymentBasis>().Single();

				AssertEquals(0m, costPaymentBasis.PBS_PerUnitRate);
				AssertEquals(0m, sellPaymentBasis.PBS_PerUnitRate);

				AssertEquals(25m, costPaymentBasis.PBS_ChargeableAmount);
				AssertEquals(25m, sellPaymentBasis.PBS_ChargeableAmount);
			}
		}

		public void TestPaymentBasis_ModifyingChargeClearsPaymentBases()
		{
			var client = Helper.NewOrgHeader();
			var creditor = Helper.NewOrgHeader();
			creditor.OH_IsCreditor = true;
			var consignee = Helper.NewOrgHeader();
			var consignor = Helper.NewOrgHeader();
			consignor.OH_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, "KG");
			((UnitCalculator)rateLine1.Calculator).PerUnit = 7m;

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine("FRT", UnitCalculator.Code, "KG").GetCalculator<UnitCalculator>().PerUnit = 1;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultShippingLineAddress(creditor);
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 7000m,
					JR_OSCostAmt = 1000m,
				}
			};

			AutorateAndAssert(expected, shipment, client);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var shipmentCharge = job.Charges.Cast<BaseCharge>().Single();

				var costPaymentBasis = ((IPaymentBasisViewCharge)shipmentCharge).CostPaymentBasesView.Cast<JobPaymentBasis>().Single();
				var sellPaymentBasis = ((IPaymentBasisViewCharge)shipmentCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().Single();

				AssertEquals("Current payment basis unit rate", 7m, sellPaymentBasis.PBS_PerUnitRate);
				AssertEquals("Current payment basis unit rate", 1m, costPaymentBasis.PBS_PerUnitRate);

				shipmentCharge.JR_OSCostAmt = 85m;
				var costPaymentBases = ((IPaymentBasisViewCharge)shipmentCharge).CostPaymentBasesView;
				var sellPaymentBases = ((IPaymentBasisViewCharge)shipmentCharge).SellPaymentBasesView;
				AssertEquals(1, sellPaymentBases.Count);
				AssertEquals(0, costPaymentBases.Count);

				shipmentCharge.JR_OSSellAmt = 95m;
				costPaymentBases = ((IPaymentBasisViewCharge)shipmentCharge).CostPaymentBasesView;
				sellPaymentBases = ((IPaymentBasisViewCharge)shipmentCharge).SellPaymentBasesView;
				AssertEquals(0, sellPaymentBases.Count);
				AssertEquals(0, costPaymentBases.Count);
			}
		}

		public void TestPaymentBasis_ModifyingCostClearsCostPaymentBases()
		{
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Factory.New<Costing>();
			costing.TH_OH = creditor.PK;

			var costEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry20GP.TI_RX_NKCurrency = "AUD";
			costEntry20GP.RateLines.RemoveAndDeleteAll();
			costEntry20GP.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 1000m;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_Vessel = "ADMIRAL";
			consol.Transports[0].JW_VoyageFlight = "123S";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var container20GP = consol.Containers.AddNew();
			container20GP.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container20GP.JC_ContainerNum = "CONT00001";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = TransportModes.Sea;
			shipment1.JS_PackingMode = ContainerModes.FCL;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_INCO = "CFR";
			shipment1.JS_OH_DeliveryAgent = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;

			var packline1_1 = shipment1.OuterPackLines.AddNew();
			packline1_1.JL_ActualVolume = 5.2m;
			packline1_1.JL_ActualWeight = 15000m;
			packline1_1.JL_JC = container20GP.PK;

			Factory.Save();

			using (var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				job1.JH_OA_LocalChargesAddr = consignor.MainAddress.PK;

				Factory.Save();

				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "FRT",
						E6_OSCostAmount = 1000m,
					},
				};

				AutoCostAndAssert("Total cost of both rate entries with FRT charge applied to the consol", null, expectedCosts, consol);

				var consolCost = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).FirstOrDefault();

				AssertEquals(1, ((IPaymentBasisViewCharge)consolCost).CostPaymentBasesView.Count);

				consolCost.E6_LocalCostAmount = 25m;

				AssertEquals(0, ((IPaymentBasisViewCharge)consolCost).CostPaymentBasesView.Count);
			}
		}

		public void TestAutoRatingShipment_FirstPlusCalculatorSeparateQuantityReferenceResults_WithContainerNumber()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "USLAX", "AUSYD", "", "20GP");

			var fpaRateLine = rateEntry1.AddRateLine("DCART", FirstPlusAdditionalCalculator.Code, QuantityUnit.CN);
			var firstPlusCalculator = fpaRateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			firstPlusCalculator.First = 0m;
			firstPlusCalculator.Additional = 2.5m;

			var unitRateLine = rateEntry1.AddRateLine("DDOC", UnitCalculator.Code, QuantityUnit.CN);
			unitRateLine.GetCalculator<UnitCalculator>().PerUnit = 6m;

			Factory.Save();

			#region Setup Consol, Shipment and Containers

			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerNum = "CONT000001";
			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerNum = "CONT000002";
			var container3 = consol.Containers.AddNew();
			container3.JC_RC = GP20.PK;
			container3.JC_ContainerNum = "CONT000003";
			var container4 = consol.Containers.AddNew();
			container4.JC_RC = GP20.PK;
			container4.JC_ContainerNum = "CONT000004";
			var container5 = consol.Containers.AddNew();
			container5.JC_RC = GP20.PK;
			container5.JC_ContainerNum = "CONT000005";

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, NewClient.PK, "USLAX", "AUSYD", 0m, consol: consol);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.JS_F3_NKPackType = PkgUnit.Pallet;
			shipment.JS_OuterPacks = 80;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_JC = container1.PK;
			line1.JL_F3_NKPackType = PkgUnit.Pallet;

			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 15;
			line2.JL_JC = container2.PK;
			line2.JL_F3_NKPackType = PkgUnit.Pallet;

			var line3 = shipment.OuterPackLines.AddNew();
			line3.JL_PackageCount = 25;
			line3.JL_JC = container3.PK;
			line3.JL_F3_NKPackType = PkgUnit.Pallet;

			var line4 = shipment.OuterPackLines.AddNew();
			line4.JL_PackageCount = 10;
			line4.JL_JC = container4.PK;
			line4.JL_F3_NKPackType = PkgUnit.Pallet;

			var line5 = shipment.OuterPackLines.AddNew();
			line5.JL_PackageCount = 20;
			line5.JL_JC = container5.PK;
			line5.JL_F3_NKPackType = PkgUnit.Pallet;

			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 30m,
					RevenueCalculationDescription = "DDOC: 5 20GP Container(s) @ AUD 6.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 10m,
					JR_OSCostAmt = 10m,
					RevenueCalculationDescription = "DCART: 4 20GP Container(s) @ AUD 2.50/Container"
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			var shipmentCharges = job.Charges;

			var ddocCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "DDOC");
			var ddocPaymentBases = ((IPaymentBasisViewCharge)ddocCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT000001", "CONT000002", "CONT000003", "CONT000004", "CONT000005" }, ddocPaymentBases.Select(x => x.PBS_ChargeableDescription));

			var dcartCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "DCART");
			var dcartPaymentBasis = ((IPaymentBasisViewCharge)dcartCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT000002", "CONT000003", "CONT000004", "CONT000005" }, dcartPaymentBasis.Select(x => x.PBS_ChargeableDescription));

			unitRateLine.TL_WeightVolume = PkgUnit.Pallet;
			fpaRateLine.TL_WeightVolume = PkgUnit.Pallet;
			firstPlusCalculator.First = 0m;
			firstPlusCalculator.Additional = 3m;

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 480m,
					RevenueCalculationDescription = "DDOC: 80 Pallet(s) @ AUD 6.00/Pallet"
				},
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 237m,
					JR_OSCostAmt = 237m,
					RevenueCalculationDescription = "DCART: 79 Pallet(s) @ AUD 3.00/Pallet"
				}
			};

			var message = "This test covers the current functionality which may not be the correct from a product perspective. The point of this test is to raise awareness of this functionality in case this changes in future rather than to ensure it does not change.";
			AutorateAndAssert(message, expected, shipment, NewClient);

			shipmentCharges = job.Charges;

			ddocCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "DDOC");
			ddocPaymentBases = ((IPaymentBasisViewCharge)ddocCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertEquals("80", ddocPaymentBases.Single().Quantity);
			var expectedDescription = "CONT000001, CONT000002, CONT000003, CONT000004, CONT000005";
			message = "Since there was only one payment basis for all references, due to not enough space, some container numbers have been truncated";
			AssertEquals(message, expectedDescription, ddocPaymentBases.Single().PBS_ChargeableDescription);

			dcartCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "DCART");
			dcartPaymentBasis = ((IPaymentBasisViewCharge)dcartCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertEquals("79", dcartPaymentBasis.Single().Quantity);
			AssertEquals(message, expectedDescription, dcartPaymentBasis.Single().PBS_ChargeableDescription);
		}

		public void TestAutoRatingShipment_FirstPlusCalculatorSeparateQuantityReferenceResults_WithoutContainerNumber()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var gp20RateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "USLAX", "AUSYD", "", "20GP");
			gp20RateEntry.RateLines.RemoveAndDeleteAll();
			var gp20RateLine = gp20RateEntry.AddRateLine("DCART", FirstPlusAdditionalCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			var gp20RateLineFpaCalculator = gp20RateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			gp20RateLineFpaCalculator.First = 6m;
			gp20RateLineFpaCalculator.Additional = 2.5m;

			var gp40RateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD", "", "40GP");
			gp40RateEntry.RateLines.RemoveAndDeleteAll();
			var gp40RateLine = gp40RateEntry.AddRateLine("FRT", FirstPlusAdditionalCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			gp40RateLine.TL_WeightVolumeMultiple = 10;
			var gp40RateLineFpaCalculator = gp40RateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			gp40RateLineFpaCalculator.First = 10m;
			gp40RateLineFpaCalculator.Additional = 5m;

			var pl20RateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD", "", "20PL");
			pl20RateEntry.RateLines.RemoveAndDeleteAll();
			var pl20RateLine = pl20RateEntry.AddRateLine("CAF", FirstPlusAdditionalCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			var pl20RateLineCalculator = pl20RateLine.GetCalculator<FirstPlusAdditionalCalculator>();
			pl20RateLineCalculator.First = 20m;
			pl20RateLineCalculator.Additional = 15m;

			var hc40RateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "USLAX", "AUSYD", "", "40HC");
			hc40RateEntry.RateLines.RemoveAndDeleteAll();
			var hc40RateLine = hc40RateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			hc40RateLine.GetCalculator<UnitCalculator>().PerUnit = 6m;

			Factory.Save();

			#region Setup Consol, Shipment and Containers

			var hc40 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40HC"));
			var pl20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20PL"));

			var consol = Factory.New<ForwardingConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerNum = "CONT000001";
			container1.JC_ContainerCount = 1;

			var container2 = consol.Containers.AddNew();
			container2.JC_RC = GP20.PK;
			container2.JC_ContainerNum = "CONT000002";
			container2.JC_ContainerCount = 1;

			var container3 = consol.Containers.AddNew();
			container3.JC_RC = GP20.PK;
			container3.JC_ContainerCount = 4;

			var container4 = consol.Containers.AddNew();
			container4.JC_RC = GP40.PK;
			container4.JC_ContainerNum = "CONT000004";
			container4.JC_ContainerCount = 1;

			var container5 = consol.Containers.AddNew();
			container5.JC_RC = GP40.PK;
			container5.JC_ContainerCount = 20;

			var container6 = consol.Containers.AddNew();
			container6.JC_RC = hc40.PK;
			container6.JC_ContainerCount = 5;

			var container7 = consol.Containers.AddNew();
			container7.JC_RC = hc40.PK;
			container7.JC_ContainerNum = "CONT000007";
			container7.JC_ContainerCount = 1;

			var container8 = consol.Containers.AddNew();
			container8.JC_RC = pl20.PK;
			container8.JC_ContainerCount = 15;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, NewClient.PK, "USLAX", "AUSYD", 0m, consol: consol);
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_JC = container1.PK;

			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_JC = container2.PK;

			var line3 = shipment.OuterPackLines.AddNew();
			line3.JL_JC = container3.PK;

			var line4 = shipment.OuterPackLines.AddNew();
			line4.JL_JC = container4.PK;

			var line5 = shipment.OuterPackLines.AddNew();
			line5.JL_JC = container5.PK;

			var line6 = shipment.OuterPackLines.AddNew();
			line6.JL_JC = container6.PK;

			var line7 = shipment.OuterPackLines.AddNew();
			line7.JL_JC = container7.PK;

			var line8 = shipment.OuterPackLines.AddNew();
			line8.JL_JC = container8.PK;

			Factory.Save();

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DCART",
					JR_OSSellAmt = 18.50m,
					RevenueCalculationDescription = "DCART: 1 20GP Container(s) @ AUD 6.00/Container + 1 20GP Container(s) @ AUD 2.50/Container + 4 20GP Container(s) @ AUD 2.50/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 11m,
					RevenueCalculationDescription = "FRT: 1 40GP Container(s) @ AUD 10.00/10 Container + 20 40GP Container(s) @ AUD 5.00/10 Container"
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 36m,
					RevenueCalculationDescription = "BAF: 1 40HC Container(s) @ AUD 6.00/Container + 5 40HC Container(s) @ AUD 6.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 230m,
					RevenueCalculationDescription = "CAF: 1 20PL Container(s) @ AUD 20.00/Container + 14 20PL Container(s) @ AUD 15.00/Container"
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			var shipmentCharges = job.Charges;

			var dcartCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "DCART");
			var dcartPaymentBases = ((IPaymentBasisViewCharge)dcartCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT000001", "CONT000002", string.Empty }, dcartPaymentBases.Select(x => x.PBS_ChargeableDescription));
			AssertEquals("4", dcartPaymentBases.Single(x => x.PBS_ChargeableDescription.IsEmpty).Quantity);

			var frtCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "FRT");
			var frtPaymentBases = ((IPaymentBasisViewCharge)frtCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT000004", string.Empty }, frtPaymentBases.Select(x => x.PBS_ChargeableDescription));
			AssertEquals("20", frtPaymentBases.Single(x => x.PBS_ChargeableDescription.IsEmpty).Quantity);

			var bafCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "BAF");
			var bafPaymentBases = ((IPaymentBasisViewCharge)bafCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "CONT000007", string.Empty }, bafPaymentBases.Select(x => x.PBS_ChargeableDescription));
			AssertEquals("5", bafPaymentBases.Single(x => x.PBS_ChargeableDescription.IsEmpty).Quantity);

			var cafCharge = shipmentCharges.Cast<Charge>().Single(x => x.ChargeCode.AC_Code == "CAF");
			var cafPaymentBases = ((IPaymentBasisViewCharge)cafCharge).SellPaymentBasesView.Cast<JobPaymentBasis>().ToList();
			Assert(cafPaymentBases.Select(x => x.PBS_ChargeableDescription).All(x => x.IsEmpty));
			AssertContainsExactElementsInAnyOrder(new[] { "1", "14" }, cafPaymentBases.Select(x => x.Quantity));
		}

		public void TestAutoRatingUnsavedShipment_NoDuplicateCharge()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false; // this config is not part of this change. It's just mainly not to set any tax rate on job charges

			Helper.NewClientRate(NewClient).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "NZ", "ODOC", 60m, CurrencyCodes.Australia);
			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "NZ", "ODOC", 20m, CurrencyCodes.Australia);

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.ConsigneePK = NewClient.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 60m,
					JR_OSCostAmt = 20m
				}
			};

			AutorateAndAssert(expected, shipment, NewClient);
			AssertEquals("Pre-Condition: Code Property Has not yet been generated", true, shipment.JS_UniqueConsignRef.IsEmpty);

			using (var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate())
			{
				var jobCharge = shipmentJob.Charges[0];

				Assert(jobCharge.CostPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));
				Assert(jobCharge.SellPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));

				AutorateAndAssert("The charge should be deleted and recreated and there should be still one charge and no duplicate", expected, shipment, NewClient, job: shipmentJob);

				jobCharge = shipmentJob.Charges[0];
				Assert(jobCharge.CostPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));
				Assert(jobCharge.SellPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));

				Factory.Save();

				Assert("Saving Payment basis has populated the operational Job Code", jobCharge.CostPaymentBases.All(x => x.PBS_AdapterID == shipment.JS_UniqueConsignRef));
				Assert("Saving Payment basis has populated the operational Job Code", jobCharge.SellPaymentBases.All(x => x.PBS_AdapterID == shipment.JS_UniqueConsignRef));

				AutorateAndAssert("Reautorating should not created duplicate charges", expected, shipment, NewClient, job: shipmentJob);
			}
		}

		public void TestAutoRatingUnsavedShipmentAndTransportBooking_NoDuplicateCharge()
		{
			DataRegistryRating.Instance.AllowSavingOfAutoRatingLogNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false; // this config is not part of this change. It's just mainly not to set any tax rate on job charges

			#region Rates Setup

			var chargeCode = Helper.ChargeCodes.New("TBK1", "Transport Booking 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var rate = Helper.NewClientRate(NewClient);

			var rateEntry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "USLAX");
			rateEntry1.RateLines.RemoveAndDeleteAll();

			var freightRateLine = rateEntry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			freightRateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;

			var rateEntry2 = rate.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			rateEntry2.RateLines.RemoveAndDeleteAll();

			var line2 = rateEntry2.AddRateLine(chargeCode, FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 8000;

			var costProvider = Factory.NewWithValidTestData<OrgHeader>();
			costProvider.OH_IsCreditor = true;

			var cost = Helper.NewCosting(costProvider);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 2m;

			Factory.Save();

			#endregion

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = PkgUnit.Pallet;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = NewClient.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 15000m
					}
			};

			AutorateAndAssert(expected, shipment, NewClient);

			var job = new Job.Loader(shipment).TryLoadOrCreate();
			var jobCharge = job.Charges[0];

			Assert(jobCharge.SellPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));

			var manager = new DtbDeliveryManager(Factory, shipment, DtbBookingDirection.PIC, false);
			try
			{
				// Create Pickup transport job via Action Menu
				UnitTestUserNotification.Instance.AddYesAnswer();
				manager.CreateTransportBooking();

				AssertEquals("Please save this Shipment before creating a Transport Booking.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery());
				AssertNull("No Transport Consolation should have been created as the Shipment is unsaved", consolidation);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

				Factory.Save();
				Assert("Saving shipment should populate the operational Job Code", jobCharge.SellPaymentBases.All(x => x.PBS_AdapterID == shipment.JS_UniqueConsignRef));

				manager.CreateTransportBooking();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery());
				var booking = consolidation.Bookings.Single();
				booking.Address.OrganisationPK = costProvider.PK;

				var fromInstruction = booking.Instructions.Single(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp);
				fromInstruction.Address.OrganisationPK = NewClient.PK;

				var toInstruction = booking.Instructions.Single(x => x.KN_InstructionType == InstructionTypes.Codes.Delivery);
				toInstruction.Address.OrganisationPK = Consignee.PK;

				freightRateLine.GetCalculator<UnitCalculator>().PerUnit = 7m;
				Factory.Save();

				using (var plugin = new InvoicingPluginToFreight(booking))
				{
					expected = new[]
					{
						new AssertionCharge
							{
								ChargeCode = "FRT",
								JR_OSSellAmt = 15000m //should not changed
							},
						new AssertionCharge
							{
								ChargeCode = "TBK1",
								JR_OSSellAmt = 8000,
								JR_OSCostAmt = 6000,
								JR_Desc = "Transport Booking 1 {TB00000001}"
							}
					};

					var emptyArray = Array.Empty<string>();
					plugin.ExecuteAutorating(AutoRateOptions.AutorateCostsRevenue);
					var message = "Since we're autorating from TB00000001, only the charges for that booking should be updated. " +
						"The shipment's rates should not change because we should not re-autorate it. There should be no duplicate charges";
					var summary = GetAutoRatingSummary(message, shipment, emptyArray, emptyArray);
					AssertCharges(summary, expected, job);
				}

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						JR_OSSellAmt = 21000m
					},
					new AssertionCharge
					{
						ChargeCode = "TBK1",
						JR_OSSellAmt = 8000,
						JR_OSCostAmt = 6000,
						JR_Desc = "Transport Booking 1 {TB00000001}"
					}
				};

				AutorateAndAssert("Re-AutoRating the shipment should autorate all the jobs again, there should not be any duplicate charges and all values now match the updated rates", expected, shipment, NewClient);
			}
			finally
			{
				manager.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestAutoRatingUnsavedSpotQuote_NoDuplicateCharge()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false; // this config is not part of this change. It's just mainly not to set any tax rate on job charges

			var clientRate = Helper.NewClientRate(Consignor);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var costing = Helper.NewCosting(null);
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = entry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 10m;

			Factory.Save();

			var spotQuote = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, "", Consignor, Consignor, null, null, "AUSYD", "USLAX", 100m, 0m, QuotedBookingState.QuoteOnly);

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1000m,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			AutorateAndAssert(expectedCharges, spotQuote, Consignor);
			var quotedBookingJob = new Job.Loader(spotQuote).TryLoadOrCreate();

			var frtCharge = quotedBookingJob.Charges.Where(x => x.ChargeCode.AC_Code == "FRT").Single();
			Assert(frtCharge.SellPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));

			var bafCharge = quotedBookingJob.Charges.Where(x => x.ChargeCode.AC_Code == "BAF").Single();
			Assert(bafCharge.CostPaymentBases.All(x => x.PBS_AdapterID.IsEmpty));

			Factory.Save();

			Assert(frtCharge.SellPaymentBases.All(x => x.PBS_AdapterID == spotQuote.Quote.TH_QuoteNumber));
			Assert(bafCharge.CostPaymentBases.All(x => x.PBS_AdapterID == spotQuote.Quote.TH_QuoteNumber));
		}

		public void TestAutoRatingUnsavedQuotedBooking_NoDuplicateCharge()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false; // this config is not part of this change. It's just mainly not to set any tax rate on job charges

			#region Rates Setup

			var clientRate = Helper.NewClientRate(Consignor);
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var costing = Helper.NewCosting(null);
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX");
			entry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = entry2.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 10m;

			Factory.Save();

			#endregion

			var quotedBooking = CreateQuotedBooking(TransportModes.Air, RateMode.LSE, IncoTerms.CostAndFreight, Consignor, Consignor, Consignee, null, "AUSYD", "USLAX", 100m, 0m, QuotedBookingState.QuoteOnly);
			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2000m,
					RevenueCalculationDescription = "FRT: 100 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1000m,
					CostCalculationDescription = "BAF: 100 Kilogram(s) @ AUD 10.00/KG"
				},
			};
			AutorateAndAssert(expectedCharges, quotedBooking, Consignor);

			Factory.Save();

			UnitTestUserNotification.Instance.AddYesAnswer();
			quotedBooking.ConvertQuoteToQuotedBooking();

			var booking = quotedBooking.Booking;
			booking.JS_ActualWeight = 105m;

			expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 2100m,
					RevenueCalculationDescription = "FRT: 105 Kilogram(s) @ AUD 20.00/KG",
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 1050m,
					CostCalculationDescription = "BAF: 105 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			var quotedBookingJob = quotedBooking.Job as Job;
			AssertEquals("Spot Quote Charges are available on quotedBooking", 2, quotedBookingJob.Charges.Count);
			AutorateAndAssert("Autorating should not create duplicate or extra charges", expectedCharges, quotedBooking, Consignor, job: quotedBookingJob);

			Factory.Save();
			AutorateAndAssert("Re-Autorate quoted Booking should not create duplicate charges", expectedCharges, quotedBooking, Consignor, job: quotedBookingJob);
		}

		public void TestAutoRatingUnsavedCommonCartage_NoDuplicateCharges()
		{
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("PT20", "Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("PT40", "Transport", UnitCalculator.Code, ChargeCodeGroupList.Codes.Transport);

			var clientRate = Helper.NewClientRate(NewClient);
			var gp20Entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			gp20Entry.TI_RC = GP20.PK;
			var gp20Line = gp20Entry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			gp20Line.GetCalculator<UnitCalculator>().PerUnit = 100m;

			var gp40Entry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, RateMode.FRO, "AU", "");
			gp40Entry.TI_RC = GP40.PK;
			var gp40Line = gp40Entry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.CN, CurrencyCodes.Australia);
			gp40Line.GetCalculator<UnitCalculator>().PerUnit = 120m;

			Factory.Save();

			#region CreateContainerisedCartage

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = CartageJobType.NEW_FCLImportToCNE;
			cartage.JJ_ContainerMode = CartageContainerMode.Containerized;
			cartage.LocalClientAddressPK = NewClient.MainAddress.PK;

			var container20GP = Factory.NewWithValidTestData<CommonContainer>();
			container20GP.JC_RC = GP20.PK;

			var move1 = Factory.New<CommonBookedCtgMove>();
			move1.EW_JJ = cartage.PK;
			move1.EW_JC_Container = container20GP.PK;
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_RunSheetSequence = 1;
			leg1.JU_EW = move1.PK;

			var container40GP = Factory.NewWithValidTestData<CommonContainer>();
			container40GP.JC_RC = GP40.PK;

			var move2 = Factory.New<CommonBookedCtgMove>();
			move2.EW_JJ = cartage.PK;
			move2.EW_JC_Container = container40GP.PK;
			move2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage.SecondDocAddress.PK;

			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_RunSheetSequence = 2;
			leg2.JU_EW = move2.PK;

			#endregion

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PT20",
					JR_LocalSellAmt = 100m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 100.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "PT40",
					JR_LocalSellAmt = 120m,
					RevenueCalculationDescription = "1 40GP Container(s) @ AUD 120.00/Container"
				}
			};

			AutorateAndAssert(expected, cartage, NewClient, autorateCosts: false);

			var cartageJob = new Job.Loader(cartage).TryLoadOrCreate();

			var sellPaymentBases1 = ((IPaymentBasisViewCharge)cartageJob.Charges[0]).SellPaymentBasesView.Cast<JobPaymentBasis>();
			var sellPaymentBases2 = ((IPaymentBasisViewCharge)cartageJob.Charges[1]).SellPaymentBasesView.Cast<JobPaymentBasis>();
			Assert(sellPaymentBases1.Single().PBS_AdapterID.IsEmpty);
			Assert(sellPaymentBases2.Single().PBS_AdapterID.IsEmpty);

			AutorateAndAssert("re-autorate the job should not create duplicate charges", expected, cartage, NewClient, job: cartageJob, autorateCosts: false);

			Factory.Save();

			sellPaymentBases1 = ((IPaymentBasisViewCharge)cartageJob.Charges[0]).SellPaymentBasesView.Cast<JobPaymentBasis>();
			sellPaymentBases2 = ((IPaymentBasisViewCharge)cartageJob.Charges[1]).SellPaymentBasesView.Cast<JobPaymentBasis>();
			AssertEquals("T00001000", sellPaymentBases1.Single().PBS_AdapterID);
			AssertEquals("T00001000", sellPaymentBases2.Single().PBS_AdapterID);

			gp20Line.GetCalculator<UnitCalculator>().PerUnit = 80m;
			gp40Line.GetCalculator<UnitCalculator>().PerUnit = 90m;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "PT20",
					JR_LocalSellAmt = 80m,
					RevenueCalculationDescription = "1 20GP Container(s) @ AUD 80.00/Container"
				},
				new AssertionCharge
				{
					ChargeCode = "PT40",
					JR_LocalSellAmt = 90m,
					RevenueCalculationDescription = "1 40GP Container(s) @ AUD 90.00/Container"
				}
			};

			AutorateAndAssert("re-autorate the job should not create duplicate charges and should update the existing charges per changes", expected, cartage, NewClient, job: cartageJob, autorateCosts: false);
		}

		public void TestAutoRate_PerUnitRateWithMultiply_PopulateMultiplyToPaymentBasis()
		{
			var cost = CreateCosting();
			var costRate = CreateRate(cost);
			var costLine = costRate.AddPerUnitCharge("FRT", 1000);
			costLine.TL_WeightVolumeMultiple = 100;

			var shipment = CreateShipment(containerMode: "LCL");
			shipment.JS_ActualWeight = 500;
			shipment.JS_UnitOfWeight = "KG";

			var consol = CreateConsol(containerMode: "LCL");
			consol.Shipments.Add(shipment);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalCostAmt = 5000m,
					CostCalculationDescription = "500 Kilogram(s) @ USD 1000.00/100 KG"
				}
			};

			AutorateAndAssert(expected, shipment, Consignee, autorateCosts: true);

			var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			var charge = job.Charges[0] as BaseCharge;
			var actual = charge.CostPaymentBases.Select(c => $"{c.PBS_ChargeableAmount}|{c.PBS_PerUnitRate}|{c.PBS_RateUnit}|{c.PBS_RateUnitMultiplier}").ToArray();

			var expectedResult = new[]
			{
				"500|10.000|KG|100.0"
			};

			AssertContainsExactElementsInAnyOrder(
				"Payment basis details should match the expected values",
				expectedResult,
				actual
			);
		}

		public void TestIJobPaymentBasisProperties()
		{
			var pbs = Factory.New<JobPaymentBasis>();
			pbs.PBS_AdapterID = "S00002134";
			pbs.PBS_AdapterType = "Shipment";
			pbs.PBS_ChargeableDescription = "This is description";
			pbs.PBS_MinRate = 1M;
			pbs.PBS_MaxRate = 20M;
			pbs.PBS_FlatRate = 4M;
			pbs.PBS_PerUnitRate = 2M;
			pbs.PBS_ChargeableAmount = 200M;
			pbs.PBS_ChargeableUnit = "KG";
			pbs.PBS_ChargeableUnitType = "Weight";
			pbs.PBS_RateUnit = "1";
			pbs.PBS_RateUnitType = "1111";
			pbs.PBS_RX_NKRateCurrency = "AUD";
			pbs.PBS_RateReference = "UNT";

			var basisInterface = (IJobPaymentBasis)pbs;
			AssertEquals("AdapterID", "S00002134", basisInterface.AdapterID);
			AssertEquals("AdapterType", "Shipment", basisInterface.AdapterType);
			AssertEquals("ChargeableDescription", "This is description", basisInterface.ChargeableDescription);
			AssertEquals("MinRate", 1M, basisInterface.MinRate);
			AssertEquals("MaxRate", 20M, basisInterface.MaxRate);
			AssertEquals("FlatRate", 4M, basisInterface.FlatRate);
			AssertEquals("PerUnitRate", 2M, basisInterface.PerUnitRate);
			AssertEquals("ChargeableAmount", 200M, basisInterface.ChargeableAmount);
			AssertEquals("ChargeableUnit", "KG", basisInterface.ChargeableUnit);
			AssertEquals("ChargeableUnitType", "Weight", basisInterface.ChargeableUnitType);
			AssertEquals("RateUnit", "1", basisInterface.RateUnit);
			AssertEquals("RateUnitType", "1111", basisInterface.RateUnitType);
			AssertEquals("RateCurrency", "AUD", basisInterface.RateCurrency);
			AssertEquals("RateReference", "UNT", basisInterface.RateReference);
			AssertEquals("RateCurrencyDescription", Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD").RX_Desc, basisInterface.RateCurrencyDescription);
		}

		RatingHeader CreateCosting()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			return costing;
		}

		RateEntry CreateRate(
			RatingHeader header,
			string category = "LCL",
			string mode = "LCL",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = null,
			string commodity = "GEN")
		{
			var entry = header.AddRateEntry(category, mode, origin, destination, "STD", container, commodity);
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}
	}

	#region Business  Object TestCase

	[TestedType(typeof(JobPaymentBasis))]
	public class JobPaymentBasisBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetJobPaymentBasisWithChargeReference(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetJobPaymentBasisWithChargeReference(factory);
		}

		JobPaymentBasis GetJobPaymentBasisWithChargeReference(BusinessObjectFactory factory)
		{
			var freightChargeCode = Env.Registry.FreightChargeCode;
			var client = new TestHelper(factory).NewOrgHeader();
			client.OH_IsDebtor = true;

			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CFR";

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.LocalChargesPK = client.PK;
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = freightChargeCode;
			charge.JR_OH_SellAccount = client.PK;
			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_OSCostAmt = 900m;

			factory.Save();

			var pbs = Factory.NewWithValidTestData<JobPaymentBasis>();
			pbs.PBS_JR = charge.PK;

			return pbs;
		}
	}

	#endregion
}
