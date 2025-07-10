using System.Windows.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(PossibleMatchesForm))]
	public class PossibleMatchesFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PossibleMatchesForm(new PossibleMatchesWrapper(CostSell.Revenue));
		}

		public void TestRatingSuspendedOnFormLoad()
		{
			using (var form = new PossibleMatchesForm(new PossibleMatchesWrapper(CostSell.Revenue)))
			{
				Assert("AutoRating should be suspended while Possible Matches form is open", _Rating.IsSuspended);
			}

			Assert("AutoRating should be resumed when Possible Matches form is closed", !_Rating.IsSuspended);
		}
	}
}
