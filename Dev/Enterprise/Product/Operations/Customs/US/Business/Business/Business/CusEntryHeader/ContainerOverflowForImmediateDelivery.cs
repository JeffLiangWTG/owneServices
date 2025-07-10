using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ContainerOverflowForImmediateDelivery : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ContainerOverflowForImmediateDelivery()
		{
		}

		public ZString ContainerString
		{
			get { return containerString; }
			set { containerString = value; }
		}
		ZString containerString;
	}
}
