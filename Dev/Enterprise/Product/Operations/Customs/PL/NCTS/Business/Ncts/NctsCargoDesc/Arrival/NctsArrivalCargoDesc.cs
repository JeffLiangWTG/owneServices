using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsArrivalCargoDesc : EU.NCTS.Business.NctsArrivalCargoDesc
	  , Integration.Customs.PL.IArrivalCargoDesc
{
	public NctsArrivalCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ChildEditable(true)]
	public new NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
}
