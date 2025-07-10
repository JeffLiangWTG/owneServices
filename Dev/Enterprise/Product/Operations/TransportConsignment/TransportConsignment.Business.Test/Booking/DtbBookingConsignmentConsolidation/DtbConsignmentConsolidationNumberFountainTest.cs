using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbConsignmentConsolidationNumberFountainTest : DtbTransportConsolidationNumberFountainTest<DtbConsignmentConsolidation>
	{
		#region Implementation

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbConsignmentConsolidationID; }
		}

		protected override ZString ConsolidationJobType
		{
			get { return TransportConsolidationJobTypes.Codes.Consignment; }
		}

		#endregion
	}
}
