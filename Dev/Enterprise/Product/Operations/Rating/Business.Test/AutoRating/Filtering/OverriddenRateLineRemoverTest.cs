using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Rating.Business.Testing.FreightAutoRaterJobUpdatingTests;

namespace Enterprise.Rating.Business.Testing
{
	public class OverriddenRateLineRemoverTest : TestCaseWithFactory
	{
		#region Different Measure Dimensions

		public void TestWithDifferentMeasureDimensions()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);
			var line2 = testRate.AddRateEntry("ORG", "AIR", "AUSYD", "").AddRateLine("ODOC", FlatCalculator.Code);

			AssertRemove(line2, remover, line1, line2);

			line1.TL_OP_ProductNumber = Helper.NewOrgSupplierPart(testRate.Header).PK;
			AssertRemove(null, remover, line1, line2);

			line2.TL_OP_ProductNumber = line1.TL_OP_ProductNumber;
			AssertRemove(line2, remover, line1, line2);
		}

		#endregion

		#region Different Freight Legs

		public void TestWithDifferentFreightLegs()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetVia(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var remover = new OverriddenRateLineRemover(criteria, false);

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN").RateLines[0];
			var line2 = testRate.AddRateEntry("AIR", "LSE", "SGSIN", "USLAX").RateLines[0];
			var line3 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "SG").RateLines[0];
			var line4 = testRate.AddRateEntry("AIR", "LSE", "SG", "USLAX").RateLines[0];

			AssertRemove(null, remover, line1, line2);
			AssertRemove(line1, remover, line1, line3);
			AssertRemove(line2, remover, line2, line4);
		}

		#endregion

		#region Test AgencyCalculatorComparer

		public void TestAgencyCalculatorComparer()
		{
			var criteria = new TestRatingCriteria("USLAX", "AUSYD", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.First.Value.Equals(new AgencyCalculatorComparer()));

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("DST", "AIR", "", "AUSYD");
			var line1 = entry.AddRateLine("CCLR", AgencyCalculator.Code);
			line1.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			line1.Calculator.MessageSubType = "FRM";
			var line2 = entry.AddRateLine("CCLR", AgencyCalculator.Code);
			line2.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			var line3 = entry.AddRateLine("CCLR", AgencyCalculator.Code);
			line3.Calculator.MessageSubType = "FRM";
			var line4 = entry.AddRateLine("CCLR", AgencyCalculator.Code);

			AssertRemove(line1, remover, line1, line2, line3, line4);
			AssertRemove(line1, remover, line4, line3, line2, line1);
			AssertRemove(line2, remover, line2, line3, line4);
			AssertRemove(line2, remover, line4, line3, line2);
			AssertRemove(line3, remover, line3, line4);
			AssertRemove(line3, remover, line4, line3);
		}

		#endregion

		#region Test CostsProviderComparer

		public void TestCostsProviderComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(!comparers.Contains(new CostsProviderComparer(criteria)));
			remover = new OverriddenRateLineRemover(criteria, true);
			comparers = remover.GetComparers();
			Assert(comparers.Contains(new CostsProviderComparer(criteria)));
			Assert(comparers.Find(new CostsProviderComparer(criteria)).Previous.Value.Equals(new GatewayAgentOrderComparer(criteria)));

			var costs1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costs1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine("OFUMI", FlatCalculator.Code);

			var costs2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = costs2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine("OFUMI", FlatCalculator.Code);

			var costs3 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry3 = costs3.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line3 = entry3.AddRateLine("OFUMI", FlatCalculator.Code);

			AssertRemove(null, remover, line1, line2, line3);
			AssertRemove(null, remover, line3, line2, line1);

			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(costs3.Header, "provider3"));
			AssertRemove(line3, remover, line1, line2, line3);
			AssertRemove(line3, remover, line2, line3, line1);

			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(costs3.Header, "provider3"), null, GetTestOrgWithSourceFromHeader(costs1.Header, "provider1"));
			var lines = new List<IRateLine> { line1, line2, line3 };
			remover.Remove(lines);
			AssertEquals(2, lines.Count);
			AssertEquals(line1, lines[0]);
			AssertEquals(line3, lines[1]);

			lines = new List<IRateLine> { line3, line2, line1 };
			remover.Remove(lines);
			AssertEquals(2, lines.Count);
			AssertEquals(line3, lines[0]);
			AssertEquals(line1, lines[1]);

			var serviceInfo = new JobServiceInfo(true, ChargeCodeGroupList.Codes.Origin, Core.Constants.FreightServiceType.Codes.Fumigation, "Fumigation", 1m, null, costs2.Header);

			criteria.JobServices = new JobServicesCollection();
			criteria.JobServices.Add(serviceInfo);
			AssertRemove(line2, remover, line1, line2, line3);
		}

		#endregion

		#region Test Carrier Service Level Comparer

		public void TestCarrierServiceLevelComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new ColumnComparer(RateEntrySchema.TI_PL_NKCarrierServiceLevel)));
			remover = new OverriddenRateLineRemover(criteria, true);
			comparers = remover.GetComparers();
			Assert(comparers.Contains(new ColumnComparer(RateEntrySchema.TI_PL_NKCarrierServiceLevel)));

			var costs1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costs1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine("OFUMI", FlatCalculator.Code);

			var costs2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = costs2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine("OFUMI", FlatCalculator.Code);

			var costs3 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry3 = costs3.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line3 = entry3.AddRateLine("OFUMI", FlatCalculator.Code);

			AssertRemove(null, remover, line1, line2, line3);
			AssertRemove(null, remover, line3, line2, line1);

			entry2.TI_PL_NKCarrierServiceLevel = "STD";
			AssertRemove(line2, remover, line1, line2, line3);
			AssertRemove(line2, remover, line3, line2, line1);
		}

		#endregion

		#region Test IncoTermComparer

		public void TestIncoTermComparer_Disabled()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var options = new NotApplicableRateLineRemover.FilterOptions() { IsCosting = false, DisablePaymentTermsFilter = true };
			var remover = new OverriddenRateLineRemover(criteria, options);

			var incotermComparers = remover.GetComparers().OfType<IncoTermComparer>().ToList();
			AssertEquals("There is no incoterm comparer", 0, incotermComparers.Count);
		}

		public void TestIncoTermComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var comparers = remover.GetComparers();
			Assert(!comparers.Contains(new IncoTermComparer()));
			remover = new OverriddenRateLineRemover(criteria, false);
			comparers = remover.GetComparers();
			Assert(comparers.Contains(new IncoTermComparer()));
			Assert(comparers.Find(new IncoTermComparer()).Next.Value.Equals(new RateTypeComparer(criteria)));

			var rateCNR = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rateCNR.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var lineCNR = entry1.AddRateLine("ODOC", FlatCalculator.Code);

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);

			var rateCNE = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry3 = rateCNE.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var lineCNE = entry3.AddRateLine("ODOC", FlatCalculator.Code);

			criteria.Consignor = rateCNR.Header;
			criteria.Consignee = rateCNE.Header;

			criteria.JobDirection = Directions.Export;
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));
			AssertRemove(lineCNE, remover, lineCNR, line2, lineCNE);
			AssertRemove(lineCNE, remover, lineCNE, line2, lineCNR);

			criteria.ClearCache();
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "FOB"));
			AssertRemove(lineCNR, remover, lineCNR, line2, lineCNE);
			AssertRemove(lineCNR, remover, lineCNE, line2, lineCNR);

			criteria.ClearCache();
			criteria.LocalClient = rate2.Header;
			AssertRemove(line2, remover, lineCNR, line2, lineCNE);

			criteria.ClearCache();
			criteria.ExportBroker = rate2.Header;
			AssertRemove(lineCNR, remover, lineCNR, line2, lineCNE);
		}

		#endregion

		#region Test TransportProviderConsortiumComparer

		public void TestTransportProviderConsortiumComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new TransportProviderConsortiumComparer()));
			Assert(comparers.Find(new TransportProviderConsortiumComparer()).Previous.Value.Equals(new TransportProviderComparer()));

			var consortiumOrgProxy1 = Helper.NewOrgHeader();
			consortiumOrgProxy1.OH_IsShippingConsortium = true;
			var consortiumOrgProxy2 = Helper.NewOrgHeader();
			consortiumOrgProxy2.OH_IsShippingConsortium = true;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var noProviderEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var consortium1Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium1Entry.TI_OH_TransportProvider = consortiumOrgProxy1.PK;

			var consortium2Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium2Entry.TI_OH_TransportProvider = consortiumOrgProxy2.PK;

			var concreteEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			concreteEntry.TI_OH_TransportProvider = Helper.NewOrgHeader().PK;

			var line1 = noProviderEntry.RateLines[0];
			var line2 = consortium1Entry.RateLines[0];
			var line3 = consortium2Entry.RateLines[0];
			var line4 = concreteEntry.RateLines[0];

			AssertRemove(line4, remover, line1, line2, line3, line4);
			AssertRemove(line4, remover, line4, line3, line2, line1);

			AssertException(
				string.Format(TransportProviderConsortiumComparer.GetExceptionMessage(consortiumOrgProxy1.OH_Code, consortiumOrgProxy2.OH_Code)),
				remover, line1, line2, line3);

			AssertException(
				string.Format(TransportProviderConsortiumComparer.GetExceptionMessage(consortiumOrgProxy2.OH_Code, consortiumOrgProxy1.OH_Code)),
				remover, line3, line2, line1);
		}

		public void TestTransportProviderConsortiumComparer_SameOrg()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new TransportProviderConsortiumComparer()));
			Assert(comparers.Find(new TransportProviderConsortiumComparer()).Previous.Value.Equals(new TransportProviderComparer()));

			var consortiumOrgProxy1 = Helper.NewOrgHeader();
			consortiumOrgProxy1.OH_IsShippingConsortium = true;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var consortium1Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium1Entry.TI_OH_TransportProvider = consortiumOrgProxy1.PK;

			var consortium2Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium2Entry.TI_OH_TransportProvider = consortiumOrgProxy1.PK;

			var line1 = consortium1Entry.RateLines[0];
			var line2 = consortium2Entry.RateLines[0];

			var lines = new List<IRateLine>();
			lines.Add(line1);
			lines.Add(line2);
			remover.Remove(lines);
			AssertEquals("None removed, as both lines have the same consortium org", 2, lines.Count);
		}

		#endregion

		#region Test SupplierConsortiumComparer

		public void TestSupplierConsortiumComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new SupplierConsortiumComparer()));
			Assert(comparers.Find(new SupplierConsortiumComparer()).Previous.Value.Equals(new ColumnComparer(RateEntrySchema.TI_OH_Supplier)));

			var consortiumOrgProxy1 = Helper.NewOrgHeader();
			consortiumOrgProxy1.OH_IsShippingConsortium = true;
			var consortiumOrgProxy2 = Helper.NewOrgHeader();
			consortiumOrgProxy2.OH_IsShippingConsortium = true;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var noProviderEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var consortium1Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium1Entry.TI_OH_Supplier = consortiumOrgProxy1.PK;
			var consortium2Entry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			consortium2Entry.TI_OH_Supplier = consortiumOrgProxy2.PK;
			var concreteEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			concreteEntry.TI_OH_Supplier = Helper.NewOrgHeader().PK;

			var line1 = noProviderEntry.RateLines[0];
			var line2 = consortium1Entry.RateLines[0];
			var line3 = consortium2Entry.RateLines[0];
			var line4 = concreteEntry.RateLines[0];

			AssertRemove(line4, remover, line1, line2, line3, line4);
			AssertRemove(line4, remover, line4, line3, line2, line1);

			AssertException(
				string.Format(SupplierConsortiumComparer.GetExceptionMessage(consortiumOrgProxy1.OH_Code, consortiumOrgProxy2.OH_Code)),
				remover, line1, line2, line3);
			AssertException(
				string.Format(SupplierConsortiumComparer.GetExceptionMessage(consortiumOrgProxy2.OH_Code, consortiumOrgProxy1.OH_Code)),
				remover, line3, line2, line1);
		}

		#endregion

		#region Test ForwarderGroupComparer

		public void TestForwarderGroupComparer()
		{
			var agent = Helper.NewOrgHeader();
			var forwarderGroup = Helper.NewOrgHeader();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new ForwarderGroupComparer()));
			Assert(comparers.Find(new ForwarderGroupComparer()).Previous.Value.Equals(new SupplierConsortiumComparer()));

			var relatedParty = agent.AllRelatedParties.AddNew();
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderGroup;
			relatedParty.PR_OH_RelatedParty = forwarderGroup.PK;

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var noProviderEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var groupEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			groupEntry.TI_OH_Supplier = forwarderGroup.PK;
			var concreteEntry = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			concreteEntry.TI_OH_Supplier = agent.PK;

			var line1 = noProviderEntry.RateLines[0];
			var line2 = groupEntry.RateLines[0];
			var line3 = concreteEntry.RateLines[0];

			AssertRemove(line3, remover, line1, line2, line3);
			AssertRemove(line3, remover, line3, line2, line1);
		}

		#endregion

		#region Test TransitTimeComparer

		public void TestTransitTimeComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, Helper.Containers["20GP"], null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			Assert(comparers.Last.Value.Equals(new TransitTimeComparer(criteria)));

			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry10Days = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry10Days.TI_TransitTime = "10";
			var entry20Days = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry20Days.TI_TransitTime = "20";
			var entrySameDay = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entrySameDay.TI_TransitTime = "SMD";
			var entryOvernight = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entryOvernight.TI_TransitTime = "OVN";
			var entry1Day = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1Day.TI_TransitTime = "1";
			var entryBlank = testRate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entryBlank.TI_TransitTime = "";

			void SetCriteriaTransitTime(string transitTime)
			{
				var jobDatesProvider = new Mock<IJobDatesProvider>();
				jobDatesProvider.Setup(m => m.TransitTime).Returns(transitTime);

				criteria.JobDatesProvider = jobDatesProvider.Object;
				remover = new OverriddenRateLineRemover(criteria, false);
			}

			var entries = new[] { entry10Days, entry20Days, entrySameDay, entryOvernight, entry1Day, entryBlank };
			var lines = entries.Select(x => x.RateLines[0]).ToArray();
			var linesInReverseOrder = lines.Reverse().ToArray();

			void AssertRemoveWithTwoListOfLines(RateEntry entry)
			{
				var expectedLine = entry?.RateLines[0];
				AssertRemove(expectedLine, remover, lines);
				AssertRemove(expectedLine, remover, linesInReverseOrder);
			}

			SetCriteriaTransitTime("1");
			AssertRemoveWithTwoListOfLines(entry1Day);

			SetCriteriaTransitTime("2");
			AssertRemoveWithTwoListOfLines(entry1Day);

			SetCriteriaTransitTime("11");
			AssertRemoveWithTwoListOfLines(entry10Days);

			SetCriteriaTransitTime("15");
			AssertRemoveWithTwoListOfLines(entry10Days);

			SetCriteriaTransitTime("16");
			AssertRemoveWithTwoListOfLines(entry20Days);

			SetCriteriaTransitTime("");
			AssertRemoveWithTwoListOfLines(null);

			SetCriteriaTransitTime("SMD");
			AssertRemoveWithTwoListOfLines(entrySameDay);

			SetCriteriaTransitTime("OVN");
			AssertRemoveWithTwoListOfLines(entryOvernight);

			entries = new[] { entry10Days, entry20Days, entry1Day, entryBlank };
			lines = entries.Select(x => x.RateLines[0]).ToArray();
			linesInReverseOrder = lines.Reverse().ToArray();

			SetCriteriaTransitTime("SMD");
			AssertRemoveWithTwoListOfLines(entry1Day);

			SetCriteriaTransitTime("OVN");
			AssertRemoveWithTwoListOfLines(entry1Day);
		}

		#endregion

		#region Test RateTypeComparer

		public void TestRateTypeComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 100m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var org1 = Helper.NewOrgHeader(1);
			org1.OH_Code = "ORG1";
			var org2 = Helper.NewOrgHeader(1);
			org2.OH_Code = "ORG2";
			var org3 = Helper.NewOrgHeader(1);
			org3.OH_Code = "ORG3";

			Factory.Save();

			var rateOrg1 = Helper.NewClientRate(org1);
			var rateLineOrg1 = rateOrg1.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");
			var rateOrg2 = Helper.NewClientRate(org2);
			var rateLineOrg2 = rateOrg2.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");
			var rateOrg3 = Helper.NewClientRate(org3);
			var rateLineOrg3 = rateOrg3.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX").AddRateLine("ODOC");

			var defaultLevelOrg1 = org1.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevelOrg1.P7_ApplyGroupRate = false;
			var defaultLevelOrg2 = org2.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevelOrg2.P7_ApplyGroupRate = false;
			var defaultLevelOrg3 = org3.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			defaultLevelOrg3.P7_ApplyGroupRate = false;
			var orgRateTariffLevels = new[] { defaultLevelOrg1, defaultLevelOrg2, defaultLevelOrg3 };

			// relationships: ORG1 < ORG2 < ORG3
			org2.RelatedManagementSubsidiaryRelations.AddOrganisation(org1);
			org3.RelatedManagementSubsidiaryRelations.AddOrganisation(org2);

			var sourceList = new List<RateLine> { rateLineOrg1, rateLineOrg2, rateLineOrg3 };
			var expectedApplyGroupRates = new[] { false, false, false };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			var expectedLines = new[] { rateLineOrg1, rateLineOrg2, rateLineOrg3 };
			var message = "GIVEN all rates from different clients THEN all lines remains";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);

			defaultLevelOrg1.P7_ApplyGroupRate = true;
			expectedApplyGroupRates = new[] { true, false, false };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			expectedLines = new[] { rateLineOrg1 };
			message = @"GIVEN 3 and 1 IsClientSame but 1 has higher RateTypePriority THEN remove 3
