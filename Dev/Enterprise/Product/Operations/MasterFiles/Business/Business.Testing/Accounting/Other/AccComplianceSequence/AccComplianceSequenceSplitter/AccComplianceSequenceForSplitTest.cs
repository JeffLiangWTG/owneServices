using System.Reflection;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceForSplit))]
	sealed class AccComplianceSequenceForSplitTest : AccComplianceSequenceTest
	{
		public void TestBookType()
		{
			sequenceForSplit.IsNewBook = true;
			AssertEquals("New", sequenceForSplit.XD_Calc_BookType);

			sequenceForSplit.IsNewBook = false;
			AssertEquals("Existing", sequenceForSplit.XD_Calc_BookType);
		}

		public void TestReadonlyProperties()
		{
			sequenceForSplit.IsNewBook = true;
			AssertEquals("BookType / Prefix / StartNumber / NextNumber properties should be readonly.",
						 true, GetPrivatePropertyValue(sequenceForSplit, "XD_BookType_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_Prefix_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_StartNumber_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_NextNumber_ReadOnly"));
			AssertEquals("EndNumber / ExpiryDate properties should be readonly only for new sequence book",
						 true, GetPrivatePropertyValue(sequenceForSplit, "XD_EndNumber_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_StartDate_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_ExpiryDate_ReadOnly"));

			sequenceForSplit.IsNewBook = false;
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_BookType_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_Prefix_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_StartNumber_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_NextNumber_ReadOnly"));
			AssertEquals(false, GetPrivatePropertyValue(sequenceForSplit, "XD_EndNumber_ReadOnly"));
			AssertEquals(true, GetPrivatePropertyValue(sequenceForSplit, "XD_StartDate_ReadOnly"));
			AssertEquals(false, GetPrivatePropertyValue(sequenceForSplit, "XD_ExpiryDate_ReadOnly"));
		}

		public void TestCalculatedNumbers()
		{
			sequenceForSplit.XD_MaximumNumberDigits = 4;

			sequenceForSplit.XD_StartNumber = ZDecimal.Zero;
			AssertEquals(string.Empty, sequenceForSplit.XD_Calc_StartNumberString);

			sequenceForSplit.XD_StartNumber = 1;
			AssertEquals("0001", sequenceForSplit.XD_Calc_StartNumberString);

			sequenceForSplit.XD_EndNumber = ZDecimal.Zero;
			AssertEquals(string.Empty, sequenceForSplit.XD_Calc_EndNumberString);

			sequenceForSplit.XD_EndNumber = 2;
			AssertEquals("0002", sequenceForSplit.XD_Calc_EndNumberString);
		}

		public void TestXD_IsActiveForXD_EndNumber()
		{
			sequenceForSplit.XD_StartNumber = 10;
			sequenceForSplit.XD_EndNumber = 100;
			sequenceForSplit.XD_NextNumber = 20;

			sequenceForSplit.XD_EndNumber = 19;
			AssertEquals(false, sequenceForSplit.XD_IsActive);

			sequenceForSplit.XD_EndNumber = 25;
			AssertEquals(true, sequenceForSplit.XD_IsActive);
		}

		public object GetPrivatePropertyValue(object target, string propertyName)
		{
			var propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
			return propertyInfo.GetValue(target);
		}

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();

			sequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
			sequence.XD_StartDate = new ZDate(2018, 5, 20);
			sequence.XD_ExpiryDate = new ZDateTime(2018, 10, 5);

			sequenceForSplit = Factory.Load<AccComplianceSequenceForSplit>(sequence.PK);
		}

		AccComplianceSequenceForSplit sequenceForSplit;

		#endregion
	}
}
