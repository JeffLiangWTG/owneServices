using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class NonPersistentAddressOverrideCombinationControlTest : ZMultiCombinationControlTest
	{
		public override ZMultiCombinationControl GetNewMultiCombinationControl()
		{
			return new NonPersistentAddressOverrideCombinationControl<DocDeliveryContact>(new NonPersistentAddressOverrideColumnStyleInfo<DocDeliveryContact>());
		}

		public void RunSelectNonPersistentCopyRecipientsFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(NonPersistentCopyRecipientsFindBox), false);
		}

		public void RunSelectTextBox()
		{
			RunBindToListTest(FieldType.Text, typeof(TextBox), false);
		}

		[RequiresSTA]
		public new void TestShowingCodeFindBox()
		{
			RunBindToListTest(FieldType.TextCodeFindBox, typeof(NonPersistentCopyRecipientsFindBox), false);
		}
	}
}
