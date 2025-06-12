namespace CargoWise.eServices.Authentication.WebService
{
	using System;

	public interface IDatabaseHelper
	{
		bool IsDatabaseAlive();
		bool CheckSystemIDExistence(string systemID);
		bool CheckCodeExistence(string enterpriseCode, string serverCode);
		bool ValidateSystemIDAndPassword(string systemID, string password);
		bool ValidateCodeAndPassword(string enterpriseCode, string serverCode, string password);
		DateTime? GetSystemLastEditUTC(string systemID);
		DateTime? ReadSystemLastEditUTC(string systemID);
	}
}
