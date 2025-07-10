using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class AccSurchargeApplicationLogHelper
	{
		public static void AddLog(EnterpriseBusinessObject parent, BusinessObject bizO, bool isDeleting = false)
		{
			if (parent == null || bizO == null || bizO.IsDeleted)
			{
				return;
			}

			if (!(bizO is IBusinessObjectLogging))
			{
				throw new NotImplementedException(nameof(bizO));
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
				var logReferences = ((IBusinessObjectLogging)bizO).GetLogReference().Split('\n');
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
