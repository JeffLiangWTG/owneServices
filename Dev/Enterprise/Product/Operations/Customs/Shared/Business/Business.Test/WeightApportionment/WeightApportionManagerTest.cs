using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	sealed class WeightApportionManagerTest : TestCaseWithFactory
	{
		public void TestIsWeightApportionmentDeffered()
		{
			var manager = new WeightApportionManager();
			using (manager.DeferWeightApportionment())
			{
				AssertEquals(true, manager.IsWeightApportionmentDeffered);
			}
		}

		public void TestDeferWeightApportionment()
		{
			var dummy1 = new WeightApportioneeDummy()
			{
				Amount = 200,
				Weight = 45m,
				WeightUQ = ZString.Empty
			};

			var dummy2 = new WeightApportioneeDummy()
			{
				Amount = 300m,
				Weight = ZDecimal.Zero,
				WeightUQ = Core.Constants.Weight.Grams
			};

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(120m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1, dummy2 });

			var manager = new WeightApportionManager();
			using (manager.DeferWeightApportionment())
			{
				using (manager.DeferWeightApportionment())
				{
					manager.ApportionAll(weightHolderMock.Object, null);
					AssertWeightApportioneesUnchanged();
				}
				AssertWeightApportioneesUnchanged();
			}
			AssertWeightApportionee(dummy1, 200m, 48m, Core.Constants.Weight.Kilograms);
			AssertWeightApportionee(dummy2, 300m, 72000m, Core.Constants.Weight.Grams);

			void AssertWeightApportioneesUnchanged()
			{
				AssertWeightApportionee(dummy1, 200m, 45m, ZString.Empty);
				AssertWeightApportionee(dummy2, 300m, ZDecimal.Zero, Core.Constants.Weight.Grams);
			}
		}

		List<WeightApportioneeDummy> GetWeightApportioneeDummys()
		{
			List<WeightApportioneeDummy> list = new List<WeightApportioneeDummy>();
			list.Add(new WeightApportioneeDummy() { Amount = 371.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 540.00m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 456.50m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 168.75m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 317.25m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 78.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 88.90m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 194.10m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 156.40m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 105.75m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 149.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 1350.00m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 149.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 476.00m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 1564.00m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 129.40m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 86.10m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 288.75m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 82.80m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 235.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 444.40m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 202.72m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 124.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 88.90m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 126.90m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 94.20m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 189.90m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 296.30m, WeightUQ = "KG", NetWeightUQ = "KG" });
			list.Add(new WeightApportioneeDummy() { Amount = 28.80m, WeightUQ = "KG", NetWeightUQ = "KG" });
			return list;
		}

		public void TestReconcileWeight()
		{
			CombineAssertions("WeightApportionees have single unit.", () =>
			{
				var weightApportioneeDummyArray = GetWeightApportioneeDummys().ToArray();
				var childWeightHoldersMock = new Mock<IWeightHolder>();
				childWeightHoldersMock.Setup(m => m.TotalWeight).Returns(new ZWeight(54.000m, Core.Constants.Weight.Kilograms));
				childWeightHoldersMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(50.605m, Core.Constants.Weight.Kilograms));
				childWeightHoldersMock.Setup(m => m.AllApportionees).Returns(weightApportioneeDummyArray);

				var weightHolderMock = new Mock<IWeightHolder>();
				weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(54.000m, Core.Constants.Weight.Kilograms));
				weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Kilograms));
				weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { new WeightApportioneeDummy() { Amount = 251425.9458m, Weight = 54.000m, WeightUQ = "KG", NetWeight = 50.605m, NetWeightUQ = "KG", NeedToApportionNetWeight = true } });
				weightHolderMock.Setup(m => m.WeightHolders).Returns(new IWeightHolder[] { childWeightHoldersMock.Object });

				var manager = new WeightApportionManager();
				manager.ApportionAll(weightHolderMock.Object, null);

				AssertWeightApportionee(weightApportioneeDummyArray[0], 371.20m, 2.335m, Core.Constants.Weight.Kilograms, 2.188m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[1], 540.00m, 3.397m, Core.Constants.Weight.Kilograms, 3.183m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[2], 456.50m, 2.872m, Core.Constants.Weight.Kilograms, 2.691m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[3], 168.75m, 1.062m, Core.Constants.Weight.Kilograms, 0.995m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[4], 317.25m, 1.996m, Core.Constants.Weight.Kilograms, 1.871m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[5], 78.20m, 0.492m, Core.Constants.Weight.Kilograms, 0.461m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[6], 88.90m, 0.559m, Core.Constants.Weight.Kilograms, 0.524m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[7], 194.10m, 1.221m, Core.Constants.Weight.Kilograms, 1.144m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[8], 156.40m, 0.984m, Core.Constants.Weight.Kilograms, 0.922m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[9], 105.75m, 0.665m, Core.Constants.Weight.Kilograms, 0.623m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[10], 149.20m, 0.939m, Core.Constants.Weight.Kilograms, 0.880m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[11], 1350.00m, 8.493m, Core.Constants.Weight.Kilograms, 7.959m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[12], 149.20m, 0.939m, Core.Constants.Weight.Kilograms, 0.880m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[13], 476.00m, 2.994m, Core.Constants.Weight.Kilograms, 2.806m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[14], 1564.00m, 9.837m, Core.Constants.Weight.Kilograms, 9.218m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[15], 129.40m, 0.814m, Core.Constants.Weight.Kilograms, 0.763m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[16], 86.10m, 0.542m, Core.Constants.Weight.Kilograms, 0.508m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[17], 288.75m, 1.816m, Core.Constants.Weight.Kilograms, 1.702m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[18], 82.80m, 0.521m, Core.Constants.Weight.Kilograms, 0.488m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[19], 235.20m, 1.480m, Core.Constants.Weight.Kilograms, 1.387m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[20], 444.40m, 2.796m, Core.Constants.Weight.Kilograms, 2.620m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[21], 202.72m, 1.275m, Core.Constants.Weight.Kilograms, 1.195m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[22], 124.20m, 0.781m, Core.Constants.Weight.Kilograms, 0.732m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[23], 88.90m, 0.559m, Core.Constants.Weight.Kilograms, 0.524m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[24], 126.90m, 0.798m, Core.Constants.Weight.Kilograms, 0.748m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[25], 94.20m, 0.593m, Core.Constants.Weight.Kilograms, 0.556m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[26], 189.90m, 1.195m, Core.Constants.Weight.Kilograms, 1.120m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[27], 296.30m, 1.864m, Core.Constants.Weight.Kilograms, 1.747m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[28], 28.80m, 0.181m, Core.Constants.Weight.Kilograms, 0.170m, Core.Constants.Weight.Kilograms);
			});

			CombineAssertions("WeightApportionees have multiple unit.", () =>
			{
				var weightApportioneeDummyArray = GetWeightApportioneeDummys().ToArray();
				var apportioneeDummy = weightApportioneeDummyArray[14];
				apportioneeDummy.WeightUQ = "G";
				apportioneeDummy.NetWeightUQ = "LB";
				var childWeightHoldersMock = new Mock<IWeightHolder>();
				childWeightHoldersMock.Setup(m => m.TotalWeight).Returns(new ZWeight(54.000m, Core.Constants.Weight.Kilograms));
				childWeightHoldersMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(50.605m, Core.Constants.Weight.Kilograms));
				childWeightHoldersMock.Setup(m => m.AllApportionees).Returns(weightApportioneeDummyArray);

				var weightHolderMock = new Mock<IWeightHolder>();
				weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(54.000m, Core.Constants.Weight.Kilograms));
				weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Kilograms));
				weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { new WeightApportioneeDummy() { Amount = 251425.9458m, Weight = 54.000m, WeightUQ = "KG", NetWeight = 50.605m, NetWeightUQ = "KG", NeedToApportionNetWeight = true } });
				weightHolderMock.Setup(m => m.WeightHolders).Returns(new IWeightHolder[] { childWeightHoldersMock.Object });

				var manager = new WeightApportionManager();
				manager.ApportionAll(weightHolderMock.Object, null);

				AssertWeightApportionee(weightApportioneeDummyArray[0], 371.20m, 2.335m, Core.Constants.Weight.Kilograms, 2.188m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[1], 540.00m, 3.397m, Core.Constants.Weight.Kilograms, 3.183m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[2], 456.50m, 2.872m, Core.Constants.Weight.Kilograms, 2.691m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[3], 168.75m, 1.062m, Core.Constants.Weight.Kilograms, 0.995m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[4], 317.25m, 1.996m, Core.Constants.Weight.Kilograms, 1.871m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[5], 78.20m, 0.492m, Core.Constants.Weight.Kilograms, 0.461m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[6], 88.90m, 0.559m, Core.Constants.Weight.Kilograms, 0.524m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[7], 194.10m, 1.221m, Core.Constants.Weight.Kilograms, 1.144m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[8], 156.40m, 0.984m, Core.Constants.Weight.Kilograms, 0.922m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[9], 105.75m, 0.665m, Core.Constants.Weight.Kilograms, 0.623m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[10], 149.20m, 0.939m, Core.Constants.Weight.Kilograms, 0.880m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[11], 1350.00m, 8.493m, Core.Constants.Weight.Kilograms, 7.959m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[12], 149.20m, 0.939m, Core.Constants.Weight.Kilograms, 0.880m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[13], 476.00m, 2.994m, Core.Constants.Weight.Kilograms, 2.806m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[14], 1564.00m, 9836.747m, Core.Constants.Weight.Grams, 9.218m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[15], 129.40m, 0.814m, Core.Constants.Weight.Kilograms, 0.763m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[16], 86.10m, 0.542m, Core.Constants.Weight.Kilograms, 0.508m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[17], 288.75m, 1.816m, Core.Constants.Weight.Kilograms, 1.702m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[18], 82.80m, 0.521m, Core.Constants.Weight.Kilograms, 0.488m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[19], 235.20m, 1.480m, Core.Constants.Weight.Kilograms, 1.387m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[20], 444.40m, 2.796m, Core.Constants.Weight.Kilograms, 2.620m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[21], 202.72m, 1.275m, Core.Constants.Weight.Kilograms, 1.195m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[22], 124.20m, 0.781m, Core.Constants.Weight.Kilograms, 0.732m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[23], 88.90m, 0.559m, Core.Constants.Weight.Kilograms, 0.524m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[24], 126.90m, 0.798m, Core.Constants.Weight.Kilograms, 0.748m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[25], 94.20m, 0.593m, Core.Constants.Weight.Kilograms, 0.556m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[26], 189.90m, 1.195m, Core.Constants.Weight.Kilograms, 1.120m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[27], 296.30m, 1.864m, Core.Constants.Weight.Kilograms, 1.747m, Core.Constants.Weight.Kilograms);
				AssertWeightApportionee(weightApportioneeDummyArray[28], 28.80m, 0.181m, Core.Constants.Weight.Kilograms, 0.170m, Core.Constants.Weight.Kilograms);
			});
		}

		public void TestApportionAll()
		{
			var dummy1 = new WeightApportioneeDummy();
			dummy1.Amount = 200m;
			dummy1.Weight = 45m;
			dummy1.WeightUQ = ZString.Empty;

			var dummy2 = new WeightApportioneeDummy();
			dummy2.Amount = 300m;
			dummy2.Weight = ZDecimal.Zero;
			dummy2.WeightUQ = Core.Constants.Weight.Grams;

			var dummy3 = new WeightApportioneeDummy();
			dummy3.Amount = 500m;
			dummy3.Weight = ZDecimal.Zero;
			dummy3.WeightUQ = Core.Constants.Weight.Grams;

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(120m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1, dummy2 });

			var manager = new WeightApportionManager();
			manager.ApportionAll(weightHolderMock.Object, null);

			AssertWeightApportionee(dummy1, 200m, 48m, Core.Constants.Weight.Kilograms);
			AssertWeightApportionee(dummy2, 300m, 72000m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy3, 500m, 0m, Core.Constants.Weight.Grams);

			manager.ApportionAll(weightHolderMock.Object, dummy3);

			AssertWeightApportionee(dummy1, 200m, 24m, Core.Constants.Weight.Kilograms);
			AssertWeightApportionee(dummy2, 300m, 36000m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy3, 500m, 60000m, Core.Constants.Weight.Grams);
		}

		public void TestApportionAllCore_ExtremeNumbers()
		{
			var dummy1 = new WeightApportioneeDummy();
			dummy1.Amount = 999999999999999999999999999m;
			dummy1.Weight = 99999m;
			dummy1.WeightUQ = ZString.Empty;

			var dummy2 = new WeightApportioneeDummy();
			dummy2.Amount = 9m;
			dummy2.Weight = 9m;
			dummy2.WeightUQ = ZString.Empty;

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(99999m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy2 });

			var manager = new WeightApportionManager();
			AssertNoExceptionThrown(() => { manager.ApportionAll(weightHolderMock.Object, dummy1); });
		}

		public void TestApportionAll_NetWeight()
		{
			var dummy1 = new WeightApportioneeDummy();
			dummy1.Amount = 200m;
			dummy1.Weight = 45m;
			dummy1.WeightUQ = ZString.Empty;
			dummy1.NetWeight = 45m;
			dummy1.NetWeightUQ = ZString.Empty;
			dummy1.NeedToApportionNetWeight = true;

			var dummy2 = new WeightApportioneeDummy();
			dummy2.Amount = 300m;
			dummy2.Weight = ZDecimal.Zero;
			dummy2.WeightUQ = Core.Constants.Weight.Grams;
			dummy2.NetWeight = ZDecimal.Zero;
			dummy2.NetWeightUQ = Core.Constants.Weight.Kilograms;
			dummy2.NeedToApportionNetWeight = true;

			var dummy3 = new WeightApportioneeDummy();
			dummy3.Amount = 500m;
			dummy3.Weight = ZDecimal.Zero;
			dummy3.WeightUQ = Core.Constants.Weight.Grams;
			dummy3.NetWeight = ZDecimal.Zero;
			dummy3.NetWeightUQ = Core.Constants.Weight.Ounces;
			dummy3.NeedToApportionNetWeight = true;

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(1m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(900m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1, dummy2 });

			var manager = new WeightApportionManager();
			manager.ApportionAll(weightHolderMock.Object, null);

			AssertWeightApportionee(dummy1, 200m, 0.4m, Core.Constants.Weight.Kilograms, 360m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy2, 300m, 600m, Core.Constants.Weight.Grams, 540m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy3, 500m, 0m, Core.Constants.Weight.Grams, 0m, Core.Constants.Weight.Ounces);

			manager.ApportionAll(weightHolderMock.Object, dummy3);

			AssertWeightApportionee(dummy1, 200m, 0.2m, Core.Constants.Weight.Kilograms, 180m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy2, 300m, 300m, Core.Constants.Weight.Grams, 270m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy3, 500m, 500m, Core.Constants.Weight.Grams, 450m, Core.Constants.Weight.Grams);

			dummy3.Amount = 500m;
			dummy3.Weight = ZDecimal.Zero;
			dummy3.WeightUQ = Core.Constants.Weight.Grams;
			dummy3.NetWeight = ZDecimal.Zero;
			dummy3.NetWeightUQ = ZString.Empty;
			dummy3.NeedToApportionNetWeight = false;
			manager.ApportionAll(weightHolderMock.Object, dummy3);

			AssertWeightApportionee(dummy1, 200m, 0.2m, Core.Constants.Weight.Kilograms, 180m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy2, 300m, 300m, Core.Constants.Weight.Grams, 270m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy3, 500m, 500m, Core.Constants.Weight.Grams, 0m, string.Empty);
		}

		public void TestApportionAll_NetWeightWhenWeightIsEmpty()
		{
			var dummy1 = new WeightApportioneeDummy();
			dummy1.Amount = 2m;
			dummy1.Weight = ZDecimal.Zero;
			dummy1.WeightUQ = ZString.Empty;
			dummy1.NetWeight = ZDecimal.Zero;
			dummy1.NetWeightUQ = ZString.Empty;
			dummy1.NeedToApportionNetWeight = true;

			var dummy2 = new WeightApportioneeDummy();
			dummy2.Amount = 3m;
			dummy2.Weight = ZDecimal.Zero;
			dummy2.WeightUQ = ZString.Empty;
			dummy2.NetWeight = ZDecimal.Zero;
			dummy2.NetWeightUQ = ZString.Empty;
			dummy2.NeedToApportionNetWeight = true;

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(0m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(500m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1, dummy2 });

			var manager = new WeightApportionManager();
			manager.ApportionAll(weightHolderMock.Object, null);

			AssertWeightApportionee(dummy1, 2m, 0m, string.Empty, 200m, Core.Constants.Weight.Grams);
			AssertWeightApportionee(dummy2, 3m, 0m, string.Empty, 300m, Core.Constants.Weight.Grams);
		}

		public void TestApportionAll_CannotConvertNetWeight()
		{
			var dummy1 = new WeightApportioneeDummy();
			dummy1.Amount = 200m;
			dummy1.Weight = 0m;
			dummy1.WeightUQ = ZString.Empty;
			dummy1.NetWeight = 0m;
			dummy1.NetWeightUQ = ZString.Empty;
			dummy1.NeedToApportionNetWeight = true;

			var weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(1m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(900m, "XXX"));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1 });

			var manager = new WeightApportionManager();
			AssertNoExceptionThrown(() => manager.ApportionAll(weightHolderMock.Object, null));
			AssertWeightApportionee(dummy1, 200m, 1m, Core.Constants.Weight.Kilograms, 0m, ZString.Empty);

			dummy1.Weight = 0m;
			dummy1.WeightUQ = ZString.Empty;
			dummy1.NetWeight = 0m;
			dummy1.NetWeightUQ = ZString.Empty;

			weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(0.00001m, Core.Constants.Weight.Milligrams));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(10000m, Core.Constants.Weight.Kilograms));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1 });

			AssertNoExceptionThrown(() => manager.ApportionAll(weightHolderMock.Object, null));
			AssertWeightApportionee(dummy1, 200m, 0.00001m, Core.Constants.Weight.Milligrams, 0m, ZString.Empty);

			dummy1.Weight = 0m;
			dummy1.WeightUQ = ZString.Empty;
			dummy1.NetWeight = 0m;
			dummy1.NetWeightUQ = ZString.Empty;

			weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(1m, "XXX"));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(900m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1 });

			AssertNoExceptionThrown(() => manager.ApportionAll(weightHolderMock.Object, null));
			AssertWeightApportionee(dummy1, 200m, 1m, "XXX", 0m, ZString.Empty);

			var dummy2 = new WeightApportioneeDummy();
			dummy2.Amount = 300m;
			dummy2.Weight = ZDecimal.Zero;
			dummy2.WeightUQ = Core.Constants.Weight.Grams;
			dummy2.NetWeight = ZDecimal.Zero;
			dummy2.NetWeightUQ = Core.Constants.Weight.Kilograms;
			dummy2.NeedToApportionNetWeight = true;

			weightHolderMock = new Mock<IWeightHolder>();
			weightHolderMock.Setup(m => m.TotalWeight).Returns(new ZWeight(1m, "XXX"));
			weightHolderMock.Setup(m => m.TotalNetWeight).Returns(new ZWeight(900m, Core.Constants.Weight.Grams));
			weightHolderMock.Setup(m => m.AllApportionees).Returns(new IWeightApportionee[] { dummy1, dummy2 });

			AssertNoExceptionThrown(() => manager.ApportionAll(weightHolderMock.Object, null));
			AssertWeightApportionee(dummy1, 200m, 0m, ZString.Empty, 0m, ZString.Empty);
			AssertWeightApportionee(dummy2, 300m, 0m, ZString.Empty, 0m, Core.Constants.Weight.Kilograms);
		}

		void AssertWeightApportionee(WeightApportioneeDummy dummy, ZDecimal amount, ZDecimal weight, ZString weightUQ)
		{
			AssertEquals("Amount", amount, dummy.Amount);
			AssertEquals("Weight", weight, dummy.Weight);
			AssertEquals("WeightUQ", weightUQ, dummy.WeightUQ);
		}

		void AssertWeightApportionee(WeightApportioneeDummy dummy, ZDecimal amount, ZDecimal weight, ZString weightUQ, ZDecimal netWeight, ZString netWeightUQ)
		{
			AssertWeightApportionee(dummy, amount, weight, weightUQ);
			AssertEquals("NetWeight", netWeight, dummy.NetWeight);
			AssertEquals("NetWeightUQ", netWeightUQ, dummy.NetWeightUQ);
		}

		class WeightApportioneeDummy : IWeightApportionee
		{
			#region IWeightApportionee Members

			public ZDecimal Amount
			{
				get { return amount; }
				set { amount = value; }
			}
			ZDecimal amount;

			public ZDecimal Weight
			{
				get { return weight; }
				set { weight = value; }
			}
			ZDecimal weight;

			public ZString WeightUQ
			{
				get { return weightUQ; }
				set { weightUQ = value; }
			}
			ZString weightUQ;

			public ZDecimal NetWeight
			{
				get { return netWeight; }
				set { netWeight = value; }
			}
			ZDecimal netWeight;

			public ZString NetWeightUQ
			{
				get { return netWeightUQ; }
				set { netWeightUQ = value; }
			}
			ZString netWeightUQ;

			public bool NeedToApportionNetWeight
			{
				get { return needToApportionNetWeight; }
				set { needToApportionNetWeight = value; }
			}
			bool needToApportionNetWeight = true;

			public ZDecimal MinimumReapportionedLineWeight
			{
				get { return minimumReapportionedWeight; }
				set { minimumReapportionedWeight = value; }
			}
			ZDecimal minimumReapportionedWeight = 0m;

			#endregion
		}
	}
}
