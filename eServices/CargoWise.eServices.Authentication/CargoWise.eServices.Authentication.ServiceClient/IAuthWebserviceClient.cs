namespace CargoWise.eServices.Authentication.ServiceClient
{
	public interface IAuthWebserviceApi
	{
		bool CheckSystemExistence(string systemID);
		bool CheckSystemExistence(string enterpriseCode, string serverCode);
		bool ValidateSystem(string systemID, string password);
		bool ValidateSystem(string enterpriseCode, string serverCode, string password);
        bool Ping();
    }
}
