using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ComplianceRiskStatusObject : NonPersistentBusinessObject
	{
		public ComplianceRiskStatusObject(string jobRisk, string partyRisk, string locationRisk, string commodityRisk)
		{
			JobRisk = jobRisk;
			PartyRisk = partyRisk;
			LocationRisk = locationRisk;
			CommodityRisk = commodityRisk;
		}

		public ZString JobRisk { get; }
		public ZString PartyRisk { get; }
		public ZString LocationRisk { get; }
		public ZString CommodityRisk { get; }
	}
}
