using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface ICO2eParent
	{
		ZGuid JobCO2eParentID { get; }
		ZString JobCO2eParentTableCode { get; }
		IJobCO2eCollection JobCO2eCollection { get; }
		void RefreshCO2e();
	}
}
