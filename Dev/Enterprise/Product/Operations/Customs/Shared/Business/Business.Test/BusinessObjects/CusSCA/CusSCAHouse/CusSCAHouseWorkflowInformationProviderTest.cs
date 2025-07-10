using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSCAHouseWorkflowInformationProviderTest : TestCaseWithFactory
	{
		public void TestAll()
		{
			var bill = Factory.New<TestCusSCAOceanBill>();
			var house = Factory.New<CusSCAHouseForTest>();
			house.CA_CB = bill.PK;
			house.CA_RL_NKLoadPort = "LOAD1";
			house.CA_RL_NKDischargePort = "DEST1";
			var workflowInfo = ((IWorkflowProvider)house).GetWorkflowInformationProvider();
			AssertNotNull(workflowInfo);
			AssertEquals("DEST1", workflowInfo.Destination);
			AssertEquals("LOAD1", workflowInfo.Origin);
			var workflowInfo2 = ((IWorkflowProvider)house).GetWorkflowInformationProvider();
			AssertEquals("Cached", workflowInfo2, workflowInfo);
			AssertEquals(bill.Branch.Company.PK, workflowInfo.Companies.First());
			house.CA_CB = ZGuid.Empty;
			AssertEquals(0, workflowInfo.Companies.Count());
		}

		class CusSCAHouseForTest : BaseCusSCAHouse
		{
			public CusSCAHouseForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection()
			{
				throw new NotImplementedException();
			}
		}
	}
}
