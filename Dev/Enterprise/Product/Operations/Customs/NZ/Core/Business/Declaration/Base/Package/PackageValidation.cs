using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackageValidation : CusDecHouseContainerPackValidation
	{
		public PackageValidation(Package package)
			: base(package)
		{
		}

		protected new Package Package
		{
			get { return (Package)base.Package; }
		}

		protected override void CheckCW_PackType()
		{
			base.CheckCW_PackType();
			var isTSW = false;
			var packingGroup = Package.PackingGroup;
			if (packingGroup != null)
			{
				var declaration = (JobDeclaration)packingGroup.Declaration;
				{
					isTSW = declaration.IsTSWDeclaration;
				}
			}

			if (!isTSW || !Package.CW_PackType.IsEmpty || !IsAttachedToEmptyContainer)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Package.CW_PackTypeInfo, Package.PackTypeList);
				ValidateCW_PackQty();
			}
		}

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();
			if (Package.CW_PackQty == 0 && !IsAttachedToEmptyContainer)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Package.CW_PackQtyInfo);
			}

			if (Package.CW_PackQty != 1 && UniversalReferenceHelper.UNEPackageTypeIsBulk(Parent.Factory, Package.CW_PackType))
			{
				Package.CW_PackQtyInfo.AddMessageError(Package.CW_PackType + " is a bulk type and qty should be 1.");
			}
		}

		bool IsAttachedToEmptyContainer
		{
			get
			{
				var group = Package.PackingGroup;
				if (group != null)
				{
					CusContainer container = group.Container;
					if (container != null && container.CO_FCL_LCL_AIR == ContainerModeList.Codes.Empty)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override void CheckCW_HouseBill()
		{
			base.CheckCW_HouseBill();

			var packingGroup = Package.PackingGroup;
			if (packingGroup != null)
			{
				var bill = packingGroup.Bill;
				if (bill != null)
				{
					var declaration = (JobDeclaration)packingGroup.Declaration;
					if (declaration.IsTSWDeclaration)
					{
						if (HouseBillsHaveBeenEntered && !bill.IsHouseBill)
						{
							Package.CW_HouseBillInfo.AddError(MessageErrorLinkPackLinesToHouseBill);
						}
					}
					else if (!bill.IsHouseBill)
					{
						Package.CW_HouseBillInfo.AddError(MessageErrorPackLinesMustBeLinkedToHouseBill);
					}
				}
			}
		}

		internal const string MessageErrorPackLinesMustBeLinkedToHouseBill = "Packing lines must be linked to a House Bill.";
		internal const string MessageErrorLinkPackLinesToHouseBill = "If House Bill(s) have been entered, the packing lines must be linked to a House Bill.";

		bool HouseBillsHaveBeenEntered
		{
			get
			{
				var packingGroup = Package.PackingGroup;
				return !packingGroup.Declaration.JE_HouseBill.IsEmpty;
			}
		}
	}
}
