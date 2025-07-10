using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class EntryDeclarantInfo : ICusAgentInfo
	{
		public EntryDeclarantInfo(GlbStaff declarant)
		{
			if (declarant == null)
			{
				throw new ArgumentNullException(nameof(declarant));
			}

			this.declarant = declarant;
			declarantWrapper = SGGlbStaffWrapper.Get(this.declarant);
		}

		readonly GlbStaff declarant;
		readonly SGGlbStaffWrapper declarantWrapper;

		public ZString Name => declarant.GS_FullName;

		public ZString EntityIdentifier => declarantWrapper.Tradenetv4Password.GP_UserID;

		public ZString Passport => declarant.GS_Passport;

		public ZString Code => declarantWrapper.Tradenetv4Password.GP_MailBoxID;

		public ZString Phone => declarant.GS_WorkPhone;
	}
}
