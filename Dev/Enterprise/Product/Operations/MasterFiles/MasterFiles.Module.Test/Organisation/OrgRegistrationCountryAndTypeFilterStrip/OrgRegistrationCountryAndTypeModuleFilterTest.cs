using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgRegistrationCountryAndTypeModuleFilter))]
	sealed class OrgRegistrationCountryAndTypeModuleFilterTest : ModuleCodeFilterTest
	{
		public void TestUpdateTypeListDelegate()
		{
			bool isDelegateCalled = false;

			UpdateDependentList updateListDelegate = delegate
			{
				isDelegateCalled = true;
			};

			GetCodeQuery queryDelegate = delegate
			{ return new ZQuery(); };

			OrgRegistrationCountryAndTypeModuleFilterForTest filter
				= new OrgRegistrationCountryAndTypeModuleFilterForTest("TEST FILTER", queryDelegate, new RefCountryCollection(Factory), new CodeDescriptionPairList(), updateListDelegate, null, false);

			filter.Property1 = "AU";
			AssertEquals(true, isDelegateCalled);
		}

		public void TestShallowCloneUseDifferentListForDropEdit()
		{
			GetDefaultDependentList defaultListDelegate = delegate
			{ return new CodeDescriptionPairList(); };

			GetCodeQuery queryDelegate = delegate
			{ return new ZQuery(); };

			IList originalDropEditList = new CodeDescriptionPairList();
			originalDropEditList.Add(new CodeDescriptionPair("AAA", "TEST"));
			OrgRegistrationCountryAndTypeModuleFilterForTest filter
				= new OrgRegistrationCountryAndTypeModuleFilterForTest("TEST FILTER", queryDelegate, new RefCountryCollection(Factory), originalDropEditList, null, defaultListDelegate, false);

			OrgRegistrationCountryAndTypeModuleFilter clonedFilter = (OrgRegistrationCountryAndTypeModuleFilter)filter.ShallowCloneForTest();
			AssertNotEquals(originalDropEditList, clonedFilter.List2);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			GetDefaultDependentList defaultListDelegate = delegate
			{ return new CodeDescriptionPairList(); };
			GetCodeQuery queryDelegate = delegate
			{ return new ZQuery(); };
			return new OrgRegistrationCountryAndTypeModuleFilter("TEST", queryDelegate, new RefCountryCollection(Factory), new CodeDescriptionPairList(), null, defaultListDelegate, false);
		}

		class OrgRegistrationCountryAndTypeModuleFilterForTest : OrgRegistrationCountryAndTypeModuleFilter
		{
			public OrgRegistrationCountryAndTypeModuleFilterForTest(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection findBoxList, IList dropEditList, UpdateDependentList updateListDelegate, GetDefaultDependentList defaultListDelegate, bool notIn)
				: base(description, queryDelegate, findBoxList, dropEditList, updateListDelegate, defaultListDelegate, notIn)
			{ }

			public ModuleFilter ShallowCloneForTest()
			{
				return ShallowCloneCore();
			}
		}

		#endregion
	}
}
