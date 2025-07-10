using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	using static UniversalReferenceConstants.ProcedureCodes;

	public partial class CusEntryInstructionValidation : AutoZACusEntryInstructionValidation
	{
		public CusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		protected new CusEntryInstruction Parent
		{
			get { return (CusEntryInstruction)base.Parent; }
		}

		protected JobDeclaration Declaration => EntryInstruction.JobDeclaration;

		protected CusEntryInstruction EntryInstruction => Parent;

		MessageDataProviderKeyFactor MessageKeyFactor
		{
			get { return Parent.MessageKeyFactor; }
		}

		protected override void CheckCEI_DateForDuty()
		{
			base.CheckCEI_DateForDuty();
			if (!Parent.CEI_DateForDuty.IsEmpty && !Parent.HasMovementReferenceNumber)
			{
				if (Parent.CEI_MRNToBeReplaced.IsEmpty)
				{
					Parent.CEI_DateForDutyInfo.AddWarning(ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
				}
				else
				{
					var originalAssessmentDate = Parent.JobDeclaration?.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber == Parent.CEI_MRNToBeReplaced)?.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Empty;

					if (!originalAssessmentDate.IsEmpty && Parent.CEI_DateForDuty != originalAssessmentDate)
					{
						Parent.CEI_DateForDutyInfo.AddWarning(ValidationConstants.EntryInstruction.AssessmentDateNotSameToOriginalAssessmentDate);
					}
				}
			}
		}

		#region CheckCEI_Style

		protected override void CheckCEI_Style()
		{
			base.CheckCEI_Style();
			MandatoryValidation.CheckEntered(Parent.CEI_StyleInfo);
			var declaration = Parent.JobDeclaration;
			if (declaration != null)
			{
				CheckCEI_Style_Origin(declaration);
				CheckCEI_Style_FinalDestination(declaration);
				CheckCEI_Style_Group(declaration);
				CheckCEI_Style_RemovalTransportCode(declaration);
				CheckCEI_Style_ImporterRebateUserCode();
			}
			ValidateCEI_OA_Warehouse();
			ValidateCEI_OA_Warehouse2();
			ValidateOnlyOneInstructionForChangeOfOwnership();
		}

		void ValidateOnlyOneInstructionForChangeOfOwnership()
		{
			var parent = Parent;
			if (parent.HasBothOutOfAndIntoRegimeProcedure)
			{
				var provider = parent?.JobDeclaration?.CustomsEntryInstructionProvider;
				var parentDeclarationInstructions = provider?.CustomsEntryInstructions;
				if (parentDeclarationInstructions != null && parentDeclarationInstructions.Count > 1)
				{
					parent.CEI_StyleInfo.AddError(OnlyOneInstructionForChangeOfOwnership);
				}
			}
		}

		public static string OnlyOneInstructionForChangeOfOwnership
		{
			get { return Res.GetString("{4283694F-B335-40A8-A322-425736E8F5E0}", "There is an Entry Instruction for Change of Ownership.\r\nA Change of Ownership job should have only one Entry Instruction."); }
		}

		#region CheckCEI_Style_FinalDestination

		void CheckCEI_Style_FinalDestination(JobDeclaration declaration)
		{
			var finalDestinationCountry = declaration.FinalDestination?.Country;
			if (finalDestinationCountry != null)
			{
				if (Parent.CEI_Style == _21)
				{
					if (finalDestinationCountry.RN_Code == Core.Constants.CountryCodes.SouthAfrica || finalDestinationCountry.IsBLNS)
					{
						Parent.CEI_StyleInfo.AddMessageError(FinalDestinationCannotBeZAOrBLNS(Parent.CusProcedure.ZZ6_Description));
					}
				}
				else if (Parent.CEI_Style == _22)
				{
					if (finalDestinationCountry.RN_Code == Core.Constants.CountryCodes.SouthAfrica)
					{
						Parent.CEI_StyleInfo.AddMessageError(FinalDestinationCannotBeZA(Parent.CusProcedure.ZZ6_Description));
					}
				}
			}
		}

		public static string FinalDestinationCannotBeZAOrBLNS(string procedureDescription)
		{
			return Res.GetString("DD389FF1-D1D1-4EF6-BB19-880163813ADA", "Final Destination cannot be in ZA or in a BLNS country/region for {0}", procedureDescription);
		}
		public static string FinalDestinationCannotBeZA(string procedureDescription)
		{
			return Res.GetString("2A712FF3-D232-483B-848D-7F9F1B0AA0A5", "Final Destination cannot be in ZA for {0}", procedureDescription);
		}

		#endregion

		#region CheckCEI_Style_Origin

		void CheckCEI_Style_Origin(JobDeclaration declaration)
		{
			var originCountry = declaration.Origin?.Country;
			if (originCountry != null)
			{
				if (Parent.CEI_Style == _21)
				{
					if (originCountry.RN_Code == Core.Constants.CountryCodes.SouthAfrica || originCountry.IsBLNS)
					{
						Parent.CEI_StyleInfo.AddMessageError(PortOfOriginCannotBeZAOrBLNS(Parent.CusProcedure.ZZ6_Description));
					}
				}
				else if (Parent.CEI_Style == _22)
				{
					if (!originCountry.IsBLNS)
					{
						Parent.CEI_StyleInfo.AddMessageError(PortOfOriginCannotBeZA(Parent.CusProcedure.ZZ6_Description));
					}
				}
			}
		}

		public static string PortOfOriginCannotBeZAOrBLNS(string procedureDescription)
		{
			return Res.GetString("DD71371C-295E-484B-934A-AC900BA49695", "Port of Origin cannot be in ZA or in a BLNS country/region for {0}", procedureDescription);
		}

		public static string PortOfOriginCannotBeZA(string procedureDescription)
		{
			return Res.GetString("A94A0E7A-4907-4E90-AEFB-AED7892242C2", "Port of Origin must be in a BLNS country/region for {0}", procedureDescription);
		}

		#endregion

		#region CheckCEI_Style_Group

		void CheckCEI_Style_Group(JobDeclaration declaration)
		{
			var group = Parent.ProcedureGroup;
			var instructions = Parent.JobDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions;
			if (instructions.Cast<CusEntryInstruction>().Any(x => ((x.CusProcedure?.ZZ6_Group ?? ZString.Empty) != group)))
			{
				var groupName = group.IsEmpty ? new ZString("No Group") : group;
				Parent.CEI_StyleInfo.AddMessageError(ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + groupName);
			}

			if (group == UniversalReferenceConstants.RefCusProcedureGroup.BLNS)
			{
				var origin = declaration.Origin?.Country;
				var destination = declaration.FinalDestination?.Country;
				if (origin != null && destination != null)
				{
					if (declaration.IsImport && (!origin.IsBLNS || destination.Code != Core.Constants.CountryCodes.SouthAfrica))
					{
						Parent.CEI_StyleInfo.AddMessageError(ValidationConstants.EntryInstruction.ImportBlnsToZaRequired);
					}
					else if (declaration.IsExport && (origin.Code != Core.Constants.CountryCodes.SouthAfrica || !destination.IsBLNS))
					{
						Parent.CEI_StyleInfo.AddMessageError(ValidationConstants.EntryInstruction.ExportZaToBlnsRequired);
					}
				}
			}
		}

		#endregion

		#region CheckCEI_Style_RemovalTransportCode

		void CheckCEI_Style_RemovalTransportCode(JobDeclaration declaration)
		{
			if (!declaration.IsImportByExternalBroker && MessageDataProviderInstruction.ShouldOutputRemovalTransportMode(MessageKeyFactor) && (declaration.JE_RemovalTransportCode.IsEmpty || (declaration.IsExport && declaration.JE_RemovalTransportCode == Core.Constants.TransportModes.Other)))
			{
				Parent.CEI_StyleInfo.AddMessageError(ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			}
		}

		#endregion

		#region CheckCEI_Style_ImporterRebateUserCode

		void CheckCEI_Style_ImporterRebateUserCode()
		{
			if (MessageDataProviderInstruction.ShouldOutputImporterRebateUserCodeAsRebateUserCode(MessageKeyFactor) && Parent.RebateUserCode.IsEmpty)
			{
				Parent.CEI_StyleInfo.AddMessageError(ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			}
		}

		#endregion

		#endregion

		protected override void CheckCEI_OH_BondHolder()
		{
			var targetInfo = Parent.CEI_OH_BondHolderInfo;
			CheckOrganisationContainsCode(Parent.BondHolder, targetInfo, OrgCusCode.CodeTypes.BondHolderCode);

			if (!Parent.CEI_OH_BondHolder.IsEmpty)
			{
				CheckBondHolderOrRemover(targetInfo, Parent.BondHolder?.OH_Code ?? ZString.Empty, Parent.BondHolderBondGuaranteeValue);
			}
			if (Parent.CEI_OH_BondHolder.IsEmpty && Parent.CEI_ProvisionalPaymentSuretyAmount.IsEmpty)
			{
				if (Parent.TotalBondSuretyAmount > Parent.BondGuaranteeValue)
				{
					targetInfo.AddMessageError(ValidationConstants.EntryInstruction.AdditionalBondIsRequiredWhenTotalBNDExceedsBGV);
				}
			}
		}

		void CheckRemover(ZPropertyInfo targetInfo, OrgHeader header, ZDecimal bondGuaranteeValue, ZBool isSelected)
		{
			CheckOrganisationContainsCode(header, targetInfo, OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode);

			if (Parent.CEI_OH_Carrier.IsEmpty)
			{
				if (Parent.TotalBondSuretyAmount > 0 && MessageDataProviderInstruction.RemoverTransporterCodeRequired(MessageKeyFactor))
				{
					targetInfo.AddMessageError(ValidationConstants.EntryInstruction.RemoverRequiredForCPC);
				}

				if (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.Value && Parent.IsBondHolderRequired)
				{
					targetInfo.AddMessageError(ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);
				}
			}

			if (Parent.CEI_OH_BondHolder.IsEmpty && isSelected)
			{
				CheckBondHolderOrRemover(targetInfo, header?.OH_Code ?? ZString.Empty, bondGuaranteeValue);
			}
		}

		protected override void CheckCEI_OH_Carrier()
		{
			var targetInfo = Parent.CEI_OH_CarrierInfo;
			CheckRemover(targetInfo, Parent.Remover, Parent.RemoverBondGuaranteeValue, Parent.CEI_RemoverEDI);
		}

		public void ValidateOH_SubContractor()
		{
			((IValidationInternals)this).Validate(Parent.OH_SubContractorInfo, CheckOH_SubContractor);
		}

		protected virtual void CheckOH_SubContractor()
		{
			var targetInfo = Parent.OH_SubContractorInfo;
			TypeValidation.CheckValidGuid(targetInfo);
			CheckRemover(targetInfo, Parent.SubContractor, Parent.SubContractorBondGuaranteeValue, Parent.CEI_SubContractorEDI);
		}

		protected override void CheckCEI_OH_Owner()
		{
			base.CheckCEI_OH_Owner();
			if (Parent.HasBothOutOfAndIntoRegimeProcedure)
			{
				if (Parent.CEI_OH_Owner.IsEmpty)
				{
					Parent.CEI_OH_OwnerInfo.AddMessageError(ValidationConstants.EntryInstruction.NewOwnerRequiredForCPC);
				}
				else
				{
					if (Parent.CEI_OH_Owner == Parent.JobDeclaration?.JE_OH_Importer)
					{
						Parent.CEI_OH_OwnerInfo.AddMessageError(ValidationConstants.EntryInstruction.NewOwnerShouldBeDifferentToOld);
					}

					var instructions = Parent.JobDeclaration?.CustomsEntryInstructionProvider?.CustomsEntryInstructions;
					if (instructions?.Cast<CusEntryInstruction>()?.Any(x => (x.HasBothOutOfAndIntoRegimeProcedure && x.CEI_OH_Owner != Parent.CEI_OH_Owner)) ?? false)
					{
						Parent.CEI_OH_OwnerInfo.AddMessageError(ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
					}

					var newOwner = Parent.Factory.Load<OrgHeader>(Parent.CEI_OH_Owner);
					if (newOwner == null || newOwner.LocalCustomsClientCode.IsEmpty)
					{
						Parent.CEI_OH_OwnerInfo.AddMessageError(ValidationConstants.EntryInstruction.NewOwnerRequireCustomsImporterCode);
					}
				}
			}
		}

		protected override void CheckCEI_OA_Warehouse()
		{
			base.CheckCEI_OA_Warehouse();
			var startsWith = ToWarehouseMustMatchCustomsOffice
				? ZString.Empty
				: Parent.CustomsOffice;

			CheckOrganisationAddressContainsCodeStartingWith(startsWith, Parent.Warehouse, Parent.CEI_OA_WarehouseInfo, OrgCusCode.CodeTypes.WarehouseControlledPremisesID);

			if (Parent.CEI_OA_Warehouse.IsEmpty)
			{
				if (MessageDataProviderInstruction.IsFromWarehouseRequired(MessageKeyFactor))
				{
					Parent.CEI_OA_WarehouseInfo.AddMessageError(ValidationConstants.EntryInstruction.FromWarehouseRequiredForCPC);
				}
			}
			else if (Parent.JobDeclaration?.IsImportByExternalBroker ?? false)
			{
				Parent.CEI_OA_WarehouseInfo.AddMessageError(MandatoryValidation.DoNotEnterMessage(Parent.CEI_OA_WarehouseInfo.HumanReadableName));
			}
			ValidateCEI_OA_Warehouse2();
		}

		bool ToWarehouseMustMatchCustomsOffice => Parent.CEI_Style == _41 || Parent.CEI_Style == _44 || Parent.CEI_Style == _46 || Parent.CEI_Style == _47 || Parent.CEI_Style == _48;

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();

			var startsWith = ToWarehouseMustMatchCustomsOffice
				? Parent.CustomsOffice
				: ZString.Empty;

			CheckOrganisationAddressContainsCodeStartingWith(startsWith, Parent.Warehouse2, Parent.CEI_OA_Warehouse2Info, OrgCusCode.CodeTypes.WarehouseControlledPremisesID);

			if (MessageDataProviderInstruction.ShouldOutputToWarehouse(MessageKeyFactor))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_OA_Warehouse2Info);
			}
			CheckCEI_OA_Warehouse2_SimilarWarehouses();
		}

		void CheckCEI_OA_Warehouse2_SimilarWarehouses()
		{
			if (Parent.HasBothOutOfAndIntoRegimeProcedure)
			{
				var fromWarehouse = Parent.Warehouse;
				var toWarehouse = Parent.Warehouse2;
				if (fromWarehouse == null || fromWarehouse != toWarehouse)
				{
					Parent.CEI_OA_Warehouse2Info.AddMessageError(ValidationConstants.EntryInstruction.SameToFromWarehouseRequired);
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOH_SubContractor();
		}

		#region Implementation
		public const string CTNCodeStartingWith = "'CTN', 'STE', 'WOR' OR 'PRL'";

		public void CheckOrganisationAddressContainsCodeStartingWith(ZString codeStartsWith, OrgAddress checkingAddress, ZPropertyInfo targetInfo, string codeTypeToCheck)
		{
			if (checkingAddress != null)
			{
				var regNo = checkingAddress.GetRegNoWithOrganisationAddress(codeTypeToCheck);

				if (regNo.IsEmpty)
				{
					targetInfo.AddMessageError(Res.GetString("6EB01479-BB6B-43E9-B328-D36E7A825B59", "The selected organization Address does not have a {0}", ZAOrgCusCode.GetDescriptionFromCode(codeTypeToCheck)));
				}
				else if ("CTN".Equals(codeStartsWith, StringComparison.OrdinalIgnoreCase))
				{
					if (!regNo.StartsWith("CTN", StringComparison.OrdinalIgnoreCase) &&
						!regNo.StartsWith("STE", StringComparison.OrdinalIgnoreCase) &&
						!regNo.StartsWith("WOR", StringComparison.OrdinalIgnoreCase) &&
						!regNo.StartsWith("PRL", StringComparison.OrdinalIgnoreCase))
					{
						targetInfo.AddMessageError(Res.GetString("98D00C19-C5B4-4725-B6AD-DF647DA08485", "The {0} code must start with {1}", ZAOrgCusCode.GetDescriptionFromCode(codeTypeToCheck), CTNCodeStartingWith));
					}
				}
				else if (!regNo.StartsWith(codeStartsWith, StringComparison.OrdinalIgnoreCase))
				{
					targetInfo.AddMessageError(Res.GetString("FA4AF4D9-D5EA-4C71-8292-3EFB5DF3BC54", "The {0} code must start with '{1}'", ZAOrgCusCode.GetDescriptionFromCode(codeTypeToCheck), codeStartsWith));
				}
			}
		}

		public void CheckOrganisationContainsCode(OrgHeader checkingOrg, ZPropertyInfo targetInfo, string codeTypeToCheck)
		{
			if (checkingOrg != null && checkingOrg.CustomsCodes.GetCustomsRegNo(codeTypeToCheck, Core.Constants.CountryCodes.SouthAfrica).IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("05859743-F4DD-437F-A141-D98E74342D1E", "The selected organization does not have a {0}", ZAOrgCusCode.GetDescriptionFromCode(codeTypeToCheck)));
			}
		}

		CodeDescriptionPairList ZAOrgCusCode
		{
			get { return fZAOrgCusCode ?? (fZAOrgCusCode = new OrgCodeLists().CustomsCodes_List(Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.SouthAfrica))); }
		}
		CodeDescriptionPairList fZAOrgCusCode;

		void CheckBondHolderOrRemover(ZPropertyInfo targetInfo, ZString code, ZDecimal bondGuaranteeValue)
		{
			if (!bondGuaranteeValue.IsEmpty && Parent.TotalBondSuretyAmount > bondGuaranteeValue)
			{
				targetInfo.AddMessageError(Res.GetString("C843423C-0FDF-421E-8E41-64CDBF68983E", "Bond amount from lines exceed Bond Guarantee Value for {0}, customs will likely reject this entry.", code));
			}
			if (Parent.TotalBondSuretyAmount > 0 && bondGuaranteeValue <= 0 && ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.Value)
			{
				targetInfo.AddMessageError(ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);
			}
		}
		#endregion

		protected override void CheckCEI_CustomsOfficeOverride()
		{
			base.CheckCEI_CustomsOfficeOverride();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_CustomsOfficeOverrideInfo, Parent.Lookups.CustomsOfficeList);
		}

		protected override void CheckCEI_UCROverride()
		{
			base.CheckCEI_UCROverride();
			var declaration = Declaration;
			var instruction = EntryInstruction;
			instruction.UCRHelper.ValidateUCR(instruction, declaration);
		}

		protected override void CheckCEI_BankCode()
		{
			base.CheckCEI_BankCode();
			ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_BankCodeInfo, Parent.Lookups.BankCodes);
		}

		protected override void CheckCEI_RefType()
		{
			base.CheckCEI_RefType();
			if (!EntryInstruction.CEI_IsUCROverridden)
			{
				ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_RefTypeInfo, Parent.Lookups.RefTypeList);

				if (EntryInstruction.CEI_RefType == RefTypeList.Codes.Invoice)
				{
					var invoiceNumberLength = UCRHelper.BestInvoice(EntryInstruction.JobDeclaration)?.JZ_InvoiceNumber.KeepAlphanumericCharacters().Length ?? 0;
					if (invoiceNumberLength > 19)
					{
						EntryInstruction.CEI_RefTypeInfo.AddWarning(ValidationConstants.EntryInstruction.UCRInvoiceRefNumTruncated);
					}
					else if (invoiceNumberLength == 0)
					{
						EntryInstruction.CEI_RefTypeInfo.AddWarning(ValidationConstants.EntryInstruction.UCRNoInvoicesFound);
					}
				}
			}
		}

		protected override void CheckCEI_EntityType()
		{
			base.CheckCEI_EntityType();

			if (Declaration.IsBLNSValidationRequired)
			{
				if (!EntryInstruction.CEI_IsUCROverridden && RequiresAValidMainSupplier)
				{
					ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_EntityTypeInfo, Parent.Lookups.EntityTypeList);
					UCRHelper.GetEntityCode(EntryInstruction.CEI_EntityType, EntryInstruction.JobDeclaration);
					var decisionReason = UCRHelper.DecisionReason;
					if (!decisionReason.IsEmpty)
					{
						EntryInstruction.CEI_EntityTypeInfo.AddMessageError(decisionReason);
					}
				}
			}
		}

		protected override void CheckCEI_Scope()
		{
			base.CheckCEI_Scope();
			if (!EntryInstruction.CEI_IsUCROverridden)
			{
				ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_ScopeInfo, Parent.Lookups.ScopeList);
			}
		}

		protected override void CheckCEI_TransactionValue()
		{
			base.CheckCEI_TransactionValue();

			if (EntryInstruction.JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import && EntryInstruction.CEI_TransactionValue == 0)
			{
				EntryInstruction.CEI_TransactionValueInfo.AddWarning(ResString.GetMultilingualString("5520848C-FE35-47AD-BF3B-6E5AD3CE8834", "Transaction Value & Currency is required if an Advance Payment Notification (APN) has been declared."));
			}
			if (EntryInstruction.CEI_TransactionValue.DecimalPlaces > 0)
			{
				EntryInstruction.CEI_TransactionValueInfo.AddMessageError(ResString.GetMultilingualString("3B361E64-AAEF-4C02-B072-0628DA56953F", "Transaction Value - Decimal Places not allowed, Please recapture"));
			}

			ValidateCEI_CreditTerms();
		}

		protected override void CheckCEI_RX_NKTransactionValueCurrency()
		{
			base.CheckCEI_RX_NKTransactionValueCurrency();
			if (EntryInstruction.CEI_RX_NKTransactionValueCurrency.IsEmpty && EntryInstruction.CEI_TransactionValue != 0)
			{
				EntryInstruction.CEI_RX_NKTransactionValueCurrencyInfo.AddMessageError(ResString.GetMultilingualString("E3669206-455D-4190-8A22-43E3118CF1D3", "Transaction currency cannot be empty if transaction value is not 0."));
			}
			ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_RX_NKTransactionValueCurrencyInfo, Parent.Lookups.TransactionValueCurrencies);
		}

		protected override void CheckCEI_CreditTerms()
		{
			base.CheckCEI_CreditTerms();

			var invoiceDetailsEnabled = JobComInvoiceLine.ZAAddInvoiceDetailsToCUSDECMessageEnabled;
			var messageType = Declaration?.JE_MessageType ?? ZString.Empty;
			var isExport = messageType == ZAJobMessageTypeList.Codes.Export;

			if (invoiceDetailsEnabled)
			{
				if (isExport && EntryInstruction.CEI_CreditTerms.IsEmpty)
				{
					Parent.CEI_CreditTermsInfo.AddMessageError(ValidationConstants.EntryInstruction.CreditTermsRequiredForExport);
				}
				else
				{
					if (isExport)
					{
						ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.CreditTermInvalid);
					}
					else if (!EntryInstruction.CEI_CreditTerms.IsEmpty)
					{
						Parent.CEI_CreditTermsInfo.AddMessageError(ValidationConstants.EntryInstruction.CreditTermsOnlyRequiredForExport);
					}
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(EntryInstruction.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.NoValidCreditTerm);

				if (Declaration != null && Declaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export && EntryInstruction.CEI_CreditTerms.IsEmpty)
				{
					Parent.CEI_CreditTermsInfo.AddMessageError(ValidationConstants.EntryInstruction.EmptyCreditTerms);
				}
			}

			if (isExport && (!EntryInstruction.CEI_CreditTerms.IsEmpty || !invoiceDetailsEnabled))
			{
				if (EntryInstruction.CEI_CreditTerms == CreditTermsCodeList.Codes.NEP)
				{
					if (EntryInstruction.CEI_TransactionValue != 0 || !EntryInstruction.CEI_BankCode.IsEmpty)
					{
						Parent.CEI_CreditTermsInfo.AddMessageError(ValidationConstants.EntryInstruction.InvalidNEPCreditTermSelection);
					}
				}
				else if (EntryInstruction.CEI_CreditTerms.IsNumbersOnlyOrEmpty)
				{
					if (EntryInstruction.CEI_TransactionValue <= 0)
					{
						EntryInstruction.CEI_CreditTermsInfo.AddMessageError(ValidationConstants.EntryInstruction.InvalidNumberCreditTermSelection);
					}
				}
			}
		}

		protected override void CheckCEI_UCROrderNumber()
		{
			base.CheckCEI_UCROrderNumber();

			if (Declaration.IsBLNSValidationRequired)
			{
				var declaration = Declaration;
				var entryInstruction = EntryInstruction;

				if (declaration != null && (declaration.IsExport || declaration.IsImport))
				{
					var ucrReferenceNumber = entryInstruction.CEI_UCROrderNumber;

					if (!ucrReferenceNumber.IsEmpty)
					{
						if (entryInstruction.CEI_RefType == RefTypeList.Codes.Invoice || entryInstruction.CEI_RefType == RefTypeList.Codes.DeclarantGenerated)
						{
							entryInstruction.CEI_UCROrderNumberInfo.AddWarning(ValidationConstants.EntryInstruction.UCRValueWillNotBeUsed);
						}
						else if (!((ZInt)ucrReferenceNumber.Length).IsInRange(14, 19) || !ucrReferenceNumber.IsLettersAndNumbersOnlyOrEmpty)
						{
							entryInstruction.CEI_UCROrderNumberInfo.AddMessageError(ValidationConstants.EntryInstruction.InvalidUCRNumber);
						}
					}
					else if (!entryInstruction.CEI_IsUCROverridden && !(entryInstruction.CEI_RefType == RefTypeList.Codes.Invoice || entryInstruction.CEI_RefType == RefTypeList.Codes.DeclarantGenerated))
					{
						entryInstruction.CEI_UCROrderNumberInfo.AddMessageError(ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
					}
				}
			}
		}

		protected override void CheckCEI_PreviousMRN()
		{
			base.CheckCEI_PreviousMRN();
			var isimx = EntryInstruction.JobDeclaration?.IsImportByExternalBroker ?? false;
			if (isimx)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_PreviousMRNInfo);
				var coll = EntryInstruction.JobDeclaration?.CustomsEntryInstructions;
				if (coll != null && coll.OfType<CusEntryInstruction>().Any(x => x.CEI_PreviousMRN == Parent.CEI_PreviousMRN && x.PK != Parent.PK))
				{
					Parent.CEI_PreviousMRNInfo.AddMessageError(Res.GetString("31CCF87C-7BAF-4F56-AA6B-B17592A0BFF4", "Same WHS MRN is entered on a different instruction on this job"));
				}
			}
			else if (JobComInvoiceLineValidation.IsReExportToBLNSCPC(EntryInstruction.CEI_Style))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CEI_PreviousMRNInfo);
			}
			ValidationHelper.ValidateMRNFormat(Parent.CEI_PreviousMRNInfo);
		}

		protected override void CheckCEI_PortOfExit()
		{
			base.CheckCEI_PortOfExit();
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_PortOfExitInfo, Parent.Lookups.PortsOfExit);

			if (Parent.CEI_PortOfExit.IsEmpty)
			{
				if (MessageDataProviderInstruction.ShouldOutputPortOfExit(MessageKeyFactor))
				{
					Parent.CEI_PortOfExitInfo.AddMessageError(ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				}
			}
			else
			{
				if (!MessageDataProviderInstruction.ShouldOutputPortOfExit(MessageKeyFactor))
				{
					if (MessageKeyFactor.ShipmentType != ZAJobMessageTypeList.Codes.Import && !(MessageKeyFactor.CountryOfDestination?.IsBLNS ?? false))
					{
						Parent.CEI_PortOfExitInfo.AddMessageError(ValidationConstants.EntryInstruction.PortOfDestinationOrExitNotRequired);
					}
				}
				if (Declaration != null && Declaration.IsExport && (Declaration.IsRoad || Declaration.IsRail) && Parent.CEI_PortOfExit != EntryInstruction.CustomsOffice)
				{
					if (MessageKeyFactor.CPC != UniversalReferenceConstants.ProcedureCodes._52
						&& MessageKeyFactor.CPC != UniversalReferenceConstants.ProcedureCodes._53
						&& MessageKeyFactor.CPC != UniversalReferenceConstants.ProcedureCodes._67
						&& MessageKeyFactor.CPC != UniversalReferenceConstants.ProcedureCodes._68)
					{
						Parent.CEI_PortOfExitInfo.AddMessageError(ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
					}
				}
			}
		}

		protected override void CheckCEI_ProvisionalPaymentType()
		{
			base.CheckCEI_ProvisionalPaymentType();
			var targetInfo = EntryInstruction.CEI_ProvisionalPaymentTypeInfo;
			if (MessageKeyFactor.IsImport() || MessageKeyFactor.IsExBond())
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
				if (EntryInstruction?.ProvisionalPaymentPayInfos?.HasPPTypeBeenLiquidated(Parent.CEI_ProvisionalPaymentType, 1) ?? false)
				{
					targetInfo.AddWarning(ValidationConstants.EntryInstruction.ProvisionalPaymentCaseAlreadyClosed);
				}
			}
		}

		protected override void CheckCEI_ProvisionalPaymentAmount()
		{
			base.CheckCEI_ProvisionalPaymentAmount();
			if (MessageKeyFactor.IsImport() || MessageKeyFactor.IsExBond())
			{
				var targetInfo = Parent.CEI_ProvisionalPaymentAmountInfo;
				if (!Parent.CEI_ProvisionalPaymentType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfIsZero(targetInfo);
					if (!Parent.CEI_ProvisionalPaymentAmount.IsEmpty)
					{
						var entryHeader = EntryInstruction?.EntryHeader;
						if (entryHeader != null && entryHeader.MergedLines.OfType<CusEntryLine>().All(x => !x.IsLine1))
						{
							targetInfo.AddMessageError(ValidationConstants.EntryInstruction.LineOneIsRequiredForHeaderLevelProvisionalPayment);
						}
					}
				}
			}
		}

		protected override void CheckCEI_MRNToBeReplaced()
		{
			base.CheckCEI_MRNToBeReplaced();
			var targetInfo = Parent.CEI_MRNToBeReplacedInfo;
			if (!Parent.CEI_MRNToBeReplaced.IsEmpty)
			{
				if (Declaration != null && Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().All(x => x.MovementReferenceNumber != Parent.CEI_MRNToBeReplaced))
				{
					targetInfo.AddWarning(ValidationConstants.EntryInstruction.MRNToBeReplacedNotInThisJob);
				}
				if (EntryInstruction.CEI_DateForDuty.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.EntryInstruction.MRNToBeReplacedEntered);
				}
			}

			ValidationHelper.ValidateMRNFormat(targetInfo);
		}

		protected override void CheckCEI_ExchangeRateDate()
		{
			base.CheckCEI_ExchangeRateDate();
			var targetInfo = Parent.CEI_ExchangeRateDateInfo;
			if (!Parent.UZ_ExchangeRateDate_ReadOnly && !Parent.CEI_ExchangeRateDate.IsEmpty)
			{
				if (EntryInstruction.CEI_DateForDuty.IsEmpty)
				{
					targetInfo.AddWarning(ValidationConstants.EntryInstruction.ExchangeRateDateWillBeCalculated);
				}
				else
				{
					if (Parent.CEI_ExchangeRateDate.Date > EntryInstruction.CEI_DateForDuty.Date)
					{
						targetInfo.AddWarning(ValidationConstants.EntryInstruction.ExchangeRateDateGreaterThanAssessmentDate);
					}
					else if (Parent.CEI_ExchangeRateDate.Date < EntryInstruction.CEI_DateForDuty.Date.AddDays(-2))
					{
						targetInfo.AddWarning(ValidationConstants.EntryInstruction.ExchangeRateDateTooFarFromAssessmentDate);
					}
				}
			}
		}

		protected override void CheckCEI_RebateUserOverride()
		{
			base.CheckCEI_RebateUserOverride();

			if (!EntryInstruction.CEI_RebateUserOverride.IsEmpty)
			{
				var rebateCode = EntryInstruction.RebateOverrideUser?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.RebateUserCode, Core.Constants.CountryCodes.SouthAfrica) ?? ZString.Empty;

				if (rebateCode.IsEmpty)
				{
					Parent.CEI_RebateUserOverrideInfo.AddMessageError(ValidationConstants.EntryInstruction.RebateUserOverrideHasNoRebateCode);
				}
			}
		}

		protected override void CheckCEI_ProvisionalPaymentSuretyAmount()
		{
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CEI_ProvisionalPaymentSuretyAmountInfo);
		}

		bool RequiresAValidMainSupplier => Declaration.JE_MessageType != ZAJobMessageTypeList.Codes.ExBond;

		UCRHelper UCRHelper => fUCRHelper ?? (fUCRHelper = new UCRHelper());
		UCRHelper fUCRHelper;
	}
}
