using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface IRefEquipmentTemplate
	{
		ZString RET_Description { get; set; }
		ZString RET_Template { get; set; }
	}
}
