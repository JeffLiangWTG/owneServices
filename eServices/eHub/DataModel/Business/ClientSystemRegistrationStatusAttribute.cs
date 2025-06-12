using System;

namespace CargoWise.eHub.DataModel.Business
{
	public sealed class ClientSystemRegistrationStatusAttribute : Attribute
	{
		public ClientSystemRegistrationStatusAttribute(string registrationType)
		{
			RegistrationType = registrationType;
		}

		public string RegistrationType;
	}
}
