using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ProgressFormDirectUpdateLabel))]
	public class ProgressFormDirectUpdateLabelBasherTest : ProgressFormBasherTest
	{
		protected override Form GetFormToBashCore() => new ProgressFormDirectUpdateLabel();
	}
}
