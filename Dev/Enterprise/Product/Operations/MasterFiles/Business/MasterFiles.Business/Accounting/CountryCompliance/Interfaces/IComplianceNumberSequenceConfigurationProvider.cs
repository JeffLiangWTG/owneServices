using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IComplianceNumberSequenceConfigurationProvider
	{
		/// <summary>
		/// Returns a list of mandatory configurations used in the registry ComplianceNumberSequenceConfiguration.
		/// </summary>
		ComplianceNumberSequenceConfiguration[] GetMandatoryComplianceNumberSequenceConfiguration();

		/// <summary>
		/// Set a configuration as default and mandatory in the compliance sequence
		/// </summary>
		CodeDescriptionPair DefaultMandatoryComplianceNumberSequenceConfigurationCode { get; }

		/// <summary>
		/// Returns a ZQuery filter used to find the AccComplianceSequence using AH_transactionReference
		/// </summary>
		ZQuery GetComplianceSequenceFromTransactionReferenceFilter(ZString complianceSubType, ZString transactionReference, ZGuid companyPK);

		/// <summary>
		/// Extract the sequence number from AH_Transactionreference using the format from the default mandatory configuration
		/// </summary>
		string GetSequenceNumber(string transactionReference);
	}
}
