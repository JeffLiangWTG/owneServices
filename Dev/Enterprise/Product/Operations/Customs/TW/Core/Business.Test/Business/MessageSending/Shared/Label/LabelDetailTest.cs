using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing.MessageSending
{
	sealed class LabelDetailTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(label.EndNumber, NUnit.Framework.Is.EqualTo("00000050").Using(CustomComparers.TypeComparison), "EndNumber");
				NUnit.Framework.Assert.That(label.StartNumber, NUnit.Framework.Is.EqualTo("00000010").Using(CustomComparers.TypeComparison), "StartNumber");
				NUnit.Framework.Assert.That(label.Track, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "Track");
				NUnit.Framework.Assert.That(label.Year, NUnit.Framework.Is.EqualTo("102").Using(CustomComparers.TypeComparison), "Year");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var bo = Factory.New<CusTWProductLabelRange>();
			bo.TW0_EndNumber = "00000050";
			bo.TW0_StartNumber = "00000010";
			bo.TW0_RunNumber = "A";
			bo.TW0_Year = "102";
			label = new LabelDetail(bo);
		}

		ILabelDetail label;
	}
}
