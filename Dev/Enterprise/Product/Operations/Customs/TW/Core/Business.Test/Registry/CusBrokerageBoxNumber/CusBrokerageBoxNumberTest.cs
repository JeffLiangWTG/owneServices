using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusBrokerageBoxNumber))]
	sealed class CusBrokerageBoxNumberTest : RegistryBusinessObjectTemplateTestCase<CusBrokerageBoxNumber>
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestBoxNumber()
		{
			currentElement.BoxNumber = "600";
			NUnit.Framework.Assert.That(currentElement.BoxNumber, NUnit.Framework.Is.EqualTo("600").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeArea()
		{
			currentElement.CustomsOfficeArea = "A";
			NUnit.Framework.Assert.That(currentElement.CustomsOfficeArea, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsDefaultBoxNumber()
		{
			currentElement.IsDefaultBoxNumber = ZBool.True;
			NUnit.Framework.Assert.That(currentElement.IsDefaultBoxNumber, NUnit.Framework.Is.EqualTo(ZBool.True));
		}

		#endregion
		#region override
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override BusinessObject GetNewBusinessObject()
		{
			var coll = new CusBrokerageBoxNumberCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			return coll.AddNew();
		}

		protected override CusBrokerageBoxNumber GetBusinessObjectToClone() => (CusBrokerageBoxNumber)GetNewBusinessObject();
		protected override CusBrokerageBoxNumber GetBusinessObjectToSerialise() => (CusBrokerageBoxNumber)GetNewBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			collection = new CusBrokerageBoxNumberCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			currentElement = collection.AddNew();
		}

		CusBrokerageBoxNumber currentElement;
		CusBrokerageBoxNumberCollection collection;
		#endregion
	}
}
