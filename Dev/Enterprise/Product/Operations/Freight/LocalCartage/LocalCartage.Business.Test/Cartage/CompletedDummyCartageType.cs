using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CompletedDummyCartageType : DummyCartageType
	{
		public CompletedDummyCartageType(DummyCartageParent parent) : base(parent)
		{
		}

		internal string method = "";
		internal string containerNo = "";
		internal DocAddressType addressType = DocAddressType.None;
		internal ZDateTime timeOut = ZDateTime.Empty;
		internal int hit;
		public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			method = "ContainerPickupCompleted";
			containerNo = container.JC_ContainerNum;
			this.addressType = addressType;
			this.timeOut = timeOut;
			hit++;
		}

		public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			method = "LoosePickupCompleted";
			containerNo = "";
			this.addressType = addressType;
			this.timeOut = timeOut;
			hit++;
		}

		public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
		{
			method = "ContainerDeliveryCompleted";
			containerNo = container.JC_ContainerNum;
			this.addressType = addressType;
			this.timeOut = timeOut;
			hit++;
		}

		public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
		{
			method = "LooseDeliveryCompleted";
			containerNo = "";
			this.addressType = addressType;
			this.timeOut = timeOut;
			hit++;
		}
	}
}
