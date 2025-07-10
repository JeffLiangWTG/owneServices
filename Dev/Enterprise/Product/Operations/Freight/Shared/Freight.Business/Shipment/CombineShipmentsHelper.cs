using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CombineShipmentsHelper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CombineShipmentsHelper(CommonShipment shipment, JobSailing sailingParent)
			: base(shipment.Factory)
		{
			this.Shipment = shipment;
			this.SailingParent = sailingParent;
		}

		public CombineShipmentsHelper(CommonShipment shipment, CommonConsol consolParent)
			: base(shipment.Factory)
		{
			this.Shipment = shipment;
			this.ConsolParent = consolParent;
		}

		readonly CommonShipment Shipment;
		readonly CommonConsol ConsolParent;
		readonly JobSailing SailingParent;

		public bool ShipmentCanBeCombined()
		{
			return ShipmentCanBeCombined(Shipment);
		}

		public bool ShipmentCanBeCombined(CommonShipment shipmentToCheck)
		{
			return shipmentToCheck.JS_JS_ColoadMasterShipment.IsEmpty &&
				!shipmentToCheck.JS_IsCancelled &&
				!IsReferencedByOtherShipments(shipmentToCheck) &&
				!HasBillingCharges(shipmentToCheck) &&
				CheckParentExists(shipmentToCheck) &&
				CheckShipmentDetailsExist(shipmentToCheck);
		}

		public bool ShipmentHasSiblings
		{
			get { return RelatedShipments.Count > 0; }
		}

		public string CombineSelectedShipments(params CommonShipment[] selectedShipments)
		{
			string message = RunPreCombineCheckers(selectedShipments);

			if (string.IsNullOrEmpty(message))
			{
				foreach (CommonShipment sibling in selectedShipments)
				{
					ConvertSiblingToArchived(sibling);
				}

				ResetRelatedShipments();
			}

			return message;
		}

		#region PreCombine Sanity Checks

		string RunPreCombineCheckers(params CommonShipment[] selectedShipments)
		{
			var checkMessage = CheckAllSelectedShipmentsAreFromRelated(selectedShipments);

			if (!string.IsNullOrEmpty(checkMessage))
			{
				return checkMessage;
			}

			checkMessage = CheckAllShipmentsCouldBeDetachedFromConsol(selectedShipments);

			if (!string.IsNullOrEmpty(checkMessage))
			{
				return checkMessage;
			}

			checkMessage = CheckAllShipmentsHaveDeletableJobHeaders(selectedShipments);

			if (!string.IsNullOrEmpty(checkMessage))
			{
				return checkMessage;
			}

			return string.Empty;
		}

		string CheckAllSelectedShipmentsAreFromRelated(params CommonShipment[] selectedShipments)
		{
			List<string> nonRelatedShipmentPKs = new List<string>();
			foreach (CommonShipment selectedShipment in selectedShipments)
			{
				if (!RelatedShipments.Contains(selectedShipment.PK))
				{
					nonRelatedShipmentPKs.Add(selectedShipment.JS_UniqueConsignRef);
				}
			}

			if (nonRelatedShipmentPKs.Count > 0)
			{
				string allNonRelatedShipmentPKs = string.Join(", ", nonRelatedShipmentPKs.ToArray());
				return Res.GetString("d4a3ca5b-4992-46e6-92a9-4b4036b4fd46", "Following shipment(s) are un-related to the original shipment: {0}", allNonRelatedShipmentPKs);
			}

			return string.Empty;
		}

		string CheckAllShipmentsCouldBeDetachedFromConsol(CommonShipment[] selectedShipments)
		{
			var reasons = selectedShipments
						.Select(x =>
						{
							var reason = CheckShipmentCouldBeDetachedFromConsol(x);
							return string.IsNullOrWhiteSpace(reason)
								? string.Empty
								: Res.GetString("5faf5c0f-b53f-4bf6-9fa0-10873a08886f", "Shipment {0} couldn't be detached from the consol: {1}", x.JS_UniqueConsignRef, reason);
						})
						.Where(z => !string.IsNullOrWhiteSpace(z))
						.ToArray();

			return reasons.Any() ? string.Join(System.Environment.NewLine, reasons) : string.Empty;
		}

		string CheckAllShipmentsHaveDeletableJobHeaders(CommonShipment[] selectedShipments)
		{
			var reasons = selectedShipments.Select(x => JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(x.PK, x.JS_UniqueConsignRef)).Where(z => !string.IsNullOrWhiteSpace(z)).ToArray();
			return reasons.Any() ? string.Join(System.Environment.NewLine, reasons) : string.Empty;
		}

		string CheckShipmentCouldBeDetachedFromConsol(CommonShipment shipmentToCheck)
		{
			if (shipmentToCheck != null && !IsSailingParent && shipmentToCheck.Consols.Contains(ConsolParent))
			{
				BusinessObject shipmentConsolPivot = shipmentToCheck.Consols.GetRelationshipBusinessObject(ConsolParent);
				try
				{
					shipmentConsolPivot.RunDeleteCheckers();
				}
				catch (CannotDeleteException ex)
				{
					return ex.Message;
				}
			}

			return string.Empty;
		}

		#endregion

		#region Related Shipments

		public ShipmentCollection RelatedShipments
		{
			get
			{
				if (relatedShipments == null)
				{
					relatedShipments = new ShipmentCollection(Factory);

					ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(CommonShipment));

					filter.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, Shipment.PK);
					filter.AddToFilter(JobShipmentSchema.JS_IsCancelled, ZBool.False);
					filter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, Shipment.JS_RL_NKDestination);
					filter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, Shipment.JS_RL_NKOrigin);
					filter.AddToFilter(JobShipmentSchema.JS_JS_ColoadMasterShipment, null);
					filter = AddDocAddressFilter(filter, Shipment.ConsignorDocumentaryAddress);
					filter = AddDocAddressFilter(filter, Shipment.ConsigneeDocumentaryAddress);

					if (IsSailingParent)
					{
						filter.AddToFilter(JobShipmentSchema.JS_JX, Shipment.JS_JX);
					}
					else if (ConsolParent != null)
					{
						ZDBOnlySubQuery consolPivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
						consolPivotSubQuery.AddToFilter(JobConShipLinkSchema.JN_JK, ConsolParent.PK);
						filter.AddSubQuery(consolPivotSubQuery, JoinCondition.And);
					}

					ShipmentCollection potentialRelatedShipments = new ShipmentCollection(Factory, filter);
					potentialRelatedShipments.Load();

					foreach (CommonShipment potentialShipment in potentialRelatedShipments)
					{
						if (ShipmentCanBeCombined(potentialShipment))
						{
							relatedShipments.Add(potentialShipment);

							if (HasUnmatchedConsigneeOrConsignor(potentialShipment))
							{
								potentialShipment.AddRowWarning(Res.GetString("a8102e72-944a-407b-9886-e28563668e9d", "There are still UNMATCHED organization details on this Shipment. Any Organization details found in the Unmatched Organization Details note will be lost if you continue to merge this Shipment."));
							}
						}
					}

					relatedShipments.SetReadOnlyIncludingChildren(true);
				}

				return relatedShipments;
			}
		}
		ShipmentCollection relatedShipments;

		bool HasUnmatchedConsigneeOrConsignor(CommonShipment shipment)
		{
			return shipment.ConsigneePK == OrgHeader.UnmatchedOrganisationPK
				|| shipment.ConsignorPK == OrgHeader.UnmatchedOrganisationPK;
		}

		void ResetRelatedShipments()
		{
			relatedShipments = null;
		}

		ZDBOnlyQuery AddDocAddressFilter(ZDBOnlyQuery soFar, JobDocAddress docAddress)
		{
			ZDBOnlyQuery result = soFar;
			if (docAddress.Organisation != null)
			{
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

				orgHeaderSubQuery.AddToFilter(OrgAddressSchema.OA_OH, docAddress.OrganisationPK);
				orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, docAddress.E2_AddressType);
				docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			}
			else if (docAddress.IsValidAddress && docAddress.E2_AddressOverride)
			{
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);

				result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
				result.AddToFilter(JobDocAddressSchema.E2_CompanyName, docAddress.E2_CompanyNameTruncated);
				result.AddToFilter(JobDocAddressSchema.E2_City, docAddress.E2_City);
				result.AddToFilter(JobDocAddressSchema.E2_State, docAddress.E2_State);
				result.AddToFilter(JobDocAddressSchema.E2_RN_NKCountryCode, docAddress.E2_RN_NKCountryCode);
				result.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			}

			return result;
		}

		#endregion

		#region Implementation

		bool IsSailingParent
		{
			get { return SailingParent != null; }
		}

		bool CheckParentExists(CommonShipment shipmentToCheck)
		{
			return IsSailingParent ? shipmentToCheck.JS_JX == SailingParent.PK : shipmentToCheck.Consols.Contains(ConsolParent);
		}

		bool CheckShipmentDetailsExist(CommonShipment shipmentToCheck)
		{
			return !shipmentToCheck.JS_RL_NKDestination.IsEmpty &&
				!shipmentToCheck.JS_RL_NKOrigin.IsEmpty
				&& shipmentToCheck.ConsigneeDocumentaryAddress.IsValidAddress
				&& shipmentToCheck.ConsignorDocumentaryAddress.IsValidAddress;
		}

		bool IsReferencedByOtherShipments(CommonShipment shipmentToCheck)
		{
			return Factory.LoadTop1<CommonShipment>(new ZQuery(JobShipmentSchema.JS_JS_ColoadMasterShipment, shipmentToCheck.PK)) != null;
		}

		bool HasBillingCharges(CommonShipment shipmentToCheck)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JobHeaderSchema.JH_ParentID, shipmentToCheck.PK);

			JobHeader relatedJob = Factory.LoadTop1<JobHeader>(filter);
			if (relatedJob != null)
			{
				return Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, relatedJob.PK)) != null;
			}

			return false;
		}

		void ConvertSiblingToArchived(CommonShipment sibling)
		{
			MergeNotesFromShipment(sibling);
			MoveOrdersToShipment(sibling);

			sibling.JS_ShipmentStatus = ShipmentStatusList.Codes.Transferred;

			bool automaticallyUpdatePackLineContainers = true;

			if (ConsolParent != null)
			{
				automaticallyUpdatePackLineContainers = ConsolParent.AutomaticallyUpdatePackLineContainers;
				ConsolParent.AutomaticallyUpdatePackLineContainers = false;
			}

			foreach (PackLine line in sibling.OuterPackLines)
			{
				var args = new BusinessObjectCloneArgs(Enumerable.Empty<string>(), true);
				PackLine clonedLine = (PackLine)line.Clone(args);
				Shipment.OuterPackLines.Add(clonedLine);

				CommonContainer linesContainer = null;
				if (IsSailingParent)
				{
					linesContainer = line.GetContainer(SailingParent);
					clonedLine.SetContainer(SailingParent, linesContainer);
				}
				else
				{
					linesContainer = line.GetContainer(ConsolParent);
					clonedLine.SetContainer(ConsolParent, linesContainer);
				}

				if (linesContainer != null)
				{
					linesContainer.PackLines.Remove(line);
				}
			}

			foreach (PackLine line in sibling.InnerPackLines)
			{
				PackLine clonedLine = (PackLine)line.Clone();
				Shipment.InnerPackLines.Add(clonedLine);
			}

			if (ConsolParent != null)
			{
				ConsolParent.AutomaticallyUpdatePackLineContainers = automaticallyUpdatePackLineContainers;
			}

			if (!IsSailingParent && sibling.Consols.Contains(ConsolParent))
			{
				sibling.Consols.Remove(ConsolParent);
			}

			Shipment.JS_ActualWeight += sibling.JS_ActualWeight;
			Shipment.JS_ActualVolume += sibling.JS_ActualVolume;
			Shipment.JS_OuterPacks += sibling.JS_OuterPacks;
			Shipment.JS_TotalPackageCount += sibling.JS_TotalPackageCount;

			if (Shipment.JS_F3_NKPackType != sibling.JS_F3_NKPackType)
			{
				Shipment.JS_F3_NKPackType = (ZString)FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			}

			if (Shipment.JS_F3_NKTotalCountPackType != sibling.JS_F3_NKTotalCountPackType)
			{
				Shipment.JS_F3_NKTotalCountPackType = (ZString)FreightPacksDataRegistry.Instance.InnerPackUnit.Value;
			}

			var workflowProvider = sibling as IWorkflowProvider;
			if (workflowProvider != null)
			{
				InitWorkflowItems(workflowProvider);
			}

			sibling.JS_IsCancelled = ZBool.True;

			JobHeader.DeactivateAllJobs(sibling, true);
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", Justification = "Calling getter")]
		static void InitWorkflowItems(IWorkflowProvider workflowProvider)
		{
			var workflowItems = workflowProvider.WorkflowItems;
		}

		void MoveOrdersToShipment(CommonShipment sourceShipment)
		{
			Enterprise.Integration.Forwarding.IOrder[] orders = Factory.Load<Enterprise.Integration.Forwarding.IOrder>(new ZQuery(JobOrderHeaderSchema.JD_JS, sourceShipment.PK));
			foreach (BusinessObject order in orders)
			{
				order[JobOrderHeaderSchema.JD_JS] = Shipment.PK;
			}
		}

		void MergeNotesFromShipment(CommonShipment sourceShipment)
		{
			foreach (StmNote srcNote in sourceShipment.Notes.GetAllNotes())
			{
				var destNote = Shipment.Notes.FindByDescription(srcNote.ST_Description);
				var isSrcNoteSerializableNote = IsSerializableNote(srcNote);
				if (destNote.Length > 0)
				{
					if (!isSrcNoteSerializableNote)
					{
						destNote[0].ST_NoteDataAsText += System.Environment.NewLine + System.Environment.NewLine + srcNote.ST_NoteDataAsText;
					}
				}
				else
				{
					var clonedNote = (StmNote)srcNote.Clone();
					Shipment.Notes.Add(clonedNote);

					if (isSrcNoteSerializableNote)
					{
						clonedNote.ST_NoteText = srcNote.ST_NoteDataAsText;
					}
				}
			}

			string noteText = Res.GetString("d34fda01-f740-4769-bda7-924344ad695c", "Packages transferred to Shipment {0} by {1} on {2}", Shipment.JS_UniqueConsignRef, GlbStaff.CurrentUser.GS_FullName, ZDateTime.Now);
			StmNote[] existingNotes = sourceShipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.ToString());
			if (existingNotes.Length > 0)
			{
				existingNotes[0].ST_NoteDataAsText += System.Environment.NewLine + System.Environment.NewLine + noteText;
			}
			else
			{
				StmNote transferNote = sourceShipment.Notes.AddNew();
				transferNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
				transferNote.ST_NoteType = PredefinedNoteTypes.Instance.HandlingInstructions.DefaultVisibility.ToString();
				transferNote.ST_NoteDataAsText = noteText;
			}
		}

		bool IsSerializableNote(StmNote note)
		{
			var desc = note.ST_Description;
			var noteType = note.ST_Description_List
				.Cast<PredefinedNoteType>()
				.FirstOrDefault(x => x.Code.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase) || x.Description.Trim().Equals(desc, StringComparison.OrdinalIgnoreCase));

			return noteType != null && noteType.SerializableNoteType != null;
		}

		#endregion
	}
}
