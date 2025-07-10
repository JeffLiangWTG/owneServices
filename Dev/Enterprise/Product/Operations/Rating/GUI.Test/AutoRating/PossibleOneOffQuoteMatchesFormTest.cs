using System.Windows.Forms;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(PossibleOneOffQuoteMatchesForm))]
	public class PossibleOneOffQuoteMatchesFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PossibleOneOffQuoteMatchesForm(new SimpleOneOffQuoteCollectionWrapper(new QuoteCollection(Factory)));
		}
	}
}
