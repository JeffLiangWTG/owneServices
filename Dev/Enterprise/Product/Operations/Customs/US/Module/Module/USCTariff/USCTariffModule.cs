using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module for USCTariff.
	/// </summary>
	public class USCTariffModule : USCFilterGridModule
	{
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Tariff;

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			Globals.Message.Show("There is no information to show for the selected record");
			return null;
		}

		protected override IFilterControl GetNewFilterControl() => new USCTariffFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var tariffCollection = new USCTariffCollection(Factory);
			tariffCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(USCTariff.FilterSchema.Date, USCTariff.FilterSchema.DatePropertyNameToDefault, new ZString(ModuleDateFilter.DateRangeSearchTexts.Today)));
			return tariffCollection;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCTariffFilterStripBusinessObject();

		protected override bool ShouldLoadFilterBusinessObjectDefaults => true;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;
	}
}
