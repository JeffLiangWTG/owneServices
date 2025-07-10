using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PermitCountrySpecificInstruction_ForTest : PermitCountrySpecificInstruction
	{
		public PermitCountrySpecificInstruction_ForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override AppliesToIndicator GetAppliesToIndicator(ZString permitType, ZString permitSubType)
		{
			switch (permitType)
			{
				case "EPT":
					return AppliesToIndicator.ForceEmpty;
				case "MND":
					return AppliesToIndicator.Mandatory;
				case "OPT":
					return AppliesToIndicator.Optional;
				default:
					return AppliesToIndicator.ForceEmpty;
			}
		}

		public override ZString GetValueFromFieldType(ZString ruleCode)
		{
			return Enum.TryParse(ruleCode, out FieldType fieldType) ? fieldType.ToString() : nameof(FieldType.Text);
		}
	}
}
