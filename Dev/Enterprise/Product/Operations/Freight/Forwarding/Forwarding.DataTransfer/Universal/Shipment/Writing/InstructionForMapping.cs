using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class InstructionForMapping
	{
		public ZString PickupDeliveryType;
		public ZString DropMode;
		public ZString VehicleRegistration;
		public ZString PickupDeliveryInstruction;
		public ZBool ConfirmAddressOverride;
		public JobDocAddress ConfirmAddress;
		public List<CommonPickupDeliveryConfirm> Confirms;
	}
}
