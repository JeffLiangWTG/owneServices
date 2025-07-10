using System.Linq;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	sealed class DAEPackWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestDAEPackWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			CreateAndPopulatePack(bill);
			CreateAndPopulatePack(bill);

			IDaeDeclaration wrapper = new DAEWrapper(header, header.Bills.Cast<AsycudaBill>().ToArray());
			var manifest = wrapper.Manifests.First();

			IDaeBillOfLading bill1 = manifest.BillsOfLading.ElementAt(0);

			IDaeBillOfLadingLine pack1 = bill1.Lines.ElementAt(0);
			IDaeBillOfLadingLine pack2 = bill1.Lines.ElementAt(1);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals((ZShort)1, pack1.LineNo);
				AssertEquals((ZDecimal)127.900, pack1.Weight);
				AssertEquals("BLK", pack1.PackUQ);
				AssertEquals((ZDecimal)1.000, pack1.PackQty);
				AssertEquals("MarksAndNumbers", pack1.MarksAndNumbers);
				AssertEquals("DIAGNOSTIC LABORATORY", pack1.GoodsDescription);

				AssertEquals((ZShort)2, pack2.LineNo);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_Weight = 127.900m;
			pack.APA_WeightUQ = "KG";
			pack.APA_PackUQ = "BBK";
			pack.APA_PackQty = 1;
			pack.APA_MarksAndNumbers = "MarksAndNumbers";
			pack.APA_GoodsDescription = "DIAGNOSTIC LABORATORY";
		}
	}
}

