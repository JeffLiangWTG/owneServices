using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class SummaryEntryPanelTest : TestCase
	{
		public void TestTag()
		{
			using (var panel = new SummaryEntryPanel())
			{
				AssertEquals(RatingConstants.RateCategory.SummaryRatesCategory, panel.Tag.ToString());
			}
		}
	}
}
