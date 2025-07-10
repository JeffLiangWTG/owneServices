using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration.Attributes;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Model
{
	abstract class NameAttributeBaseTest<TTestedType>
	{
		protected abstract Dictionary<string, string> ExpectedPropertyNameAndAttributeValue { get; }

		[Test]
		public void PropertiesShouldHaveCorrectNameAttributes()
		{
			var properties = typeof(TTestedType).GetProperties();

			Assert.That(properties.Select(p => p.Name), Is.EquivalentTo(ExpectedPropertyNameAndAttributeValue.Keys));

			foreach (var propertyNameAttributeValuePair in ExpectedPropertyNameAndAttributeValue)
			{
				var property = properties.FirstOrDefault(p => p.Name == propertyNameAttributeValuePair.Key);

				var attribute = property.GetCustomAttribute<NameAttribute>();

				Assert.That(attribute?.Names[0], Is.EqualTo(propertyNameAttributeValuePair.Value), $"{propertyNameAttributeValuePair.Key} NameAttribute value is not what is expected.");
			}
		}
	}
}
