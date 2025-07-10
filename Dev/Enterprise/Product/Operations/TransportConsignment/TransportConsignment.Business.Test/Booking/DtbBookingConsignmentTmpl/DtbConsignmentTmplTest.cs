using System;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentTmpl))]
	class DtbConsignmentTmplTest : DtbTransportTmplTest
	{
		#region Related Entities

		#region Instructions

		protected override Type ExpectedTemplateInstructionCollectionType
		{
			get { return typeof(DtbConsignmentInstructionTmplCollection); }
		}

		#endregion

		#endregion

		#region TestLookups

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbConsignmentTmplLookups); }
		}

		#endregion

		#region TestValidation

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbConsignmentTmplValidation); }
		}

		#endregion
	}
}
