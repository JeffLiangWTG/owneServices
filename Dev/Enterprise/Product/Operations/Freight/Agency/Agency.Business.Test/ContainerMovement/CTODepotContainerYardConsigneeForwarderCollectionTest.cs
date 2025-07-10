using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(CTODepotContainerYardConsigneeForwarderCollection))]
	internal class CTODepotContainerYardConsigneeForwarderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionFilter()
		{
			OrgHeader other = GetOtherOrg();
			OrgHeader cTO = GetCTO();
			OrgHeader packDepot = GetPackDepot();
			OrgHeader unPackDepot = GetUnPackDepot();
			OrgHeader containerYard = GetContainerYard();
			OrgHeader consignee = GetConsignee();
			OrgHeader forwarder = GetForwarder();
			Collection.Load();
			AssertEquals("should contain CTO", true, Collection.Contains(cTO));
			AssertEquals("should contain pack depot", true, Collection.Contains(packDepot));
			AssertEquals("should contain unpack depot", true, Collection.Contains(unPackDepot));
			AssertEquals("should contain container yard", true, Collection.Contains(containerYard));
			AssertEquals("should contain consignee", true, Collection.Contains(consignee));
			AssertEquals("should contain forwarder", true, Collection.Contains(forwarder));
			AssertEquals("should not contain random other org", false, Collection.Contains(other));
		}

		#region Implementation
		#region Get*Org
		OrgHeader GetOtherOrg()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsMiscFreightServices = true;
			return result;
		}

		OrgHeader GetCTO()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsAirCTO = true;
			result.OH_IsMiscFreightServices = true;
			return result;
		}

		OrgHeader GetPackDepot()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsMiscFreightServices = true;
			result.OH_IsPackDepot = true;
			return result;
		}

		OrgHeader GetUnPackDepot()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsMiscFreightServices = true;
			result.OH_IsUnpackDepot = true;
			return result;
		}

		OrgHeader GetContainerYard()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsMiscFreightServices = true;
			result.OH_IsContainerYard = true;
			return result;
		}

		OrgHeader GetConsignee()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsConsignee = true;
			return result;
		}

		OrgHeader GetForwarder()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_IsForwarder = true;
			return result;
		}

		#endregion
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrganisationDefaults orgDefaults = new OrganisationDefaults();
			return new CTODepotContainerYardConsigneeForwarderCollection(Factory);
		}
		#endregion
	}
}
