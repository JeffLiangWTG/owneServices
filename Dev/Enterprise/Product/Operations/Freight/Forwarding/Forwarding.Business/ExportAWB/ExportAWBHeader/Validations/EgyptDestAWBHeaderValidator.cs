using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class EgyptDestAWBHeaderValidator : AutoExportAWBHeaderValidation, IAWBHeaderValidator
	{
		public EgyptDestAWBHeaderValidator(ExportAWBHeader parent) : base(parent)
		{
		}

		public new ExportAWBHeader Parent
		{
			get { return (ExportAWBHeader)base.Parent; }
		}

		public bool IsApplicable()
		{
			return Parent.DestinationCountryCode == Core.Constants.CountryCodes.Egypt;
		}

		#region EH_HandlingInformation

		protected override void CheckEH_HandlingInformation()
		{
			if (Parent.IsHAWB)
			{
				CheckACIDNumber();
			}
			if (Parent.IsMAWB)
			{
				if (Parent.IsAWBOverridden)
				{
					CheckACIDNumber();
				}
				else if (Parent.Consol != null && !HasACIDNumber())
				{
					Parent.EH_HandlingInformationInfo.AddWarning(egyptACIDNumberRequiredMessage);
				}
			}
		}

		bool HasACIDNumber()
		{
			return Parent.Consol.Numbers.Cast<CusEntryNumber>()
				.Any(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Egypt && c.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference);
		}

		void CheckACIDNumber()
		{
			if (!Parent.EH_HandlingInformation.Contains((NoResString)"ACID Number:", System.StringComparison.InvariantCultureIgnoreCase))
			{
				Parent.EH_HandlingInformationInfo.AddWarning(egyptACIDNumberRequiredMessage);
			}
		}

		string egyptACIDNumberRequiredMessage =>
			Parent is ShipmentExportAWBHeader ? Res.GetString("99e3f0ad-5ce0-423e-8fa5-53de95f3d94f", "The Shipment's ACID number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the ACI in the Shipment's Reference Numbers grid.")
			: Res.GetString("3f63c234-6397-4e6c-b82e-cddcfa583504", "The ACID number is required to comply with ACI (Advanced Cargo Information) reporting for cargo destined to Egypt. Enter the ACI in the Reference Numbers grid.");

		#endregion
	}
}
