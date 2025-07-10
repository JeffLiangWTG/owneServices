using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(PersonMergePreview))]
	public class PersonMergeResultTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var merge = new PersonMergePreview();

			var namePair = new CodeDescriptionPair("Name", "Edmon Wilson");
			var homePair = new CodeDescriptionPair("Home", "0449743938");
			var securityPair = new CodeDescriptionPair("Security", "1283433");
			var addressPair = new CodeDescriptionPair("Address", "72 O'Riordan Street");

			merge.DiscardedProperties.Add(namePair);
			merge.DiscardedProperties.Add(homePair);
			merge.IdenticalProperties.Add(securityPair);
			merge.CopiedProperties.Add(addressPair);

			CombineAssertions(() =>
			{
				AssertEquals("Name is discarded", true, merge.DiscardedProperties.Contains(namePair));
				AssertEquals("Home is discarded", true, merge.DiscardedProperties.Contains(homePair));
				AssertEquals("Security is identical", true, merge.IdenticalProperties.Contains(securityPair));
				AssertEquals("Address is copied", true, merge.CopiedProperties.Contains(addressPair));

				AssertEquals("2 fields should be discarded", 2, merge.DiscardedProperties.Count);
				AssertEquals("1 field should be identical", 1, merge.IdenticalProperties.Count);
				AssertEquals("1 field should be copied", 1, merge.CopiedProperties.Count);
			});
		}

		public void TestWhiteList_HasCorrectNumOfFields()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();

			Factory.Save();

			var merger = new PersonMergePreview();

			var numberOfFieldsToMerge = merger.ColumnGroupingsToMerge.SelectMany(i => i).ToList().Count;

			const int numOfExcludedFields = 10;
			var expectedNumberOfPERFieldsToMerge = GlbPersonSchema.All.Count - numOfExcludedFields;

			AssertEquals(@"The number of columns on the GlbPerson schema has changed, which will affect the behaviour when merging Persons.
If you have added a new column to the GlbPerson schema, then either:
	Add the column name to the appropriate grouping in ProposedPersonMerge.PERColumnGroupingsToMerge if you wish for the column to be considered during merging. OR
	Increment numOfExcludedFields in this test to indicate that the new column should be ignored during merging", expectedNumberOfPERFieldsToMerge, numberOfFieldsToMerge);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => new PersonMergePreview();

		#endregion
	}
}
