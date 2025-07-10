using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationValidation : AutoTWJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override ZBool ShouldCheckDeclarationWithSameDirectMasterBill => !(Declaration.IsSea && Declaration.JE_MasterBill == Constants.NIL);

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			var parent = Parent;
			if (parent.IsSea)
			{
				var targetInfo = parent.JE_VesselNameInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

				var vessel = parent.Vessel;
				if (vessel != null && vessel.RV_RadioCallSign.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.Declaration.CallSignMandatory);
				}
			}
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			var parent = Parent;
			var transportMode = parent.JE_TransportMode;
			var customsOfficeInfo = parent.JE_CustomsOfficeInfo;
			if (!parent.JE_CustomsOffice.IsEmpty)
			{
				var customsOffice = parent.CustomsOffice;
				if (customsOffice == null)
				{
					customsOfficeInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else if ((transportMode == TransportTypeList.Codes.Sea || transportMode == TransportTypeList.Codes.Air) && !customsOffice.MatchTransportMode(transportMode))
				{
					customsOfficeInfo.AddMessageError(ValidationConstants.Declaration.CustomsOfficeNotForTransportMode);
				}
			}
		}

		protected override void CheckJE_LocationOfGoods()
		{
			var declaration = Parent;
			var locationOfGoodsInfo = declaration.JE_LocationOfGoodsInfo;

			if (!declaration.JE_LocationOfGoods.IsEmpty)
			{
				var locationOfGoods = declaration.LocationOfGoods;
				if (locationOfGoods == null)
				{
					locationOfGoodsInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else if (!locationOfGoods.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.CustomsOffice, declaration.JE_CustomsOffice))
				{
					locationOfGoodsInfo.AddWarning(ValidationConstants.Declaration.LocationOfGoodsDoesNotBelongToCustomsOffice(declaration.JE_LocationOfGoods, declaration.JE_CustomsOffice));
				}
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaymentMethodInfo, Parent.Lookups.PaymentMethodsList);
		}

		protected override void CheckJE_DefermentAccountNumber()
		{
			base.CheckJE_DefermentAccountNumber();

			if (IsDefermentAccountNumberRequired())
			{
				Parent.JE_DefermentAccountNumberInfo.AddMessageError(ValidationConstants.Declaration.CassNoOrAccountNoMandatory);
			}
		}

		bool IsDefermentAccountNumberRequired()
		{
			var result = false;
			var parent = Parent;
			if (parent.JE_DefermentAccountNumber.IsEmpty && parent.JE_MessageType == Common.Shared.SharedJobMessageTypeList.Codes.Import)
			{
				switch (parent.JE_PaymentMethod)
				{
					case IMPPaymentMethod.Codes._2:
					case IMPPaymentMethod.Codes._3:
					case IMPPaymentMethod.Codes._4:
					case IMPPaymentMethod.Codes._5:
					case IMPPaymentMethod.Codes._8:
						result = true;
						break;
				}
			}
			return result;
		}

		protected override void CheckJE_GS_NKCusAgent()
		{
			base.CheckJE_GS_NKCusAgent();

			var parent = Parent;
			var cusAgentInfo = parent.JE_GS_NKCusAgentInfo;
			if (!parent.JE_GS_NKCusAgent.IsEmpty)
			{
				var cusAgentCertificateNumber = parent.CusAgentCertificateNumber;
				if (cusAgentCertificateNumber.IsEmpty)
				{
					cusAgentInfo.AddMessageError(ValidationConstants.CusInBondHeader.BrokerStaffNotHaveValidCertificateNumber);
				}
				else if (!new System.Text.RegularExpressions.Regex("^[0-9A-Za-z]{5}$").IsMatch(cusAgentCertificateNumber))
				{
					cusAgentInfo.AddMessageError(Res.GetString("AFA25C65-1501-4E56-A003-C054B8728E34", "The Taiwan broker certificate must be exactly 5 characters long with only numbers and capital English letters."));
				}

				var credential = parent.GetCredential();
				if (credential == null || credential.GP_PasswordStatus != PasswordStatusList.Codes.Valid)
				{
					cusAgentInfo.AddMessageError(Res.GetString("E7187467-E074-45D6-84CF-9D8E818AC5EB", "The selected Broker Staff does not have a valid Credential."));
				}
			}
			else
			{
				cusAgentInfo.AddMessageError(ValidationConstants.Declaration.BrokerStaffShouldNotBeEmpty);
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			var parent = Parent;
			var customsProfile = parent.JE_CustomsProfile;
			var customsProfileInfo = parent.JE_CustomsProfileInfo;
			if (!parent.JE_GS_NKCusAgent.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(customsProfileInfo);
			}

			if (!customsProfile.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(customsProfileInfo);

				var credential = parent.GetCredential();
				if (credential != null)
				{
					var isTesting = credential.IsTesting();
					if (TWCustomsDataRegistry.IsTestMode && !isTesting)
					{
						customsProfileInfo.AddMessageError(ValidationConstants.Declaration.MailboxNotForTesting);
					}
					else if (!TWCustomsDataRegistry.IsTestMode && isTesting)
					{
						customsProfileInfo.AddMessageError(ValidationConstants.Declaration.MailboxNotForProduction);
					}

					var expiryDate = credential.GP_ExpiryDate;
					if (expiryDate.IsValid)
					{
						if (expiryDate.Date < ZDate.Today)
						{
							customsProfileInfo.AddMessageError(ValidationConstants.Declaration.CertificateExpired);
						}
						else if (expiryDate.Date < ZDate.Today.AddDays(31))
						{
							customsProfileInfo.AddWarning(ValidationConstants.Declaration.CertificateWillExpire(expiryDate.Date.ToISO8601ShortDateString()));
						}
					}
				}
			}
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			var parent = Parent;
			var customsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.GetFallBackValueAtAllLevels(parent.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			var interfaceSubmissionType = customsInterface?.SubmissionType ?? ZString.Empty;
			if (!interfaceSubmissionType.IsEmpty)
			{
				if (parent.JE_ApplicationCode.IsEmpty)
				{
					parent.JE_ApplicationCodeInfo.AddError(ValidationConstants.Declaration.ApplicationCodeShouldNotBeEmpty);
				}
				ListValidation.MessageErrorIfInvalidCode(parent.JE_ApplicationCodeInfo);
			}
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();

			var parent = Parent;
			if (parent.IsAir)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.JE_ContainerModeInfo, parent.Lookups.CargoIdTypeList);
			}
			var entryInstruction = parent.CusEntryInstruction;
			var declarationType = entryInstruction.CEI_Style;
			if ((declarationType == Constants.DeclarationTypes.Import.F3 || declarationType == Constants.DeclarationTypes.Export.F4)
				&& parent.JE_ContainerMode != ContainerModeList.Codes.Other)
			{
				parent.JE_ContainerModeInfo.AddMessageError(ValidationConstants.Declaration.ContainerModeMustbeOther(declarationType));
			}
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKPortOfLoadingInfo, Parent.Lookups.PortOfLoadings, MessageErrorPortCodeInvalid);
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			CheckZ99PortValidation(Parent.IsZ99FinalDestination, Parent.JE_RL_NKFinalDestinationInfo);
		}
		protected override void CheckJE_RL_NKOrigin()
		{
			var parent = Parent;
			var originInfo = parent.JE_RL_NKOriginInfo;

			CheckZ99PortValidation(parent.IsZ99PortOfOrigin, originInfo);

			if (parent.JE_RL_NKOrigin.IsEmpty)
			{
				var codes = new HashSet<string> { CertificateTypeList.Codes.Code1, CertificateTypeList.Codes.Code7, CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code10, CertificateTypeList.Codes.Code15, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 };
				foreach (var invoiceLine in parent.FilteredInvoiceLines)
				{
					if (CheckInvoiceLineLinkControllingMsgHeadersCode(invoiceLine, codes))
					{
						originInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(originInfo.HumanReadableName));
						break;
					}
				}
			}
		}

		protected override void CheckJE_MergeBy()
		{
			base.CheckJE_MergeBy();

			var mergeByInfo = Parent.JE_MergeByInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(mergeByInfo);
			var entryInstruction = Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
			if (entryInstruction != null && Parent.JE_MergeBy == MergeByCodeList.Codes.CondensedDeclaration)
			{
				var declarationType = entryInstruction.CEI_Style;
				if (entryInstruction.CEI_ExamMode != ExamModeList.Codes.WrittenReview)
				{
					mergeByInfo.AddMessageError(Res.GetString("87D75151-64A1-40C0-BD11-265847608D1B", "A declaration can only be condensed when its Examination Mode is 8"));
				}

				if (!declarationType.IsEmpty && !CondensedDeclarationTypes.Contains(declarationType))
				{
					mergeByInfo.AddMessageError(Res.GetString("77A0AEBD-E41D-43CC-AAEC-BE8ABC927682", "A declaration with a bonded declaration type cannot be condensed"));
				}

				if (entryInstruction.HasValidControllingMsgHeader)
				{
					mergeByInfo.AddMessageError(Res.GetString("1BADA3D1-6A07-4F93-89D2-CBC5CCA22765", "A declaration with controlling reporting requirements cannot be condensed"));
				}

				if (entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.IsCarRelatedDataEmpty))
				{
					mergeByInfo.AddMessageError(Res.GetString("D50DADD8-4654-47A1-B171-0D3CD9A0B470", "A declaration with car information reporting requirements cannot be condensed"));
				}

				if (entryInstruction.InvoiceLines.Length < 50)
				{
					mergeByInfo.AddMessageError(Res.GetString("75F69330-7E1E-4CBB-865B-DDD06A03B4CC", "A condensed declaration must have more than 50 invoice lines"));
				}

				if (entryInstruction.CEI_DutyRefund)
				{
					mergeByInfo.AddMessageError(Res.GetString("733F2615-E333-4FB2-9888-7780ECD32613", "A condensed declaration cannot apply for Duty Refund"));
				}
			}
		}

		List<ZString> CondensedDeclarationTypes => new List<ZString> { Constants.DeclarationTypes.Import.G1, Constants.DeclarationTypes.Import.G2, Constants.DeclarationTypes.Export.G5 };

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			var parent = Parent;
			var targetInfo = parent.JE_OH_SupplierInfo;
			var orgHeader = parent.Supplier;
			var isAddressOverride = parent.SupplierDocumentaryAddress?.E2_AddressOverride ?? ZBool.False;
			if (!isAddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			CheckEnglishCompanyNameIfNeed(targetInfo, orgHeader);
			if (parent.IsExport)
			{
				CheckPOAorPOCisValidIfNeed(targetInfo, orgHeader);
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			var parent = Parent;
			var targetInfo = parent.JE_OH_ImporterInfo;
			var orgHeader = parent.Importer;
			var isAddressOverride = parent.ImporterDocumentaryAddress?.E2_AddressOverride ?? ZBool.False;
			if (!isAddressOverride)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			CheckEnglishCompanyNameIfNeed(targetInfo, orgHeader);
			if (parent.IsImport)
			{
				CheckPOAorPOCisValidIfNeed(targetInfo, orgHeader);
			}
		}

		protected override void CheckJE_OH_Exporter()
		{
			base.CheckJE_OH_Exporter();
			CheckOrganizationCompanyAndAddressLength(Parent.JE_OH_ExporterInfo, Parent.Exporter?.MainAddress);
		}

		protected override void CheckJE_OH_Consignee()
		{
			base.CheckJE_OH_Consignee();
			CheckOrganizationCompanyAndAddressLength(Parent.JE_OH_ConsigneeInfo, Parent.OrgConsignee?.MainAddress);
		}

		protected override void CheckJE_OH_NotifyParty()
		{
			base.CheckJE_OH_NotifyParty();
			CheckOrganizationCompanyAndAddressLength(Parent.JE_OH_NotifyPartyInfo, Parent.NotifyParty?.MainAddress);
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();
			var targetInfo = Parent.JE_OA_DeclarantAddressInfo;
			var declarantAddress = Parent.DeclarantAddress;
			if (Parent.IsImport)
			{
				CheckOrganizationCompanyAndAddressLength(targetInfo, declarantAddress);
			}

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (declarantAddress?.Header is OrgHeader header &&
				!OrgHeaderHelper.CheckHasCusCode(header, Core.Constants.CountryCodes.Taiwan, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }))
			{
				targetInfo.AddMessageError(ValidationConstants.Declaration.Declarant.AValidIDIsReruired);
			}
		}

		void CheckOrganizationCompanyAndAddressLength(ZPropertyInfo targetInfo, OrgAddress address)
		{
			if (address != null)
			{
				IPartyDetails partyDetailsWrapper = new PartyDetailsWrapper("", "", "", address);

				if (partyDetailsWrapper.Name.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignNameMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignNameMaxlength));
				}
				if (partyDetailsWrapper.ChineseName.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength));
				}
				if (partyDetailsWrapper.Address.Line.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength));
				}
				if (partyDetailsWrapper.Address.ChineseLine.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength));
				}
			}
		}

		#region DeclarationNumberDisplay
		public void ValidateDeclarationNumberDisplay()
		{
			ValidateCalculatedProperty(Parent.DeclarationNumberDisplayInfo);
		}

		protected void CheckDeclarationNumberDisplay()
		{
			var parent = Parent;
			var targetInfo = parent.DeclarationNumberDisplayInfo;

			if (!CommonHelper.IsMatchEntryNumber(parent, parent.DeclarationNumber))
			{
				targetInfo.AddMessageError(ValidationConstants.AllocateNumber.KeyComponentValuesChanged);
			}

			if (parent.DeclarationNumberDisplay.IsEmpty)
			{
				if (RegistryHelper.ValidateEntryNumber)
				{
					targetInfo.AddMessageError(Res.GetString("EE4A67EC-8092-4BFF-B588-39D518A934FC", "You have not allocate an Entry number."));
				}
				else
				{
					targetInfo.AddWarning(Res.GetString("41E0CBB9-D10C-48E2-9559-2A7D67B378FE", "You have not allocated an Entry Number. The system will automatically allocate entry number when sending the message."));
				}
			}
		}
		#endregion

		void CheckEnglishCompanyNameIfNeed(ZPropertyInfo info, OrgHeader orgHeader)
		{
			if (!info.Value.IsEmpty && orgHeader != null && !OrgHeaderHelper.HasCompanyNameOfLanguage(orgHeader.Addresses.MainAddress, Core.Constants.Languages.English))
			{
				info.AddMessageError(ValidationConstants.Declaration.TheOrganizationShouldHaveEnglishCompanyName);
			}
		}

		void CheckPOAorPOCisValidIfNeed(ZPropertyInfo info, OrgHeader orgHeader)
		{
			if (!info.Value.IsEmpty)
			{
				poaValidator.Validate(Parent, orgHeader, info, RefDocTypes.PowerOfAttorney, RefDocTypes.PowerOfAttorneyCustoms, string.Empty, new List<(Predicate<JobRequiredDocument> MatchingCondition, string MatchingConditionText)>()
				{
					(ExtraMatch, "")
				}, AuthorityToActValidator.CurrentCountryOrDirectionCondition(Core.Constants.CountryCodes.Taiwan));
			}
		}

		AuthorityToActValidator poaValidator => Parent.Factory.GetCachedValue<AuthorityToActValidator>();

		protected Predicate<JobRequiredDocument> ExtraMatch
		{
			get
			{
				var customsOffice = Parent.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault()?.CEI_CustomsOffice.SubstringSafe(0, 1) ?? ZString.Empty;
				var result = new Predicate<JobRequiredDocument>(jobReqdoc => ZBool.False);
				if (!customsOffice.IsEmpty)
				{
					result = new Predicate<JobRequiredDocument>(jobReqDoc => (
											jobReqDoc.EQ_DocUsage == JobRequiredDocument.DocUsage.Broker
											&& !jobReqDoc.EQ_DocNumber.IsEmpty
											&& jobReqDoc.Attributes.Any(attrib => customsOffice == attrib.D0_AttribValue)
											&& jobReqDoc.EQ_DocCategory == Core.Constants.ReferenceTypes.ClientSupplierRelationship));
				}
				return result;
			}
		}

		protected override bool ShouldValidatePackagesActualPackageCount => false;

		protected override void CheckJE_TotalNoOfPacks()
		{
			var declaration = Parent;
			var totalNoOfPacksInfo = declaration.JE_TotalNoOfPacksInfo;
			var totalNoOfPacks = declaration.JE_TotalNoOfPacks;
			MandatoryValidation.MessageErrorIfIsZero(totalNoOfPacksInfo);
			CompareValidation.CheckNumberNotNegative(totalNoOfPacksInfo);
			ValidationHelper.CheckNoOfPacksBalance(declaration, totalNoOfPacksInfo);
			ValidateJE_TotalNoOfPacksPackType();
			if (totalNoOfPacks > 99999999)
			{
				totalNoOfPacksInfo.AddError(Res.GetString("0BAB818E-74B0-46FD-9555-94E47D92BACC", "The number {0} is too large, the maximum value allowed for {1} is 99,999,999.", totalNoOfPacks, totalNoOfPacksInfo.HumanReadableName));
			}
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TotalNoOfPacksPackTypeInfo);
			CheckJE_TotalNoOfPacksPackTypeIsAValidCode();
			ValidateJE_TotalNoOfPacks();
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			var declaration = Parent;
			var totalWeightInfo = declaration.JE_TotalWeightInfo;
			MandatoryValidation.MessageErrorIfNotEntered(totalWeightInfo);

			if (declaration.EntryHeader?.IsTotalNetWeightGreaterThanTotalGrossWeight ?? false)
			{
				totalWeightInfo.AddMessageError(ValidationConstants.CusEntryHeader.NetWeightNotBeGreaterThanGrossWeight);
			}
		}

		protected ZBool HasGovernmentVATCodeOrRodIdCardOrPassportNumber(OrgHeader organisation) =>
			organisation?.CustomsCodes?.Cast<OrgCusCode>().Any(x => (x.OK_CodeType == OrgCusCode.CodeTypes.VATCode || x.OK_CodeType == OrgCusCode.TaiwanCodeTypes.PID || x.OK_CodeType == OrgCusCode.CodeTypes.PassportID) && x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan) ?? false;

		void CheckZ99PortValidation(ZBool isZ99Port, ZPropertyInfo portInfo)
		{
			var portCode = (ZString)portInfo.Value;
			if (!portCode.IsEmpty)
			{
				if (isZ99Port && portCode.Length != 5)
				{
					portInfo.AddMessageError(ValidationConstants.Declaration.Z99UNLOCOCodeMustBe5Characters);
				}
				else if (RefCountry.LoadFromCountryCode(Parent.Factory, portCode.Left(2)) == null)
				{
					portInfo.AddMessageError(ValidationConstants.Declaration.Z99UNLOCOCodeMustHasValidCountryCode);
				}
			}
		}

		public override void ValidateAll()
		{
			Parent.LoadBeforeValationAll();
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateDeclarationNumberDisplay();
			}
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			var parent = Parent;
			if (parent.JE_MasterBill.IsEmpty && !parent.IsAir)
			{
				var targetInfo = parent.JE_MasterBillInfo;
				if (parent.AutomaticallyDeclareNILForDeclarationType)
				{
					targetInfo.AddWarning(ValidationConstants.Declaration.AutomaticallyDeclareNIL(targetInfo.HumanReadableName));
				}
				else if (parent.MasterBillIsRequiredForDeclarationType)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
				else
				{
					CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(targetInfo);
				}
			}
		}

		protected virtual void CheckMasterBillWhenImporterOrSupplierIsFreeTradeZone(ZPropertyInfo targetInfo)
		{
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			var parent = Parent;
			var propertyInfo = parent.JE_VoyageFlightNoInfo;
			if (parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			else if (parent.IsAir)
			{
				var flightNo = parent.JE_VoyageFlightNo;
				if (flightNo.IsEmpty)
				{
					var codes = new HashSet<string> { CertificateTypeList.Codes.Code15 };
					foreach (var invoiceLine in parent.FilteredInvoiceLines)
					{
						if (CheckInvoiceLineLinkControllingMsgHeadersCode(invoiceLine, codes))
						{
							propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(parent.JE_VoyageFlightNoInfo.Description));
							break;
						}
					}
				}
				else if (flightNo != Constants.NIL)
				{
					CommonHelper.CheckVoyageFlightNoFormat(propertyInfo, flightNo);
				}
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			var parent = Parent;
			if (parent.JE_ExportDate.IsEmpty)
			{
				var codes = new HashSet<string> { CertificateTypeList.Codes.Code15 };
				foreach (var invoiceLine in parent.FilteredInvoiceLines)
				{
					if (CheckInvoiceLineLinkControllingMsgHeadersCode(invoiceLine, codes))
					{
						parent.JE_ExportDateInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(parent.JE_ExportDateInfo.HumanReadableName));
						break;
					}
				}
			}
		}

		protected override void CheckJE_SLD()
		{
			var parent = Parent;
			if (!parent.JE_SLD.IsEmpty && Declaration.IsSea && parent.JE_SLD.Length != 4)
			{
				parent.JE_SLDInfo.AddMessageError(ValidationConstants.Declaration.LengthForSONoOrManifest);
			}
		}

		protected override void CheckJE_OtherBankAccount()
		{
			base.CheckJE_OtherBankAccount();
			var tW_OtherBankAccount = Parent.JE_OtherBankAccount;
			if (!tW_OtherBankAccount.IsEmpty && !tW_OtherBankAccount.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.JE_OtherBankAccountInfo.AddMessageError(ValidationConstants.Declaration.BankAccountOnlyAllowsAlphanumericCharacters);
			}
		}

		protected override void CheckJE_Z99FinalDestination()
		{
			base.CheckJE_Z99FinalDestination();
			var declaration = Declaration;
			if (declaration.IsZ99FinalDestination)
			{
				MandatoryValidation.WarnIfNotEntered(declaration.JE_Z99FinalDestinationInfo);
			}
		}

		protected override void CheckJE_Z99PortOfOrigin()
		{
			base.CheckJE_Z99PortOfOrigin();
			var declaration = Declaration;
			if (declaration.IsZ99PortOfOrigin)
			{
				MandatoryValidation.WarnIfNotEntered(declaration.JE_Z99PortOfOriginInfo);
			}
		}

		protected override void CheckJE_DeclDocType()
		{
			base.CheckJE_DeclDocType();
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_DeclDocTypeInfo);
		}

		ZBool CheckInvoiceLineLinkControllingMsgHeadersCode(JobComInvoiceLine invoiceLine, HashSet<string> codes) => invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().Any(x => x.IsLinkedCMHeader && x.MessageType == ControllingMessageTypeList.Codes.NX101 && codes.Contains(x.CertificateType));

		protected override Customs.Business.BillValidator GetBillValidator() => new BillValidator();
	}
}
