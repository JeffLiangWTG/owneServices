using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	class PermitTypeModuleFilterForTest : PermitTypeModuleFilter
	{
		public PermitTypeModuleFilterForTest(ZString description, GetPermitTypeQuery queryDelegate) : base(description, queryDelegate, GetCountries)
		{
		}

		static IList GetCountries()
		{
			return new CodeDescriptionPairList()
			{
				new CodeDescriptionPair("ZA", "South Africa")
			};
		}

		public new PermitTypeList PermitTypes => new PermitTypeList_ForModuleFilterTest();

		public new IList PermitSubTypes => new PermitSubTypeList_ForModuleFilterTest();

		sealed class PermitTypeList_ForModuleFilterTest : PermitTypeList
		{
			public PermitTypeList_ForModuleFilterTest()
			{
				AddRange(new CodeDescriptionPairList()
				{
					new CodeDescriptionPair("IMP", "Basic Import Permit"), new CodeDescriptionPair("EXP", "Basic Export Permit")
				});
			}
		}

		sealed class PermitSubTypeList_ForModuleFilterTest : PermitSubTypeList
		{
			public PermitSubTypeList_ForModuleFilterTest()
			{
				AddRange(new CodeDescriptionPairList()
					{
						new CodeDescriptionPair("LVE", "Light Motor Vehicles")
					});
			}
		}
	}
}
