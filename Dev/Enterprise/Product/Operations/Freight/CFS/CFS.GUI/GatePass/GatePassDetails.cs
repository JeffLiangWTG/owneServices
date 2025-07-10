using System;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class GatePassDetails : ZUserControl
	{
		public GatePassDetails()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(ConsigneeDocumentaryDocAddressControl);

			if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Canada)
			{
				this.HouseCCNTextBox.Visible = false;
			}
		}

		public GatePassShipment GatePassShipment { get; set; }

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Details Button

		void DetailsButton_Click(object sender, EventArgs e)
		{
			if (GatePassShipment != null && !GatePassShipment.CustomsManualStatus.IsEmpty)
			{
				var form = new HtmlInterpretationForm(FormatManualReleaseInformation(GatePassShipment));
				try
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
				finally
				{
					form.Dispose();
				}
			}
			else
			{
				ZString customsInfo = GatePassShipment != null ? GatePassShipment.UserFriendlyStatus : ZString.Empty;
				if (!customsInfo.IsEmpty)
				{
					if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
					{
						ZFormModaliser.ShowDialogAndDispose(new HtmlInterpretationForm(customsInfo));
					}
					else
					{
						Globals.Message.ShowInformation(customsInfo, Res.GetString("8c9dafb1-3dc6-4e09-aaef-6a54e83a8be9", "Customs Information"));
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("26bf3687-153e-447d-9745-9059888e98f0", "No additional information is available."), Res.GetString("cad28c3d-e07c-4c70-8b00-7dc3dd9130d6", "Customs Information"));
				}
			}
		}

		ZString FormatManualReleaseInformation(CFSShipment shipment)
		{
			var builder = new ZStringBuilder(Res.GetString("A76D4323-92D0-493D-B2CB-E792FA367978", "Release Date:{0}", shipment.ManualReleaseDate));
			builder.Append(Res.GetString("21D2B6C9-7087-4C6A-86E8-5A32B334BC99", "Release Reason:{0}", shipment.ManualReleaseReason));
			builder.Append(Res.GetString("28B42AEE-3EF4-4729-B979-06697FE8663F", "Release User:{0}", shipment.ManualReleaseUser));
			builder.Append(Res.GetString("94DFFFE6-5AD6-410D-92EE-798856FDADD3", "System Date:{0}", shipment.ManualReleaseSystemDate));
			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}

