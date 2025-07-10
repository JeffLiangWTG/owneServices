using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolExportAWBOtherChargesValidation : ExportAWBOtherChargesValidation
	{
		public ConsolExportAWBOtherChargesValidation(ConsolExportAWBOtherCharges parent)
			: base(parent)
		{
		}

		public new ConsolExportAWBOtherCharges Parent
		{
			get { return (ConsolExportAWBOtherCharges)base.Parent; }
		}

		protected override void CheckEO_ChargeCode()
		{
			base.CheckEO_ChargeCode();
			CheckNoDuplicateChargeCodeAndEntitlementCodeCombinations(Parent.EO_ChargeCodeInfo);
		}

		protected override void CheckEO_EntitlementCode()
		{
			base.CheckEO_EntitlementCode();
			CheckNoDuplicateChargeCodeAndEntitlementCodeCombinations(Parent.EO_EntitlementCodeInfo);
		}

		void CheckNoDuplicateChargeCodeAndEntitlementCodeCombinations(ZPropertyInfo propertyInfo)
		{
			if (Parent.Master != null)
			{
				if (Parent.Master.AWBOtherCharges
					.Cast<ConsolExportAWBOtherCharges>()
					.Any((c) => c.EO_ChargeCode == Parent.EO_ChargeCode && c.EO_EntitlementCode == Parent.EO_EntitlementCode && c.PK != Parent.PK))
				{
					propertyInfo.AddNotification(Parent.Master.Validation.NotificationLevelForFieldsRequiredForElectronicTransmission,
						Res.GetString("c401bd6e-0420-4053-a76b-f663d6f19081", "Duplicate combinations of Other Charge Code and Entitlement Code are not permitted when sending AWB electronically. Other charges can be grouped by switching the following registry setting to YES: Freight > AWB > MAWB > Other Charges > Group By IATA Code."));
				}

				foreach (var charge in Parent.Master.AWBOtherCharges)
				{
					if (charge.PK != Parent.PK)
					{
						charge.Validation.ValidateEO_ChargeCode();
						charge.Validation.ValidateEO_EntitlementCode();
					}
				}
			}
		}
	}
}
