using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolStmNoteCollectionView))]
	sealed class ForwardingConsolStmNoteCollectionViewTest : BusinessObjectCollectionViewTestCase<ForwardingConsolStmNoteCollectionView>
	{
		public void TestParent()
		{
			var view = new ForwardingConsolStmNoteCollectionView(Consol);
			AssertEquals(Consol, view.Parent);
		}

		#region Implementation

		protected override ForwardingConsolStmNoteCollectionView GetCollectionToTest() => new ForwardingConsolStmNoteCollectionView(Consol);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var note = Factory.New<ForwardingConsolStmNote>();
			note.ST_Table = Consol.TableName;
			note.ST_ParentID = Consol.PK;

			return note;
		}

		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());
		ForwardingConsol consol;

		#endregion
	}
}
