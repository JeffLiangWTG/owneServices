using System;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionTmplTest : DtbTransportBusinessObjectTestCase
	{
		#region Related Entities

		#region TestTemplate

		public void TestTemplate()
		{
			var template = Factory.New(ExpectedTemplateType);
			var instructionTemplate = (DtbTransportInstructionTmpl)GetNewBusinessObject();

			instructionTemplate.K2_KT_BookingTmpl = template.PK;
			AssertEquals(template, instructionTemplate.Template);
		}

		protected abstract Type ExpectedTemplateType { get; }

		#endregion

		#endregion
	}
}
