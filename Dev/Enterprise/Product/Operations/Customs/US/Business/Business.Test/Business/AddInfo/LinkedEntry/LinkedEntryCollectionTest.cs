using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LinkedEntryCollection))]
	sealed class LinkedEntryCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNewEntry()
		{
			var entry = Declaration.LinkedEntryNumbers.AddNew();
			AssertEquals("B7_Type", CusAddInfoTypeAttribute.Codes.USLinkedEntry, entry.B7_Type);
			AssertEquals("B7_ParentTableCode", JobDeclarationSchema.Constants.Prefix, entry.B7_ParentTableCode);
			AssertEquals("B7_ParentID", Declaration.PK, entry.B7_ParentID);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => Declaration.LinkedEntryNumbers;

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<LinkedEntry>();

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
