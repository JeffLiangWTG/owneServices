using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentVariation : AutoDtbConsignmentVariation, IWorkflowTriggerEventSource
	{
		public DtbConsignmentVariation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.Statuses")]
		public override ZString LTV_Status
		{
			get => base.LTV_Status;
			set => base.LTV_Status = value;
		}

		[List("Lookups.Types")]
		public override ZString LTV_Type
		{
			get => base.LTV_Type;
			set => base.LTV_Type = value;
		}

		public ZString DifferenceInQty
		{
			get
			{
				var difference = LTV_ActualQty - LTV_PlannedQty;

				if (difference == 0)
				{
					return Res.GetString("6c0e4a6e-3b6c-4076-a088-a88e843f36b5", "0 items");
				}

				var absoluteDifference = Math.Abs(difference);

				if (difference < 0)
				{
					return absoluteDifference == 1
						? Res.GetString("690529cd-ce94-47c0-b266-be7479c4be12", "{0} fewer item", absoluteDifference)
						: Res.GetString("feac3915-a295-430c-afc3-f273135eb97d", "{0} fewer items", absoluteDifference);
				}
				else
				{
					return absoluteDifference == 1
						? Res.GetString("0629cb61-edfc-4dca-988c-ccde6f1b022f", "{0} additional item", absoluteDifference)
						: Res.GetString("a31814f3-8177-44c5-8b30-143940157d18", "{0} additional items", absoluteDifference);
				}
			}
		}

		public ZString DifferenceInWeight => GetDifference(LTV_ActualWeight, LTV_PlannedWeight, LTV_WeightUQ);

		public ZString DifferenceInVolume => GetDifference(LTV_ActualVolume, LTV_PlannedVolume, LTV_VolumeUQ);

		ZString GetDifference(decimal actual, decimal planned, string unit)
		{
			if (actual.Equals(planned))
			{
				return Res.GetString("6b778809-c057-41a4-8f34-39586d4a4c8c", "{0} {1}", 0, unit);
			}
			var difference = actual - planned;
			return difference < 0 ? Res.GetString("5ffab92e-44c7-4707-9e27-5b5f3af823ec", "{0} fewer {1}", Math.Abs(difference), unit) : Res.GetString("a6805628-f812-4358-9b18-ad12e1e03880", "{0} additional {1}", Math.Abs(difference), unit);
		}

		#endregion

		#region Related Business Objects

		public DtbConsignment Consignment
		{
			get
			{
				if (LTV_JobTableCode == DtbConsignmentSchema.Constants.Prefix)
				{
					return Factory.Load<DtbConsignment>(LTV_JobId);
				}
				else if (LTV_ParentTableCode == DtbConsignmentActionPackageDivotSchema.Constants.Prefix)
				{
					var consignmentQuery = new ZDBOnlyQuery(typeof(DtbConsignment));
					var addressSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAddress), DtbConsignmentAddressSchema.LTS_LTC_Consignment);
					var actionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress);
					var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction);
					divotSubQuery.AddToFilter(DtbConsignmentActionPackageDivotSchema.PK, LTV_ParentId);

					actionSubQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
					addressSubQuery.AddSubQuery(actionSubQuery, JoinCondition.And);
					consignmentQuery.AddSubQuery(addressSubQuery, JoinCondition.And);

					return Factory.LoadTop1<DtbConsignment>(consignmentQuery);
				}

				throw new NotImplementedException($@"Variation has neither a Consignment as its job, nor an Action Package Divot as its parent. This case must be implemented.
LTV_ParentTableCode: {LTV_ParentTableCode}
LTV_JobTableCode: {LTV_JobTableCode}");
			}
		}

		public DtbConsignmentRunSheet RunSheet
		{
			get
			{
				if (LTV_JobTableCode == DtbConsignmentRunSheetSchema.Constants.Prefix)
				{
					return Factory.Load<DtbConsignmentRunSheet>(LTV_JobId);
				}
				else if (LTV_ParentTableCode == DtbConsignmentActionPackageDivotSchema.Constants.Prefix)
				{
					var runSheetQuery = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));
					var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
					var actionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction);
					var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction);
					divotSubQuery.AddToFilter(DtbConsignmentActionPackageDivotSchema.PK, LTV_ParentId);

					actionSubQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
					instructionSubQuery.AddSubQuery(actionSubQuery, JoinCondition.And);
					runSheetQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);

					return Factory.LoadTop1<DtbConsignmentRunSheet>(runSheetQuery);
				}

				throw new NotImplementedException($@"Variation has neither a Run Sheet as its job, nor an Action Package Divot as its parent. This case must be implemented.
LTV_ParentTableCode: {LTV_ParentTableCode}
LTV_JobTableCode: {LTV_JobTableCode}");
			}
		}

		public DtbConsignmentAction Action
		{
			get
			{
				if (LTV_ParentTableCode == DtbConsignmentActionPackageDivotSchema.Constants.Prefix)
				{
					var actionQuery = new ZDBOnlyQuery(typeof(DtbConsignmentAction));
					var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction);
					divotSubQuery.AddToFilter(DtbConsignmentActionPackageDivotSchema.PK, LTV_ParentId);
					actionQuery.AddSubQuery(divotSubQuery, JoinCondition.And);

					return Factory.LoadTop1<DtbConsignmentAction>(actionQuery);
				}

				return null;
			}
		}

		public DtbConsignmentAddress VariationAddress
		{
			get
			{
				if (LTV_ParentTableCode == DtbConsignmentActionPackageDivotSchema.Constants.Prefix)
				{
					var addressQuery = new ZDBOnlyQuery(typeof(DtbConsignmentAddress));
					var actionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress);
					var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction);
					divotSubQuery.AddToFilter(DtbConsignmentActionPackageDivotSchema.PK, LTV_ParentId);

					actionSubQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
					addressQuery.AddSubQuery(actionSubQuery, JoinCondition.And);

					return Factory.LoadTop1<DtbConsignmentAddress>(addressQuery);
				}

				throw new NotImplementedException($@"Variation doesn't have an Action Package Divot as its parent. This case must be implemented. LTV_ParentTableCode: {LTV_ParentTableCode}");
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("d6628ff3-bab8-4a2d-82fd-531ec012b47a", "Consignment Variation");

		#endregion

		#region IWorkflowTriggerEventSource Members

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var parents = new List<IWorkflowProviderCore>(2);
				parents.AddSafe(Consignment);
				parents.AddSafe(RunSheet);

				return parents;
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return GlbCompany.GetCurrentCompany(Factory); }
		}

		#endregion
	}
}
