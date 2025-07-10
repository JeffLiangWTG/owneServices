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
	public class CFSShipmentCFSNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(CFSShipment); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return JobShipmentSchema.JS_UniqueConsignRef; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.JobShipmentNumberCFS; }
		}

		public void TestDefaultCFSShipmentValues()
		{
			var testBizO = new BusinessObjectFactory().New(BizOTypeToTest);

			AssertEquals("Shipment should be CFS", ZBool.True, (ZBool)testBizO[JobShipmentSchema.Constants.JS_IsCFSRegistered]);
			AssertEquals("Shipment should NOT be Forwarding", ZBool.False, (ZBool)testBizO[JobShipmentSchema.Constants.JS_IsForwardRegistered]);
		}
	}
}
