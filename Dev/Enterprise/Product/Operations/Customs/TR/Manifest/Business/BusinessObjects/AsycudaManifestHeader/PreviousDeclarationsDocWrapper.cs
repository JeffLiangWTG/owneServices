using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class PreviousDeclarationsDocWrapper : DocumentWrapper
	{
		public PreviousDeclarationsDocWrapper(AsycudaManifestHeaderDocWrapper headerDoc, AsycudaBill bill, ZString previousNumber)
		{
			this.bill = bill;
			this.previousNumber = previousNumber;
		}
		readonly AsycudaBill bill;
		readonly ZString previousNumber;

		public ZString ABL_SequenceNumber => bill.ABL_SequenceNumber.ToString();
		public ZString ABL_BillNumber => bill.ABL_BillNumber;
		public ZString ABL_ShipperName => bill.ABL_ShipperName;
		public ZString ABL_ConsigneeName => bill.ABL_ConsigneeName;
		public ZString ABL_NotifyPartyName => bill.ABL_NotifyPartyName;
		public ZString CompanyNameOfAgent => bill.ContainerAgent?.CompanyName ?? ZString.Empty;
		public ZString ABL_RL_NKOrigin => bill.ABL_RL_NKOrigin;
		public ZString AMA_CustomsDischargePort => bill.Header.AMA_CustomsDischargePort;
		public ZString ContainerInformation => bill.Packs.Cast<AsycudaPack>().Any(x => x.Container != null) ? Res.GetString("06FA10B8-49D2-43EE-88FF-86867B912662", "E") : Res.GetString("4ED5D416-254E-491E-8075-BAD9B976B131", "H");
		public ZString CustomsEntryNumber => previousNumber.IsEmpty ? bill.CustomsEntryNumber : previousNumber;
		public ZString IsTransshipment => bill.TransshipmentType == TransshipmentTypeList.Codes.TT4 ? Res.GetString("C953FB27-2F98-45BF-9667-055756D7625B", "E") : Res.GetString("20BACD21-B626-4079-A346-2DBEB2E8162E", "H");
		public AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => bill.Packs;
	}
}
