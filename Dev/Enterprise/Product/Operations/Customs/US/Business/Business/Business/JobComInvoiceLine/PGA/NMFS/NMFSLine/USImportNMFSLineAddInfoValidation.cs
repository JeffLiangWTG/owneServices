using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USImportNMFSLineAddInfoValidation : USNMFSLineAddInfoValidation
	{
		public USImportNMFSLineAddInfoValidation(USNMFSLineAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_ProgramType()
		{
			base.CheckUS_ProgramType();
			if (IsPgaRequiredValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ProgramTypeInfo, Parent.Lookups.NMFSPrograms);
				if ((IsHMSPGARequiredValidationMode || Is370PGARequiredValidationMode || IsSIMPPGARequiredValidationMode || IsCOAPGARequiredValidationMode) && NMFSLine.HarvestingDetails.Count == 0 && NMFSLine.RequiresFullData)
				{
					Parent.US_ProgramTypeInfo.AddMessageError(ValidationConstants.NMFS.HarvestingDetailIsRequired(Parent.US_ProgramType));
				}
				ValidateUS_AMLRPermitNumber();
				ValidateUS_HMSPermitNumber();
				ValidateUS_PreApprovalIssuedNumber();
				ValidateUS_PreApprovalIssuedQuantity();
				ValidateUS_PreApprovalIssuedQuantityUQ();
				ValidateUS_Commodity();
				ValidateUS_DocumentType();
				ValidateUS_DISDocumentID();
				ValidateUS_DolphinSafeStatus();
				ValidateUS_CaptainStatement();
				ValidateUS_ObserverStatement();
				ValidateUS_IDCPMemberCertification();
			}
		}

		protected override void CheckUS_SourceType()
		{
			base.CheckUS_SourceType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SourceTypeInfo, Parent.Lookups.SourceTypes);

			if ((IsSIMPPGARequiredValidationMode || IsCOAPGARequiredValidationMode) && NMFSLine.RequiresFullData)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SourceTypeInfo);
			}
		}

		protected override void CheckUS_IFTPPermitNumber()
		{
			base.CheckUS_IFTPPermitNumber();
			if (IsHMSPGARequiredValidationMode || Is370PGARequiredValidationMode || IsAMRPGARequiredValidationMode || ((IsSIMPPGARequiredValidationMode || IsCOAPGARequiredValidationMode) && NMFSLine.RequiresFullData))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IFTPPermitNumberInfo);
			}
		}

		protected override void CheckUS_EBCDNumber()
		{
			base.CheckUS_EBCDNumber();
			ValidateUS_DISDocumentID();
		}

		protected override void CheckUS_PreApprovalIssuedNumber()
		{
			base.CheckUS_PreApprovalIssuedNumber();
			if (IsAMRFrozenToothfishRequiredValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PreApprovalIssuedNumberInfo);
			}
		}

		protected override void CheckUS_PreApprovalIssuedQuantity()
		{
			base.CheckUS_PreApprovalIssuedQuantity();
			if (IsAMRFrozenToothfishRequiredValidationMode)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.US_PreApprovalIssuedQuantityInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.US_PreApprovalIssuedQuantityInfo);
			}
		}

		protected override void CheckUS_PreApprovalIssuedQuantityUQ()
		{
			base.CheckUS_PreApprovalIssuedQuantityUQ();
			if (IsAMRFrozenToothfishRequiredValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_PreApprovalIssuedQuantityUQInfo, Parent.Lookups.WeightUQList);
			}
		}

		protected override void CheckUS_Commodity()
		{
			base.CheckUS_Commodity();
			if (IsAMRPGARequiredValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CommodityInfo, Parent.Lookups.FishStateList);
				ValidateUS_PreApprovalIssuedNumber();
				ValidateUS_PreApprovalIssuedQuantity();
				ValidateUS_PreApprovalIssuedQuantityUQ();
				ValidateUS_DocumentType();
				ValidateUS_DISDocumentID();
			}
		}

		protected override void CheckUS_DISDocumentID()
		{
			base.CheckUS_DISDocumentID();
			if (IsPgaRequiredValidation)
			{
				if (Parent.US_DISDocumentID.IsEmpty)
				{
					if (!HasDocumentDetails)
					{
						if (NMFSLine.Is370ProgramType || NMFSLine.IsHMSProgramType)
						{
							if (Parent.US_ReExportNumber.IsEmpty)
							{
								Parent.US_DISDocumentIDInfo.AddWarning(ValidationConstants.NMFS.DocumentIDIsRequired);
							}
						}
						else if (NMFSLine.IsAMRProgramType && !NMFSLine.IsFrozenToothfish)
						{
							Parent.US_DISDocumentIDInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("DIS Document ID"));
						}
					}
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.US_DISDocumentIDInfo, Parent.Lookups.DISDocumentIDList);

					if (HasDocumentDetails)
					{
						Parent.US_DISDocumentIDInfo.AddWarning(ValidationConstants.NMFS.DocumentDetailsWontBeSent);
					}
				}
			}
		}

		protected override void CheckUS_DocumentType()
		{
			base.CheckUS_DocumentType();
			if (IsPgaRequiredValidation)
			{
				if (NMFSLine.US_DocumentType.IsEmpty)
				{
					if (!HasDocumentDetails)
					{
						if (NMFSLine.IsAMRProgramType && !NMFSLine.IsFrozenToothfish)
						{
							Parent.US_DocumentTypeInfo.AddMessageError(ValidationConstants.NMFS.DocumentIsRequiredForAMRFreshToothfish);
						}
						else if (NMFSLine.IsHMSProgramType)
						{
							Parent.US_DocumentTypeInfo.AddWarning(ValidationConstants.NMFS.DocumentIsRequiredForHMSNonShark);
						}
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_DocumentTypeInfo, Parent.Lookups.DocumentTypeList);

					if (HasDocumentDetails)
					{
						Parent.US_DocumentTypeInfo.AddWarning(ValidationConstants.NMFS.DocumentDetailsWontBeSent);
					}
				}
			}
		}

		protected override void CheckUS_DolphinSafeStatus()
		{
			base.CheckUS_DolphinSafeStatus();
			if (Is370PGARequiredValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_DolphinSafeStatusInfo, Parent.Lookups.DolphinSafeStatusList);
			}
		}

		protected override void CheckUS_Confidential()
		{
			base.CheckUS_Confidential();
			if (IsSIMPPGARequiredValidationMode || IsCOAPGARequiredValidationMode)
			{
				if (!Parent.US_Confidential)
				{
					Parent.US_ConfidentialInfo.AddMessageError(string.Format(ValidationConstants.NMFS.ConfidentialMustBeTrue, NMFSLine.US_ProgramType));
				}
			}
		}

		protected override void CheckUS_OtherAuthorizationNumber()
		{
			base.CheckUS_OtherAuthorizationNumber();
			if (Parent.US_OtherAuthorizationNumber.IsEmpty && !Parent.US_AuthorizationType.IsEmpty)
			{
				Parent.US_OtherAuthorizationNumberInfo.AddMessageError(ValidationConstants.NMFS.AuthorizationNumberRequired);
			}

			ValidateUS_AuthorizationType();
		}

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();

			if (Parent.US_NetWeight.IsEmpty && IsSIMPPGARequiredValidationMode && !NMFSLine.IsNotSIMPProgramTypeOrIsHCF)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
			}

			ValidateUS_NetWeight();
		}

		protected override void CheckUS_NetWeightUQ()
		{
			base.CheckUS_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NetWeightUQInfo, Parent.Lookups.UnitOfMeasureList);

			if (!Parent.US_NetWeight.IsEmpty && IsSIMPPGARequiredValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightUQInfo);
			}
		}

		protected override void CheckUS_SpeciesCode()
		{
			base.CheckUS_SpeciesCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_SpeciesCodeInfo, Parent.Lookups.SpeciesCodeList);

			if (IsSIMPPGARequiredValidationMode || IsCOAPGARequiredValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SpeciesCodeInfo);
			}
		}

		protected override void CheckUS_AuthorizationType()
		{
			base.CheckUS_AuthorizationType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AuthorizationTypeInfo, Parent.Lookups.AuthorizationTypes);

			if (!Parent.US_OtherAuthorizationNumber.IsEmpty && Parent.US_AuthorizationType.IsEmpty)
			{
				Parent.US_AuthorizationTypeInfo.AddMessageError(ValidationConstants.NMFS.AuthorizationTypeRequired);
			}
			ValidateUS_OtherAuthorizationNumber();
		}

		protected new USNMFSLineAddInfo Parent
		{
			get { return (USNMFSLineAddInfo)base.Parent; }
		}

		protected NMFSLine NMFSLine
		{
			get { return Parent.Parent; }
		}

		protected bool IsPgaRequiredValidation
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null)
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}

				return result;
			}
		}

		protected bool IsAMRFrozenToothfishRequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsAMRProgramType && nmfsLine.IsFrozenToothfish)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		protected bool IsAMRPGARequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsAMRProgramType)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		protected bool IsHMSPGARequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsHMSProgramType)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		protected bool Is370PGARequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.Is370ProgramType)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		protected bool IsSIMPPGARequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsSIMProgramType)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		protected bool IsCOAPGARequiredValidationMode
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsCOAProgramType)
				{
					result = IsPgaRequiredValidation;
				}

				return result;
			}
		}

		bool HasDocumentDetails
		{
			get { return NMFSLine != null && NMFSLine.DocumentDetails.Count > 0; }
		}
	}
}
