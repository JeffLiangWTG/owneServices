using System;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class SecurityAccessor : ISecurityAccessor
	{
		public SecurityAccessor() : this(new eServices.eHubDataAccess.Sql.SecurityAccessor()) { }

		public SecurityAccessor(eServices.eHubDataAccess.Integration.ISecurityAccessor securityAccessor)
		{
			this.securityAccessor = securityAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.ISecurityAccessor securityAccessor;

		public PasswordDetail UpdatePassword(string clientID, string password)
			=> securityAccessor.UpdatePassword(clientID, password);

		public bool ValidatePassword(string clientID, string password)
			=> securityAccessor.ValidatePassword(clientID, password);

		public bool CheckAccess(string clientID, string operation)
			=> securityAccessor.CheckAccess(clientID, operation);

		public void InsertToRegistrationLog(string clientID, string ipAddress, DateTime requestTimeUTC)
			=> securityAccessor.InsertToRegistrationLog(clientID, ipAddress, requestTimeUTC);

		public bool CheckClientAuthorisation(string senderID, string recipientID)
			=> securityAccessor.CheckClientAuthorisation(senderID, recipientID);
	}
}
