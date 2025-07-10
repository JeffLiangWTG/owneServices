using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public interface IMAWBAllocationParent
	{
		BusinessObjectFactory Factory { get; }
		ZGuid PK { get; }

		MAWBAllocation MAWBAllocation { get; }
		ZString AWBServiceLevel { get; }
		ZString MasterBill { get; }
		ZString MasterBillAirlinePrefix { get; set; }
		ZString MasterBillMAWB { get; set; }
		ZPropertyInfo MasterBillMAWBInfo { get; }
		ZString MasterBillNeutralMAWB { get; }
		ZPropertyInfo MasterBillNeutralMAWBInfo { get; }
		ZString MawbBookingReference { get; }
		ZString TwoLetterAirlineCode { get; }
		ZString MawbPortOfLoading { get; }
		ZString MawbPortOfDischarge { get; }
		bool IsAir { get; }
		ZBool IsValidForNeutralMaster { get; }
		ZBool IsNeutralMaster { get; set; }
		ZString Prefix { get; }
		IStmNoteParent NotesParent { get; }
	}
}
