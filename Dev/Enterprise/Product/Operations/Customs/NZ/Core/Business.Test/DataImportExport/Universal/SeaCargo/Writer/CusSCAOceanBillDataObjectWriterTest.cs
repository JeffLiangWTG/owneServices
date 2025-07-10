using System.Reflection;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	[TestedType(typeof(CusSCAOceanBillDataObjectWriter))]
	partial class CusSCAOceanBillDataObjectWriterTest : CusSCAOceanBillDataObjectWriterTest<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPackingLine>
	{
		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager writeManager)
		{
			return new CusSCAOceanBillDataObjectWriter(writeManager);
		}

		protected override string OceanBillShipmentXML => TestFileHelper.ReadResourceFileContents("OceanBillShipment.xml", Assembly.GetExecutingAssembly());

		protected override void SetUp()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, new CodeDescriptionPair("BG", "Bag"));
			Factory.Save();
			base.SetUp();
		}
	}
}
