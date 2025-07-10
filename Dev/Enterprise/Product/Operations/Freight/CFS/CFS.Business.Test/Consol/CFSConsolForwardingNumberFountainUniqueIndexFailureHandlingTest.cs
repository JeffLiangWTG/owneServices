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
	public class CFSConsolForwardingNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
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
			get { return Env.NumberFountains.JobConsolNumber; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			testBizO[JobConsolSchema.Constants.JK_IsCFS] = ZBool.False;
			testBizO[JobConsolSchema.Constants.JK_IsForwarding] = ZBool.True;
		}
	}
}
