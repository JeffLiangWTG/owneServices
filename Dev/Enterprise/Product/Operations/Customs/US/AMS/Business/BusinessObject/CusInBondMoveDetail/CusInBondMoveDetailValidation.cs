using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveDetailValidation : Customs.Business.CusInBondMoveDetailValidation
	{
		public CusInBondMoveDetailValidation(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			if (IsInventoryRecordValidationMode)
			{
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					ValidateAtLeastOneContainerExist();
				}
			}
		}

		void ValidateAtLeastOneContainerExist()
		{
			if (IsInventoryRecordValidationMode && Parent.Containers.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			}
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		protected override void CheckB9_InBoundQty()
		{
			base.CheckB9_InBoundQty();

			if (Parent.IsPermitToTransferValidationMode && IsB9_InBoundQtyRequired && !Parent.B9_InBoundQty.IsEmpty)
			{
				var bill = Parent.Bill;
				if (bill != null && Parent.B9_InBoundQty > bill.B0_ManifestQty)
				{
					Parent.B9_InBoundQtyInfo.AddMessageError(ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty);
				}
			}
		}

		bool IsB9_InBoundQtyRequired
		{
			get
			{
				var moveHeader = Parent.MoveHeader;
				return moveHeader != null && (!moveHeader.IsNVOCCHeader || !moveHeader.BM_InBondCarrierID.IsEmpty);
			}
		}

		protected override void CheckB9_B0()
		{
			base.CheckB9_B0();
			if (Parent.IsInBondMovement)
			{
				if (!Parent.IsSubsequentInBondMovement)
				{
					CheckOneMoveDetailPerBillInBondMovement();
				}

				if (!Parent.B9_BMInfo.HasNotifications())
				{
					var bill = Parent.Bill;
					if (bill != null && (bill.IsInventoryRecordValidationMode || bill.IsSubsequentInBondValidationMode))
					{
						var invalidCodes = new List<ZString>();
						var inbondList = BillReferenceList.GetCachedACEM1InBondList(Factory);
						foreach (var reference in bill.ShipmentReferenceDetails)
						{
							if (!reference.BR_Qualifier.IsEmpty && !inbondList.ContainsCode(reference.BR_Qualifier) && !invalidCodes.Contains(reference.BR_Qualifier))
							{
								invalidCodes.Add(reference.BR_Qualifier);
							}
						}
						if (invalidCodes.Count > 0)
						{
							invalidCodes.Sort();
							var messageBuilder = new ZStringBuilder(invalidCodes);
							Parent.B9_B0Info.AddWarning(ValidationConstants.MoveDetail.InvalidReferencesForInBond(messageBuilder.ToStringWithDelimiterBetweenAppends(", ")));
						}
					}
				}
			}
		}

		void CheckOneMoveDetailPerBillInBondMovement()
		{
			var bill = Parent.Bill;
			if (bill != null)
			{
				var header = bill.Header;
				if (header != null)
				{
					var moveHeaderPK = Parent.B9_BM;
					var billPK = bill.PK;
					if (header.InBondMovementHeaders.
						Cast<CusInBondMoveHeader>().
						FirstOrDefault((x) => x.PK != moveHeaderPK &&
							x.BM_SubApplicationCode == SubApplicationCodeList.Codes.MasterInBond &&
							x.MovementDetails.
								FirstOrDefault(y => y.B9_B0 == billPK) != null) != null)
					{
						Parent.B9_B0Info.AddError(ValidationConstants.MoveDetail.BillOfLadingShouldHaveOneMasterInBondOnly);
					}
				}
			}
		}

		protected override void CheckB9_ForeignDestPortKCode()
		{
			base.CheckB9_ForeignDestPortKCode();
			if ((IsInBondExportationValidationMode || IsSubsequentInBondValidationMode) && Parent.IsInBondMovement)
			{
				var bill = Parent.Bill;
				if (bill != null && !bill.IsOceanBillType)
				{
					if (Parent.IsTransportandExportEntryType || Parent.IsImmediateExportEntryType)
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B9_ForeignDestPortKCodeInfo);
					}
					else if (!Parent.B9_ForeignDestPortKCode.IsEmpty)
					{
						Parent.B9_ForeignDestPortKCodeInfo.AddMessageError(ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType);
					}
				}
			}
		}

		protected override void CheckB9_MonetaryValue()
		{
			base.CheckB9_MonetaryValue();
			if (IsSubsequentInBondValidationMode && Parent.B9_MonetaryValue <= 0m)
			{
				Parent.B9_MonetaryValueInfo.AddMessageError(ValidationConstants.MoveDetail.MonetaryValueIsRequired);
			}
		}

		protected override void CheckB9_ExportDate()
		{
			base.CheckB9_ExportDate();
			if (IsInBondExportationValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B9_ExportDateInfo);
			}
		}

		protected override void CheckB9_ExportLadenOn()
		{
			base.CheckB9_ExportLadenOn();
			if (IsInBondExportationValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B9_ExportLadenOnInfo);
			}
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}

		bool IsInBondExportationValidationMode
		{
			get { return Parent.IsInBondExportationValidationMode; }
		}

		bool IsSubsequentInBondValidationMode
		{
			get { return Parent.IsSubsequentInBondValidationMode; }
		}

		bool IsInventoryRecordValidationMode
		{
			get
			{
				var result = false;
				if (Parent.IsAMSMovement)
				{
					var bill = Parent.Bill;
					result = bill != null && bill.IsInventoryRecordValidationMode;
				}
				return result;
			}
		}
	}
}
