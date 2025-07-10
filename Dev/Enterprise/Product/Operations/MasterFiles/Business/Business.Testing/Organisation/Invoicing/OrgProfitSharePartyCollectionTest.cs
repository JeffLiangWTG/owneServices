using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgProfitSharePartyLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitSharePartyGeneralCollection))]
	public class OrgProfitSharePartyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetParty()
		{
			var details = Factory.New<OrgProfitShareDetails>();

			var result = details.PartyDetails.GetParty("XXX");
			AssertNull(result);

			result = details.PartyDetails.GetParty("SND");
			AssertNull(result);

			var newParty = details.PartyDetails.AddNew();
			newParty.PS_PartyType = "SND";

			result = details.PartyDetails.GetParty("SND");
			AssertNotNull(result);
			AssertEquals(newParty, result);

			result = details.PartyDetails.GetParty("RCV");
			AssertNull(result);

			result = details.PartyDetails.GetParty("CON");
			AssertNull(result);

			newParty = details.PartyDetails.AddNew();
			newParty.PS_PartyType = "CON";

			result = details.PartyDetails.GetParty("CON");
			AssertNotNull(result);
			AssertEquals(newParty, result);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var details = Factory.New<OrgProfitShareDetails>();
			return new OrgProfitSharePartyGeneralCollection(details);
		}

		protected static void FillInAllPartyTypes(OrgProfitShareDetails profitShareDetails)
		{
			foreach (var partyCode in PartyTypeCodes.GeneralTypes)
			{
				var newParty = profitShareDetails.PartyDetails.AddNew();
				newParty.PS_PartyType = partyCode;
			}

			foreach (var partyCode in PartyTypeCodes.GatewayConsolProfitRedistributionTypes)
			{
				var newParty = profitShareDetails.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
				newParty.PS_PartyType = partyCode;
			}
		}
	}

	[TestedType(typeof(OrgProfitSharePartyGeneralCollection))]
	public class OrgProfitSharePartyGeneralCollectionTest : OrgProfitSharePartyCollectionTest
	{
		public void TestCollectionHasOnlyOrgProfitSharePartiesForGeneralDetails()
		{
			var details = Factory.New<OrgProfitShareDetails>();
			FillInAllPartyTypes(details);

			var expectedPartyTypes = new[] { "SEN", "RCV", "HDF", "CON", "PIC", "DLY" };
			var collection = new OrgProfitSharePartyGeneralCollection(details);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(
				expectedPartyTypes,
				collection.OfType<OrgProfitShareParty>().Select(x => x.PS_PartyType));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var details = Factory.New<OrgProfitShareDetails>();
			return details.PartyDetails;
		}
	}

	[TestedType(typeof(OrgProfitSharePartyRedistributionCollection))]
	public class OrgProfitSharePartyRedistributionCollectionTest : OrgProfitSharePartyCollectionTest
	{
		public void TestCollectionHasOnlyOrgProfitSharePartiesForRedistributionDetails()
		{
			var details = Factory.New<OrgProfitShareDetails>();
			FillInAllPartyTypes(details);

			var expectedPartyTypes = new[] { "SDA", "SPA" };
			var collection = new OrgProfitSharePartyRedistributionCollection(details);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(
				expectedPartyTypes,
				collection.OfType<OrgProfitShareParty>().Select(x => x.PS_PartyType));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var details = Factory.New<OrgProfitShareDetails>();
			return details.PartyDetailsForGatewayProfitShareRedistribution;
		}
	}
}
