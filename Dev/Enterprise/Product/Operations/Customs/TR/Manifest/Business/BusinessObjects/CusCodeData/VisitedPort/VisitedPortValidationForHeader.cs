using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedPortValidationForHeader : VisitedPortValidation
	{
		public VisitedPortValidationForHeader(VisitedPort parent) : base(parent)
		{
		}

		public AsycudaManifestHeader Header => (AsycudaManifestHeader)Parent.Parent;

		protected override void CheckCY_Date()
		{
			base.CheckCY_Date();

			var header = Header;
			if (header != null)
			{
				var isMandatory = !((header.IsAir || header.IsSea) && header.AMA_ManifestType == TRManifestTypes.Codes.CIKONC);

				if (isMandatory)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DateInfo);
				}
			}
		}
	}
}
