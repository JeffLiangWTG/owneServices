using System;
using System.Collections.Generic;
using System.Linq;
using eServices.Configuration.Schemas;
using CargoWise.eServices.Encryption.Server.Decryptor;


#if NET48
using eServices.eHubDataModel.eHubTransactions;
#else
using Microsoft.EntityFrameworkCore;
using eServices.eHubDataModel.eHubTransactionsCore;
#endif

namespace eServices.Configuration.Helper
{
	public class ConfigurationValidationHelper
	{
		readonly eHubTransactionsContext dbContext;
		public ConfigurationValidationHelper(eHubTransactionsContext dbContext)
		{
#if NET48
			this.dbContext = dbContext ?? new eHubTransactionsContext();
#else
			this.dbContext = dbContext ?? new eHubTransactionsContext(new DbContextOptions<eHubTransactionsContext>());
#endif
		}

		public List<string> ValidateCredential(Group group)
		{
			var errors = new List<string>();

			if (group.CredentialItems != null)
			{
				foreach (var credential in group.CredentialItems)
				{
					// If Credential is empty: <Credential />, we will use db value.
					if (credential.UserName == null && credential.Password == null)
					{
						continue;
					}

					if (string.IsNullOrEmpty(credential.UserName))
					{
						errors.Add(string.Format("UserName in {0}'s credential must not be empty.", credential.Name));
						continue;
					}

					if (credential.Password == null || credential.Password.Length == 0)
					{
						errors.Add(string.Format("Password in {0}'s credential must not be empty.", credential.Name));
						continue;
					}

					try
					{
						EhubServerDecryptor.DecryptBinary(credential.Password);
					}
					catch (Exception e)
					{
						errors.Add(string.Format("Failed to decrypt {0}'s password. Error: {1}", credential.Name, e.Message));
					}
				}
			}

			return errors;
		}

		public List<string> ValidateSystemGroup(Group systemGroup)
		{
			var errors = new List<string>();
			if (string.IsNullOrEmpty(systemGroup.Reference) || systemGroup.Reference.Length != 6)
			{
				errors.Add(string.Format("Invalid System group's reference: {0}", systemGroup.Reference));
			}
#if NET48
			else if (!dbContext.eHubClientSystems.Any(x => x.EH_ID == systemGroup.Reference))
#else
			else if (!dbContext.eHubClientSystem.Any(x => x.EH_ID == systemGroup.Reference))
#endif
			{
				errors.Add(string.Format("Client system '{0}' doesn't exist.", systemGroup.Reference));
			}

			return errors;
		}

		public List<string> ValidateCompanyGroup(string systemId, Group companyGroup)
		{
			var errors = new List<string>();
			if (string.IsNullOrEmpty(systemId) || systemId.Length != 6)
			{
				errors.Add(string.Format("Invalid System Id: {0}", systemId));
			}
			else if (string.IsNullOrEmpty(companyGroup.Reference) || companyGroup.Reference.Length != 3)
			{
				errors.Add(string.Format("Invalid Company group's reference: '{0}'.", companyGroup.Reference));
			}
			else
			{
				var clientId = systemId.Substring(0, 3) + companyGroup.Reference + systemId.Substring(3);

#if NET48
				if (!dbContext.eHubClients.Any(x => x.CC_ID == clientId))
#else
				if (!dbContext.eHubClient.Any(x => x.CC_ID == clientId))
#endif
				{
					errors.Add(string.Format("Client '{0}' doesn't exist.", clientId));
				}
			}

			return errors;
		}
	}
}
