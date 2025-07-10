using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusLineTariffDetail : AddInfo
	{
		public AddInfoCusLineTariffDetail(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public CusLineTariffDetail LineTariffDetail => (CusLineTariffDetail)Parent;

		public new AddInfoCusLineTariffDetailLookups Lookups => (AddInfoCusLineTariffDetailLookups)base.Lookups;

		public new AddInfoCusLineTariffDetailValidation Validation => (AddInfoCusLineTariffDetailValidation)base.Validation;

		protected override USAddInfoLookups GetNewLookups() => new AddInfoCusLineTariffDetailLookups(this);

		protected override USAddInfoValidation GetNewValidation() => new AddInfoCusLineTariffDetailValidation(this);

		protected override ZString GetTransportMode() => Core.Constants.TransportModes.Unknown;

		protected override bool IsExportCore => false;
	}
}
