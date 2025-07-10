using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryInstructionComparer))]
	public abstract class CusEntryInstructionComparerAbstractTest<T> : TestCaseWithFactory
			where T : CusEntryInstructionComparer
	{
		public virtual void TestCompare()
		{
			CombineAssertions(() =>
			{
				var testInstruction1 = Factory.New<CusEntryInstruction>();
				var testInstruction2 = Factory.New<CusEntryInstruction>();

				testInstruction1.CEI_Style = "12";
				testInstruction1.CEI_Description = "ABB";
				testInstruction1.CEI_SubStyle = "JPB";
				testInstruction2.CEI_Style = "12";
				testInstruction2.CEI_Description = "ABB";
				testInstruction2.CEI_SubStyle = "JPB";
				Assert("same2 CusEntryInstruction", comparer.Compare(testInstruction1, testInstruction2) == 0);

				testInstruction1.CEI_Style = "11";
				testInstruction2.CEI_Style = "12";
				Assert("1<2 Style", comparer.Compare(testInstruction1, testInstruction2) < 0);

				testInstruction1.CEI_Style = "12";
				testInstruction2.CEI_Style = "11";
				Assert("1>2 Style", comparer.Compare(testInstruction1, testInstruction2) > 0);

				testInstruction2.CEI_Style = "12";
				testInstruction1.CEI_SubStyle = "AA";
				testInstruction2.CEI_SubStyle = "JPB";
				Assert("1<2 SubStyle", comparer.Compare(testInstruction1, testInstruction2) < 0);

				testInstruction1.CEI_SubStyle = "JPB";
				testInstruction2.CEI_SubStyle = "AA";
				Assert("1>2 SubStyle", comparer.Compare(testInstruction1, testInstruction2) > 0);

				testInstruction2.CEI_SubStyle = "JPB";
				testInstruction1.CEI_Description = "ABB";
				testInstruction2.CEI_Description = "ABC";
				Assert("1<2 Description", comparer.Compare(testInstruction1, testInstruction2) < 0);

				testInstruction1.CEI_Description = "ABC";
				testInstruction2.CEI_Description = "ABB";
				Assert("1>2 Description", comparer.Compare(testInstruction1, testInstruction2) > 0);
			});
		}

		public virtual void TestGetUniquenessNotificationSeverity()
		{
			AssertEquals(NotificationType.Warning, comparer.GetUniquenessNotificationSeverity());
		}

		protected override void SetUp()
		{
			base.SetUp();
			comparer = (T)Activator.CreateInstance(typeof(T));
		}

		protected T comparer;
	}
}
