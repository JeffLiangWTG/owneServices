using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultEPaymentReferenceValidation
	{
		public DefaultEPaymentReferenceValidation(DefaultEPaymentReference parent)
		{
			Parent = parent;
		}
		readonly DefaultEPaymentReference Parent;

		public void ValidateAll()
		{
			ValidateProviderCode();
			ValidateReferenceType();
			CheckIsPaymentReferenceTypeDuplicated();
		}

		public void ValidateProviderCode()
		{
			Parent.ProviderCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.ProviderCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ProviderCodeInfo);
		}

		public void ValidateReferenceType()
		{
			Parent.ReferenceTypeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(Parent.ReferenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ReferenceTypeInfo);
		}

		void CheckIsPaymentReferenceTypeDuplicated()
		{
			Parent.RemoveRowError(DuplicateDefaultPaymentReferenceError);

			if (Parent.ProviderCodeInfo.HasErrors() || Parent.ReferenceTypeInfo.HasErrors())
			{
				return;
			}

			var parentCollection = ((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(x => x is DefaultEPaymentReferenceCollection).Cast<DefaultEPaymentReference>();
			if (parentCollection.Except(new[] { Parent }).Any(x => x.ProviderCode == Parent.ProviderCode))
			{
				Parent.AddRowError(DuplicateDefaultPaymentReferenceError);
			}
		}

		string DuplicateDefaultPaymentReferenceError => Res.GetString("19F14ACA-AA52-46B8-B6A3-F216671E3C67", "One Default Payment Reference is allowed for a Provider.");
	}
}
