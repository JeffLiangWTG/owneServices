using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(EntryDatesForm))]
	public class EntryDatesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EntryDatesForm(new EntryStartEndDates(ZDate.Today, ZDate.Today.AddMonths(6)));
		}
	}
}
