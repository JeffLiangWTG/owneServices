using System.Linq;
using System.Reflection;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PackingLineExtensionTest : TestCase
	{
		public void TestPackingLineExtension_ReferenceNumber()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			AssertEquals("PackageID not exists", false, packingLine.IsLoosePackageIDDataObject());

			packingLine.ReferenceNumber = "";
			AssertEquals("PackageID is empty", false, packingLine.IsLoosePackageIDDataObject());

			packingLine.ReferenceNumber = "P1";
			AssertEquals("PackageID exists", true, packingLine.IsLoosePackageIDDataObject());
		}

		public void TestPackingLineExtension_PackQty()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P1" };
			AssertEquals("PackQty not exists", true, packingLine.IsLoosePackageIDDataObject());

			packingLine.PackQty = 0;
			AssertEquals("PackQty not equals to 1", false, packingLine.IsLoosePackageIDDataObject());
			packingLine.PackQty = 2;
			AssertEquals("PackQty not equals to 1", false, packingLine.IsLoosePackageIDDataObject());
			packingLine.PackQty = -1;
			AssertEquals("PackQty not equals to 1", false, packingLine.IsLoosePackageIDDataObject());

			packingLine.PackQty = 1;
			AssertEquals("PackQty equals to 1", true, packingLine.IsLoosePackageIDDataObject());
		}

		public void TestPackingLineExtension_FieldsOtherThanReferenceNumberAndPackQty()
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ReferenceNumber = "P1", PackQty = 1 };
			var otherProperties = packingLine.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			AssertEquals("PackageID and PackQty are all exist but not other properties.", true, packingLine.IsLoosePackageIDDataObject());
			AssertEquals("Precondition: Other properties are all null.", true, otherProperties.Where(p => p.Name != nameof(packingLine.ReferenceNumber) && p.Name != nameof(packingLine.PackQty)).All(p => p.GetValue(packingLine) == null));

			packingLine.PackType = new PackageType();
			AssertEquals("Redundant field exists.", false, packingLine.IsLoosePackageIDDataObject());
		}
	}
}
