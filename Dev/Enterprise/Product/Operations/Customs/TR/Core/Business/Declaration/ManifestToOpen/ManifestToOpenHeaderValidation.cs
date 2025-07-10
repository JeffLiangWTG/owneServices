using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenHeaderValidation : CusEntryNumValidation
	{
		public ManifestToOpenHeaderValidation(ManifestToOpenHeader parent) : base(parent)
		{
		}

		public new ManifestToOpenHeader Parent => (ManifestToOpenHeader)base.Parent;

		protected override void CheckCE_IssueDate()
		{
			base.CheckCE_IssueDate();

			if (Parent.HasBillNumber)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CE_IssueDateInfo);
			}
		}

		protected override void CheckCE_ExpiryDate()
		{
			base.CheckCE_ExpiryDate();

			if (Parent.HasBillNumber)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CE_ExpiryDateInfo);
			}
		}
	}
}
