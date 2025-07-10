using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWarehouseTransactionLineCollectionTest : TestCaseWithFactory
	{
		public virtual void TestConstructor()
		{
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine[] lines = new WhsWarehouseTransactionLine[3] { line1, line2, line3 };
			WhsWarehouseTransactionLineCollection collection = new WhsWarehouseTransactionLineCollection();
			AssertEquals(0, collection.Count);
			collection = WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			AssertEquals(3, collection.Count);
			collection = WhsWarehouseTransactionLineCollection.GetNew(line1);
			AssertEquals(1, collection.Count);
			collection = WhsWarehouseTransactionLineCollection.GetNew(lines);
			AssertEquals(3, collection.Count);
			AssertEquals(true, collection.Contains(line1));
			AssertEquals(true, collection.Contains(line2));
			AssertEquals(true, collection.Contains(line3));
		}

		public virtual void TestIndexer()
		{
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLineCollection collection =
				WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			AssertEquals(line1, collection[0]);
			AssertEquals(line2, collection[1]);
			collection[0] = line3;
			AssertEquals(line3, collection[0]);
			AssertEquals(false, collection.Contains(line1));
			AssertEquals(true, collection.Contains(line2));
			AssertEquals(true, collection.Contains(line3));
		}

		public virtual void TestAdd()
		{
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLineCollection collection = new WhsWarehouseTransactionLineCollection();
			AssertEquals(0, collection.Count);
			collection.Add(line1);
			AssertEquals(1, collection.Count);
			collection.Add(line2);
			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(line1));
			AssertEquals(true, collection.Contains(line2));
		}

		public virtual void TestSort()
		{
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line3 = new WhsWarehouseTransactionLine();
			line1.Quantity = 20m;
			line2.Quantity = 25m;
			line3.Quantity = 10m;
			WhsWarehouseTransactionLineCollection collection =
				WhsWarehouseTransactionLineCollection.GetNew(line1, line2, line3);
			AssertEquals("Precondition", line1, collection[0]);
			AssertEquals("Precondition", line2, collection[1]);
			AssertEquals("Precondition", line3, collection[2]);
			collection.Sort(new SortLinesByQuantity());
			AssertEquals(line3, collection[0]);
			AssertEquals(line1, collection[1]);
			AssertEquals(line2, collection[2]);
		}

		public virtual void TestClone()
		{
			WhsWarehouseTransactionLine line1 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLine line2 = new WhsWarehouseTransactionLine();
			WhsWarehouseTransactionLineCollection collection =
				WhsWarehouseTransactionLineCollection.GetNew(line1, line2);
			WhsWarehouseTransactionLineCollection collection1 = collection.Clone();
			AssertEquals(2, collection1.Count);
			AssertEquals(true, collection1.Contains(line1));
			AssertEquals(true, collection1.Contains(line2));
		}

		public virtual void TestCount()
		{
			// Count is tested by all these Tests so no need to test again
			Assert(true);
		}

		public virtual void TestContains()
		{
			// Contains() is tested by all these Tests so no need to test again
			Assert(true);
		}

		public virtual void TestGetEnumerator()
		{
			WhsWarehouseTransactionLineCollection collection = new WhsWarehouseTransactionLineCollection();
			AssertNotNull(collection.GetEnumerator());
		}

		//public class TestWhsWarehouseTransactionLine : WhsWarehouseTransactionLine
		//{
		//    #region IWarehouseTransactionLine Members
		//    public Enterprise.MasterFiles.Business.OrgAddress Warehouse
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.Warehouse getter implementation
		//            return null;
		//        }
		//    }
		//    public Enterprise.MasterFiles.Business.OrgSupplierPart Product
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.Product getter implementation
		//            return null;
		//        }
		//    }
		//    public CargoWise.Types.ZDecimal Quantity
		//    {
		//        get { return fQuantity; }
		//        set { fQuantity = value; }
		//    }
		//    CargoWise.Types.ZDecimal fQuantity;
		//    public CargoWise.Types.ZString QuantityUnit
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.QuantityUnit getter implementation
		//            return new CargoWise.Types.ZString();
		//        }
		//    }
		//    public CargoWise.Types.ZString EntryKey
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.EntryKey getter implementation
		//            return new CargoWise.Types.ZString();
		//        }
		//    }
		//    public CargoWise.Types.ZShort EntryLineNumber
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.EntryLineNumber getter implementation
		//            return new CargoWise.Types.ZShort();
		//        }
		//    }
		//    public CargoWise.Types.ZString PartAttrib1
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib1 getter implementation
		//            return new CargoWise.Types.ZString();
		//        }
		//    }
		//    public CargoWise.Types.ZString PartAttrib2
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib2 getter implementation
		//            return new CargoWise.Types.ZString();
		//        }
		//    }
		//    public CargoWise.Types.ZString PartAttrib3
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib3 getter implementation
		//            return new CargoWise.Types.ZString();
		//        }
		//    }
		//    public bool HasErrors
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.HasErrors getter implementation
		//            return false;
		//        }
		//    }
		//    public bool HasWarnings
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.HasWarnings getter implementation
		//            return false;
		//        }
		//    }
		//    public NotificationCollection WarehouseProblems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.WarehouseProblems getter implementation
		//            return null;
		//        }
		//    }
		//    public NotificationCollection QuantityProblems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.QuantityProblems getter implementation
		//            return null;
		//        }
		//    }
		//    public NotificationCollection EntryKeyProblems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.EntryKeyProblems getter implementation
		//            return null;
		//        }
		//    }
		//    public NotificationCollection PartAttrib1Problems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib1Problems getter implementation
		//            return null;
		//        }
		//    }
		//    public NotificationCollection PartAttrib2Problems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib2Problems getter implementation
		//            return null;
		//        }
		//    }
		//    public NotificationCollection PartAttrib3Problems
		//    {
		//        get
		//        {
		//            // TODO:  Add TestIWarehouseTransactionLine.PartAttrib3Problems getter implementation
		//            return null;
		//        }
		//    }
		//	  #endregion
		//}
		public class SortLinesByQuantity : IComparer
		{
			int IComparer.Compare(object o1, object o2)
			{
				IWhsWarehouseTransactionLine line1 = (IWhsWarehouseTransactionLine)o1;
				IWhsWarehouseTransactionLine line2 = (IWhsWarehouseTransactionLine)o2;
				return line1.Quantity.CompareTo(line2.Quantity);
			}
		}
	}
}
