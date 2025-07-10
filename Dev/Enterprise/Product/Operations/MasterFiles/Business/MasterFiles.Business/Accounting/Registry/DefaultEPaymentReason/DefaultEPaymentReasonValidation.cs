using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultEPaymentReasonValidation
	{
		public DefaultEPaymentReasonValidation(DefaultEPaymentReason parent)
		{
			Parent = parent;
		}
		readonly DefaultEPaymentReason Parent;

		public void ValidateAll()
		{
			ValidateProviderCode();
			ValidateReasonCode();
		}

		public void ValidateProviderCode()
		{
			Parent.ProviderCodeInfo.ClearAllNotifications();
			Parent.RemoveRowError(DuplicateDefaultPaymentReasonError);
			MandatoryValidation.CheckEntered(Parent.ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ProviderCodeInfo);
			if (!Parent.ProviderCodeInfo.HasErrors())
			{
				var parentCollection = (DefaultEPaymentReasonCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(x => x is DefaultEPaymentReasonCollection);
				if (parentCollection != null && parentCollection.Cast<DefaultEPaymentReason>().Any(x => x.PK != Parent.PK && x.ProviderCode == Parent.ProviderCode))
				{
					Parent.AddRowError(DuplicateDefaultPaymentReasonError);
				}
			}
		}

		public void ValidateReasonCode()
		{
			Parent.ReasonCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.ReasonCodeInfo);
		}

		static string DuplicateDefaultPaymentReasonError => Res.GetString("8322f91b-5058-4d01-b0f1-cfe363db4963", "Only one default payment reason is allowed for a Provider.");
	}
}
