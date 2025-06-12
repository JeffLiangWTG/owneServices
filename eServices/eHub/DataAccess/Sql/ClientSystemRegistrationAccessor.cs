using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	public class ClientSystemRegistrationAccessor : IClientSystemRegistrationAccessor
	{
		public ClientSystemRegistrationAccessor() : this(new eServices.eHubDataAccess.Sql.ClientSystemRegistrationAccessor()) { }

		public ClientSystemRegistrationAccessor(eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor clientSystemRegistrationAccessor)
		{
			this.clientSystemRegistrationAccessor = clientSystemRegistrationAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IClientSystemRegistrationAccessor clientSystemRegistrationAccessor;

		public void UpdateSystemRegistrationFlag1(string systemId, string registrationType, string qualifier, string code, int flag)
			=> clientSystemRegistrationAccessor.UpdateSystemRegistrationFlag1(systemId, registrationType, qualifier, code, flag);

		public void UpdateSystemRegistrationConfigXml(string systemId, string registrationType, string qualifier, string code, string xmlString)
			=> clientSystemRegistrationAccessor.UpdateSystemRegistrationConfigXml(systemId, registrationType, qualifier, code, xmlString);

		public int GetSystemRegistrationFlag1(string systemId, string registrationType, string qualifier, string code)
			=> clientSystemRegistrationAccessor.GetSystemRegistrationFlag1(systemId, registrationType, qualifier, code);

		public XmlDocument GetSystemRegistrationConfigXml(string systemId, string registrationType, string qualifier, string code)
			=> clientSystemRegistrationAccessor.GetSystemRegistrationConfigXml(systemId, registrationType, qualifier, code);

		public void InsertSystemRegistration(string systemId, string registrationType, string qualifier, string code, int flag, string xmlString, string attr1, string attr2, DateTime issuedUTC, DateTime expiryUTC)
			=> clientSystemRegistrationAccessor.InsertSystemRegistration(systemId, registrationType, qualifier, code, flag, xmlString, attr1, attr2, issuedUTC, expiryUTC);

		public void UpdateSystemRegistrationFlag1AndConfigXml(string systemId, string registrationType, string qualifier, string code, int flag, string xmlString)
			=> clientSystemRegistrationAccessor.UpdateSystemRegistrationFlag1AndConfigXml(systemId, registrationType, qualifier, code, flag, xmlString);

		public bool DoesSystemRegistrationExist(string systemId, string registrationType, string qualifier, string code)
			=> clientSystemRegistrationAccessor.DoesSystemRegistrationExist(systemId, registrationType, qualifier, code);

		public IEnumerable<string> GetSystemRegistrationsCode(string systemId, string registrationType, string qualifier)
			=> clientSystemRegistrationAccessor.GetSystemRegistrationsCode(systemId, registrationType, qualifier);

		public void DeleteSystemRegistration(string systemId, string registrationType, string qualifier, string code)
			=> clientSystemRegistrationAccessor.DeleteSystemRegistration(systemId, registrationType, qualifier, code);
	}
}
