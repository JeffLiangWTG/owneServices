using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ShipmentSummary : NonPersistentBusinessObject
	{
		public ShipmentSummary(DynamicBusinessObject dynamicObj)
		{
			DynamicObj = dynamicObj;
		}

		DynamicBusinessObject DynamicObj { get; set; }

		public ZString UniqueConsignRef
		{
			get { return (ZString)DynamicObj[JobShipmentSchema.Constants.JS_UniqueConsignRef]; }
		}

		public ZString ConsolReference
		{
			get { return (ZString)DynamicObj[JobConsolSchema.Constants.JK_UniqueConsignRef]; }
		}

		public ZDateTime DepartureDate
		{
			get { return (ZDateTime)DynamicObj[nameof(DepartureDate)]; }
		}

		public ZString Origin
		{
			get { return (ZString)DynamicObj[JobShipmentSchema.Constants.JS_RL_NKOrigin]; }
		}

		public ZString Destination
		{
			get { return (ZString)DynamicObj[JobShipmentSchema.Constants.JS_RL_NKDestination]; }
		}
	}
}
