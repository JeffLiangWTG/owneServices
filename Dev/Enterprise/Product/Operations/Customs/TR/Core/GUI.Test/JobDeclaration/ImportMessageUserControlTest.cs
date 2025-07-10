using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusEntryHeaderCharges = Enterprise.Customs.TR.Business.Declaration.CusEntryHeaderCharges;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class ImportMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryFeesTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var messageUserControl = new ImportMessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var entryFeesTabPage = messageUserControl.FindSingle<ZTabPage>("EntryFeesTabPage");
				entryFeesTabPage.Show();

				var entryFeesGrid = (ZGrid)messageUserControl.Controls.Find("EntryFeesGrid", true).SingleOrDefault();

				CombineAssertions(() =>
				{
					AssertEquals("EntryFeesTabPage visible", true, entryFeesTabPage.TabVisible);
					AssertEquals("Caption", "Entry Fees", entryFeesGrid.CaptionText);
					AssertNotNull(entryFeesGrid + ": ChargeType column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeType));
					AssertNotNull(entryFeesGrid + ": Description column", entryFeesGrid.GetColumnStyle("DescriptionOfChargeType"));
					AssertNotNull(entryFeesGrid + ": Amount column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeAmount));
					AssertNotNull(entryFeesGrid + ": MethodOfPayment column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_MethodOfPayment));
					AssertNotNull(entryFeesGrid + ": Action column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_RateOverrideReasonCode));
					AssertNotNull(entryFeesGrid + ": Method of Calculation column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_Source));
				});
			}
		}
	}
}
