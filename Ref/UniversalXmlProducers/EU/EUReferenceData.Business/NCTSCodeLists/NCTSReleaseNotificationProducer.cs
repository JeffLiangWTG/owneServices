namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class NCTSReleaseNotificationProducer : NCTSCodeListXmlProducer
	{
		protected override IUCCExportCodeListDetail CodeListDetail => new NCTSReleaseNotification();
	}
}
