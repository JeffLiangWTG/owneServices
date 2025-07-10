using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class ReservedFieldCollectionTest<T> : CusCodeDataCollectionTest<T> where T : ReservedField
	{
		[ExpectNoExceptions]
		public void TestCY_Type()
		{
			var coll = GetCusCodeDataCollection();
			var element = (CusCodeData)coll.AddNew();
			NUnit.Framework.Assert.That(element.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.ReservedField).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			var reservedFields = GetCusCodeDataCollection();
			for (var i = 0; i < 11; i++)
			{
				reservedFields.AddNew();
				NUnit.Framework.Assert.That(reservedFields.AllowNew, NUnit.Framework.Is.EqualTo(reservedFields.Count < 10), ZString.Format("count:{0}", reservedFields.Count).ToString());
			}
		}

		[ExpectNoExceptions]
		public void TestOrderReservedFields()
		{
			var coll = (ReservedFieldCollection<T>)GetCusCodeDataCollection();
			coll.RemoveAll();
			var element1 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element2 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element3 = (CusCodeData)GetNewElementToAddToTheCollection();
			var element4 = (CusCodeData)GetNewElementToAddToTheCollection();
			coll.Add(element1);
			coll.Add(element2);
			coll.Add(element3);
			coll.Add(element4);
			element1.CY_Code = "A";
			element2.CY_Code = "B";
			element3.CY_Code = "1";
			element4.CY_Code = "0";
			var elements = coll.OrderList;
			NUnit.Framework.Assert.That(elements.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(elements.ElementAt(0).CY_Code, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(1).CY_Code, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(2).CY_Code, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(3).CY_Code, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
			element1.CY_Code = "B";
			element2.CY_Code = "0";
			element3.CY_Code = "1";
			element4.CY_Code = "A";
			elements = coll.OrderList;
			NUnit.Framework.Assert.That(elements.Count(), NUnit.Framework.Is.EqualTo(4));
			NUnit.Framework.Assert.That(elements.ElementAt(0).CY_Code, NUnit.Framework.Is.EqualTo("0").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(1).CY_Code, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(2).CY_Code, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(elements.ElementAt(3).CY_Code, NUnit.Framework.Is.EqualTo("B").Using(CustomComparers.TypeComparison));
		}
	}
}
