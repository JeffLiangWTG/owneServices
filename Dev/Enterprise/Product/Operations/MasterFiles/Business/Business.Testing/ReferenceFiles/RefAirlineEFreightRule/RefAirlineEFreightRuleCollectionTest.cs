using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineEFreightRuleCollection))]
	sealed class RefAirlineEFreightRuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRefAirlineEFreightRuleCollectionAddNew()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();
			var collection = new RefAirlineEFreightRuleCollection(airLine);

			var rule = collection.AddNew();
			AssertEquals(airLine.PK, rule.RME_RM);
		}

		public void TestGetBestMatchRule()
		{
			var airLine = Factory.New<RefAirline>();

			var rule1 = AddNewRule(airLine, "AUSYD", "SGSIN");
			var rule2 = AddNewRule(airLine, "AUSYD", "SG");
			var rule3 = AddNewRule(airLine, "AUSYD", "");
			var rule4 = AddNewRule(airLine, "AU", "SGSIN");
			var rule5 = AddNewRule(airLine, "AU", "SG");
			var rule6 = AddNewRule(airLine, "AU", "");
			var rule7 = AddNewRule(airLine, "", "SGSIN");
			var rule8 = AddNewRule(airLine, "", "SG");
			AddNewRule(airLine, "AUSYD", "SGTGN");
			AddNewRule(airLine, "AUBNE", "SGSIN");
			AddNewRule(airLine, "AUBNE", "SGTGN");
			AddNewRule(airLine, "", "");

			var origin = Factory.New<RefUNLOCO>();
			origin.RL_Code = "AUSYD";
			origin.RL_RN_NKCountryCode = "AU";

			var destination = Factory.New<RefUNLOCO>();
			destination.RL_Code = "SGSIN";
			destination.RL_RN_NKCountryCode = "SG";

			AssertEquals("Match full code", rule1.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			destination.RL_Code = "SGAYC";
			AssertEquals("Match origin and destination country code", rule2.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			destination.RL_Code = "NZAKL";
			destination.RL_RN_NKCountryCode = "NZ";
			AssertEquals("Match origin code", rule3.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			origin.RL_Code = "AUADE";
			destination.RL_Code = "SGSIN";
			destination.RL_RN_NKCountryCode = "SG";
			AssertEquals("Match origin country and destination code", rule4.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			destination.RL_Code = "SGAYC";
			AssertEquals("Match origin country and destination country code", rule5.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			destination.RL_Code = "NZAKL";
			destination.RL_RN_NKCountryCode = "NZ";
			AssertEquals("Match origin country code", rule6.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			origin.RL_Code = "NZAKL";
			origin.RL_RN_NKCountryCode = "NZ";
			destination.RL_Code = "SGSIN";
			destination.RL_RN_NKCountryCode = "SG";
			AssertEquals("Match destination code", rule7.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			destination.RL_Code = "SGSEM";
			AssertEquals("Match destination country code", rule8.PK, airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination).PK);

			origin.RL_Code = "CNSHA";
			origin.RL_RN_NKCountryCode = "CN";
			destination.RL_Code = "USCHI";
			destination.RL_RN_NKCountryCode = "US";
			AssertNull(airLine.EFreightStatusCollection.GetBestMatchRule(origin, destination));
		}

		RefAirlineEFreightRule AddNewRule(RefAirline airLine, string origin, string destination)
		{
			var rule = airLine.EFreightStatusCollection.AddNew();
			rule.RME_OriginLocation = origin;
			rule.RME_DestinationLocation = destination;

			return rule;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefAirlineEFreightRuleCollection(Factory.NewWithValidTestData<RefAirline>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RefAirlineEFreightRule>();
		}
	}
}
