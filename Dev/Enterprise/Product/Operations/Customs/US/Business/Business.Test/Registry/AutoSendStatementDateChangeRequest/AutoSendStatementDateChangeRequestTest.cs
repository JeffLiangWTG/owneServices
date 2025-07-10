using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoSendStatementDateChangeRequest))]
	sealed class AutoSendStatementDateChangeRequestTest : RegistryBusinessObjectTemplateTestCase<AutoSendStatementDateChangeRequest>
	{
		public void TestOverrideAllOrByOrganisationAndValidation()
		{
			AutoSendStatementDateChangeRequest obj = new AutoSendStatementDateChangeRequest();
			obj.OverrideAllOrByOrganisation = OverrideAllOrByOrganisationList.Codes.ALL;
			AssertEquals("OverrideAllOrByOrganisation setter OK", "ALL", obj.OverrideAllOrByOrganisation);

			obj.OverrideAllOrByOrganisation = "XXX";
			AssertHasError(obj.OverrideAllOrByOrganisationInfo, AutoSendStatementDateChangeRequest.OverrideValues);

			obj.OverrideAllOrByOrganisation = OverrideAllOrByOrganisationList.Codes.ORG;
			AssertNoError(obj.OverrideAllOrByOrganisationInfo, AutoSendStatementDateChangeRequest.OverrideValues);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override AutoSendStatementDateChangeRequest GetBusinessObjectToClone()
		{
			AutoSendStatementDateChangeRequest result = new AutoSendStatementDateChangeRequest();
			result.OverrideAllOrByOrganisation = ZString.Empty;
			return result;
		}

		protected override AutoSendStatementDateChangeRequest GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
