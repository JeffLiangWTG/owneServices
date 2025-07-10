using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class JobDateTypes
	{
		public static class Codes
		{
			public const string ArrivalDate = "ARV";
			public const string DepartureDate = "DEP";
			public const string EstimatedArrivalDate = "EAD";
			public const string EstimatedDepartureDate = "EDD";
			public const string AWBIssueDate = "AWB";
			public const string CustomsClearanceDate = "CUS";
			public const string PickupDate = "PIC";
			public const string DeliveryDate = "DEL";
			public const string VesselArrivalDate = "VAD";
			public const string VesselDepartureDate = "VDD";
			public const string HouseBillIssueDate = "HBD";
			public const string JobOpenDate = "JOP";
			public const string FirstContainerGateInDate = "FGI";
			public const string LastContainerGateInDate = "LGI";
			public const string CFSReceivalStartDate = "CFS";
			public const string HBLPlaceOfReceiptArrivalDate = "POR";
			public const string InterimReceiptDate = "IRD";
			public const string YardInDate = "YID";
			public const string YardOutDate = "YOD";
			public const string GateInDate = "GIN";
			public const string GateOutDate = "GOU";
			public const string CostingAutoratingDateOverride = "ADO";
			public const string RevenueAutoratingDateOverride = "RDO";
		}

		public static class Descriptions
		{
			public static MultilingualString ArrivalDate => ResString.GetMultilingualString("db80a50d-ed77-48c8-afb3-78a3ec6a0b4a", "Arrival Date");
			public static MultilingualString DepartureDate => ResString.GetMultilingualString("bffdc860-64bc-499d-8586-ba279c4057ae", "Departure Date");
			public static MultilingualString EstimatedArrivalDate => ResString.GetMultilingualString("7E76EE6B-74CA-4F92-8806-263E83CC40F8", "Estimated Arrival Date");
			public static MultilingualString EstimatedDepartureDate => ResString.GetMultilingualString("D786641C-E34D-4228-8E91-FCF426558396", "Estimated Departure Date");
			public static MultilingualString AWBIssueDate => ResString.GetMultilingualString("e295ed9d-e4d3-419a-861c-fde3ac64a7bc", "AWB Issue Date");
			public static MultilingualString CustomsClearanceDate => ResString.GetMultilingualString("401addc8-536b-4305-b7a4-7e6540dacbb6", "Customs Clearance Date");
			public static MultilingualString PickupDate => ResString.GetMultilingualString("dc571cf4-25e9-4a54-a4cf-cb54e7d5c0f6", "Pickup Date");
			public static MultilingualString DeliveryDate => ResString.GetMultilingualString("b783f53c-5d63-4dac-abc0-3b228adc8419", "Delivery Date");
			public static MultilingualString VesselArrivalDate => ResString.GetMultilingualString("747369ef-9812-4916-86d3-cf6e20cfe1a8", "Vessel Arrival Date");
			public static MultilingualString VesselDepartureDate => ResString.GetMultilingualString("7e020594-4f1c-4851-aa93-5ec1ac45eedc", "Vessel Departure Date");
			public static MultilingualString HouseBillIssueDate => ResString.GetMultilingualString("ea142ce8-480f-4870-8820-3e29acee98e7", "House Bill Issue Date");
			public static MultilingualString JobOpenDate => ResString.GetMultilingualString("145D6A95-AF74-494E-99A0-BDD9FE4FC52D", "Job Open Date");
			public static MultilingualString FirstContainerGateInDate => ResString.GetMultilingualString("18E7135B-AD4D-4993-ABF1-5D5884C5A947", "First Container Gate In Date");
			public static MultilingualString LastContainerGateInDate => ResString.GetMultilingualString("F1AB525A-E1D5-4268-A388-80A43F3DEA91", "Last Container Gate In Date");
			public static MultilingualString CFSReceivalStartDate => ResString.GetMultilingualString("78204744-C545-42A4-B35A-D241B3B1555D", "CFS Receival Start Date");
			public static MultilingualString HBLPlaceOfReceiptArrivalDate => ResString.GetMultilingualString("782E2C16-EBBB-4EA3-9F32-B0FC656A5974", "HBL Place Of Receipt Arrival Date");
			public static MultilingualString InterimReceiptDate => ResString.GetMultilingualString("f83d642d-d2c5-448c-8b44-6d774e9a2da6", "Interim Receipt Date");
			public static MultilingualString YardInDate => ResString.GetMultilingualString("cc1a1cf9-8265-481b-a29b-e63471613007", "Yard In Date");
			public static MultilingualString YardOutDate => ResString.GetMultilingualString("aa26cf1f-6e91-498c-b99e-4c0d9ce13c13", "Yard Out Date");
			public static MultilingualString GateInDate => ResString.GetMultilingualString("ab113b81-2dbd-4ae6-9128-2c5a1b6974fa", "Gate In Date");
			public static MultilingualString GateOutDate => ResString.GetMultilingualString("88daaa39-0184-4ed6-af2a-25fbe7c8571f", "Gate Out Date");
			public static MultilingualString CostingAutoratingDateOverride => ResString.GetMultilingualString("b0fa9c47-e354-417f-aed6-75f6f41dc24f", "Costing Autorating Date");
			public static MultilingualString RevenueAutoratingDateOverride => ResString.GetMultilingualString("bee8ba31-4ab9-4545-8efb-311aa874422c", "Revenue Autorating Date");
		}

		public static CodeDescriptionPairList JobDateTypeList
		{
			get
			{
				var dateTypeList = new CodeDescriptionPairList();

				dateTypeList.AddPair(Codes.ArrivalDate, Descriptions.ArrivalDate);
				dateTypeList.AddPair(Codes.DepartureDate, Descriptions.DepartureDate);
				dateTypeList.AddPair(Codes.EstimatedArrivalDate, Descriptions.EstimatedArrivalDate);
				dateTypeList.AddPair(Codes.EstimatedDepartureDate, Descriptions.EstimatedDepartureDate);
				dateTypeList.AddPair(Codes.AWBIssueDate, Descriptions.AWBIssueDate);
				dateTypeList.AddPair(Codes.CustomsClearanceDate, Descriptions.CustomsClearanceDate);
				dateTypeList.AddPair(Codes.PickupDate, Descriptions.PickupDate);
				dateTypeList.AddPair(Codes.DeliveryDate, Descriptions.DeliveryDate);
				dateTypeList.AddPair(Codes.VesselArrivalDate, Descriptions.VesselArrivalDate);
				dateTypeList.AddPair(Codes.VesselDepartureDate, Descriptions.VesselDepartureDate);
				dateTypeList.AddPair(Codes.HouseBillIssueDate, Descriptions.HouseBillIssueDate);
				dateTypeList.AddPair(Codes.JobOpenDate, Descriptions.JobOpenDate);
				dateTypeList.AddPair(Codes.FirstContainerGateInDate, Descriptions.FirstContainerGateInDate);
				dateTypeList.AddPair(Codes.LastContainerGateInDate, Descriptions.LastContainerGateInDate);
				dateTypeList.AddPair(Codes.CFSReceivalStartDate, Descriptions.CFSReceivalStartDate);
				dateTypeList.AddPair(Codes.HBLPlaceOfReceiptArrivalDate, Descriptions.HBLPlaceOfReceiptArrivalDate);
				dateTypeList.AddPair(Codes.InterimReceiptDate, Descriptions.InterimReceiptDate);
				dateTypeList.AddPair(Codes.YardInDate, Descriptions.YardInDate);
				dateTypeList.AddPair(Codes.YardOutDate, Descriptions.YardOutDate);
				dateTypeList.AddPair(Codes.GateInDate, Descriptions.GateInDate);
				dateTypeList.AddPair(Codes.GateOutDate, Descriptions.GateOutDate);
				dateTypeList.AddPair(Codes.CostingAutoratingDateOverride, Descriptions.CostingAutoratingDateOverride);
				dateTypeList.AddPair(Codes.RevenueAutoratingDateOverride, Descriptions.RevenueAutoratingDateOverride);

				return dateTypeList;
			}
		}
	}
}
