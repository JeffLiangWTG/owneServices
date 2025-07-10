using Enterprise.ZArchitecture.Modules;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.PL.GUI;

public class EDIMessageForm : Messaging.GUI.EDIMessageForm
{
	public EDIMessageForm(EDIMessage message) : base(message)
	{
		PlugIns.Add(ControllerIDs.eDocsPlugIn);
	}
}
