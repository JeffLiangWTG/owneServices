namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	/// <summary>
	/// Possible errors
	/// </summary>
	public enum Errors
	{
		None = 0,
		BadCommandLineArguments = -1,
		SendInterchangeFailed = -2,
		MsgServerConnectionException = -3,
		PathNotFound = -4,
		Unknow = -100,
	}
}
