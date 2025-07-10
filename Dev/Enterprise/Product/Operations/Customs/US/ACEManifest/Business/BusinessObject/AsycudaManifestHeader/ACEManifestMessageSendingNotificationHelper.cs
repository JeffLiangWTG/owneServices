using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class ACEManifestMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public ACEManifestMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var result = base.GetNotifications();

			if (result.IsEmpty && ((AsycudaManifestHeader)header).AirAMSOriginatorCode.IsEmpty)
			{
				result = Res.GetString("06318EF7-E38B-4524-8A58-C5081F9ED4DD", "US Air Import Manifests can only be submitted by a company with an Originator Code. This can be added under CFS Address > Details > Config > Registration Numbers / Codes, (using type 'AMO') or added under Current Branch ({0}) or Current Company ({1}) Organization Proxy > Details > Config > Registration Numbers / Codes, (using type 'AMO')",
					GlbBranch.CurrentBranch.GB_BranchName, GlbCompany.CurrentCompany.CompanyName);
			}

			return result;
		}
	}
}
