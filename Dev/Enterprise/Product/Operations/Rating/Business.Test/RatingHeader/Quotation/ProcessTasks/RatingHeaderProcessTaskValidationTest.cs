using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingHeaderProcessTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestP9_OA()
		{
			CreateTaskAndAssertP9_OA<QuotationProcessTask>();
			CreateTaskAndAssertP9_OA<ClientRateProcessTask>();
		}

		public void TestQuotationP9_Type()
		{
			CreateTaskAndAssertP9_Type<QuotationProcessTask>();
			CreateTaskAndAssertP9_Type<ClientRateProcessTask>();
		}

		#region Private methods

		void CreateTaskAndAssertP9_OA<T>() where T : ProcessTask
		{
			var task = Factory.New<T>();
			AssertNoErrors(task.P9_OAInfo);

			task.OrganisationPK = ZGuid.NewZGuid();
			Assert(task.P9_OA.IsEmpty);

			task.Validation.ValidateP9_OA();
			AssertNoErrors(task.P9_OAInfo);
		}

		void CreateTaskAndAssertP9_Type<T>() where T : ProcessTask
		{
			var task = Factory.New<T>();
			task.P9_Description = "Task";
			task.P9_Type = "XXX";
			Assert(task.P9_TypeInfo.HasError("Enter a valid Task Type."));

			var trigger = Factory.New<T>();
			trigger.P9_Description = "Trigger";
			trigger.P9_Type = "TRG";
			AssertNoErrors(trigger.P9_TypeInfo);
		}

		#endregion
	}
}
