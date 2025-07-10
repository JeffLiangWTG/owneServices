using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(SearchFilterCriteriaInfo))]
	public class SearchFilterCriteriaInfoTestCase : DataObjectInfoTestCase<SearchFilterCriteriaInfo>
	{
		#region TestClientCode

		public void TestClientCode()
		{
			var criteria = new SearchFilterCriteriaInfo();
			AssertEquals("", criteria.AreaCode);
			AssertEquals("", criteria.ClientCode);
			AssertEquals("", criteria.PickMethod);
			AssertEquals((short)0, criteria.PickGroup);
			AssertEquals("", criteria.EquipmentRegistrationNumber);

			criteria.AreaCode = "AREA";
			criteria.PickMethod = "EQP";
			criteria.PickGroup = 2;
			criteria.ClientCode = "CLIENTCODE";
			criteria.EquipmentRegistrationNumber = "RegNo";
			AssertEquals("AREA", criteria.AreaCode);
			AssertEquals("CLIENTCODE", criteria.ClientCode);
			AssertEquals("EQP", criteria.PickMethod);
			AssertEquals((short)2, criteria.PickGroup);
			AssertEquals("RegNo", criteria.EquipmentRegistrationNumber);
		}

		#endregion

		protected new SearchFilterCriteriaInfo Parent => (SearchFilterCriteriaInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new SearchFilterCriteriaInfo();
		}
	}
}
