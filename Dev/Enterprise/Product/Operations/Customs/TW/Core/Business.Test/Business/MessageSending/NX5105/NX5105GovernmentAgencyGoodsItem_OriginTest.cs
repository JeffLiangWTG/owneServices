using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105GovernmentAgencyGoodsItem_OriginTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGovernmentAgencyGoodsItem_Origin()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			IOrigin origin = new NX5105GovernmentAgencyGoodsItem_Origin(invoiceLine);
			invoiceLine.JI_CountryOfOrigin = "";
			NUnit.Framework.Assert.That(origin.CountryCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Origin.CountryCode should be ");
			invoiceLine.JI_CountryOfOrigin = "IT";
			NUnit.Framework.Assert.That(origin.CountryCode, NUnit.Framework.Is.EqualTo("IT").Using(CustomComparers.TypeComparison), "Origin.CountryCode should be ");
			invoiceLine.CertificateOfOriginNumber = "";
			NUnit.Framework.Assert.That(origin.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(IAdditionalDocument)));
			invoiceLine.CertificateOfOriginNumber = "123";
			invoiceLine.CertificateOfOriginNumberItemNumber = 1234;
			NUnit.Framework.Assert.That(origin.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Origin.AdditionalDocument.ID should be ");
			NUnit.Framework.Assert.That(origin.AdditionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(1234).Using(CustomComparers.TypeComparison), "Origin.AdditionalDocument.SequenceNumeric should be ");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				IOrigin origin = new NX5105GovernmentAgencyGoodsItem_Origin(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				IOrigin origin = new NX5105GovernmentAgencyGoodsItem_Origin(invoiceLine);
			}

			);
		}
	}
}
