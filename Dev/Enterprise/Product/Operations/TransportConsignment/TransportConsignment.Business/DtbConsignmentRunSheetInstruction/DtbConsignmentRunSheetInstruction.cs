using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[UniversalDataContext(DataContextType.TransportConsignmentRunSheetInstruction)]
	public class DtbConsignmentRunSheetInstruction : AutoDtbConsignmentRunSheetInstruction,
		IDtbConsignmentRunSheetInstruction,
		IPalletTransactionParent,
		IPalletTransactionConfirmationProvider,
		ISignatureSupporter,
		IWorkflowProvider,
		IProcessHandlingInfoProvider
	{
		public DtbConsignmentRunSheetInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region schema

		public new class Schema : AutoDtbConsignmentRunSheetInstruction.Schema
		{
		}

		#endregion

		#region Related Entities

		#region Address

		public JobDocAddress Address
		{
			get
			{
				JobDocAddress address = null;
				//#warning May need to cache, but then need to handle when allowing confirmations to be reassigned to different instructions.
				if (IsConsignmentConfirmation)
				{
					var confirmation = Confirmations.FirstOrDefault();
					var instruction = confirmation != null ? confirmation.Instruction : null;
					address = instruction != null ? instruction.Address : null;
				}
				else
				{
					var action = Actions.FirstOrDefault();
					var consignmentAddress = action != null ? action.ConsignmentAddress : null;
					address = consignmentAddress != null ? consignmentAddress.Address : null;
				}

				return address;
			}
		}

		#endregion

		#region Confirmations

		[ChildEditable]
		public DtbConsignmentConfirmationCollection Confirmations
		{
			get
			{
				if (confirmations == null)
				{
					confirmations = new DtbConsignmentConfirmationCollection(this);

					// update the GUI to reflect new totals (weight, volume etc.).
					confirmations.CollectionCountChange += (sender, e) =>
					{
						ClearTotalsCaches();

						var runsheet = !IsDeleted ? RunSheet : null;
						if (runsheet != null)
						{
							// Tested in DtbConsignmentRunSheet.cs
							runsheet.IsHazardousInfo.RefreshBinding();
							runsheet.RequiresRefrigerationInfo.RefreshBinding();
						}

						this.RefreshBinding();
					};
					RegisterEditableChildObject(confirmations);
				}

				return confirmations;
			}
		}

		DtbConsignmentConfirmationCollection confirmations;

		#endregion

		#region Actions

		[ChildEditable]
		public DtbConsignmentActionCollection Actions
		{
			get
			{
				if (actions == null)
				{
					actions = new DtbConsignmentActionCollection(this);
					RegisterEditableChildObject(actions);
				}

				return actions;
			}
		}

		DtbConsignmentActionCollection actions;

		#endregion

		#region ConsignmentActions

		public IEnumerable<IConsignmentAction> ConsignmentActions
		{
			get
			{
				if (IsConsignmentAction)
				{
					return Actions;
				}
				else
				{
					return Confirmations;
				}
			}
		}

		#endregion

		#region RunSheet

		public DtbConsignmentRunSheet RunSheet
		{
			get { return Factory.Load<DtbConsignmentRunSheet>(K1_KG_RunSheet); }
		}

		#endregion

		#region Pallet Transactions

		[ChildEditable]
		public PkgPalletTransactionConfirmationDependentCollection PalletTransactions
		{
			get
			{
				if (palletTransactions == null)
				{
					palletTransactions = new PkgPalletTransactionConfirmationDependentCollection(this);
					RegisterEditableChildObject(palletTransactions);
				}

				return palletTransactions;
			}
		}

		PkgPalletTransactionConfirmationDependentCollection palletTransactions;

		#endregion

		#endregion

		#region Properties

		#region K1_IsAcceptedByDriver

		[ReadOnly(true)]
		public override ZBool K1_IsAcceptedByDriver
		{
			get { return base.K1_IsAcceptedByDriver; }
			set { base.K1_IsAcceptedByDriver = value; }
		}

		#endregion

		#region K1_KG_RunSheet

		[RelatedBusinessObject("RunSheet")]
		public override ZGuid K1_KG_RunSheet
		{
			get { return base.K1_KG_RunSheet; }
			set
			{
				// tested in DtbConsignmentRunSheetTest.TestLockRunSheetAndCheckInstructions
				var previousRunSheet = RunSheet;
				if (previousRunSheet != null)
				{
					previousRunSheet.MarkAsNeedingCritialValidationCheck();
				}

				base.K1_KG_RunSheet = value;

				var newRunSheet = RunSheet;
				if (newRunSheet != null)
				{
					newRunSheet.MarkAsNeedingCritialValidationCheck();
				}
			}
		}

		#endregion

		#region K1_Sequence

		[ReadOnly(true)]
		public override ZInt K1_Sequence
		{
			get { return base.K1_Sequence; }
			set { base.K1_Sequence = value; }
		}

		#endregion

		#region K1_TimeIn

		public override ZDateTimeOffset K1_TimeIn
		{
			get { return base.K1_TimeIn; }
			set
			{
				var previousValue = K1_TimeIn;
				base.K1_TimeIn = value;

				if (previousValue != K1_TimeIn)
				{
					BookingEventsRequired = true;
				}
			}
		}

		#endregion

		#region K1_TimeOut

		public override ZDateTimeOffset K1_TimeOut
		{
			get { return base.K1_TimeOut; }
			set
			{
				var previousValue = K1_TimeOut;
				base.K1_TimeOut = value;

				if (previousValue != K1_TimeOut)
				{
					foreach (var confirmation in Confirmations)
					{
						confirmation.Instruction.UpdateStatus();
					}
					BookingEventsRequired = true;
				}
			}
		}

		#endregion

		#region K1_ReceivedBy

		[ReadOnly(true)]
		public override ZString K1_ReceivedBy
		{
			get { return base.K1_ReceivedBy; }
			set { base.K1_ReceivedBy = value; }
		}

		#endregion

		#region K1_ReceivedBySignature

		public override ZBlob K1_ReceivedBySignature
		{
			get { return base.K1_ReceivedBySignature; }
			set
			{
				foreach (var confirmation in Confirmations.Where(c => !c.HasSignature))
				{
					confirmation.RequiresSignatureEventLog = true;
				}

				base.K1_ReceivedBySignature = value;
			}
		}

		#endregion

		#region K1_FailureReason

		[List("Lookups.FailureReasons")]
		public override ZString K1_FailureReason
		{
			get { return base.K1_FailureReason; }
			set { base.K1_FailureReason = value; }
		}

		#endregion

		#region AddressAsSingleLine

		[ResourceStringData("DtbConsignmentRunSheetInstruction|AddressAsSingleLine", Caption = "Address")]
		public ZString AddressAsSingleLine
		{
			get
			{
				var address = Address;
				var addressLine = address != null ? address.GetAddressLine(Factory) : ZString.Empty;
				return addressLine.IsEmpty && IsOwnDepot ? Res.GetString("DtbConsignmentRunSheetInstruction|Depot", "DEPOT") : addressLine.ToString();
			}
		}

		#endregion

		#region FailureReasonDescription

		[ResourceStringData("DtbConsignmentRunSheetInstruction|FailureReasonDescription", Caption = "Failure Reason Description", ShortCaption = "Failure Description")]
		public ZString FailureReasonDescription
		{
			get
			{
				return Factory.GetCachedValue("DtbConsignmentRunSheetInstruction|FailureReasons|" + K1_FailureReason, () => GetFailureReasonDescription());
			}
		}

		ZString GetFailureReasonDescription()
		{
			var result = ZString.Empty;
			if (!K1_FailureReason.IsEmpty)
			{
				result = Lookups.FailureReasons.GetDescriptionFromCode(K1_FailureReason);
			}
			return result;
		}

		#endregion

		#region HasSignature

		[ResourceStringData("DtbConsignmentRunSheetInstruction|HasSignature", Caption = "Has Signature", MediumCaption = "Signature", ShortCaption = "Sign.")]
		public ZBool HasSignature
		{
			get { return !K1_ReceivedBySignature.IsEmpty; }
		}

		#endregion

		#region IsOwnDepot

		/// <summary>
		/// Should always have at least 1 confirmation. Maybe later when we allow free floating Depots.
		/// </summary>
		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|IsOwnDepot", Caption = "Is Own Depot", ShortCaption = "Depot")]
		public ZBool IsOwnDepot
		{
			get
			{
				bool result = false;
				if (IsConsignmentAction)
				{
					result = Actions.Any() && Actions.First().IsOwnDepot;
				}
				else
				{
					result = Confirmations.Any() && Confirmations.First().Instruction.IsOwnDepot;
				}
				return result;
			}
		}

		#endregion

		#region IsHazardous

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|IsHazardous", Caption = "Hazardous", ShortCaption = "Haz.")]
		public ZBool IsHazardous
		{
			get { return Confirmations.GetIsHazardous(); }
		}

		#endregion

		#region RequiresRefrigeration

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|RequiresRefrigeration", Caption = "Refrigeration", ShortCaption = "Re-frig.")]
		public ZBool RequiresRefrigeration
		{
			get { return Confirmations.GetRequiresRefrigeration(); }
		}

		#endregion

		// calculated -- totals

		#region BookedPickupPackages

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|BookedPickupPackages", Caption = "Booked Pickup", ShortCaption = "Booked PIC")]
		public ZString BookedPickupPackages
		{
			get { return bookedPickupPackages ?? (bookedPickupPackages = Confirmations.GetCompleteBookedPickupPackageSummary()).Value; }
		}

		ZString? bookedPickupPackages;

		#endregion

		#region TotalDeliveryPackages

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryPackages", Caption = "Delivery Packs", ShortCaption = "DLV Packs")]
		public ZInt TotalDeliveryPackages
		{
			get { return totalDeliveryPackages ?? (totalDeliveryPackages = Confirmations.GetTotalDeliveryPackages()).Value; }
		}

		ZInt? totalDeliveryPackages;

		#endregion

		#region TotalDeliveryVolume

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryVolume", Caption = "Delivery Volume", ShortCaption = "DLV Vol.")]
		public ZDecimal TotalDeliveryVolume
		{
			get { return totalDeliveryVolume ?? (totalDeliveryVolume = Confirmations.GetTotalDeliveryVolume()).Value; }
		}

		ZDecimal? totalDeliveryVolume;

		#endregion

		#region TotalDeliveryWeight

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalDeliveryWeight", Caption = "Delivery Weight", ShortCaption = "DLV Wgt.")]
		public ZDecimal TotalDeliveryWeight
		{
			get { return totalDeliveryWeight ?? (totalDeliveryWeight = Confirmations.GetTotalDeliveryWeight()).Value; }
		}

		ZDecimal? totalDeliveryWeight;

		#endregion

		#region TotalPickupPackages

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupPackages", Caption = "Pickup Packs", ShortCaption = "PIC Packs")]
		public ZInt TotalPickupPackages
		{
			get { return totalPickupPackages ?? (totalPickupPackages = Confirmations.GetTotalPickupPackages()).Value; }
		}

		ZInt? totalPickupPackages;

		#endregion

		#region TotalPickupVolume

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupVolume", Caption = "Pickup Volume", ShortCaption = "PIC Vol.")]
		public ZDecimal TotalPickupVolume
		{
			get { return totalPickupVolume ?? (totalPickupVolume = Confirmations.GetTotalPickupVolume()).Value; }
		}

		ZDecimal? totalPickupVolume;

		#endregion

		#region TotalPickupWeight

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalPickupWeight", Caption = "Pickup Weight", ShortCaption = "PIC Wgt.")]
		public ZDecimal TotalPickupWeight
		{
			get { return totalPickupWeight ?? (totalPickupWeight = Confirmations.GetTotalPickupWeight()).Value; }
		}

		ZDecimal? totalPickupWeight;

		#endregion

		#region TotalWeightUnit

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalWeightUnit", Caption = "Weight Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalWeightUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		#endregion

		#region TotalVolumeUnit

		[ResourceStringData("DtbConsignmentConfirmationTotalsHelper|TotalVolumeUnit", Caption = "Volume Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public ZString TotalVolumeUnit
		{
			get { return DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		#endregion

		#region ClearTotalsCaches

		void ClearTotalsCaches()
		{
			bookedPickupPackages = null;
			totalDeliveryPackages = null;
			totalDeliveryWeight = null;
			totalDeliveryVolume = null;
			totalPickupPackages = null;
			totalPickupWeight = null;
			totalPickupVolume = null;
		}

		#endregion

		#endregion

		#region Flags

		#region IsFailed

		public bool IsFailed => !K1_FailureReason.IsEmpty;

		#endregion

		#region IsCompleted

		public bool IsCompleted => !K1_TimeOut.IsEmpty && !K1_ReceivedBy.IsEmpty && K1_FailureReason.IsEmpty;

		#endregion

		#region IsInProgress

		public bool IsInProgress => !K1_TimeIn.IsEmpty && (K1_TimeOut.IsEmpty || K1_ReceivedBy.IsEmpty);

		#endregion

		#region IsAccepted

		public bool IsAccepted => K1_IsAcceptedByDriver && K1_TimeIn.IsEmpty;

		#endregion

		#region IsNotAccepted

		public bool IsNotAccepted => !K1_IsAcceptedByDriver;

		#endregion

		#region IsConsignmentAction

		public ZBool IsConsignmentAction
		{
			get { return !IsConsignmentConfirmation; }
		}

		ZBool IsConsignmentConfirmation
		{
			get { return Factory.LoadTop1<DtbConsignmentConfirmation>(new ZQuery(DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction, PK)) != null; }
		}

		#endregion

		#region IsDeliveringConsignments

		public bool IsDeliveringConsignments => Actions.Any(a => a.IsDelivery);

		#endregion

		#region IsPickingUpConsignments

		public bool IsPickingUpConsignments => Actions.Any(a => a.IsPickUp);

		#endregion

		#endregion

		#region Saving

		public override void OnSaving()
		{
			UpdateConsignmentLogsIfRequired();

			base.OnSaving();
		}

		void UpdateConsignmentLogsIfRequired()
		{
			if (BookingEventsRequired)
			{
				foreach (var confirmation in Confirmations)
				{
					var instruction = confirmation.Instruction;
					if (!K1_TimeInInfo.OriginalValue.Equals(K1_TimeIn))
					{
						instruction.AddLogToBooking(Events.Arrival, K1_TimeIn.ToUtcZDateTime().ToOffset());
					}

					if (!K1_TimeOutInfo.OriginalValue.Equals(K1_TimeOut))
					{
						instruction.AddLogToBooking(Events.Departure, K1_TimeOut.ToUtcZDateTime().ToOffset());
					}
				}

				BookingEventsRequired = false;
			}
		}

		bool BookingEventsRequired;

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbConsignmentRunSheetInstructionFetchStrategy(this);
		}

		#endregion

		#region Delete

		public sealed override void Delete()
		{
			RemoveConfirmations(); // tested by SaveAndDeleteBusinessObject
			RemoveActions(); // tested by SaveAndDeleteBusinessObject

			var instructions = RunSheet != null ? RunSheet.RunSheetInstructions : null;
			base.Delete();
			if (instructions != null)
			{
				instructions.Sequence();
			}

			var palletsOnThisInstruction = PalletTransactions.Where(x => x.KTR_ParentTableCode == DtbConsignmentRunSheetInstructionSchema.Constants.Prefix).ToArray();
			foreach (var pallet in palletsOnThisInstruction)
			{
				pallet.Delete();
			}

			// tested by WorkflowProvider tests
			WorkflowItems.RemoveAndDeleteAll();
		}

		void RemoveConfirmations()
		{
			foreach (var confirmation in Confirmations.ToArray())
			{
				Confirmations.RemoveFromRelationship(confirmation);
			}
		}

		void RemoveActions()
		{
			foreach (var action in Actions.ToArray())
			{
				Actions.RemoveFromRelationship(action);
			}
		}

		#endregion

		#region IPalletTransactionParent Members

		ZString IPalletTransactionParent.GetJobDescription(Type contextType)
		{
			if (contextType == typeof(DtbConsignmentRunSheetInstruction))
			{
				return Res.GetString("937b6cfb-da4e-4e3b-85d7-04af5b269b03", "Run Sheet");
			}

			return Res.GetString("f2e09014-e3d4-42af-8a83-920b9ea1438a", "Run Sheet {0} #{1}", RunSheet.KG_RunSheetNumber, K1_Sequence);
		}

		public IDocAddress TransferFrom(string transferType)
		{
			return null;
		}

		public IDocAddress TransferTo(string transferType)
		{
			return null;
		}

		GlbBranch IPalletTransactionParent.RelevantBranch
		{
			get { return RunSheet.TruckDriver != null ? RunSheet.TruckDriver.HomeBranch : null; }
		}

		IEnumerable<string> IPalletTransactionParent.JobReferences
		{
			get { yield return RunSheet.KG_RunSheetNumber + " #" + K1_Sequence; }
		}

		#endregion

		#region IPalletTransactionConfirmationProvider Members

		IEnumerable<DtbConsignmentConfirmation> IPalletTransactionConfirmationProvider.Confirmations
		{
			get { return Confirmations.ToArray(); }
		}

		#endregion

		#region ISignatureSupporter

		ZBlob ISignatureSupporter.ReceivedBySignature
		{
			get { return K1_ReceivedBySignature; }
		}

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbConsignmentRunSheetInstructionProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		DtbConsignmentRunSheetInstructionProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new DtbConsignmentRunSheetInstructionProcessHandlingInfo(this); }
		}

		#endregion

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (!K1_InstructionType.IsEmpty && !ValidNonEmptyInstructionTypes.Contains(K1_InstructionType))
			{
				K1_InstructionType = ZString.Empty;
			}
		}

		static List<string> ValidNonEmptyInstructionTypes => new List<string>()
		{
			"WEI",
			"ORG",
			"DST",
			"VIA"
		};

#endif

		#endregion
	}
}
