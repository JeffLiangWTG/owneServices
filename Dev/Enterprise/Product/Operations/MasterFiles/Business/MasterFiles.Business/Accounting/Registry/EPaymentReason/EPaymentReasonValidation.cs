using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EPaymentReasonValidation
	{
		public EPaymentReasonValidation(EPaymentReason parent)
		{
			Parent = parent;
		}
		readonly EPaymentReason Parent;

		public void ValidateAll()
		{
			ValidateProviderCode();
			ValidateReasonCode();
			ValidateReasonDescription();
		}

		public void ValidateProviderCode()
		{
			Parent.ProviderCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ProviderCodeInfo);
		}

		public void ValidateReasonDescription()
		{
			Parent.ReasonDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.ReasonDescriptionInfo);
		}

		public void ValidateReasonCode()
		{
			Parent.ReasonCodeInfo.ClearAllNotifications();
			Parent.RemoveRowError(DuplicateReasonCodeError);
			MandatoryValidation.CheckEntered(Parent.ReasonCodeInfo);
			if (!Parent.ReasonCodeInfo.HasErrors())
			{
				var parentCollection = (EPaymentReasonCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(x => x is EPaymentReasonCollection);
				if (parentCollection != null && parentCollection.Cast<EPaymentReason>().Any(x => x.PK != Parent.PK && x.ProviderCode == Parent.ProviderCode && x.ReasonCode == Parent.ReasonCode))
				{
					Parent.AddRowError(DuplicateReasonCodeError);
				}
			}
		}

		static string DuplicateReasonCodeError => Res.GetString("1162682f-32f4-4147-990d-aeabc1b90fbb", "At least one more payment reason already exist with the same Code.");
	}
}
