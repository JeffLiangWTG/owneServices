using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendCargoManifestEntryStatusQueryActionMethodApplicator : USDeclarationOperationalActionMethodApplicator
	{
		public SendCargoManifestEntryStatusQueryActionMethodApplicator(BusinessObjectFactory factory)
			: base("Send Cargo/Manifest/Entry Status Query operational action", factory)
		{ }

		#region Schema

		public new class Schema : USDeclarationOperationalActionMethodApplicator.Schema
		{
			public const string Action = "Action";
			public const int ActionMaxLength = 3;
			public const string OutputOption = "OutputOption";
			public const int OutputOptionMaxLength = 21;
			public const string RequestForReleatedBOL = "RequestForReleatedBOL";
			public const string UpdateEntryWithResults = "UpdateEntryWithResults";
		}

		#endregion

		#region RequestForReleatedBOL

		bool RequestForReleatedBOL_ReadOnly
		{
			get { return Action != CargoManifestStatusQueryActionList.Codes.MAWB && Action != CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill; }
		}

		[ReadOnlyMember(nameof(RequestForReleatedBOL_ReadOnly))]
		public ZBool RequestForReleatedBOL
		{
			get => requestForReleatedBOL;
			set
			{
				SetNonPersistentPropertyValue(RequestForReleatedBOLInfo, ref requestForReleatedBOL, value);
			}
		}
		ZBool requestForReleatedBOL;
		public ZPropertyInfo RequestForReleatedBOLInfo => GetZPropertyInfo(Schema.RequestForReleatedBOL);

		#endregion

		#region UpdateEntryWithResults

		bool UpdateEntryWithResults_ReadOnly
		{
			get
			{
				return Action != CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill &&
						Action != CargoManifestStatusQueryActionList.Codes.MAWB &&
						Action != CargoManifestStatusQueryActionList.Codes.HAWB &&
						!(Action == CargoManifestStatusQueryActionList.Codes.Entry && USCustomsDataRegistry.Instance.RequestForBillAndEntryData.Value);
			}
		}

		[ReadOnlyMember(nameof(UpdateEntryWithResults_ReadOnly))]
		public ZBool UpdateEntryWithResults
		{
			get => updateEntryWithResults;
			set
			{
				SetNonPersistentPropertyValue(UpdateEntryWithResultsInfo, ref updateEntryWithResults, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutputOption();
				}

				DefaultLimitOutputOptionIfRequired();
			}
		}
		ZBool updateEntryWithResults;
		public ZPropertyInfo UpdateEntryWithResultsInfo => GetZPropertyInfo(Schema.UpdateEntryWithResults);

		#endregion

		#region Action

		[List(nameof(Lookups) + "." + nameof(SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups.ActionList))]
		public ZString Action
		{
			get => action;
			set
			{
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAction();
				}

				if (RequestForReleatedBOL_ReadOnly)
				{
					RequestForReleatedBOL = false;
				}

				if (UpdateEntryWithResults_ReadOnly)
				{
					UpdateEntryWithResults = false;
				}

				DefaultLimitOutputOptionIfRequired();
			}
		}
		ZString action;
		public ZPropertyInfo ActionInfo => GetZPropertyInfo(Schema.Action);

		#endregion

		#region OutputOption

		[List(nameof(Lookups) + "." + nameof(SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups.OutputOptionList))]
		public ZString OutputOption
		{
			get => outputOption;
			set
			{
				SetNonPersistentPropertyValue(OutputOptionInfo, ref outputOption, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutputOption();
				}
			}
		}
		ZString outputOption;
		public ZPropertyInfo OutputOptionInfo => GetZPropertyInfo(Schema.OutputOption);

		#endregion

		#region GetNewLookups

		public SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups Lookups
		{
			get { return lookups ?? (lookups = new SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups(this)); }
		}

		SendCargoManifestEntryStatusQueryActionMethodApplicatorLookups lookups;

		#endregion

		#region GetValidation

		protected override USActionMethodApplicatorValidation GetValidation() => new SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation(this);

		public new SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation Validation => (SendCargoManifestEntryStatusQueryActionMethodApplicatorValidation)base.Validation;

		public override ValidationModes ValidationMode => ValidationModes.None;

		#endregion

		#region Overrides

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var runner = new SendCargoManifestEntryStatusQueryActionRunner(log, Action, OutputOption, UpdateEntryWithResults, RequestForReleatedBOL);
			jobsPK = new List<ZGuid>();
			jobsPK.AddRange(runner.PerformFunctionOperationalAction(true, targets));
		}

		protected override ZString MessageDescriptionCore => "Cargo/Manifest/Entry Status Query";

		#endregion

		#region New Methods

		void DefaultLimitOutputOptionIfRequired()
		{
			var outputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
			if (Action == CargoManifestStatusQueryActionList.Codes.Entry || UpdateEntryWithResults)
			{
				outputOption = LimitOutputCodeList.Codes._0MostRecentResults;
			}

			OutputOption = outputOption;
		}

		#endregion
	}
}
