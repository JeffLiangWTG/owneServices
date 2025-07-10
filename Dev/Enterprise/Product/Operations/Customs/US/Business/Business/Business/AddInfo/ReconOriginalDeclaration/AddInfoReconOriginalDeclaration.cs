using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoReconOriginalDeclaration : AddInfo
	{
		public AddInfoReconOriginalDeclaration(ZPropertyInfo propertyInfo)
			: base(propertyInfo)
		{
		}

		public ReconOriginalDeclaration Classification
		{
			get { return (ReconOriginalDeclaration)Parent; }
		}

		public new AddInfoReconOriginalDeclarationLookups Lookups
		{
			get { return (AddInfoReconOriginalDeclarationLookups)base.Lookups; }
		}

		public new AddInfoReconOriginalDeclarationValidation Validation
		{
			get { return (AddInfoReconOriginalDeclarationValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoReconOriginalDeclarationLookups(this);
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			return new AddInfoReconOriginalDeclarationValidation(this);
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
