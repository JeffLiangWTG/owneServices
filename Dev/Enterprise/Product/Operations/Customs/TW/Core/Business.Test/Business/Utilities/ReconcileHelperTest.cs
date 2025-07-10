using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ReconcileHelperTest : TestCaseWithFactory
	{
		class ReconcileHelperTestCandidate : IReconcileCandidate
		{
			public ZDecimal ReconciledCustomsValue { set; get; }

			public ZDecimal CustomsValue { set; get; }

			public ZInt LineNumber { set; get; }

			public ZDecimal PreReconciledCustomsValue => CustomsValue;
			public ZInt SecondarySortingValue => LineNumber;
		}

		[ExpectNoExceptions]
		public void TestReconcileCustomsValues()
		{
			var candidates = new List<ReconcileHelperTestCandidate>();
			var candidate1 = new ReconcileHelperTestCandidate { CustomsValue = 1m, LineNumber = 1 };
			var candidate2 = new ReconcileHelperTestCandidate { CustomsValue = 3m, LineNumber = 2 };
			var candidate3 = new ReconcileHelperTestCandidate { CustomsValue = 2m, LineNumber = 3 };
			candidates.Add(candidate1);
			candidates.Add(candidate2);
			candidates.Add(candidate3);
			ReconcileHelper.ReconcileValues(candidates, 2);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(4m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			ReconcileHelper.ReconcileValues(candidates, -2);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			ReconcileHelper.ReconcileValues(candidates, 20000);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(6667m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(6670m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(6669m).Using(CustomComparers.TypeComparison));
			candidate1.CustomsValue = 10000m;
			candidate2.CustomsValue = 30000m;
			candidate3.CustomsValue = 20000m;
			ReconcileHelper.ReconcileValues(candidates, -20000);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3334m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(23333m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(13333m).Using(CustomComparers.TypeComparison));
			ReconcileHelper.ReconcileValues(candidates, 0);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(10000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(30000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(20000m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReconcileCustomsValuesWhenCustomsValueIsOne()
		{
			var candidates = new List<ReconcileHelperTestCandidate>();
			var candidate1 = new ReconcileHelperTestCandidate { CustomsValue = 1m, LineNumber = 1 };
			var candidate2 = new ReconcileHelperTestCandidate { CustomsValue = 2828m, LineNumber = 2 };
			var candidate3 = new ReconcileHelperTestCandidate { CustomsValue = 1m, LineNumber = 3 };
			candidates.Add(candidate1);
			candidates.Add(candidate2);
			candidates.Add(candidate3);
			ReconcileHelper.ReconcileValues(candidates, -2);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "candidate1");
				NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2826m).Using(CustomComparers.TypeComparison), "candidate2");
				NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "candidate3");
				NUnit.Framework.Assert.That(candidates.Count, NUnit.Framework.Is.EqualTo(3), "candidates count");
			});
		}

		[ExpectNoExceptions]
		public void TestSecondaySortingValue()
		{
			var candidates = new List<ReconcileHelperTestCandidate>();
			var candidate1 = new ReconcileHelperTestCandidate { CustomsValue = 1m, LineNumber = 1 };
			var candidate2 = new ReconcileHelperTestCandidate { CustomsValue = 2m, LineNumber = 2 };
			var candidate3 = new ReconcileHelperTestCandidate { CustomsValue = 2m, LineNumber = 3 };
			candidates.Add(candidate1);
			candidates.Add(candidate2);
			candidates.Add(candidate3);
			ReconcileHelper.ReconcileValues(candidates, 1);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
			candidates.Clear();
			candidates.Add(candidate1);
			candidates.Add(candidate3);
			candidates.Add(candidate2);
			ReconcileHelper.ReconcileValues(candidates, 1);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSecondaySortingValueWhenNoValue()
		{
			var candidates = new List<ReconcileHelperTestCandidate>();
			var candidate1 = new ReconcileHelperTestCandidate { CustomsValue = 1m, LineNumber = 0 };
			var candidate2 = new ReconcileHelperTestCandidate { CustomsValue = 2m, LineNumber = 0 };
			var candidate3 = new ReconcileHelperTestCandidate { CustomsValue = 2m, LineNumber = 0 };
			candidates.Add(candidate1);
			candidates.Add(candidate2);
			candidates.Add(candidate3);
			ReconcileHelper.ReconcileValues(candidates, 1);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
			candidates.Clear();
			candidates.Add(candidate1);
			candidates.Add(candidate3);
			candidates.Add(candidate2);
			ReconcileHelper.ReconcileValues(candidates, 1);
			NUnit.Framework.Assert.That(candidate1.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(1m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate3.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(3m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(candidate2.ReconciledCustomsValue, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison));
		}
	}
}
