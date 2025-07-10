using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusClassPartPivot : AddInfo
	{
		public AddInfoCusClassPartPivot(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

		public new AddInfoCusClassPartPivotLookups Lookups
		{
			get { return (AddInfoCusClassPartPivotLookups)base.Lookups; }
		}

		public new AddInfoCusClassPartPivotValidation Validation
		{
			get { return (AddInfoCusClassPartPivotValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusClassPartPivotLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusClassPartPivotValidation(this);
		}

		protected override bool IsExportCore => Parent?.IsExportTariff ?? false;

		protected override ZString GetTransportMode() => Core.Constants.TransportModes.Unknown;
	}
}
