using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESCommodityProvider : ICommodity
{
	protected AESCommodityProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
		invoiceLine = entryLine.RandomLine;
		EntryInstruction = Argument.NotNull(invoiceLine.EntryInstruction, $"{nameof(invoiceLine)}.{nameof(JobComInvoiceLine.EntryInstruction)}");
	}

	protected CusEntryInstruction EntryInstruction { get; }
	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine invoiceLine;

	public static AESCommodityProvider NewOrNull(CusEntryLine entryLine) => entryLine is not null
		? new AESCommodityProvider(entryLine)
		: null;

	public string DescriptionOfGoods => CachedValueHelper.GetValue(ref descriptionOfGoods,
		() =>
		{
			var nDescription = invoiceLine.JI_NDescription;
			return nDescription.IsEmpty ? invoiceLine.JI_Description : nDescription;
		});
	CachedValue<string> descriptionOfGoods;

	public int DescriptionOfGoodsMaxLength => invoiceLine.DescriptionMaxSize;

	public string CusCode => invoiceLine.ZG_CusNumber;

	public ICommodityCode CommodityCode => CachedValueHelper.GetValue(ref commodityCode,
		() => new AESCommodityCodeProvider(invoiceLine));
	CachedValue<ICommodityCode> commodityCode;

	public IReadOnlyCollection<IDangerousGoods> DangerousGoods => dangerousGoods ??= GetDangerousGoods();
	IReadOnlyCollection<IDangerousGoods> dangerousGoods;

	public IGoodsMeasure GoodsMeasure => CachedValueHelper.GetValue(ref goodsMeasure,
		() => new AESGoodsMeasureProvider(entryLine));
	CachedValue<IGoodsMeasure> goodsMeasure;

	public IReadOnlyCollection<IDutiesAndTaxesType> CalculationOfTaxes => calculationOfTaxes ??= GetCalculationOfTaxes(); 
	IReadOnlyCollection<IDutiesAndTaxesType> calculationOfTaxes;

	protected virtual IReadOnlyCollection<IDangerousGoods> GetDangerousGoods() => invoiceLine.UNDGs
		.Where(x => x.UNDGSubstance != null)
		.Select((x, i) => new AESDangerousGoodsProvider(x.UNDGSubstance, i + 1))
		.ToArray<IDangerousGoods>();

	protected virtual IReadOnlyCollection<IDutiesAndTaxesType> GetCalculationOfTaxes() => Array.Empty<IDutiesAndTaxesType>(); // TODO: in the near future
}
