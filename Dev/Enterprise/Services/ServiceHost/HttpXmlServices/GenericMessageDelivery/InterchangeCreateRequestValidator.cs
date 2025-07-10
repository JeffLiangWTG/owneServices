using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.GMD
{
	sealed class InterchangeCreateRequestValidator
	{
		public InterchangeCreateRequestValidator(string[] supportedInterchangeTypes, Interchange interchange)
		{
			this.supportedInterchangeTypes = Argument.NotNull(supportedInterchangeTypes?.ToHashSet(), nameof(supportedInterchangeTypes));
			this.interchange = Argument.NotNull(interchange, nameof(interchange));
		}

		readonly Interchange interchange;
		readonly HashSet<string> supportedInterchangeTypes;

		public bool IsValid(out string message, out GlbBranch branch)
		{
			branch = null;
			var interchangeType = interchange.InterchangeType?.ToUpperInvariant();
			if (!supportedInterchangeTypes.Contains(interchangeType))
			{
				message = $"Interchange Type '{interchangeType}' is not supported";
				return false;
			}

			var recipientId = interchange.RecipientId;
			if (string.IsNullOrWhiteSpace(recipientId))
			{
				message = (NoResString)"Recipient Id is missing";
				return false;
			}

			if (recipientId.Length != 9)
			{
				message = (NoResString)"Recipient Id should be a nine character code ({enterprise code}{company code}{server code}) identifying a company on a database";
				return false;
			}
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			if (!registrationKey.EnterpriseCode.Equals(recipientId.Substring(0, 3), StringComparison.InvariantCultureIgnoreCase))
			{
				message = (NoResString)"Recipient Id's enterprise code portion does not match product enterprise code";
				return false;
			}
			if (!registrationKey.ServerCode.Equals(recipientId.Substring(6, 3), StringComparison.InvariantCultureIgnoreCase))
			{
				message = (NoResString)"Recipient Id's server code portion does not match product server code";
				return false;
			}
			var factory = new BusinessObjectFactory();
			var companyCode = recipientId.Substring(3, 3).ToUpperInvariant();
			var company = FindCompany(factory, companyCode);

			if (company == null)
			{
				message = $"Unable to find active company using code '{companyCode}'.";
				return false;
			}
			branch = company.ActiveBranches.FirstOrDefault();
			if (branch == null)
			{
				message = $"Unable to find active branch for company '{companyCode}'.";
				return false;
			}
			message = null;
			return true;
		}

		GlbCompany FindCompany(BusinessObjectFactory factory, string companyCode)
		{
			var query = new ZQuery(GlbCompanySchema.GC_Code, companyCode);
			query.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			return factory.LoadTop1<GlbCompany>(query);
		}
	}
}
