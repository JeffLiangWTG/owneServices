using System;
using System.Collections.Generic;
using System.Xml;

namespace CargoWise.eHub.DataAccess.Integration
{
    public interface IClientSystemRegistrationAccessor
    {
		void UpdateSystemRegistrationFlag1(string systemId, string registrationType, string qualifier, string code, int flag);
		void UpdateSystemRegistrationConfigXml(string systemId, string registrationType, string qualifier, string code, string xmlString);
		int GetSystemRegistrationFlag1(string systemId, string registrationType, string qualifier, string code);
		XmlDocument GetSystemRegistrationConfigXml(string systemId, string registrationType, string qualifier, string code);
		void InsertSystemRegistration(string systemId, string registrationType, string qualifier, string code, int flag, string xmlDoc, string attr1, string attr2, DateTime issuedUTC, DateTime expiryUTC);
		void UpdateSystemRegistrationFlag1AndConfigXml(string systemId, string registrationType, string qualifier,
			string code, int flag, string xmlString);
		bool DoesSystemRegistrationExist(string systemId, string registrationType, string qualifier, string code);
		IEnumerable<string> GetSystemRegistrationsCode(string systemId, string registrationType, string qualifier);
		void DeleteSystemRegistration(string systemId, string registrationType, string qualifier, string code);
    }
}
