using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class OrgCountryDataUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public OrgCountryDataUniqueIndexFailureHandler(OrgCountryData countryData)
		{
			this.countryData = countryData;
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return OrgCountryDataSchema.Constants.Indexes.FK_UC__OV_OH_OrgHeader_OV_RN_NKClientCountryRelation_OV_OA_ApprovedLocation; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			OrgHeader orgHeader = countryData.OrgHeader;
			string message = GenerateMessage();
			countryData.Delete();
			orgHeader.Factory.ClearQueryCache();
			if (orgHeader.IsInDatabase)
			{
				orgHeader.Reload();
			}
			if (Globals.IsUserInteractive)
			{
				notifier.ReportError(message, Res.GetString("ec5edf67-0a22-4e96-acc7-3973a7f5ca68", "Save Error"));
			}
		}

		string GenerateMessage()
		{
			return Res.GetString("9ea205de-35d2-49b2-aa37-0717d40f8609", "Another user has made changes to {0}, please review your changes and save again.", countryData.OrgHeader.OH_Code);
		}

		readonly OrgCountryData countryData;
	}
}
