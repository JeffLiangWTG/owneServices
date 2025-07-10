using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(OrgRegistrationCodeType))]
	sealed class OrgRegistrationCodeTypeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrgRegistrationCodeTypeSchema()
		{
			AssertEquals("OrgRegistrationCodeType", OrgRegistrationCodeType.Schema.TableName);
			AssertEquals("Type", OrgRegistrationCodeType.Schema.Type);
			AssertEquals("Description", OrgRegistrationCodeType.Schema.Description);
			AssertEquals("Country", OrgRegistrationCodeType.Schema.Country);
			AssertEquals("Primary", OrgRegistrationCodeType.Schema.Primary);
			AssertEquals("Category", OrgRegistrationCodeType.Schema.Category);
			AssertEquals("Local Business Number", OrgRegistrationCodeType.Schema.LocalBusinessNumber);
		}

		public void TestOrgRegistrationCodeTypeColumnsForExcel()
		{
			AssertEquals(100 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.Type).Width);
			AssertEquals(300 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.Description).Width);
			AssertEquals(50 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.Country).Width);
			AssertEquals(50 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.Primary).Width);
			AssertEquals(300 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.Category).Width);
			AssertEquals(300 * 48, OrgRegistrationCodeTypeCollection.ColumnsForExcel.First(c => c.Description == OrgRegistrationCodeType.Schema.LocalBusinessNumber).Width);
		}

		public void TestLocalRegCodeExcelExportColumn()
		{
			var column = new OrgRegistrationCodeTypeExcelExportColumn("James Harden MVP", 123);
			AssertNotNull(column.GetFormat(ZString.Empty));
			AssertEquals("James Harden MVP", column.Description);
			AssertEquals(123, column.Width);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgRegistrationCodeType();
		}
	}
}
