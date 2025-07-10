using System.Collections.Generic;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.BusinessObjects
{
	[TestFixture]
	class ConditionTest
	{
		[Test]
		public void GetDocumentType()
		{
			var condition = EmptyCondition();
			Assert.AreEqual(typeof(CargoWise.RefDbRepo.FRReferenceData.Services.Condition), condition.GetType());
		}

		Condition EmptyCondition()
		{
			return new Condition(string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, new List<Component>());
		}
	}
}
