using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PortBasedOrgAddressDeciderTest : TestCaseWithFactory
	{
		public void TestBestAddressTypeForDelivery()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Delivery, delegate
			{ return "AUSYD"; });
			SetupAddress(OrgAddressType.Delivery, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUSYD");
			SetupAddress(OrgAddressType.Pickup, "AUSYD");

			AssertEquals(OrgAddressType.Delivery.Code, decider.Address1);
		}

		public void TestBestAddressTypeForPickup()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Pickup, delegate
			{ return "AUSYD"; });
			SetupAddress(OrgAddressType.Pickup, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUSYD");
			SetupAddress(OrgAddressType.Delivery, "AUSYD");

			AssertEquals(OrgAddressType.Pickup.Code, decider.Address1);
		}

		public void TestBestAddressTypeOtherPortForDelivery()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Delivery, delegate
			{ return "AUMEL"; });
			SetupAddress(OrgAddressType.Delivery, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUSYD");
			SetupAddress(OrgAddressType.Pickup, "AUSYD");

			AssertEquals(OrgAddressType.Delivery.Code, decider.Address1);
		}

		public void TestBestAddressTypeOtherPortForPickup()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Pickup, delegate
			{ return "AUMEL"; });
			SetupAddress(OrgAddressType.Pickup, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUSYD");
			SetupAddress(OrgAddressType.Delivery, "AUSYD");

			AssertEquals(OrgAddressType.Pickup.Code, decider.Address1);
		}

		public void TestRelatedPort()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Pickup, delegate
			{ return "AUSYD"; });
			SetupAddress(OrgAddressType.Pickup, "AUMEL");
			SetupAddress(OrgAddressType.Pickup, "AUSYD");

			AssertEquals("AUSYD", decider.Address2);
		}

		public void TestRelatedPortAndBestAddressType()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Delivery, delegate
			{ return "AUSYD"; });
			SetupAddress(OrgAddressType.Delivery, "AUMEL");
			SetupAddress(OrgAddressType.Delivery, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUSYD");
			SetupAddress(OrgAddressType.PickupAndDelivery, "AUMEL");
			SetupAddress(OrgAddressType.Pickup, "AUMEL");
			SetupAddress(OrgAddressType.Pickup, "AUSYD");

			AssertEquals(OrgAddressType.Delivery.Code, decider.Address1);
			AssertEquals("AUSYD", decider.Address2);
		}

		public void TestPreferRelatedPortOverAddressType()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Delivery, delegate
			{ return "AUSYD"; });
			SetupAddress(OrgAddressType.Delivery, "AUMEL");
			SetupAddress(OrgAddressType.Office, "AUSYD");
			AssertEquals("AUSYD", decider.Address2);
		}

		public void TestMainAddressFallbackDelivery()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Delivery, delegate
			{ return "AUSYD"; });
			AssertEquals("main", decider.Address1);
		}

		public void TestMainAddressFallbackPickup()
		{
			OrgAddressDecider decider = SetupAddressDecider(CargoAddressType.Pickup, delegate
			{ return "AUSYD"; });
			AssertEquals("main", decider.Address1);
		}

		#region Implementation

		OrgAddress SetupAddress(OrgAddressType orgAddressType, ZString relatedPort)
		{
			OrgAddress result = Org.Addresses.AddNew();
			result.OA_RL_NKRelatedPortCode = relatedPort;
			result.AddAddressType(orgAddressType);
			result.OA_Address1 = orgAddressType.Code;
			result.OA_Address2 = relatedPort;
			return result;
		}

		public OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_IsGlobalAccount = true;
					org.MainAddress.OA_Address1 = "main";
					org.MainAddress.OA_RL_NKRelatedPortCode = "SGSIN";
				}
				return org;
			}
		}
		OrgHeader org;

		OrgAddressDecider SetupAddressDecider(CargoAddressType addressType, PortBasedOrgAddressDecider.GetRelatedPortDelegate getRelatedPort)
		{
			OrgAddressDecider decider = new PortBasedOrgAddressDecider(Org, addressType, getRelatedPort);
			return decider;
		}

		#endregion
	}
}
