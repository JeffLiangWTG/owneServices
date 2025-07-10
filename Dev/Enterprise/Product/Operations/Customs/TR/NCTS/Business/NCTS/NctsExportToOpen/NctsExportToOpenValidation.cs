using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsExportToOpenValidation : Customs.Business.CusSupportingInfoValidation
	{
		public NctsExportToOpenValidation(NctsExportToOpen parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if(!Parent.CSI_Procedure.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}
	}
}
