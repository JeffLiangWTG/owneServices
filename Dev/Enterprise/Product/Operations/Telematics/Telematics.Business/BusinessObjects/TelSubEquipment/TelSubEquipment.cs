using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class TelSubEquipment : AutoTelSubEquipment
	{
		public TelSubEquipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.TelSubEquipmentTypeList")]
		public override ZString TSE_Type
		{
			get => base.TSE_Type;
			set => base.TSE_Type = value;
		}
	}
}
