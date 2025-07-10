using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Test
{
	sealed class ConsolidatedDeclarationDetailsaUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ConsolidatedDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestEntryNumberTextBox()
		{
			CombineAssertions(() =>
			{
				var entryNumberControl = control.EntryNumberTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, entryNumberControl.CharacterCasing);
				AssertType<ZTextBox>("Type", entryNumberControl);
			});
		}

		public void TestEntryStatusTextBox()
		{
			CombineAssertions(() =>
			{
				var entryStatusControl = control.EntryStatusTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Upper, entryStatusControl.CharacterCasing);
				AssertType<ZTextBox>("Type", entryStatusControl);
			});
		}

		public void TestTSWCombinedStatusTextBox()
		{
			CombineAssertions(() =>
			{
				var tSWCombinedStatusControl = control.TSWCombinedStatusTextBox;
				AssertEquals("CharacterCasing", CharacterCasing.Normal, tSWCombinedStatusControl.CharacterCasing);
				AssertType<ZTextBox>("Type", tSWCombinedStatusControl);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ConsolidatedDeclarationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ConsolidatedDeclarationDetailsUserControl control;
	}
}
