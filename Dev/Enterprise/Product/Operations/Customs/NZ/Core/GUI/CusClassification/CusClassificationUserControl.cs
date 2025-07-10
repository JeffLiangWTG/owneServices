using System.ComponentModel;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	/// <summary>
	/// Summary description for NZClassificationUserControl.
	/// </summary>
	public partial class CusClassificationUserControl : BaseClassificationUserControl
	{
		internal NZCClassFindBox nZCClassificationFindBox;
		private ZArchitecture.GUI.ZTemplateTabControl infoTabControl;
		private ZArchitecture.GUI.ZTabPage permitTabPage;
		private ZArchitecture.ZGrid permitGrid;
		private ZArchitecture.GUI.ZTabPage prohibitTabPage;
		private ZArchitecture.ZGrid prohibitsGrid;
		private ZArchitecture.GUI.ZTabPage otherInfoTabPage;
		private ZArchitecture.ZGrid otherInfoGrid;
		internal NZCClassFindBox cC_PartsOfClassificationNZCClassFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox cC_ConcessionCodeCodeFindBox;
		internal Universal.GUI.TariffFindBox tariffNumFindBox;
		internal Universal.GUI.TariffFindBox partsOfClassificationFindBox;
		internal ZArchitecture.GUI.ZDropEdit concessionDropEdit;
		private IContainer components;

		public CusClassificationUserControl()
		{
			InitializeComponent();
			SetupTariffFindBox();
			SetControlVisibility();
			cC_ConcessionCodeCodeFindBox.ModuleID = ModuleIDs.Customs.NZ.Concession;
		}

		void SetupTariffFindBox()
		{
			tariffNumFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			tariffNumFindBox.GetCountryCode = () => Core.Constants.CountryCodes.NewZealand;
			tariffNumFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			tariffNumFindBox.GetEffectiveDate = () => (CurrentDataItem as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;

			partsOfClassificationFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			partsOfClassificationFindBox.GetCountryCode = () => Core.Constants.CountryCodes.NewZealand;
			partsOfClassificationFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			partsOfClassificationFindBox.GetEffectiveDate = () => (CurrentDataItem as ITariffValidationData)?.DateForDutyRate ?? CargoWise.Types.ZDateTime.Today;
		}

		void SetControlVisibility()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var useRefDB = UniversalTariffHelper.UseRefDatabaseData;
				nZCClassificationFindBox.Visible = !useRefDB;
				cC_PartsOfClassificationNZCClassFindBox.Visible = !useRefDB;
				cC_ConcessionCodeCodeFindBox.Visible = !useRefDB;

				tariffNumFindBox.Visible = useRefDB;
				partsOfClassificationFindBox.Visible = useRefDB;
				concessionDropEdit.Visible = useRefDB;
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}


