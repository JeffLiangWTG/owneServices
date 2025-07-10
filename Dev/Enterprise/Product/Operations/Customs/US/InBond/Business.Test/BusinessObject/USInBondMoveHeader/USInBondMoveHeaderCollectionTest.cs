using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(USInBondMoveHeaderCollection))]
	public class USInBondMoveHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<USInBondMoveHeaderCollection>
	{
		public override void TestAdd()
		{
			Assert(true); //View is created from movements, add method will not be invoked actually
		}

		public override void TestDelete()
		{
			Assert(true); //View is created from movements, delete method will not be invoked actually
		}

		#region Implementation

		CusInBondHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusInBondHeader>();
				}
				return header;
			}
		}
		CusInBondHeader header;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CusInBondMoveHeader moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = Header.PK;
			moveHeader.InBondNumber = "INB789789";
			Factory.Save();

			return Factory.Load<USInBondMoveHeader>(moveHeader.PK);
		}

		protected override USInBondMoveHeaderCollection GetCollectionToTest()
		{
			var moveHeader1 = Header.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "INB234234";
			var moveHeader2 = Header.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "INB636985";
			Factory.Save();

			var collection = new USInBondMoveHeaderCollection(Factory);
			return collection;
		}

		#endregion
	}
}
