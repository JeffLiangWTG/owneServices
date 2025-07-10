using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeDischargeListMessageProvider : IDischargeList
	{
		public ETradeDischargeListMessageProvider(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;
		BusinessObject IMessageSender.Parent => Header;
		IBusinessObjectCollection IMessageSender.Messages => Header.Messages;
		ZString IMessageSender.JobReference => Header.AMA_JobReference;
		ZString IDischargeList.CustomsOffice => TRMessageHelper.RemoveCountryCodePrefix(Header.AMA_CustomsOffice);
		ZString IDischargeList.RegistrationNo => Header.RegistrationNumber;
		IEnumerable<IBillBL> IDischargeList.Bills => SetBills(Header);
		ZString IDischargeList.DeclarationOwnerRepresentativeNameAndTitle => GlbCompany.CurrentCompany.GC_Name;
		ZString IDischargeList.DeclarationOwnerRepresentativeTaxNo => GlbCompany.CurrentCompany.GC_BusinessRegNo;
		ZString IDischargeList.GoodsLocationName => Header.MasterBill.ABL_LocationInformation;
		ZString IDischargeList.GoodsLocationCode => Header.MasterBill.ABL_GoodsLocation;
		IEnumerable<IBillBL> SetBills(AsycudaManifestHeader header)
		{
			foreach (var item in header.Bills.Where(x => !x.Separated))
			{
				yield return new BillBLProvider(item);
			}
		}
	}

	public class BillBLProvider : IBillBL
	{
		public BillBLProvider(AsycudaBill bill)
		{
			Bill = Argument.NotNull(bill, nameof(bill));
		}
		protected readonly AsycudaBill Bill;

		ZString IBillBL.SequenceNo => Bill.ABL_SequenceNumber.ToString();
		ZString IBillBL.BillNo => Bill.ABL_BillNumber;
		ZBool IBillBL.IsContainer => Bill.Header.IsContainerized;
		ZString IBillBL.LineNo => Bill.ABL_SequenceNumber.ToString();
		ZString IBillBL.PackType => Bill.ABL_ManifestUQ;
		ZInt IBillBL.PackQuantity => Bill.ABL_ManifestQty;
		ZString IBillBL.MarksAndNumbers => Bill.ABL_MarksAndNumbers;
		ZString IBillBL.Unit => BillWeightUnit;
		IEnumerable<IPackBL> IBillBL.Packs => GetPacks(Bill);
		ZString IBillBL.ShipperName => Bill.ABL_ShipperName;
		ZString IBillBL.ConsigneeNameAndTitle => Bill.ABL_ConsigneeName;
		ZString IBillBL.ConsigneeTaxNo => Bill.ABL_ConsigneeRegNo;
		ZDecimal IBillBL.GrossWeight => Bill.GrossWeightInKG;
		ZDecimal IBillBL.NetWeight => Core.Constants.Weight.ConvertSafe(Bill.ABL_NetWeight, Bill.ABL_NetWeightUQ, Core.Constants.Weight.Kilograms);

		IEnumerable<IPackBL> GetPacks(AsycudaBill bill)
		{
			foreach (var item in bill.Packs)
			{
				yield return new PackBLProvider((AsycudaPack)item);
			}
		}
		const string BillWeightUnit = "KGM";
	}

	public class PackBLProvider : IPackBL
	{
		public PackBLProvider(AsycudaPack pack)
		{
			Pack = Argument.NotNull(pack, nameof(pack));
		}
		protected readonly AsycudaPack Pack;

		ZString IPackBL.LineNo => Pack.APA_LineNo.ToString();
		ZString IPackBL.GoodDescription => Pack.PackedItem.API_GoodsDescription;
		ZString IPackBL.Tariff => Pack.PackedItem.API_Tariff;
		ZString IPackBL.Unit => BillWeightUnit;
		ZDecimal IPackBL.GrossWeight => Pack.PackedItem.API_CustomsQty2;
		ZDecimal IPackBL.NetWeight => Pack.PackedItem.API_CustomsQty3;

		const string BillWeightUnit = "KGM";
	}
}
