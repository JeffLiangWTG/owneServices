using System;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CNReferenceData.Tests
{
	[TestFixture]
	public class ExtensionHelperFixture
	{
		[Test]
		public void GetThirdWednesdayByMonthYear()
		{
			Assert.AreEqual(new DateTime(2017, 12, 20), new DateTime(2017, 12, 1).GetThirdWednesdayByMonthYear());
			Assert.AreEqual(new DateTime(2017, 12, 20), new DateTime(2017, 12, 11).GetThirdWednesdayByMonthYear());
			Assert.AreEqual(new DateTime(2017, 12, 20), new DateTime(2017, 12, 21).GetThirdWednesdayByMonthYear());
			Assert.AreEqual(new DateTime(2017, 12, 20), new DateTime(2017, 12, 31).GetThirdWednesdayByMonthYear());
		}
	}
}
