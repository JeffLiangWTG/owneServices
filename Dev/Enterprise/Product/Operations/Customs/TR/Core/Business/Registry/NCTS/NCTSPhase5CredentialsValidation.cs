using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSPhase5CredentialsValidation : ZValidation
	{
		public NCTSPhase5CredentialsValidation(NCTSPhase5Credentials parent) : base(parent)
		{
			this.parent = parent;
		}

		readonly NCTSPhase5Credentials parent;

		public override void ValidateAll()
		{
			parent.ClearAllNotifications();
		}

		public override Type AutoValidationType => typeof(NCTSPhase5CredentialsValidation);
	}
}
