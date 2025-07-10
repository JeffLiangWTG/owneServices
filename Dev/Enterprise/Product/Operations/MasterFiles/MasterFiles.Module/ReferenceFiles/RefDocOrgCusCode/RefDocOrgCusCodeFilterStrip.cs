using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public class RefDocOrgCusCodeFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is CountryCustomsCodeFilter moduleFilter)
			{
				result = GetCountryCustomsCodeFilterControls(moduleFilter);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		Control[] GetCountryCustomsCodeFilterControls(CountryCustomsCodeFilter moduleFilter)
		{
			var caption1 = moduleFilter.ItemDescription1 ?? Res.GetData("b062ca8c-0a35-56ab-476d-d41bb6d360ed", "Country/Region");
			var caption2 = moduleFilter.ItemDescription2 ?? Res.GetData("db652480-db24-9a97-464c-a918f3ec797f", "Type");

			var countryFindBox = new ZCodeFindBox();
			var typeDropEdit = new ZDropEdit();

			int width = CalculateNewWidthForCodeFindBox(MeasureLabel(caption2.Caption));
			ControlDpiScalingHelper.SetWidth(ref countryFindBox, width, false);

			countryFindBox.ModuleID = ZArchitecture.Modules.ModuleIDs.RefCountry;
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

			return new Control[] { countryFindBox, typeDropEdit };
		}
	}
}
