using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(NZImportTCOBulkChangeForm))]
	class NZImportTCOBulkChangeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NZImportTCOBulkChangeForm(new NZTariffBulkChange(Factory));
		}

		protected override bool AllowSaveOnFormForTestHasChanges
		{
			get
			{
				return false;
			}
		}

		public void TestNotSetHasChangesInOnshow()
		{
			var bizo = new NZTariffBulkChange(Factory);
			using (var form = new NZImportTCOBulkChangeFormForTest(bizo))
			{
				form.OnShowForTest();
				Assert(!bizo.HasChanges);
			}
		}

		class NZImportTCOBulkChangeFormForTest : NZImportTCOBulkChangeForm
		{
			public NZImportTCOBulkChangeFormForTest(NZTariffBulkChange businessEntity) : base(businessEntity)
			{
			}

			public void OnShowForTest()
			{
				OnShown(new System.EventArgs());
			}
		}
	}
}
