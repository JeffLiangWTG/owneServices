using System;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.TransportConsignment.Module.Testing
{
	public abstract class DtbChildFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFieldOnRunsheet()
		{
			AssertEquals(ExpectedFieldOnRunsheet, GetNewChildFilterBusinessObject.FieldOnRunsheet);
		}

		public void TestChildBizOPKOrNK()
		{
			AssertEquals(ExpectedChildBizOPKOrNK, GetNewChildFilterBusinessObject.ChildBizOPKOrNK);
		}

		public void TestChildBizOPKOrNKSameTypeAsExpectedFieldOnRunsheet()
		{
			AssertEquals(GetNewChildFilterBusinessObject.FieldOnRunsheet.TypeInfo, GetNewChildFilterBusinessObject.ChildBizOPKOrNK.TypeInfo);
		}

		#region TestTypeOfBusinessObjectToQuery

		public virtual void TestTypeOfBusinessObjectToQuery()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			AssertEquals(((DtbChildFilterBusinessObject)filterBizO).TypeOfBusinessObjectToQuery, TypeOfBusinessObjectToQuery);
		}

		#endregion

		#region TestActiveFilters

		public virtual void TestActiveFilters()
		{
			var filterBizO = new DtbRoutePlannerFilterBusinessObject();
			var childFilterBizO = GetNewChildFilterBusinessObject;
			filterBizO.AddChildFilterBusinessObject(childFilterBizO);

			var stateFilter1 = (ModuleTextFilter)filterBizO[ChildFilterName];
			stateFilter1.IsActive = true;
			stateFilter1.Property = "NSW";
			AssertContainsExactElementsInAnyOrder(new ModuleFilter[] { stateFilter1 }, childFilterBizO.ActiveFilters);

			var stateFilter2 = DuplicateFilter(stateFilter1.Description);
			stateFilter2.Category = FilterCategoryOfChildFilter;
			stateFilter2.IsActive = true;
			stateFilter2.Property = "VIC";
			filterBizO.ModuleFilters.AddFilter(stateFilter2);
			AssertContainsExactElementsInAnyOrder(new ModuleFilter[] { stateFilter1, stateFilter2 }, childFilterBizO.ActiveFilters);
		}

		#endregion

		protected abstract DtbChildFilterBusinessObject GetNewChildFilterBusinessObject { get; }
		protected abstract SchemaColumn ExpectedFieldOnRunsheet { get; }
		protected abstract SchemaColumn ExpectedChildBizOPKOrNK { get; }
		protected abstract Type TypeOfBusinessObjectToQuery { get; }
		protected abstract FilterCategory FilterCategoryOfChildFilter { get; }
		protected abstract string ChildFilterName { get; }
		protected abstract ModuleTextFilter DuplicateFilter(string description);
	}
}
