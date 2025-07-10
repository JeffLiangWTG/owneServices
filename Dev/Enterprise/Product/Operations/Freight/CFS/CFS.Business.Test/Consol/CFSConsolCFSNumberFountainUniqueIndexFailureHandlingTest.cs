using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSConsolCFSNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return JobConsolSchema.JK_UniqueConsignRef; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.JobConsolNumberCFS; }
		}

		public void TestDefaultCFSShipmentValues()
		{
			BusinessObject testBizO = new BusinessObjectFactory().New(BizOTypeToTest);

			AssertEquals("Shipment should be CFS", ZBool.True, (ZBool)testBizO[JobConsolSchema.Constants.JK_IsCFS]);
			AssertEquals("Shipment should NOT be Forwarding", ZBool.False, (ZBool)testBizO[JobConsolSchema.Constants.JK_IsForwarding]);
		}
	}
}
