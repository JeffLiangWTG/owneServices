using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			var header = Parent.Header;
			if (header != null)
			{
				if (Parent.ABL_BolType == Core.Constants.ShipmentTypes.StandardHouse || Parent.ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster)
				{
					if (header.AMA_ManifestType == nameof(ManifestDocumentType.ALM) && Parent.ABL_BillNumber != header.AMA_MasterBill)
					{
						Parent.ABL_BillNumberInfo.AddMessageError(ValidationConstants.BillNumberRequiresTheSame);
					}
				}
			}
		}

		bool CheckExistingLRN(AsycudaBill bill)
		{
			if (bill != null)
			{
				var entryNumbers = bill.CustomsEntryNumbers;
				return entryNumbers.Count != 0
					&& entryNumbers.Cast<ABLEntryNum>().Any(entNum => (entNum.CE_EntryType == ZaLRNTypes.Codes.ABT || entNum.CE_EntryType == ZaLRNTypes.Codes.AFM) && !entNum.CE_EntryNum.IsEmpty);
			}
			return false;
		}

		protected override void CheckABL_ShipmentType()
		{
			var header = Parent.Header;
			if (header?.IsRoad ?? false)
			{
				ListValidation.MessageErrorIfInvalidCode(base.Parent.ABL_ShipmentTypeInfo);
			}
			else
			{
				base.CheckABL_ShipmentType();
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();
			if (Parent.Header != null && !Parent.ABL_BillNumber.IsEmpty && Parent.ABL_OA_Shipper.IsEmpty && (Parent.ABL_BolType == Core.Constants.ShipmentTypes.StandardHouse || Parent.ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ShipperInfo);
			}
		}

		protected override void CheckABL_UCRNumber()
		{
			base.CheckABL_UCRNumber();

			var header = Parent.Header;
			if (header != null && header.AMA_Nature == ShipmentTypeList.Codes.Export22)
			{
				var manifestType = header.AMA_ManifestType;
				if (manifestType == nameof(ManifestDocumentType.COM)
					|| manifestType == nameof(ManifestDocumentType.COH)
					|| manifestType == nameof(ManifestDocumentType.BBB)
					|| manifestType == nameof(ManifestDocumentType.FWB)
					|| manifestType == nameof(ManifestDocumentType.HAB)
					|| manifestType == nameof(ManifestDocumentType.RMA)
					|| manifestType == nameof(ManifestDocumentType.RFM)
					|| manifestType == nameof(ManifestDocumentType.ALM)
					|| manifestType == nameof(ManifestDocumentType.ALH))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_UCRNumberInfo);
				}
			}
		}

		protected override ZBool NeedsToCheckABL_Consignee => !Parent.ToOrderConsignment;
		protected override ZBool NeedsToCheckABL_ConsigneePostcode => !Parent.ToOrderConsignment;

		protected override void CheckCustomsEntryNumberType()
		{
			if (Parent.Header?.IsRoad ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CustomsEntryNumberTypeInfo);
			}
			base.CheckCustomsEntryNumberType();
		}

		protected override void CheckCustomsEntryNumber()
		{
			base.CheckCustomsEntryNumber();
			if (Parent.Header?.IsRoad ?? false)
			{
				if (!CheckExistingLRN(Parent) && Parent.CustomsEntryNumber.IsEmpty)
				{
					Parent.CustomsEntryNumberInfo.AddMessageError("At least one LRN Number must be captured per bill");
				}
			}
		}
	}
}
