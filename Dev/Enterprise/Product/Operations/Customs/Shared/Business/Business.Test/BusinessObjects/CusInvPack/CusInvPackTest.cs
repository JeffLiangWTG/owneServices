using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusInvPack))]
	public abstract class CusInvPackTest<TParent> : EnterpriseBusinessObjectTestCase
			where TParent : BusinessObject
	{
		public void TestParent()
		{
			packagePivot.B5_ParentID = ZGuid.Empty;
			AssertNull("Parent", packagePivot.Parent);

			packagePivot.B5_ParentTableCode = parent.TablePrefix;
			packagePivot.B5_ParentID = parent.PK;
			AssertEquals("Parent", parent, packagePivot.Parent);

			packagePivot.Parent = Factory.New<DummyBusinessObject>();
			AssertNull("Parent", packagePivot.Parent);

			packagePivot.Parent = parent;
			AssertEquals("Parent", parent, packagePivot.Parent);
		}

		public void TestB5_UnitTypeDescription()
		{
			packagePivot.B5_UnitType = ZString.Empty;
			AssertEquals("B5_UnitTypeDescription", "", packagePivot.B5_UnitTypeDescription);
			CodeDescriptionPairList list = packagePivot.Lookups.UnitTypeList;
			if (list.Count > 0)
			{
				ICodeDescription pair = list[0];
				packagePivot.B5_UnitType = pair.Code;
				AssertEquals("B5_UnitTypeDescription", pair.Description, packagePivot.B5_UnitTypeDescription);

				pair = list[list.Count - 1];
				packagePivot.B5_UnitType = pair.Code;
				AssertEquals("B5_UnitTypeDescription", pair.Description, packagePivot.B5_UnitTypeDescription);
			}
			Assert("PreCondition: 'Z~' should not exists", !list.ContainsCode("Z~"));
			packagePivot.B5_UnitType = "Z~";
			AssertEquals("B5_UnitTypeDescription", "", packagePivot.B5_UnitTypeDescription);
		}

		protected virtual CusInvPack GetNewPackagePivot() => (CusInvPack)Factory.New(TestedTypeHelper.GetTestedType(GetType()));

		protected virtual TParent GetNewParent() => Factory.New<TParent>();

		protected override void SetUp()
		{
			base.SetUp();
			parent = GetNewParent();
			packagePivot = GetNewPackagePivot();
			packagePivot.B5_ParentTableCode = parent.TablePrefix;
			packagePivot.B5_ParentID = parent.PK;
		}

		TParent parent;
		protected CusInvPack packagePivot;
	}
}