GIVEN 2 and 1 IsClientSame but 1 has higher RateTypePriority THEN remove 2";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);

			defaultLevelOrg2.P7_ApplyGroupRate = true;
			expectedApplyGroupRates = new[] { true, true, false };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			expectedLines = new[] { rateLineOrg1 };
			message = @"GIVEN 3 and 2 IsClientSame but 2 is PrioritiseGroupClientRate THEN remove 3
GIVEN 2 and 1 IsClientSame but 1 has higher RateTypePriority THEN remove 2";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);

			//==== from here compare only lines from org2 and org3
			sourceList.Remove(rateLineOrg1);

			defaultLevelOrg2.P7_ApplyGroupRate = true;
			expectedApplyGroupRates = new[] { true, true, false };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			expectedLines = new[] { rateLineOrg2 };
			message = "GIVEN 3 and 2 IsClientSame but 2 is the PrioritiseGroupClientRate THEN remove 3";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);

			defaultLevelOrg3.P7_ApplyGroupRate = true;
			expectedApplyGroupRates = new[] { true, true, true };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			expectedLines = new[] { rateLineOrg2 };
			message = "GIVEN 3 and 2 IsClientSame but 2 is the PrioritiseGroupClientRate THEN remove 3";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);

			defaultLevelOrg2.P7_ApplyGroupRate = false;
			expectedApplyGroupRates = new[] { true, false, true };
			AssertApplicableGroupRates(expectedApplyGroupRates, orgRateTariffLevels);
			expectedLines = new[] { rateLineOrg2, rateLineOrg3 };
			message = @"GIVEN 3 and 2 are not IsClientSame - 2 is child to 3 but only 3 is group rate applicable.
