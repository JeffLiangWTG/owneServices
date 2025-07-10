using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101ConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalDocument()
		{
			var certificate = header.CertificateOfOrigins.AddNew();
			certificate.CSI_ReferenceNumber = "21C1115A3300/00006R";
			certificate = header.CertificateOfOrigins.AddNew();
			certificate.CSI_ReferenceNumber = "21C1115A3300/00007R";
			var additionalDocument = consignment.AdditionalDocument.ToArray();
			NUnit.Framework.Assert.That(additionalDocument.Length, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(additionalDocument[0].ID, NUnit.Framework.Is.EqualTo("21C1115A3300/00006R").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument[1].ID, NUnit.Framework.Is.EqualTo("21C1115A3300/00007R").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUnloadingLocation()
		{
			header.TW1_RL_NKPortOfUnloading = "TWZ99";
			header.TW1_PortOfUnloadingName = "台湾";
			var unloadingLocation = consignment.UnloadingLocation;
			NUnit.Framework.Assert.That(unloadingLocation.ID, NUnit.Framework.Is.EqualTo("TWZ99").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(unloadingLocation.Name, NUnit.Framework.Is.EqualTo("台湾").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem()
		{
			NUnit.Framework.Assert.That(consignment.GovernmentAgencyGoodsItem, NUnit.Framework.Is.TypeOf<NX101GovernmentAgencyGoodsItem>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			consignment = new NX101Consignment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX101Consignment consignment;
	}
}
