using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public class FTZDocsAndCartageValidation : JobDocsAndCartageValidation
	{
		public FTZDocsAndCartageValidation(JobDocsAndCartage parent)
			: base(parent)
		{
		}

		JobDeclaration Declaration
		{
			get
			{
				var declaration = Parent.Parent as JobDeclaration;
				var shipment = Parent.ShipmentInParent;
				if (shipment != null)
				{
					declaration = (shipment as ForwardingShipment)?.GetDeclaration() as JobDeclaration;
				}
				return declaration;
			}
		}

		public void CheckJP_OA_DeliveryCartageCoAddr(ZPropertyInfo info)
		{
			var declaration = Declaration;
			if (declaration != null && declaration.IsFTZAdmission)
			{
				if (declaration.DeliveryOrPickupCartageCoPK.IsEmpty)
				{
					if (IsFTZPTTValidationMode || (IsFTZAdmissionValidationMode && declaration.US_F_IncludePTT))
					{
						info.AddMessageError(string.Format(CultureInfo.InvariantCulture, GetErrorMessage(), "Carrier"));
					}
				}
				else
				{
					if (IsFTZAdmissionValidationMode && !declaration.US_F_IncludePTT)
					{
						info.AddWarning(ValidationConstants.FTZ.CarrierWillNotBeSend);
					}

					OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(info, OrgMatchedCustomsRegNoType.EIN, string.Format(CultureInfo.InvariantCulture, OrganisationValidation.EIN_SSN_CBNCodeRequired, "Carrier"), false, false);
				}
			}
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return FTZJobDeclarationValidationHelper.IsFTZAdmissionValidationMode(Declaration); }
		}

		bool IsFTZPTTValidationMode
		{
			get { return FTZJobDeclarationValidationHelper.IsFTZPTTValidationMode(Declaration); }
		}

		string GetErrorMessage()
		{
			return FTZJobDeclarationValidationHelper.GetErrorMessage(Declaration);
		}
	}
}
