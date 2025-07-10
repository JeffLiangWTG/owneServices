using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Test
{
	[TestedType(typeof(MapChargeCodesForm))]
	public class MapChargeCodesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MapChargeCodesForm();
		}
	}
}
