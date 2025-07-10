using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusDispositionExtensionMethodTest : TestCaseWithFactory
	{
		public void TestCloseStatus()
		{
			var job = Factory.New<JobDeclaration>();
			var cusDisposition = job.EntryPGACusDispositions.AddNew();
			cusDisposition.CDI_StatusKey = "FDA";
			cusDisposition.CDI_Status = "01";
			cusDisposition.CloseStatus();
			Assert(cusDisposition.CDI_Status == PGADispositionCodeList.MarkAsClosedCode);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, "PGA FDA CLOSED");
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			AssertNotNull(job.Logs.Find(query).FirstOrDefault());
		}
	}
}
