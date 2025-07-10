using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(NZTariffBulkChangeForm))]
	class NZTariffBulkChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NZTariffBulkChangeForm(new NZTariffBulkChange(Factory));
		}

		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get
			{
				return false;
			}
		}

		public void TestGridId()
		{
			var tariffBulkChange = new NZTariffBulkChange(Factory);
			using (var form = new NZTariffBulkChangeForm(tariffBulkChange))
			{
				var grid = (ZGrid)form.Controls.Find("OldTariffsZGrid", true).Single();
				AssertEquals("GridLayoutKX4qV9Ye2v3cyDdknN+4+Q==", grid.GridId);
			}
		}
	}
}
