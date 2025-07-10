using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccountingLogHelper
	{
		void AddLog<BizoType>(EnterpriseBusinessObject parent, BizoType bizO, bool isDeleting = false) where BizoType : BusinessObject, IBusinessObjectLogging;
	}

	class AccountingLogHelper : IAccountingLogHelper
	{
		void IAccountingLogHelper.AddLog<BizoType>(EnterpriseBusinessObject parent, BizoType bizO, bool isDeleting)
		{
			if (parent == null || bizO == null || bizO.IsDeleted)
			{
				return;
			}

			bool shouldAddLog;
			if (isDeleting)
			{
				shouldAddLog = bizO.IsInDatabase;
			}
			else
			{
				shouldAddLog = !bizO.IsInDatabase || bizO.HasChanges;
			}

			if (shouldAddLog)
			{
				var logReferences = bizO.GetLogReference().Split('\n');
				foreach (var logLine in logReferences)
				{
					var trimmedLogline = logLine.Trim();
					if (!trimmedLogline.IsEmpty)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						parent.Logs.AddNew(Events.EditedARecord, trimmedLogline);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}
			}
		}
	}
}
