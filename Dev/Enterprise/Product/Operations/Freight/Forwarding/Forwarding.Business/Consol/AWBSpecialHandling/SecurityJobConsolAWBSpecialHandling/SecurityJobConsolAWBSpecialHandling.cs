using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SecurityJobConsolAWBSpecialHandling : JobConsolAWBSpecialHandling
	{
		public SecurityJobConsolAWBSpecialHandling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string NotSecured = "NON";

		public override ZString JKH_Code
		{
			get { return base.JKH_Code; }
			set
			{
				if (base.JKH_Code != value)
				{
					if (RequiresVerificationOfFreight(value))
					{
						if (ParentConsolidation.UserHasVerifiedFreightIsSecure(ParentConsolidation.JK_OverrideWaybillDefaults))
						{
							ParentConsolidation.FreightHasBeenVerifiedAsSecure = true;
						}
						else
						{
							ParentConsolidation.FreightHasBeenVerifiedAsSecure = false;
							if (base.JKH_Code.IsEmpty)
							{
								Delete();
							}

							return;
						}
					}

					base.JKH_Code = value;
				}
			}
		}

		#region Implementation

		bool RequiresVerificationOfFreight(ZString newCode)
		{
			if (newCode == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft)
			{
				var consol = ParentConsolidation;
				if (consol != null && SupplyChainSecurityConfiguration.SCSSupportedCountryList.Any(countryCode =>
					consol.JK_RL_NKLoadPort.StartsWith(countryCode, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty, countryCode, StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}
			}
			return false;
		}

		ForwardingConsol ParentConsolidation => Consol;

		#endregion
	}
}
