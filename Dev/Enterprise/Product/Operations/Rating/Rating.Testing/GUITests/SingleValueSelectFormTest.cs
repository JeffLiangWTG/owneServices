using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Test
{
	[TestedType(typeof(SingleValueSelectForm))]
	public class SingleValueSelectFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SingleValueSelectForm();
		}
	}
}
