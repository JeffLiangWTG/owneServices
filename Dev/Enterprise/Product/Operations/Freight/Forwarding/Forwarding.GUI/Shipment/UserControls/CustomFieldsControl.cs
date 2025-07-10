using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CustomFieldsControl : ZUserControl
	{
		public CustomFieldsControl()
		{
			InitializeComponent();
			ShipmentCustomFields.NothingSetupMessageLabelText = Res.GetString("fb33f9d0-105e-4c49-96d5-99c03b30f5c5", "To make use of this tab, please setup shipment custom fields in Workflow Manager or in the Freight.Shipment section of the system registry.");
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			SetupCustomFieldsHint();
		}

		void SetupCustomFieldsHint()
		{
			if (DataSource != null)
			{
				var hintsDict = new Dictionary<string, string>();

				var properties = new UserDefinedPropertyCollection((BusinessObject)DataSource);

				var customFieldsDescriptor = new JobDocsAndCartageCustomFieldsDescriptor();
				foreach (var customFieldInfo in customFieldsDescriptor.ActiveCustomFieldsInfos)
				{
					var propertyName = customFieldsDescriptor.BindTo("DocsAndCartage", customFieldInfo); // binding path to custom field
					properties.Add(propertyName, customFieldInfo.Caption);

					var addedProperty = properties.GetProperties().FirstOrDefault(p => p.CustomColumnDefinition.Name == propertyName);
					if (addedProperty != null)
					{
						hintsDict[addedProperty.Identifier] = customFieldInfo.Hint;
					}
				}

				var rowLayoutPanel = ShipmentCustomFields.Controls.Find("rowLayoutPanel", true).FirstOrDefault();
				if (rowLayoutPanel != null)
				{
					foreach (Control childControl in rowLayoutPanel.Controls)
					{
						var bindingMember = childControl.GetBindingMember();

						if (hintsDict.ContainsKey(bindingMember)
							&& childControl is IResCaptionedControl resCaptionedControl
							&& !string.IsNullOrEmpty(hintsDict[bindingMember]))
						{
							resCaptionedControl.CaptionResourceString = new ResourceStringData("", hintsDict[bindingMember]);
						}
					}
				}
			}
		}
	}
}
