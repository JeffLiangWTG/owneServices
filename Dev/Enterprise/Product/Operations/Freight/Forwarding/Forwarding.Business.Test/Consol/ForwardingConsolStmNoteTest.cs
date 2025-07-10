using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolStmNote))]
	sealed class ForwardingConsolStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var note = Factory.New<ForwardingConsolStmNote>();

			AssertEquals(typeof(ForwardingConsolStmNoteValidation), note.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var note = Factory.NewWithValidTestData<ForwardingConsolStmNote>();
			note.ST_Table = Consol.TableName;
			note.ST_ParentID = Consol.PK;

			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var note = factory.NewWithValidTestData<ForwardingConsolStmNote>();
			note.ST_Table = Consol.TableName;
			note.ST_ParentID = Consol.PK;

			return note;
		}

		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());
		ForwardingConsol consol;

		#endregion
	}
}
