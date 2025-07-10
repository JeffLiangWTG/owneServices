using System;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PickupDeliveryConfirmData),
	Enterprise.Core.Constants.DocManagerCodes.PickupDeliveryConfirm)]

namespace Enterprise.Freight.Business
{
	public class PickupDeliveryConfirmData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CommonPickupDeliveryConfirm); } }
		protected override Type CollectionType
		{
			get { return typeof(CommonPickupDeliveryConfirmCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("e7b2c9a6-a8ad-4d02-b519-8499c53f8c26", "Pickup Delivery Confirm"); } }
	}
}
