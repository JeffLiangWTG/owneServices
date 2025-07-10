using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondBillValidation : CommonCusInBondBillValidation
	{
		public CusInBondBillValidation(CusInBondBill parent)
			: base(parent)
		{
			isAMSHBREffective = ZZCustomsFunctionality.IsAMSHBREffective;
		}

		readonly bool isAMSHBREffective;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateOceanBillOfLadingIsEnteredForNVOCC();
				ValidateSecondaryNotifyParties();
			}
		}

		protected override void CheckB0_ForeignPortOfUnladingKCode()
		{
			if (IsForeignPortOfUnladingKCodeRequired && !Parent.IsOceanBillType)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_ForeignPortOfUnladingKCodeInfo);
			}
		}

		bool IsForeignPortOfUnladingKCodeRequired
		{
			get
			{
				return IsInventoryRecordValidationMode &&
					(
						Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF ||
						Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF ||
						Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond ||
						Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond
					);
			}
		}

		protected override void CheckB0_PlaceOfDelivery()
		{
			if (IsPlaceOfDeliveryRequired && !Parent.IsOceanBillType)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_PlaceOfDeliveryInfo);
			}
		}

		bool IsPlaceOfDeliveryRequired
		{
			get { return IsInventoryRecordValidationMode && BillOfLadingStatusIndicatorList.IsISF(Parent.B0_BillStatus, isAMSHBREffective); }
		}

		void ValidateSecondaryNotifyParties()
		{
			if (IsInventoryRecordValidationMode)
			{
				if (Parent.SecondaryNotifyParties.Count == 0)
				{
					if (Parent.IsNVOCCBill)
					{
						Parent.AddRowMessageError(ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
					}
				}
				else if (Parent.SecondaryNotifyParties.Count(x => !x.CY_Data.IsEmpty) > 8)
				{
					Parent.AddRowMessageError(ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
				}
			}
		}

		void ValidateOceanBillOfLadingIsEnteredForNVOCC()
		{
			if (!Parent.IsNVOCCHeader && IsInventoryRecordValidationMode && Parent.IsNVOCCBill && Parent.ShipmentReferenceDetails[BillReferenceList.Codes.OB] == null)
			{
				Parent.AddRowMessageError(ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			}
		}

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();

			if (Parent.IsInDatabase)
			{
				var originalBillOfLading = (ZString)Parent.B0_MasterBillNumberInfo.OriginalValue;
				var billOfLading = Parent.B0_MasterBillNumber;
				if (originalBillOfLading != billOfLading)
				{
					if (Parent.IsBillAlreadyOnFile)
					{
						Parent.B0_MasterBillNumberInfo.AddError(ValidationConstants.Bill.ChangingBOLNumberWhenBillIsOnFile(originalBillOfLading, billOfLading));
					}
					else if (Parent.IsMessagingInProgress)
					{
						Parent.B0_MasterBillNumberInfo.AddError(ValidationConstants.Bill.ChangingBOLNumberWhenMessagingIsInProgress(originalBillOfLading, billOfLading));
					}
				}
			}

			if (Parent.HasSailingLinkage && ((ISailingSynchronisationTarget<BillOfLading>)Parent).Source == null)
			{
				Parent.B0_MasterBillNumberInfo.AddWarning(ValidationConstants.SailingSynchronisation.BillMightBeIncorectlyAdded);
			}

			var parentHeader = Parent.Header;
			if (parentHeader != null && !parentHeader.HasHVLVParent)
			{
				var headerWithDuplicatedBillNumber = GetHeaderWithDuplicatedBillNumber(parentHeader);
				if (headerWithDuplicatedBillNumber != null)
				{
					Parent.B0_MasterBillNumberInfo.AddMessageError(
						ValidationConstants.Bill.BillOfLadingNumberIsDuplicated(parentHeader, headerWithDuplicatedBillNumber.BH_JobReference,
						headerWithDuplicatedBillNumber.Company?.GC_Name ?? ZString.Empty, headerWithDuplicatedBillNumber.Branch?.GB_BranchName ?? ZString.Empty));
				}
			}
		}

		CusInBondHeader GetHeaderWithDuplicatedBillNumber(CusInBondHeader parentHeader)
		{
			CusInBondHeader result = null;
			var masterBillNumber = Parent.B0_MasterBillNumber;
			if (!masterBillNumber.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
				query.AddToFilter(CusInBondHeaderSchema.BH_IsActive, 1);
				query.AddToFilter(CusInBondHeaderSchema.BH_TransitDirection, parentHeader.BH_TransitDirection);
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.AMS);
				query.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, parentHeader.PK);

				var cusInBondBillSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
				cusInBondBillSubQuery.AddToFilter(CusInBondBillSchema.B0_IssuerCode, Parent.B0_IssuerCode);
				cusInBondBillSubQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, masterBillNumber);
				cusInBondBillSubQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, SQLComparisonOperator.NotEqual, CusInBondBill.OceanBillType);
				query.AddSubQuery(cusInBondBillSubQuery, JoinCondition.And);

				if (parentHeader.IsNVOCCHeader)
				{
					var oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
					oceanBillSubQuery.AddToFilter(CusInBondBillSchema.B0_IssuerCode, parentHeader.OceanBill.B0_IssuerCode);
					oceanBillSubQuery.AddToFilter(CusInBondBillSchema.B0_MasterBillNumber, parentHeader.OceanBill.B0_MasterBillNumber);
					oceanBillSubQuery.AddToFilter(CusInBondBillSchema.B0_ShipmentType, parentHeader.OceanBill.B0_ShipmentType);
					query.AddSubQuery(oceanBillSubQuery, JoinCondition.And);
				}
				result = Parent.Factory.LoadTop1<CusInBondHeader>(query);
			}
			return result;
		}

		protected override void CheckB0_IssuerCode()
		{
			base.CheckB0_IssuerCode();

			if (Parent.IsInDatabase)
			{
				var originalIssuerCode = (ZString)Parent.B0_IssuerCodeInfo.OriginalValue;
				var issuerCode = Parent.B0_IssuerCode;
				if (originalIssuerCode != issuerCode)
				{
					if (Parent.IsBillAlreadyOnFile)
					{
						Parent.B0_IssuerCodeInfo.AddError(ValidationConstants.Bill.ChangingIssuerCodeWhenBillIsOnFile(originalIssuerCode, issuerCode));
					}
					else if (Parent.IsMessagingInProgress)
					{
						Parent.B0_IssuerCodeInfo.AddError(ValidationConstants.Bill.ChangingIssuerCodeWhenMessagingIsInProgress(originalIssuerCode, issuerCode));
					}
				}
			}

			var header = Parent.Header;
			if (header != null && header.IsNVOCCHeader && header.BH_CarrierSCAC != Parent.B0_IssuerCode)
			{
				Parent.B0_IssuerCodeInfo.AddWarning(ValidationConstants.Bill.ShouldBeIdenticalWithSCACCode);
			}

			ValidateB0_MasterBillNumber();
		}

		protected override void CheckB0_PortOfLadingKCode()
		{
			base.CheckB0_PortOfLadingKCode();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_PortOfLadingKCodeInfo);
			}
		}

		protected override void CheckB0_RL_NKPortOfLading()
		{
			base.CheckB0_RL_NKPortOfLading();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.WarnIfInvalidCode(Parent.B0_RL_NKPortOfLadingInfo);
			}
		}

		protected override void CheckB0_ManifestQty()
		{
			base.CheckB0_ManifestQty();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_ManifestQtyInfo);
				MandatoryValidation.MessageErrorIfIsZero(Parent.B0_ManifestQtyInfo);
				var moveDetail = Parent.MovementDetail;
				if (moveDetail != null && Parent.B0_ManifestQty != moveDetail.Containers.TotalManifestQty)
				{
					Parent.B0_ManifestQtyInfo.AddMessageError(ValidationConstants.Bill.TotalManifestQtyNotEqualSumOfPieceCount);
				}
			}
		}

		protected override void CheckB0_ManifestUQ()
		{
			base.CheckB0_ManifestUQ();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_ManifestUQInfo);
			}
		}

		protected override void CheckB0_Weight()
		{
			base.CheckB0_Weight();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_WeightInfo);
				MandatoryValidation.MessageErrorIfIsZero(Parent.B0_WeightInfo);
			}
		}

		protected override void CheckB0_WeightUQ()
		{
			base.CheckB0_WeightUQ();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_WeightUQInfo);
			}
		}

		protected override void CheckB0_BillStatus()
		{
			base.CheckB0_BillStatus();
			if (IsInventoryRecordValidationMode || IsPTTValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_BillStatusInfo);
			}

			if (IsPTTValidationMode && !Parent.B0_BillStatus.IsEmpty && !BillOfLadingStatusIndicatorList.IsEligibleForPTT(Parent.B0_BillStatus))
			{
				Parent.B0_BillStatusInfo.AddMessageError(ValidationConstants.Bill.BillStatusNotForPTT);
			}

			if (Parent.B0_BillStatus == BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF && IsInventoryRecordValidationMode && Parent.MasterInBondMovement == null)
			{
				Parent.B0_BillStatusInfo.AddMessageError(ValidationConstants.Bill.MasterInBondIsRequiredWhenInBondType62_63WithISF);
			}

			ValidateB0_Firms();
			ValidateB0_ForeignPortOfUnladingKCode();
			ValidateB0_PlaceOfDelivery();
		}

		protected override void CheckB0_MasterInBondIndicator()
		{
			base.CheckB0_MasterInBondIndicator();
			if (Parent.B0_MasterInBondIndicator && IsInventoryRecordValidationMode && Parent.MasterInBondMovement == null)
			{
				Parent.B0_MasterInBondIndicatorInfo.AddMessageError(ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue);
			}
			ValidateB0_InBondPortOfDestDCode();
		}

		protected override void CheckB0_Volume()
		{
			base.CheckB0_Volume();
			if (IsInventoryRecordValidationMode)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.B0_VolumeInfo);
			}
			ValidateB0_VolumeUQ();
		}

		protected override void CheckB0_VolumeUQ()
		{
			base.CheckB0_VolumeUQ();
			if (IsInventoryRecordValidationMode)
			{
				if (!Parent.B0_Volume.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_VolumeUQInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_VolumeUQInfo);
			}
		}

		protected override void CheckB0_PlaceOfReceipt()
		{
			base.CheckB0_PlaceOfReceipt();
			if (IsInventoryRecordValidationMode && !Parent.IsOceanBillType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_PlaceOfReceiptInfo);
			}
		}

		protected override void CheckB0_LastForeignPortKCode()
		{
			base.CheckB0_LastForeignPortKCode();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B0_LastForeignPortKCodeInfo);
			}
		}

		protected override void CheckB0_RL_NKLastForeignPort()
		{
			base.CheckB0_RL_NKLastForeignPort();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.WarnIfInvalidCode(Parent.B0_RL_NKLastForeignPortInfo);
			}
		}

		protected override void CheckB0_TransportPaymentMethod()
		{
			base.CheckB0_TransportPaymentMethod();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_TransportPaymentMethodInfo);
			}
		}

		protected override void CheckB0_TransportModeToPortOfLading()
		{
			base.CheckB0_TransportModeToPortOfLading();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_TransportModeToPortOfLadingInfo);
			}
		}

		protected override void CheckB0_ForeignPortOfContractKCode()
		{
			base.CheckB0_ForeignPortOfContractKCode();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.B0_ForeignPortOfContractKCodeInfo);
			}
		}

		protected override void CheckB0_RL_NKForeignPortOfContract()
		{
			base.CheckB0_RL_NKForeignPortOfContract();
			if (IsInventoryRecordValidationMode)
			{
				ListValidation.WarnIfInvalidCode(Parent.B0_RL_NKForeignPortOfContractInfo);
			}
		}

		protected override void CheckB0_InBondPortOfDestDCode()
		{
			base.CheckB0_InBondPortOfDestDCode();
			ValidateB0_DateOfDischarge();
		}

		protected override void CheckB0_DateOfDischarge()
		{
			base.CheckB0_DateOfDischarge();
			if (!Parent.B0_InBondPortOfDestDCode.IsEmpty)
			{
				var header = Parent.Header;
				if (header != null && header.Bills.Any(p => p != Parent && p.B0_InBondPortOfDestDCode == Parent.B0_InBondPortOfDestDCode
					&& p.B0_DateOfDischarge != Parent.B0_DateOfDischarge))
				{
					Parent.B0_DateOfDischargeInfo.AddError(ValidationConstants.Bill.SamePortHasSameEstUnloadingDate(Parent.B0_InBondPortOfDestDCode));
				}
			}
		}
	}
}
