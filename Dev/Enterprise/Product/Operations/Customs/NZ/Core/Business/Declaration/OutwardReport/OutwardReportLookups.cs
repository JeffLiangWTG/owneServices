using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport
{
	public class OutwardReportLookups : ZLookups
	{
		public OutwardReportLookups(OutwardReportManifestStatus oCRManifestStatus)
			: base(oCRManifestStatus) { }
		public ICodeDescriptionPairList MsgTransportList
		{
			get { return Factory.GetCachedValue<MsgTransportList>(); }
		}
	}
}
