using System;
using System.Linq;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class CustomsConditionListFixture
	{
		[Test]
		public void TestGet()
		{
			var list = CustomsConditionList.All;
			foreach (var pair in list)
			{
				Assert.NotNull(CustomsConditionList.Get(pair.Value.Code.Last()));
			}

			Assert.Throws<NotSupportedException>(() => CustomsConditionList.Get('.'));
		}
	}
}
