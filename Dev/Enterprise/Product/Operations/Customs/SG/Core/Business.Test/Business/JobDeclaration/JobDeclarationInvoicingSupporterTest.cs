using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobDeclaration.JobDeclarationInvoicingSupporter))]
	public class JobDeclarationInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			JobDeclaration jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return jobDeclaration;
		}

		protected override ZString TestingCountry
		{
			get
			{
				return Core.Constants.CountryCodes.Singapore;
			}
		}
	}
}
