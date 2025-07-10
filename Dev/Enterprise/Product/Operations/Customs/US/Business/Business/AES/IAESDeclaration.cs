using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IAESDeclaration
	{
		void LogCustomsCommencedIfNeeded();
		BusinessObjectFactory Factory { get; }
		Customs.Business.ISendsMessagesToCustoms MessageInitiator { get; }
		IEnumerable<IAESEntry> ActiveEntryHeaders { get; }
	}

	public interface IAESEntry : IAESTIRMessageAttachee
	{
		ZBool US_ShouldBeReportToCustoms { get; set; }
		bool HasBeenLodgedAtCustoms { get; }
		bool IsCurrentlyWithdrawn { get; }
		ZBool US_SendWithdrawn { get; set; }
		ZString CH_Status { get; set; }
		ZString CH_EntryStatus { get; set; }
		void PopulateEntrySubmittedDateIfRequired(ZDateTime? submittedDate = null);
		ZString CH_BGMReference { get; set; }
		UpdateActionCode MessageAction { get; }
		IDisposable SuspendMarkingAsNeedingValidation();
	}
}
