using CargoWise.RefDbRepo.Common.Contract_0_9;
using CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.Test
{
	[TestFixture]
	public class TransformHelperFixture
	{
		[Test]
		public void TestTransformTopLevelTable()
		{
			var tester = new RefCusTariff();
			TransformHelper.Transform<RefCusTariff>(tester, V10Transform.Instance());

			var tester2 = new RefCusTariff();
			var tariffType = new RefCusTariffType();
			tariffType.ZZI_ZZZ_NKDataGrouping = "ZA";
			tester2.RefCusTariffType = tariffType;

			TransformHelper.Transform<RefCusTariff>(tester2, V10Transform.Instance());
			Assert.AreEqual("ZA", tester2.RefCusTariffType.ZZI_RN_CountryOrGrouping);
		}

		class TransformRefCusTariffUOMStratergy : ITransformStrategy
		{
			public Type[] ToBeTransformedTypes
			{
				get { return new Type[] { typeof(RefCusTariffUOM) }; }
			}

			public bool RequireTransform(Type dataSetType, int version)
			{
				return true;
			}

			public T Transform<T>(T data)
			{
				var result = data;
				if (result != null)
				{
					if (result is RefCusTariffUOM)
					{
						(result as RefCusTariffUOM).ZZ8_UOM = "XX";
					}
				}
				return result;
			}
		}

		[Test]
		public void TestTransformRefCusTariffUOM()
		{
			var tester = new RefCusTariff();
			var testUom = new RefCusTariffUOM();
			testUom.ZZ8_ZZZ_NKDataGrouping = "group";
			var tariffUom = new RefCusTariffUOM();
			tester.RefCusTariffUOMs = new RefCusTariffUOM[] { tariffUom };

			TransformHelper.Transform<RefCusTariff>(tester, new TransformRefCusTariffUOMStratergy());
			Assert.AreEqual("XX", tester.RefCusTariffUOMs.First().ZZ8_UOM);
		}

		[Test]
		public void TestNeedTransform()
		{
			var stringVar = "xyz";
			var mock = new Mock<ITransformStrategy>().Object;
			Assert.AreEqual(false, TransformHelper.NeedTransform(stringVar.GetType(), mock));

			var intVar = 123;
			Assert.AreEqual(false, TransformHelper.NeedTransform(intVar.GetType(), mock));

			var dateVar = DateTime.Now;
			Assert.AreEqual(false, TransformHelper.NeedTransform(dateVar.GetType(), mock));

			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusTariff), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusTariffType), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCarrierCode), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusCodeList), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusCodeType), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusMap), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusNomenclatureGroup), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusNomenclatureGroupNote), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusProcedure), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusRateType), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusTaxOrFee), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusTradeAgreement), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusTradeAgreementCountry), V10Transform.Instance()));
			Assert.AreEqual(true, TransformHelper.NeedTransform(typeof(RefCusRateCode), V10Transform.Instance()));

			Assert.AreEqual(false, TransformHelper.NeedTransform(typeof(RefCusTariffUOM), V10Transform.Instance()));
			Assert.AreEqual(false, TransformHelper.NeedTransform(typeof(RefCusPreference), V10Transform.Instance()));

			Assert.That(TransformHelper.NeedTransform(typeof(RefCusCodeList), SV564Transform.Instance()));
		}

		[Test]
		public void TestPathPlanner()
		{
			var mock = new Mock<ITransformStrategy>().Object;
			var pathPlanner = PathPlannerFactory.Create(mock.ToString(), typeof(RefCusTariff));
			Assert.AreEqual(true, pathPlanner.IsFirstTimeCreated);
			pathPlanner.PathNodeTouched = false;

			var needTransform = TransformHelper.NeedTransform(typeof(RefCusTariff), mock);
			Assert.AreEqual(false, pathPlanner.IsFirstTimeCreated);
			Assert.AreEqual(false, needTransform);

			pathPlanner.PathNodeTouched = true;
			needTransform = TransformHelper.NeedTransform(typeof(RefCusTariff), mock);
			Assert.AreEqual(true, needTransform);
		}

		[Test]
		public void TestNeedTransformConcurrency()
		{
			var mock = new Mock<ITransformStrategy>().Object;
			var threads = new Thread[10];
			var exceptionThrown = false;
			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = new Thread(() =>
				{
					try
					{
						TransformHelper.NeedTransform(typeof(RefCusTariff), mock);
					}
					catch
					{
						exceptionThrown = true;
					}
				});
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				while (thread.ThreadState != ThreadState.Stopped)
				{
					continue;
				}
			}

			Assert.False(exceptionThrown, "An exception is not excepted in this method, but was thrown");
		}
	}
}
