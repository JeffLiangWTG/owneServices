using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(PackageVersionOverride))]
	class PackageVersionOverrideTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestPackageNameValidation()
		{
			var packageVersionOverride = new PackageVersionOverride();
			AsssertMandatory((value) => packageVersionOverride.PackageName = value,
				packageVersionOverride.PackageNameInfo, "Please enter a Package Name.");
		}

		public void TestPackageVersionValidation()
		{
			var packageVersionOverride = new PackageVersionOverride();
			AsssertMandatory((value) => packageVersionOverride.PackageVersion = value,
				packageVersionOverride.PackageVersionInfo, "Please enter a Package Version.");
		}

		public void TestUniqueKey()
		{
			string message = $"Only one package and version for a carrier can be overridden per account number.";
			var packageVersionOverrideCollection = new PackageVersionOverrideCollection();
			var packageVersionOverride1 = packageVersionOverrideCollection.AddNew();
			var packageVersionOverride2 = packageVersionOverrideCollection.AddNew();
			var packageVersionOverride3 = packageVersionOverrideCollection.AddNew();
			var packageVersionOverride4 = packageVersionOverrideCollection.AddNew();
			var packageVersionOverride5 = packageVersionOverrideCollection.AddNew();

			AssertEquals("Precondition: UniqueKey should have no errors", expected: false, packageVersionOverride1.UniqueKeyInfo.HasErrors());
			AssertEquals("Precondition: UniqueKey should have no errors", expected: false, packageVersionOverride2.UniqueKeyInfo.HasErrors());
			AssertEquals("Precondition: UniqueKey should have no errors", expected: false, packageVersionOverride3.UniqueKeyInfo.HasErrors());
			AssertEquals("Precondition: UniqueKey should have no errors", expected: false, packageVersionOverride4.UniqueKeyInfo.HasErrors());
			AssertEquals("Precondition: UniqueKey should have no errors", expected: false, packageVersionOverride5.UniqueKeyInfo.HasErrors());

			packageVersionOverride1.Code = "ABC";
			packageVersionOverride1.AccountNumber = "1234";
			packageVersionOverride2.Code = "ABC";
			packageVersionOverride2.AccountNumber = "1234";
			packageVersionOverride3.Code = "XYZ";
			packageVersionOverride4.Code = "XYZ";
			packageVersionOverride5.Code = "PQR";

			AssertEquals("UniqueKey should not have errors", expected: false, packageVersionOverride1.UniqueKeyInfo.HasError(message));
			AssertEquals("UniqueKey should have an error because it is not unique", expected: true, packageVersionOverride2.UniqueKeyInfo.HasError(message));
			AssertEquals("UniqueKey should not have errors", expected: false, packageVersionOverride3.UniqueKeyInfo.HasError(message));
			AssertEquals("UniqueKey should have an error because it is not unique", expected: true, packageVersionOverride4.UniqueKeyInfo.HasError(message));
			AssertEquals("UniqueKey should not have errors", expected: false, packageVersionOverride5.UniqueKeyInfo.HasError(message));

			packageVersionOverride2.Code = "POP";
			packageVersionOverride4.AccountNumber = "1234";
			AssertEquals("UniqueKey should not have errors", expected: false, packageVersionOverride2.UniqueKeyInfo.HasError(message));
			AssertEquals("UniqueKey should not have errors", expected: false, packageVersionOverride4.UniqueKeyInfo.HasError(message));
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override bool IsCodeUniqueInCollection => false;

		protected override string CodeDisplayName => "Carrier Code";

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObject();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObject();

		RegistryBusinessObjectTemplate GetBusinessObject()
		{
			var packageVersionOverride = new PackageVersionOverride();
			packageVersionOverride.Code = "ZZDUM";
			packageVersionOverride.PackageName = "DummyPackage";
			packageVersionOverride.PackageVersion = "1.0.1";
			packageVersionOverride.AccountNumber = "1248y319DX";

			return packageVersionOverride;
		}

		static void AsssertMandatory(Action<string> setProperty, ZPropertyInfo propertyInfo, string errorMessage)
		{
			AssertNoErrors(propertyInfo);

			setProperty("dummy");
			AssertNoErrors(propertyInfo);

			setProperty(string.Empty);
			AssertHasError(propertyInfo, errorMessage);
		}

		#endregion
	}
}
