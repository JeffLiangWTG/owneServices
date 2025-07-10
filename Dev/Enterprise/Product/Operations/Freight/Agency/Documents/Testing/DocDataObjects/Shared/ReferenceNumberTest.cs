using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ReferenceNumber))]
	class ReferenceNumberTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var lookups = new AgencyHouseBillLookups(Factory);

			return new ReferenceNumber
			{
				Value = "ABC123",
				Type = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = "ABC"
				},
				CountryOfIssue = new Country(Factory, lookups.Countries)
				{
					Code = "AU"
				}
			};
		}
	}
}
