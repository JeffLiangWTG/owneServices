using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineSyncrhoniserForm))]
	public class CusEntryLineSyncrhoniserFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new CusEntryLineSyncrhoniserForm(new CusEntryLineSyncroniser(new CusEntryLineBuilder().GetEntryLineWithTwoInvoiceLines()));
		public void TestGridId()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var synchroniser = new CusEntryLineSyncroniser(entryLine);
			using (var form = new CusEntryLineSyncrhoniserForm(synchroniser))
			{
				var permitCodeGrid = (ZGrid)form.Controls.Find("PermitCodesGrid", true).Single();
				var prohibitedCodesGrid = (ZGrid)form.Controls.Find("ProhibittedCodesGrid", true).Single();
				var otherInfosGrid = (ZGrid)form.Controls.Find("OtherInfosGrid", true).Single();
				CombineAssertions(() =>
				{
					AssertEquals("ac88b0b4-bd93-4fa0-afd8-1d38dc242166", permitCodeGrid.GridId);
					AssertEquals("533d7d96-efe2-4e63-a0e3-c76dbb270271", prohibitedCodesGrid.GridId);
					AssertEquals("29b98c60-f2fd-40e5-9207-684bdfdd1762", otherInfosGrid.GridId);
				}

				);
			}
		}
	}
}
