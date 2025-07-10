using System;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentInstructionTmpl))]
	class DtbConsignmentInstructionTmplTest : DtbTransportInstructionTmplTest
	{
		#region Related Entities

		#region TestTemplate

		protected override Type ExpectedTemplateType
		{
			get { return typeof(DtbConsignmentTmpl); }
		}

		#endregion

		#endregion

		#region TestLookups

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbConsignmentInstructionTmplLookups); }
		}

		#endregion

		#region TestValidation

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbConsignmentInstructionTmplValidation); }
		}

		#endregion
	}
}
