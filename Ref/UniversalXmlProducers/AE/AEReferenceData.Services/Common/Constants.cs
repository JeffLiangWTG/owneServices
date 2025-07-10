using System;

namespace CargoWise.RefDbRepo.AEReferenceData.Business;

public static class Constants
{
	public const string UnitedArabEmirates = "AE";

	public static class ProgramFunctions
	{
		public const string DubaiDecRefData = "DUBAIDECREFDATA";
	}

	public static class DataGrouping
	{
		public const string Dubai = "AED";
	}

	public static class DefaultValues
	{
		public readonly static DateTime MaxDateTime = new DateTime(2079, 06, 06, 23, 59, 00);
		public readonly static DateTime MinDateTime = new DateTime(1900, 01, 01, 00, 00, 00);
		public const string NotApplicableXmlValue = "N";
		public const string ApplicableXmlValue = "Y";
		public const string DeclarationTypeCode = "DeclarationTypeCode";
		public const string GCCCode = "GCC Code";
	}

	public static class RefCusCodeTypes
	{
        public const string CustomsOffices = "CUSOF";
        public const string CustomsResponse = "CSTA";
		public const string ExitPoint = "EXIT";
		public const string VehicleType = "VEHTP";
		public const string VehicleBrand = "VEHBR";
		public const string DeclarationPurpose = "DECPR";
		public const string InvoiceType = "INVTP";
	}

	public static class SheetNames
	{
        public const string CustomsLocation = "Customs Location";
        public const string DeclarationStatus = "Declaration Status";
		public const string ExitPoint = "Exit Point";
		public const string DeclarationType = "Declaration_Type";
		public const string AmendCancelReason = "Amendment_Cancel Reason";
		public const string VehicleType = "Vehicle Type";
		public const string VehicleBrand = "Vehicle_brand";
		public const string DeclarationPurpose = "Declaration Purpose";
		public const string InvoiceType = "Invoice_type";
	}

	public static class RefLanguageTypes
	{
		public const string Arabic = "AR";
	}
}
