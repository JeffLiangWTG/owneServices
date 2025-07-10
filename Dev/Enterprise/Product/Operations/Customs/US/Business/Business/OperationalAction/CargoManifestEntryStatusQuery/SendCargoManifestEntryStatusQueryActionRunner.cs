using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class SendCargoManifestEntryStatusQueryActionRunner : USDeclarationOperationalActionRunner
	{
		public SendCargoManifestEntryStatusQueryActionRunner(IOperationalActionSectionLog log, ZString action, ZString outputOption, ZBool updateEntryWithResults, ZBool requestForReleatedBOL)
			: base(log)
		{
			this.action = action;
			this.outputOption = outputOption;
			this.updateEntryWithResults = updateEntryWithResults;
			this.requestForReleatedBOL = requestForReleatedBOL;
		}
		readonly ZString action;
		readonly ZString outputOption;
		readonly ZBool updateEntryWithResults;
		readonly ZBool requestForReleatedBOL;

		protected override OperationalActionBulkMessageSender<JobDeclaration> GetMessageSenderCore(JobDeclaration declaration)
		{
			return new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(declaration, action, outputOption, updateEntryWithResults, requestForReleatedBOL);
		}

		protected override bool IsJobEligibleForSending(JobDeclaration declaration)
		{
			var declarationLink = declaration.GetDeclarationIdLink();

			if (!declaration.IsImport)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, "Job {0}: Cargo/Manifest/Entry query can only be sent for import declarations.", new object[] { declarationLink });
				return false;
			}

			if (IsMAWBOrHAWB && declaration.JE_TransportMode != TransportTypeList.Codes.Air)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. Action ‘{1}’ can only be used when transport mode is Air.", new object[] { declarationLink, action });
				return false;
			}

			if (action == CargoManifestStatusQueryActionList.Codes.MAWB && declaration.Bills.FindByBillType(Customs.Business.BillTypeList.Codes.MasterBill).Length <= 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. There is no master bill found.", new object[] { declarationLink });
				return false;
			}

			if (action == CargoManifestStatusQueryActionList.Codes.HAWB && declaration.Bills.FindByBillType(Customs.Business.BillTypeList.Codes.HouseBill).Length <= 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. There is no house bill found.", new object[] { declarationLink });
				return false;
			}

			if (action == CargoManifestStatusQueryActionList.Codes.Entry)
			{
				if (declaration.IsFTZAdmission)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. Action ‘ENT’ is not available for FTZ job.", new object[] { declarationLink });
					return false;
				}

				if (declaration.ActiveEntryHeaders.EntrySummaryEntry == null && declaration.ActiveEntryHeaders.SimplifiedEntry == null)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. There is no entry found.", new object[] { declarationLink });
					return false;
				}
			}

			if (action == CargoManifestStatusQueryActionList.Codes.InBond)
			{
				if (declaration.IsFTZAdmission)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. Action ‘INB’ is not available for FTZ job.", new object[] { declarationLink });
					return false;
				}

				if (declaration.ITNumberCollection.Count <= 0)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. There is no IT numbers found.", new object[] { declarationLink });
					return false;
				}
			}

			if (action == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill)
			{
				if (declaration.JE_TransportMode == TransportTypeList.Codes.Air)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. Action ‘ORT’ cannot be used when transport mode is Air.", new object[] { declarationLink });
					return false;
				}

				if (declaration.Bills.FindByBillType(Customs.Business.BillTypeList.Codes.MasterBill).Length <= 0 && declaration.Bills.FindByBillType(Customs.Business.BillTypeList.Codes.HouseBill).Length <= 0)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cargo/Manifest/Entry query cannot be sent. There is no bill found.", new object[] { declarationLink });
					return false;
				}
			}

			return true;
		}

		bool IsMAWBOrHAWB => action.Equals(CargoManifestStatusQueryActionList.Codes.MAWB) || action.Equals(CargoManifestStatusQueryActionList.Codes.HAWB);
	}
}
