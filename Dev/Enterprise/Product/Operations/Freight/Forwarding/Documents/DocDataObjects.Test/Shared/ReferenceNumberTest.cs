using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	[TestedType(typeof(ReferenceNumber))]
	sealed class ReferenceNumberTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var lookups = new HouseBillLookups(Factory);

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
