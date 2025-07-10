using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedureAttribute.Loader))]
	class RefCusProcedureAttributeLoaderTest : LoaderTestCase
	{
		public void TestLoadAttributesList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "00", "0C9", "", "IMP");
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "40", "00", "0C8", "", "IMP");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "01");
			helper.CreateRefCusProcedureAttribute(procedure1.PK, AttributeNames.Codes.TAXFLAG, "02");
			helper.CreateRefCusProcedureAttribute(procedure2.PK, AttributeNames.Codes.TAXFLAG, "03");
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Test 1", new ZString[] { "01", "02" }, RefCusProcedureAttribute.Loader.LoadAttributesList(Factory, "40", "00", "0C9", Core.Constants.CountryCodes.Germany, "IMP", AttributeNames.Codes.TAXFLAG));
				AssertArrayEqualsByElements("Test 2", new ZString[] { "03" }, RefCusProcedureAttribute.Loader.LoadAttributesList(Factory, "40", "00", "0C8", Core.Constants.CountryCodes.Germany, "IMP", AttributeNames.Codes.TAXFLAG));
				AssertArrayEqualsByElements("Test 3", Array.Empty<ZString>(), RefCusProcedureAttribute.Loader.LoadAttributesList(Factory, "40", "00", "0C7", Core.Constants.CountryCodes.Germany, "IMP", AttributeNames.Codes.TAXFLAG));
			}

			);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusProcedureAttribute.Loader(Factory);
		}
	}
}
