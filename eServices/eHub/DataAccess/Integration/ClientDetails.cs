using System;

namespace CargoWise.eHub.DataAccess.Integration
{
	public class ClientDetails
	{
		public ClientDetails(string fullName, Guid ediProdLink, string email)
		{
			this.FullName = fullName;
			this.EdiProdLink = ediProdLink;
			this.Email = email;
		}

		public string FullName { get; private set; }
		public Guid EdiProdLink { get; private set; }
		public string Email { get; private set; }

		public bool IsEmptyOrInvalid
		{
			get { return string.IsNullOrEmpty(Email); }
		}

		public static implicit operator ClientDetails(eServices.eHubDataAccess.Integration.ClientDetails clientDetails)
			=> clientDetails is null ? null : new ClientDetails(clientDetails.FullName, clientDetails.EdiProdLink, clientDetails.Email);

		public static implicit operator eServices.eHubDataAccess.Integration.ClientDetails(ClientDetails clientDetails)
			=> clientDetails is null ? null : new eServices.eHubDataAccess.Integration.ClientDetails(clientDetails.FullName, clientDetails.EdiProdLink, clientDetails.Email);
	}
}
