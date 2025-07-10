using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class OrganisationFilterStrip : ZFilterStrip
	{
		public OrganisationFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is OrgForwarderModuleFilter)
			{
				result = new Control[] { new OrgForwarderFilterControl() };
			}
			else if (currentModuleFilter is OrgCodeMappingForeignModuleFilter)
			{
				var control = new OrgCodeMappingForeignFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgCodeMappingLocalModuleFilter)
			{
				var control = new OrgCodeMappingLocalFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgReceivablesModuleFilter)
			{
				OrgReceivablesFilterControl orgReceivablesFilterControl = new OrgReceivablesFilterControl();
				ControlDpiScalingHelper.SetHeight(ref orgReceivablesFilterControl, orgReceivablesFilterControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ZDropEdit comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				orgReceivablesFilterControl.Controls.Add(comparisonOperatorDropEdit);
				result = new Control[] { orgReceivablesFilterControl };
			}
			else if (currentModuleFilter is OrgTransactionsModuleFilter)
			{
				OrgTransactionsFilterControl orgTransactionsFilterControl = new OrgTransactionsFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref orgTransactionsFilterControl, orgTransactionsFilterControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				FilterControlBindingSource.SetBindingMember(orgTransactionsFilterControl, ".");
				result = new Control[] { orgTransactionsFilterControl };
			}
			else if (currentModuleFilter is OrgRelatedPartiesModuleFilter)
			{
				OrgRelatedPartiesFilterControl control = new OrgRelatedPartiesFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgRegistrationCountryAndTypeModuleFilter)
			{
				result = GetOrgRegistrationCountryAndTypeModuleFilterControls();
			}
			else if (currentModuleFilter is OrgTypeModuleFilter)
			{
				OrgTypeFilterControl orgTypeFilterControl = new OrgTypeFilterControl();
				PreferredHeight = orgTypeFilterControl.Height + ControlDpiScalingHelper.OnePixel;

				result = new Control[] { orgTypeFilterControl };
			}
			else if (currentModuleFilter is OrgSecondaryTypeModuleFilter)
			{
				var filterControls = base.GetCurrentFilterControls(currentModuleFilter);
				var dropEditControl = filterControls.FirstOrDefault(control => control is ZDropEdit) as ZDropEdit;
				if (dropEditControl != null)
				{
					ControlDpiScalingHelper.SetWidth(dropEditControl.CodeBox, dropEditControl.Width - ZDropEdit.ButtonWidth, false);
					dropEditControl.ShowDescriptionBox = false;
					dropEditControl.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
				}
				result = filterControls;
			}
			else if (currentModuleFilter is FeesAndChargesFilter)
			{
				var filterControl = new FeesAndChargesFilterControl();
				PreferredHeight = filterControl.Height + ControlDpiScalingHelper.OnePixel;

				result = new Control[] { filterControl };
			}
			else if (currentModuleFilter is OrgAddressWithActiveStatusModuleTextFilter)
			{
				var control = new OrgAddressActiveStatusAndInfoFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				var codeBox = (control.Controls.Find("ActiveStatusDropEdit", true).SingleOrDefault() as ZDropEdit)?.CodeBox;
				if (codeBox != null)
				{
					ControlDpiScalingHelper.SetWidth(codeBox, DropListCodeBoxWidth, true);
				}
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				control.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgLocatedWithinModuleFilter)
			{
				var control = new OrgLocatedWithinModuleFilterControl();
				PreferredHeight = control.Height + ControlDpiScalingHelper.OnePixel;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgContactsActiveStatusAndInfoModuleFilter)
			{
				var control = new OrgContactsActiveStatusAndInfoFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				var codeBox = (control.Controls.Find("ActiveStatusDropEdit", true).SingleOrDefault() as ZDropEdit)?.CodeBox;
				if (codeBox != null)
				{
					ControlDpiScalingHelper.SetWidth(codeBox, DropListCodeBoxWidth, true);
				}
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				control.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrganisationHasSalesRelationFilter)
			{
				result = GetOrganisationHasSalesRelationFilterControls();
			}
			else if (currentModuleFilter is OrgTaxConfigurationModuleFilter)
			{
				var control = new OrgTaxConfigurationFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgCreditScoresDnBRatingModuleFilter)
			{
				var control = new OrgCreditScoresDnBRatingFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgSalesMainCompetitorModuleFilter)
			{
				var control = new OrgSalesMainCompetitorFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgHasMainCompetitorModuleFilter)
			{
				var control = new OrgHasMainCompetitorFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		Control[] GetOrgRegistrationCountryAndTypeModuleFilterControls()
		{
			Control[] result;
			OrgRegistrationCountryAndTypeModuleFilter moduleFilter = (OrgRegistrationCountryAndTypeModuleFilter)CurrentDataItem.CurrentModuleFilter;
			ResourceStringData caption1 = moduleFilter.ItemDescription1 ?? Res.GetData("926E0F3D-1A5C-44D3-A1B0-0616D8BC8A5C", "Country/Region");
			ResourceStringData caption2 = moduleFilter.ItemDescription2 ?? Res.GetData("A32CFB99-9CB0-420A-9F34-4BC5079B5321", "Type");

			ZCodeFindBox countryFindBox = new ZCodeFindBox();
			ZDropEdit typeDropEdit = new ZDropEdit();

			int width = CalculateNewWidthForCodeFindBox(MeasureLabel(caption2.Caption));
			ControlDpiScalingHelper.SetWidth(ref countryFindBox, width, false);

			countryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			countryFindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref countryFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref countryFindBox, FilterControlsBox1Start, true);
			countryFindBox.BindTo = "Property1";
			countryFindBox.BindToList = "List1";
			countryFindBox.TabIndex = 2;
			countryFindBox.CaptionResourceString = caption1;
			FilterControlBindingSource.SetBindingMember(countryFindBox, countryFindBox.BindTo);

			typeDropEdit.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref typeDropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref typeDropEdit, FilterControlsBox2Start(typeDropEdit), true);
			typeDropEdit.BindTo = "Property2";
			typeDropEdit.BindToList = "List2";
			typeDropEdit.TabIndex = 4;
			typeDropEdit.CaptionResourceString = caption2;
			FilterControlBindingSource.SetBindingMember(typeDropEdit, typeDropEdit.BindTo);

			result = new Control[] { countryFindBox, typeDropEdit };
			return result;
		}

		Control[] GetOrganisationHasSalesRelationFilterControls()
		{
			var checkBox = new ZCheckBox();
			checkBox.CaptionResourceString = Res.GetData("FB1A9ACD-6B4A-4E68-99F9-BE6F9EC6A035", "Yes");
			checkBox.AutoSize = true;
			ControlDpiScalingHelper.SetTop(ref checkBox, LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref checkBox, FilterControlsStart, true);
			checkBox.BindTo = "BoolProperty";
			checkBox.TabIndex = 1;
			FilterControlBindingSource.SetBindingMember(checkBox, checkBox.BindTo);

			var dropEdit = new ZDropEdit();
			dropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref dropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref dropEdit, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref dropEdit, FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(dropEdit.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
			dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			dropEdit.ShowDescriptionBox = true;
			dropEdit.TabIndex = 2;
			dropEdit.BindTo = "TypeProperty";
			FilterControlBindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);

			return new Control[] { dropEdit, checkBox };
		}
	}
}

