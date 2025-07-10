using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MAWBAllocationParentForTest : IMAWBAllocationParent
	{
		public MAWBAllocationParentForTest(BusinessObjectFactory factory)
		{
			Factory = factory;
			SetDefaults();
		}

		void SetDefaults()
		{
			PK = ZGuid.NewZGuid();
			Prefix = "JS";
			AWBServiceLevel = "STD";
			IsAir = true;
			IsValidForNeutralMaster = true;
			IsNeutralMaster = true;
			MAWBAllocation = new MAWBAllocation(this);
		}

		public ZGuid PK { get; set; }
		public ZString Prefix { get; set; }
		public BusinessObjectFactory Factory { get; set; }
		public MAWBAllocation MAWBAllocation { get; set; }
		public ZString AWBServiceLevel { get; set; }
		public ZString MasterBill { get; set; }
		public ZString MasterBillAirlinePrefix { get; set; }
		public ZString MasterBillMAWB { get; set; }
		public ZPropertyInfo MasterBillMAWBInfo { get; set; }
		public ZString MasterBillNeutralMAWB { get; set; }
		public ZPropertyInfo MasterBillNeutralMAWBInfo { get; set; }
		public ZString MawbBookingReference { get; set; }
		public ZString TwoLetterAirlineCode { get; set; }
		public ZString MawbPortOfLoading { get; set; }
		public ZString MawbPortOfDischarge { get; set; }
		public bool IsAir { get; set; }
		public ZBool IsValidForNeutralMaster { get; set; }
		public ZBool IsNeutralMaster { get; set; }
		public IStmNoteParent NotesParent { get; set; }
	}
}
