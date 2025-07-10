using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsHeader : EU.NCTS.Business.NctsHeader, Integration.Customs.PL.ICusInBondHeader
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public new EU.NCTS.Business.INctsBillCollection<NctsBill> Bills => (EU.NCTS.Business.INctsBillCollection<NctsBill>)base.Bills;

	protected override EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection() => new EU.NCTS.Business.NctsBillCollection<NctsBill>(this);

	public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;

	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	protected override Type BillTypeCore => typeof(NctsBill);

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader> CusAuthorizationUsages
		=> (EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsHeader> GetCusAuthorizationUsages()
		=> new EU.NCTS.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>(this);

	public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

	protected override Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this);

	protected override ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => JobDocAddressValidationFactory.GetValidation(addressToValidate, this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
}
