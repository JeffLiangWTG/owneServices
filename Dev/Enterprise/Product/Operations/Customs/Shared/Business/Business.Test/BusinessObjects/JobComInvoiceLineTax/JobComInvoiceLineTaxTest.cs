using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	public class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		/// <summary>
		/// JobComInvoiceLineTax is used in Data Transfer in a context
		/// that does not expect defaults other than the ones defined in the database.
		/// </summary>
		public void TestNoBusinessLayerDefaultValues()
		{
			var jobComInvoiceLineTaxType = typeof(JobComInvoiceLineTax);
			var defaulValuesMethod = jobComInvoiceLineTaxType.GetMethod("SetDefaultValues", BindingFlags.Instance | BindingFlags.NonPublic);

			CombineAssertions(() =>
			{
				AssertNotEquals("SetDefaultValues declaring type.", jobComInvoiceLineTaxType, defaulValuesMethod.DeclaringType);
				AssertEquals("BaseType", typeof(AutoJobComInvoiceLineTax), jobComInvoiceLineTaxType.BaseType);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var jz = factory.New<BaseJobComInvoiceHeader>();
			var ji = jz.InvoiceLines.AddNew();
			var jlt = (JobComInvoiceLineTax)base.GetNewBusinessObjectForDeleteTest(factory);
			jlt.JLT_JI = ji.PK;
			jlt.JLT_Type = "A00";
			return jlt;
		}
	}
}
