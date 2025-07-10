using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.AWB.Testing
{
	[TestedType(typeof(AWBDisplayOption))]
	public class AWBDisplayOptionTest : RegistryBusinessObjectTemplateTestCase<AWBDisplayOption>
	{
		#region Validation

		public void TestValidateEntitlement()
		{
			AssertNoErrors("Precondition: EntitlementInfo should not have errors.", BizObj.EntitlementInfo);
			BizObj.Entitlement = "A";
			AssertNoErrors("Precondition: EntitlementInfo should not have errors.", BizObj.EntitlementInfo);
			BizObj.Entitlement = "C";
			AssertNoErrors("Precondition: EntitlementInfo should not have errors.", BizObj.EntitlementInfo);
			BizObj.Entitlement = "S";
			AssertNoErrors("Precondition: EntitlementInfo should not have errors.", BizObj.EntitlementInfo);
			BizObj.Entitlement = "F";
			AssertHasErrorContaining(BizObj.EntitlementInfo, "Enter a valid selection");
		}

		public void TestValidateVisibility()
		{
			AssertNoErrors("Precondition: VisibilityInfo should not have errors.", BizObj.VisibilityInfo);
			BizObj.Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			AssertNoErrors("Precondition: VisibilityInfo should not have errors.", BizObj.VisibilityInfo);
			BizObj.Visibility = nameof(AWBDisplayOptionVisibility.Show);
			AssertNoErrors("Precondition: VisibilityInfo should not have errors.", BizObj.VisibilityInfo);
			BizObj.Visibility = "dnno";
			AssertHasErrorContaining(BizObj.VisibilityInfo, "Enter a valid selection");
		}

		#endregion

		public void TestVisibility()
		{
			BizObj.Visibility = nameof(AWBDisplayOptionVisibility.Show);
			AssertEquals("Show", BizObj.Visibility);
			BizObj.Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			AssertEquals("Hide", BizObj.Visibility);
		}

		public void TestEntitlement()
		{
			BizObj.Entitlement = "A";
			AssertEquals("A", BizObj.Entitlement);
			BizObj.Entitlement = "C";
			AssertEquals("C", BizObj.Entitlement);
			BizObj.Entitlement = "S";
			AssertEquals("S", BizObj.Entitlement);
		}

		public void TestVisibilityModeIsResString()
		{
			var list = BizObj.VisibilityModes;
			foreach (var pair in list)
			{
				Assert(((IMultilingualDescription)pair).MultilingualDescription is ResourceString);
			}
		}

		#region Implementation

		protected override AWBDisplayOption GetBusinessObjectToClone()
		{
			return new AWBDisplayOption();
		}

		protected override AWBDisplayOption GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new AWBDisplayOption BizObj
		{
			get { return base.BizObj; }
		}

		#endregion
	}
}
