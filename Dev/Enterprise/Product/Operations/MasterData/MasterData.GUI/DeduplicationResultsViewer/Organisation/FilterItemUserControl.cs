using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class FilterItemUserControl : ZUserControl
	{
		public FilterItemUserControl()
		{
			InitializeComponent();
			base.SetDataBinding(OrgFilterItemDataSource, "");
			CreateCheckBoxes();
			HookValueChangedEvent();
		}

		void CreateCheckBoxes()
		{
			CheckBoxLayoutPanel.Controls.RemoveAndDisposeAll();

			foreach (var checkBoxItem in OrgFilterItemDataSource.CheckBoxItems)
			{
				var checkBox = new ZCheckBox();
				checkBox.Text = checkBoxItem.Label;
				checkBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
				checkBox.Name = checkBoxItem.Code + "CheckBox";
				checkBox.Size = ControlDpiScalingHelper.NewScaledSize(115, 24, true);
				checkBox.UseVisualStyleBackColor = true;
				checkBox.Checked = checkBoxItem.IsSelected;
				checkBox.Tag = checkBoxItem.Code;
				checkBox.CheckedChanged += (sender, obj) =>
				{
					checkBoxItem.IsSelected = checkBox.Checked;
				};

				CheckBoxLayoutPanel.Controls.Add(checkBox);
			}
		}

		public OrgFilterItemDataSource OrgFilterItemDataSource => _orgFilterItemDataSource ?? (_orgFilterItemDataSource = new OrgFilterItemDataSource());
		OrgFilterItemDataSource _orgFilterItemDataSource;

		public Dictionary<DynamicFilterConditionType, string> FilterConditionTypeDictionary { get; } =
			new Dictionary<DynamicFilterConditionType, string>
			{
				{ DynamicFilterConditionType.Contains, OrgFilterOptionList.Descriptions.Contains.GetUnresolvedString() },
				{ DynamicFilterConditionType.ExactMatch, OrgFilterOptionList.Descriptions.ExactMatch.GetUnresolvedString() },
				{ DynamicFilterConditionType.StartsWith, OrgFilterOptionList.Descriptions.StartsWith.GetUnresolvedString() },
				{ DynamicFilterConditionType.NotContain, OrgFilterOptionList.Descriptions.NotContain.GetUnresolvedString() },
				{ DynamicFilterConditionType.NotEqual, OrgFilterOptionList.Descriptions.NotEqual.GetUnresolvedString() },
				{ DynamicFilterConditionType.NotStartWith, OrgFilterOptionList.Descriptions.NotStartWith.GetUnresolvedString() }
			};

		public override void SetDataBinding(object dataSource, string dataMember)
		{
		}

		void RemoveFilterButton_Click(object sender, EventArgs e)
		{
			var parent = (KTableLayoutPanel)Parent;
			var otherFilters = parent.Controls.OfType<FilterItemUserControl>().Where(v => v != this).ToList();
			if (otherFilters.Count == 1)
			{
				otherFilters.First().RemoveFilterButton.Enabled = false;
			}

			parent.Controls.Remove(this);
			Dispose();

			parent.AutoScroll = false;
			parent.AutoScroll = parent.Height >= ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
		}

		void HookValueChangedEvent()
		{
			OrgFilterItemDataSource.OrgFilterTypeDescriptionInfo.ValueChanged += OrgFilterTypeDescriptionValueChanged;
		}

		void UnhookValueChangedEvent()
		{
			OrgFilterItemDataSource.OrgFilterTypeDescriptionInfo.ValueChanged -= OrgFilterTypeDescriptionValueChanged;
		}

		void OrgFilterTypeDescriptionValueChanged(object sender, EventArgs e)
		{
			var isTextBoxSearchBar = OrgFilterItemDataSource.FilterStyle == FilterStyle.Input;

			var isCheckBoxSearchBar = OrgFilterItemDataSource.FilterStyle == FilterStyle.CheckBox;

			ShowFilterItem(isTextBoxSearchBar, isCheckBoxSearchBar);
		}

		void ShowFilterItem(bool isTextBoxSearchBar, bool isCheckBoxSearchBar)
		{
			FilterConditionDropEdit.Visible = isTextBoxSearchBar;
			FilterKeywordTextBox.Visible = isTextBoxSearchBar;
			CheckBoxLayoutPanel.Visible = isCheckBoxSearchBar;
			Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(isCheckBoxSearchBar ? 100 : 38);

			var parent = (KTableLayoutPanel)Parent;
			parent.AutoScroll = false;
			parent.AutoScroll = parent.Height >= ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
		}

		void FilterResultsWithTextBox(List<((FilterStyle, ZString), object)> filterDetails)
		{
			DynamicFilterConditionType? conditionType = FilterConditionTypeDictionary
				.FirstOrDefault(v => v.Value == OrgFilterItemDataSource.OrgFilterOption).Key;
			ZString? keyword = OrgFilterItemDataSource.OrgFilterKeyword;

			filterDetails.Add(((FilterStyle.Input, OrgFilterItemDataSource.OrgFilterType), (conditionType, keyword)));
		}

		void FilterResultsWithCheckBoxes(List<((FilterStyle, ZString), object)> filterDetails)
		{
			var selectedCheckBoxes = new List<ZString>();

			OrgFilterItemDataSource.CheckBoxItems.ForEach(u =>
			{
				if (u.IsSelected)
				{
					selectedCheckBoxes.Add(u.Code);
				}
			});

			filterDetails.Add(((FilterStyle.CheckBox, OrgFilterItemDataSource.OrgFilterType), selectedCheckBoxes));
		}

		internal void AddFilterResultToFilterDetails(List<((FilterStyle, ZString), object)> filterDetails)
		{
			var filterType = OrgFilterItemDataSource.FilterStyle;
			if (filterType == FilterStyle.Input)
			{
				FilterResultsWithTextBox(filterDetails);
			}
			else if (filterType == FilterStyle.CheckBox)
			{
				FilterResultsWithCheckBoxes(filterDetails);
			}
			else
			{
				filterDetails.Add(((FilterStyle.Default, null), null));
			}
		}
	}
}
