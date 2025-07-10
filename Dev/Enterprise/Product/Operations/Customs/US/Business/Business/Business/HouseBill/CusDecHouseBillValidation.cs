using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusDecHouseBillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public CusDecHouseBillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		public JobDeclaration Declaration
		{
			get { return Bill.Declaration; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}

		protected override bool NeedToValidateHouseBillAndPackages
		{
			get { return false; }
		}

		protected override void CheckCU_BillType()
		{
			base.CheckCU_BillType();

			var declaration = Declaration;
			if (declaration != null && declaration.IsACE && Bill != null)
			{
				if (Bill.IsSubHouseBill)
				{
					Bill.CU_BillTypeInfo.AddMessageError(SubHouseIsNotAllowedForACE);
				}
				else
				{
					var billValidator = (BillValidator)GetBillValidator();
					billValidator.CheckNumberOfBillsForLowValueEntries(declaration, Bill.CU_BillTypeInfo, Bill.CU_BillType);
				}
			}
		}
		internal const string SubHouseIsNotAllowedForACE = "Bill Type Sub House is not allowed in ACE at this time.";

		protected override void CheckCU_BillNum()
		{
			var declaration = Declaration;
			if (declaration != null && declaration.IsImport)
			{
				var billValidator = (BillValidator)GetBillValidator();

				var isMasterBillForFTZAdmission = declaration.IsFTZAdmission && Bill.IsMasterBill;
				var billLength = isMasterBillForFTZAdmission ? BillValidator.Constants.MaximumFTZBillLength : BillValidator.Constants.MaximumBillLength;

				billValidator.CheckInvalidLength(Bill.CU_BillNumInfo, Bill.CU_BillTypeDescription, billLength);
				billValidator.CheckInvalidCharacters(Bill.CU_BillNumInfo, Bill.CU_BillTypeDescription);
				if (declaration.IsFTZAdmission)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Bill.CU_BillNumInfo, "Bill Num");

					if (IsFTZAdmissionValidationMode && Bill.IsFTZLowestBill && !declaration.IsTemporaryDeposit)
					{
						if (!Bill.HasLinkedInvoiceLines)
						{
							Bill.CU_BillNumInfo.AddMessageError(LinesMissing);
						}
					}
				}
				else
				{
					billValidator.CheckMandatory(IsHouseBillMandatoryForSCAC, Bill.CU_BillNumInfo, "House Bill");
				}
				base.CheckCU_BillNum();
			}
		}
		internal const string LinesMissing = "At least one invoice line is required for a bill when sending FT transmissions.";

		bool IsHouseBillMandatoryForSCAC
		{
			get { return !Declaration.IsConsumptionFTZ && Bill.IsHouseBill && !Bill.US_UI_NKBillIssuerSCAC.IsEmpty; }
		}

		protected override void CheckCU_ParentBillUniqueCode()
		{
			base.CheckCU_ParentBillUniqueCode();

			if (Bill.IsSubHouseBill)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Bill.CU_ParentBillUniqueCodeInfo);
			}
		}

		protected override void CheckCU_NoOfPacks()
		{
			base.CheckCU_NoOfPacks();

			if (IsManifestQuantityMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CU_NoOfPacksInfo);
			}

			if ((!Declaration.IsHandCarry && !Declaration.IsACEAutoRoadAndPedTransportMode) || !Declaration.IsACECargoCertificationMode)
			{
				int totalNoOfPacks = Bill.ITAndSplitDetails.TotalNoOfPacks;
				if (totalNoOfPacks > 0 && Bill.CU_NoOfPacks > 0 && totalNoOfPacks != Bill.CU_NoOfPacks)
				{
					Bill.CU_NoOfPacksInfo.AddWarning(string.Format(NoOfPacksNotEqualITNumbersNoOfPacks, totalNoOfPacks));
				}

				var declaration = Declaration;
				if (IsACECargoReleaseValidationMode && !IsEntrySummaryValidationMode && Bill.CU_NoOfPacks > 0 && Bill.IsBillQtySentAsManifestQtyInACE3461 && !Bill.US_SESplitShip && !declaration.US_NonAMS && declaration.US_EntryType != EntryTypeList.Codes.LowValue)
				{
					Bill.CU_NoOfPacksInfo.AddWarning(ACECargoReleaseQTY);
				}

				if (Bill.CU_NoOfPacks > int.MaxValue)
				{
					Bill.CU_NoOfPacksInfo.AddMessageError(NoOfPacksValueIsInvalid);
				}
			}

			ValidateCU_PackType();
		}
		internal const string ManifestQuantityIsMandatory = "You should enter a manifest quantity. You can enter here or at children bills(House bill or Sub-HouseBill).";
		internal const string NoOfPacksNotEqualITNumbersNoOfPacks = "The sum of the Manifest Qty for all IT Numbers({0}) does not equal the Bill Manifest Qty.";
		internal const string ACECargoReleaseQTY = "CBP will collect this information from AMS. You can leave this quantity as zero.";
		internal const string NoOfPacksValueIsInvalid = "This value exceeds the limit Customs has set. System will have to send zero for manifest quantity.";
		internal const string TotalInvoiceQuantityDoesNotAddUpToManifestQty = "Total most-inner pack quantity entered in invoice lines do not add up to total quantity of this bill.";

		protected override void CheckCU_PackType()
		{
			base.CheckCU_PackType();

			if (Declaration.IsImport || Declaration.IsImportByExternalBroker)
			{
				if (Parent.CU_NoOfPacks > 0 || Parent.ITAndSplitDetails.OfType<ITAndSplitDetails>().Any(x => x.US_NoOfPacks > 0))
				{
					if (!Declaration.IsHandCarry && !Declaration.IsACEAutoRoadAndPedTransportMode)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.CU_PackTypeInfo);
					}
				}
			}
		}

		protected override void CheckCU_PackTypeIsAValidCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.CU_PackTypeInfo, Parent.Lookups.NoOfPacksPackType_List, ValidationConstants.InvalidPackTypeMessage);
		}

		#region ValidateITNumber

		public void ValidateITNumber()
		{
			ValidateCalculatedProperty(Bill.ITNumberInfo);
		}

		protected void CheckITNumber()
		{
			if (Parent.ITNumber != Bill.Constants.Multiple)
			{
				ITNumberValidator.ValidateITNumber(Parent.ITNumberInfo, Bill);
				if (Parent.Declaration != null)
				{
					Parent.Declaration.AddInfoValidation.ValidateUS_SchDEntry();
				}
			}
		}

		#endregion
		#region Related Objects

		protected override Customs.Business.BillValidator GetBillValidator()
		{
			return new BillValidator();
		}

		#endregion

		#region Boolean Flags

		bool IsAZAdmissionType
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && (declaration.IsRegularFTZAdmission || declaration.US_F_AdmissionType == FTZAdmissionTypeCodeList.Codes.ZoneToZone);
			}
		}

		internal bool IsCargoReleaseValidationMode
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && declaration.IsCargoReleaseValidationMode;
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && declaration.IsACECargoReleaseValidationMode;
			}
		}

		internal bool IsEntrySummaryValidationMode
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && declaration.IsEntrySummaryValidationMode;
			}
		}

		internal bool IsManifestQuantityMandatory
		{
			get
			{
				if (IsExWarehouse)
				{
					return false;
				}
				else
				{
					var declaration = Bill.Declaration;
					var isACECargoRelease = declaration != null && declaration.IsACECargoRelease;
					var isNonAMS = declaration != null && declaration.US_NonAMS;
					var isLowValue = declaration != null && declaration.IsLowValue;

					if (IsEntrySummaryValidationMode && Bill.NoITNumbersExist
						|| IsCargoReleaseValidationMode && !isACECargoRelease
						|| IsFTZAdmissionValidationMode && (IsAZAdmissionType || HasInbondMovement)
						|| isACECargoRelease && (isNonAMS || isLowValue))
					{
						return Parent.IsLowestBill;
					}
				}

				return false;
			}
		}

		ZBool IsFTZAdmissionValidationMode
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && declaration.IsFTZAdmissionValidationMode;
			}
		}

		bool HasInbondMovement
		{
			get { return !Bill.NoITNumbersExist; }
		}

		bool IsExWarehouse
		{
			get
			{
				var declaration = Bill.Declaration;
				return declaration != null && declaration.IsExWarehouse;
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateITNumber();
		}
	}
}
