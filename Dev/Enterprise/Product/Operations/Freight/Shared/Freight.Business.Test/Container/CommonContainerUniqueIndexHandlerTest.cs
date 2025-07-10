using System;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerUniqueIndexHandlerTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(CommonContainer);

		protected override SchemaColumn ColumnThatUsesNumberFountain => JobContainerSchema.JC_ContainerJobID;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.JobContainerJobID;
	}
}
