namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("52")]
	public abstract partial class ENS52 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS52()
			: base("52")
		{
		}

		/// <summary>
		/// This indicator is no longer required. Refer to SCMS Message 05-001017.
		/// </summary>
		[MessageBlockString(1, 3, "O")]
		public ZString ChinaHongKongSWPMIndicator;

		/// <summary>
		/// Required if C/O and C/E are Canada, the entry and export dates are either on or after January 31, 2000, and the merchandise is covered by one of the given tariff numbers. If not required, space fill.
		/// </summary>
		[MessageBlockString(8, 4, "C")]
		public ZString CanadianExportCertificateSugar;

		/// <summary>
		/// Required if the entered tariff numbers are 99025111 or 99025112. The Department of Commerce issues the wool license.
		/// </summary>
		[MessageBlockString(9, 12, "C")]
		public ZString WoolLicense;

		/// <summary>
		/// Required if the entered tariff number is 98201115.
		/// </summary>
		[MessageBlockString(9, 21, "C")]
		public ZString CBTPACertificationNumber;

		/// <summary>
		/// Required for tariff numbers subject to various permit and license requirements.
		/// </summary>
		[MessageBlockString(9, 30, "C")]
		public ZString MiscellaneousPermitLicenseNumber;

		/// <summary>
		/// This field is a 2-byte numeric field that indicates the program that requires the Other Data Element-1 field to be completed.
		/// </summary>
		[MessageBlockString(2, 39, "C")]
		public ZString OtherDataIndicator1; // string

		/// <summary>
		/// This field contains up to 9 positions that are required to be submitted based on the Other Data Indicator-1 numeric field. An admin message will be sent to the trade indicating the type of data that is to be submitted in the Other Data Element-1 field.
		/// </summary>
		[MessageBlockString(9, 41, "C")]
		public ZString OtherDataElement1;

		/// <summary>
		/// This field is a 2-byte numeric field identifying the program that requires the Other Data Element-2 field to be completed.
		/// </summary>
		[MessageBlockString(2, 50, "C")]
		public ZString OtherDataIndicator2; // string

		/// <summary>
		/// This field contains up to 12 positions that are required to be submitted based on the Other Indicator-2 number field. An admin message will be sent indicating the type of data that is to be submitted in the Other Data Element-2 field.
		/// </summary>
		[MessageBlockString(12, 52, "C")]
		public ZString OtherDataElement2;

		/// <summary>
		/// This field represents miscellaneous information that must be reported for a summary line. This field will be used to report the first sale indicator as required by the 2008 farm legislation.
		/// </summary>
		[MessageBlockString(1, 64, "C")]
		public ZString MiscellaneousIndicator;
	}
}
