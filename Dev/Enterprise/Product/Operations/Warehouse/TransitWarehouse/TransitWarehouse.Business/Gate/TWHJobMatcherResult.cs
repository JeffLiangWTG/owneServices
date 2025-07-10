using System.Collections.Generic;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHJobMatcherResult
	{
		public string ReferenceNumber { get; set; }

		public ReferenceNumberTypes? ReferenceNumberType { get; set; }

		public ValidationErrorCode? ErrorCode { get; set; }

		public string ErrorMessage { get; set; }

		public IEnumerable<WhsItemPackageState> PackageStates { get; set; }

		public decimal? GrossWeightValue { get; set; }

		public string GrossWeightUnit { get; set; }

		public decimal? GrossVolumeValue { get; set; }

		public string GrossVolumeUnit { get; set; }

		public int? QuantityValue { get; set; }
	}

	public enum ReferenceNumberTypes
	{
		Unknown,
		ContainerNumber,
		VehicleReference,
		HouseBill,
		MasterBill,
		ReceiveConsignment,
		DispatchConsignment,
		AdvancedShippingNotice,
		DispatchLoadList,
		ForwardingShipmentNumber,
		ForwardingConsolNumber
	}

	public enum ValidationResult
	{
		Accept,
		Decline
	}

	public enum ValidationErrorCode
	{
		FNF,
		JNF,
		VDG,
		INV,
	}
}
