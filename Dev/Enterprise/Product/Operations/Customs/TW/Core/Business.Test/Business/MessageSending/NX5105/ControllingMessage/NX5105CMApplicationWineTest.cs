using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMApplicationWineTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			NUnit.Framework.Assert.That(wine.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(0));
			header.EthanolPermitNumbers.AddNew().CSI_ReferenceNumber = "DN123456789012";
			NUnit.Framework.Assert.That(wine.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(wine.AdditionalDocuments.First().ID, NUnit.Framework.Is.EqualTo("DN123456789012").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGovernmentProcedurePreviousCode()
		{
			NUnit.Framework.Assert.That(wine.GovernmentProcedurePreviousCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			header.TW1_PreWineInspectionStatus = "1";
			NUnit.Framework.Assert.That(wine.GovernmentProcedurePreviousCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			NUnit.Framework.Assert.That(wine.PreviousDocument.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			header.TW1_PrePermitNumber = "DN81E905510000";
			NUnit.Framework.Assert.That(wine.PreviousDocument.ID, NUnit.Framework.Is.EqualTo("DN81E905510000").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTWControllingMessageHeader>();
			wine = new NX5105CMApplicationWine(header);
		}

		CusTWControllingMessageHeader header;
		IApplicationWine wine;
	}
}
