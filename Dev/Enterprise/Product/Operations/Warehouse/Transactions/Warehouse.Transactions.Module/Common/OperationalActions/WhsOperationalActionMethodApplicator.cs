using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WhsOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		protected WhsOperationalActionMethodApplicator(string name)
			: base(name)
		{
		}

		protected WhsOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected static LogControllerLink GetDocketIdLink(WhsDocket docket) => new LogControllerLink(docket.WD_DocketID, ((IRelatedJob)docket).ControllerID, docket.PK);

		protected static LogControllerLink GetPickNoLink(WhsPick pick) => new LogControllerLink(pick.WP_PickNo, ControllerIDs.WhsPicking, pick.PK);

		protected static LogControllerLink GetInvoiceIdLink(JobStorage invoice) => new LogControllerLink(invoice.ET_StorageJobNumber, ControllerIDs.WhsInvoicing, invoice.PK);

		protected static LogControllerLink GetLoadIdLink(WhsLoad load) => new LogControllerLink(load.WLO_JobID, ControllerIDs.WhsLoad, load.PK);

		protected void SaveAndHandleErrors(Action action, IOperationalActionSectionLog log, string format, string description, LogControllerLink link, int reattempts = 1)
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
			{
				try
				{
					action();
				}
				catch (ZCannotSaveException ex)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, format, description, link, ex.Message);
				}
				catch (ZSaveConcurrencyException)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, format, description, link, Res.GetString("609d94a3-730c-4b30-81d5-a998a7c08521", "While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please close and open this form to try again."));
				}
			}, null, attempts: reattempts);
		}
	}
}
