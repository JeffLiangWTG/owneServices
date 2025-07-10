using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageCountry))]
	sealed class VoyageCountryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultAllocationMethodFromCountryCode()
		{
			var header = new AllocationMethodDefaultHeader(Factory);
			header.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;
			header.Rules.Add(new AllocationMethodDefaultRule(Factory) { CountryCode = "AU", AllocationMethod = AllocationMethodList.Codes.Country });
			header.Rules.Add(new AllocationMethodDefaultRule(Factory) { CountryCode = "NZ", AllocationMethod = AllocationMethodList.Codes.Origin });

			FreightConfigurationRegistry.Instance.DefaultAllocationMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, header);

			var newFactory = new BusinessObjectFactory();
			var country = newFactory.New<VoyageCountry>();

			country.J0_RN_NKCountry = "AU";
			AssertEquals(AllocationMethodList.Codes.Country, country.J0_AllocationMethod);

			country.J0_RN_NKCountry = "NZ";
			AssertEquals(AllocationMethodList.Codes.Origin, country.J0_AllocationMethod);

			country.J0_RN_NKCountry = "US";
			AssertEquals(AllocationMethodList.Codes.Ignore, country.J0_AllocationMethod);

			header.Rules.Add(new AllocationMethodDefaultRule(Factory) { CountryCode = "US", AllocationMethod = AllocationMethodList.Codes.Sailing });
			FreightConfigurationRegistry.Instance.DefaultAllocationMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, header);

			country.J0_RN_NKCountry = "CA";
			AssertEquals(AllocationMethodList.Codes.Ignore, country.J0_AllocationMethod);

			country.J0_RN_NKCountry = "US";
			AssertEquals(AllocationMethodList.Codes.Sailing, country.J0_AllocationMethod);
		}

		public void TestISlotAllocationParentCode()
		{
			ISlotAllocationParent parent = Factory.New<VoyageCountry>();
			AssertEquals(JobVoyCountrySchema.Constants.Prefix, parent.Code);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Country/Region = 'AU'", Factory.New<JobVoyage>().Countries.GetCountry("AU", true).HumanReadableName);
			AssertEquals("Country/Region = 'NZ'", Factory.New<JobVoyage>().Countries.GetCountry("NZ", true).HumanReadableName);
		}

		public void TestClone()
		{
			VoyageCountry country = Factory.New<VoyageCountry>();
			SlotAllocation allocation1 = country.SlotAllocations.GetAllocation(ZGuid.Empty);
			allocation1.SetAspect("AS1", 10);

			VoyageCountry countryClone = (VoyageCountry)country.Clone();
			SlotAllocation allocation2 = countryClone.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertNotEquals(allocation1.PK, allocation2.PK);
			AssertEquals("AS1 aspect", 10m, allocation2.GetAspect("AS1"));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Countries.GetCountry("AU", true);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			return voyage.Countries.GetCountry("AU", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var voyage = factory.NewWithValidTestData<JobVoyage>();
			return voyage.Countries.GetCountry(Core.Constants.CountryCodes.Australia, true);
		}

		#endregion
	}
}
