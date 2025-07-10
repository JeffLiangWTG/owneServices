using System.Windows.Forms;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(BuildConsolForm))]
	public class BuildConsolFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Freight.Business.Testing.SailingsForTestClasses helper = new Freight.Business.Testing.SailingsForTestClasses(Factory);
			PackContainerHelper sailingHelper = new PackContainerHelper(helper.SydLaxSailing);
			return new BuildConsolForm(sailingHelper);
		}
	}
}
