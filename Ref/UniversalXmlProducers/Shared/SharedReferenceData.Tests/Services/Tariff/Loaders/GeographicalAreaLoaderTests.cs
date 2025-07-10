using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders.Tests
{
	[TestFixture]
	sealed class GeographicalAreaLoaderTests
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var element = TestHelper.GetXmlElement("GeographicalArea", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_GeographicalArea_001.xml");
			var model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model, Is.Not.Null);
			Assert.That(model.HJID, Is.EqualTo("23613"));
			Assert.That(model.Description, Is.EqualTo("South Africa"));
			Assert.That(model.GeographicalAreaId, Is.EqualTo("ZA"));
			Assert.That(model.StartDate, Is.Not.Null.And.EqualTo(new DateTime(1986, 09, 27)));
			Assert.That(model.EndDate, Is.Not.Null.And.EqualTo(new DateTime(2050, 12, 27, 13, 14, 15)));
			Assert.That(model.Countries, Is.Not.Null);
			Assert.That(model.Countries.Count, Is.EqualTo(0));
			Assert.That(model.Descriptions, Is.Not.Null);
			Assert.That(model.Descriptions.Count, Is.GreaterThan(0));

			element = TestHelper.GetXmlElement("GeographicalArea", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_GeographicalArea_001.xml", skip: 2);
			model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.GeographicalAreaId, Is.EqualTo("1011"));
			Assert.That(model.Countries, Is.Not.Null);
			Assert.That(model.Countries.Count, Is.GreaterThan(0));
			Assert.That(model.Descriptions, Is.Not.Null);
			Assert.That(model.Descriptions.Count, Is.GreaterThan(0));
			var country = model.Countries.FirstOrDefault(x => x.GeographicalAreaHjid == "23533");
			Assert.That(country, Is.Not.Null);
			Assert.That(country.StartDate, Is.EqualTo(new DateTime(1992, 01, 01)));
			Assert.That(country.EndDate, Is.EqualTo(new DateTime(1997, 04, 28, 23, 59, 59)));
			Assert.That(country.HJID, Is.EqualTo("24995"));

			element = TestHelper.GetXmlElement("GeographicalArea", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.UT_GeographicalArea_001.xml", skip: 5);
			model = sharedLoader.ConvertXElementToModel(element);

			Assert.That(model.GeographicalAreaId, Is.EqualTo("BF"));
			Assert.That(model.Countries, Is.Not.Null);
			Assert.That(model.Countries.Count, Is.EqualTo(0));
			Assert.That(model.Descriptions, Is.Not.Null);
			Assert.That(model.Descriptions.Count, Is.EqualTo(4));
			var description = model.Descriptions.FirstOrDefault(x => x.HJID == "8308301");
			Assert.That(description, Is.Not.Null);
			Assert.That(description.HJID, Is.EqualTo("8308301"));
			Assert.That(description.Description, Is.EqualTo("Belgien"));
			Assert.That(description.LanguageCode, Is.EqualTo("DE"));
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			sharedLoader = new GeographicalAreaLoaderTester();
		}

		class GeographicalAreaLoaderTester : GeographicalAreaLoader
		{
			public GeographicalArea ConvertXElementToModel(XElement element)
			{
				var model = CreateMinimumModelFromXElement(element);
				LoadMetaInfo(model, element);
				model.ProcessUpdate(element);
				return model;
			}

			public new bool IsValidElement(XElement element) => base.IsValidElement(element);
		}

		GeographicalAreaLoaderTester sharedLoader;
		#endregion
	}
}
