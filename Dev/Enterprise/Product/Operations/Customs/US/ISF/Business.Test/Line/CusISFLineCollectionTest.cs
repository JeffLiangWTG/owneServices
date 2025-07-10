using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFLineCollection))]
	sealed class CusISFLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusISFLineCollection>
	{
		public void TestDefaultManufacturer()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			CusISFLine line1 = header.Lines.AddNew();
			AssertEquals(ZGuid.Empty, line1.BL_ManufacturerDocAddressPK);
			ISFDocAddress manufacturer1 = header.ManufacturerAddresses.AddNew();
			CusISFLine line2 = header.Lines.AddNew();
			AssertEquals(ZGuid.Empty, line1.BL_ManufacturerDocAddressPK);
			AssertEquals(manufacturer1.PK, line2.BL_ManufacturerDocAddressPK);
			ISFDocAddress manufacturer2 = header.ManufacturerAddresses.AddNew();
			CusISFLine line3 = header.Lines.AddNew();
			AssertEquals(ZGuid.Empty, line1.BL_ManufacturerDocAddressPK);
			AssertEquals(manufacturer1.PK, line2.BL_ManufacturerDocAddressPK);
			AssertEquals(ZGuid.Empty, line3.BL_ManufacturerDocAddressPK);
		}

		protected override CusISFLineCollection GetCollectionToTest() => new CusISFLineCollection(Factory.New<CusISFHeader>());
	}
}
