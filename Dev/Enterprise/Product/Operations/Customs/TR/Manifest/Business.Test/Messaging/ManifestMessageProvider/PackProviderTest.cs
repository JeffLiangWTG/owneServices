using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class PackProviderTest : TestCaseWithFactory
	{
		public void TestGoodsInformationMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
				ILadingLines ladingLines = billofLading.LadingLines.FirstOrDefault();
				var goodsInformation = ladingLines.GoodsInformation.ToArray();
				CombineAssertions("Package Level", () =>
				{
					AssertEquals(Convert.ToDecimal(0), ladingLines.GrossWeight);
					AssertEquals(15, ladingLines.PackQuantity);
					AssertEquals(AsycudaPack.Pack, ladingLines.PackType);
					AssertEquals("YER", ladingLines.ContainerType);
					AssertEquals("CNTR000001", ladingLines.ContainerNumber);
					AssertEquals("Seal 1", ladingLines.SealNumber);
					AssertEquals(Convert.ToDecimal(0), ladingLines.NetWeight);
					AssertEquals(TurkishConstants.WeightUnitType, ladingLines.WeightUQ);
					AssertEquals(1, ladingLines.LineNo);
					AssertEquals(TurkishConstants.ContainerEmpty, ladingLines.ContainerLoadStatus);
					AssertEquals(4, goodsInformation.Length);
					AssertEquals(1, goodsInformation[0].OrderNo);
				});
			}
		}

		public void TestContainerType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			var sumDec = new ManifestMessageProvider(header);
			IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
			ILadingLines ladingLines = billofLading.LadingLines.FirstOrDefault();
			AssertEquals(ZString.Empty, ladingLines.ContainerType);
			container.ACN_ContainerNumber = "CNTR000001";
			sumDec = new ManifestMessageProvider(header);
			billofLading = sumDec.BillofLadings.FirstOrDefault();
			ladingLines = billofLading.LadingLines.FirstOrDefault();
			container.Relation = ZString.Empty;
			AssertEquals(ZString.Empty, ladingLines.ContainerType);
			container.Relation = "Foreign";
			AssertEquals("YAB", ladingLines.ContainerType);
			container.Relation = "Local";
			AssertEquals("YER", ladingLines.ContainerType);
		}

		public void TestContainerLoadStatus()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			var sumDec = new ManifestMessageProvider(header);
			IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
			ILadingLines ladingLines = billofLading.LadingLines.FirstOrDefault();
			AssertEquals(ZString.Empty, ladingLines.ContainerLoadStatus);
			container.ACN_ContainerNumber = "CNTR000001";
			sumDec = new ManifestMessageProvider(header);
			billofLading = sumDec.BillofLadings.FirstOrDefault();
			ladingLines = billofLading.LadingLines.FirstOrDefault();
			container.ACN_EmptyFullIndicator = ZString.Empty;
			AssertEquals(ZString.Empty, ladingLines.ContainerLoadStatus);
			container.ACN_EmptyFullIndicator = "MT";
			AssertEquals(TurkishConstants.ContainerEmpty, ladingLines.ContainerLoadStatus);
			container.ACN_EmptyFullIndicator = "XX";
			AssertEquals(TurkishConstants.ContainerFull, ladingLines.ContainerLoadStatus);
		}

		public void TestPackType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var sumDec = new ManifestMessageProvider(header);
			IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
			ILadingLines ladingLines = billofLading.LadingLines.FirstOrDefault();
			AssertEquals("Package Type est default", AsycudaPack.Pack, ladingLines.PackType);
			pack.APA_PackUQ = "ABC";
			sumDec = new ManifestMessageProvider(header);
			billofLading = sumDec.BillofLadings.FirstOrDefault();
			ladingLines = billofLading.LadingLines.FirstOrDefault();
			AssertEquals("Package Type", "ABC", ladingLines.PackType);
		}
	}
}
