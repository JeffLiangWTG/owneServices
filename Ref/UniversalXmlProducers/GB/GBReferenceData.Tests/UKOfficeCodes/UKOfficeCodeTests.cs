using CargoWise.RefDbRepo.GBReferenceData.Business.UKOfficeCodes;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.UKOfficeCodes.Tests
{
	[TestFixture]
	sealed class UKOfficeCodeTests
	{
		[Test]
		public void IsValid()
		{
			var model = new UKOfficeCode();
			Assert.That(model.IsValid, Is.False);
			model.Code = "Code";
			Assert.That(model.IsValid, Is.False);
			model.UsualName = "Usual Name";
			Assert.That(model.IsValid, Is.True);
		}

		[Test]
		public void Description()
		{
			var model = new UKOfficeCode();
			Assert.That(model.Description, Is.Empty);
			model.Code = "Code";
			Assert.That(model.Description, Is.Empty);
			model.UsualName = "Usual Name";
			Assert.That(model.Description, Is.EqualTo("Usual Name"));
			model.City = "City";
			Assert.That(model.Description, Is.EqualTo("Usual Name City"));
		}
	}
}
