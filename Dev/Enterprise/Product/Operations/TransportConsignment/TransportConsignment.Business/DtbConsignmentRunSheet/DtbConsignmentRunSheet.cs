using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	[UserDefinedValues]
	[CodeProperty(DtbConsignmentRunSheetSchema.Constants.KG_RunSheetNumber)]
	[DescriptionProperty(DtbConsignmentRunSheetSchema.Constants.KG_RunSheetNumber)]
	[DebuggerDisplay("UniqueID={KG_RunSheetNumber}")]
	[UniversalDataContext(DataContextType.TransportConsignmentRunSheet)]
	[VisualizableDocumentsSupportable(nameof(DtbConsignmentRunSheetVisualizableDocumentSupporter))]
	public partial class DtbConsignmentRunSheet :
		AutoDtbConsignmentRunSheet,
		IDtbConsignmentRunSheet,
		IEDocsProvider,
		IJobCostingPlugIn,
		IRatingSupporter,
		IWorkflowProvider,
		IJobNumber,
		IProcessHandlingInfoProvider,
		IHaveServices,
		ICustomFieldProvider
	{
		public DtbConsignmentRunSheet(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region schema

		public new class Schema : AutoDtbConsignmentRunSheet.Schema
		{
			public const string IsHazardous = "IsHazardous";
			public const string RequiresRefrigeration = "RequiresRefrigeration";
			public const string Status = "Status";
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			KG_EndTime = ZDateTimeOffset.Today.EndOfDay();
			KG_StartTime = ZDateTimeOffset.Today;

			KG_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		#endregion

		#region NoteTypesCore

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection fNoteTypes = base.NoteTypesCore;
				fNoteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

				return fNoteTypes;
			}
		}

		#endregion

		#region Related Entities

		#region RunSheetInstructions

		[ChildEditable]
		public DtbConsignmentRunSheetInstructionCollection RunSheetInstructions
		{
			get
			{
				if (runSheetInstructions == null)
				{
					runSheetInstructions = new DtbConsignmentRunSheetInstructionCollection(this);
					RegisterEditableChildObject(runSheetInstructions);
					var confirmations = runSheetInstructions.SelectMany(r => r.Confirmations).ToArray();
					Factory.InitialiseConfirmationsTotalsCache(PK.ToString(), confirmations);
					OriginalInstructionsCache = RunSheetInstructions.Select(i => i.PK.ToGuid()).ToArray();
					foreach (var confirmation in confirmations)
					{
						Factory.AddFetchHint(typeof(DtbConsignmentInstruction), confirmation.KK_KN_BookingInstruction);
					}
					runSheetInstructions.CollectionCountChange += RunSheetInstructions_CollectionCountChange;
				}
				return runSheetInstructions;
			}
		}

		void RunSheetInstructions_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			IsHazardousInfo.RefreshBinding();
			RequiresRefrigerationInfo.RefreshBinding();
		}

		DtbConsignmentRunSheetInstructionCollection runSheetInstructions;

		/// <summary>
		/// Clear the Original Instruction Cache if this run sheet was loaded into memory after RoutePlanner was refreshed
		/// ie. was created by another user, we should only allow assigned to it if no instructions exist
		/// </summary>
		internal void ClearOriginalInstructionsCache()
		{
			var load = RunSheetInstructions;
			OriginalInstructionsCache = Array.Empty<Guid>();
		}

		IEnumerable<Guid> OriginalInstructionsCache; // instructions are added to runsheets via the route planner, if OriginalInstructionsCache causes an issue in your test, your test is wrong, use RunSheet.AddNewRunSheetInstructions

		#endregion

		#region Variations

		DtbConsignmentVariation[] GetVariationsFromDatabase()
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignmentVariation));
			query.AddToFilter(DtbConsignmentVariationSchema.LTV_ParentTableCode,
				DtbConsignmentActionPackageDivotSchema.Constants.Prefix);

			var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.PK);
			var actionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.PK);
			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.PK);
			instructionSubQuery.AddToFilter(DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, PK);

			actionSubQuery.AddSubQuery(DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction, instructionSubQuery, JoinCondition.And);
			divotSubQuery.AddSubQuery(DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction, actionSubQuery, JoinCondition.And);
			query.AddSubQuery(DtbConsignmentVariationSchema.LTV_ParentId, divotSubQuery, JoinCondition.And);

			return Factory.Load<DtbConsignmentVariation>(query);
		}

		#endregion

		#endregion

		#region Properties

		#region KG_ActualDuration

		[ZDateTimeDurationValue]
		public override ZDateTime KG_ActualDuration
		{
			get => base.KG_ActualDuration;
			set => base.KG_ActualDuration = value.ConvertToDurationBasedDate(KG_ActualDurationInfo);
		}

		#endregion

		#region KG_AdHocDriversName

		[ReadOnlyMember(nameof(IsAdHocDriversNameAndLicenseReadOnly))]
		public override ZString KG_AdHocDriversName
		{
			get { return base.KG_AdHocDriversName; }
			set
			{
				base.KG_AdHocDriversName = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateKG_OH_TransportCo();
				}
			}
		}

		#endregion

		#region KG_AdHocDriversLicence

		[ReadOnlyMember(nameof(IsAdHocDriversNameAndLicenseReadOnly))]
		public override ZString KG_AdHocDriversLicence
		{
			get { return base.KG_AdHocDriversLicence; }
			set { base.KG_AdHocDriversLicence = value; }
		}

		#endregion

		#region	KG_AdHocTruckRegistration

		[ReadOnlyMember(nameof(IsAdHocTruckRegistrationReadOnly))]
		public override ZString KG_AdHocTruckRegistration
		{
			get { return base.KG_AdHocTruckRegistration; }
			set { base.KG_AdHocTruckRegistration = value; }
		}

		#endregion

		#region KG_AdHocTransportCoName

		[ReadOnlyMember(nameof(IsAdHocTransportCoNameReadOnly))]
		public override ZString KG_AdHocTransportCoName
		{
			get { return base.KG_AdHocTransportCoName; }
			set { base.KG_AdHocTransportCoName = value; }
		}

		#endregion

		#region KG_GS_NKTruckDriver

		public override ZString KG_GS_NKTruckDriver
		{
			get { return base.KG_GS_NKTruckDriver; }
			set
			{
				base.KG_GS_NKTruckDriver = value;
				if (TruckDriver != null)
				{
					KG_AdHocDriversName = "";
					KG_AdHocDriversLicence = "";
				}
			}
		}

		#endregion

		#region KG_OH_TransportCo

		public override ZGuid KG_OH_TransportCo
		{
			get { return base.KG_OH_TransportCo; }
			set
			{
				base.KG_OH_TransportCo = value;
				if (TransportCo != null)
				{
					KG_AdHocTransportCoName = "";
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateKG_AdHocDriversName();
				}
			}
		}

		#endregion

		#region KG_RQ_Truck

		public override ZGuid KG_RQ_Truck
		{
			get { return base.KG_RQ_Truck; }
			set
			{
				base.KG_RQ_Truck = value;
				if (Truck != null)
				{
					KG_AdHocTruckRegistration = "";
				}
			}
		}

		#endregion

		#region KG_StartTime

		public override ZDateTimeOffset KG_StartTime
		{
			get { return base.KG_StartTime; }
			set
			{
				var previousValue = KG_StartTime;
				base.KG_StartTime = value;

				if (previousValue != KG_StartTime)
				{
					// test in DtbConsignmentRunSheetValidation
					ValidateTruckAndDriver();
				}

				if (value.IsValid && KG_EndTime.IsValid)
				{
					KG_Duration = GetDuration(KG_StartTime, KG_EndTime);
				}
			}
		}

		#endregion

		#region KG_EndTime

		public override ZDateTimeOffset KG_EndTime
		{
			get { return base.KG_EndTime; }
			set
			{
				var previousValue = KG_EndTime;
				base.KG_EndTime = value;

				if (value.IsValid && KG_StartTime.IsValid)
				{
					KG_Duration = GetDuration(KG_StartTime, KG_EndTime);
				}

				if (previousValue != KG_EndTime)
				{
					// test in DtbConsignmentRunSheetValidation
					ValidateTruckAndDriver();
				}
			}
		}

		#endregion

		#region KG_Duration

		[ZDateTimeDurationCalculatedOnEmpty]
		[ResourceStringData("DtbConsignmentRunSheet|Duration", Caption = "Run Sheet Duration", ShortCaption = "Duration")]
		public override ZDateTime KG_Duration
		{
			get { return base.KG_Duration; }
			set { base.KG_Duration = value.IsEmpty ? GetDuration(KG_StartTime, KG_EndTime) : value.ConvertToDurationBasedDate(KG_DurationInfo); }
		}

		ZDateTime GetDuration(ZDateTimeOffset startTime, ZDateTimeOffset endTime)
		{
			if (startTime.IsValid && endTime.IsValid)
			{
				return (endTime - startTime);
			}

			return ZDateTime.Empty;
		}

		#endregion

		[ZDateTimeDurationValue]
		public override ZDateTime KG_TransitTime
		{
			get => base.KG_TransitTime;
			set => base.KG_TransitTime = value.ConvertToDurationBasedDate(KG_TransitTimeInfo);
		}

		#region KG_IsPlanning

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZBool KG_IsPlanning
		{
			get { return base.KG_IsPlanning; }
			set
			{
				base.KG_IsPlanning = value;
				StatusInfo.RefreshBinding();
			}
		}

		#endregion

		// calculated

		#region IsHazardous

		[ResourceStringData("DtbConsignmentRunSheet|IsHazardous", Caption = "Hazardous", ShortCaption = "Haz.")]
		public ZBool IsHazardous
		{
			get { return RunSheetInstructions.Any(a => a.Confirmations.Any(b => b.Instruction.Booking.KM_IsHazardous)); }
		}

		public ZPropertyInfo IsHazardousInfo
		{
			get { return GetZPropertyInfo(Schema.IsHazardous); }
		}

		#endregion

		#region RequiresRefrigeration

		[ResourceStringData("DtbConsignmentRunSheet|RequiresRefrigeration", Caption = "Refrigeration",
			ShortCaption = "Re-frig.")]
		public ZBool RequiresRefrigeration
		{
			get { return RunSheetInstructions.Any(a => a.Confirmations.Any(b => b.Instruction.Booking.KM_RequiresRefrigeration)); }
		}

		public ZPropertyInfo RequiresRefrigerationInfo
		{
			get { return GetZPropertyInfo(Schema.RequiresRefrigeration); }
		}

		#endregion

		// calculated

		#region DriversLicense

		[ResourceStringData("DtbConsignmentRunSheet|DriversLicense", Caption = "Driver's License", ShortCaption = "License")]
		public ZString DriversLicense
		{
			get
			{
				string result = null;

				var driver = TruckDriver;
				if (driver != null)
				{
					var certificate = driver.Certificates.FirstOrDefault(c => c.XZ_Type == CertificateTypePairList.Codes.CA1);
					if (certificate != null)
					{
						result = certificate.XZ_RefNumber;
					}
				}

				return result ?? KG_AdHocDriversLicence;
			}
		}

		#endregion

		#region DriversName

		[ResourceStringData("DtbConsignmentRunSheet|DriversName", Caption = "Driver")]
		public ZString DriversName
		{
			get
			{
				var driver = TruckDriver;
				return (driver != null) ? driver.GS_FullName : KG_AdHocDriversName;
			}
		}

		#endregion

		#region TransportCoName

		[ResourceStringData("DtbConsignmentRunSheet|TransportCoName", Caption = "Transport Company",
			ShortCaption = "Transport Co.")]
		public ZString TransportCoName
		{
			get
			{
				var transportCo = TransportCo;
				return (transportCo != null) ? transportCo.OH_FullNameTruncated : KG_AdHocTransportCoName;
			}
		}

		#endregion

		#region TruckRegistration

		[ResourceStringData("DtbConsignmentRunSheet|TruckRegistration", Caption = "Vehicle Registration",
			MediumCaption = "Registration", ShortCaption = "Rego.")]
		public ZString TruckRegistration
		{
			get
			{
				var truck = Truck;
				return (truck != null) ? truck.RQ_Registration : KG_AdHocTruckRegistration;
			}
		}

		#endregion

		#region ValidateTruckAndDriver

		public void ValidateTruckAndDriver()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateKG_GS_NKTruckDriver();
				Validation.ValidateKG_RQ_Truck();
				Validation.ValidateKG_AdHocDriversName();
				Validation.ValidateKG_OH_TransportCo();
			}
		}

		#endregion

		#endregion

		#region Flags

		// calculated

		#region IsInProgress

		[ResourceStringData("DtbConsignmentRunSheet|IsActive", Caption = "In Progress", ShortCaption = "In Progress")]
		public ZBool IsInProgress
		{
			get
			{
				return RunSheetInstructions.Any() &&
					   RunSheetInstructions.Any(i => !i.K1_TimeIn.IsEmpty || !i.K1_TimeOut.IsEmpty || i.K1_IsAcceptedByDriver) &&
					   !IsCompleted;
			}
		}

		#endregion

		#region IsCompleted

		ZBool IsCompleted
		{
			get { return RunSheetInstructions.Any() && RunSheetInstructions.All(i => !i.K1_TimeIn.IsEmpty && !i.K1_TimeOut.IsEmpty); }
		}

		#endregion

		#region HasNewConsignments

		public ZBool HasNewConsignments
		{
			get { return Factory.GetCachedValue(Invariant($"DtbConsignmentRunSheet|HasNewConsignments|{PK}"), () => RunSheetInstructions.Any(i => i.IsConsignmentAction)); } // Factory cached key must not be translatable.
		}

		#endregion

		#region Status

		[ResourceStringData("DtbConsignmentRunSheet|Status", Caption = "Run Sheet Status", ShortCaption = "Status")]
		public ZString Status
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsCompleted)
				{
					result = Res.GetString("82f5312d-a658-4345-aff1-7cbd7cd9715a", "Completed");
				}
				else if (IsInProgress)
				{
					result = Res.GetString("2dd006bc-ee35-4923-9ae8-072f1cb8cd41", "In Progress");
				}
				else if (!KG_IsPlanning)
				{
					result = Res.GetString("743ec580-75f1-4655-95f7-a8163eb3d0a8", "Not Started");
				}
				else if (KG_IsPlanning)
				{
					result = Res.GetString("c220060d-79fb-43f9-9da1-8f4b16dfb476", "Planning");
				}

				return result;
			}
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IsStaffDriver

		[ResourceStringData("DtbConsignmentRunSheet|IsStaffDriver", Caption = "Is Staff Driver", MediumCaption = "Is Staff", ShortCaption = "Staff")]
		public ZBool IsStaffDriver
		{
			get { return TruckDriver != null; }
		}

		#endregion

		#region SpecialInstructionsExist

		[ResourceStringData("DtbAddressPoint|SpecialInstructionsExist", Caption = "Special Instructions", MediumCaption = "Instructions", ShortCaption = "Sp. Instr.")]
		public ZBool SpecialInstructionsExist
		{
			get { return RunSheetInstructions.Any(i => i.Confirmations.Any(c => c.Instruction.SpecialInstructionExists)); }
		}

		#endregion

		#region HasFailedRunSheetInstructions

		[ResourceStringData("DtbConsignmentRunSheet|HasFailedRunSheetInstructions", Caption = "Failed instructions", MediumCaption = "Failures", ShortCaption = "Fail.")]
		public ZBool HasFailedRunSheetInstructions
		{
			get { return RunSheetInstructions.Any(rsi => !rsi.K1_FailureReason.IsEmpty); }
		}

		#endregion

		#endregion

		#region ReadOnly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required by tests - TestKG_AdHocDriversName_ReadOnly and TestKG_AdHocDriversLicence_ReadOnly")]
		bool IsAdHocDriversNameAndLicenseReadOnly
		{
			get { return IsStaffDriver; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required by test - TestKG_AdHocTruckRegistration_ReadOnly")]
		bool IsAdHocTruckRegistrationReadOnly
		{
			get { return Truck != null; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required by test - TestKG_AdHocTransportCoName_ReadOnly")]
		bool IsAdHocTransportCoNameReadOnly
		{
			get { return TransportCo != null; }
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbConsignmentRunSheetFetchStrategy(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d1bb081e-9c70-4a58-840d-cdf8c313d70e", "Run Sheet {0}", KG_RunSheetNumber); }
		}

		#endregion

		#region Save

		#region OnSaving

		public override void OnSaving()
		{
			PopulateUnqiueIDIfNeeded();
			base.OnSaving();
		}

		#endregion

		#region PopulateUnqiueIDIfNeeded

		protected void PopulateUnqiueIDIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && KG_RunSheetNumber.IsEmpty)
			{
				KG_RunSheetNumber = Env.NumberFountains.DtbConsignmentRunSheetID.GetNextFormatted(Factory);
			}
		}

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				KG_RunSheetNumber = "";
			}
		}

		#endregion

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (MarkedAsNeedingCritialValidationCheck && IsInDatabase)
			{
				LockRunSheetAndCheckInstructions();
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				OriginalInstructionsCache = RunSheetInstructions.Select(i => i.PK.ToGuid()).ToArray();
			}
		}

		#endregion

		#region LockRunSheetAndCheckInstructions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LockRunSheetAndCheckInstructions()
		{
			if (OriginalInstructionsCache == null)
			{
				var poke = RunSheetInstructions; // will populate cache
			}

			var sqlText = Invariant($@"
SELECT {DtbConsignmentRunSheetSchema.Constants.PK}
FROM {DtbConsignmentRunSheetSchema.Constants.SqlSchemaName}.{DtbConsignmentRunSheetSchema.Constants.TableName} WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
WHERE {DtbConsignmentRunSheetSchema.Constants.PK} = '{PK.ToString()}'

SELECT {DtbConsignmentRunSheetInstructionSchema.Constants.PK}
FROM {DtbConsignmentRunSheetInstructionSchema.Constants.SqlSchemaName}.{DtbConsignmentRunSheetInstructionSchema.Constants.TableName}
WHERE {DtbConsignmentRunSheetInstructionSchema.Constants.K1_KG_RunSheet} = '{PK.ToString()}'
");  // SQL query to run in DB

			var instructionPKsInDB = new List<Guid>();
			var command = ((IDbConnected)Factory).Connection.Command(sqlText);
			using (var reader = command.ExecuteReader()) // Prevent others from inserting instructions for this runsheet
			{
				reader.NextResult(); // skip the RunSheet Result
				while (reader.Read())
				{
					instructionPKsInDB.Add((Guid)reader[DtbConsignmentRunSheetInstructionSchema.Constants.PK]);
				}
			}

			RaiseOnAfterUPDLockForTest();

			var inDBButNotInFactory = instructionPKsInDB.Except(OriginalInstructionsCache);
			var inFactoryButNotInDB = OriginalInstructionsCache.Except(instructionPKsInDB);
			if (inDBButNotInFactory.Any() || inFactoryButNotInDB.Any())
			{
				throw new ZCannotSaveException(Invariant($@"Another user has already made changes to {HumanReadableName}.
Refresh the Route Planner and try again."), "Cannot Save Allocation.");
			}
		}

		#endregion

		#region MarkAsNeedingCritialValidationCheck

		internal void MarkAsNeedingCritialValidationCheck()
		{
			MarkedAsNeedingCritialValidationCheck = true;
		}

		bool MarkedAsNeedingCritialValidationCheck;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteAllInstructions();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		// Deleting Instructions calls Sequence on collection. To handle deletion properly we do the below.
		void DeleteAllInstructions()
		{
			foreach (var instruction in RunSheetInstructions.ToArray())
			{
				instruction.Delete();
			}
		}

		#endregion

		#region Add New Run Sheet Instructions

		#region AddNewRunSheetInstructions

		internal DtbConsignmentRunSheetInstruction[] AddNewRunSheetInstructions(DtbAddressPoint[] addressPoints)
		{
			Argument.NotNull(addressPoints, "addressPoints");
			Array.ForEach(addressPoints, a => Argument.NotNull(a, "addressPoint"));

			var result = new List<DtbConsignmentRunSheetInstruction>(addressPoints.Length);
			foreach (var addressPoint in addressPoints)
			{
				var instructions = AddNewRunSheetInstructions(addressPoint.Confirmations.ToArray());
				result.AddRange(instructions);
			}

			return result.ToArray();
		}

		/// <summary>
		/// Will assign all confirmations to a single new RunSheetInstruction if addresses are the same.
		/// In Direct Mode, addresses should differ and a new RunSheetInstruction will be created for each address.
		/// </summary>
		internal DtbConsignmentRunSheetInstruction[] AddNewRunSheetInstructions(params DtbConsignmentConfirmation[] confirmations)
		{
			var result = new List<DtbConsignmentRunSheetInstruction>();

			// Group confirmations by Address, should create one RunSheetInstruction per address
			var confirmationsGroupedByAddress = confirmations.ToLookup(c => c.Instruction.Address.GetAddressUniqueKey());
			foreach (var lookupKey in confirmationsGroupedByAddress.Select(l => l.Key))
			{
				var confirmationsByAddress = confirmationsGroupedByAddress[lookupKey];
				result.Add(AddNewRunSheetInstruction(confirmationsByAddress.ToArray()));
			}

			return result.ToArray();
		}

		#endregion

		#region AddNewRunSheetInstruction

		/// <summary>
		/// Always insert Instruction after Last Pickup or Delivery that is NOT a Depot.
		/// Link the delivery to its pickup if it's also on this RunSheet, otherwise link to a Depot.
		/// </summary>
		DtbConsignmentRunSheetInstruction AddNewRunSheetInstruction(DtbConsignmentConfirmation[] confirmations)
		{
			Argument.NotNull(confirmations, "confirmations");
			Array.ForEach(confirmations, c => Argument.NotNull(c, "confirmation"));
			EnsureConfirmationNotAlreadyAttachedToARunSheetInstruction(confirmations);

			// add a new run sheet instruction w/ confirmations and sequence.
			var runSheetInstruction = AddNewRunSheetInstructionAndSequence(GetNextSequence());
			runSheetInstruction.Confirmations.AddRange(confirmations);

			// create relationships with Depot or Direct Instructions.
			CreateConfirmationLinks(runSheetInstruction);

			return runSheetInstruction;
		}

		int GetNextSequence()
		{
			var lastNonDepotInstruction = RunSheetInstructions.OrderBy(i => i.K1_Sequence).LastOrDefault(i => !i.IsOwnDepot);
			return (lastNonDepotInstruction != null) ? lastNonDepotInstruction.K1_Sequence + 1 : 0;
		}

		#endregion

		#region AddNewRunSheetInstructionAndSequence

		/// <summary>
		/// Adds a New Run Sheet Instruction to the RunSheetInstruction Collection.
		/// Pass in a Sequence to insert in that position, otherwise it will be added to the end.
		/// </summary>
		DtbConsignmentRunSheetInstruction AddNewRunSheetInstructionAndSequence(int sequence = 0)
		{
			var runSheetInstruction = RunSheetInstructions.AddNew();

			if (sequence != 0)
			{
				foreach (var instruction in RunSheetInstructions)
				{
					if (instruction.K1_Sequence >= sequence)
					{
						instruction.K1_Sequence++;
					}
				}

				runSheetInstruction.K1_Sequence = sequence;
			}

			return runSheetInstruction;
		}

		#endregion

		#region EnsureConfirmationNotAlreadyAttachedToARunSheetInstruction

		// #warning this may change, not sure how we should handle this case.
		void EnsureConfirmationNotAlreadyAttachedToARunSheetInstruction(DtbConsignmentConfirmation[] confirmations)
		{
			if (confirmations.Any(c => !c.IsUnAllocated))
			{
				throw new ArgumentException("One or more confirmations are already attached to a RunSheet Instruction.");
			}
		}

		#endregion

		#endregion

		#region Confirmation Links

		#region CreateConfirmationLinks

		/// <summary>
		/// If there were any new Delivery Confirmations added to this runsheet where the Related Pickup was already attached,
		///	the Pickup no longer requires to be delivered at the Depot, so remove that depot.
		///	Otherwise the Pickups require a Delivery Depot and the Deliveries require a Pickup Depot.
		/// </summary>
		void CreateConfirmationLinks(DtbConsignmentRunSheetInstruction runSheetInstruction)
		{
			var sequence = runSheetInstruction.K1_Sequence;
			var confirmationLinkActions = BuildConfirmationLinkActions(runSheetInstruction);

			// do stuff with each action
			RemoveDepotLinkForDirects(confirmationLinkActions);
			LinkPickupConfirmationsToDeliveryDepot(confirmationLinkActions, sequence);
			LinkDeliveryConfirmationsToPickupDepot(confirmationLinkActions, sequence);
		}

		#endregion

		#region RemoveDepotLinkForDirects

		/// <summary>
		/// If there were any Delivery Confirmations added to this runsheet where the Related Pickup was already attached,
		///	the Pickup no longer requires to be delivered at the Depot, so remove that depot.
		/// </summary>
		void RemoveDepotLinkForDirects(IEnumerable<ConfirmationLinkActions> confirmationLinkActions)
		{
			var directConfirmationLinkActions = confirmationLinkActions.Where(a => a.AssignDeliveryToDirectPickup);
			foreach (var confirmationLinkAction in directConfirmationLinkActions)
			{
				var relatedDepotConfirmations = confirmationLinkAction.Pickup.GetRelatedDepotConfirmations();
				foreach (var confirmation in relatedDepotConfirmations)
				{
					if (!confirmation.IsDeleted) // deleting a delivery confirmation will delete the instruction, which will delete the pickup confirmation, we want to ignore this pickup confirmation now
					{
						if (confirmation.IsOwnDepot)
						{
							RemoveConfirmationAndRunSheetInstructionIfEmpty(confirmation);
							RemoveConsignmentDepotInstructionIfUnassigned(confirmation.Instruction);
						}
						else
						{
							throw new InvalidOperationException("Should only need to remove Depots when adding Directs.");
						}
					}
				}
			}
		}

		#endregion

		#region LinkPickupConfirmationsToDeliveryDepot

		/// <summary>
		/// Any newly added Pickups are to be linked to a Delivery Depot.
		/// </summary>
		void LinkPickupConfirmationsToDeliveryDepot(IEnumerable<ConfirmationLinkActions> confirmationLinkActions, int runSheetInstructionSequence)
		{
			var pickupConfirmationsRequiringDeliveryDepots = confirmationLinkActions.Where(a => a.AssignPickupToDeliveryDepot);
			if (pickupConfirmationsRequiringDeliveryDepots.Any())
			{
				InsertDepotOnConsignmentAndAssignConfirmations(true, runSheetInstructionSequence, pickupConfirmationsRequiringDeliveryDepots.Select(a => a.Pickup));
			}
		}

		#endregion

		#region LinkDeliveryConfirmationsToPickupDepot

		/// <summary>
		/// Any newly added Deliveries are to be linked to a Pickup Depot.
		/// </summary>
		void LinkDeliveryConfirmationsToPickupDepot(IEnumerable<ConfirmationLinkActions> confirmationLinkActions, int runSheetInstructionSequence)
		{
			var deliveryConfirmationsRequiringPickupDepots = confirmationLinkActions.Where(a => a.AssignDeliveryToPickupDepot);
			if (deliveryConfirmationsRequiringPickupDepots.Any())
			{
				InsertDepotOnConsignmentAndAssignConfirmations(false, runSheetInstructionSequence, deliveryConfirmationsRequiringPickupDepots.Select(a => a.Delivery));
			}
		}

		#endregion

		#region InsertDepotOnConsignmentAndAssignConfirmations

		void InsertDepotOnConsignmentAndAssignConfirmations(ZBool isPickup, int sequence, IEnumerable<DtbConsignmentConfirmation> confirmsToAssign)
		{
			foreach (var confirmation in confirmsToAssign) // will be either pickup *or* delivery confirmations.
			{
				// create the Depot Instruction if it never existed.

				confirmation.Transport.UpdateDepotInstructions(); //use when deleting direct

				var relatedDepotConfirmation = confirmation.GetRelatedConfirmation();
				if (!relatedDepotConfirmation.IsOwnDepot)
				{
					throw new InvalidOperationException("We just inserted a Depot, our confirmation should be linked to it.");
				}

				DtbConsignmentRunSheetInstruction depotInstruction;
				var sequencedInstructions = RunSheetInstructions.OrderBy(i => i.K1_Sequence);
				if (isPickup)
				{
					depotInstruction = sequencedInstructions.FirstOrDefault(i => i.K1_Sequence > sequence && i.IsOwnDepot && i.Address.E2_OA_Address == relatedDepotConfirmation.Instruction.Address.E2_OA_Address);
					if (depotInstruction == null)
					{
						depotInstruction = AddNewRunSheetInstructionAndSequence();
					}
				}
				else
				{
					depotInstruction = sequencedInstructions.FirstOrDefault(i => i.K1_Sequence < sequence && i.IsOwnDepot && i.Address.E2_OA_Address == relatedDepotConfirmation.Instruction.Address.E2_OA_Address);
					if (depotInstruction == null)
					{
						depotInstruction = AddNewRunSheetInstructionAndSequence(1); // 1 = Start
					}
				}

				depotInstruction.Confirmations.Add(relatedDepotConfirmation); // the depot confirmation gets its cached package values from its related Confirmation.
			}
		}

		#endregion

		#region ConfirmationLinkActions

		#region BuildConfirmationLinkActions

		/// <summary>
		/// All Confirmations added to a RunSheet require a linked Depot or Direct confirmation.
		/// BuildConfirmationLinkActions returns the list of newly added confirmations with their required Action.
		///		- assign Delivery to Direct Pickup.
		///		- assign Delivery to Pickup Depot.
		///		- assign Pickup to Delivery Depot.
		/// </summary>
		IEnumerable<ConfirmationLinkActions> BuildConfirmationLinkActions(DtbConsignmentRunSheetInstruction newRunSheetInstruction)
		{
			var result = new List<ConfirmationLinkActions>(newRunSheetInstruction.Confirmations.Count);

			var otherNonDepotConfirmationsOnThisRunSheet = RunSheetInstructions.Where(i => i != newRunSheetInstruction && !i.IsOwnDepot).SelectMany(i => i.Confirmations);
			var otherNonDepotConsignmentsOnThisRunSheet = GetConfirmationsByConsignments(otherNonDepotConfirmationsOnThisRunSheet);
			foreach (var confirmation in newRunSheetInstruction.Confirmations)
			{
				var transport = confirmation.Transport;
				if (otherNonDepotConsignmentsOnThisRunSheet.TryGetValue(transport, out List<DtbConsignmentConfirmation> relatedConfirmations))
				{
					if (relatedConfirmations.Count > 1)
					{
						throw new NotSupportedException("Cannot attach a third Non-Depot Confirmation from the same Consignment to a RunSheet. Basically it already contains a Direct.");
					}

					var relatedConfirmation = relatedConfirmations.First();
					if (relatedConfirmation.IsDelivery)
					{
						throw new NotSupportedException(Invariant($@"A Delivery Confirmation was added to this RunSheet before its Pickup for the same Consignment. Details of the delivery: run sheet number is {this.KG_RunSheetNumber}, consignmentID is {relatedConfirmation.ConsignmentID}, consignee name is {relatedConfirmation.ConsigneeName}."));
					}

					result.Add(new ConfirmationLinkActions(relatedConfirmation, confirmation));
				}
				else
				{
					result.Add(new ConfirmationLinkActions(confirmation));
				}
			}

			return result;
		}

		#endregion

		#region GetConfirmationsByConsignments

		// A RunSheet can contain a Pickup and Delivery confirmation from the Same Consignment, so we need to index a list.
		Dictionary<DtbBookingConsignment, List<DtbConsignmentConfirmation>> GetConfirmationsByConsignments(IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			var result = new Dictionary<DtbBookingConsignment, List<DtbConsignmentConfirmation>>();

			foreach (var confirmation in confirmations)
			{
				var transport = confirmation.Transport;

				List<DtbConsignmentConfirmation> transportConfirmations;
				if (!result.TryGetValue(transport, out transportConfirmations))
				{
					transportConfirmations = new List<DtbConsignmentConfirmation>();
					result.Add(transport, transportConfirmations);
				}

				transportConfirmations.Add(confirmation);
			}

			return result;
		}

		#endregion

		#region ConfirmationLinkActions

		class ConfirmationLinkActions
		{
			#region Constructor

			public ConfirmationLinkActions(DtbConsignmentConfirmation directPickup, DtbConsignmentConfirmation directDelivery)
			{
				this.Pickup = directPickup;
				this.Delivery = directDelivery;
				this.RequiredAction = RequiredActions.AssignDeliveryToDirectPickup;
			}

			public ConfirmationLinkActions(DtbConsignmentConfirmation pickupOrDeliveryRequiringRelatedDepot)
			{
				if (pickupOrDeliveryRequiringRelatedDepot.IsPickUp)
				{
					this.Pickup = pickupOrDeliveryRequiringRelatedDepot;
					this.RequiredAction = RequiredActions.AssignPickupToDeliveryDepot;
				}
				else
				{
					this.Delivery = pickupOrDeliveryRequiringRelatedDepot;
					this.RequiredAction = RequiredActions.AssignDeliveryToPickupDepot;
				}
			}

			public readonly DtbConsignmentConfirmation Pickup;
			public readonly DtbConsignmentConfirmation Delivery;

			#endregion

			#region RequiredAction

			enum RequiredActions
			{
				AssignDeliveryToDirectPickup,
				AssignDeliveryToPickupDepot,
				AssignPickupToDeliveryDepot
			}

			readonly RequiredActions RequiredAction;

			#endregion

			#region Flags

			public bool AssignDeliveryToDirectPickup
			{
				get { return RequiredAction == RequiredActions.AssignDeliveryToDirectPickup; }
			}

			public bool AssignDeliveryToPickupDepot
			{
				get { return RequiredAction == RequiredActions.AssignDeliveryToPickupDepot; }
			}

			public bool AssignPickupToDeliveryDepot
			{
				get { return RequiredAction == RequiredActions.AssignPickupToDeliveryDepot; }
			}

			#endregion
		}

		#endregion

		#endregion

		#endregion

		#region Remove Confirmation / RunSheet Instruction

		#region RemoveConfirmationFromInstruction

		public bool RemoveConfirmationFromInstruction(bool removePickupsForDirectDeliveries, params DtbConsignmentConfirmation[] itemsToRemove)
		{
			if (itemsToRemove.Any(c => c.IsUnAllocated || c.RunSheetInstruction.K1_KG_RunSheet != PK))
			{
				throw new InvalidOperationException("Should not be removing Confirmations from another RunSheet this way.");
			}

			var someItemsCouldNotBeRemoved = false;

			foreach (var confirmation in itemsToRemove)
			{
				someItemsCouldNotBeRemoved |= RemoveFromRunSheet(removePickupsForDirectDeliveries, confirmation);
			}

			return someItemsCouldNotBeRemoved;
		}

		#endregion

		#region RemoveRunSheetInstruction

		public bool RemoveRunSheetInstructions(bool removePickupsForDirectDeliveries, params DtbConsignmentRunSheetInstruction[] instructions)
		{
			if (instructions.Any(rsi => rsi.K1_KG_RunSheet != PK || rsi.IsOwnDepot))
			{
				throw new InvalidOperationException("Should not be removing Depot RunSheet Instructions or RunSheet Instructions from another RunSheet this way.");
			}

			var someItemsCouldNotBeRemoved = false;

			foreach (var rsi in instructions)
			{
				// related runsheet instructions can be deleted when removing, this check makes sure we don't try to remove an instruction deleted this way.
				if (!rsi.IsDeleted)
				{
					someItemsCouldNotBeRemoved |= RemoveFromRunSheet(removePickupsForDirectDeliveries, rsi);
				}
			}

			MarkAsNeedingCritialValidationCheck();

			return someItemsCouldNotBeRemoved;
		}

		#endregion

		#region RemoveFromRunSheet

		bool RemoveFromRunSheet(bool removePickupsForDirectDeliveries, DtbConsignmentRunSheetInstruction runSheetInstruction)
		{
			return RemoveFromRunSheetCore(removePickupsForDirectDeliveries, runSheetInstruction, runSheetInstruction.Confirmations.ToArray());
		}

		bool RemoveFromRunSheet(bool removePickupsForDirectDeliveries, DtbConsignmentConfirmation confirmation)
		{
			return RemoveFromRunSheetCore(removePickupsForDirectDeliveries, confirmation.RunSheetInstruction, confirmation);
		}

		bool RemoveFromRunSheetCore(bool removePickupsForDirectDeliveries, DtbConsignmentRunSheetInstruction runSheetInstruction, params DtbConsignmentConfirmation[] confirmations)
		{
			bool someItemsCouldNotBeRemoved = false;

			if (IsRunSheetInstructionOrRelatedDeliveriesCompleted(runSheetInstruction, confirmations))
			{
				someItemsCouldNotBeRemoved = true; // already completed
			}
			else
			{
				RemoveRelatedConfirmationAndLinks(confirmations);

				if (removePickupsForDirectDeliveries)
				{
					RemovePickupConfirmationAndLinksForDirect(confirmations);
				}

				RemoveConfirmationAndLinks(confirmations);
			}

			return someItemsCouldNotBeRemoved;
		}

		#region IsRunSheetInstructionOrRelatedDeliveriesCompleted

		bool IsRunSheetInstructionOrRelatedDeliveriesCompleted(DtbConsignmentRunSheetInstruction runSheetInstruction, IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return runSheetInstruction.IsCompleted || confirmations.Any(c => c.IsPickUp && IsRelatedDeliveryCompleted(c));
		}

		bool IsRelatedDeliveryCompleted(DtbConsignmentConfirmation confirmation)
		{
			bool result = false;

			var relatedConfirmation = confirmation.GetRelatedConfirmationExcludingDepot();
			if (relatedConfirmation != null && relatedConfirmation.IsDelivery)
			{
				var relatedRunSheetInstruction = relatedConfirmation.RunSheetInstruction;
				result = relatedRunSheetInstruction != null && relatedRunSheetInstruction.IsCompleted;
			}

			return result;
		}

		#endregion

		#endregion

		#region RemoveConfirmationAndLinks

		void RemoveConfirmationAndLinks(DtbConsignmentConfirmation[] confirmations)
		{
			foreach (var confirmation in confirmations)
			{
				RemoveConfirmationAndLinks(confirmation);
			}
		}

		void RemoveRelatedConfirmationAndLinks(DtbConsignmentConfirmation[] confirmations)
		{
			foreach (var confirmation in confirmations)
			{
				if (confirmation.IsPickUp)
				{
					var relatedDeliveryConfirmation = confirmation.GetRelatedConfirmationExcludingDepot();
					if (relatedDeliveryConfirmation != null)
					{
						RemoveConfirmationAndLinks(relatedDeliveryConfirmation);
					}
				}
			}
		}

		void RemovePickupConfirmationAndLinksForDirect(DtbConsignmentConfirmation[] confirmations)
		{
			foreach (var confirmation in confirmations)
			{
				if (IsDirectDeliveryConfirmationAndPickupNotCompleted(confirmation))
				{
					RemoveConfirmationAndRunSheetInstructionIfEmpty(confirmation.GetRelatedConfirmationExcludingDepot());
				}
			}
		}

		bool IsDirectDeliveryConfirmationAndPickupNotCompleted(DtbConsignmentConfirmation confirmation)
		{
			bool result = false;

			if (confirmation.IsDelivery)
			{
				var relatedPickupConfirmation = confirmation.GetRelatedConfirmationExcludingDepot();
				if (relatedPickupConfirmation != null)
				{
					var runSheetInstruction = relatedPickupConfirmation.RunSheetInstruction;
					result = runSheetInstruction != null && !runSheetInstruction.IsCompleted && runSheetInstruction.K1_KG_RunSheet == PK;
				}
			}

			return result;
		}

		/// <summary>
		/// Remove Confirmation From it's Run Sheet Instruction.
		/// Delete that Run Sheet Instruction if it has no more Confirmations.
		///
		/// If the related confirmation is a 
		/// - Depot, remove it and also remove it from the consignment if it no longer belongs to a RunSheet (was auto-added, so we auto-remove).
		/// - Direct Delivery, remove it from this Run Sheet.
		/// - Direct Pickup, keep it, but link it to a Delivery Depot.
		/// </summary>
		/// <param name="confirmation"></param>
		public void RemoveConfirmationAndLinks(DtbConsignmentConfirmation confirmation)
		{
			if (confirmation.IsOwnDepot)
			{
				throw new ArgumentException("Cannot remove a Depot Run Sheet Instruction this way.");
			}

			RemoveConfirmationAndRunSheetInstructionIfEmpty(confirmation);

			var relatedLegConfirmation = confirmation.GetRelatedConfirmation();
			if (relatedLegConfirmation != null)
			{
				if (relatedLegConfirmation.IsOwnDepot)
				{
					RemoveConfirmationAndRunSheetInstructionIfEmpty(relatedLegConfirmation);
				}
				else if (relatedLegConfirmation.IsDelivery)
				{
					RemoveConfirmationAndRunSheetInstructionIfEmpty(relatedLegConfirmation);
				}
				else if (relatedLegConfirmation.IsPickUp && !relatedLegConfirmation.IsUnAllocated) // should not link pickup to depot if it is no longer allocated
				{
					LinkPickupConfirmationsToDeliveryDepot(new[] { new ConfirmationLinkActions(relatedLegConfirmation) }, relatedLegConfirmation.RunSheetInstruction.K1_Sequence);
				}
			}

			confirmation.Transport.UpdateDepotInstructions();
		}

		#endregion

		#region RemoveConsignmentDepotInstructionIfUnassigned

		/// <summary>
		/// Remove the depot confirmation from its consignment if it no longer belongs to a run sheet.
		/// </summary>
		void RemoveConsignmentDepotInstructionIfUnassigned(DtbConsignmentInstruction depotInstruction)
		{
			if (!depotInstruction.IsOwnDepot)
			{
				throw new InvalidOperationException("Tried to delete a non-depot instruction from the consignment.");
			}

			if (!depotInstruction.Confirmations.Any(c => !c.IsUnAllocated))
			{
				depotInstruction.Delete();
			}
		}

		#endregion

		#region RemoveConfirmationAndRunSheetInstructionIfEmpty

		/// <summary>
		/// Remove the confirmation from it's run sheet instruction.
		/// Delete that run sheet instruction if it has no more confirmations.
		/// </summary>
		void RemoveConfirmationAndRunSheetInstructionIfEmpty(DtbConsignmentConfirmation confirmation)
		{
			var runSheetInstruction = confirmation.RunSheetInstruction;
			if (runSheetInstruction != null)
			{
				var runSheetInstructionConfirmations = runSheetInstruction.Confirmations;

				runSheetInstructionConfirmations.RemoveFromRelationship(confirmation);
				if (!runSheetInstructionConfirmations.Any())
				{
					runSheetInstruction.Delete();
				}
			}
		}

		#endregion

		#endregion

		#region InstructionsHaveAnyDirectDeliveryConfirmations

		public bool InstructionsHaveAnyDirectDeliveryConfirmations(DtbConsignmentRunSheetInstruction[] itemsToRemove)
		{
			var pickups = new HashSet<DtbConsignmentConfirmation>();
			var directDeliveries = new List<DtbConsignmentConfirmation>();

			foreach (var item in itemsToRemove)
			{
				pickups.UnionWith(item.Confirmations.Where(c => c.IsPickUp && !c.IsOwnDepot));
				directDeliveries.AddRange(GetDirectDeliveryConfirmations(item.Confirmations));
			}

			// there is no point asking to remove the related pickups, if the pickup has also been selected for removal
			return directDeliveries.Any(d => !pickups.Contains(d.GetRelatedConfirmationExcludingDepot()));
		}

		#endregion

		#region SomeConfirmationsHaveDirectDelivery

		public bool SomeDeliveryConfirmationsAreDirect(IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return GetDirectDeliveryConfirmations(confirmations).Any();
		}

		IEnumerable<DtbConsignmentConfirmation> GetDirectDeliveryConfirmations(IEnumerable<DtbConsignmentConfirmation> confirmations)
		{
			return from confirmation in confirmations
				   where IsDirectDeliveryConfirmationAndPickupNotCompleted(confirmation)
				   select confirmation;
		}

		#endregion

		// interfaces

		#region IAllowUserToDelete Members

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !RunSheetInstructions.Any();
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("DtbConsignmentRunSheet|ReasonForNotAbleToDelete", "A Run-sheet containing Instructions cannot be deleted. Remove all Instructions before deleting."); }
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DtbConsignmentRunSheetDocumentSupporter(this)); }
		}

		DocumentSupporter documentSupporter;

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		#endregion

		#region DocManagerInfo Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DtbConsignmentRunSheetDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobCostingPlugIn Members

		void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference)
		{
			Logs.AddNew(@event, reference);
		}

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn plugin)
		{
			var result = ZString.Empty;

			var consignment = plugin as DtbBookingConsignment;
			if (consignment != null)
			{
				var confirmations = consignment.AllConfirmations.Where(x => !x.IsOwnDepot).ToArray();
				var runSheetConfirmations = RunSheetInstructions.Where(i => !i.IsOwnDepot).SelectMany(i => i.Confirmations).ToArray();
				var runSheetHasPickup = runSheetConfirmations.Where(rsc => rsc.IsPickUp).Any(rsc => confirmations.Contains(rsc));
				var runSheetHasDelivery = runSheetConfirmations.Where(rsc => rsc.IsDelivery).Any(rsc => confirmations.Contains(rsc));

				result = runSheetHasPickup && runSheetHasDelivery ? "BOTH" : runSheetHasPickup ? "PIC" : "DLV";
			}

			return result;
		}

		RefCurrency IJobCostingPlugIn.ConsolCurrency
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ConsolExchangeRate
		{
			get { return 0m; }
		}

		RefUNLOCO IJobCostingPlugIn.DischargePort
		{
			get { return null; }
		}

		decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
		{
			return 0m;
		}

		bool IJobCostingPlugIn.IsMasterCollect
		{
			get { return false; }
		}

		ZString IJobCostingPlugIn.JK_UniqueConsignRef
		{
			get { return KG_RunSheetNumber; }
		}

		RefUNLOCO IJobCostingPlugIn.LoadPort
		{
			get { return null; }
		}

		JobProfitLossCollection IJobCostingPlugIn.ProfitLossContainer
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgent
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty
		{
			get { return null; }
		}

		OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty
		{
			get { return null; }
		}

		ZString IJobCostingPlugIn.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

		ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

		ZString IJobCostingPlugIn.Module => ApportionmentMethodModules.TransportBooking;

		ZString IJobCostingPlugIn.Direction => ZString.Empty;

		CodeDescriptionPairList IJobCostingPlugIn.PrepaidCollectList
		{
			get
			{
				var list = new PrepaidCollectList();
				list.RemoveCode(PrepaidCollectList.Codes.Both);

				return list;
			}
		}

		IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter
		{
			get { return new DtbConsignmentRunSheetCostSupporter(this); }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new DtbConsignmentRunSheetConsolRatingAdaptersProvider(this); }
		}

		#endregion

		#region IWorkflowProvider Members

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbConsignmentRunSheetProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		#endregion

		#region IWorkflowProviderCore Members

		CargoWise.Integration.IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return KG_RunSheetNumber; }
		}

		#endregion

		#region IHaveServices

		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new JobServiceDependentCollection(this, Factory);
					services.Load();
				}

				return services;
			}
		}
		JobServiceDependentCollection services;

		ZString IHaveServices.TransportMode
		{
			get { return ""; }
		}

		ZString IHaveServices.ContainerMode
		{
			get { return ""; }
		}

		IHaveServices[] IHaveServices.DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		BusinessObject IHaveServices.ServiceParent
		{
			get { return this; }
		}

		bool IHaveServices.NeedsServiceEvents
		{
			get { return false; }
		}

		ZString IHaveServices.TableCode
		{
			get { return TablePrefix; }
		}

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Factory.Load<GlbBranch>(KG_GB_Branch);

		#endregion

		#region ProcessHandlingInfo Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new DtbConsignmentRunSheetProcessHandlingInfo(this); }
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new DtbConsignmentRunSheetFountainUniqueIndexFailureHandler(this)); }
		}

		class DtbConsignmentRunSheetFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public DtbConsignmentRunSheetFountainUniqueIndexFailureHandler(DtbConsignmentRunSheet runSheet)
				: base(DtbConsignmentRunSheetSchema.Constants.Indexes.NR_UC__KG_RunSheetNumber, runSheet)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.DtbConsignmentRunSheetID; }
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant - Run Sheet Already Exists Trigger Message")]
		public const string RunSheetAlreadyExistsTriggerErrorMessage = "Run Sheet already exists for this Driver/Truck/Transport Co and Date Range.";

		partial void RaiseOnAfterUPDLockForTest();

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, KG_PL_NKCarrierServiceLevel)); }
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			customBusinessObject = shouldRefresh ? null : customBusinessObject;
			return CustomBusinessObject;
		}

		[ChildEditable]
		CustomBusinessObject CustomBusinessObject
		{
			get
			{
				if (customBusinessObject == null)
				{
					var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
					customBusinessObject = new CustomBusinessObject(Factory, this, properties);
					RegisterEditableChildObject(customBusinessObject);
					customBusinessObject.SetReadOnlyIncludingChildren(ReadOnly);
				}

				return customBusinessObject;
			}
		}
		CustomBusinessObject customBusinessObject;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				base.ReadOnly = value;
				if (customBusinessObject != null)
				{
					customBusinessObject.SetReadOnlyIncludingChildren(ReadOnly);
				}
			}
		}

		#endregion

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var relatedObjects = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				relatedObjects.AddRange(GetVariationsFromDatabase());

				return relatedObjects.ToArray();
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.TransportConsignment.Business
{
	public partial class DtbConsignmentRunSheet
	{
		public EventHandler OnAfterUPDLock;

		partial void RaiseOnAfterUPDLockForTest()
		{
			if (OnAfterUPDLock != null)
			{
				OnAfterUPDLock(this, EventArgs.Empty);
			}
		}
	}
}

#endif
#endregion
