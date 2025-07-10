using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateDeclarationNumberDisplay();
			}
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_NatureCore()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
		}

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_TransportModeInfo);
		}

		protected override ZBool NeedsToCheckAMA_ManifestType => false;

		protected override void CheckAMA_ManifestType()
		{
			base.CheckAMA_ManifestType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_ManifestTypeInfo, Parent.Lookups.ManifestTypes);
		}

		protected override void MandatoryCheckOfCustomsOffice()
		{
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_CustomsOfficeInfo);
		}

		protected override void CheckAMA_PaymentMethod()
		{
			base.CheckAMA_PaymentMethod();
			var parent = Parent;
			if (parent.IsImport)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.AMA_PaymentMethodInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(parent.AMA_PaymentMethodInfo);
			}
		}

		protected override void CheckAMA_VehicleRegistration()
		{
			base.CheckAMA_VehicleRegistration();
			var parent = Parent;
			if (parent.IsSea)
			{
				var vesselRegistrationNumber = parent.AMA_VehicleRegistration;
				if (!vesselRegistrationNumber.IsEmpty && vesselRegistrationNumber.Length != AsycudaManifestHeader.Schema.AMA_VehicleRegistrationMaxLength)
				{
					parent.AMA_VehicleRegistrationInfo.AddMessageError(Res.GetString("EB4556C9-4061-479F-B3CA-ED41E31EF593", "Vessel Registration Number is exactly 6 characters long."));
				}
			}
		}

		protected override void CheckAMA_ManifestNumber()
		{
			base.CheckAMA_ManifestNumber();
			var parent = Parent;
			if (parent.IsSea && parent.IsImport)
			{
				var manifestNumber = parent.AMA_ManifestNumber;
				if (!manifestNumber.IsEmpty && manifestNumber.Length != AsycudaManifestHeader.Schema.AMA_ManifestNumberMaxLength)
				{
					parent.AMA_ManifestNumberInfo.AddMessageError(Res.GetString("2E075BBE-90CC-4DC1-9113-658692AA882C", "Manifest Number is exactly 4 characters long."));
				}
			}
		}

		protected override void CheckAMA_RecipientReference()
		{
			base.CheckAMA_RecipientReference();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_RecipientReferenceInfo);
		}

		protected override void CheckAMA_GS_NKCustomsAgent()
		{
			base.CheckAMA_GS_NKCustomsAgent();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_GS_NKCustomsAgentInfo);

			if (Parent.CustomsAgent is GlbStaff staff)
			{
				if (Parent.CustomsAgentDescription.IsEmpty)
				{
					Parent.AMA_GS_NKCustomsAgentInfo.AddMessageError(Res.GetString("48AE8285-B0F5-46AC-A6EC-B39A404EBC03", "The selected Customs Agent does not have a Customs Clearance Agent of Special Examination Certificate Number. Go to Staff > Human Resources > Certificates, ID and Training to create a TW-BRK certificate."));
				}

				if (Parent.TWBrkCertificate is GenRegCertAccredMaintList certificate)
				{
					var expiryDate = certificate.XZ_ExpiryOrDueDate;
					if (!expiryDate.IsEmpty)
					{
						if (expiryDate < ZDateTime.Today)
						{
							Parent.AMA_GS_NKCustomsAgentInfo.AddMessageError(Res.GetString("D0EC2217-E146-4F6B-BB4A-5EDD27629025", "The Customs Clearance Agent of Special Examination Certificate Number of the selected Customs Agent has expired."));
						}
						else if (expiryDate.AddMonths(-1) < ZDateTime.Today)
						{
							Parent.AMA_GS_NKCustomsAgentInfo.AddWarning(Res.GetString("1503883D-8443-4AEE-AF53-D81E4EA37A99", "The Customs Clearance Agent of Special Examination Certificate Number of the selected Customs Agent will expire on {0}.", expiryDate.ToString("MM dd, yyyy")));
						}
					}
				}

				if (!TWGlbStaffWrapper.Get(staff).TWPasswordCollection.Any())
				{
					Parent.AMA_GS_NKCustomsAgentInfo.AddMessageError(Res.GetString("AB3119A0-DB63-4C41-9F7E-BE47EDDF3AA4", "The entered Customs Agent does not have a valid mailbox. Go to Staff > Credentials to create a mailbox."));
				}
			}
		}

		protected override void CheckAMA_CustomsProfile()
		{
			base.CheckAMA_CustomsProfile();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_CustomsProfileInfo);
		}

		protected override void CheckAMA_OA_Carrier()
		{
			base.CheckAMA_OA_Carrier();
			if (!Parent.AMA_OA_Carrier.IsEmpty)
			{
				var hasTWVatCode = Parent.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan && x.OK_CodeType == OrgCusCode.CodeTypes.VATCode) ?? false;
				if (!hasTWVatCode)
				{
					Parent.AMA_OA_CarrierInfo.AddMessageError(Res.GetString("6D0CBF00-A186-408A-B9A6-615A72F255D0", "The entered Carrier does not have a Taiwan VAT code. Go to Organization > Details > Config > Registration Numbers / Codes to create a TW-VAT code."));
				}
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}

		#region DeclarationNumberDisplay
		public void ValidateDeclarationNumberDisplay()
		{
			ValidateCalculatedProperty(Parent.DeclarationNumberDisplayInfo);
		}

		protected void CheckDeclarationNumberDisplay()
		{
			var targetInfo = Parent.DeclarationNumberDisplayInfo;

			if (!CommonHelper.IsMatchEntryNumber(Parent, Parent.DeclarationNumber))
			{
				targetInfo.AddMessageError(ValidationConstants.AllocateNumber.KeyComponentValuesChanged);
			}
		}
		#endregion
	}
}
