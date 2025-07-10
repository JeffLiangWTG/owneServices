using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class CommonDefinedResourceStringsTest : TestCaseWithFactory
	{
		public void TestSetPackageTypeProperty()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);
			CommonDefinedResourceStrings.SetPackageTypeProperty(shipment.JS_F3_NKPackTypeInfo, "XXXX", "blah", context, true);
			AssertContains($"Warning: Maximum length of this field has been exceeded (blah has exceeded the maximum length allowed by the system. When you edit this record, the package type field will error. Please use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type; value=XXXX)", buffer.AsString);

			buffer.Clear();
			CommonDefinedResourceStrings.SetPackageTypeProperty(shipment.JS_F3_NKPackTypeInfo, "YYY", "blah", context, true);
			AssertContains($"Warning: blah is not valid. When you edit this record, the package type field will error. To avoid this error you can either use Code Mapping to map this value to the appropriate {Core.Constants.ProductName} package type or you can add this value to the reference files (Reference Files -> Package Types)", buffer.AsString);

			buffer.Clear();
			CommonDefinedResourceStrings.SetPackageTypeProperty(shipment.JS_F3_NKPackTypeInfo, Core.Constants.PkgUnit.Bottle, "blah", context, true);
			Assert("it should not return any errors", !buffer.HasErrors);
			Assert("it should not return any warnings", !buffer.HasWarnings);
		}
	}
}
