using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AESCommodityCodeProvider : ICommodityCode
{
	public AESCommodityCodeProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	readonly JobComInvoiceLine invoiceLine;

	public string HarmonizedSystemSubHeadingCode =>
		CachedValueHelper.GetValue(ref harmonizedSystemSubHeadingCode, () => invoiceLine.JI_Tariff.Left(6));
	CachedValue<string> harmonizedSystemSubHeadingCode;

	public string CombinedNomenclatureCode =>
		CachedValueHelper.GetValue(ref combinedNomenclatureCode, () => invoiceLine.JI_Tariff.SubstringSafe(6, 2));
	CachedValue<string> combinedNomenclatureCode;

	public IReadOnlyCollection<IAdditionalCode> TARICAdditionalCodes => taricAdditionalCodes ?? (taricAdditionalCodes =
		invoiceLine.SupplementaryCodes
			.Where(x => !IsNationalCode(x.CY_Code))
			.Select((x, i) => new AESAdditionalCodeProvider(x, i + 1))
			.ToArray<IAdditionalCode>());
	IReadOnlyCollection<IAdditionalCode> taricAdditionalCodes;

	public IReadOnlyCollection<IAdditionalCode> NationalAdditionalCodes => nationalAdditionalCodes ?? (nationalAdditionalCodes =
		invoiceLine.SupplementaryCodes
			.Where(x => IsNationalCode(x.CY_Code))
			.Select((x, i) => new AESAdditionalCodeProvider(x, i + 1))
			.ToArray<IAdditionalCode>());
	IReadOnlyCollection<IAdditionalCode> nationalAdditionalCodes;

	bool IsNationalCode(ZString supplementaryCode) => !supplementaryCode.IsEmpty && "JKLMNOPQRSTUVWXYZ".Contains(supplementaryCode[0]);
}
