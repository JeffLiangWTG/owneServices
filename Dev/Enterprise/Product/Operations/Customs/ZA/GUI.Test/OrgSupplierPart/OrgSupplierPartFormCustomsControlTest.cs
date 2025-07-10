using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using UniversalReferenceConstants = Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlTest : OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestControlsVisibility()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			var partPivot = part.PivotsForBinding.AddNew();
			using (var form = new MasterFiles.GUI.OrgSupplierPartForm(part))
			{
				using (var control = new OrgSupplierPartFormCustomsControlGlobal())
				{
					form.Controls.Add(control);
					form.Show();
					CombineAssertions(() =>
					{
						//Exports
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
						Assert(!control.PreferenceDropEdit.Visible);
						Assert(control.ROOTypeDropEdit.Visible);
						Assert(control.RulesOfOriginCertificateTextBox.Visible);
						//Imports
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
						Assert(control.PreferenceDropEdit.Visible);
						Assert(!control.ROOTypeDropEdit.Visible);
						Assert(control.RulesOfOriginCertificateTextBox.Visible);
						//Both
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
						Assert(!control.PreferenceDropEdit.Visible);
						Assert(!control.ROOTypeDropEdit.Visible);
						Assert(!control.RulesOfOriginCertificateTextBox.Visible);
					});
				}
			}
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartFormCustomsControlGlobal();

		protected override string ExpectedTariffColumnName => CusClassPartPivotSchema.Constants.CI_TariffNum;
		protected override string ExpectedCustomsCountryCode => Core.Constants.CountryCodes.SouthAfrica;
		protected override string ExpectedDataGrouping => Core.Constants.CountryCodes.SouthAfrica;
		protected override string ExpectedUniversalTariffType => UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
	}
}
