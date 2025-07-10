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
	public class CFSShipmentForwardingNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
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
			get { return Env.NumberFountains.JobShipmentNumber; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			testBizO[JobShipmentSchema.Constants.JS_IsCFSRegistered] = ZBool.False;
			testBizO[JobShipmentSchema.Constants.JS_IsForwardRegistered] = ZBool.True;
		}
	}
}
