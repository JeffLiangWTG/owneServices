using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword))]
	sealed class GlbExternalPasswordTest : GlbExternalPasswordBase_TWTest<GlbExternalPassword>
	{
		protected override ZString InterchangeType => EDIInterchangeTypeList.Codes.Configuration;

		[ExpectNoExceptions]
		public override void TestSetDefaultValues()
		{
			var password = GlbExternalPassword;
			NUnit.Framework.Assert.That(password.GP_PasswordType, NUnit.Framework.Is.EqualTo(PasswordTypesList.Codes.TVA).Using(CustomComparers.TypeComparison), "GP_PasswordType");
			NUnit.Framework.Assert.That(password.GP_PasswordStatus, NUnit.Framework.Is.EqualTo(PasswordStatusList.Codes.Invalid).Using(CustomComparers.TypeComparison), "GP_PasswordStatus");
			NUnit.Framework.Assert.That(password.GP_GC, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.PK), "GP_GC");
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			NUnit.Framework.Assert.That(GlbExternalPassword.Lookups, NUnit.Framework.Is.TypeOf<GlbExternalPasswordLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			NUnit.Framework.Assert.That(GlbExternalPassword.Validation, NUnit.Framework.Is.TypeOf<GlbExternalPasswordValidation>());
		}
	}
}
