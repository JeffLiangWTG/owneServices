using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingModuleConsolCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public ForwardingModuleConsolCollectionFetchStrategy(ForwardingModuleConsolCollection collection) : base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			foreach (ForwardingModuleConsol consol in Collection)
			{
				var factory = consol.Factory;

				foreach (var column in columns)
				{
					if (column.ColumnName == CommonConsol.Schema.JK_Calc_ContainerCount)
					{
						factory.AddFetchHint(JobContainerSchema.JC_JK, consol.PK);
					}

					if (column.ColumnName == ForwardingConsol.Schema.JK_Calc_IsCargoOnly)
					{
						factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, consol.PK);
					}
				}

				if (consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.Ordinal) && consol.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air && !consol.IsDomestic() && !consol.JK_MasterBillNum.IsEmpty)
				{
					// CCSUK
					factory.AddFetchHint(CusMAWBSchema.CM_JK, consol.PK);
				}
			}

			var addPresenceOfNotesFetchHints = columns.Any(col => col.ColumnName.StartsWith(nameof(ForwardingConsol.NotesChecker), StringComparison.OrdinalIgnoreCase));
			if (addPresenceOfNotesFetchHints)
			{
				AddPresenceOfNotesFetchHints(businessObjects);
			}

			if (columns.Any(col => col.ColumnName == nameof(ForwardingModuleConsol.JK_SecurityStatus)))
			{
				AddFetchHint_ForJK_SecurityStatus(businessObjects);
			}
		}

		void AddPresenceOfNotesFetchHints(BusinessObject[] businessObjects)
		{
			foreach (ForwardingModuleConsol consol in businessObjects)
			{
				consol.NotesChecker.AddBusinessObjectsWithRelatedNotesFetchHints();
			}

			foreach (ForwardingModuleConsol consol in businessObjects)
			{
				consol.NotesChecker.AddNotesIncludingRelatedFetchHints();
			}
		}

		#region FetchHint For JK_SecurityStatus

		void AddFetchHint_ForJK_SecurityStatus(BusinessObject[] businessObjects)
		{
			var airConsols = businessObjects.OfType<ForwardingModuleConsol>().Where(c => c.IsAir);
			if (airConsols.Any())
			{
				foreach (ForwardingModuleConsol consol in airConsols)
				{
					consol.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, consol.JK_RL_NKLoadPort);
					consol.Factory.AddFetchHint(JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol, consol.PK);

					if (consol.IsAWBHeaderAccessible && consol.IsAWBValuesOverriddenProperty)
					{
						consol.Factory.AddFetchHint(ExportAWBHeaderSchema.EH_ParentID, consol.PK);
						AddFetchHint_ForShipmentAndCusEntryNum(consol);
					}
					else if (consol.SupplyChainSecurityConfiguration.IsEnabled)
					{
						consol.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, consol.PK);
						AddFetchHint_ForShipmentAndCusEntryNum(consol);
					}

					foreach (Transport transport in consol.Transports)
					{
						consol.Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, transport.JW_RL_NKDiscPort);
					}
				}
			}
		}

		void AddFetchHint_ForShipmentAndCusEntryNum(ForwardingModuleConsol consol)
		{
			var shipmentPKs = consol.Factory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol.PK))
					.Select(link => link.JN_JS);
			foreach (var shipmentPK in shipmentPKs)
			{
				consol.Factory.AddFetchHint(JobShipmentSchema.PK, shipmentPK);
				consol.Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, shipmentPK);
			}
		}

		#endregion
	}
}
