using CargoWise.ComponentModel;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Messaging.Testing.Shared
{
	sealed class CustomAttributeExtensionsTest : TestCase
	{
		interface ITestInterface
		{
			[DecimalPlaces(5)]
			ZDecimal PropertyWithInterfaceLevelAttribute { get; }

			[DecimalPlaces(4)]
			ZDecimal PropertyWithBothLevelAttribute { get; }

			ZDecimal PropertyWithoutAttribute { get; }

			ZDecimal PropertyWithClassLevelAttribute { get; }

			[MaxLength(5)]
			ZString PropertyZStringWithInterfaceLevelAttribute { get; }

			[MaxLength(4)]
			ZString PropertyZStringWithBothLevelAttribute { get; }

			ZString PropertyZStringWithoutAttribute { get; }

			ZString PropertyZStringWithClassLevelAttribute { get; }
		}

		interface IInterfaceForMultipleInterfacesTesting
		{
			ZDecimal PropertyForMultipleInterfacesTesting { get; }
		}

		class TestClass : ITestInterface, IInterfaceForMultipleInterfacesTesting
		{
			public ZDecimal PropertyWithInterfaceLevelAttribute => 1.33333333m;

			[DecimalPlaces(3)]
			public ZDecimal PropertyWithBothLevelAttribute => 1.33333333m;

			public ZDecimal PropertyWithoutAttribute => 1.33333333m;

			[DecimalPlaces(2)]
			public ZDecimal PropertyWithClassLevelAttribute => 1.33333333m;

			public ZString PropertyZStringWithInterfaceLevelAttribute => "AAAAAAAAAA";

			ZDecimal IInterfaceForMultipleInterfacesTesting.PropertyForMultipleInterfacesTesting => 2.444m;

			[MaxLength(3)]
			public ZString PropertyZStringWithBothLevelAttribute => "AAAAAAAAAA";

			public ZString PropertyZStringWithoutAttribute => "AAAAAAAAAA";

			[MaxLength(2)]
			public ZString PropertyZStringWithClassLevelAttribute => "AAAAAAAAAA";
		}

		[ExpectNoExceptions]
		public void TestGetDecimalValueByPropertyWithDecimalPlacesAttribute()
		{
			NUnit.Framework.Assert.That(testInst.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(testInst.PropertyWithInterfaceLevelAttribute)), NUnit.Framework.Is.EqualTo(1.33333m).Using(CustomComparers.TypeComparison), "PropertyWithInterfaceLevelAttribute");
			NUnit.Framework.Assert.That(testInst.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(testInst.PropertyWithBothLevelAttribute)), NUnit.Framework.Is.EqualTo(1.333m).Using(CustomComparers.TypeComparison), "PropertyWithBothLevelAttribute");
			NUnit.Framework.Assert.That(testInst.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(testInst.PropertyWithoutAttribute)), NUnit.Framework.Is.EqualTo(1.33333333m).Using(CustomComparers.TypeComparison), "PropertyWithoutAttribute");
			NUnit.Framework.Assert.That(testInst.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(testInst.PropertyWithClassLevelAttribute)), NUnit.Framework.Is.EqualTo(1.33m).Using(CustomComparers.TypeComparison), "PropertyWithClassLevelAttribute");

			var testMulti = (IInterfaceForMultipleInterfacesTesting)testInst;
			NUnit.Framework.Assert.That(testMulti.GetDecimalValueByPropertyWithDecimalPlacesAttribute(nameof(testMulti.PropertyForMultipleInterfacesTesting)), NUnit.Framework.Is.EqualTo(2.444m).Using(CustomComparers.TypeComparison), "PropertyForMultipleInterfacesTesting");
		}

		[ExpectNoExceptions]
		public void TestGetStringValueByPropertyWithMaxLengthAttribute()
		{
			NUnit.Framework.Assert.That(testInst.GetStringValueByPropertyWithMaxLengthAttribute(nameof(testInst.PropertyZStringWithInterfaceLevelAttribute)), NUnit.Framework.Is.EqualTo("AAAAA").Using(CustomComparers.TypeComparison), "PropertyZStringWithInterfaceLevelAttribute");
			NUnit.Framework.Assert.That(testInst.GetStringValueByPropertyWithMaxLengthAttribute(nameof(testInst.PropertyZStringWithBothLevelAttribute)), NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "PropertyZStringWithBothLevelAttribute");
			NUnit.Framework.Assert.That(testInst.GetStringValueByPropertyWithMaxLengthAttribute(nameof(testInst.PropertyZStringWithoutAttribute)), NUnit.Framework.Is.EqualTo("AAAAAAAAAA").Using(CustomComparers.TypeComparison), "PropertyZStringWithoutAttribute");
			NUnit.Framework.Assert.That(testInst.GetStringValueByPropertyWithMaxLengthAttribute(nameof(testInst.PropertyZStringWithClassLevelAttribute)), NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "PropertyZStringWithClassLevelAttribute");
		}

		protected override void SetUp()
		{
			base.SetUp();
			testInst = new TestClass();
		}
		ITestInterface testInst;
	}
}
