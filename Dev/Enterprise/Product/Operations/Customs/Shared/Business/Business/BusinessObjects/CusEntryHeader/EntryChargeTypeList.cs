//using System;
//
//namespace Enterprise.Customs.Business
//{
//    public class EntryChargeTypeElement : Enterprise.Registry.Business.Customs.EntryChargeTypeElement
//    {
//        public EntryChargeTypeElement(string Code, string Description, bool IsPaidWhenMessageClears)
//            : base(Code, Description, IsPaidWhenMessageClears)
//        {
//        }
//    }
//    public class EntryChargeTypeList : Enterprise.Registry.Business.Customs.EntryChargeTypeList
//    {
//        public new EntryChargeTypeElement this[int index]
//        {
//            get { return (EntryChargeTypeElement) base[index]; }
//            set {  base[index] = value; }
//        }
//        protected override Type ExpectedElementType
//        {
//            get { return typeof(EntryChargeTypeElement); }
//        }
//    }
//}
//#region Test
//#if DEBUG
//namespace Enterprise.Customs.Business.Testing
//{
//    using NUnit.Framework;
//    using Enterprise.ZArchitecture;
//    using Enterprise.ZArchitecture.Business;
//    using Enterprise.ZArchitecture.Business.Testing;

//    public abstract class EntryChargeTypeListTestCase : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
//    {
//    }

//    internal class EntryChargeTypeListTest : TestCase
//    {
//        public void TestAddWith3Params()
//        {
//            List.Add("Code", "Description", true);
//            EntryChargeTypeElement Element = (EntryChargeTypeElement) List[0];
//            AssertEquals("Code", "Code", Element.Code);
//            AssertEquals("Description", "Description", Element.Description);
//            AssertEquals("IsPaidWhenMessageClears", true, Element.IsPaidWhenMessageClears);
//        }

//        public void TestEntryChargeTypeElement()
//        {
//            EntryChargeTypeElement Element = new EntryChargeTypeElement("Code", "Description", true);
//            AssertEquals("Code", "Code", Element.Code);
//            AssertEquals("Description", "Description", Element.Description);
//            AssertEquals("IsPaidWhenMessageClears", true, Element.IsPaidWhenMessageClears);

//            EntryChargeTypeElement Element2 = new EntryChargeTypeElement("1", "2", false);
//            AssertEquals("Code", "1", Element2.Code);
//            AssertEquals("Description", "2", Element2.Description);
//            AssertEquals("IsPaidWhenMessageClears", false, Element2.IsPaidWhenMessageClears);
//        }

//        public void TestUsingEntryChargeTypeElementsInList()
//        {
//            EntryChargeTypeElement Element = new EntryChargeTypeElement("Code", "Description", true);
//            List.Add(Element);
//            AssertEquals("Count", 1, List.Count);
//            AssertEquals("in list", Element, List[0]);

//            foreach (EntryChargeTypeElement E in List)
//            {
//                AssertEquals("Code", "Code", E.Code);
//                AssertEquals("Description", "Description", E.Description);
//                AssertEquals("IsPaidWhenMessageClears", true, E.IsPaidWhenMessageClears);
//            }
//        }

//        [ExpectException(typeof(NotSupportedException))]
//        public void TestCannotAddNormalCodeDescriptionPairs()
//        {
//            List.Add(new CodeDescriptionPair("a", "b"));
//        }

//        [ExpectException(typeof(NotSupportedException))]
//        public void TestCannotAddPairs()
//        {
//            List.AddPair("a", "b");
//        }

//        [ExpectException(typeof(NotSupportedException))]
//        public void TestCannotAddPairsWithPK()
//        {
//            List.AddPair(Guid.NewGuid(), "a", "b");
//        }

//        EntryChargeTypeList List
//        {
//            get
//            {
//                if (fList == null) fList = new EntryChargeTypeList();
//                return fList;
//            }
//        }
//        EntryChargeTypeList fList;
//    }
//}
//#endif
//#endregion

