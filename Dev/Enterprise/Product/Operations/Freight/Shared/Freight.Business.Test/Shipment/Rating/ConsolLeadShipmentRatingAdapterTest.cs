using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolLeadShipmentRatingAdapterTest : TestCaseWithFactory
	{
		#region BCN

		public void TestDebtorOrgs()
		{
			var leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var controllingCustomerOrg = Factory.New<OrgHeader>();
			var controllingCustomerAddress = leadShipment.ControllingCustomerAddress;
			controllingCustomerAddress.OrganisationPK = controllingCustomerOrg.PK;
			AssertEquals("Controlling Customer", controllingCustomerOrg, leadShipment.ControllingCustomer);

			var consignorOrg = Factory.New<OrgHeader>();
			var consignorAddress = leadShipment.ConsignorDocumentaryAddress;
			consignorAddress.OrganisationPK = consignorOrg.PK;
			AssertEquals("Consignor", consignorOrg, leadShipment.Consignor);

			var consigneeOrg = Factory.New<OrgHeader>();
			var consigneeAddress = leadShipment.ConsigneeDocumentaryAddress;
			consigneeAddress.OrganisationPK = consigneeOrg.PK;
			AssertEquals("Consignee", consigneeOrg, leadShipment.Consignee);

			var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, null);
			CombineAssertions("DebtorOrgs should have CCUS, CNR, CNE", () =>
			{
				AssertEquals("Count", 3, adapter?.DebtorOrgs.Count);
				AssertEquals("Controlling customer", controllingCustomerOrg, adapter.DebtorOrgs[RatingDebtorOrgTypes.CCUS]);
				AssertEquals("Consignor", consignorOrg, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
				AssertEquals("Consignee", consigneeOrg, adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			});
		}

		public void TestAutoRatedFor()
		{
			var leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var subShipment = Factory.New<CommonShipment>();

			Factory.Save();

			var adapter1 = new ConsolLeadShipmentRatingAdapter(leadShipment, null);
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN only leadShipment THEN AutoRatedFor should return leadShipment",
				new[] { leadShipment.JobNumber },
				adapter1.AutoRatedFor.Cast<CommonShipment>().Select(x => x.JobNumber)
			);

			var adapter2 = new ConsolLeadShipmentRatingAdapter(leadShipment, subShipment);
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN leadShipment and subShipment THEN AutoRatedFor should return subShipment",
				new[] { subShipment.JobNumber },
				adapter2.AutoRatedFor.Cast<CommonShipment>().Select(x => x.JobNumber)
			);
		}

		#region Payment Term

		public void TestPaymentTerm_LeadShipment()
		{
			var leadShipment = CommonShipment.New(Factory);
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;

			var job = AddJobToShipment(leadShipment);
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			foreach (var incoterm in new[] { Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeCarrierSeller, Core.Constants.IncoTerms.FreeCarrierBuyer })
			{
				leadShipment.JS_INCO = incoterm;
				var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, null);
				AssertPaymentTerm
				(
					adapter,
					job,
					expectedPaymentTerms: new Dictionary<string, string>()
					{
						{  Core.Constants.ConsolInvoicingStyles.Master, incoterm },
						{  Core.Constants.ConsolInvoicingStyles.Apportion, incoterm },
						{  Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster, incoterm },
					},
					message: $"GIVEN no SubShipment, THEN PaymentTerm should be from LeadShipment Incoterm {incoterm}."
				);
			}
		}

		public void TestPaymentTerm_SubShipment()
		{
			var leadShipment = CommonShipment.New(Factory);
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			leadShipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;

			var subShipment = leadShipment.CoLoadShipments.AddNew();

			var job = AddJobToShipment(subShipment);
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			foreach (var incoterm in new[] { Core.Constants.IncoTerms.FreeCarrier, Core.Constants.IncoTerms.FreeCarrierSeller, Core.Constants.IncoTerms.FreeCarrierBuyer })
			{
				subShipment.JS_INCO = incoterm;
				var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, subShipment);
				AssertPaymentTerm
				(
					adapter,
					job,
					expectedPaymentTerms: new Dictionary<string, string>()
					{
						{  Core.Constants.ConsolInvoicingStyles.Master, Core.Constants.IncoTerms.ExWorks },
						{  Core.Constants.ConsolInvoicingStyles.Apportion, incoterm },
						{  Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster, incoterm },
					},
					message: $"GIVEN SubShipment exist, THEN PaymentTerm should be from SubShipment Incoterm {incoterm}, unless InvoicingStyle=MAS."
				);
			}
		}

		void AssertPaymentTerm(ConsolLeadShipmentRatingAdapter adapter, JobHeader job, Dictionary<string, string> expectedPaymentTerms, string message)
		{
			foreach (var invoicingStyle in expectedPaymentTerms.Keys)
			{
				job.LocalCharges.CompanyData.OB_ARBuyersConsolInvoicingStyle = invoicingStyle;
				AssertContainsExactElementsInAnyOrder
				(
					$"{message} InvoicingStyle={invoicingStyle}",
					new[] { expectedPaymentTerms[invoicingStyle], expectedPaymentTerms[invoicingStyle] },
					adapter.PaymentTerm.PaymentTermInfoCollection.Select(x => x.Value)
				);
			}
		}

		JobHeader AddJobToShipment(CommonShipment shipment)
		{
			JobHeader result = Factory.NewJobForTesting<JobHeader>();
			result.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			result.JH_ParentID = shipment.PK;
			result.JH_GC = GlbCompany.CurrentCompany.PK;
			result.JH_GB = GlbBranch.CurrentBranch.PK;
			result.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}

		public void TestPaymentTerm_Set()
		{
			var leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var subShipment = Factory.New<CommonShipment>();

			var job = AddJobToShipment(subShipment);
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			job.LocalCharges.CompanyData.OB_ARBuyersConsolInvoicingStyle = Core.Constants.ConsolInvoicingStyles.Apportion;

			var paymentTerm = new PaymentTermInfos();
			paymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Cost, Core.Constants.IncoTerms.FreeCarrier));

			var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, subShipment);
			AssertContainsExactElementsInAnyOrder
			(
				"Precondition",
				System.Array.Empty<string>(),
				adapter.PaymentTerm.PaymentTermInfoCollection.Select(x => x.Value)
			);

			adapter.PaymentTerm = paymentTerm;
			AssertContainsExactElementsInAnyOrder
			(
				"GIVEN subShipment with APP invoicing-style THEN should able to set PaymentTerm",
				new[] { Core.Constants.IncoTerms.FreeCarrier },
				adapter.PaymentTerm.PaymentTermInfoCollection.Select(x => x.Value)
			);
		}

		#endregion

		#region IAutoRating

		public void TestJobDatesProvider()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();

			IAutoRating adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);
			AssertType<CommonShipmentJobDatesProvider>(adapter.JobDatesProvider);
		}

		public void TestAdapterTypeAndID()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();

			IAutoRating adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);
			AssertEquals(AdapterType.Consolidation, adapter.AdapterType);
			AssertEquals(masterShipment.JS_UniqueConsignRef, adapter.OperationalJobCode);
		}

		#endregion

		#region IAutoRatingLocations

		public void TestRateOrigin()
		{
			var leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_RL_NKFreightRateOrigin = "AUMEL";

			var subShipment = Factory.New<CommonShipment>();

			var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, subShipment);
			AssertEquals("AUMEL", adapter.RateOrigin.Code);
		}

		public void TestRateDestination()
		{
			var leadShipment = Factory.New<CommonShipment>();
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_RL_NKFreightRateDestination = "AUMEL";

			var subShipment = Factory.New<CommonShipment>();

			var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, subShipment);
			AssertEquals("AUMEL", adapter.RateDestination.Code);
		}

		#endregion

		#region ISpotRate members

		public void TestSellSpotRateInfo()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();

			var currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			masterShipment.JS_RX_NKFrtRateCurrency = currency;
			masterShipment.JS_UnitFreightRate = 0m;

			ISpotRate adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);

			AssertEquals(0m, adapter.SellSpotRateInfo.Rate.Amount);
			AssertEquals(currency, adapter.SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.StandardRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.ClientRate, adapter.SellSpotRateInfo.AutoratedValueType);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "CCC";
			jobHeader.JH_GE = department.PK;
			jobHeader.JH_ParentID = masterShipment.PK;

			masterShipment.JS_UnitFreightRate = 5m;
			AssertEquals(5m, adapter.SellSpotRateInfo.Rate.Amount);
			AssertEquals(currency, adapter.SellSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.SellSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.SpotRate, adapter.SellSpotRateInfo.AutoratedValueType);
		}

		public void TestCostSpotRateInfo()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();

			var currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			masterShipment.JS_RX_NKFrtRateCurrency = currency;
			masterShipment.JS_FreightCostRate = 0m;

			ISpotRate adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);

			AssertEquals(0m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals(currency, adapter.CostSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.StandardRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.Cost, adapter.CostSpotRateInfo.AutoratedValueType);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "CCC";
			jobHeader.JH_GE = department.PK;
			jobHeader.JH_ParentID = masterShipment.PK;

			masterShipment.JS_FreightCostRate = 5m;
			AssertEquals(5m, adapter.CostSpotRateInfo.Rate.Amount);
			AssertEquals(currency, adapter.CostSpotRateInfo.Rate.Currency.Code);
			AssertEquals(Core.Constants.FreightRateAutoratingModes.Code.FreightPlusRate, adapter.CostSpotRateInfo.AutoratedMode);
			AssertEquals(AutoratedValueType.NegotiatedCost, adapter.CostSpotRateInfo.AutoratedValueType);
		}

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		public void TestConditionsSupporter()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();

			IAutoRating adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);
			var adapterWithConditions = adapter as IAutoRatingFreightConditionsSupportable;
			AssertNotNull(adapterWithConditions);

			var conditionsSupporter = adapterWithConditions.ConditionsSupporter;
			AssertNotNull(conditionsSupporter);
			AssertType<ShipmentRateLineConditionsSupporter>(conditionsSupporter);
			AssertEquals(masterShipment.PK, conditionsSupporter.ObjectToWrap.PK);
		}

		#endregion

		#region Measures

		public void TestRateableMeasures_Packages()
		{
			var consol = CreateBuyersConsolConsol();
			var leadShipment = CreateBuyerConsolLeadShipment(consol);
			var subShipment = CreateColoadShipment(consol, leadShipment);

			consol.AddContainer("20GP", count: 2, containerMode: "BCN", packLines: new[]
			{
				leadShipment.AddPackLine(count: 10, packType: "PLT", weight: 100, volume: 1),
				subShipment.AddPackLine(count: 20, packType: "PLT", weight: 200, volume: 2),
				subShipment.AddPackLine(count: 5, packType: "BOX", weight: 50, volume: 0.5m)
			});

			consol.AddContainer("40GP", count: 4, containerMode: "BCN", packLines: new[]
			{
				leadShipment.AddPackLine(count: 40, packType: "PLT", weight: 400, volume: 4),
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(leadShipment, null);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			CombineAssertions(() =>
			{
				AssertEquals("lead shipment weight is from lead and sub shipment", 750m, measures.GetActual(MeasureType.Weight));
				AssertEquals("lead shipment volume is from lead and sub shipment", 7.5m, measures.GetActual(MeasureType.Volume));
				AssertEquals("lead shipment packages is from lead and sub shipment", 75m, measures.GetActual(MeasureType.Package));
			});
		}

		public void TestRateableMeasures_Containers_MultiplePackLinesWithMultipleCommodities_Case_1()
		{
			// Consol
			//		Container1	20GP	ALUM
			//		Container2  20GP	COIN
			//		Container3	20GP	FISH
			//
			//	Shipment1 (BCN lead)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of EFRT
			//	Shipment2 (BCN related)
			//		Container1	10 Packs of GLAS
			//		Container2	20 Packs of GLUE
			//	Shipment3 (FCL)
			//		Container1	10 Packs of FELT
			//		Container2	20 Packs of DAIR
			//		Container3  30 Packs of FISH
			//
			//	Expectation:
			//		1x20GP ALUM		<- Container commodity overrides packline commodity
			//		1x20GP COIN		<- Container commodity overrides packline commodity

			var consol = CreateBuyersConsolConsol();
			var shipment1 = CreateBuyerConsolLeadShipment(consol);
			var shipment2 = CreateColoadShipment(consol, shipment1);
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;

			consol.AddContainer(number: "C1", commodity: "ALUM", packLines: new[]
			{
				shipment1.AddPackLine(commodity: "GAME"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GAME"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "FELT")
			});
			consol.AddContainer(number: "C2", commodity: "COIN", packLines: new[]
			{
				shipment1.AddPackLine(commodity: "EFRT"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GLUE"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "DAIR")
			});
			consol.AddContainer(number: "C3", commodity: "FISH", packLines: new[]
			{
				shipment3.AddPackLine(commodity: "FISH")
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(shipment1, shipment1);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetContainerGroups();
			var actual = GetContainersAsString(containers);
			AssertArrayEqualsByElements(
				[
					"C1 20GP ALUM",
					"C2 20GP COIN"
				],
				actual.ToArray());
		}

		public void TestRateableMeasures_Containers_MultiplePackLinesWithMultipleCommodities_Case_2()
		{
			// Consol
			//		Container1	20GP
			//		Container2  20GP
			//		Container3	20GP	FISH
			//
			//	Shipment1 (BCN lead)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of EFRT
			//	Shipment2 (BCN related)
			//		Container1	10 Packs of GLAS
			//		Container2	20 Packs of GLUE
			//	Shipment3 (FCL)
			//		Container1	10 Packs of FELT
			//		Container2	20 Packs of DAIR
			//		Container3  30 Packs of FISH
			//
			//	Expectation:
			//		1x20GP			<-	Shipment 1 GAME + Shipment 2 GLAS = {BLANK} commodity, i.e. ambiguous
			//		1x20GP			<-  Shipment 1 EFRT + Shipment 2 GLUE = {BLANK} commodity, i.e. ambiguous

			var consol = CreateBuyersConsolConsol();
			var shipment1 = CreateBuyerConsolLeadShipment(consol);
			var shipment2 = CreateColoadShipment(consol, shipment1);
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;

			consol.AddContainer(number: "C1", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "GAME"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GLAS"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "FELT")
			});
			consol.AddContainer(number: "C2", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "EFRT"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GLUE"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "DAIR")
			});
			consol.AddContainer(number: "C3", commodity: "FISH", packLines: new[]
			{
				shipment3.AddPackLine(commodity: "FISH")
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(shipment1, shipment1);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetContainerGroups();
			var actual = GetContainersAsString(containers);
			AssertArrayEqualsByElements(
				[
					"C1 20GP ",
					"C2 20GP "
				],
				actual.ToArray());
		}

		public void TestRateableMeasures_Containers_MultiplePackLinesWithMultipleCommodities_Case_3()
		{
			// Consol
			//		Container1	20GP
			//		Container2  20GP
			//		Container3	20GP	FISH
			//
			//	Shipment1 (BCN lead)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of EFRT
			//	Shipment2 (BCN related)
			//		Container1	10 Packs of GAME
			//		Container2	20 Packs of GLUE
			//	Shipment3 (FCL)
			//		Container1	10 Packs of FELT
			//		Container2	20 Packs of DAIR
			//		Container3  30 Packs of FISH
			//
			//	Expectation:
			//		1x20GP GAME		<-	Shipment 1 GAME + Shipment 2 GAME = GAME commodity
			//		1x20GP			<-  Shipment 1 EFRT + Shipment 2 GLUE = {BLANK} commodity, i.e. ambiguous

			var consol = CreateBuyersConsolConsol();
			var shipment1 = CreateBuyerConsolLeadShipment(consol);
			var shipment2 = CreateColoadShipment(consol, shipment1);
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;

			consol.AddContainer(number: "C1", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "GAME"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GAME"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "FELT")
			});
			consol.AddContainer(number: "C2", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "EFRT"),	// BCN Lead
				shipment2.AddPackLine(commodity: "GLUE"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "DAIR")
			});
			consol.AddContainer(number: "C3", commodity: "FISH", packLines: new[]
			{
				shipment3.AddPackLine(commodity: "FISH")
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(shipment1, shipment1);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetContainerGroups();

			var actual = GetContainersAsString(containers);
			AssertArrayEqualsByElements(
				[
					"C1 20GP GAME",
					"C2 20GP "
				],
				actual.ToArray());
		}

		public void TestRateableMeasures_Containers_MultiplePackLinesWithMultipleCommodities_Case_4()
		{
			// Consol
			//		Container1	20GP
			//		Container2  20GP
			//		Container3	20GP	ZZZ
			//
			//	Shipment1 (BCN lead)
			//		Container1	10 Packs of XXX
			//		Container2	20 Packs
			//	Shipment2 (BCN related)
			//		Container1	10 Packs
			//		Container2	20 Packs of YYY
			//	Shipment3 (FCL)
			//		Container1	10 Packs of ZZZ
			//		Container2	20 Packs of ZZZ
			//		Container3  30 Packs of ZZZ
			//
			//	Expectation:
			//		1x20GP XXX		<-	Shipment 1 XXX + Shipment 2 {BLANK} = XXX commodity
			//		1x20GP YYY		<-  Shipment 1 {BLANK} + Shipment 2 {YYY} = YYY commodity

			Factory.New<RefCommodityCode>().RH_Code = "XXX";
			Factory.New<RefCommodityCode>().RH_Code = "YYY";
			Factory.New<RefCommodityCode>().RH_Code = "ZZZ";

			var consol = CreateBuyersConsolConsol();
			var shipment1 = CreateBuyerConsolLeadShipment(consol);
			var shipment2 = CreateColoadShipment(consol, shipment1);
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;

			consol.AddContainer(number: "C1", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: "XXX"),	// BCN Lead
				shipment2.AddPackLine(commodity: ""),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "ZZZ")
			});
			consol.AddContainer(number: "C2", commodity: ZString.Empty, packLines: new[]
			{
				shipment1.AddPackLine(commodity: ""),	// BCN Lead
				shipment2.AddPackLine(commodity: "YYY"),	// BCN CoLoad
				shipment3.AddPackLine(commodity: "ZZZ")
			});
			consol.AddContainer(number: "C3", commodity: "ZZZ", packLines: new[]
			{
				shipment3.AddPackLine(commodity: "ZZZ")
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(shipment1, shipment1);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetContainerGroups();

			var actual = GetContainersAsString(containers);
			AssertArrayEqualsByElements(
				[
					"C1 20GP XXX",
					"C2 20GP YYY"
				],
				actual.ToArray());
		}

		public void TestRateableMeasures_Containers_LeadShipmentNotAttachedToConsol()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_TransportMode = "SEA";
			masterShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			masterShipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			masterShipment.JS_RL_NKOrigin = "USLAX";
			masterShipment.JS_RL_NKDestination = "AUSYD";

			var consol = CreateBuyersConsolConsol("USLAX", "AUSYD", "SEA");
			var subShipment1 = CreateColoadShipment(consol, masterShipment);
			var otherShipment = consol.Shipments.AddNew();
			otherShipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var ship1Pack1 = subShipment1.AddPackLine();
			var ship1Pack2 = subShipment1.AddPackLine();

			var ship2Pack1 = otherShipment.AddPackLine();
			var ship2Pack2 = otherShipment.AddPackLine();

			consol.AddContainer(number: "C1", commodity: "ALUM", packLines: new[]
			{
				ship1Pack1,
			});
			consol.AddContainer(commodity: "COIN", packLines: new[]
			{
				ship1Pack2,
				ship2Pack1,
			});
			consol.AddContainer(commodity: "HAZ", packLines: new[]
			{
				ship2Pack2,
			});

			var adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, subShipment1);
			var measures = (RateableMeasureSet)adapter.RateableMeasures;
			var containers = measures.GetContainerGroups();
			var actual = GetContainersAsString(containers);
			AssertArrayEqualsByElements(
				[
					"C1 20GP ALUM",
					" 20GP COIN"
				],
				actual.ToArray());
		}

		IEnumerable<string> GetContainersAsString(IEnumerable<RateableMeasureSet.ContainerGroup> groups)
		{
			var containers = new List<string>();

			foreach (var group in groups)
			{
				var containerTypePK = group.ContainerTypePK;
				var container = Factory.Load<RefContainer>(containerTypePK);

				containers.Add(group.ContainerNumber + " " + container.RC_Code + " " + group.CommodityCode);
			}

			return containers;
		}

		public void TestRateableMeasures_LowestBill()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_TransportMode = "SEA";
			masterShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var consol = CreateBuyersConsolConsol("USLAX", "AUSYD", "SEA");
			var subShipment1 = CreateColoadShipment(consol, masterShipment);
			var subShipment2 = CreateColoadShipment(consol, masterShipment);

			var adapterForMasterShipment = new ConsolLeadShipmentRatingAdapter(masterShipment, masterShipment);
			var measuresForMasterShipment = (RateableMeasureSet)adapterForMasterShipment.RateableMeasures;
			AssertEquals(2m, measuresForMasterShipment.LowestBill);

			var adapterForSubShipment1 = new ConsolLeadShipmentRatingAdapter(masterShipment, subShipment1);
			var measuresForSubShipment1 = (RateableMeasureSet)adapterForSubShipment1.RateableMeasures;
			AssertEquals(1m, measuresForSubShipment1.LowestBill);

			var adapterForSubShipment2 = new ConsolLeadShipmentRatingAdapter(masterShipment, subShipment2);
			var measuresForSubShipment2 = (RateableMeasureSet)adapterForSubShipment2.RateableMeasures;
			AssertEquals(1m, measuresForSubShipment2.LowestBill);

			var adapterForNoSubShipment = new ConsolLeadShipmentRatingAdapter(masterShipment, null);
			var measuresForNoSubShipment = (RateableMeasureSet)adapterForNoSubShipment.RateableMeasures;
			AssertEquals(2m, measuresForNoSubShipment.LowestBill);

			Assert("This test uses FluentAssertions", true);
		}

		#endregion

		#region IJobDataUpdater

		public void TestUpdateClientContractNumber_LeadShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			AssertUpdateClientContractNumber(masterShipment, null, masterShipment);
		}

		public void TestUpdateClientContractNumber_SubShipment()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var subShipment = Factory.New<CommonShipment>();
			AssertUpdateClientContractNumber(masterShipment, subShipment, subShipment);
		}

		void AssertUpdateClientContractNumber(CommonShipment masterShipment, CommonShipment subShipment, CommonShipment autoRatedFor)
		{
			AssertContainsExactElementsInAnyOrder
			(
				"Pre-condition",
				new[] { autoRatedFor.JobNumber },
				(new ConsolLeadShipmentRatingAdapter(masterShipment, subShipment)).AutoRatedFor.Cast<CommonShipment>().Select(x => x.JobNumber)
			);

			// blank + BBB = BBB
			AssertUpdateClientContractNumber(
				existingNumber: "",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + BBB = AAA
			AssertUpdateClientContractNumber(
				existingNumber: "AAA",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + AAA = AAA
			AssertUpdateClientContractNumber(
				existingNumber: "AAA",
				newNumbers: new[] { "AAA" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + empty => AAA
			AssertUpdateClientContractNumber(
				existingNumber: "AAA",
				newNumbers: System.Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + blank => AAA
			AssertUpdateClientContractNumber(
				existingNumber: "AAA",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + many = AAA
			AssertUpdateClientContractNumber(
				existingNumber: "AAA",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// blank + BBB x 3 = BBB
			AssertUpdateClientContractNumber(
				existingNumber: "",
				newNumbers: new[] { "BBB", "BBB", "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// blank + multiple = error
			AssertUpdateClientContractNumber(
				existingNumber: "",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				existingNumber: "",
				newNumbers: System.Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				existingNumber: "",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			void AssertUpdateClientContractNumber(
				string existingNumber,
				IEnumerable<string> newNumbers,
				DataUpdateResult expectedResult,
				string expectedNumber)
			{
				ErrorReporter.Clear();

				var jobHeader = new JobHeader.Loader(autoRatedFor).TryLoadOrCreate();
				if (existingNumber != null)
				{
					jobHeader.JH_ClientContractNumber = existingNumber;
				}

				var jobDataUpdater = (IJobDataUpdater)(new ConsolLeadShipmentRatingAdapter(masterShipment, subShipment));
				var result = jobDataUpdater.UpdateClientContractNumber(newNumbers);

				var actualHasError = ErrorReporter.TotalErrorCount > 0;

				CombineAssertions(() =>
				{
					AssertEquals("Should update successfully", expectedResult, result);
					AssertEquals("Number", expectedNumber, jobHeader.JH_ClientContractNumber);
				});
			}
		}

		#endregion

		#region Apportion

		public void TestOnAutoRated_ShouldUpdateChargesAmountBasedOnShipmentWeightComparedToBcnWeight()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var sub1 = consol.Shipments.AddNew();
			sub1.JS_TransportMode = Constants.TransportModes.Sea;
			sub1.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var sub2 = consol.Shipments.AddNew();
			sub2.JS_TransportMode = Constants.TransportModes.Sea;
			sub2.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var lead = consol.Shipments.AddNew();
			lead.JS_TransportMode = Constants.TransportModes.Sea;
			lead.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			lead.CoLoadShipments.Add(sub1);
			lead.CoLoadShipments.Add(sub2);

			lead.JS_DocumentedWeight = 2000m;
			sub1.JS_DocumentedWeight = 500m;
			sub2.JS_DocumentedWeight = 2500m;

			// Apportion to Lead shipment
			var adapter = new ConsolLeadShipmentRatingAdapter(lead, lead);
			var charge = new Mock<IAutoRatedCharge>();
			charge.Setup(c => c.CostSell).Returns(CostSell.Revenue);
			adapter.OnAutoRated(new[] { charge.Object });
			charge.Verify(c => c.Multiply(0.4m), Times.Once, "The portion of the lead shipment is 40% (shipment 2000 KG vs total 5000 KG) and thus the calculated amount must be multiplied by 0.4");

			// Apportion to Sub1 shipment
			adapter = new ConsolLeadShipmentRatingAdapter(lead, sub1);
			charge = new Mock<IAutoRatedCharge>();
			charge.Setup(c => c.CostSell).Returns(CostSell.Revenue);
			adapter.OnAutoRated(new[] { charge.Object });
			charge.Verify(c => c.Multiply(0.1m), Times.Once, "The portion of the sub shipment is 10% (shipment 500 KG vs total 5000 KG) and thus the calculated amount must be multiplied by 0.1");

			// Apportion to Sub2 shipment
			adapter = new ConsolLeadShipmentRatingAdapter(lead, sub2);
			charge = new Mock<IAutoRatedCharge>();
			charge.Setup(c => c.CostSell).Returns(CostSell.Revenue);
			adapter.OnAutoRated(new[] { charge.Object });
			charge.Verify(c => c.Multiply(0.5m), Times.Once, "The portion of the sub shipment is 50% (shipment 2500 KG vs total 5000 KG) and thus the calculated amount must be multiplied by 0.5");

			// No apportion
			adapter = new ConsolLeadShipmentRatingAdapter(lead, null);
			charge = new Mock<IAutoRatedCharge>();
			charge.Setup(c => c.CostSell).Returns(CostSell.Revenue);
			adapter.OnAutoRated(new[] { charge.Object });
			charge.Verify(c => c.Multiply(It.IsAny<ZDecimal>()), Times.Never, "No shipment for apportion provided. The amount should not be updated.");

			Assert(true);
		}

		public void TestOnAutoRated_UnitFactorIsBcnOrScn_ShouldNotUpdateChargesAmount()
		{
			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

			var sub = consol.Shipments.AddNew();
			sub.JS_TransportMode = Constants.TransportModes.Sea;
			sub.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var lead = consol.Shipments.AddNew();
			lead.JS_TransportMode = Constants.TransportModes.Sea;
			lead.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			lead.CoLoadShipments.Add(sub);

			lead.JS_DocumentedWeight = 400m;
			sub.JS_DocumentedWeight = 100m;

			void TestSetup(string unitFactor, CalculatorType calculator, string unit, decimal expectedShare, string message)
			{
				var adapter = new ConsolLeadShipmentRatingAdapter(lead, lead);
				var charge = new Mock<IAutoRatedCharge>();
				charge.Setup(c => c.CostSell).Returns(CostSell.Revenue);
				charge.Setup(c => c.UnitFactor).Returns(unitFactor);
				charge.Setup(c => c.CalculatorType).Returns(calculator);
				charge.Setup(c => c.ChargeUnit).Returns(unit);
				adapter.OnAutoRated(new[] { charge.Object });

				charge.Verify(c => c.Multiply(expectedShare), Times.Once, message);
			}

			TestSetup(null, CalculatorType.Unit, "KG", 0.8m, "We always apportion if it is not instructed not to apportion via Unit Factor");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "CN", 0.8m, "No Apportion unit factor is applicable only to certain units (weird product requirement). CN unit is not one of them.");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "CN", 0.8m, "No Apportion unit factor is applicable only to certain units (weird product requirement). CN unit is not one of them.");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.FlatPlusPerUnit, "KG", 0.8m, "No Apportion unit factor is applicable only to certain calculators (weird product requirement). FPU calculator is not one of them.");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.FlatPlusPerUnit, "KG", 0.8m, "No Apportion unit factor is applicable only to certain calculators (weird product requirement). FPU calculator is not one of them.");

			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "HB", 1m, "No Apportion unit factor is applicable to HB unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "LW", 1m, "No Apportion unit factor is applicable to LW unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "KG", 1m, "No Apportion unit factor is applicable to KG unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "M3", 1m, "No Apportion unit factor is applicable to M3 unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "SV", 1m, "No Apportion unit factor is applicable to SV unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "HR", 1m, "No Apportion unit factor is applicable to HR unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "DY", 1m, "No Apportion unit factor is applicable to DY unit");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Unit, "WK", 1m, "No Apportion unit factor is applicable to WK unit");

			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "HB", 1m, "No Apportion unit factor is applicable to HB unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "LW", 1m, "No Apportion unit factor is applicable to LW unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "KG", 1m, "No Apportion unit factor is applicable to KG unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "M3", 1m, "No Apportion unit factor is applicable to M3 unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "SV", 1m, "No Apportion unit factor is applicable to SV unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "HR", 1m, "No Apportion unit factor is applicable to HR unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "DY", 1m, "No Apportion unit factor is applicable to DY unit");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Unit, "WK", 1m, "No Apportion unit factor is applicable to WK unit");

			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Flat, null, 1m, "No Apportion unit factor is applicable to Flat calculator");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Flat, null, 1m, "No Apportion unit factor is applicable to Flat calculator");
			TestSetup(UnitFactorList.Codes.BCN, CalculatorType.Combined, "KG", 1m, "No Apportion unit factor is applicable to CMB calculator");
			TestSetup(UnitFactorList.Codes.SCN, CalculatorType.Combined, "KG", 1m, "No Apportion unit factor is applicable to CMB calculator");

			Assert(true);
		}

		#endregion

		CommonConsol CreateBuyersConsolConsol(string origin = "UAIEV", string destination = "AUSYD", string transportMode = "SEA")
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			return consol;
		}

		CommonShipment CreateBuyerConsolLeadShipment(CommonConsol consol, string transportMode = "SEA")
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;

			return shipment;
		}

		CommonShipment CreateColoadShipment(CommonConsol consol, CommonShipment leadShipment)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = leadShipment.JS_TransportMode;
			shipment.JS_PackingMode = leadShipment.JS_PackingMode;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment.JS_RL_NKOrigin = leadShipment.JS_RL_NKOrigin;
			shipment.JS_RL_NKDestination = leadShipment.JS_RL_NKDestination;
			shipment.JS_JS_ColoadMasterShipment = leadShipment.PK;

			return shipment;
		}

		public void TestChargeCodeGroups_BCN()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			IAutoRating adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);
			var expectedChargeCodeGroups = new[] { "DST", "FRT", "INS", "LOD", "UNL" };
			AssertContainsExactElementsInAnyOrder(expectedChargeCodeGroups, adapter.ChargeCodeGroups);
		}

		#endregion

		#region SCN
		public void TestChargeCodeGroups_SCN()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.ShippersConsolLead;
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.ShippersConsolLead;

			IAutoRating adapter = new ConsolLeadShipmentRatingAdapter(masterShipment, shipment);
			var expectedChargeCodeGroups = new[] { "ORG", "FRT", "INS", "LOD", "UNL" };
			AssertContainsExactElementsInAnyOrder(expectedChargeCodeGroups, adapter.ChargeCodeGroups);
		}
		#endregion
	}
}
