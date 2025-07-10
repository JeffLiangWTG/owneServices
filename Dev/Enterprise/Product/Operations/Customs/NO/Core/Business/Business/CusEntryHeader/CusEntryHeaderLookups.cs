using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
	{
	}

	new CusEntryHeader Parent => base.Parent as CusEntryHeader;

	new JobDeclaration Declaration => base.Declaration as JobDeclaration;

	public ICodeDescriptionPairList CH_ReCalcOrigDecl_List
	{
		get
		{
			var codeList = new CodeDescriptionPairList();
			if (Parent?.GetOriginalDeclaration() is not JobDeclaration declarationOriginal)
			{
				return codeList;
			}
			var codesToAdd = declarationOriginal.CustomsEntryHeaders
				?.Select(ch => ch?.MovementReferenceNumber ?? ZString.Empty)
				?.Where(mrn => mrn.IsValid)
				?.Select(mrn => mrn.ToString())
				?.Select(mrn => new CodeDescriptionPair(mrn, mrn));
			codeList.AddPairsIfNotExist(codesToAdd);
			return codeList;
		}
	}

	public ICodeDescriptionPairList CH_ReCalcCaseCode_List => Factory.GetCachedValue("NO/05b05d71-d9e0-4e7e-a376-4ec8877cd443", GetReCalcCaseCode_List);

	ICodeDescriptionPairList GetReCalcCaseCode_List() => new RecalculationCaseCodeList();

	public ICodeDescriptionPairList CH_ReCalcDeclType_List => Factory.GetCachedValue(ReCalcDeclType_CacheKey, GetReCalcDeclType_List);

	string ReCalcDeclType_CacheKey => $"NO/62686745-ee71-4df8-985a-6b7c49490dc8/{Declaration?.JE_CopyStatus}";

	ICodeDescriptionPairList GetReCalcDeclType_List()
	{
		var codeList = new CodeDescriptionPairList();
		switch (Declaration?.JE_CopyStatus)
		{
			case NODeclarationCopyStatus.Codes.Recalculation:
				codeList.AddPair("EB/RE", ResString.GetMultilingualString("NO/568e1837-803d-4690-8f47-1670ef14b956", "EB/RE"));
				codeList.AddPair("SO", ResString.GetMultilingualString("NO/31406a8e-0075-4f6f-bc5d-dc712890cb12", "SO"));
				break;
			case NODeclarationCopyStatus.Codes.ReExport:
				codeList.AddPair("MA", ResString.GetMultilingualString("NO/17dcf49a-a470-4774-856c-eb624cfbe8b5", "MA"));
				break;
			case NODeclarationCopyStatus.Codes.FinalImport:
				codeList.AddPair("EN", ResString.GetMultilingualString("NO/b6e00404-cb7c-4430-94bc-4b8bddb2d160", "EN"));
				break;
		}
		return codeList;
	}

	public CodeDescriptionPairList PaymentMethodCodeList => Factory.GetCachedValue($"NO.CusEntryHeader.PaymentMethodCodeList.{Parent.HasPayments}", GetPaymentMethodCodeList);

	CodeDescriptionPairList GetPaymentMethodCodeList()
	{
		var codeList = new NOPaymentMethodCodeList();
		if (Parent.HasPayments)
		{
			codeList.RemoveCode(NOPaymentMethodCodeList.Codes.NoDutiesOrVatPayable);
		}
		else
		{
			codeList.RemoveCode(NOPaymentMethodCodeList.Codes.ForwardersDayCredit);
			codeList.RemoveCode(NOPaymentMethodCodeList.Codes.Cash);
			codeList.RemoveCode(NOPaymentMethodCodeList.Codes.ImportersDeferred);
		}
		return codeList;
	}

	internal CodeDescriptionPairList PhaseStatusList => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();
}
