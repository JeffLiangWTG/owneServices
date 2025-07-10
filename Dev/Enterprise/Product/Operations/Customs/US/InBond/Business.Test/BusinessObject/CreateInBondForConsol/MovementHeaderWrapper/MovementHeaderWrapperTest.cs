using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(MovementHeaderWrapper))]
	sealed class MovementHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInBondNumber()
		{
			wrapper.InBondNumber = "INB00001";
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(wrapper.InBondNumberInfo);
			AssertEquals("In-Bond Number", resourceStringData.Caption);
			AssertEquals("INB00001", wrapper.InBondNumber);
			AssertEquals(true, wrapper.InBondNumberInfo.ReadOnly);
		}

		public void TestEntryType()
		{
			wrapper.EntryType = "61";
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(wrapper.EntryTypeInfo);
			AssertEquals("Entry Type", resourceStringData.Caption);
			AssertEquals("61", wrapper.EntryType);

			var entryTypeList = wrapper.Lookups.EntryTypeList;
			AssertEquals("InbondEntryTypeList", Factory.GetCachedValue<InbondCommonTypeList>(), entryTypeList);
		}

		public void TestInBondCarrier()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TSTCONSIGNEE";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;

			wrapper.InBondCarrierAddress = address.PK;
			AssertEquals(wrapper.CarrierOrgPK, org.PK);
		}

		public void TestDestination()
		{
			wrapper.Destination = "0123";
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(wrapper.DestinationInfo);
			AssertEquals("US Destination", resourceStringData.Caption);
			AssertEquals("0123", wrapper.Destination);

			AssertEquals(ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today), wrapper.Lookups.RegionDistrictPorts);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new MovementHeaderWrapper(Factory);
		}
		MovementHeaderWrapper wrapper;
	}
}
