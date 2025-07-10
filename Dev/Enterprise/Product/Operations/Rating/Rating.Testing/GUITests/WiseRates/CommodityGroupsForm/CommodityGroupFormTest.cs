using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Test
{
	[TestedType(typeof(CommodityGroupForm))]
	public class CommodityGroupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CommodityGroupForm(new CommodityGroupViewModelCollection());
		}
	}
}
