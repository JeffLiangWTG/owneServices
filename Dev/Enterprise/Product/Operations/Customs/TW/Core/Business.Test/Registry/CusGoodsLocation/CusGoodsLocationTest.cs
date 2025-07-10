using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusGoodsLocation))]
	sealed class CusGoodsLocationTest : RegistryBusinessObjectTemplateTestCase<CusGoodsLocation>
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestGoodsLocation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeList = helper.CreateCusCodeList("TW", "CUSOF", "FF", "FF THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(codeList.PK, TransportTypeList.Codes.Sea);
			var facility = helper.CreateCusCodeList("TW", "FAC", "ANP0063D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", "FAC", "TW");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "FF");
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "CC");
			Factory.Save();

			currentElement.GoodsLocation = "600";
			NUnit.Framework.Assert.That(currentElement.GoodsLocation, NUnit.Framework.Is.EqualTo("600").Using(CustomComparers.TypeComparison));
			currentElement.CustomsOffice = "CC";
			currentElement.GoodsLocation = "ANP0060D";
			NUnit.Framework.Assert.That(currentElement.GoodsLocationItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "DD";
			NUnit.Framework.Assert.That(currentElement.GoodsLocationItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "AA";
			NUnit.Framework.Assert.That(currentElement.GoodsLocationItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "CC";
			currentElement.GoodsLocation = "ANP0063D";
			NUnit.Framework.Assert.That(currentElement.GoodsLocationItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "FF";
			NUnit.Framework.Assert.That(currentElement.GoodsLocationItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
		}

		[ExpectNoExceptions]
		public void TestCustomsOffice()
		{
			currentElement.CustomsOffice = "A";
			NUnit.Framework.Assert.That(currentElement.CustomsOffice, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			currentElement.CustomsOffice = "CC";
			NUnit.Framework.Assert.That(currentElement.CustomsOfficeItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "DD";
			NUnit.Framework.Assert.That(currentElement.CustomsOfficeItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			currentElement.CustomsOffice = "AA";
			NUnit.Framework.Assert.That(currentElement.CustomsOfficeItem, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			currentElement.MessageType = "IMP";
			NUnit.Framework.Assert.That(currentElement.MessageType, NUnit.Framework.Is.EqualTo("IMP").Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region override
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new CusGoodsLocationCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override CusGoodsLocation GetBusinessObjectToClone() => (CusGoodsLocation)GetNewBusinessObject();
		protected override CusGoodsLocation GetBusinessObjectToSerialise() => (CusGoodsLocation)GetNewBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			collection = new CusGoodsLocationCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			currentElement = collection.AddNew();
		}

		CusGoodsLocation currentElement;
		CusGoodsLocationCollection collection;
		#endregion
	}
}
