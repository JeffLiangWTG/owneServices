using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI
{
	public partial class PersonMergePreviewUserControl : ZUserControl
	{
		public PersonMergePreviewUserControl(GlbPerson mergedPerson, PersonMergePreview personMergePreview, string discardedPersonName)
		{
			InitializeComponent();
			this.mergedPerson = mergedPerson;
			this.personMergePreview = personMergePreview;
			this.discardedPersonName = discardedPersonName;

			SetDefaultHeight();
			SetGroupBoxCaption();
			PopulatePanel(LeftBottomLayoutPanel, personMergePreview.MergedProperties);
			PopulatePanel(RightBottomLayoutPanel, personMergePreview.DiscardedProperties);
			UpdateHeight();
		}

		void SetDefaultHeight()
		{
			Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(defaultHeight);
		}

		void SetGroupBoxCaption()
		{
			MergedPropertiesGroupBox.Text = Res.GetString("93AB6832-1D89-428A-BA98-AB2DF22DB677", "After merge : {0}", mergedPerson.PER_FullName);
			DiscardedPropertiesGroupBox.Text = Res.GetString("13AB6832-1289-428A-BA98-CB2DF22DB677", "Information discarded from : {0}", discardedPersonName);
		}

		void PopulatePanel(KTableLayoutPanel panel, CodeDescriptionPairList properties)
		{
			var itemsOrder = SystemDataRegistry.Instance.PersonMergePreviewItemsRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var propertiesToShow = new CodeDescriptionPairList();

			foreach (PersonMergePreviewItem potentialItem in itemsOrder)
			{
				if (potentialItem.Visibility)
				{
					var propertyValue = properties.GetDescriptionFromCode(potentialItem.ColumnName);
					if (propertyValue != null)
					{
						propertiesToShow.Add(new CodeDescriptionPair(potentialItem.ColumnName.ToString(), propertyValue));
					}
				}
			}

			if (propertiesToShow.Count > 0)
			{
				panel.RowCount = 0;
				panel.RowStyles.RemoveAt(0); // Remove unneeded default style for first row
				var propertiesShown = 0;
				var addressAdded = false;

				foreach (CodeDescriptionPair currentProperty in propertiesToShow)
				{
					if (personMergePreview.addressColumns.Contains(currentProperty.Code))
					{
						if (!addressAdded)
						{
							AddPersonMergeLabelControlToPanel(panel, ResString.GetMultilingualString("ff730b45-a127-4179-af26-322c45fac4e0", "Address"), GetFormattedAddress(propertiesToShow), propertiesShown);
							addressAdded = true;
							propertiesShown++;
						}
					}
					else
					{
						AddPersonMergeLabelControlToPanel(panel, GetHumanReadableName(currentProperty.Code), currentProperty.Description, propertiesShown);
						propertiesShown++;
					}
				}
			}
		}

		string GetFormattedAddress(CodeDescriptionPairList properties)
		{
			var address1 = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_HomeAddress1);
			var address2 = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_HomeAddress2);
			var city = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_City);
			var state = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_State);
			var postcode = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_Postcode);
			var countryCode = GetPropertyByColumnNameIfExists(properties, GlbPersonSchema.Constants.PER_RN_NKCountry);

			return new AddressFormatter(mergedPerson.Factory, null, null, address1, address2, city, state, postcode, (NoResString)countryCode, null, true, countryCode).PostalAddressWithoutCompanyName();
		}

		string GetPropertyByColumnNameIfExists(CodeDescriptionPairList propertiesToShow, string columnName)
		{
			return propertiesToShow.GetDescriptionFromCode(columnName) ?? string.Empty;
		}

		void AddPersonMergeLabelControlToPanel(KTableLayoutPanel panel, string code, string description, int rowNumber)
		{
			var personMergePreviewLabelUserControl = new PersonMergePreviewLabelUserControl(code, description)
			{
				Name = "PersonMergePreviewLabelUserControl" + rowNumber,
				Dock = DockStyle.Fill,
			};

			panel.Controls.Add(personMergePreviewLabelUserControl, 0, rowNumber);
			panel.RowCount = rowNumber + 1;
		}

		string GetHumanReadableName(string columnName)
		{
			return mergedPerson.FindPropertyInfo(columnName)?.HumanReadableName;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "ControlDpiScalingHelper is already used")]
		void UpdateHeight()
		{
			Height += LeftBottomLayoutPanel.Height > RightBottomLayoutPanel.Height ? LeftBottomLayoutPanel.Height : RightBottomLayoutPanel.Height;
			if (personMergePreview.MergedProperties.Count > maximumItemsToShow || personMergePreview.DiscardedProperties.Count > maximumItemsToShow)
			{
				Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(defaultHeight + maximumItemsToShow * previewLabelHeight);
			}
		}

		#region Implementation

		readonly GlbPerson mergedPerson;

		readonly string discardedPersonName;

		readonly PersonMergePreview personMergePreview;

		const int defaultHeight = 28;

		const int maximumItemsToShow = 15;

		const int previewLabelHeight = 25;

		#endregion
	}
}
