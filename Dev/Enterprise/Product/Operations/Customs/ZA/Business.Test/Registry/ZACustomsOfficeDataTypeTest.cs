using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(ZACustomsOfficeDataType))]
	sealed class ZACustomsOfficeDataTypeTest : GuidRegistryDataTypeTest
	{
		public void TestValidateCustomsOfficeCode()
		{
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var codeType = helper.CreateCusCodeType("CUSOF", "CUSOF");
			var code1 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.SouthAfrica, codeType.ZZK_CodeType, "AAA", "AAA DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(Enterprise.Core.Constants.CountryCodes.SouthAfrica, codeType.ZZK_CodeType, "BBBBB", "BBBBB DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
			var customsCodeCode1 = ZACustomsRegistry.Instance.CustomsOfficeCode;
			AssertNoExceptionThrown(() =>
			{
				customsCodeCode1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code1.PK.ToGuid());
			});
			AssertExceptionThrown(typeof(RegistryValidationException), string.Format("Customs Office Code should not be longer than {0}.", JobDeclaration.Schema.JE_CustomsOfficeMaxLength), () =>
			{
				customsCodeCode1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, code2.PK.ToGuid());
			});
		}

		protected override GuidRegistryDataType GetNewDataType()
		{
			return new ZACustomsOfficeDataType();
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}
	}
}
