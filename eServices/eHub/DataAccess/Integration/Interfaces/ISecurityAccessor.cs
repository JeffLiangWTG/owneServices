using System;
namespace CargoWise.eHub.DataAccess.Integration
{
	public interface ISecurityAccessor
	{
		bool ValidatePassword(string clientID, string password);
		PasswordDetail UpdatePassword(string clientID, string password);
		bool CheckAccess(string clientID, string operation);
		void InsertToRegistrationLog(string clientID, string ipAddress, DateTime requestTimeUTC);
		bool CheckClientAuthorisation(string senderID, string recipientID);
	}
}