using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsArrivalMovementHeader : EU.NCTS.Business.NctsArrivalMovementHeader
	, Integration.Customs.PL.IArrivalMovementHeader
{
	public NctsArrivalMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	[ResourceStringData("245582A6-A1FC-4478-80A8-770436192545", Caption = "Goods Location from Authorization", ShortCaption = "Goods Loc. from Auth.")]
	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.AuthorizationRuleList))]
	public override ZString AuthorizationLocation
	{
		get => base.AuthorizationLocation;
		set => base.AuthorizationLocation = value;
	}

	[ResourceStringData("97E67EF1-CC39-4155-B634-2D75B8DBA0C8", Caption = "Trader Representative for Communication", MediumCaption = "Trader Rep. For Comm.", ShortCaption = "Trader Rep.")]
	[List(nameof(Lookups) + "." + nameof(NctsArrivalMovementHeaderLookups.Organisations))]
	public ZGuid RepresentativeTrader
	{
		get => Representative.OrganisationPK;
		set
		{
			var oldValue = RepresentativeTrader;
			Representative.OrganisationPK = value;
			RepresentativeTraderInfo.RefreshBinding(oldValue);
		}
	}

	protected override ZValidation GetRepresentativeJobDocAddressAdditionalValidation(JobDocAddress representativeJobDocAddress) => new RepresentativeJobDocAddressValidation(Representative);

	public ZPropertyInfo RepresentativeTraderInfo => GetWrappedZPropertyInfo(nameof(RepresentativeTrader), x => Representative.OrganisationPKInfo);

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new NctsArrivalMovementHeaderLookups Lookups => (NctsArrivalMovementHeaderLookups)base.Lookups;

	protected override CusInBondMoveHeaderLookups GetNewLookups() => new NctsArrivalMovementHeaderLookups(this);

	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	public new NctsArrivalMovementHeaderValidation Validation => (NctsArrivalMovementHeaderValidation)base.Validation;

	protected override CusInBondMoveHeaderValidation GetNewValidation() => new NctsArrivalMovementHeaderValidation(this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
}
