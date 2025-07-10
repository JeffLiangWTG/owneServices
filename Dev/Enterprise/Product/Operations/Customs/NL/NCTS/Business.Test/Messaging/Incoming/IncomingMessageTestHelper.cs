using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

public static class IncomingMessageTestHelper
{
	public static CusGuaranteeHeader SetupGuarantee(NctsHeader nctsHeader, ZString transactionReference)
	{
		var factory = nctsHeader.Factory;
		var eoriCode = "123456789000";
		var org = factory.New<OrgHeader>();
		org.OH_Code = "ORGCODE";
		org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var guaranteeHeader = GenerateGuaranteeHeader(factory, org, "19860101", transactionReference, "111", 100m);
		nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.Declarant.E2_OA_Address = org.MainAddress.PK;
		nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 150m;
		guarantee.PW_BondNumber = guaranteeHeader.CPH_Number;
		nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();
		return guaranteeHeader;
	}

	static CusGuaranteeHeader GenerateGuaranteeHeader(BusinessObjectFactory factory, OrgHeader org, ZString pW_BondNumber, ZString transactionReference, ZString transactionAppId, ZDecimal valueTransaction)
	{
		var guaranteeHeader = factory.New<CusGuaranteeHeader>();
		guaranteeHeader.CPH_Number = pW_BondNumber;
		guaranteeHeader.CPH_OH_PermitHolder = org.PK;
		guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
		guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
		guaranteeHeader.AddTransaction(transactionReference, "CMT-TO-CONF", "OpeningBalance", "", valueTransaction, 0, PermitTransactionStatusList.Codes.Pending, transactionType: PermitTransactionTypeList.Codes.OBL, isAggregated: true);
		guaranteeHeader.AddTransaction(transactionReference, "CMT-CON", transactionAppId, "", -10, 0, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
		return guaranteeHeader;
	}
}