Line2 is not group rate applicable to ORG3 and Line3 is not group rate applicable to ORG2 - so both lines remain";
			AssertRemoveInOrderAndReversedOrder(message, remover, expectedLines, sourceList);
		}

		void AssertRemoveInOrderAndReversedOrder(string message, OverriddenRateLineRemover remover, IEnumerable<RateLine> expectedLines, IEnumerable<RateLine> sourceList)
		{
			var list = sourceList.ToList();
			remover.Remove(list);
			AssertContainsExactElementsInAnyOrder(message, expectedLines, list);

			var reverseList = sourceList.Reverse().ToList();
			remover.Remove(reverseList);
			AssertContainsExactElementsInAnyOrder(message, expectedLines, list);
		}

		void AssertApplicableGroupRates(bool[] expected, OrgRateTariffLevel[] orgRateTariffLevels)
		{
			CombineAssertions("OrgHeaders should have group rate applicable values", () =>
				{
					AssertEquals(expected.Length, orgRateTariffLevels.Length);

					for (int i = 0; i < orgRateTariffLevels.Length; i++)
					{
						AssertEquals(expected[i], orgRateTariffLevels[i].P7_ApplyGroupRate);
					}
				}
			);
		}

		#endregion

		#region Test NamedAccountComparer

		public void TestRemove_RatesWithDifferentNamedAccounts_ShouldPreferOneWithJobNamedAccount()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, Helper.Containers["20GP"], null);
			var remover = new OverriddenRateLineRemover(criteria, false);

			var comparers = remover.GetComparers();
			AssertCollectionContains(
				"Lines must be compared by named accounts",
				comparers.FirstOrDefault(c => c.GetType().Name == "NamedAccountComparer"),
				comparers
			);

			var costing = Helper.NewCosting(null);
			var entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYS", "USLAX", string.Empty, "20GP");
			var entry2 = costing.AddRateEntry("FCL", "SEA", "AUSYS", "USLAX", string.Empty, "20GP");
			var entry3 = costing.AddRateEntry("FCL", "SEA", "AUSYS", "USLAX", string.Empty, "20GP");

			var entries = new[] { entry1, entry2, entry3 };
			var linesFromEntries = entries.Select(x => x.RateLines[0]);

			var lines = linesFromEntries.ToList();

			criteria.SetNamedAccount("CCC");

			entry1.NamedAccounts = new[] { "AAA" };
			entry2.NamedAccounts = new[] { "BBB" };
			entry3.NamedAccounts = new[] { "CCC" };

			var expectedLines = new[] { entry3.RateLines[0] };

			remover.Remove(lines);
			AssertContainsExactElementsInAnyOrder(
				"The expected lines after removal should only include the one with the correct named account",
				expectedLines,
				lines
			);

			lines = linesFromEntries.Reverse().ToList();
			remover.Remove(lines);
			AssertContainsExactElementsInAnyOrder(
				"The expected lines after removal should only include the one with the correct named account when reversed",
				expectedLines,
				lines
			);
		}

		#endregion

		#region Test PaymentTermOverrideComparer
		public void TestPaymentTermOverrideComparer()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 1, null, 100, "KG", 1m, "M3", null, "CCX");
			var remover = new OverriddenRateLineRemover(criteria, true);
			var comparers = remover.GetComparers();
			Assert(comparers.Contains(new PaymentTermOverrideComparer()));

			var clRate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = clRate1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1.TI_PaymentTerm = "CCX";
			var rateLine1 = entry1.AddRateLine("ODOC", FlatCalculator.Code);

			var clRate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2 = clRate2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry2.TI_PaymentTerm = string.Empty;
			var rateLine2 = entry2.AddRateLine("ODOC", FlatCalculator.Code);

			AssertRemove(rateLine1, remover, rateLine1, rateLine2);
		}

		#endregion

		#region TestGlobalRates

		public void TestRateTypeComparer_GlobalCosts()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costs1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);

			var costs2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = costs2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);

			var globalCosts = Helper.NewGlobalCosting(costs2.Header);
			var globalCostsEntry = globalCosts.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var globalCostLine = globalCostsEntry.AddRateLine(globalChargeCode.AC_Code, FlatCalculator.Code);
			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(costs2.Header, "provider3"));

			AssertRemove(line2, remover, line1, line2, globalCostLine);
			AssertRemove(line2, remover, line2, globalCostLine, line1);
		}

		public void TestRateTypeComparer_GlobalRates()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line2 = entry2.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);

			var globalRate = Helper.NewGlobalClientRate(rate2.Header);
			var globalRateEntry = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var globalRateLine = globalRateEntry.AddRateLine(globalChargeCode.AC_Code, FlatCalculator.Code);
			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(rate2.Header, "provider3"));

			AssertRemove(line2, remover, line1, line2, globalRateLine);
			AssertRemove(line2, remover, line2, globalRateLine, line1);
		}

		public void TestRateTypeComparer_GlobalTariff()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var companyTariff1 = Factory.New<CompanyTariff>();
			var entry1 = companyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line1 = entry1.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);

			var globalTariff = Factory.New<GlobalTariff>();
			var globalTariffEntry = globalTariff.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var globalTariffRateLine = globalTariffEntry.AddRateLine(globalChargeCode.AC_Code, FlatCalculator.Code);
			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(companyTariff1.Header, "provider3"));

			AssertRemove(line1, remover, line1, globalTariffRateLine);
			AssertRemove(line1, remover, globalTariffRateLine, line1);
		}

		public void TestRateTypeCompare_GlobalRatesAndCompanyTariff()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var globalTariff = Factory.New<GlobalTariff>();
			var globalTariffEntry = globalTariff.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var globalTariffRateLine = globalTariffEntry.AddRateLine(globalChargeCode.AC_Code, FlatCalculator.Code);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientRateEntry = clientRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var clientRateLine = clientRateEntry.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);
			criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(clientRate.Header, "provider3"));

			AssertRemove(clientRateLine, remover, clientRateLine, globalTariffRateLine);
			AssertRemove(clientRateLine, remover, globalTariffRateLine, clientRateLine);
		}

		public void TestRateTypeComparer_QuoteAndGlobalClientRate_GlobalSellRatesOverrideLocal()
		{
			AssertRateTypeComparer_QuoteAndGlobalRate_GlobalSellRatesOverrideLocal(globalSellRatesOverrideLocal: true, RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestRateTypeComparer_QuoteAndGlobalClientRate_NoGlobalSellRatesOverrideLocal()
		{
			AssertRateTypeComparer_QuoteAndGlobalRate_GlobalSellRatesOverrideLocal(globalSellRatesOverrideLocal: false, RatingConstants.RatingHeaderTypes.ClientRate);
		}

		public void TestRateTypeComparer_QuoteAndGlobalTariff_GlobalSellRatesOverrideLocal()
		{
			AssertRateTypeComparer_QuoteAndGlobalRate_GlobalSellRatesOverrideLocal(globalSellRatesOverrideLocal: true, RatingConstants.RatingHeaderTypes.Tariff);
		}

		public void TestRateTypeComparer_QuoteAndGlobalTariff_NoGlobalSellRatesOverrideLocal()
		{
			AssertRateTypeComparer_QuoteAndGlobalRate_GlobalSellRatesOverrideLocal(globalSellRatesOverrideLocal: false, RatingConstants.RatingHeaderTypes.Tariff);
		}

		void AssertRateTypeComparer_QuoteAndGlobalRate_GlobalSellRatesOverrideLocal(bool globalSellRatesOverrideLocal, string rateType)
		{
			using (RatingDataRegistry.Instance.GlobalSellRatesOverrideLocal.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, globalSellRatesOverrideLocal))
			{
				AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out AccChargeCode normalChargeCodeLinked, out AccChargeCode globalChargeCode);

				var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
				var remover = new OverriddenRateLineRemover(criteria, true);

				var client = Helper.NewOrgHeader(companyTariffDefault: 1);

				var quote = Helper.NewQuote(client);
				var quoteEntry = quote.AddRateEntry("ORG", "AIR", "AUSYD", "");
				var quoteRateLine = quoteEntry.AddRateLine(globalChargeCode.AC_Code, FlatCalculator.Code);

				var globalRate = GetGlobalRatingHeader(rateType, client);
				var globalRateEntry = globalRate.AddRateEntry("ORG", "AIR", "AUSYD", "");
				var globalRateLine = globalRateEntry.AddRateLine(normalChargeCodeLinked.AC_Code, FlatCalculator.Code);
				criteria.Creditors = Creditors.New(GetTestOrgWithSourceFromHeader(globalRate.Header, "provider3"));

				// quote Rate Line should take priority
				AssertRemove(expectedRemainingLine: quoteRateLine, remover, sourceLines: new[] { globalRateLine, quoteRateLine });
				AssertRemove(expectedRemainingLine: quoteRateLine, remover, sourceLines: new[] { quoteRateLine, globalRateLine });
			}
		}

		RatingHeader GetGlobalRatingHeader(string rateType, OrgHeader client)
		{
			switch (rateType)
			{
				case RatingConstants.RatingHeaderTypes.ClientRate:
					return Helper.NewGlobalClientRate(client);
				case RatingConstants.RatingHeaderTypes.Tariff:
					var tariff = Factory.New<GlobalTariff>();
					tariff.TH_GlobalRateLevel = 1;
					return tariff;
				default:
					throw new NotImplementedException();
			}
		}

		#endregion

		#region TestMatchingLocationComparer

		public void TestMatchingLocationComparer_FirstLoad()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstLoadLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_FirstLoadLRC = "";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_FirstLoad_TakesPriorityOver_LastDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetLastDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstLoadLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastDischargeLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_FirstLoad_TakesPriorityOver_FirstRouteSetLoad()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetFirstRouteSetLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstLoadLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_FirstRouteSetLoadPortLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_FirstLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetLastRouteSetDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstLoadLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastRouteSetDischargePortLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_LastDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetLastDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_LastDischargeLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastDischargeLRC = "";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_LastDischarge_TakesPriorityOver_FirstRouteSetLoad()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetLastDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetFirstRouteSetLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_LastDischargeLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_FirstRouteSetLoadPortLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_LastDischarge_TakesPriorityOver_LastRouteSetDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetLastDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetLastRouteSetDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_LastDischargeLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastRouteSetDischargePortLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_FirstRouteSetLoad()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstRouteSetLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstRouteSetLoadPortLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_FirstRouteSetLoadPortLRC = "";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_FirstRouteSetLoad_TakesPriorityOver_LastRouteSetDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetFirstRouteSetLoad(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			criteria.SetLastRouteSetDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_FirstRouteSetLoadPortLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastRouteSetDischargePortLRC = "HKHKG";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		public void TestMatchingLocationComparer_LastRouteSetDischarge()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.SetLastRouteSetDischarge(LocationHelper.GetCachedLocationFromString("HKHKG", Factory));
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());

			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry1.TI_LastRouteSetDischargePortLRC = "HKHKG";
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);

			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_LastRouteSetDischargePortLRC = "";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry1_line, remover, costentry1_line, costentry2_line);
		}

		#endregion

		#region TestContractNumberComparer

		public void TestContractNumberComparer_WhenCriteriaHasContractNumberAndBlankContractNumber_ChargeCodesWithoutContractNumberIsOverridden()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.CarrierContractNumbers = new ZString[] { "CONTRACT1", "" };
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);
			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_ContractNumber = "CONTRACT1";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(costentry2_line, remover, costentry1_line, costentry2_line);
		}

		public void TestContractNumberComparer_WhenNoContractNumberInCriteria_ChargeCodesNotOverridden()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var costentry1_line = costEntry1.AddRateLine("FRT", FlatCalculator.Code);
			var costEntry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			costEntry2.TI_ContractNumber = "CONTRACT1";
			var costentry2_line = costEntry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(null, remover, costentry1_line, costentry2_line);
		}

		public void TestContractNumberComparer_WhenBlankCriteriaAndRateContractNumbers_RateNotRemoved()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.CarrierContractNumbers = new ZString[] { "" };
			var remover = new OverriddenRateLineRemover(criteria, true);

			var costs = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line1 = entry1.AddRateLine("FRT", FlatCalculator.Code);
			var entry2 = costs.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX");
			var line2 = entry2.AddRateLine("FRT", FlatCalculator.Code);

			AssertRemove(null, remover, line1, line2);
		}

		public void TestContractNumberComparer_AddingToList()
		{
			var testRatingObject = new AutoRatingObjectWithUpdatePlugin("AUSYD", "USLAX", FreightMode.SEA, null, 22m, 30m, null);
			var criteria = new RatingCriteria(testRatingObject, Factory);
			AssertNotNull("costing uses ChargeCodeComparer", new OverriddenRateLineRemover(criteria, true).GetComparers().OfType<ContractNumberComparer>().SingleOrDefault());

			testRatingObject.IsMultipleClientContractNumberSupported_ForTest = false;
			AssertNotNull("revenue uses ChargeCodeComparer if not IsMultipleClientContractNumberSupported", new OverriddenRateLineRemover(criteria, false).GetComparers().OfType<ContractNumberComparer>().SingleOrDefault());

			testRatingObject.IsMultipleClientContractNumberSupported_ForTest = true;
			AssertNull("revenue doesn't use ChargeCodeComparer if IsMultipleClientContractNumberSupported", new OverriddenRateLineRemover(criteria, false).GetComparers().OfType<ContractNumberComparer>().SingleOrDefault());
		}

		public void TestContractNumberComparer_NonBlankNumbers()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 500m, 1m, null);
			criteria.ClientContractNumbers = new ZString[] { "CLIENT1" };
			criteria.CarrierContractNumbers = new ZString[] { "CARRIER1" };
			AssertEquals("revenue", "CLIENT1", new ContractNumberComparer(criteria, false).NonBlankNumbers.Single());
			AssertEquals("costs", "CARRIER1", new ContractNumberComparer(criteria, true).NonBlankNumbers.Single());
		}

		#endregion

		#region TestBCNOrSCNComparer

		public void TestBCNOrSCNComparer_FCLShouldBePreferredOverLCL_BCNMode() => TestBCNOrSCNComparer_FCLShouldBePreferredOverLCL(Constants.RateMode.BCN);

		public void TestBCNOrSCNComparer_FCLShouldBePreferredOverLCL_SCNMode() => TestBCNOrSCNComparer_FCLShouldBePreferredOverLCL(Constants.RateMode.SCN);

		void TestBCNOrSCNComparer_FCLShouldBePreferredOverLCL(ZString rateEntryMode)
		{
			const string origin = "AUSYD";
			const string destination = "USLAX";

			var freightMode = rateEntryMode == Constants.RateMode.BCN
				? FreightMode.BCN
				: FreightMode.SCN;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entryFCL1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, rateEntryMode, origin, destination, "FRT", 100);
			var lineFCL1 = entryFCL1.RateLines[0];
			var entryFCL2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, rateEntryMode, origin, destination, "FRT", 100);
			var lineFCL2 = entryFCL2.RateLines[0];
			var entryLCL1 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, rateEntryMode, origin, destination, "FRT", 100);
			var lineLCL1 = entryLCL1.RateLines[0];
			var entryLCL2 = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, rateEntryMode, origin, destination, "FRT", 100);
			var lineLCL2 = entryLCL2.RateLines[0];

			var testRatingObject = new AutoRatingObjectWithUpdatePlugin(origin, destination, freightMode, null, 22m, 30m, null);
			var criteria = new RatingCriteria(testRatingObject, Factory);
			var remover = new OverriddenRateLineRemover(criteria, false);

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: freightMode == FreightMode.BCN))
			{
				AssertEquals("Comparison of identical FCL lines should return 0.", 0, remover.CompareForTest(lineFCL1, lineFCL2, out var comparer));
				AssertNull("Comparer for identical FCL lines should be null.", comparer);

				AssertEquals("Comparison of identical LCL lines should return 0.", 0, remover.CompareForTest(lineLCL1, lineLCL2, out comparer));
				AssertNull("Comparer for identical LCL lines should be null.", comparer);

				Assert("FCL should be preferred over LCL.", remover.CompareForTest(lineFCL1, lineLCL1, out comparer) > 0);
				AssertType("The comparer for FCL vs LCL should be of type BCNOrSCNComparer.", typeof(BCNOrSCNComparer), comparer);

				Assert("LCL should not be preferred over FCL.", remover.CompareForTest(lineLCL2, lineFCL1, out comparer) < 0);
				AssertType("The comparer for LCL vs FCL should be of type BCNOrSCNComparer.", typeof(BCNOrSCNComparer), comparer);
			}
		}

		public void TestBCNOrSCNComparer_ShouldNotCompareWhenOneRateEntryIsAirOrNotFreight()
		{
			const string origin = "AUSYD";
			const string destination = "USLAX";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entryFCL = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SCN, origin, destination, "FRT", 100);
			var lineFCL = entryFCL.RateLines[0];
			var entryAIR = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.SCN, origin, destination, "FRT", 100);
			var lineAIR = entryAIR.RateLines[0];
			var entryORG = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.SCN, origin, destination, "FRT", 100);
			var lineOrg = entryORG.RateLines[0];

			var testRatingObject = new AutoRatingObjectWithUpdatePlugin(origin, destination, FreightMode.SCN, null, 22m, 30m, null);
			var criteria = new RatingCriteria(testRatingObject, Factory);
			var remover = new OverriddenRateLineRemover(criteria, false);

			// This case is not valid because rate entries should have been filtered by criteria freight mode.
			// Only 1 entry remains. There is no chance for line comparisons. However, it is here for test coverage.
			// Still, the line from FCL entry should have higher priority.
			int comparisonResult = remover.CompareForTest(lineFCL, lineAIR, out var comparer);
			AssertEquals("LineFCL should have a higher priority than LineAIR", true, comparisonResult > 0);
			AssertType("Comparer should be of type BCNOrSCNComparer", typeof(BCNOrSCNComparer), comparer);

			comparisonResult = remover.CompareForTest(lineFCL, lineOrg, out comparer);
			AssertEquals("LineFCL and LineOrg should have equal priority", 0, comparisonResult);
			AssertNull("Comparer should be null for LineFCL and LineOrg", comparer);

			comparisonResult = remover.CompareForTest(lineAIR, lineOrg, out comparer);
			AssertEquals("LineAIR and LineOrg should have equal priority", 0, comparisonResult);
			AssertNull("Comparer should be null for LineAIR and LineOrg", comparer);
		}

		#endregion

		#region Implementation

		OrgWithSource GetTestOrgWithSourceFromHeader(OrgHeader org, string source)
		{
			return OrgWithSource.New(org, new List<string>() { source });
		}

		void AssertRemove(RateLine expectedRemainingLine, OverriddenRateLineRemover remover, params RateLine[] sourceLines)
		{
			var lines = new List<IRateLine>(sourceLines);
			remover.Remove(lines);
			if (expectedRemainingLine != null)
			{
				AssertEquals(1, lines.Count);
				AssertEquals(expectedRemainingLine, lines[0]);
			}
			else
			{
				AssertEquals(sourceLines.Length, lines.Count);
			}
		}

		void AssertException(string expectedMessage, OverriddenRateLineRemover remover, params RateLine[] sourceLines)
		{
			var caughtMessage = "";
			try
			{
				var lines = new List<IRateLine>(sourceLines);
				AssertEquals(sourceLines.Length, lines.Count);
				remover.Remove(lines);
			}
			catch (AutoRaterException ex)
			{
				caughtMessage = ex.Message;
			}

			AssertEquals(expectedMessage, caughtMessage);
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
