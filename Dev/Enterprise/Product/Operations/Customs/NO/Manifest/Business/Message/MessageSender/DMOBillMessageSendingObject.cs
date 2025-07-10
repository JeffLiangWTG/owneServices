using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class DMOBillMessageSendingObject : DMOMessageSendingObject
{
	public DMOBillMessageSendingObject(AsycudaBill bill) : base(bill?.Factory)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
	}

	public AsycudaBill Bill { get; }

	public override ZString CustomsLevel => GetCustomsLevel();

	public override ZString BillNumber => Bill.ABL_BillNumber;

	public override ZString Representative => Bill.Forwarder?.CompanyName ?? ZString.Empty;

	public override ZString Consignee => Bill.Consignee?.CompanyName ?? ZString.Empty;

	string GetCustomsLevel()
	{
		return Bill switch
		{
			{ IsMasterBill: true } => NODMOEDIMessageTypeList.Descriptions.MCS,
			{ IsHouseBill: true } => NODMOEDIMessageTypeList.Descriptions.HCS,
			_ => string.Empty
		};
	}
}
