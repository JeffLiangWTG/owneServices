using CargoWise.EntityFramework;
using Enterprise.Customs.PL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.Module;

public class EDIMessageController : Messaging.Module.EDIMessageController
{
	protected override IZForm GetForm(IBusiness businessEntity) => new EDIMessageForm((Messaging.Business.EDIMessage)businessEntity);
}
