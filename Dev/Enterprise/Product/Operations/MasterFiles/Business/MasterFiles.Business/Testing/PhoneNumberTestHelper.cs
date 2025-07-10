#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using static NUnit.Framework.Assertion;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class PhoneNumberTestHelper
	{
		public static void AssertPropertyIsManuallyVerified(BusinessObjectFactory factory, string isManuallyVerifiedPropertyName, string exceptedParentTableCode, string expectedParentTableColumnName, BusinessObject parent1, BusinessObject parent2)
		{
			var validationAck1 = new GenCustomAddOnRuleAckCollection(parent1);
			var validationAck2 = new GenCustomAddOnRuleAckCollection(parent2);
			AssertEquals("Precondition", 0, validationAck1.Count);
			AssertEquals("Precondition", 0, validationAck2.Count);
			AssertEquals("Precondition", false, (ZBool)parent1[isManuallyVerifiedPropertyName]);
			AssertEquals("Precondition", false, (ZBool)parent2[isManuallyVerifiedPropertyName]);

			parent1[isManuallyVerifiedPropertyName] = true;
			parent2[isManuallyVerifiedPropertyName] = true;
			AssertEquals(true, (ZBool)parent1[isManuallyVerifiedPropertyName]);
			AssertEquals(true, (ZBool)parent2[isManuallyVerifiedPropertyName]);

			AssertEquals(1, validationAck1.Count);
			AssertEquals(exceptedParentTableCode, validationAck1[0].XK_ParentTableCode);
			AssertEquals(expectedParentTableColumnName, validationAck1[0].XK_ParentTableColumn);
			AssertEquals(parent1.PK, validationAck1[0].XK_ParentID);

			AssertEquals(1, validationAck2.Count);
			AssertEquals(exceptedParentTableCode, validationAck2[0].XK_ParentTableCode);
			AssertEquals(expectedParentTableColumnName, validationAck2[0].XK_ParentTableColumn);
			AssertEquals(parent2.PK, validationAck2[0].XK_ParentID);
		}
	}
}

#endif
