using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class ContainerLoadPlanControlBashFetchTest : FilterControlBashFetchHintTest<CFSContainerLoadList>
	{
		protected override SchemaPKColumn PkColumn => ContainerLoadListHeaderSchema.PK;
		#region BashFetchTest

		public void TestBashFetchForView_CLH_LoadListId()
		{
			BashFetchForView("CLH_LoadListId", 0);
		}

		public void TestBashFetchForView_CLH_Status()
		{
			BashFetchForView("CLH_Status", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateUser()
		{
			BashFetchForView("CLH_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateBranch()
		{
			BashFetchForView("CLH_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateDepartment()
		{
			BashFetchForView("CLH_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_CLH_SystemCreateTimeUtc()
		{
			BashFetchForView("CLH_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_CLH_SystemLastEditUser()
		{
			BashFetchForView("CLH_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_CLH_SystemLastEditTimeUtc()
		{
			BashFetchForView("CLH_SystemLastEditTimeUtc", 0);
		}

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 12; i++)
			{
				var loadListHeader = factory.NewWithValidTestData<CFSContainerLoadList>();
				loadListHeader.CLH_LoadListId = "CLL0000" + i;
				loadListHeader.CLH_Status = "SHP";
				loadListHeader.CLH_SystemCreateTimeUtc = ZDateTime.Now;
				loadListHeader.CLH_SystemLastEditTimeUtc = ZDateTime.Now;

				result.Add(loadListHeader.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new ContainerLoadPlanCollection(Factory);
			var filterBusinessObject = new ContainerLoadPlanFilterBusinessObject();
			return new ContainerLoadPlanFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ContainerLoadPlanCollection(Factory);
		}

		#endregion
	}
}
