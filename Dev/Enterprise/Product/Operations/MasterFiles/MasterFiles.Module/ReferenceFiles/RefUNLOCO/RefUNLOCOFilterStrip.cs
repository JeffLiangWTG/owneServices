using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.ReferenceFiles.RefUNLOCO;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class RefUNLOCOFilterStrip : ZFilterStrip
	{
		public RefUNLOCOFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is RefUNLOCOCountryStateModuleFilter)
			{
				result = GetRefUNLOCOCountryStateModuleFilterModuleFilterControls();
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		Control[] GetRefUNLOCOCountryStateModuleFilterModuleFilterControls()
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
			stateFindBox.CaptionResourceString = Res.GetData("F0C66154-517F-4A4B-9FC6-63DAEE99C47A", "State");
			FilterControlBindingSource.SetBindingMember(stateFindBox, stateFindBox.BindTo);

			int width = CalculateNewWidthForCodeFindBox(MeasureLabel(Res.GetData("F0C66154-517F-4A4B-9FC6-63DAEE99C47A", "State").Caption));
			ControlDpiScalingHelper.SetWidth(ref countryFindBox, width, false);

			countryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			countryFindBox.ShowDescriptionBox = false;
			ControlDpiScalingHelper.SetTop(ref countryFindBox, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref countryFindBox, stateFindBox.Left - MeasureLabel(Res.GetData("F0C66154-517F-4A4B-9FC6-63DAEE99C47A", "State").Caption) - countryFindBox.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(2 * SpaceBetweenLabelAndControl), false);
			countryFindBox.BindTo = "Property1";
			countryFindBox.BindToList = "Countries";
			countryFindBox.TabIndex = 3;
			countryFindBox.CaptionResourceString = Res.GetData("926E0F3D-1A5C-44D3-A1B0-0616D8BC8A5C", "Country/Region");
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

