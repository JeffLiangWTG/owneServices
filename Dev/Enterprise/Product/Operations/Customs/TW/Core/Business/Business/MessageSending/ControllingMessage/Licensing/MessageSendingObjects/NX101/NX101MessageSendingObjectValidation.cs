
namespace Enterprise.Customs.TW.Business
{
	public class NX101MessageSendingObjectValidation : LicensingMessageSendingObjectValidation
	{
		public NX101MessageSendingObjectValidation(AutoControllingMessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckAction()
		{
			base.CheckAction();

			var parent = Parent;
			var header = parent.Header;
			var targetInfo = parent.ActionInfo;
			var action = parent.Action;
			if (action == NX101ActionCodeList.Codes._9)
			{
				if (parent.CertificateType == CertificateTypeList.Codes.Code13)
				{
					targetInfo.AddError(Res.GetString("E55CB292-851F-4CB9-8712-EB159E31E9C4", "Code 9 - Create does not support Certificate Type 13."));
				}

				if (!header.TW1_PrePermitNumber.IsEmpty)
				{
					targetInfo.AddWarning(Res.GetString("8ABC6934-E77B-4893-8537-CD61F8C7F6EB", "You have entered Previous Permit Number, if you want to apply for a replacement or lost replacement, please select '18' or '17'."));
				}
			}
			else if (action == NX101ActionCodeList.Codes._17 || action == NX101ActionCodeList.Codes._18)
			{
				if (header.TW1_PrePermitNumber.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("42050F95-77C9-4AED-A6AB-E3EC74849B4E", "You have not entered a Previous Permit No."));
				}

				if (action == NX101ActionCodeList.Codes._18 && !header.TW1_ReturnPreviousCOO && !header.TW1_IsSpecialApplication)
				{
					targetInfo.AddMessageError(Res.GetString("9D7F8D83-1739-49BD-BF6C-41AAB76D3DE4", "When Return Previous COO is false, you must apply for Special Application."));
				}
			}
		}
	}
}
