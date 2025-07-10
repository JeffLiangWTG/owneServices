using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusClassification : AddInfo
	{
		public AddInfoCusClassification(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public CusClassification Classification
		{
			get { return (CusClassification)Parent; }
		}

		public new AddInfoCusClassificationLookups Lookups
		{
			get { return (AddInfoCusClassificationLookups)base.Lookups; }
		}

		public new AddInfoCusClassificationValidation Validation
		{
			get { return (AddInfoCusClassificationValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoCusClassificationLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			return new AddInfoCusClassificationValidation(this);
		}

		protected override ZString GetTransportMode()
		{
			return Core.Constants.TransportModes.Unknown;
		}

		protected override bool IsExportCore
		{
			get { return false; }
		}
	}
}
