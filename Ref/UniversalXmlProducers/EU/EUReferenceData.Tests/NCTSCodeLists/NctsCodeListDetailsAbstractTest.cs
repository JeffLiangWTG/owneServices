using System.Collections.Generic;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	abstract class NctsCodeListDetailsAbstractTest
	{
		[Test]
		public virtual void Domain()
		{
			Assert.That(nctsCodeListDetails.Domain, Is.EqualTo(Constants.UccConstants.NCTSDomain));
		}

		[Test]
		public void AttributeValues()
		{
			Assert.That(nctsCodeListDetails.AttributeValues, Is.EqualTo(ExpectedAttributeValues));
		}

		[Test]
		public void ExtraType()
		{
			Assert.That(nctsCodeListDetails.ExtraType, Is.EqualTo(ExpectedExtraType));
		}

		[SetUp]
		public void SetUp()
		{
			nctsCodeListDetails = GetNctsCodeListDetails();
		}

		protected abstract IUCCExportCodeListDetail GetNctsCodeListDetails();

		protected virtual string ExpectedExtraType => null;

		protected virtual List<(string, string)> ExpectedAttributeValues => new List<(string, string)> { };

		IUCCExportCodeListDetail nctsCodeListDetails;
	}
}
