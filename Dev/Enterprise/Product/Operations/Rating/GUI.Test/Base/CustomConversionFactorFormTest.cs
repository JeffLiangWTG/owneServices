using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(CustomConversionFactorForm))]
	public class CustomConversionFactorFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CustomConversionFactorForm();
		}
	}
}
