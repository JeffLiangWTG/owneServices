using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoCOValidation : AddInfoJobDeclarationValidation
	{
		public AddInfoCOValidation(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_ApplicationProductType()
		{
			base.CheckSG_ApplicationProductType();
			if (Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKT)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_ApplicationProductTypeInfo, "Application Product Type");
			}
		}

		protected override void CheckSG_Cert1Type()
		{
			base.CheckSG_Cert1Type();

			var info = Parent.SG_Cert1TypeInfo;
			MandatoryValidation.MessageErrorIfNotEntered(info, "Certificate Type");
			ValidateCertType(info);
		}

		protected override void CheckSG_Cert2Type()
		{
			base.CheckSG_Cert2Type();
			if (!Parent.SG_Cert2Type.IsEmpty)
			{
				var info = Parent.SG_Cert2TypeInfo;
				if (Parent.SG_Cert2Type == Parent.SG_Cert1Type)
				{
					info.AddMessageError("Cert. Type 2 should be different to Cert. Type 1");
				}

				ValidateCertType(info);
			}
		}

		void ValidateCertType(ZPropertyInfo info)
		{
			var certType = info.Value?.ToString() ?? ZString.Empty;
			var invalidCertificateTypeCodeList = Parent.Factory.GetCachedValue<InvalidCertificateTypeCodeList>();
			if (!string.IsNullOrWhiteSpace(certType) && invalidCertificateTypeCodeList.ContainsCode(certType))
			{
				info.AddMessageError(Res.GetString("3DE5E87E-2876-48A3-8C4D-D423EF87D61D", "{0} - ({1}) is no longer allowed by SG Customs.", certType, invalidCertificateTypeCodeList.GetDescriptionFromCode(certType)));
			}
		}

		protected override void CheckSG_RN_NKFinalDestination()
		{
			base.CheckSG_RN_NKFinalDestination();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RN_NKFinalDestinationInfo, "Country/Region of Final Destination");
		}

		protected override void CheckSG_OutwardTransportMode()
		{
			base.CheckSG_OutwardTransportMode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardTransportModeInfo, "Outward Transport Mode");
		}

		protected override void CheckSG_OutwardVoyageFlightNo()
		{
			base.CheckSG_OutwardVoyageFlightNo();
			if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA || Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_4_Air)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVoyageFlightNoInfo, "Outward Voyage/Flight No.");
			}
		}

		protected override void CheckSG_OutwardVesselName()
		{
			base.CheckSG_OutwardVesselName();
			if (Parent.SG_OutwardTransportMode == TransportModeCodeList.Codes.TransportMode_1_SEA)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_OutwardVesselNameInfo, "Outward Vessel");
			}
		}

		protected override void CheckSG_CertSendInvDetails()
		{
			base.CheckSG_CertSendInvDetails();
			if (Parent.SG_CertSendInvDetails && !SGCertificatesCodeList.IsInvoiceDetailsAllowed(Declaration.Certificate1Type))
			{
				Parent.SG_CertSendInvDetailsInfo.AddWarning("Are you certain you need to send Invoice Details with this Certificate of Origin?" + System.Environment.NewLine + System.Environment.NewLine + "Invoice Details are optional for Certificate Types 1, 2, 3, 12, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 , 32 , 33 & 34." + System.Environment.NewLine + "Invoice Details are not applicable for other Certificate Types");
			}
			else if (!Parent.SG_CertSendInvDetails && SGCertificatesCodeList.IsInvoiceDetailsAllowed(Declaration.Certificate1Type))
			{
				Parent.SG_CertSendInvDetailsInfo.AddWarning("Invoice Details can be sent with this Certificate of Origin" + System.Environment.NewLine + System.Environment.NewLine + "Invoice Details are optional for these Certificate Types: 1, 2, 3, 12, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 , 32 , 33 & 34." + System.Environment.NewLine + "Invoice Details are not applicable for other Certificate Types");
			}
		}
	}
}
