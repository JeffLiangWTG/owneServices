using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Web.Declaration.CA.IMP;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class CanadaImportTestStatusControl : StatusControl
	{
		public void SetUpControlForTesting()
		{
			DeliveryInstructionSeparator = new Label();
			DeliveryInstructions2Text = new ZTextLabel();
			WrapperReleaseOfficeText = new ZTextLabel();
			ReleaseOfficeText = new ZCodeFindBoxLabel();
			CCNText = new ZTextLabel();
			EffectiveCCNText = new ZTextLabel();
			DeclarationDatesRow = new HtmlTableRow();
		}

		public void OnPreRenderForTesting() => OnPreRender(new EventArgs());
		public ZTextLabel WrapperReleaseOfficeTextForTesting => WrapperReleaseOfficeText;
		public ZCodeFindBoxLabel ReleaseOfficeTextForTesting => ReleaseOfficeText;
		public ZTextLabel CCNTextForTesting => CCNText;
		public ZTextLabel EffectiveCCNTextForTesting => EffectiveCCNText;
		public HtmlTableRow DeclarationDatesRowForTesting => DeclarationDatesRow;

		public void SetDataSourceForTest(BusinessObject dataSource)
		{
			DataSource = dataSource;
		}
	}
}
