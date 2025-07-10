using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusCodeDataCollectionTest<T> : BusinessObjectCollectionTestCase where T : CusCodeData
	{
		public void TestContainsCode()
		{
			var coll = GetCusCodeDataCollection();

			AssertEquals(false, coll.ContainsCode("X"));
			var element = (CusCodeData)coll.AddNew();
			element.CY_Code = "X";

			AssertEquals(true, coll.ContainsCode("X"));
		}

		public void TestSetDefaultValuesForNewChild()
		{
			var coll = GetCusCodeDataCollection();
			var element = (CusCodeData)coll.AddNew();
			AssertEquals(coll.CY_Type, element.CY_Type);
			AssertEquals(coll.Master.PK, element.Parent.PK);
		}

		public void TestCreateRelationshipFilter()
		{
			var coll = GetCusCodeDataCollection();
			coll.RemoveAll();

			var element1 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element2 = (CusCodeData)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			element2.CY_Type = "~~~";
			AssertNotEquals("PreCondition:CY_Type passed into the collection is not the same as element2's", element2.CY_Type, coll.CY_Type);

			coll.Load();
			AssertEquals("contains element1", true, coll.Contains(element1));
			AssertEquals("should not contain element2", false, coll.Contains(element2));
		}

		public void TestGetElementHaving()
		{
			var coll = GetCusCodeDataCollection();
			coll.RemoveAll();

			var element1 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element2 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element3 = (CusCodeData)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			element1.CY_Code = "A";
			element2.CY_Code = "B";
			element3.CY_Code = "A";

			var elements = coll.GetElementsHaving("A");
			AssertEquals(2, elements.Length);
			AssertEquals("GetElementsHaving", element1, elements[0]);
			AssertEquals("GetElementsHaving", element3, elements[1]);
			AssertEquals("GetFirstElementHaving", element1, coll.GetFirstElementHaving("A"));
			AssertEquals("GetFirstElementHaving", element2, coll.GetFirstElementHaving("B"));
		}

		public void TestGetStringDataHaving()
		{
			var coll = GetCusCodeDataCollection();
			coll.RemoveAll();

			var element1 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element2 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element3 = (CusCodeData)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			element1.CY_Code = "A";
			element2.CY_Code = "B";
			element3.CY_Code = "A";

			element1.CY_Data = "AAAA";
			element2.CY_Data = "BBBB";
			element3.CY_Data = "CCCC";

			AssertEquals("GetStringDataHaving", "BBBB", coll.GetStringDataHaving("B"));

			ErrorReporter.Clear();

			try
			{
				coll.GetStringDataHaving("A");
			}
			finally
			{
				string developerException = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				AssertEquals("There are 2 elements in this collection having code, A", developerException);
			}
		}

		public void TestSetStringValueToCodeHaving()
		{
			var coll = GetCusCodeDataCollection();
			coll.RemoveAll();

			AssertEquals("There are no elements", 0, coll.Count);
			coll.SetStringValueHavingCodeOrDeleteIfValueEmpty("A", "BBBB");
			AssertEquals("There is one element", 1, coll.Count);

			AssertEquals("A", coll[0].CY_Code);
			AssertEquals("BBBB", coll[0].CY_Data);

			coll.SetStringValueHavingCodeOrDeleteIfValueEmpty("A", "CCCC");
			AssertEquals("There is one element", 1, coll.Count);
			AssertEquals("A", coll[0].CY_Code);
			AssertEquals("CCCC", coll[0].CY_Data);

			var duplicate = coll.AddNew("A");
			duplicate.CY_Data = "DDDD";

			ErrorReporter.Clear();

			try
			{
				coll.SetStringValueHavingCodeOrDeleteIfValueEmpty("A", "");
			}
			finally
			{
				string developerException = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				AssertEquals("There are 2 elements in this collection having code, A", developerException);
			}

			AssertEquals("as an empty value is assigned all the elements with the code have been deleted", 0, coll.Count);
			AssertEquals(true, duplicate.IsDeleted);

			var code1 = coll.AddNew("A");
			code1.CY_Data = "DDDD";
			var code2 = coll.AddNew("A");
			code2.CY_Data = "CCCD";
			coll.SetStringValueHavingCodeOrDeleteIfValueEmpty("A", "KDSD", true);
			AssertEquals(1, coll.Count);
			var code = coll[0];
			AssertEquals("A", code.CY_Code);
			AssertEquals("KDSD", code.CY_Data);
			AssertEquals(true, (code == code1 ? code2 : code1).IsDeleted);
		}

		protected abstract CusCodeDataCollection<T> GetCusCodeDataCollection();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetCusCodeDataCollection();
		}
	}
}
