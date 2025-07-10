using Enterprise.MailManager;
using Enterprise.MailManager.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Module
{
	public class HREmailsFilterBusinessObject : MailItemFilterBusinessObject
	{
		protected override CodeDescriptionPairList GetStatusListCore()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(MailStatus.Processed, Res.GetString("3FB93250-0E4C-43EF-AD12-C9B538E627AB", "Processed"));
			result.AddPair(MailStatus.Unprocessed, Res.GetString("E91F6388-5D53-4626-AD57-AD28736A795F", "Unprocessed"));
			result.AddPair(MailStatus.MarkedForReprocessing, Res.GetString("1FB3E416-8878-4B00-A6BC-EBAF14BDC64B", "Marked For Reprocessing"));
			result.AddPair(MailStatus.Failed, Res.GetString("0D9CD1A6-538D-4175-888A-370F49B0FA0B", "Failed"));
			return result;
		}
	}
}
