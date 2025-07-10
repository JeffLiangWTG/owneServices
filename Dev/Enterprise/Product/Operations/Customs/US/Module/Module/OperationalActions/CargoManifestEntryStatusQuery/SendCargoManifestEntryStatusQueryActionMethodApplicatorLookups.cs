using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups : ZLookups
	{
		public SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups(SendCargoManifestEntryStatusQueryActionMethodApplicator parent)
			: base(parent)
		{
		}

		#region ActionList

		public CodeDescriptionPairList ActionList
		{
			get
			{
				var actions = new CargoManifestStatusQueryActionList();
				actions.RemoveCode(CargoManifestStatusQueryActionList.Codes.AIR);
				return actions;
			}
		}

		#endregion

		#region OutputOptionList

		public CodeDescriptionPairList OutputOptionList => new LimitOutputCodeList();

		#endregion
	}
}
