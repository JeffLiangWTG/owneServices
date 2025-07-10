using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;
	using NUnit.Framework;

	[TestedType(typeof(NZMMessageCollection))]
	public class NZMMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			collection.AddNew();
			var message = collection[0];
			AssertNotNull("NZMMessageCollection[0]", message);
		}

		public void TestAddNewSpecificType()
		{
			var message = collection.AddNew();
			AssertNotNull("NZMMessageCollection.AddNew()", message);
		}

		public void TestFilter()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsCMD;
			message.EM_LinkUniqueID = declaration.PK;

			message = Factory.New<NZMMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.NZMAFeBACCa;
			message.EM_LinkUniqueID = declaration.PK;

			collection.Load();
			AssertEquals("Count", 1, collection.Count);
			AssertEquals("Collection contains NZM messages only", message, collection[0]);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			declaration = Factory.New<JobDeclaration>();
			return new NZMMessageCollection(TestDataBuilder.GetMAFMessaging(declaration));
		}

		protected override void SetUp()
		{
			base.SetUp();

			collection = (NZMMessageCollection)GetCollectionToTest();
		}

		NZMMessageCollection collection;
		JobDeclaration declaration;

		#endregion
	}
}
