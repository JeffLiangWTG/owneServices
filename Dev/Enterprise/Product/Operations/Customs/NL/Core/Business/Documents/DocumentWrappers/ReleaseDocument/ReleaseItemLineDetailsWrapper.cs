using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class ReleaseItemLineDetailsWrapper : NonPersistentBusinessObject, IItemLineDetails
{
	readonly CusEntryLine entryLine;

	public ReleaseItemLineDetailsWrapper(CusEntryLine entryLine, int lineItem) : base((entryLine as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
	}

	public ZInt LineItem => entryLine.CL_LineNumber;

	public ZDecimal GrossWeight => entryLine.GrossWeight.InKilogramsSafe;

	public ZDecimal NettWeight => entryLine.EffectiveNetWeight.InKilogramsSafe;

	public ZString TariffCode => entryLine.Tariff.Left(8) + entryLine.Tariff.SubstringSafe(8, 2);

	public ZString Procedure => entryLine.ProcedureCode.Left(7);

	public ZString UNDG
	{
		get
		{
			var result = new ZStringBuilder();
			entryLine.UNDGs.Select(x => x.SubstanceCode).Distinct().ForEach(x => result.Append(x));
			return result.ToStringWithDelimiterBetweenAppends(",");
		}
	}

	public ZString GoodsDescription => entryLine.GoodsDescription;

	public ZString QtyUoM => entryLine.CustomsQuantity == ZDecimal.Zero ? string.Empty : entryLine.CustomsQuantity.Round(3) + " " + entryLine.CustomsUnitQty;

	public ZDecimal StatValue => entryLine.StatisticalValue;

	public ZDecimal CustomsValue => entryLine.CustomsValue.Amount;

	public ZString PackageMarks
	{
		get
		{
			var result = new ZStringBuilder();
			entryLine.PackagingDetails.Select(x => x.Package.CW_MarksAndNos).Distinct().ForEach(x => result.Append(x));
			return result.ToStringWithDelimiterBetweenAppends(",").Trim(',');
		}
	}

	public BusinessObjectCollectionWrapper<ReleaseAdditionalDocumentWrapper> AdditionalDocuments
	{
		get
		{
			if (additionalDocuments == null)
			{
				additionalDocuments = new BusinessObjectCollectionWrapper<ReleaseAdditionalDocumentWrapper>(entryLine.AdditionalInfos.Select(x => new ReleaseAdditionalDocumentWrapper(x)));
			}
			return additionalDocuments;
		}
	}
	BusinessObjectCollectionWrapper<ReleaseAdditionalDocumentWrapper> additionalDocuments;

	IEnumerable<ICusSupportingDocument> IItemLineDetails.AdditionalDocuments => entryLine.AdditionalInfos.Select(x => new ReleaseAdditionalDocumentWrapper(x));
}
