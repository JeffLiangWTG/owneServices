namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	interface IMAFValidator
	{
		ZString GetWarningMessage();
		ZString GetErrorMessage();
	}
}
