using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CPSCForm : ZChildForm
	{
		public CPSCForm(CPSCHeader header)
			: base(header)
		{
			InitializeComponent();
			this.header = header;
			RefreshWhenProcessingCodeChanged();
		}

		readonly CPSCHeader header;

		public bool LotsGridVisible
		{
			get => !LotsAndOtherSplitContainer.Panel1Collapsed;
			set => LotsAndOtherSplitContainer.Panel1Collapsed = !value;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void MoreCodesButton_Click(object sender, EventArgs e)
		{
			if (header != null && header.IsREF)
			{
				return;
			}

			var button = sender as ZButton;
			var soureString = ZString.Empty;
			var titleText = ZString.Empty;
			ZPropertyInfo propertyInfo = null;
			if (button != null && button.Tag != null && header != null)
			{
				var key = button.Tag.ToString();
				switch (key)
				{
					case CPSCHeader.Schema.US_ModelNumber:
						soureString = header.US_ModelNumber;
						propertyInfo = header.US_ModelNumberInfo;
						titleText = "Model Numbers";
						break;
					case CPSCHeader.Schema.US_SerialNumber:
						soureString = header.US_SerialNumber;
						propertyInfo = header.US_SerialNumberInfo;
						titleText = "Serial Numbers";
						break;
					case CPSCHeader.Schema.US_RegisteredNumber:
						soureString = header.US_RegisteredNumber;
						propertyInfo = header.US_RegisteredNumberInfo;
						titleText = "Registered Numbers";
						break;
					case CPSCHeader.Schema.US_AltenateID:
						soureString = header.US_AltenateID;
						propertyInfo = header.US_AltenateIDInfo;
						titleText = "Alternate IDs";
						break;
					case CPSCHeader.Schema.US_RuleCodes:
						soureString = header.US_RuleCodes;
						propertyInfo = header.US_RuleCodesInfo;
						titleText = "Citation / Exemption Numbers";
						break;
				}

				if (propertyInfo != null && !propertyInfo.ReadOnly)
				{
					CommaSeparatedNumberCollection commaSeparatedNumberCollection = new ItemIdentityNumbersMultipleCodeCollection(soureString, new BusinessObjectFactory());
					var codesForm = new AdditionalCodesForm(commaSeparatedNumberCollection, AdditionalCodesShowType.TextBoxColumnStyleInfo, titleText, 5);
					ZFormModaliser.ShowDialogAndDispose(codesForm, this);
					if (codesForm.DialogResult == System.Windows.Forms.DialogResult.OK)
					{
						propertyInfo.Value = commaSeparatedNumberCollection.GetCodesAsCommaSeparatedString();
						propertyInfo.RefreshBinding();
					}
				}
			}
		}

		void ProcessingCode_Changed(object sender, EventArgs e)
		{
			RefreshWhenProcessingCodeChanged();
		}

		void RefreshWhenProcessingCodeChanged()
		{
			if (header != null)
			{
				if (header.IsREF)
				{
					US_ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("849DEBD4-D941-4A99-8863-7664D7E70329", "Certifier ID No.");
					US_ReferenceNumberTextBox.UpdateCaption();
					BindingSource.SetBindingMember(US_ProductIDTextBox, "US_ProductCodeVersionNumber");
					US_ProductIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("806ED9C8-9BF6-4131-A795-FF0EE3FE368D", "Product Code Version Number");
					US_ProductIDTypeDropEdit.Visible = false;
					US_ProductCodeTextBox.Visible = true;
				}
				else
				{
					US_ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("52f6bbda-7dba-4857-bf2a-e894b85ae616", "Reference");
					US_ReferenceNumberTextBox.UpdateCaption();
					BindingSource.SetBindingMember(US_ProductIDTextBox, "US_ProductID");
					US_ProductIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("65b36589-9641-495a-b80d-a4750c4857af", "Product ID");
					US_ProductIDTypeDropEdit.Visible = true;
					US_ProductCodeTextBox.Visible = false;
				}
			}
		}
	}
}
