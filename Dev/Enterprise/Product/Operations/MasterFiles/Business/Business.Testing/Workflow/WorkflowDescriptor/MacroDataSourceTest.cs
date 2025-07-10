using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class MacroDataSourceTest : TestCaseWithFactory
	{
		public void TestHasDataSourcePrefix()
		{
			AssertHasDataSourcePrefix("ZZZZ", false, "", "ZZZZ");
			AssertHasDataSourcePrefix("_DataSource123", false, "", "_DataSource123");
			AssertHasDataSourcePrefix("_DataSource", true, "", "_DataSource");
			AssertHasDataSourcePrefix("_DataSource.ZZ", true, "ZZ", "");
			AssertHasDataSourcePrefix("_DataSource.ZZ.XX", true, "ZZ", "XX");
			AssertHasDataSourcePrefix("_DataSource.ZZ.XX.YY", true, "ZZ", "XX.YY");
		}

		void AssertHasDataSourcePrefix(ZString fieldPath, bool expectedHasPrefix, ZString expectedDataSourceType, ZString expectedfiedlName)
		{
			var hasPrefix = MacroDataSource.HasDataSourcePrefix(fieldPath, out ZString dataSourceType, out ZString fiedlName);
			AssertEquals(expectedHasPrefix, hasPrefix);
			AssertEquals(expectedDataSourceType, dataSourceType);
			AssertEquals(expectedfiedlName, fiedlName);
		}

		public void TestGetDataSourceError()
		{
			var declarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var shipmentTpye = ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var parentTypes = new Type[] { declarationType, shipmentTpye };
			AssertGetDataSourceTypeError("_DataSource", parentTypes, null, "_DataSource", MacroDataSource.EmptyDataSourceTypeError);
			AssertGetDataSourceTypeError("<_DataSource>", parentTypes, null, "_DataSource", MacroDataSource.EmptyDataSourceTypeError);
			AssertGetDataSourceTypeError("<._DataSource>", parentTypes, null, "._DataSource", "");
			AssertGetDataSourceTypeError("<_DataSource.XX>", parentTypes, null, "", MacroDataSource.GetDataSourceTypeNotFoundMessage("XX"));
			AssertGetDataSourceTypeError("<_DataSource.JobDeclaration.XX>", parentTypes, declarationType, "XX", "");
			AssertGetDataSourceTypeError("Z0_WrongField", parentTypes, null, "Z0_WrongField", "");

			parentTypes = new Type[] { shipmentTpye };
			AssertGetDataSourceTypeError("<_DataSource.JobDeclaration.XX>", parentTypes, null, "XX", MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));
		}

		void AssertGetDataSourceTypeError(ZString fieldPath, Type[] parentTypes, Type expectedType, ZString expectedfiedlName, ZString expectedMessage)
		{
			var error = MacroDataSource.GetDataSourceError(fieldPath, parentTypes, out Type type, out ZString fiedlName);
			AssertEquals(expectedType, type);
			AssertEquals(expectedfiedlName, fiedlName);
			AssertEquals(expectedMessage, error);
		}
	}
}
