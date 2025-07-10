using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = nameof(OnUniversalCopyFinish))]
	public class JobConShipLink : AutoJobConShipLink, Integration.IJobConShipLink
	{
		public JobConShipLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public CommonConsol Consol
		{
			get { return Factory.Load<CommonConsol>(JN_JK); }
		}

		public CommonShipment Shipment
		{
			get { return Factory.Load<CommonShipment>(JN_JS); }
		}

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();

			try
			{
				if (Consol.JK_IsForwarding && Shipment.JS_IsBooking)
				{
					Shipment.JS_IsForwardRegistered = ZBool.True;
				}

				if (Consol.JK_IsCFS)
				{
					Shipment.JS_IsCFSRegistered = ZBool.True;
				}

				Consol.UpdatePreAllocatedAmountExceededStatusIfNecessary();
			}
			catch (System.NullReferenceException e)
			{
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Saving, JN_JK: {0}, JN_JS: {1}, IsDeleted: {2}, IsInDatabase: {3}", JN_JK, JN_JS, IsDeleted, IsInDatabase));
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"Consol is null: {0}", Consol == null));
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)"Shipment is null: {0}", Shipment == null));

				if (Shipment == null)
				{
					ShipmentLinkLogs.Add(BuildNullShipmentRowInformation());
				}

				ShipmentLinkLogs.Add(e.Message);
				ShipmentLinkLogs.Add(e.StackTrace);

				var log = string.Join(System.Environment.NewLine, ShipmentLinkLogs);

				var errorMessage = string.Format(CultureInfo.InvariantCulture, "JobConShipLink saved without JN_JK and JN_JS.\r\n{0}", log);
				ErrorReporter.ReportOnce("WI00159884.JobConShipLinkWithoutShipmentOrConsol", errorMessage);
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!IsInDatabase || JN_JKInfo.HasChanges || JN_JSInfo.HasChanges)
			{
				ProposeWorkflowRelationships();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			RemoveWorkflowRelationships(Consol, Shipment);
		}

		string BuildNullShipmentRowInformation()
		{
			var builder = new ZStringBuilder();
			var shipmentTable = (ZDataTable)((INeedDataSet)Factory).Data.Tables[JobShipmentSchema.Constants.TableName];

			if (shipmentTable != null && JN_JS.IsValid)
			{
				var shipmentRow = shipmentTable.GetRowIncludingDeleted(JN_JS.ToGuid());
				if (shipmentRow != null)
				{
					builder.AppendLine((NoResString)"Shipment row information:");
					builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "RowState: {0}, JN_JS: {1}", shipmentRow.RowState, JN_JS));
					if (shipmentRow.RowState != DataRowState.Deleted && shipmentRow.RowState != DataRowState.Detached)
					{
						builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "PK: {0}", shipmentRow[JobShipmentSchema.Constants.PK]));
					}
				}
			}

			if (builder.IsEmpty)
			{
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "No row for JN_JS: {0}", JN_JS));
			}

			return builder.ToString();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && isSavedByFactory)
			{
				isSavedByFactory = !IsNewAndAllPropertiesExceptAuditColumnsAreDuplicatesOfExistingBusinessObject;
			}
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				ShipmentLinkLogs.Clear();
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Saved, JN_JK: {0}, JN_JS: {1}, SaveSucceeded: {2}", JN_JK, JN_JS, saveSucceeded));
			}
		}

		protected virtual void OnUniversalCopyFinish()
		{
			Consol.Shipments.ReloadFromLocalCache();
			Shipment.Consols.ReloadFromLocalCache();
		}

		public override bool IsSavedByFactory
		{
			get { return isSavedByFactory; }
		}
		bool isSavedByFactory = true;

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Deleting: JN_JK: {0}, JN_JS: {1}", JN_JK, JN_JS));

				if (JN_JK.IsEmpty || JN_JS.IsEmpty)
				{
					ShipmentLinkLogs.Add(System.Environment.StackTrace);
				}
			}

			var consol = Consol;
			var shipment = consol == null ? null : Shipment;

			RemoveWorkflowRelationships(consol, shipment);

			base.Delete();
			ClearCusContainerReferenceToJobContainer(shipment, consol, true);
		}

		void ClearCusContainerReferenceToJobContainer(CommonShipment shipment, CommonConsol consol, bool forDelete)
		{
			if (shipment != null)
			{
				var consolPK = consol.PK;
				var linkQuery = new ZQuery(JobConShipLinkSchema.JN_JK, consolPK);
				linkQuery.AddToFilter(JobConShipLinkSchema.JN_JS, shipment.PK);
				linkQuery.FetchOnlyFromLocalCache = !shipment.IsInDatabase || !consol.IsInDatabase;
				if (Factory.LoadTop1<JobConShipLink>(linkQuery) == null) // ensure no other link have been added for the consol and shipment
				{
					shipment.Consols.RemoveCollectionRelationships(consol, forDelete);
					var declarations = shipment.Declarations;
					if (declarations.Length > 0)
					{
						var query = new ZQuery(CusContainerSchema.CO_JE, declarations.Select(x => x.PK));
						query.AddToFilter(CusContainerSchema.CO_JC, SQLComparisonOperator.NotEqual, null);
						query.FetchOnlyFromLocalCache = !shipment.IsInDatabase;
						var cusContainers = Factory.Load<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
						if (cusContainers.Length > 0)
						{
							Factory.AddFetchHint(JobContainerSchema.Instance, new ZQuery(JobContainerSchema.PK, cusContainers.Select(x => x.CO_JC)));
							foreach (var cusContainer in cusContainers)
							{
								var container = Factory.Load<CommonContainer>(cusContainer.CO_JC);
								if (container != null && container.JC_JK == consolPK)
								{
									cusContainer.CO_JC = ZGuid.Empty;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("Consol")]
		public override ZGuid JN_JK
		{
			get { return base.JN_JK; }
			set
			{
				if (value != JN_JK)
				{
					ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Setting new JN_JK: {0} -> {1}", JN_JK, value));
					ShipmentLinkLogs.Add(System.Environment.StackTrace);

					isSavedByFactory = true;
					var oldConsol = Consol;
					var shipment = oldConsol == null ? null : Shipment;

					if (oldConsol != null)
					{
						RemoveWorkflowRelationships(oldConsol, shipment);
					}

					base.JN_JK = value;

					ClearCusContainerReferenceToJobContainer(shipment, oldConsol, false);
				}
			}
		}

		[RelatedBusinessObject("Shipment")]
		public override ZGuid JN_JS
		{
			get { return base.JN_JS; }
			set
			{
				if (value != JN_JS)
				{
					ShipmentLinkLogs.Add(string.Format(CultureInfo.InvariantCulture, "Setting new JN_JS: {0} -> {1}", JN_JS, value));
					ShipmentLinkLogs.Add(System.Environment.StackTrace);

					var oldShipment = Shipment;

					if (oldShipment != null)
					{
						RemoveWorkflowRelationships(Consol, oldShipment);
					}

					isSavedByFactory = true;
					base.JN_JS = value;
				}
			}
		}

		#endregion

		#region Fields

		ICollection<string> ShipmentLinkLogs => shipmentLinkLogs;
		readonly List<string> shipmentLinkLogs = new List<string>();

		#endregion

		#region Workflow Relationships

		public void ProposeWorkflowRelationships()
		{
			if (!IsDeleted)
			{
				var fromEntity = Shipment as IWorkflowProviderCore;
				var toEntity = Consol as IWorkflowProviderCore;

				if (fromEntity != null && toEntity != null)
				{
					ObjectFactory.Get<IWorkflowProvidersLinkageService>().WorkflowProvidersLinked(fromEntity, toEntity, Factory);
				}
			}
		}

		static void RemoveWorkflowRelationships(CommonConsol consol, CommonShipment shipment)
		{
			var fromEntity = shipment as IWorkflowProviderCore;
			var toEntity = consol as IWorkflowProviderCore;

			if (fromEntity != null && toEntity != null)
			{
				ObjectFactory.Get<IWorkflowProvidersLinkageService>().WorkflowProvidersUnLinked(fromEntity, toEntity, shipment.Factory);
			}
		}

		#endregion
	}
}
