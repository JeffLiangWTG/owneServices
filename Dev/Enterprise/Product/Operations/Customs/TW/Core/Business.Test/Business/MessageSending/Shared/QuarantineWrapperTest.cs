using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class QuarantineWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestQuarantineWrapper()
		{
			NUnit.Framework.Assert.That(quarantine.ObjectFeature, NUnit.Framework.Is.EqualTo("Yellow").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(quarantine.Treatment, NUnit.Framework.Is.EqualTo("None").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(quarantine.Animal, NUnit.Framework.Is.TypeOf<AnimalWrapper>());
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocument()
		{
			NUnit.Framework.Assert.That(quarantine.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalDocument>)));

			var collection = invoiceLine.SlaughterDateCollection;
			var bizObj1 = collection.AddNew();
			bizObj1.CY_Date = ZDateTime.BrettsBirthday;
			var bizObj2 = collection.AddNew();
			bizObj2.CY_Date = ZDateTime.BrettsBirthday.AddDays(1);
			quarantine = new QuarantineWrapper(invoiceLine);
			NUnit.Framework.Assert.That(quarantine.AdditionalDocument.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(quarantine.AdditionalDocument.Select(x => x.SlaughterDateTime), NUnit.Framework.Is.EquivalentTo(new ZDate[] { ZDateTime.BrettsBirthday.Date, ZDateTime.BrettsBirthday.AddDays(1).Date }));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation()
		{
			NUnit.Framework.Assert.That(quarantine.AdditionalInformation, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IAdditionalInformation>)));

			var collection = invoiceLine.PackingHouseCollection;
			var bizObj1 = collection.AddNew();
			bizObj1.CY_Code = "H1";
			var bizObj2 = collection.AddNew();
			bizObj2.CY_Code = "H2";
			quarantine = new QuarantineWrapper(invoiceLine);
			NUnit.Framework.Assert.That(quarantine.AdditionalInformation.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(quarantine.AdditionalInformation.Select(x => x.PackingHouse), NUnit.Framework.Is.EquivalentTo(new ZString[] { "H1", "H2" }));
		}

		[ExpectNoExceptions]
		public void TestPacking()
		{
			NUnit.Framework.Assert.That(quarantine.Packing, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IPackaging>)));

			var collection = invoiceLine.PackingDateCollection;
			var bizObj1 = collection.AddNew();
			bizObj1.CY_Date = ZDateTime.BrettsBirthday.AddDays(2);
			var bizObj2 = collection.AddNew();
			bizObj2.CY_Date = ZDateTime.BrettsBirthday.AddDays(3);
			quarantine = new QuarantineWrapper(invoiceLine);
			NUnit.Framework.Assert.That(quarantine.Packing.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(quarantine.Packing.Select(x => x.PackingDateTime), NUnit.Framework.Is.EquivalentTo(new ZDate[] { ZDateTime.BrettsBirthday.AddDays(2).Date, ZDateTime.BrettsBirthday.AddDays(3).Date }));
		}

		protected override void SetUp()
		{
			invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_QuarantineFeatures = "Yellow";
			invoiceLine.JI_QuarantineTreatment = "None";
			quarantine = new QuarantineWrapper(invoiceLine);
			base.SetUp();
		}

		IQuarantine quarantine;
		JobComInvoiceLine invoiceLine;
	}
}
