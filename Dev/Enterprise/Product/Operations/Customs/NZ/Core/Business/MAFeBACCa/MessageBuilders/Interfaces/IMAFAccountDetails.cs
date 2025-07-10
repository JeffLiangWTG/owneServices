namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	public interface IMAFAccountDetails
	{
		ZString AccountNumber { get; } // C 12
		ZString AccountHolderName { get; } // C 50
		ZString AccountHolderDescription { get; }
		ZString WhereToSetupAccountDetailsDescription { get; } //Either account details or description where to setup them should be provided.
	}
}
