using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public class RelatedOrgPartyScreeningStatus : StmEntityScreeningLog, IRelatedOrgPartyScreeningStatus
	{
		public RelatedOrgPartyScreeningStatus(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString RelatedOrganization { get; set; }
	}
}
