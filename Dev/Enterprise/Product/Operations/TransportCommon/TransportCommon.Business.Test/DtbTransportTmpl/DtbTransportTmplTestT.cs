using System;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportTmplTest : DtbTransportBusinessObjectTestCase
	{
		#region Related Entities

		#region TestInstructions

		public void TestInstructions()
		{
			var template = (DtbTransportTmpl)GetNewBusinessObject();

			AssertEquals("Instructions should be registered editable on Transport Booking Template", true, template.IsRegisteredEditableChildObject(template.Instructions));
			AssertEquals(ExpectedTemplateInstructionCollectionType, template.Instructions.GetType());
		}

		protected abstract Type ExpectedTemplateInstructionCollectionType { get; }

		#endregion

		#endregion
	}
}
