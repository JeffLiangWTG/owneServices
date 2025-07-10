using System;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolidatedTransportBookingNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(CommonConsolidatedTransportBooking); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return JobConsolidatedTransportBookingSchema.D1_UniqueConsignRef; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.JobConsolidatedTransportBookingNumber; }
		}
	}
}
