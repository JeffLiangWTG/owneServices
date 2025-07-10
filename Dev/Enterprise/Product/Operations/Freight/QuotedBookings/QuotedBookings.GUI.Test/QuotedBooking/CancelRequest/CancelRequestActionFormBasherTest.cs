using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	[TestedType(typeof(CancelRequestActionForm))]
	public class CancelRequestActionFormBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new CancelRequestActionForm();
		}
		#endregion
	}
}
