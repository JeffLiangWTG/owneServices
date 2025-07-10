using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public class CommonGoodsItemsIntegrator : ICommonGoodsItemsIntegrator
	{
		public CommonGoodsItemsIntegrator(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		public IEnumerable<ICommonGoodsItem> GetCommonGoodsItemsForIntegration()
		{
			foreach (var invoiceLine in entryHeader.InvoiceLines)
			{
				yield return ConvertToCommonGoodsItem(invoiceLine);
			}
		}

		protected virtual ICommonGoodsItem ConvertToCommonGoodsItem(BaseJobComInvoiceLine item)
		{
			ICommonGoodsItem result = null;

			if (item is BaseJobComInvoiceLine line)
			{
				var packages = line.PackagesPivot.Select(p => new CommonPackage
				{
					BillOrReferenceNumber = p.Package.CW_HouseBill,
					PackageType = p.Package.CW_PackType,
					PackageCount = p.CHC_NumberOfPacks,
					MarksAndNumbers = p.Package.CW_MarksAndNos
				});
				result = new CommonGoodsItem()
				{
					GoodsDescription = line.JI_Description,
					GrossMass = line.JI_Weight,
					NetMass = line.JI_NetWeight,
					GrossMassUnit = line.JI_WeightUQ,
					NetMassUnit = line.JI_NetWeightUQ,
					CommodityCode = line.JI_FormattedTariff,
					Value = line.CurrencyConverter.ConvertExact(line.JI_LinePriceMoney, line.Declaration.LocalCurrency).Amount,
					Packages = packages.ToList<ICommonPackage>()
				};
			}

			return result;
		}

		public void CopyCommonGoodsItems(IEnumerable<ICommonGoodsItem> goodsItemsForIntegration, int notUsed = 0)
		{
			var invoiceHeader = entryHeader.Declaration.Invoices.FirstOrDefault();
			if (invoiceHeader != null && entryHeader.MergedLines.Count > 0)
			{
				var cei = entryHeader.EntryInstruction?.PK ?? ZGuid.Empty;
				foreach (var goodsItem in goodsItemsForIntegration)
				{
					var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine.JI_CEI = cei;
					CopyFromCommonGoodsItem(goodsItem, invoiceLine);
				}
			}
		}

		protected virtual void CopyFromCommonGoodsItem(ICommonGoodsItem source, BaseJobComInvoiceLine line)
		{
			line.JI_Description = source.GoodsDescription.Left(line.JI_DescriptionInfo.MaxLength);
			line.JI_Weight = source.GrossMass;
			line.JI_NetWeight = source.NetMass;
			line.JI_WeightUQ = source.GrossMassUnit.Left(line.JI_WeightUQInfo.MaxLength);
			line.JI_NetWeightUQ = source.NetMassUnit.Left(line.JI_NetWeightUQInfo.MaxLength);
			line.JI_FormattedTariff = source.CommodityCode.Left(line.JI_FormattedTariffInfo.MaxLength);
			line.JI_LinePrice = source.Value;

			line.PackagesPivot.RemoveAndDeleteAll();
			var declaration = line.Declaration;
			var bills = declaration.Bills;
			foreach (var package in source.Packages)
			{
				var targetBill = bills.FindByBillNumberAndType(package.BillOrReferenceNumber, BillTypeList.Codes.HouseBill);
				if (targetBill == null)
				{
					targetBill = bills.AddNew();
					targetBill.CU_BillType = BillTypeList.Codes.HouseBill;
					targetBill.CU_BillNum = package.BillOrReferenceNumber;
					targetBill.PackingGroups[0]?.Packages.RemoveAndDeleteAll();
				}
				var targetPackage = declaration.Packages.Cast<BasePackage>().FirstOrDefault(p => IsPackageEquals(p, package));
				if (targetPackage == null)
				{
					targetPackage = declaration.Packages.AddNew();
					targetPackage.CW_HouseBill = targetBill.CU_BillUniqueCode;
					targetPackage.CW_PackType = package.PackageType;
					targetPackage.CW_PackQty = package.PackageCount;
					targetPackage.CW_MarksAndNos = package.MarksAndNumbers;
				}
				var pivot = line.PackagesPivot.AddNew();
				pivot.CHC_CW = targetPackage.PK;
				pivot.CHC_NumberOfPacks = package.PackageCount;
			}
		}

		bool IsPackageEquals(BasePackage p1, ICommonPackage p2)
		{
			return p1.Bill != null && p1.Bill.CU_BillType == BillTypeList.Codes.HouseBill && p1.Bill.CU_BillNum == p2.BillOrReferenceNumber &&
				p1.CW_PackType == p2.PackageType && p1.CW_PackQty == p2.PackageCount && p1.CW_MarksAndNos == p2.MarksAndNumbers;
		}

		public IBusinessObjectCollection TheOtherCollectionToAttach()
		{
			return TheOtherCollectionToAttachCore(entryHeader);
		}

		protected virtual IBusinessObjectCollection TheOtherCollectionToAttachCore(BusinessObject header)
			=> new NctsHeaderToAttachCollection(entryHeader);

		readonly CusEntryHeader entryHeader;
	}
}
