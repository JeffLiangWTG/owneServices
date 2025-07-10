using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GoodsShipmentPackagingTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMarksNumbers()
		{
			var header = Factory.New<CusTWControllingMessageHeader>();
			var line = Factory.New<JobComInvoiceLine>();
			header.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			line.NX101ShippingMarks = "shipping marks test";
			IPackaging goodsShipmentPackaging = new NX101GoodsShipmentPackaging(header, line);
			NUnit.Framework.Assert.That(goodsShipmentPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo("shipping marks test").Using(CustomComparers.TypeComparison));

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			goodsShipmentPackaging = new NX101GoodsShipmentPackaging(header, line);
			NUnit.Framework.Assert.That(goodsShipmentPackaging.MarksNumbers, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}
	}
}
