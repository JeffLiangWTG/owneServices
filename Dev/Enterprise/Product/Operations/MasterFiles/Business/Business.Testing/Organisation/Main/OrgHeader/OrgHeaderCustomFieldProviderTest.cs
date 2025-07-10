using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderCustomFieldProviderTest : TestCaseWithFactory
	{
		[TestedType(typeof(OrgHeader))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestCustomFieldsReadOnlyIfNotAllowedInSecurity()
		{
			Env.Security.OrgDetailsModifyCustomFields.IsAllowed = false;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			ZDateTime dt = ZDateTime.Now;
			org.SetUserDefinedValue("Custom1", null, (ZString)"Hello World");
			org.SetUserDefinedValue("Custom2", null, dt);

			Factory.Save();

			AssertEquals(((ICustomFieldProvider)org).GetCustomBusinessObject().ReadOnly, true);
		}
		public void TestCustomFieldsEditableIfAllowedInSecurity()
		{
			Env.Security.OrgDetailsModifyCustomFields.IsAllowed = true;

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			ZDateTime dt = ZDateTime.Now;
			org.SetUserDefinedValue("Custom1", null, (ZString)"Hello World");
			org.SetUserDefinedValue("Custom2", null, dt);

			Factory.Save();

			AssertEquals(((ICustomFieldProvider)org).GetCustomBusinessObject().ReadOnly, false);
		}
	}
}
