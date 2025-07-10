using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddressCapabilityWrapperCollection))]
	sealed class OrgAddressCapabilityWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgAddressCapabilityWrapperCollection>
	{
		protected override OrgAddressCapabilityWrapperCollection GetCollectionToTest()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress parentHeader = header.MainAddress;
			return new OrgAddressCapabilityWrapperCollection(parentHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress parentHeader = header.MainAddress;
			return new OrgAddressCapabilityWrapper(parentHeader);
		}

		public void TestSetsUnlocoWhenIsElevatedToMainAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "DbWontLetThisBeBlank St";
			address.AddAddressType(OrgAddressType.Office);

			Assert("We arent main yet, so we shouldnt have our UNLOCO set", string.IsNullOrEmpty(address.OA_RL_NKRelatedPortCode));

			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			Factory.Save();

			Assert("PRE: Is main", address.IsMainAddress);
			AssertEquals("We are main, so we should have our UNLOCO set", "AUSYD", address.GetBaseOA_RL_NKRelatedPortCode());
		}

		public void TestConstructor()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			OrgAddressCapabilityWrapperCollection collection = new OrgAddressCapabilityWrapperCollection(mainAddress);
			int i = 0;
			foreach (object o in OrgAddressType.AddressTypeList)
			{
				i++;
			}
			AssertEquals(i, collection.Count);
			Assert(!collection.AllowNew);
			Assert(!collection.AllowRemove);
		}

		public override void TestAdd()
		{
			AssertEquals("Precondition : Collection.Count", OrgCodeLists.AddressType_List(Factory).Count, Collection.Count);
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();

			AssertEquals("Collection count", OrgCodeLists.AddressType_List(Factory).Count, Collection.Count);

			Collection.Add(bizO1);
			Collection.Add(bizO2);

			AssertEquals("Collection count", OrgCodeLists.AddressType_List(Factory).Count + 2, Collection.Count);
			Assert("Contains element 1", Collection.Contains(bizO1));
			Assert("Contains element 2", Collection.Contains(bizO2));
		}

		public override void TestAddNew()
		{
			try
			{
				BusinessObject bizO1 = Collection.AddNew();
				BusinessObject bizO2 = Collection.AddNew();

				AssertEquals("Collection count", OrgCodeLists.AddressType_List(Factory).Count + 2, Collection.Count);
				Assert("Contains new elements", Collection.Contains(bizO1));
				Assert("Contains new elements", Collection.Contains(bizO2));
			}
			catch (NotSupportedException)
			{
				Assert(true);
			}
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2);
			AssertEquals("Precondition : Collection count", OrgCodeLists.AddressType_List(Factory).Count + 2, Collection.Count);

			Collection.Remove(bizO1);

			AssertEquals("Collection count", OrgCodeLists.AddressType_List(Factory).Count + 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);
		}

		public override void TestDelete()
		{
			try
			{
				BusinessObject bizO1 = GetNewElementToAddToTheCollection();
				BusinessObject bizO2 = GetNewElementToAddToTheCollection();
				Collection.AddRange(bizO1, bizO2);
				AssertEquals("Precondition : Collection count", OrgCodeLists.AddressType_List(Factory).Count + 2, Collection.Count);

				Collection.RemoveAndDelete(bizO1);

				AssertEquals("Collection count", OrgCodeLists.AddressType_List(Factory).Count + 1, Collection.Count);
				Assert("Contains element 2", Collection.Contains(bizO2));
				Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
				Assert("Element 1 was removed, and deleted", bizO1.IsDeleted);
			}
			catch (NotSupportedException) // deleting not supported
			{
				Assert(true);
			}
		}

		public void TestListOfCodes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			AssertEquals(OrgAddressType.Office.Code, mainAddress.AddressCapability.GetListOfCodes());
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			AssertEquals(OrgAddressType.Delivery.Code + ", " + OrgAddressType.Office.Code, mainAddress.AddressCapability.GetListOfCodes());
		}

		public void TestGetAddressCapabilityOnCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			OrgAddressCapabilityWrapper wrapper = mainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Office);
			AssertEquals(wrapper.AddressCapabilityType, OrgConstants.AddressType.Office);
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Miscellaneous);
			wrapper = mainAddress.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Miscellaneous);
			AssertEquals(wrapper.AddressCapabilityType, OrgConstants.AddressType.Miscellaneous);
		}

		public void TestIsEmpty()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			OrgAddress otherAddress = header.Addresses.AddNew();
			Assert("MainAddress always has capabilities", !mainAddress.AddressCapability.IsEmpty);
			Assert("other address does not have capabilities", otherAddress.AddressCapability.IsEmpty);
		}

		public void TestTickCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			Assert("Payables is ticked", mainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Payables.Code));
			Assert("pickup not ticked", !mainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Pickup.Code));
		}

		public void TestUnTickCode()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			Assert("Payables is ticked", mainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Payables.Code));
			mainAddress.AddressCapability.SetCapabilityDisabled(OrgAddressType.Payables.Code);
			Assert("Payables not ticked", !mainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Payables.Code));
		}

		public void TestTickIsMain()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			mainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			mainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Payables.Code);
			Assert("Payables is ticked", mainAddress.AddressCapability.GetCapabilityEnabled(OrgAddressType.Payables.Code));
			Assert("Payables is ticked", mainAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Payables.Code));
			Assert("pickup not ticked", !mainAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Pickup.Code));
			mainAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Payables.Code);
			Assert("Payables is ticked", !mainAddress.AddressCapability.GetIsMainAddress(OrgAddressType.Payables.Code));
		}

		public void TestUntickAll()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress mainAddress = header.MainAddress;
			OrgAddress address = header.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.PickupAndDelivery.Code);
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);
			Assert("PickupAndDelivery is ticked", address.AddressCapability.GetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code));
			Assert("Pickup is ticked", address.AddressCapability.GetCapabilityEnabled(OrgAddressType.Pickup.Code));

			address.AddressCapability.DisableAllCapabilities();

			Assert("nothing is ticked", !address.AddressCapability.GetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code));
			Assert("nothing is ticked", !address.AddressCapability.GetCapabilityEnabled(OrgAddressType.Pickup.Code));
		}

		public void TestGetCapabilityEnabledMain()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Payables.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Payables.Code);
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			Assert(address.AddressCapability.GetCapabilityEnabledMain(OrgAddressType.Payables.Code));
			Assert(!address.AddressCapability.GetCapabilityEnabledMain(OrgAddressType.PickupAndDelivery.Code));
		}
	}
}
