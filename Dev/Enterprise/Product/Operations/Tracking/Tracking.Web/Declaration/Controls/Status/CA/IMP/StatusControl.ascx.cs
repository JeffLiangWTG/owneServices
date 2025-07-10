using System.Web.UI;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Tracking.Web.Declaration.CA.IMP
{
	[ToolboxData("<{0}:StatusControl runat=server></{0}:StatusControl>")]
	public partial class StatusControl : BaseStatusControl, ISelfBindingWebControl
	{
		public override void Bind(object dataSource)
		{
			if (dataSource is TrackingShipment && ((TrackingShipment)dataSource).LastImportDeclaration == null)
			{
				base.BindControl<TrackingShipment>(dataSource);
			}
			else
			{
				base.BindControl<IJobDeclaration>(dataSource);
			}
		}

		protected override void OnPreRender(System.EventArgs e)
		{
			base.OnPreRender(e);
			DeliveryInstructionSeparator.Visible = !string.IsNullOrEmpty(DeliveryInstructions2Text.Text);
			WrapperReleaseOfficeText.Visible = !string.IsNullOrEmpty(WrapperReleaseOfficeText.Text);
			ReleaseOfficeText.Visible = !WrapperReleaseOfficeText.Visible && !string.IsNullOrEmpty(ReleaseOfficeText.Text);
			EffectiveCCNText.Visible = string.IsNullOrEmpty(CCNText.Text);
			DeclarationDatesRow.Visible = DataSource is IJobDeclaration;
		}

		public override bool SkipDataBind()
		{
			if (DataSource != null)
			{
				IReleaseStatusWrapper releaseStatusWrapper = null;
				var customOffice = string.Empty;
				if (DataSource is TrackingShipment shipment)
				{
					releaseStatusWrapper = shipment.ReleaseStatusWrapper;
				}
				else if (DataSource is IJobDeclaration declaration)
				{
					releaseStatusWrapper = declaration.ReleaseStatusWrapper;
					customOffice = declaration.JE_CustomsOffice;
				}

				if (releaseStatusWrapper != null && releaseStatusWrapper is IReleaseStatusWrapper releaseStatusDocumentWrapper)
				{
					return string.IsNullOrEmpty(releaseStatusDocumentWrapper.ProcessingIndicatorDescription) && string.IsNullOrEmpty(customOffice);
				}
			}

			return true;
		}
	}
}
