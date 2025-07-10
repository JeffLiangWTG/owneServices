using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader
	, Integration.Customs.PL.IDepartureMovementHeader
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => JobDocAddressValidationFactory.GetValidation(addressToValidate, Header);

	protected override EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderPhase5Validation(this);

	protected override Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this);

	protected override bool ShouldGenerateLocalReferenceNumberOnSavingCore => false;

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader> CusAuthorizationUsages
		=> (EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsDepartureMovementHeader> GetCusAuthorizationUsages()
		=> new EU.NCTS.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsDepartureMovementHeader>(this);

	public string GetLRNAndSetIfNeeded()
	{
		var info = (ZPropertyInfoString)BM_PaperlessInbondNumInfo;
		var value = info.Value;
		if (value.IsEmpty)
		{
			var eori = GetRepresentativeOrPrincipalEORI();
			if (!eori.IsEmpty)
			{
				var formattedEori = FormatEori(eori);
				var sequence = LrnNumberFountain.GetNext(Factory).ToString("D8");
				var twoLastDigitsOfYear = ZDateTime.Now.ToString("yy");

				info.Value = $"{formattedEori}{twoLastDigitsOfYear}{sequence}";
			}
		}
		return info.Value;
	}

	ZString GetRepresentativeOrPrincipalEORI()
	{
		var result = EuEoriResolver.GetRegNoWithCountryCode(Header.MovementHeader.Representative.Organisation);
		return result.IsEmpty
			? EuEoriResolver.GetRegNoWithCountryCode(Header.Principal.Organisation)
			: result;
	}

	ZString FormatEori(ZString eori)
	{
		const int acceptedEoriLength = 12;
		return eori.Length == acceptedEoriLength
			? eori
			: eori.Length < acceptedEoriLength
				? eori.PadRight(acceptedEoriLength, '0')
				: eori.Substring(0, acceptedEoriLength);
	}

	public DateTime? GetExportDate() => !BM_ExportDate.IsEmpty && IsSimplifiedNctsProcedure
		? BM_ExportDate.ToDateTime()
		: null;

	protected override bool IsGrossWeightUQReadOnlyCore => false;

	public override ZString BM_TypeOfSecurity
	{
		get => base.BM_TypeOfSecurity;
		set
		{
			var oldValue = BM_TypeOfSecurity;
			base.BM_TypeOfSecurity = value;
			if (!IsCopying && oldValue != BM_TypeOfSecurity && !IsMarkingAsNeedingValidationSuspended)
			{
				var customsOffices = Header.IsPhase5 ? CustomsOffices : Header.CustomsOffices;
				customsOffices.MarkAsNeedingValidation();
			}
		}
	}

	[ChildEditable(true)]
	public new NctsPLOfficeCodeCollection CustomsOffices => (NctsPLOfficeCodeCollection)base.CustomsOffices;

	protected override EU.NCTS.Business.NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsPLOfficeCodeCollection(this);

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsPLOfficeCode);
		return result;
	}
}
