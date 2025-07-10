using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IInsurance
	{
		/// <summary>
		/// Insurance name. 
		/// US: (C/60), Condition: if Hazmat Shipment.
		/// </summary>
		ZString InsuranceName { get; }

		/// <summary>
		/// Insurance policy number. 
		/// US: (C/50), Condition: if Hazmat Shipment.
		/// </summary>
		ZString InsurancePolicyNumber { get; }

		/// <summary>
		/// Insurance year policy issue. 
		/// US: (C/YYYY), Condition: if Hazmat Shipment.
		/// </summary>
		ZInt InsuranceYearPolicyIssue { get; }

		/// <summary>
		/// Insurance Amount in USD. 
		/// US: (C/15), Condition: if Hazmat Shipment.
		/// </summary>
		ZDecimal InsuranceAmount { get; }
	}
}
