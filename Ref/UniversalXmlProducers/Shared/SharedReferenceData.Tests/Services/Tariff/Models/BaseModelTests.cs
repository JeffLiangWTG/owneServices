using System;
using System.Text;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.Tests
{
	[TestFixture]
	sealed class BaseModelTests
	{
		[Test]
		public void GetLatest()
		{
			var currentModel = new LoadDataOne();
			var sameModel = new LoadDataOne();
			var firstModel = new LoadDataOne();
			var lastModel = new LoadDataOne();
			var noDateModel = new LoadDataOne();
			var wrongModel = new LoadDataTwo();

			firstModel.OpDate = new DateTime(2020, 08, 13);
			currentModel.OpDate = new DateTime(2020, 08, 14);
			sameModel.OpDate = new DateTime(2020, 08, 14);
			lastModel.OpDate = new DateTime(2020, 08, 15);
			wrongModel.OpDate = new DateTime(2020, 08, 14);

			Assert.That(currentModel.GetLatest(firstModel), Is.EqualTo(currentModel));
			Assert.That(currentModel.GetLatest(sameModel), Is.EqualTo(currentModel));
			Assert.That(currentModel.GetLatest(lastModel), Is.EqualTo(lastModel));

			Assert.That(currentModel.GetLatest(noDateModel), Is.EqualTo(currentModel));
			Assert.That(noDateModel.GetLatest(currentModel), Is.EqualTo(currentModel));

			Assert.That(currentModel.GetLatest(wrongModel), Is.EqualTo(currentModel));
		}

		[Test]
		public void IsValid()
		{
			var errorCollector = new StringBuilder();
			var model = new CommonData() { HJID = "12345", Key = "ABC" };

			var result = model.IsValid(errorCollector, "UnitTest");

			Assert.That(result, Is.EqualTo(false));
			Assert.That(errorCollector.ToString(), Is.EqualTo("MetaInfo data is missing for HJID: 12345\r\n"));

			errorCollector.Clear();
			model.OpType = MetaInfoOpTypes.Created;
			model.OpDate = new DateTime(2022, 3, 4);

			result = model.IsValid(errorCollector, "UnitTest");

			Assert.That(result, Is.EqualTo(true));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty));
		}
	}
}
