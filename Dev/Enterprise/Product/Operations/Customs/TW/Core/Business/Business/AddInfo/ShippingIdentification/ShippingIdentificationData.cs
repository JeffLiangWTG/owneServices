using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.TW.Business
{
	public class ShippingIdentificationData : AutoSIData
	{
		public ShippingIdentificationData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new ShippingIdentificationDataValidation(this);
		}

		public new ShippingIdentificationDataValidation Validation => (ShippingIdentificationDataValidation)base.Validation;

		ZBool TW_ExpirationDateReadOnly => Factory.GetValue(ref expirationDateReadOnlyCached, () => !Parent.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603));
		CachedProperty<ZBool> expirationDateReadOnlyCached;

		[ReadOnlyMember(nameof(TW_ExpirationDateReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ShippingIdentificationData|TW_ExpirationDate", Caption = "Expiration Date", FullDescription = "The expiry date of the commodity.")]
		public override ZDateTime TW_ExpirationDate { get => base.TW_ExpirationDate; set => base.TW_ExpirationDate = value; }

		ZBool TW_ManufacturedDateReadOnly => Parent.ShippingIdentificationDataCollectionReadOnly;

		[ReadOnlyMember(nameof(TW_ManufacturedDateReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ShippingIdentificationData|TW_ManufacturedDate", Caption = "Manufactured Date", FullDescription = "The manufacture date of the commodity applying for inspection.")]
		public override ZDateTime TW_ManufacturedDate { get => base.TW_ManufacturedDate; set => base.TW_ManufacturedDate = value; }

		ZBool TW_ManufacturedLotNoReadOnly => Parent.ShippingIdentificationDataCollectionReadOnly;

		[ReadOnlyMember(nameof(TW_ManufacturedLotNoReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ShippingIdentificationData|TW_ManufacturedLotNo", Caption = "Lot Number", FullDescription = "The manufacturing lot number of the commodity applying for inspection.")]
		public override ZString TW_ManufacturedLotNo { get => base.TW_ManufacturedLotNo; set => base.TW_ManufacturedLotNo = value; }

		ZBool TW_ProductLotNoAmountReadOnly => !Parent.IsForCMHeaderMessageTypeNX301_DN;

		[ReadOnlyMember(nameof(TW_ProductLotNoAmountReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ShippingIdentificationData|TW_ProductLotNoAmount", Caption = "Volume (LTR)", FullDescription = "The volume of the goods with a manufacturing lot number.")]
		public override ZDecimal TW_ProductLotNoAmount { get => base.TW_ProductLotNoAmount; set => base.TW_ProductLotNoAmount = value; }
	}
}
