using System;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommunicationNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		#region Implementation

		protected override Type BizOTypeToTest => typeof(OrgSalesCall);

		protected override SchemaColumn ColumnThatUsesNumberFountain => OrgSalesCallSchema.OQ_CommunicationID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.CommunicationID;

		#endregion
	}
}
