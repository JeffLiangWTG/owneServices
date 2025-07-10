using System.Collections.Generic;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public class RadioCallSignCodeFindBox : ZCodeFindBox
	{
		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			var popup = base.CreateEmbeddedPopup(module);

			vesselModule = ((ZFilterGridModule)module);
			MasterFiles.Module.RefVesselZZFilterBusinessObject filterBusinessObject = vesselModule.FilterBusinessObject as MasterFiles.Module.RefVesselZZFilterBusinessObject;

			filterBusinessObject.ApplicableModuleFilters = new List<string>() { RefVesselZZSchema.Constants.ZZO_Code, RefVesselZZSchema.Constants.ZZO_RadioCallSign, RefVesselZZFilterBusinessObject.Descriptions.CarrierCodes, RefVesselZZFilterBusinessObject.Descriptions.CarrierNames };

			foreach (Core.Forms.ZGridColumnInfo col in vesselModule.DisplayGrid.ColumnStyles)
			{
				if (col.ColumnName != RefVesselZZSchema.Constants.ZZO_Code && col.ColumnName != RefVesselZZSchema.Constants.ZZO_RadioCallSign)
				{
					col.IsUnavailable = true;
				}
				if (col.ColumnName == "CarrierCodes" || col.ColumnName == "CarrierNames")
				{
					col.IsUnavailable = false;
				}
			}

			return popup;
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			if (RelatedSystemVessels != null && RelatedSystemVessels.Length < 2)
			{
				base.SelectFromPopupForm();
			}
			else
			{
				var vesselName = (DataSource as Business.JobDeclaration)?.JE_VesselName ?? CargoWise.Types.ZString.Empty;
				this.Description = vesselName;
				ActiveControl = DescriptionBox;
				var shouldShowPopupForm = true;
				var shouldAutoSearchAndFocus = !string.IsNullOrEmpty(vesselName) && AutoRunSearchFromFindBox;

				var embeddedPopup = PopupForm as EmbeddedModulePopup;
				if (embeddedPopup != null && shouldAutoSearchAndFocus)
				{
					SetRunSearchOnEnteringAModule(embeddedPopup);
				}

				if (shouldShowPopupForm)
				{
					PopupForm.ShowModal(this, FindForm());

					if (embeddedPopup != null && shouldAutoSearchAndFocus)
					{
						embeddedPopup.FocusFirstRecord();
					}
				}
			}
		}
		protected override bool CanReferenceByDescription => true;

		bool AutoRunSearchFromFindBox
		{
			get { return ZArchitecture.Environment.EnvProxy.Instance.Registry.AutoRunSearchFromFindBox; }
		}

		public MasterFiles.Business.RefVesselZZ[] RelatedSystemVessels => (DataSource as Business.JobDeclaration)?.Vessel?.RelatedSystemVessels;

		void SetRunSearchOnEnteringAModule(EmbeddedModulePopup embeddedPopup)
		{
			var filterControl = (ZFilterStripCommonControl)vesselModule.EmbeddedControl;
			if (filterControl != null)
			{
				filterControl.RunSearchOnEnteringAModuleOverride = true;
			}
		}
		ZFilterGridModule vesselModule;
	}
}
