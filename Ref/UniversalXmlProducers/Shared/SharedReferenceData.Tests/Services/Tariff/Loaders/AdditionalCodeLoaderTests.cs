using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	sealed class AdditionalCodeLoaderTests
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var element = TestHelper.GetXmlElement("AdditionalCode", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_AdditionalCode_001.xml");
			var model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.HJID, Is.EqualTo("16646"));
			Assert.That(model.Code, Is.EqualTo("C001"));
			Assert.That(model.CodeType, Is.EqualTo("C"));
			Assert.That(model.Description, Is.EqualTo("Sample Additional Code"));
			Assert.That(model.StartDate, Is.EqualTo(new DateTime(2015, 05, 14)));
			Assert.That(model.EndDate.Value, Is.EqualTo(new DateTime(2050, 12, 31, 11, 12, 13)));
			Assert.That(model.Descriptions, Is.Not.Null);
			Assert.That(model.Descriptions.Count, Is.GreaterThan(0));

			element = TestHelper.GetXmlElement("AdditionalCode", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_AdditionalCode_001.xml", skip: 1);
			model = sharedLoader.ConvertXElementToModel(element);
			Assert.That(model.Descriptions, Is.Not.Null);
			Assert.That(model.Descriptions.Count, Is.EqualTo(2));
			var description = model.Descriptions.FirstOrDefault(x => x.HJID == "8574803");
			Assert.That(description, Is.Not.Null);
			Assert.That(description.HJID, Is.EqualTo("8574803"));
			Assert.That(description.Description, Is.EqualTo("Cold finished bar: if cold formed: LQEX."));
			Assert.That(description.LanguageCode, Is.EqualTo("EN"));
		}

		[Test]
		public void IsValidElement()
		{
			var element = TestHelper.GetXmlElement("AdditionalCode", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_AdditionalCode_001.xml");
			var isValid = sharedLoader.IsValidElement(element);
			Assert.That(isValid, Is.True, "AdditonalCodes always true via base");
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			sharedLoader = new AdditionalCodeLoaderTester();
		}

		class AdditionalCodeLoaderTester : AdditionalCodeLoader
		{
			public AdditionalCode ConvertXElementToModel(XElement element)
			{
				var model = base.CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}
			public new bool IsValidElement(XElement element) => base.IsValidElement(element);
		}

		AdditionalCodeLoaderTester sharedLoader;
		#endregion
	}
}
