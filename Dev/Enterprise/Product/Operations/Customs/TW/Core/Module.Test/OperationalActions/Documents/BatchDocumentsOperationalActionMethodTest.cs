using System.Linq;
using Enterprise.Customs.TW.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(BatchDocumentsOperationalActionMethod))]
	abstract class BatchDocumentsOperationalActionMethodTest : OperationalActionMethodTest<BatchDocumentsOperationalActionMethod>
	{
		public void TestGetFilterRequirements()
		{
			var filterRequirements = BatchDocumentsOperationalActionMethod.GetFilterRequirements();
			Assert(filterRequirements.Any(x => x.ConstraintName == FilterConstants.Country && x.Contains(Core.Constants.CountryCodes.Taiwan)));
		}

		public void TestProperties()
		{
			var method = BatchDocumentsOperationalActionMethod;
			CombineAssertions(() =>
			{
				AssertEquals("Description", ExpectedDescription, method.Description);
				AssertEquals("Name", ExpectedName, method.Name);
				AssertEquals("HasControl", true, method.HasControl);
				AssertEquals("IsRunAgainDisabled", false, method.IsRunAgainDisabled);
				using (var control = method.NewGuiControl())
				{
					AssertType<PrintBatchDocumentsConfigurationControl>(control);
				}
			});
		}

		protected abstract string ExpectedName { get; }

		protected abstract string ExpectedDescription { get; }

		public void TestNewApplicatorType()
		{
			AssertType<PrintBatchDocumentsOperationalActionMethodApplicator>(BatchDocumentsOperationalActionMethod.NewApplicator(null, null));
		}

		protected BatchDocumentsOperationalActionMethod BatchDocumentsOperationalActionMethod => batchDocumentsOperationalActionMethod ??= CreateBatchDocumentsOperationalActionMethod();
		BatchDocumentsOperationalActionMethod batchDocumentsOperationalActionMethod;

		protected abstract BatchDocumentsOperationalActionMethod CreateBatchDocumentsOperationalActionMethod();

		protected override BatchDocumentsOperationalActionMethod NewMethod()
		{
			return BatchDocumentsOperationalActionMethod;
		}
	}
}
