using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	[SuppressFormDesignerAnalysis]
	[ToolboxItem(false)]
	[DefaultDataSourceBindingMember(null)]
	public class CityTownMultiCombinationControl : ZMultiCombinationControl
	{
		public CityTownMultiCombinationControl()
			: base()
		{
		}

		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			IGridControl result = null;
			if (typeToCreate == FieldType.Guid)
			{
				result = new CityTownFindBox()
				{
					ModuleID = ModuleIDs.RefCityTown,
				};
			}
			else
			{
				result = base.CreateGridControl(typeToCreate);
			}

			return result;
		}
	}

	internal class CityTownFindBox : ZGridGuidFindBox
	{
		internal CityTownFindBox()
		{
			PopupSelected += CityTownFindBox_PopupSelected;
			IsGuidCacheEnabled = false;
		}

		CityTownColumnStyle CurrentColumnStyle
		{
			get
			{
				var grid = Parent?.Parent as ZGrid;
				return grid?.GetCurrentColumnStyle() as CityTownColumnStyle;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			base.SelectFromPopupForm(autoSelect);

			var popupForm = PopupForm as ZForm;
			while (popupForm.Visible)
			{
				Application.DoEvents();
			}

			CurrentColumnStyle?.ForceEndEditing();
		}

		SelectionNeededEventArgs selectionArgs;

		public void SelectPK(SelectionNeededEventArgs e)
		{
			selectionArgs = e;
			SelectFromPopupForm(true);
		}

		void CityTownFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects != null && e.SelectedBusinessObjects.Length > 0)
			{
				ZoneItem.TQ_R9_CityTown = e.SelectedBusinessObjects[0].PK;
				if (selectionArgs != null)
				{
					selectionArgs.SelectedPK = e.SelectedBusinessObjects[0].PK;
					selectionArgs = null;
				}
			}
		}

		RateTransportZoneItem ZoneItem => CurrentItem as RateTransportZoneItem;
	}
}

