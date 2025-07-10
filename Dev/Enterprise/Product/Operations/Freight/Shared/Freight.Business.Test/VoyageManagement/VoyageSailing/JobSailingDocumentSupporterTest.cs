using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobSailingDocumentSupporter))]
	sealed class JobSailingDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			return voyage.Sailings[0];
		}
	}
}
