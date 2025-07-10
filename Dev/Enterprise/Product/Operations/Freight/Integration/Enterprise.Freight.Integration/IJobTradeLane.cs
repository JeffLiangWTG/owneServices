using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IJobTradeLane
	{
		ZGuid PK { get; }
		ZString EJ_Code { get; set; }
		ZString EJ_Description { get; set; }
		ZString EJ_Direction { get; set; }
		ZString EJ_Location1 { get; set; }
		ZString EJ_Location2 { get; set; }
	}
}
