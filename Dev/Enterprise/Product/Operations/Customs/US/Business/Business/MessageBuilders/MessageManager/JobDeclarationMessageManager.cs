using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	//public enum ImportMessageStatusList.MessageType { Undefined, Export, EntrySummary, InBondDeparture, InBondUpdate, CargoRelease, ElectronicInvoice, NAFTADutyDeferral, BorderCargoRelease }

	public abstract class JobDeclarationMessageManager : Customs.Business.MultiMessageManager
	{
		protected JobDeclarationMessageManager(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		protected readonly JobDeclaration declaration;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return declaration; }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		public bool MergeIfNecessary(Customs.Business.ISendsMessagesToCustoms sender)
		{
			bool result = true;
			if (declaration.MergeManager.RequiresMerge || declaration.ActiveEntryHeaders.Count == 0)
			{
				result = declaration.DoMerge(sender);
			}
			return result;
		}

		protected override string AmendmentMessageTypeUsedInConfirmation
		{
			get
			{
				return "Replacement / Update";
			}
		}

		protected override bool CheckDeniedParty(BusinessObject master)
		{
			// This functionality is performed elsewhere in US Solution.
			return true;
		}
	}
}
