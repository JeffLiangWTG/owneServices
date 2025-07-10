using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(PersonMergeConfirmationForm))]
	public class PersonMergeConfirmationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PersonMergeConfirmationForm();
		}
	}
}
