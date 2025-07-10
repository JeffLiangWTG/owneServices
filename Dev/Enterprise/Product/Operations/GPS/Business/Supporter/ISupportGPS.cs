using Enterprise.MasterFiles.Business;

namespace Enterprise.GPS.Business
{
	/// <summary>
	/// SupportGPS is vehicle or piece of equipment fitted with a GPS Tracking Device
	/// </summary>
	public interface ISupportGPS
	{
		RefEquipment Equipment { get; }
		GPSSupporterActivityCollection Activities { get; }
	}
}
