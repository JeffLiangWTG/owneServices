using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class SGAccessManifestConverter : AsycudaManifestConverter
	{
		public SGAccessManifestConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override RecipientRoleType? RecipientRoleType => UniversalDataBuss.Integration.RecipientRoleType.ASY;
	}
}
