using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateFeeChargeLevelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestServiceTypeList()
		{
			var list = new CodeDescriptionPairList();

			foreach (FeeChargeType type in OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value.FeeChargeTypes)
			{
				list.AddPair(type.Code, type.Description);
			}

			AssertContainsExactElementsInAnyOrder(list, Lookups.ServiceTypeList);
		}

		public void TestServiceLevelList()
		{
			var registry = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.Value;
			var registryChargeType = registry.FeeChargeTypes[0];
			var level = registryChargeType.FeeChargeLevels.AddNew();
			var count = registryChargeType.FeeChargeLevels.Count;

			level.Code = "SIL";
			level.Description = (NoResString)"Test Description";
			level.Amount1Type = OrgConstants.ServiceLevelAmountTypes.Code.Excess;
			level.Amount1 = 300;
			level.Amount1Currency = "AUD";
			level.Amount2Type = OrgConstants.ServiceLevelAmountTypes.Code.Maximum;
			level.Amount2Currency = "CNY";
			level.Amount2 = 600;

			OrganisationsDataRegistry.Instance.RateFeeChargeLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry);

			AssertEquals(count, Lookups.ServiceLevelList.Count);
		}

		#region Implementation

		OrgRateFeeChargeLevelLookups Lookups;
		OrgRateFeeChargeLevel Parent;

		protected override void SetUp()
		{
			base.SetUp();

			Parent = Factory.NewWithValidTestData<OrgRateFeeChargeLevel>();
			Parent.ORF_ServiceType = ServiceTypeFromRegistry;
			Lookups = new OrgRateFeeChargeLevelLookups(Parent);
		}

		static ZString ServiceTypeFromRegistry
		{
			get
			{
				var registrySetting = OrganisationsDataRegistry.Instance.RateFeeChargeLevels.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (registrySetting != null)
				{
					var type = registrySetting.FeeChargeTypes.FirstOrDefault() as FeeChargeType;

					if (type != null)
					{
						return type.Code;
					}
				}

				return "";
			}
		}

		#endregion
	}
}
