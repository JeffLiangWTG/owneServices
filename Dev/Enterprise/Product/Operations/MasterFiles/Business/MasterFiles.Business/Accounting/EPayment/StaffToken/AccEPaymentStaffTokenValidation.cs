
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using static Enterprise.MasterFiles.Business.AccEPaymentStaffTokenLookups;

namespace Enterprise.MasterFiles.Business
{
	public class AccEPaymentStaffTokenValidation : AutoAccEPaymentStaffTokenValidation
	{
		public AccEPaymentStaffTokenValidation(AutoAccEPaymentStaffToken parent) : base(parent)
		{
			Parent = parent as AccEPaymentStaffToken;
		}

		protected new AccEPaymentStaffToken Parent;

		protected override void CheckTK_AB()
		{
			base.CheckTK_AB();
			if (!Parent.TK_ABInfo.HasErrors() && Parent.BankAccount == null)
			{
				Parent.TK_ABInfo.AddError(ResString.GetMultilingualString("B0262271-4C48-44C0-A715-D6AB8A1B3366", "Token must be used for a valid bank account."));
			}
		}

		protected override void CheckTK_GC()
		{
			base.CheckTK_GC();
			if (!Parent.TK_GCInfo.HasErrors() && Parent.Company == null)
			{
				Parent.TK_GCInfo.AddError(ResString.GetMultilingualString("E20298E1-AAF5-42BC-B2C4-18A0C370D255", "Token must specify a valid Company."));
			}
		}

		protected override void CheckTK_Status()
		{
			base.CheckTK_Status();
			MandatoryValidation.CheckEntered(Parent.TK_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TK_StatusInfo);
		}

		protected override void CheckTK_ExpiryUtc()
		{
			base.CheckTK_ExpiryUtc();
			if (!Parent.TK_ExpiryUtcInfo.HasErrors())
			{
				if (Parent.IsAuthorised)
				{
					if (Parent.TK_ExpiryUtc.IsEmpty)
					{
						Parent.TK_ExpiryUtcInfo.AddError(ResString.GetMultilingualString("4401571B-CAD3-43E8-B8FF-93E88EF9E265", "Expiry date/Time must be recorded if the token is in authorized state."));
					}
					else if (Parent.TK_ExpiryUtc < Parent.TK_RequestedUtc)
					{
						Parent.TK_ExpiryUtcInfo.AddError(ResString.GetMultilingualString("B03877F5-4DA0-4596-897F-1D6A74120FF1", "Expiry date/Time cannot be earlier than the token requested date/time."));
					}
				}
				else if (!Parent.TK_ExpiryUtc.IsEmpty)
				{
					Parent.TK_ExpiryUtcInfo.AddError(ResString.GetMultilingualString("A0D997FC-50F3-4A1A-BF59-5335D98A6509", "Expiry date/Time can only be set if the token is in authorized state."));
				}
			}
		}

		protected override void CheckTK_ErrorDescription()
		{
			base.CheckTK_ErrorDescription();
			if (!Parent.TK_ErrorDescriptionInfo.HasErrors() && !Parent.TK_ErrorDescription.IsEmpty && Parent.TK_Status != StatusCodes.Error)
			{
				Parent.TK_ErrorDescriptionInfo.AddError(ResString.GetMultilingualString("5DFBDD6A-F1E6-46B5-85B0-BB865B557039", "Error Description should only be recorded if the status is ERR."));
			}
		}

		protected override void CheckTK_GS_NKStaffCode()
		{
			base.CheckTK_GS_NKStaffCode();
			MandatoryValidation.CheckEntered(Parent.TK_GS_NKStaffCodeInfo);
			if (!Parent.TK_GS_NKStaffCodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.TK_GS_NKStaffCodeInfo);
			}
			if (!Parent.TK_GS_NKStaffCodeInfo.HasErrors())
			{
				var bankAccount = Parent.BankAccount;
				if (bankAccount != null)
				{
					var staffTokens = bankAccount.EPaymentStaffTokenCollection.OfType<AccEPaymentStaffToken>();
					if (staffTokens.Where(x => x.PK != Parent.PK).Any(x => x.TK_GS_NKStaffCode == Parent.TK_GS_NKStaffCode))
					{
						Parent.TK_GS_NKStaffCodeInfo.AddError(ResString.GetMultilingualString("DE251D92-D38B-4908-A46B-D09B2E855389", "Authorization record with the same CW1 staff code already exists."));
					}
				}
			}
		}
	}
}
