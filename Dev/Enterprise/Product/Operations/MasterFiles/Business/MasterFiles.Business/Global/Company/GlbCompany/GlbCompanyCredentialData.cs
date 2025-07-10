using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyCredentialData
	{
		public GlbCompanyCredentialData(string configurationName, string interchangeTypeForSending, ZPropertyInfo[] credentialApplicableInfos, Func<GlbCompany, object> getCredential)
		{
			ConfigurationName = Argument.NotNullOrEmpty(configurationName, nameof(configurationName));
			InterchangeTypeForSending = Argument.NotNullOrEmpty(interchangeTypeForSending, nameof(interchangeTypeForSending));
			CredentialApplicableInfos = Argument.NotNull(credentialApplicableInfos, nameof(credentialApplicableInfos));
			CreateCredential = Argument.NotNull(getCredential, nameof(getCredential));

			if (CredentialApplicableInfos.Length == 0)
			{
				throw new ArgumentException("CredentialApplicableInfos cannot be empty.", nameof(credentialApplicableInfos));
			}
		}

		public ZString ConfigurationName { get; }
		public ZString InterchangeTypeForSending { get; }
		public ZPropertyInfo[] CredentialApplicableInfos { get; }
		public Func<GlbCompany, object> CreateCredential { get; }
	}
}
