using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class TransportEquipment : NonPersistentBusinessObject
	{
		protected TransportEquipment(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public static TransportEquipment New(ITransportEquipment transportEquipment, ZInt lineNo, BusinessObjectFactory factoryToWrap)
		{
			if (transportEquipment == null)
			{
				return null;
			}
			else
			{
				var transportEquipment1 = new TransportEquipment(factoryToWrap);
				transportEquipment1.ID = transportEquipment.ID;
				transportEquipment1.LineNo = lineNo;
				return transportEquipment1;
			}
		}

		public ZString ID { get; private set; }

		public ZInt LineNo { get; private set; }
	}
}
