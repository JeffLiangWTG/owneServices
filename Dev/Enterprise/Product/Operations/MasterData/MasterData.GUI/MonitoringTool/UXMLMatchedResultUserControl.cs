using System.Drawing;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterData.GUI
{
	public partial class UXMLMatchedResultUserControl : ZUserControl
	{
		public UXMLMatchedResultUserControl(UXMLMatchingDiagnosticModel model)
		{
			vm = model;

			InitializeComponent();

			AddDataControl(UXMLMatchingDiagnosticModel.DataTypes.OrgMatchInfo, model.OrgMatchInfo);

			if (model.MatchedAddressDict != null)
			{
				foreach (var addressDetail in model.MatchedAddressDict)
				{
					AddDataControl(addressDetail.Key, addressDetail.Value);
				}
			}

			AddDataControl(UXMLMatchingDiagnosticModel.DataTypes.OrgScore, model.OrgScore);
			AddDataControl(UXMLMatchingDiagnosticModel.DataTypes.AddressScore, model.AddressScore);
			AddDataControl(UXMLMatchingDiagnosticModel.DataTypes.OrgMatchActiveStatus, model.MatchOrgActiveStatus.ToString());
			AddDataControl(UXMLMatchingDiagnosticModel.DataTypes.Result, model.Result);

			layoutZpanel.RowCount = layoutZpanel.RowCount + 1;
			layoutZpanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			layoutZpanel.Controls.Add(detailsLinkLabel, 0, layoutZpanel.RowCount - 1);
			layoutZpanel.SetColumnSpan(detailsLinkLabel, 2);

			layoutZpanel.ResumeLayout(false);
			layoutZpanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		readonly UXMLMatchingDiagnosticModel vm;

		internal void SetBackColor(Color color)
		{
			BackColor = color;
			foreach (var component in components.Components)
			{
				if (component is ZLabel label)
				{
					label.BackColor = color;
				}
			}
		}

		void AddDataControl(UXMLMatchingDiagnosticModel.DataTypes dataType, string data)
		{
			var textBox = new ZTextBox();
			components.Add(textBox);
			textBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			textBox.BackColor = Color.White;
			textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			textBox.ForeColor = SystemColors.WindowText;
			textBox.Name = dataType.ToString() + "TextBox";
			textBox.ReadOnly = true;
			textBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			textBox.Text = data;

			var label = new ZLabel();
			components.Add(label);
			label.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			label.BackColor = Color.White;
			label.BorderStyle = System.Windows.Forms.BorderStyle.None;
			label.CaptionResourceString = GetLabelText(dataType);
			label.Name = dataType.ToString() + "Label";
			label.AutoSize = true;

			var row = layoutZpanel.RowCount;
			layoutZpanel.RowCount = layoutZpanel.RowCount + 1;
			layoutZpanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			layoutZpanel.Controls.Add(label, 0, row);
			layoutZpanel.Controls.Add(textBox, 1, row);
		}

		ResourceStringData GetLabelText(UXMLMatchingDiagnosticModel.DataTypes dataType)
		{
			// todo: fix if C# has any better KV structures
			ResourceStringData result = Res.GetData("ff664400-fa5f-441c-a709-7088bca990f9", "Match");
			switch (dataType)
			{
				case UXMLMatchingDiagnosticModel.DataTypes.OrgMatchInfo:
					result = Res.GetData("50B55448-4E64-49D0-90E6-E33202D93516", "Org. Info");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.AdditionalAddress:
					result = Res.GetData("8EC68ADF-2C8C-4958-B354-05C059D035D3", "Additional Address");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.Address1:
					result = Res.GetData("353FD8BE-10BA-4757-9AA6-75220B01133C", "Address 1");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.Address2:
					result = Res.GetData("64E17DA2-EC8A-4368-9AD3-AC6DC456341E", "Address 2");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.AddressShortCode:
					result = Res.GetData("9C1776D9-74A7-4F19-B9F5-523302853CE1", "Address Short Code");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.City:
					result = Res.GetData("51C2C7B3-0FD5-4A63-98A5-8E95D4FB1CA1", "City");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.State:
					result = Res.GetData("1074FB1D-3B20-42CA-AADE-17AA58932AB4", "State");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.Email:
					result = Res.GetData("04B9C3A8-7067-45BA-8978-D7DB94CE2AED", "Email");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.PostCode:
					result = Res.GetData("7DBC07C7-4C8F-4822-90FE-E107FAC66586", "Post Code");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.Country:
					result = Res.GetData("1161D07E-91CC-4551-B379-A55ABF46CEB1", "Country/Region");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.OrgScore:
					result = Res.GetData("60CC9E16-7766-4DB7-B06E-5655EB41C6C6", "Org. Score");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.AddressScore:
					result = Res.GetData("2391404D-F10D-4353-AD1C-0764C7DCFB36", "Address Score");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.OrgMatchActiveStatus:
					result = Res.GetData("09FA7644-7E17-4395-ABC1-1D81FA95A6AD", "Active Status");
					break;
				case UXMLMatchingDiagnosticModel.DataTypes.Result:
					result = Res.GetData("eea0cc67-8e7b-4ea1-b020-7b46fd40d613", "Result");
					break;
			}
			return result;
		}

		void detailsLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ShowEditForm();
		}

		void ShowEditForm()
		{
			if (viewEntityFormCheckPoint.IsAllowed)
			{
				var bizO = vm.MatchedOrg ?? factory.Load<OrgHeader>(vm.OrgPK);
				if (bizO != null)
				{
					var controller = ZControllerFactory.Instance.GetControllerForBizo(bizO) ?? ZControllerFactory.Instance.GetControllerForType(typeof(OrgHeader));
					if (controller == null)
					{
						Globals.Message.ShowInformation(Res.GetString("07322675-7A79-4EA8-9BAD-DDB2483AA946", "You do not have appropriate controller for the Business Object: Type[{0}] PK[{1}]", bizO.GetType().Name, vm.OrgPK));
					}
					else
					{
						//Use ShowEditFormUrlHandler instead of calling "controller.ShowEditForm" directly to fix this error message:
						//"This type of CollectionView does not support changes to its SourceCollection from a thread different from the Dispatcher thread."
						//That because when showing the edit form, the system was trying to update recent items as well which were created from the main UI thread.
						var url = ShowEditFormUrlHandler.Instance.Create(controller.ID, bizO.PK);
						EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
						ParentForm.TopMost = true; //bring up UXMLMatchingDiagnosticToolForm to the front
					}
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("E5A0BA28-02D8-4E05-966F-C9D698A68D4E", "This item have been removed from the system, please click button to re-calculate matched items."));
				}
			}
			else
			{
				viewEntityFormCheckPoint.ShowError();
			}
		}

		SecurityCheckpoint viewEntityFormCheckPoint => Env.Security.OrganisationView;

		readonly BusinessObjectFactory factory = new BusinessObjectFactory();
	}
}
