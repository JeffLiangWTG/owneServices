using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressCollectionForPlugin))]
	sealed class TestJobDocAddressCollectionForPlugin : BusinessObjectCollectionTestCase
	{
		public void TestHostParentBizo()
		{
			var collection = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
			var commonShipment = Factory.New<ICommonShipment>();
			AssertNull(collection.HostParentBizo);
			collection.HostParentBizo = commonShipment as IBusiness;
			AssertEquals(commonShipment, collection.HostParentBizo);
		}

		public void TestViewHidesInvalidAddresses()
		{
			JobDocAddressCollectionForPlugin collection = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));

			JobDocAddress validDocAddy = JobDocAddress.New(Parent);
			JobDocAddress invalidDocAddy = JobDocAddress.New(Parent);

			validDocAddy.E2_OA_Address = ZGuid.NewZGuid();

			collection.Add(validDocAddy);
			collection.Add(invalidDocAddy);
			collection.ReBuild();
			AssertCollectionContains(validDocAddy, collection);
			AssertCollectionNotContains(invalidDocAddy, collection);
		}

		public void TestAllowNew()
		{
			JobDocAddressCollectionForPlugin collection = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
			AssertEquals(false, collection.AllowNew);
		}

		public void TestOnAddedShouldHaveWarningWhenAddressIsInactive()
		{
			var collection = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_IsActive = true;
			docAddress.E2_OA_Address = address.PK;
			Factory.Save();

			address.OA_IsActive = false;

			collection.Add(docAddress);

			collection.OfType<JobDocAddress>().ForEach(j =>
			{
				AssertHasWarning(j.E2_OA_AddressInfo, $"This {j.E2_OA_AddressInfo.HumanReadableName} is inactive.");
			});
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDocAddressCollection collection = new JobDocAddressCollection(Factory);
			return new JobDocAddressCollectionForPlugin(collection);
		}
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return JobDocAddress.New(Parent);
		}

		JobDocAddressPersistentParentForTesting Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.New<JobDocAddressPersistentParentForTesting>();
				}
				return fParent;
			}
		}

		JobDocAddressPersistentParentForTesting fParent;

		#endregion
	}
}
