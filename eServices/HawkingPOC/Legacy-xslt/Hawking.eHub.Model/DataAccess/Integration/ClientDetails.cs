using System;

namespace Hawking.eHub.Model.DataAccess.Integration
{
    public class ClientDetails
    {
        public ClientDetails(string fullName, Guid ediProdLink, string email)
        {
            FullName = fullName;
            EdiProdLink = ediProdLink;
            Email = email;
        }

        public string FullName { get; private set; }
        public Guid EdiProdLink { get; private set; }
        public string Email { get; private set; }

        public bool IsEmptyOrInvalid
        {
            get { return string.IsNullOrEmpty(Email); }
        }
    }
}
