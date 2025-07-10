using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(DeliveryDetailsPopupForm))]
	sealed class DeliveryDetailsPopupFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DeliveryDetailsPopupForm();
		}

		#endregion
	}
}
