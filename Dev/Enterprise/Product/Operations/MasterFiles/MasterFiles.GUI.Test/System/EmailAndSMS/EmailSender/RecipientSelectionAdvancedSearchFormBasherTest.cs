using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(RecipientSelectionAdvancedSearchForm))]
	sealed class RecipientSelectionAdvancedSearchFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RecipientSelectionAdvancedSearchForm(new RecipientSelection(new AddressBookSelection()));
		}

		#endregion
	}
}
