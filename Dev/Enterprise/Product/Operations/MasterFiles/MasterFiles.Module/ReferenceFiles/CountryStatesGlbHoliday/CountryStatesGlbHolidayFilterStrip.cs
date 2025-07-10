using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class CountryStatesGlbHolidayFilterStrip : ZFilterStrip
	{
		public CountryStatesGlbHolidayFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is CountryStatesGlbHolidayCountryStateModuleFilter)
			{
				result = GetCountryStatesGlbHolidayCountryStateModuleFilterControls();
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		Control[] GetCountryStatesGlbHolidayCountryStateModuleFilterControls()
		{
			Control[] result;
			ZDropEdit searchConditionDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
			ZCodeFindBox countryFindBox = new ZCodeFindBox();
			Func<ZCodeFindBox, IBusinessObjectCollection> getCountryStatesCollectionForPopupDelegate = GetCountryStatesCollectionForPopup;
			ZGuidFindBoxWithSelectedEvent stateFindBox = new ZGuidFindBoxWithSelectedEvent(getCountryStatesCollectionForPopupDelegate, countryFindBox);
			countryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountryStates;
			stateFindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref stateFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref stateFindBox, FilterControlsBox2Start(stateFindBox), true);
			stateFindBox.BindTo = "Property2";
			stateFindBox.BindToList = "States";
			stateFindBox.TabIndex = 5;
			stateFindBox.CaptionResourceString = Res.GetData("AD6C94EB-A5E8-44EE-87C1-E29874CAE3DE", "State");
			FilterControlBindingSource.SetBindingMember(stateFindBox, stateFindBox.BindTo);
			int width = CalculateNewWidthForCodeFindBox(MeasureLabel(Res.GetData("AD6C94EB-A5E8-44EE-87C1-E29874CAE3DE", "State").Caption));
			ControlDpiScalingHelper.SetWidth(ref countryFindBox, width, false);
			countryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			countryFindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref countryFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref countryFindBox, stateFindBox.Left - MeasureLabel(Res.GetData("AD6C94EB-A5E8-44EE-87C1-E29874CAE3DE", "State").Caption) - countryFindBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2 * SpaceBetweenLabelAndControl), false);
			countryFindBox.BindTo = "Property1";
			countryFindBox.BindToList = "Countries";
			countryFindBox.TabIndex = 3;
			countryFindBox.CaptionResourceString = Res.GetData("4E8E388F-9A92-496C-B190-B4584FFBB199", "Country");
			FilterControlBindingSource.SetBindingMember(countryFindBox, countryFindBox.BindTo);
			stateFindBox.Selected += delegate(object sender, EmbeddedModulePopup.SelectedEventArgs e)
			{
				RefCountryStates state = (RefCountryStates)e.SelectedBusinessObjects[0];
				if (state != null)
				{
					countryFindBox.CodeBox.Text = state.Country.Code;
					countryFindBox.CodeBox.CommitBoundValue();

					stateFindBox.CodeBox.Text = state.RW_Code;
					stateFindBox.CodeBox.CommitBoundValue();
				}
			};
			result = new Control[] { searchConditionDropEdit, countryFindBox, stateFindBox };
			return result;
		}

#if DEBUG
		internal
#endif
		RefCountryStatesCollection GetCountryStatesCollectionForPopup(ZCodeFindBox countryFindBox)
		{
			var factory = new BusinessObjectFactory();
			RefCountry country = RefCountry.LoadFromCountryCode(factory, countryFindBox.CodeBox.Text);
			RefCountryStatesCollection collection = new RefCountryStatesCollection(factory);

			if (country != null)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", country.Code));
			}

			return collection;
		}
	}
}

