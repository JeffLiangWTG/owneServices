using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyPrincipalTest : AgencyAllocationItemTest<OrgHeader>
	{
		public void TestRefreshUsageBindingsPropergation()
		{
			int countryChangedCount = 0;
			int originChangedCount = 0;
			int sailingChangedCount = 0;
			Country.GenericPrincipal.UsedTonnesInfo.ValueChanged += delegate
			{
				countryChangedCount++;
			};
			Country.GenericPrincipal.Origins[0].UsedTonnesInfo.ValueChanged += delegate
			{
				originChangedCount++;
			};
			Country.GenericPrincipal.Sailings[0].UsedTonnesInfo.ValueChanged += delegate
			{
				sailingChangedCount++;
			};
			Country.GenericPrincipal.RefreshUsageBindings();
			CombineAssertions(delegate
			{
				AssertEquals("Calling RefreshUsageBindings on the AgencyPrincipal should propergate to the children: Country", 1, countryChangedCount);
				AssertEquals("Calling RefreshUsageBindings on the AgencyPrincipal should propergate to the children: Origin", 1, originChangedCount);
				AssertEquals("Calling RefreshUsageBindings on the AgencyPrincipal should propergate to the children: Sailing", 1, sailingChangedCount);
			});
			countryChangedCount = 0;
			originChangedCount = 0;
			sailingChangedCount = 0;
			Country.GenericPrincipal.RefreshUsageData();
			CombineAssertions(delegate
			{
				AssertEquals("PerformPreSave should also call RefreshUsageBindings: Country", 1, countryChangedCount);
				AssertEquals("PerformPreSave should also call RefreshUsageBindings: Origin", 1, originChangedCount);
				AssertEquals("PerformPreSave should also call RefreshUsageBindings: Sailing", 1, sailingChangedCount);
			});
		}

		#region Implementation
		protected override BusinessObject GetAllocationParentFromVoyage(JobVoyage voyage)
		{
			return voyage.Countries.GetCountry("AU", true);
		}

		protected override AgencyAllocationItem<OrgHeader> WrapAllocationParent(AgencyPrincipal principal, BusinessObject allocationParent)
		{
			return principal;
		}

		protected override ZString AllocationMethod
		{
			get
			{
				return AllocationMethodList.Codes.Country;
			}
		}

		protected override SlotAllocationDependentCollection GetSlotCollection(BusinessObject allocationParent)
		{
			return ((VoyageCountry)allocationParent).SlotAllocations;
		}
		#endregion
	}
}
