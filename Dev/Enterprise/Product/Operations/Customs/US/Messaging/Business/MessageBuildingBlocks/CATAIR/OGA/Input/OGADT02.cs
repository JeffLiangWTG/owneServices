namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("DT02")]
	public abstract partial class OGADT02 : MessageBlock // Need to add interface for BIRD System
	{
		public OGADT02()
			: base("DT02")
		{
		}

		/// <summary>
		/// The name of the company that manufactured the vehicle.
		/// </summary>
		[MessageBlockString(15, 5, "C")]
		public ZString MakeOfVehicle;

		/// <summary>
		/// The name of the vehicle that the company manufactured.
		/// </summary>
		[MessageBlockString(15, 20, "C")]
		public ZString Model;

		/// <summary>
		/// The year the vehicle was manufactured.
		/// </summary>
		[MessageBlockInt(4, 35, "C")]
		public ZInt Year;

		/// <summary>
		/// The unique code identifying the vehicle.
		/// </summary>
		[MessageBlockString(17, 39, "C")]
		public ZString VehicleIdentificationNumber;

		/// <summary>
		/// The code assigned to the company making the non-conforming vehicle modification.
		/// </summary>
		[MessageBlockString(8, 56, "C")]
		public ZString NHTSARegisteredImporterRINumber;

		/// <summary>
		/// The NHTSA number corresponding to the year, make and model of every non-conforming vehicle.
		/// </summary>
		[MessageBlockString(6, 64, "C")]
		public ZString VehicleEligibilityNumber;
	}
}
