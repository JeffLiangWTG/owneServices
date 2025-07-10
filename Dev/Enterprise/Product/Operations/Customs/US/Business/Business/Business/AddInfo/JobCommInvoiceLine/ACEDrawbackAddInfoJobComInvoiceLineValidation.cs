using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackAddInfoJobComInvoiceLineValidation : CommonDrawbackAddInfoJobComInvoiceLineValidation
	{
		public ACEDrawbackAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckUS_DRWImpActInd()
		{
			base.CheckUS_DRWImpActInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWImpActIndInfo, Parent.Lookups.ACEDrawbackActionList);

			if (Parent.US_DRWIsForImportSection || Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImpActIndInfo);
			}

			ValidateUS_DRWIsForManufacturerSection();
			ValidateUS_DRWClaimBasis();
		}

		protected override void CheckUS_DRWClaimBasis()
		{
			base.CheckUS_DRWClaimBasis();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWClaimBasisInfo, Parent.Lookups.DrawbackClaimBasisList);

			if (Parent.US_DRWIsForImportSection)
			{
				if (Parent.US_DRWClaimBasis.IsEmpty && ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWImpActInd))
				{
					Parent.US_DRWClaimBasisInfo.AddMessageError(ClaimBasisRequired);
				}
			}
		}
		internal const string ClaimBasisRequired = "Basis of Cliam is required when Import Action Indicator is 'X' or 'T'.";

		protected override void CheckUS_DRWImportQuantity()
		{
			base.CheckUS_DRWImportQuantity();

			if (Parent.US_DRWIsForImportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportQuantityInfo);
			}
		}

		protected override void CheckUS_DRWImportUQ()
		{
			base.CheckUS_DRWImportUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWImportUQInfo, Parent.Lookups.InvoiceUQList);

			if (Parent.US_DRWIsForImportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportUQInfo);
			}
		}

		protected override void CheckUS_DRWValuePerUQ()
		{
			base.CheckUS_DRWValuePerUQ();

			var parent = Parent;
			if (parent.US_DRWIsForImportSection)
			{
				if (parent.DRWGoodsValuePerUQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.US_DRWValuePerUQInfo);
				}
				else if (parent.IsValuePerUQNotMatch)
				{
					parent.US_DRWValuePerUQInfo.AddWarning(parent.ValuePerUQNotMatchNotification);
				}
			}
		}

		protected override void CheckUS_DRWDateRcvFrom()
		{
			base.CheckUS_DRWDateRcvFrom();

			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				if (declaration.IsDrawbackTFTEA || ACEDrawbackProvisionsList.Is1313A(declaration.US_EntryType))
				{
					if (!Parent.US_DRWDateRcvFrom.IsEmpty)
					{
						Parent.US_DRWDateRcvFromInfo.AddMessageError(DateNotRequiredForTFTEAAnd1313A);
					}
				}
				else if (Parent.US_DRWIsForImportSection && ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWImpActInd))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWDateRcvFromInfo);
				}
			}
		}
		internal const string DateNotRequiredForTFTEAAnd1313A = "Date is not required if you are filing Drawback under 1313(A) or Drawback Provision is 51-77.";

		protected override void CheckUS_DRWDateUsedFrom()
		{
			base.CheckUS_DRWDateUsedFrom();

			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				if (ACEDrawbackProvisionsList.Is1313A(declaration.US_EntryType))
				{
					if (!Parent.US_DRWDateUsedFrom.IsEmpty)
					{
						Parent.US_DRWDateUsedFromInfo.AddMessageError(DateNotRequiredFor1313A);
					}
				}
				else if (Parent.US_DRWIsForImportSection && ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWImpActInd))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWDateUsedFromInfo);
				}
			}
		}
		internal const string DateNotRequiredFor1313A = "Date is not required if you are filing Drawback under 1313(A)";

		protected override void CheckUS_DRWQuantityUsed()
		{
			base.CheckUS_DRWQuantityUsed();

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWQuantityUsedInfo);
			}
		}

		protected override void CheckUS_DRWUQUsed()
		{
			base.CheckUS_DRWUQUsed();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWUQUsedInfo, Parent.Lookups.CustomsUQList);

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWUQUsedInfo);
			}
		}

		protected override void CheckUS_DRWDateOfManufacture()
		{
			base.CheckUS_DRWDateOfManufacture();

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWDateOfManufactureInfo);
			}
		}

		protected override void CheckUS_DRWFactoryLocation()
		{
			base.CheckUS_DRWFactoryLocation();

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWFactoryLocationInfo);
			}
		}

		protected override void CheckUS_DRWDescrManufactured()
		{
			base.CheckUS_DRWDescrManufactured();

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWDescrManufacturedInfo);
			}
		}

		protected override void CheckUS_DRWManufRuleNo()
		{
			base.CheckUS_DRWManufRuleNo();

			if (Parent.US_DRWIsForManufacturerSection)
			{
				if (ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWMafActInd))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWManufRuleNoInfo);
				}
				if (!Parent.US_DRWManufRuleNo.IsEmpty && Parent.US_DRWManufRuleNo == Parent.US_DRWImpManufRuleNo)
				{
					Parent.US_DRWManufRuleNoInfo.AddMessageError(DuplicateManufRuleNo);
				}
			}
		}
		internal const string DuplicateManufRuleNo = "Manufactured Articles Manufacturing Ruling Number cannot be the same as the Import Manufacturing Ruling Number";

		protected override void CheckUS_DRWImpTrkID()
		{
			base.CheckUS_DRWImpTrkID();
			int impTrkNumber;
			if (!Parent.US_DRWImpTrkID.IsEmpty && int.TryParse(Parent.US_DRWImpTrkID, out impTrkNumber) && impTrkNumber > (JobComInvoiceLine.MaxImpTrackingNumber - 1))
			{
				Parent.US_DRWImpTrkIDInfo.AddMessageError(MaxImportTrackingNumberOverflow);
			}
		}
		internal const string MaxImportTrackingNumberOverflow = "Drawbacks are limited to 49,999 lines in order to accommodate the ACE CATAIR specifications.";

		protected override void CheckUS_DRWExportAction()
		{
			base.CheckUS_DRWExportAction();

			ValidateUS_DRWExportDest();
			ValidateUS_DRWExpBOLInd();
			ValidateUS_DRWExpBOLCarrier();
		}

		protected override void CheckUS_DRWExportQuantity()
		{
			base.CheckUS_DRWExportQuantity();

			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportQuantityInfo);
			}

			if (Parent.NoDrawbackAmounts)
			{
				Parent.US_DRWExportQuantityInfo.AddMessageError(CalculatedAmountShouldBeGreaterThanZero);
			}
		}

		internal const string CalculatedAmountShouldBeGreaterThanZero = "Please specify at least one amount for drawback claim.";

		protected override void CheckUS_DRWExportUQ()
		{
			base.CheckUS_DRWExportUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWExportUQInfo, Parent.Lookups.InvoiceUQList);

			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportUQInfo);
			}
		}

		protected override void CheckUS_DRWExportID()
		{
			base.CheckUS_DRWExportID();

			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportIDInfo);
			}
		}

		protected override void CheckUS_DRWExportDest()
		{
			base.CheckUS_DRWExportDest();

			if (Parent.US_DRWExportDest != JobComInvoiceLine.ForeignTradeZoneISOCode && Parent.US_DRWExportDest != JobComInvoiceLine.ForeignDestinationISOCode && Parent.US_DRWExportDest != JobComInvoiceLine.OuterSpaceISOCode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWExportDestInfo);
			}

			if (Parent.US_DRWIsForExportSection)
			{
				if (Parent.US_DRWExportAction == "D" && !Parent.US_DRWExportDest.IsEmpty)
				{
					Parent.US_DRWExportDestInfo.AddMessageError(FieldIrrelevantForDestroy);
				}
				else if (Parent.US_DRWExportAction == "E")
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWExportDestInfo);
				}
			}
		}
		protected override void CheckUS_DRWExpBOLInd()
		{
			base.CheckUS_DRWExpBOLInd();

			if (Parent.US_DRWIsForExportSection && Parent.US_DRWExportAction == "D" && Parent.US_DRWExpBOLInd)
			{
				Parent.US_DRWExpBOLIndInfo.AddMessageError(FieldIrrelevantForDestroy);
			}
		}

		protected override void CheckUS_DRWExpBOLCarrier()
		{
			base.CheckUS_DRWExpBOLCarrier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWExpBOLCarrierInfo, Parent.AddInfoLookups.USCarrierList);

			if (Parent.US_DRWIsForExportSection)
			{
				if (Parent.US_DRWExportAction == "D" && !Parent.US_DRWExpBOLCarrier.IsEmpty)
				{
					Parent.US_DRWExpBOLCarrierInfo.AddMessageError(FieldIrrelevantForDestroy);
				}
				else if (Parent.US_DRWExpBOLInd && Parent.US_DRWExpBOLCarrier.IsEmpty)
				{
					Parent.US_DRWExpBOLCarrierInfo.AddMessageError(BOLCarrierCodeRequired);
				}
			}
		}
		internal const string FieldIrrelevantForDestroy = "This is not required when Action Code is 'D'.";
		internal const string BOLCarrierCodeRequired = "BOL Carrier Code is required when BOL Indicator is ticked.";

		protected override void CheckUS_DRWExportDate()
		{
			base.CheckUS_DRWExportDate();

			if (Parent.US_DRWExportDate.IsValid && !Parent.US_DRWExportDate.IsInThePastDatePartOnly)
			{
				Parent.US_DRWExportDateInfo.AddMessageError(ExportDateMustBeInThePast);
			}
		}
		internal const string ExportDateMustBeInThePast = "Export/Destroy date must be a date in the past.";

		protected override bool ShouldCheckTariffIfInvalid
		{
			get { return true; }
		}

		protected override void CheckUS_TariffType()
		{
			base.CheckUS_TariffType();

			ValidateUS_ExportTariff();
		}

		protected override void CheckUS_ImportEntryNo()
		{
			base.CheckUS_ImportEntryNo();

			var declaration = Parent.Declaration;
			if (declaration != null)
			{
				if (!Parent.US_ImportEntryNo.IsEmpty && ACEDrawbackProvisionsList.Is1313D(declaration.US_EntryType))
				{
					Parent.US_ImportEntryNoInfo.AddMessageError(EntryNumberNotRequired);
				}
			}
		}
		internal const string EntryNumberNotRequired = "Entry Filer Code and Entry Number are not allowed when you are filing Drawback under 1313(D)";

		protected override void CheckUS_DRWImportEntryLine()
		{
			base.CheckUS_DRWImportEntryLine();

			var declartion = Parent.Declaration;
			if (declartion != null)
			{
				if (!Parent.US_DRWImportEntryLine.IsEmpty && ACEDrawbackProvisionsList.Is1313D(declartion.US_EntryType))
				{
					Parent.US_DRWImportEntryLineInfo.AddMessageError(LineNumberNotRequired);
				}
				else if (!Parent.US_ImportEntryNo.IsEmpty && Parent.US_DRWImportEntryLine.IsEmpty)
				{
					Parent.US_DRWImportEntryLineInfo.AddWarning(YouHaveNotEnteredEntryLineNumber);
				}

				var lineNumberRequired = Parent.US_DRWIsForImportSection && Parent.US_DRWImportEntryLine.IsEmpty;
				if (lineNumberRequired)
				{
					lineNumberRequired = Parent.Declaration != null && !Parent.Declaration.US_EntryType.IsEmpty && ACEDrawbackProvisionsList.IsTFTEAExcept57(declartion.US_EntryType);
					if (lineNumberRequired)
					{
						Parent.US_DRWImportEntryLineInfo.AddMessageError(LineNumberRequired);
					}
				}
			}
		}
		internal const string YouHaveNotEnteredEntryLineNumber = "You have not entered an import entry line.";
		internal const string LineNumberRequired = "Line Number is required for Drawback Provision is 51-56, 58-77.";
		internal const string LineNumberNotRequired = "Line Number is not required when you are filing Drawback unber 1313(D)";

		protected override void CheckUS_DRWIsForManufacturerSection()
		{
			base.CheckUS_DRWIsForManufacturerSection();

			if (Parent.US_DRWIsForImportSection && !Parent.US_DRWIsForManufacturerSection && ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWImpActInd))
			{
				Parent.US_DRWIsForManufacturerSectionInfo.AddMessageError(ManufacturedRequired);
			}

			ValidateUS_DRWImpManufRuleNo();
		}
		internal const string ManufacturedRequired = "Manufactured data is required when Import Action Indicator is 'X' or 'T'. Please enable 'Include on Manufactured Articles Section'.";

		protected override void CheckUS_DRWImpManufRuleNo()
		{
			base.CheckUS_DRWImpManufRuleNo();

			if (Parent.US_DRWIsForImportSection && ACEDrawbackActionCodeList.IsManufacturedAction(Parent.US_DRWImpActInd))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImpManufRuleNoInfo);
			}
		}

		protected override void CheckUS_DRWAccMethod()
		{
			base.CheckUS_DRWAccMethod();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWAccMethodInfo, Parent.Lookups.DrawbackAccountingCodes);

			var declaration = Parent.Declaration;
			if (declaration != null && Parent.US_DRWIsForImportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWAccMethodInfo);
			}
		}

		protected override void CheckUS_DRWMafActInd()
		{
			base.CheckUS_DRWMafActInd();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWMafActIndInfo, Parent.Lookups.ACEDrawbackActionList);

			if (Parent.US_DRWIsForManufacturerSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWMafActIndInfo);
			}
		}

		protected override void CheckUS_DRWImportQuantity2()
		{
			base.CheckUS_DRWImportQuantity2();

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportUQ2.IsEmpty && Parent.DRWImportQuantity2.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportQuantity2Info);
				}
			}

			ValidateUS_DRWImportUQ2();
			ValidateUS_DRWValuePerUQ2();
		}

		protected override void CheckUS_DRWImportUQ2()
		{
			base.CheckUS_DRWImportUQ2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWImportUQ2Info, Parent.Lookups.InvoiceUQList);

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportQuantity2.IsEmpty && Parent.DRWImportUQ2.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportUQ2Info);
				}
			}

			ValidateUS_DRWImportUQ2();
		}

		protected override void CheckUS_DRWValuePerUQ2()
		{
			base.CheckUS_DRWValuePerUQ2();

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportQuantity2.IsEmpty && Parent.DRWGoodsValuePerUQ2.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWValuePerUQ2Info);
				}
			}
		}

		protected override void CheckUS_DRWImportQuantity3()
		{
			base.CheckUS_DRWImportQuantity3();

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportUQ3.IsEmpty && Parent.DRWImportQuantity3.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportQuantity3Info);
				}
			}

			ValidateUS_DRWImportUQ3();
			ValidateUS_DRWValuePerUQ3();
		}

		protected override void CheckUS_DRWImportUQ3()
		{
			base.CheckUS_DRWImportUQ3();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWImportUQ3Info, Parent.Lookups.InvoiceUQList);

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportQuantity3.IsEmpty && Parent.DRWImportUQ3.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWImportUQ3Info);
				}
			}

			ValidateUS_DRWImportUQ3();
		}

		protected override void CheckUS_DRWValuePerUQ3()
		{
			base.CheckUS_DRWValuePerUQ3();

			if (Parent.US_DRWIsForImportSection)
			{
				if (!Parent.DRWImportQuantity3.IsEmpty && Parent.DRWGoodsValuePerUQ3.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWValuePerUQ3Info);
				}
			}
		}

		protected override void CheckUS_DRWSubstituted()
		{
			base.CheckUS_DRWSubstituted();

			if (Parent.US_DRWIsForImportSection)
			{
				if (IsSubstitutedValueRequired)
				{
					if (!Parent.DRWImportQuantity.IsEmpty && Parent.SubstitutedValuePerUnit.IsEmpty)
					{
						Parent.US_DRWSubstitutedInfo.AddMessageError(SubstitutedValueRequired);
					}
				}
				else if (!Parent.SubstitutedValuePerUnit.IsEmpty)
				{
					Parent.US_DRWSubstitutedInfo.AddMessageError(SubstitutedValueNotRequired);
				}
			}
		}
		internal const string SubstitutedValueRequired = "Substitute Value Per Unit must be provided when drawback provision is '52', '59', '66', '72', '73', '75', '76' or '77'.";
		internal const string SubstitutedValueNotRequired = "Substituted Value Per Unit is not allowed.";

		protected override void CheckUS_DRWSubstituted2()
		{
			base.CheckUS_DRWSubstituted2();

			if (Parent.US_DRWIsForImportSection)
			{
				if (IsSubstitutedValueRequired)
				{
					if (!Parent.DRWImportQuantity2.IsEmpty && Parent.SubstitutedValuePerUnit2.IsEmpty)
					{
						Parent.US_DRWSubstituted2Info.AddMessageError(SubstitutedValueRequired);
					}
				}
				else if (!Parent.SubstitutedValuePerUnit2.IsEmpty)
				{
					Parent.US_DRWSubstituted2Info.AddMessageError(SubstitutedValueNotRequired);
				}
			}
		}

		protected override void CheckUS_DRWSubstituted3()
		{
			base.CheckUS_DRWSubstituted3();

			if (Parent.US_DRWIsForImportSection)
			{
				if (IsSubstitutedValueRequired)
				{
					if (!Parent.DRWImportQuantity3.IsEmpty && Parent.SubstitutedValuePerUnit3.IsEmpty)
					{
						Parent.US_DRWSubstituted3Info.AddMessageError(SubstitutedValueRequired);
					}
				}
				else if (!Parent.SubstitutedValuePerUnit3.IsEmpty)
				{
					Parent.US_DRWSubstituted3Info.AddMessageError(SubstitutedValueNotRequired);
				}
			}
		}

		protected override void CheckUS_DRWCalcDuty()
		{
			base.CheckUS_DRWCalcDuty();
			InvoiceLine.AddInfoChildValidation.ValidateUSI_DRW99ClaimedDuty();
		}

		protected override void CheckUS_DRWCalcHMF()
		{
			base.CheckUS_DRWCalcHMF();
			InvoiceLine.AddInfoChildValidation.ValidateUSI_DRW99ClaimedHMF();
		}

		protected override void CheckUS_DRWCalcMPF()
		{
			base.CheckUS_DRWCalcMPF();
			InvoiceLine.AddInfoChildValidation.ValidateUSI_DRW99ClaimedMPF();
		}

		protected override void CheckUS_DRWCalcTax()
		{
			base.CheckUS_DRWCalcTax();
			InvoiceLine.AddInfoChildValidation.ValidateUSI_DRW99ClaimedTax();
		}

		#region Flags

		ZBool IsSubstitutedValueRequired
		{
			get
			{
				var declaration = Parent.Declaration;
				return declaration != null && ACEDrawbackProvisionsList.IsSubstitutedValueRequired(declaration.US_EntryType);
			}
		}

		#endregion
	}
}
