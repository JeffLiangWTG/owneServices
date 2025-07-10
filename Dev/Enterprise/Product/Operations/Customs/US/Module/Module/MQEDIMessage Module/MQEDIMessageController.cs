using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public abstract class MQEDIMessageController : EDIMessageController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(MQEDIMessage);

		protected override IZForm GetForm(IBusiness businessEntity) => new MQEDIMessageForm((MQEDIMessage)businessEntity);
	}
}

