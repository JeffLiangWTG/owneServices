using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TransportEquipment))]
	sealed class N5167TransportEquipmentTest : NonPersistentBusinessObjectTestCase
	{
		TransportEquipment transportEquipment;
		protected override void SetUp()
		{
			transportEquipment = TransportEquipment.New(new TransportEquipmentClassTest(), 1, Factory);
			base.SetUp();
		}

		[ExpectNoExceptions]
		public void TestNew()
		{
			NUnit.Framework.Assert.That(TransportEquipment.New(null, 0, Factory), NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Business.DocumentWrappers.TransportEquipment)), "TransportEquipment - should be [null]");
			NUnit.Framework.Assert.That(transportEquipment, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.DocumentWrappers.TransportEquipment)), "TransportEquipment - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestFields()
		{
			NUnit.Framework.Assert.That(transportEquipment.ID, NUnit.Framework.Is.EqualTo("ctn no.123").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(transportEquipment.LineNo, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return TransportEquipment.New(new TransportEquipmentClassTest(), 1, Factory);
		}
	}
}
