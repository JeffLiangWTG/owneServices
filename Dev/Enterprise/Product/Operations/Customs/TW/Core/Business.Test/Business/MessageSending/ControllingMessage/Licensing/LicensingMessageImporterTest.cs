using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageImporter))]
	sealed class LicensingMessageImporterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var org = new TestTWCreator(Factory).CreateOrganization();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.IDCode = "52889317";
			importerDocumentaryAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			importerDocumentaryAddress.E2_Phone = "+86987324348";
			importerDocumentaryAddress.E2_Email = "xx1@gmail.com";
			IPartyDetails localProcessorAddressWrapper = new LicensingMessageImporter(importerDocumentaryAddress);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.ID, NUnit.Framework.Is.EqualTo("52889317").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Name, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.Line, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison), "Line");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ChineseLine");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault(c => c.TypeID == MessageConstants.CommunicationTypeIDs.TE).ID, NUnit.Framework.Is.EqualTo("+86987324348").Using(CustomComparers.TypeComparison), "Phone");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault(c => c.TypeID == MessageConstants.CommunicationTypeIDs.MA).ID, NUnit.Framework.Is.EqualTo("xx1@gmail.com").Using(CustomComparers.TypeComparison), "Email");
			});
		}
	}
}
