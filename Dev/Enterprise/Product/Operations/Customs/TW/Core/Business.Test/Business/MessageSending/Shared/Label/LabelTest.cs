using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing.MessageSending
{
	sealed class LabelTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(label.StatusNameCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(label.LabelDetails, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ILabelDetail>)));
			});
		}

		[ExpectNoExceptions]
		public void TestGetLabels()
		{
			var collection = new CusTWProductLabelRangeCollection(Factory.New<CusTWControllingMessageHeader>());
			var productLabelRange1 = collection.AddNew();
			productLabelRange1.TW0_Status = "2";
			var productLabelRange2 = collection.AddNew();
			productLabelRange2.TW0_Status = "1";
			var productLabelRange3 = collection.AddNew();
			productLabelRange3.TW0_Status = "2";

			var labels = Label.GetLabels(collection).ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(labels.Length, NUnit.Framework.Is.EqualTo(2), "Should have 2 labels");
				NUnit.Framework.Assert.That(labels[0].StatusNameCode == "1" && labels[1].StatusNameCode == "2", NUnit.Framework.Is.True, "Labels should ordered by TW0_Status");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			label = new Label("2", null);
		}

		ILabel label;
	}
}
