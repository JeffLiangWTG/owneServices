namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	public interface IMAFContainer
	{
		ZString ContainerNumber { get; } // C 17 - At least one BillOfLadingNumber, SubBillOfLadingNumber or ContainerNumber must be provided per application.
		ZString ContainerType { get; } // M
		ZBool IsFullContainer { get; } // Cover Sheet
	}
}
