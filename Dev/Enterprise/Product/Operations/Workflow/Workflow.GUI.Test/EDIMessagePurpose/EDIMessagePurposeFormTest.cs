using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(EDIMessagePurposeForm))]
	class EDIMessagePurposeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new EDIMessagePurposeForm(Factory.New<EDIMessagePurpose>());
	}
}
