using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IClientRegistrationAccessor
    {
		void UpdateFlag1(string clientId, string registrationType, string qualifier, string code, int flag);
		void UpdateConfigXml(string clientId, string registrationType, string qualifier, string code, string xmlString);
		void Insert(string clientId, string registrationType, string qualifier, string code, int flag1, string xmlString, string attr1, string password1, DateTime? issuedUTC = null, DateTime? expiryUTC = null);
		void UpdateFlag1AndConfigXml(string clientId, string registrationType, string qualifier,
			int flag, string xmlString);
		bool Exists(string clientId, string registrationType, string qualifier);
		void Delete(string clientId, string registrationType, string qualifier);

		IEnumerable<SqlDataReader> ReadRegistrations(SqlTransaction transaction, string client, string registrationType);
		IEnumerable<Dictionary<string, object>> ReadRegistrations(SqlTransaction transaction, string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null);
		IEnumerable<Dictionary<string, object>> ReadRegistrations(string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null);
		IEnumerable<Dictionary<string, object>> ReadRegistrations(string client, string registrationType, bool useLike, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null);
		void UpdateRegistrationCode(string value, SqlTransaction transaction, string client, string registrationType, object qualifier);
		string[] ReadAttr1Password1FirstOrDefault(string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null);
	}
}
