using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EPaymentReason))]
	sealed class EPaymentReasonTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new EPaymentReason
			{
				ProviderCode = EPaymentProviderCodes.Codes.OFX,
				ReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade,
				ReasonDescription = EPaymentReasonCodes.OFXReasonCodes.CodesList.GetDescriptionFromCode(EPaymentReasonCodes.OFXReasonCodes.ServicesTrade)
			};
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject() => GetBusinessObjectToClone();

		#endregion
	}
}
