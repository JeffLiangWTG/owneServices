using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.MasterFiles.Business
{
	[SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
	[Flags]
	public enum FreightMode
	{
		UKN = 0x00000,      // Unknown 

		Containerised = 0x01000,
		NonContainerised = 0x02000,
		FullLoad = 0x00400,

		AIR = 0x10000,      // Air Freight
		SEA = 0x20000,      // Sea Freight
		RAI = 0x40000,      // Rail Freight 
		ROA = 0x80000,      // Road Freight
		MAI = 0x100000,     // Post
		BCN = 0x200000,     // Buyer’s Consol
		SCN = 0x400000,     // Shipper's Consol
		COU = 0x800000,     // Courier Freight
		OBC = 0x800001,     // On Board Courier 
		UNA = 0x800002,     // Unaccompanied

		ULD = 0x11000,      // Air Freight ULD (Containerised)
		LSE = 0x12000,      // Air Freight Loose (non Containerised)

		FCL = 0x21000,      // FCL Sea
		GRP = 0x21100,      // Groupage (used only in CFS)
		LCL = 0x22000,      // LCL Sea
		BLK = 0x22100,      // Bulk 		
		BBK = 0x22200,      // Break Bulk
		ROR = 0x22300,      // Roll On/Roll Off

		FRA = 0x41000,      // FCL Rail, 
		LRA = 0x42000,      // LCL Rail, 
		FWL = 0x42400,      // Full Wagon Load (non Containerised) Rail

		FRO = 0x81000,      // FCL Road, 
		LRO = 0x82000,      // LCL Road,  
		FTL = 0x82400,      // Full Truck Load Road

		// Local Transport
		//
		// 0x80000 = Road
		// 0x10000 = Air
		// 0x01000 = Containerized
		// 0x02000 = Loose
		// 0x00001 - 0x00013 = Incremental Types

		TCY = 0x81001,  // CTO CNE (Inclusive Dehire)
		TCN = 0x81002,  // CTO CNE

		TFY = 0x81003,  // CTO CFS (Inclusive Dehire)
		TFN = 0x81004,  // CTO CFS

		DCL = 0x82005,  // CFS CNE (Loose)
		DCA = 0x12006,  // CFS CNE (AIR)
		DCM = 0x82007,  // CFS CNE (MilkRun)

		CYE = 0x81008,  // CNE CYD
		DYE = 0x81009,  // CFS CYD

		YDE = 0x8100A,  // CYD CFS
		YCE = 0x8100B,  // CYD CNR

		SDL = 0x8200C,  // CNR CFS (Loose)
		SDA = 0x1200D,  // CNR CFS (AIR)
		SDM = 0x8200E,  // CNR CFS (MilkRun)

		DTY = 0x8100F,  // CFS CTO (Inclusive Hire)
		DTN = 0x81010,  // CFS CTO

		STY = 0x81011,  // CNR CTO (Inclusive Hire)
		STN = 0x81012,  // CNR CTO

		DDF = 0x82013,  // CFS CFS (FTL)

		FreightTypeMask = 0xFF0000,
	}
}
