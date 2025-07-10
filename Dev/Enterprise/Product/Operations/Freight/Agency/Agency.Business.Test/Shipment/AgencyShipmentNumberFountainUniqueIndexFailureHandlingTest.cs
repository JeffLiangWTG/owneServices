using System;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		#region Implementation
		protected override Type BizOTypeToTest
		{
			get
			{
				return typeof(AgencyShipment);
			}
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get
			{
				return JobShipmentSchema.JS_UniqueConsignRef;
			}
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get
			{
				return Env.NumberFountains.JobShipmentNumberAgency;
			}
		}
		#endregion
	}
}
