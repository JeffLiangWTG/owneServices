using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	public class ClientRegistrationAccessor : IClientRegistrationAccessor
	{
		public ClientRegistrationAccessor() : this(new eServices.eHubDataAccess.Sql.ClientRegistrationAccessor()) { }

		public ClientRegistrationAccessor(eServices.eHubDataAccess.Integration.IClientRegistrationAccessor clientRegistrationAccessor)
		{
			this.clientRegistrationAccessor = clientRegistrationAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IClientRegistrationAccessor clientRegistrationAccessor;

		public void UpdateRegistrationCode(string value, SqlTransaction transaction, string client, string registrationType, object qualifier)
			=> clientRegistrationAccessor.UpdateRegistrationCode(value, transaction, client, registrationType, qualifier);

		public IEnumerable<SqlDataReader> ReadRegistrations(SqlTransaction transaction, string client, string registrationType)
			=> clientRegistrationAccessor.ReadRegistrations(transaction, client, registrationType);

		public IEnumerable<Dictionary<string, object>> ReadRegistrations(SqlTransaction transaction, string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null)
			=> clientRegistrationAccessor.ReadRegistrations(transaction, client, registrationType, qualifier, code, attr1, password1, flag1, flag2);

		public IEnumerable<Dictionary<string, object>> ReadRegistrations(string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null)
			=> clientRegistrationAccessor.ReadRegistrations(client, registrationType, qualifier, code, attr1, password1, flag1, flag2);

		public IEnumerable<Dictionary<string, object>> ReadRegistrations(string client, string registrationType, bool useLike, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null)
			=> clientRegistrationAccessor.ReadRegistrations(client, registrationType, useLike, qualifier, code, attr1, password1, flag1, flag2);

		public string[] ReadAttr1Password1FirstOrDefault(string client, string registrationType, string qualifier = null, string code = null, string attr1 = null, string password1 = null, int? flag1 = null, int? flag2 = null)
			=> clientRegistrationAccessor.ReadAttr1Password1FirstOrDefault(client, registrationType, qualifier, code, attr1, password1, flag1, flag2);

		public void UpdateFlag1(string clientId, string registrationType, string qualifier, string code, int flag)
			=> clientRegistrationAccessor.UpdateFlag1(clientId, registrationType, qualifier, code, flag);

		public void UpdateConfigXml(string clientId, string registrationType, string qualifier, string code, string xmlString)
			=> clientRegistrationAccessor.UpdateConfigXml(clientId, registrationType, qualifier, code, xmlString);

		public void Insert(string clientId, string registrationType, string qualifier, string code, int flag1, string xmlString, string attr1, string password1, DateTime? issuedUTC = null, DateTime? expiryUTC = null)
			=> clientRegistrationAccessor.Insert(clientId, registrationType, qualifier, code, flag1, xmlString, attr1, password1, issuedUTC, expiryUTC);

		public void UpdateFlag1AndConfigXml(string clientId, string registrationType, string qualifier, int flag, string xmlString)
			=> clientRegistrationAccessor.UpdateFlag1AndConfigXml(clientId, registrationType, qualifier, flag, xmlString);

		public bool Exists(string clientId, string registrationType, string qualifier)
			=> clientRegistrationAccessor.Exists(clientId, registrationType, qualifier);

		public void Delete(string clientId, string registrationType, string qualifier)
			=> clientRegistrationAccessor.Delete(clientId, registrationType, qualifier);
	}
}
