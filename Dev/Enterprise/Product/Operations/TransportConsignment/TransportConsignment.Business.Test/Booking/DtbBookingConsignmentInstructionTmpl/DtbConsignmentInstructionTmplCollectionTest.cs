using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentInstructionTmplCollection))]
	class DtbConsignmentInstructionTmplCollectionTest : DtbTransportInstructionTmplCollectionTest<DtbConsignmentInstructionTmplCollection>
	{
		#region Implementation

		protected override DtbConsignmentInstructionTmplCollection GetCollectionToTest()
		{
			return new DtbConsignmentInstructionTmplCollection((DtbConsignmentTmpl)GetNewTemplate());
		}

		protected override DtbTransportTmpl GetNewTemplate()
		{
			return Factory.New<DtbConsignmentTmpl>();
		}

		#endregion
	}
}
