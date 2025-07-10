using System.Collections.Generic;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Customs.ZA.Manifest.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI.MessagingProcess
{
	public class CustomsMessagingGui : ICustomsMessagingGui
	{
		public static void SendMessages(AsycudaManifestHeader header, string msgSubType, IReadOnlyCollection<AsycudaBill> selectedBills, ZForm topLevelBusinessObjectForm)
		{
			var gui = new CustomsMessagingGui(header, msgSubType, selectedBills, topLevelBusinessObjectForm);
			_ = gui.SendMessages();
		}

		internal CustomsMessagingGui(AsycudaManifestHeader header, string msgSubType, IReadOnlyCollection<AsycudaBill> selectedBills, ZForm topLevelBusinessObjectForm)
		{
			MessagingSupporter = new CustomsMessagingSupporter(header, new CustomsMessagingProviderFactory(msgSubType, selectedBills));
			this.topLevelBusinessObjectForm = topLevelBusinessObjectForm;
		}

		internal readonly ICustomsMessagingSupporter MessagingSupporter;
		internal readonly ZForm topLevelBusinessObjectForm;

		ICustomsMessagingSupporter ICustomsMessagingGui.MessagingSupporter => MessagingSupporter;

		ZForm ICustomsMessagingGui.TopLevelBusinessObjectForm => topLevelBusinessObjectForm;
	}
}
