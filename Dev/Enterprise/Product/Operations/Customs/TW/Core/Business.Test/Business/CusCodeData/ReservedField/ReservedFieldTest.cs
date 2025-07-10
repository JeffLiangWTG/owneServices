using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class ReservedFieldTest<T> : CusCodeDataTest<T> where T : ReservedField
	{
		[ExpectNoExceptions]
		public void TestValidation()
		{
			var reservedField = (ReservedField)GetNewBusinessObject();
			NUnit.Framework.Assert.That(reservedField.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(ReservedFieldValidation)), "Validation");
		}

		[ExpectNoExceptions]
		public void TestCY_DataAllowWesternEuropeanCharactersOnly()
		{
			var reservedField = (ReservedField)GetNewBusinessObject();
			NUnit.Framework.Assert.That(reservedField.CY_DataAllowWesternEuropeanCharactersOnly, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public virtual void TestSetDefaultValues()
		{
			var reservedField = (ReservedField)GetNewBusinessObject();
			NUnit.Framework.Assert.That(reservedField.CY_Type, NUnit.Framework.Is.EqualTo("DRF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(reservedField.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.ReservedField).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(reservedField.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(reservedField.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(35));
		}

		[ExpectNoExceptions]
		public void TestCaption()
		{
			var reservedField = (ReservedField)GetNewBusinessObject();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(reservedField.CY_CodeInfo, "Code", "The type code of the reserved field.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(reservedField.CY_DataInfo, "Value", "The fields reserved for future use.");
		}
	}
}
