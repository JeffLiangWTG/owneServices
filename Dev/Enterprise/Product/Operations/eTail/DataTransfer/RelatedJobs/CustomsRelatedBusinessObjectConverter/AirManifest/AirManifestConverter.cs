using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class AirManifestConverter : CustomsRelatedBusinessObjectConverter
	{
		public AirManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.HCA;

		public override DataContextType MasterBillDataContextType => DataContextType.AirManifest;
	}
}
