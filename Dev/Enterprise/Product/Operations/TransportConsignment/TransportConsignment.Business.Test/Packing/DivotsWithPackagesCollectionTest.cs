using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DivotsWithPackagesCollection<DtbConsignmentInstructionPkgDivot>))]
	sealed class DivotsWithPackagesCollectionTest : DivotsWithPackagesCollectionTest<DtbConsignmentInstructionPkgDivot>
	{
		#region Implementation

		protected override DtbTransportInstruction GetNewInstruction()
		{
			return Factory.New<DtbConsignmentInstruction>();
		}

		#endregion
	}
}
