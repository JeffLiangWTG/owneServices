using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class RefEquipmentTemplate : AutoRefEquipmentTemplate, IRefEquipmentTemplate
	{
		public RefEquipmentTemplate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
