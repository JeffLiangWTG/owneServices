using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business
{
	public static class CusDispositionExtensionMethod
	{
		public static void CloseStatus(this CusDisposition disposition)
		{
			disposition.CDI_Status = PGADispositionCodeList.MarkAsClosedCode;
			disposition.CDI_StatusDate = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("MIAMI", ZDateTime.UtcNow.ToDateTime());

			var parentJob = disposition.Parent;
			if (parentJob != null)
			{
				var reference = ZString.Format("PGA {0} CLOSED", disposition.CDI_StatusKey);
				parentJob.GetLogs().AddNew(Events.MessageStatusChange, reference);
			}
		}
	}
}
