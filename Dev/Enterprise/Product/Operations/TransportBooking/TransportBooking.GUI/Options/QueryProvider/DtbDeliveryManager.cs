using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.DataTransfer.Universal;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.GUI.Options.QueryProvider
{
	public partial class DtbDeliveryManager : IDtbDeliveryManager
	{
		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers)
			: this(factory, new[] { parent }, direction, combineContainers)
		{
			Argument.NotNull(parent, "parent");
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent[] parents, DtbBookingDirection direction, ZBool combineContainers)
			: this(factory, parents, direction, combineContainers, null)
		{
			Argument.NotNull(parents, "parents");
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger)
			: this(factory, new[] { parent }, direction, combineContainers, logger)
		{
			Argument.NotNull(parent, "parent");
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent[] parents, DtbBookingDirection direction, ZBool combineContainers, ILogger logger)
			: this(factory, parents, direction, combineContainers, logger, null)
		{
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager) : this(factory, new[] { parent }, direction, combineContainers, logger, errorManager)
		{
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent[] parents, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager) : this(factory, parents, direction, combineContainers, logger, errorManager, suppressDialogsAndUserInteractivity: false, selectAllContainers: false)
		{
		}

		public DtbDeliveryManager(BusinessObjectFactory factory, IDtbBookingParent[] parents, DtbBookingDirection direction, ZBool combineContainers, ILogger logger, IAutomatedDtbBookingCreationErrorManager errorManager, bool suppressDialogsAndUserInteractivity, bool selectAllContainers)
		{
			Parents = Argument.NotNull(parents, "parents");
			Direction = direction;
			Factory = factory;
			CombineContainers = combineContainers;
			Logger = logger;
			ErrorManager = errorManager;
			SuppressDialogsAndUserInteractivity = suppressDialogsAndUserInteractivity;
			SelectAllContainers = selectAllContainers;
		}

		IDtbBookingParent[] Parents { get; }
		DtbBookingDirection Direction { get; }
		BusinessObjectFactory Factory { get; }
		ZBool CombineContainers { get; }
		IAutomatedDtbBookingCreationErrorManager ErrorManager { get; }
#if DEBUG
		internal
#endif
		bool SuppressDialogsAndUserInteractivity { get; } // currently only guaranteed to suppress all dialogs and user interaction for calls to DeliverOrCreateMultiple()
#if DEBUG
		internal
#endif
		bool SelectAllContainers { get; }
#if DEBUG
		internal
#endif
		bool IsUserInteractive => Globals.IsUserInteractive && !SuppressDialogsAndUserInteractivity;
#if DEBUG
		internal
#endif
		bool CanShowDialogs => Globals.CanShowDialogs && !SuppressDialogsAndUserInteractivity;
#if DEBUG
		internal
#endif
		bool SelectAllContainersFromOptions(TransportBookingDocumentOptions options) => SelectAllContainers || options.IsFCL;

		ILogger Logger { get; }
		bool IsAutomated => Logger != null;
#if DEBUG
		internal
#endif
		IOperationalActionSectionLogLoggerWrapper OperationalActionSectionLogLoggerWrapper => Logger as IOperationalActionSectionLogLoggerWrapper;

		IDtbBookingParent Parent => Parents.FirstOrDefault();

		Shipment TopLevelDO
		{
			get
			{
				if (_topLevelDO == null && Parent != null)
				{
					_topLevelDO = GetTopLevelDO(Parent);
				}

				return _topLevelDO;
			}
		}

		Shipment _topLevelDO;

		Shipment GetTopLevelDO(IDtbBookingParent parent)
		{
			return (Shipment)UniversalXmlWriter.GetDataObject(Direction == DtbBookingDirection.PIC ? RecipientRoleType.PCA : RecipientRoleType.DCA, (BusinessObject)parent);
		}

		Shipment ParentDO
		{
			get
			{
				if (_parentDO == null && TopLevelDO != null)
				{
					_parentDO = Shipment.GetSourceDataObject(TopLevelDO);
				}

				return _parentDO;
			}
		}

		Shipment _parentDO;

		public void ViewTransportBooking()
		{
			var consolidation = GetExistingBookingConsolidation(Direction, Parent);
			if (consolidation != null)
			{
				ShowBooking(consolidation, DefaultBookingView);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("be1ebb37-750e-402d-adae-191f8ae18e86", "Could not find existing Transport Booking."));
			}
		}

		public void CreateTransportBooking()
		{
			if (CheckIsSaved()) // check at least they can access tbs or have on demand
			{
				var consolidation = GetExistingBookingConsolidation(Direction, Parent);

				var canCreateOrUpdateBookingConsolidation = CheckCanCreateOrUpdateBookingConsolidation(consolidation);
				var anyServiceCommenced = CheckAnyBookingsHaveServiceCommencedLogs(consolidation);
				var anyActiveBookings = CheckIfAnyTransportBookingsAreActive(consolidation);
				var isSub = CheckIfSub(consolidation);
				if ((canCreateOrUpdateBookingConsolidation && !anyServiceCommenced && !isSub) || !anyActiveBookings)
				{
					if (!ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, ShowBookingMessageHelper.MessageOperation.Create))
					{
						if (Parent != null)
						{
							var (isShouldShow, caption, message, confirmation) = Parent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
							if (isShouldShow && Globals.Message.ShowConfirmation(message, caption, confirmation, MessageBoxIcon.Warning) == DialogResult.Cancel)
							{
								return;
							}
						}

						var options = GetOptions();
						options.ShowAutoDelivery = false;

						var createBooking = ShowContainerSelectionOptions(options) == DialogResult.Yes && ShowTransportBookingCreateOptions(options) == DialogResult.Yes; // Populates options by side effect
						if (createBooking)
						{
							var publishedResult = CreateBookingConsolidation(options, overrideBookingConsolidation: true);
							if (publishedResult.ResultType != UniversalResult.HadErrors)
							{
								var view = options.DeliveryOptions == TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner
										? TransportBookingInstructionView.Standard
										: TransportBookingInstructionView.Instruction;
								var job = (DtbBookingConsolidation)publishedResult.FindJobIfExists();
								if (job != null)
								{
									job.KB_IsOverridden = false;
									ShowBooking(job, view, publishedResult.FindChildJobsIfExists().OfType<DtbBooking>().FirstOrDefault());
								}
							}
						}
					}
				}
				else
				{
					if (isSub)
					{
						var message = GetIsSubMessage(consolidation) +
							(canCreateOrUpdateBookingConsolidation ? string.Empty : System.Environment.NewLine + GetBookingConsolidationOverriddenMessage()) +
							(anyServiceCommenced ? System.Environment.NewLine + GetServiceCommencedMessage() : string.Empty) +
							System.Environment.NewLine + Res.GetString("2020295e-e92e-4aa1-98ef-45a296867b01", "Existing sub booking will now be displayed.");
						Globals.Message.ShowWarning(message);
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("DtbDeliveryManager|CannotCreateBooking",
			@"Changes may have been made on the job that effects the Transport Booking. These changes will not be reflected on the Transport Booking as the {0}.

		Booking changes must be made directly on the booking.", canCreateOrUpdateBookingConsolidation ? GetServiceCommencedMessage() : GetBookingConsolidationOverriddenMessage()));
					}

					ShowBooking(consolidation, DefaultBookingView);
				}
			}
		}

		public IEnumerable<IDtbBooking> DeliverTransportBooking()
		{
			// check license
			// check existing booking consolidation
			// if no existing || !overridden
			//   if fcl show container selection
			//   show options - template + Standard / Instruction (no auto deliver)
			//   create / update / override existing
			// else
			//   error, booking is overridden so ... need to be done from there
			//   show options - Standard / Instruction
			// end
			// if AutoDeliver - deliver created bookings
			// else show form (first booking created / standard/instruction)

			if (CheckIsSaved()) // do not check for license or security
			{
				return Parents.Length > 1 ? DeliverOrCreateMultiple(ShowBookingMessageHelper.MessageOperation.Deliver, skipCreateIfAnyParentsInactive: true) : DeliverSingle();
			}

			return Array.Empty<IDtbBooking>();
		}

		public IEnumerable<IDtbBooking> CreateTransportBookings(string defaultTemplate)
		{
			return CheckIsSaved() ? DeliverOrCreateMultiple(ShowBookingMessageHelper.MessageOperation.Create, skipCreateIfAnyParentsInactive: false, defaultTemplate) : Array.Empty<IDtbBooking>();
		}

		IEnumerable<IDtbBooking> DeliverOrCreateMultiple(ShowBookingMessageHelper.MessageOperation messageOperation, bool skipCreateIfAnyParentsInactive, string defaultTemplate = "")
		{
			var progressMax = Parents.Length * 2;
			OperationalActionSectionLogLoggerWrapper?.SetSectionProgressMax(progressMax);
			var allOptions = GetAllOptionsFromParents(out var result, messageOperation, skipCreateIfAnyParentsInactive, defaultTemplate);
			if (allOptions.Any())
			{
				// show options for the first only
				var firstOption = allOptions.First();
				PublishToUniversalResult firstResult = null;

				var dialogResult = ShowTransportBookingDocumentOptionsForm(firstOption);
				if (dialogResult == DialogResult.Yes)
				{
					// create for all options
					foreach (var option in allOptions)
					{
						var publishedResult = CreateBookingConsolidation(option, overrideBookingConsolidation: false, option.Parent, GetTopLevelDO(option.Parent));
						result.AddRange(publishedResult.FindChildJobsIfExists().OfType<IDtbBooking>());

						if (firstResult == null)
						{
							firstResult = publishedResult;
						}
					}

					if (firstResult.ResultType != UniversalResult.HadErrors)
					{
						DtbBookingConsolidation job = null;
						switch (firstOption.DeliveryOptions)
						{
							case TransportBookingDocumentOptions.DeliveryOption.AutoDelivery:
								break;

							case TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner:
								job = (DtbBookingConsolidation)firstResult.FindJobIfExists();
								if (job != null)
								{
									ShowBooking(job, TransportBookingInstructionView.Standard, firstResult.FindChildJobsIfExists().OfType<DtbBooking>().FirstOrDefault());
								}
								result = new List<IDtbBooking>();
								break;

							case TransportBookingDocumentOptions.DeliveryOption.OpenInstructionDesigner:
								job = (DtbBookingConsolidation)firstResult.FindJobIfExists();
								if (job != null)
								{
									ShowBooking(job, TransportBookingInstructionView.Instruction, firstResult.FindChildJobsIfExists().OfType<DtbBooking>().FirstOrDefault());
								}
								result = new List<IDtbBooking>();
								break;

							default:
								result = new List<IDtbBooking>();
								break;
						}
					}
				}
			}

			return result;
		}

		// Return a list of TransportBookingDocumentOptions, and also returns IDtbBooking objects in an out parameter list called "result"
		List<TransportBookingDocumentOptions> GetAllOptionsFromParents(out List<IDtbBooking> result, ShowBookingMessageHelper.MessageOperation messageOperation, bool skipCreateIfAnyParentsInactive, string defaultTemplate)
		{
			result = new List<IDtbBooking>();
			var allOptions = new List<TransportBookingDocumentOptions>();

			foreach (var parent in Parents)
			{
				OperationalActionSectionLogLoggerWrapper?.Log(LogType.Information, Res.GetString("afd1e708-1dd8-4de1-8365-6cab884ef7b7", "Checking {0}", parent.HumanReadableName));
				var consolidation = GetExistingBookingConsolidation(Direction, parent);
				var parentCancelled = ((ICancellable)parent).IsCancelled;
				if (parentCancelled)
				{
					if (consolidation != null && consolidation.ActiveBookings.Count > 0)
					{
						OperationalActionSectionLogLoggerWrapper?.Log(LogType.Warning, Res.GetString("f038ff4c-6688-4545-8f12-07f0cfe183af", "{0} is deactivated but has one or more active bookings", parent.HumanReadableName));
						LogUnchangedActiveBookingsAndAddToResult(consolidation, parent, result);
					}
					else
					{
						// we do not yet call ErrorManager.AddError() - would need to modify AutomatedDtbBookingCreationErrorManager to keep track of potentially multiple job errors
						var inactiveParentMessage = GetInactiveParentMessage(parent);
						if (SuppressDialogsAndUserInteractivity)
						{
							Logger?.Log(LogType.Error, inactiveParentMessage);
						}
						else
						{
							Globals.Message.ShowError(inactiveParentMessage);
						}

						if (skipCreateIfAnyParentsInactive)
						{
							allOptions.Clear();
							result.Clear();
							break;
						}
					}
					BumpSectionForSkippedCreation();
					continue;
				}

				var canCreateOrUpdateBookingConsolidation = CheckCanCreateOrUpdateBookingConsolidation(consolidation);
				var anyBookingsHaveServiceCommencedLogs = CheckAnyBookingsHaveServiceCommencedLogs(consolidation);
				var isSub = CheckIfSub(consolidation);

				if (isSub)
				{
					var firstSubBooking = consolidation.Bookings.FirstOrDefault(b => b.IsSub);
					var firstSubBookingJobID = firstSubBooking?.KM_JobID ?? string.Empty;
					var masterBookingJobID = firstSubBooking?.MasterBooking.KM_JobID ?? string.Empty;
					Logger?.Log(LogType.Warning, Res.GetString("eac287f3-f7b9-46f2-bc99-702c86f44550", "Booking {0} is a sub booking under Master Transport Booking {1} and cannot be overwritten", firstSubBookingJobID, masterBookingJobID));
				}

				if (!canCreateOrUpdateBookingConsolidation)
				{
					Logger?.Log(LogType.Warning, Res.GetString("81f703e5-abea-4ba0-93a3-afe71b78622d", "Booking Consolidation {0} for {1} is overridden and cannot be overwritten", consolidation.KB_JobID, parent.HumanReadableName));
				}

				if (anyBookingsHaveServiceCommencedLogs)
				{
					var firstActiveBookingCommenced = consolidation.ActiveBookings.First(b => b.HasServiceCommencedLogs);
					Logger?.Log(LogType.Warning, Res.GetString("b0309392-77a1-4fa8-88be-d2b259318c2b", "Booking Consolidation {0} for {1} has at least one service commenced booking {2} and cannot be overwritten", consolidation.KB_JobID, parent.HumanReadableName, firstActiveBookingCommenced.KM_JobID));
				}

				if (canCreateOrUpdateBookingConsolidation && !anyBookingsHaveServiceCommencedLogs && !isSub)
				{
					if (ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, OperationalActionSectionLogLoggerWrapper, messageOperation))
					{
						if (skipCreateIfAnyParentsInactive)
						{
							allOptions.Clear();
							result.Clear();
							break;
						}
						else
						{
							BumpSectionForSkippedCreation();
							continue;
						}
					}
					else
					{
						var topLevelDO = GetTopLevelDO(parent);
						var parentDO = Shipment.GetSourceDataObject(topLevelDO);
						var consolDO = DtbParentInfoLoader.GetConsolDO(topLevelDO, Direction);
						var options = GetOptions(parent, parentDO, consolDO, defaultTemplate);
						foreach (DtbDocumentContainerOption container in options.Containers)
						{
							container.DeliverContainer = SelectAllContainersFromOptions(options);
						}

						allOptions.Add(options);

						BumpSectionForPassedCheck();
					}
				}
				else
				{
					// we do not yet call ErrorManager.AddError() - would need to modify AutomatedDtbBookingCreationErrorManager to keep track of potentially multiple job errors
					if (consolidation.ActiveBookings.Any())
					{
						LogUnchangedActiveBookingsAndAddToResult(consolidation, parent, result);
					}

					BumpSectionForSkippedCreation();
				}
			}
			return allOptions;

			void BumpSectionForSkippedCreation()
			{
				OperationalActionSectionLogLoggerWrapper?.BumpSectionProgress();
				OperationalActionSectionLogLoggerWrapper?.BumpSectionProgress();
			}

			void BumpSectionForPassedCheck()
			{
				OperationalActionSectionLogLoggerWrapper?.BumpSectionProgress();
			}

			void LogUnchangedActiveBookingsAndAddToResult(DtbBookingConsolidation consolidation, IDtbBookingParent parent, List<IDtbBooking> result)
			{
				var activeBookingJobIDs = string.Join(", ", consolidation.ActiveBookings.Select(b => b.KM_JobID.ToString()).ToArray());
				Logger?.Log(LogType.Warning, Res.GetString("5aec0ca1-b739-4f21-b9c9-a694041389ec", "Bookings {0} under Booking Consolidation {1} for {2} have not been changed", activeBookingJobIDs, consolidation.KB_JobID, parent.HumanReadableName));
				result.AddRange(consolidation.ActiveBookings);
			}
		}

		IEnumerable<IDtbBooking> DeliverSingle()
		{
			var consolidation = GetExistingBookingConsolidation(Direction, Parent);
			var checkCanCreateOrUpdateBookingConsolidation = CheckCanCreateOrUpdateBookingConsolidation(consolidation);
			var anyServiceCommencedLogs = CheckAnyBookingsHaveServiceCommencedLogs(consolidation);
			var isSub = CheckIfSub(consolidation);
			var parentCancelled = ((ICancellable)Parent).IsCancelled;

			if (anyServiceCommencedLogs)
			{
				var serviceCommencedMessage = GetServiceCommencedMessage();
				Logger?.Error(GetFailedToCreateTransportBookingMessage() + System.Environment.NewLine + serviceCommencedMessage);
				ErrorManager?.AddError(DtbBookingCreationErrorType.ServiceHasCommencedError, serviceCommencedMessage);
			}

			if (!checkCanCreateOrUpdateBookingConsolidation)
			{
				var bookingConsolidationOverriddenMessage = GetBookingConsolidationOverriddenMessage();
				Logger?.Error(GetFailedToCreateTransportBookingMessage() + System.Environment.NewLine + bookingConsolidationOverriddenMessage);
				ErrorManager?.AddError(DtbBookingCreationErrorType.ConsolidationCannotBeOverriddenAgainError, bookingConsolidationOverriddenMessage);
			}

			if (isSub)
			{
				var isSubMessage = GetIsSubMessage(consolidation);
				Logger?.Error(GetFailedToCreateTransportBookingMessage() + System.Environment.NewLine + isSubMessage);
				ErrorManager?.AddError(DtbBookingCreationErrorType.ExistingBookingIsSubError, isSubMessage);
			}

			if (parentCancelled)
			{
				if (consolidation != null && consolidation.Bookings.Count > 0)
				{
					return consolidation.Bookings;
				}
				else
				{
					var message = GetInactiveParentMessage(Parent);
					Globals.Message.ShowError(message);
					Logger?.Error(GetFailedToCreateTransportBookingMessage() + System.Environment.NewLine + message);
					ErrorManager?.AddError(DtbBookingCreationErrorType.CancelledParentError, message);
					return Array.Empty<IDtbBooking>();
				}
			}

			if (checkCanCreateOrUpdateBookingConsolidation && !anyServiceCommencedLogs && !isSub)
			{
				if (!ShowBookingMessageHelper.ShowErrorIfBookingFormOpen(consolidation, ShowBookingMessageHelper.MessageOperation.Deliver))
				{
					var options = GetOptions();
					var result = ShowContainerSelectionOptions(options);
					if (result == DialogResult.Yes)
					{
						result = ShowTransportBookingDocumentOptionsForm(options);

						if (result == DialogResult.Yes)
						{
							var publishedResult = CreateBookingConsolidation(options, overrideBookingConsolidation: false);
							if (publishedResult.ResultType != UniversalResult.HadErrors)
							{
								return DeliverOrShowBooking(publishedResult, options);
							}
							else
							{
								return Array.Empty<IDtbBooking>();
							}
						}
					}
				}
			}
			else
			{
				var options = new TransportBookingDocumentOptions(Factory, Direction);
				options.ShowCommencedError = anyServiceCommencedLogs;

				var result = ShowTransportBookingDocumentOptionsForm(options);
				if (result == DialogResult.Yes)
				{
					switch (options.DeliveryOptions)
					{
						case TransportBookingDocumentOptions.DeliveryOption.AutoDelivery:
							return consolidation.Bookings;

						case TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner:
							ShowBooking(consolidation, TransportBookingInstructionView.Standard);
							return Array.Empty<IDtbBooking>();

						case TransportBookingDocumentOptions.DeliveryOption.OpenInstructionDesigner:
							ShowBooking(consolidation, TransportBookingInstructionView.Instruction);
							return Array.Empty<IDtbBooking>();

						default:
							return Array.Empty<IDtbBooking>();
					}
				}
			}
			return Array.Empty<IDtbBooking>();
		}

		bool CheckIsSaved()
		{
			var parentsNotSaved = IsUserInteractive ? Parents.Where(p => !p.IsInDatabase || p.HasChanges) : Parents.Where(p => !p.IsInDatabase);
			var isSaved = !parentsNotSaved.Any();
			if (!isSaved)
			{
				var notSaved = ZString.Join(", ", parentsNotSaved.Select(p => p.HumanReadableName).ToArray());
				if (IsUserInteractive)
				{
					Globals.Message.Show(Res.GetString("1b935b1e-0100-450a-91fa-8c71a64771ca", "Please save this {0} before creating a Transport Booking.", notSaved), Res.GetString("24ee2a4c-680f-4958-a8b7-751fbb68f9b8", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Cannot deliver, the parent object {0} is not in database.", notSaved));
					return false;
				}
			}

			return isSaved;
		}

		bool CheckCanCreateOrUpdateBookingConsolidation(DtbBookingConsolidation consolidation)
		{
			return consolidation == null || !consolidation.KB_IsOverridden;
		}

		bool CheckAnyBookingsHaveServiceCommencedLogs(DtbBookingConsolidation consolidation)
		{
			return consolidation != null && consolidation.ActiveBookings.Any(b => b.HasServiceCommencedLogs);
		}

		bool CheckIfAnyTransportBookingsAreActive(DtbBookingConsolidation consolidation)
		{
			return consolidation == null || consolidation.ActiveBookings.Any();
		}

		bool CheckIfSub(DtbBookingConsolidation consolidation)
		{
			return consolidation != null && consolidation.KB_KB_MasterBookingConsolidation != ZGuid.Empty;
		}

		string GetFirstSubTransportBookingMasterJobID(DtbBookingConsolidation consolidation)
		{
			var masterTransportBookingJobID = string.Empty;

			if (consolidation?.Bookings.FirstOrDefault(b => b.MasterBooking != null)?.MasterBooking is DtbBooking firstMasterBooking)
			{
				masterTransportBookingJobID = firstMasterBooking.KM_JobID;
			}

			return masterTransportBookingJobID;
		}

		DtbBookingConsolidation GetExistingBookingConsolidation(DtbBookingDirection direction, IDtbBookingParent parent)
		{
			return DtbBookingConsolidation.FindExistingTransportBookingConsolidation(new BusinessObjectFactory(), parent, direction);
		}

		TransportBookingDocumentOptions GetOptions()
		{
			return GetOptions(Parent, ParentDO, DtbParentInfoLoader.GetConsolDO(TopLevelDO, Direction));
		}

		TransportBookingDocumentOptions GetOptions(IDtbBookingParent parent, Shipment parentDO, Shipment consolDO, string defaultTemplate = "")
		{
			var transportMode = parentDO.TransportMode.GetCodeAsUpperCase();

			var containerMode = DtbDataObjectExtractor.GetContainerMode(parentDO);
			if (containerMode.IsEmpty)
			{
				containerMode = DtbDataObjectExtractor.GetContainerMode(consolDO);
			}

			var cfsDO = DtbDataObjectExtractor.GetAddressDO(parentDO, DocAddressType.LocalCartageCFS, Direction, null) ?? DtbDataObjectExtractor.GetAddressDO(consolDO, DocAddressType.LocalCartageCFS, Direction, null);

			var hasCFS = cfsDO != null;
			var parentBO = (BusinessObject)parent;
			var parentManager = (IShipmentDataContextManager)parentBO.GetUniversalDataContextManager();
			var parentDataContext = parentManager.DataContextType; // how to get from parentDO ?

			var options = new TransportBookingDocumentOptions(parent, Factory, parentDataContext, Direction, CombineContainers, transportMode, containerMode, hasCFS, defaultTemplate);
			PopulateOptionContainers(consolDO, parentDO, options);

			return options;
		}

		void PopulateOptionContainers(Shipment consolDO, Shipment parentDO, TransportBookingDocumentOptions options)
		{
			DataObjectList<Container> containerDOs;
			var shipmentContainers = parentDO.ContainerCollection;
			var consolContainers = consolDO.ContainerCollection.GetContainersReferencedBy(parentDO.PackingLineCollection);

			if (shipmentContainers != null && shipmentContainers.Any() && consolContainers.Any() && consolContainers.All(c => c?.Link != null) && shipmentContainers.All(c => c?.Link != null))
			{
				containerDOs = new DataObjectList<Container>();
				containerDOs.AddRange(shipmentContainers.Where(sc => consolContainers.Any(cc => cc.Link == sc.Link)));
			}
			else
			{
				containerDOs = shipmentContainers ?? consolContainers;
			}

			foreach (var container in containerDOs)
			{
				var containerNumber = container.ContainerNumber.GetValueOrDefault();
				var containerType = container.ContainerType.GetCodeAsUpperCase();
				var seal = container.Seal.GetValueOrDefault();
				var link = container.Link.GetValueOrDefault();
				var releaseNumber = container.ReleaseNum.GetValueOrDefault();

				options.Containers.Add(new DtbDocumentContainerOption(containerNumber, containerType, seal, link, releaseNumber));
			}
		}

		DialogResult ShowContainerSelectionOptions(TransportBookingDocumentOptions options)
		{
			var result = DialogResult.Yes;

			if (options.Containers.Count > 0)
			{
				if (SelectAllContainersFromOptions(options))
				{
					if (IsUserInteractive)
					{
						using (var form = new DtbDocumentContainerOptionsForm(options))
						{
							result = ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					}
				}
				else
				{
					foreach (DtbDocumentContainerOption container in options.Containers)
					{
						container.DeliverContainer = false;
					}
				}
			}

			return result;
		}

		DialogResult ShowTransportBookingCreateOptions(TransportBookingDocumentOptions options)
		{
			var result = DialogResult.Yes;

			if (IsUserInteractive)
			{
				using (var form = new TransportBookingCreateForm(options))
				{
					result = ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
			else
			{
				options.DeliveryOptions = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			}

			return result;
		}

		DialogResult ShowTransportBookingDocumentOptionsForm(TransportBookingDocumentOptions options)
		{
			var result = DialogResult.Yes;

			if (CanShowDialogs)
			{
				using (var form = new TransportBookingDocumentForm(options))
				{
					result = ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
			else
			{
				options.DeliveryOptions = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			}

			return result;
		}

		IEnumerable<IDtbBooking> DeliverOrShowBooking(PublishToUniversalResult result, TransportBookingDocumentOptions options)
		{
			DtbBookingConsolidation job = null;
			switch (options.DeliveryOptions)
			{
				case TransportBookingDocumentOptions.DeliveryOption.AutoDelivery:
					return result.FindChildJobsIfExists().OfType<IDtbBooking>().Select(b => Factory.Load<IDtbBooking>(b.PK));

				case TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner:
					job = (DtbBookingConsolidation)result.FindJobIfExists();
					if (job != null)
					{
						ShowBooking(job, TransportBookingInstructionView.Standard, result.FindChildJobsIfExists().OfType<DtbBooking>().FirstOrDefault());
					}
					return Array.Empty<IDtbBooking>();

				case TransportBookingDocumentOptions.DeliveryOption.OpenInstructionDesigner:
					job = (DtbBookingConsolidation)result.FindJobIfExists();
					if (job != null)
					{
						ShowBooking(job, TransportBookingInstructionView.Instruction, result.FindChildJobsIfExists().OfType<DtbBooking>().FirstOrDefault());
					}
					return Array.Empty<IDtbBooking>();

				default:
					return Array.Empty<IDtbBooking>();
			}
		}

		public ZBool ShowFormsFromMainThread { get; set; }

		readonly CurrentThreadBusinessObjectLoader currentThreadBusinessObjectLoader = new CurrentThreadBusinessObjectLoader();

		T GetOnCurrentThread<T>(T maybeUnsafeBizo) where T : BusinessObject
		{
			return (T)currentThreadBusinessObjectLoader.GetOnCurrentThread(maybeUnsafeBizo);
		}

		void HandleShowingFormSafely(Action showFormAction)
		{
			if (ShowFormsFromMainThread)
			{
				MainThreadRunner.RunOnMainThread(showFormAction);
			}
			else
			{
				showFormAction();
			}
		}

		void ShowBooking(DtbBookingConsolidation consolidation, TransportBookingInstructionView view, DtbBooking selectBooking = null)
		{
			Argument.NotNull(consolidation, nameof(consolidation));
			HandleShowingFormSafely(() => ShowBookingCore(
				GetOnCurrentThread(consolidation),
				view,
				GetOnCurrentThread(selectBooking)));
		}

		void ShowBookingCore(DtbBookingConsolidation consolidation, TransportBookingInstructionView view, DtbBooking selectBooking = null)
		{
			consolidation.Bookings.RefreshFromDb();
			if (consolidation.Bookings.Count == 1 || consolidation.ActiveBookings.Count == 1)
			{
				var bookingController = ZControllerFactory.Create(ControllerIDs.DtbBooking);
				SetLastControllerForTest(bookingController);
				var form = consolidation.ActiveBookings.Count == 1 ? bookingController.ShowEditForm(consolidation.ActiveBookings.Single()) : bookingController.ShowEditForm(consolidation.Bookings.First());
				if (form != null)
				{
					if (typeof(TransportBookingForm).IsAssignableFrom(form.GetType()))
					{
						var singleForm = (TransportBookingForm)form;
						singleForm.SetInstructionView(view);
					}
					else if (typeof(TransportBookingMultiForm).IsAssignableFrom(form.GetType()))
					{
						var multiForm = (TransportBookingMultiForm)form;
						if (selectBooking != null)
						{
							SelectBooking(selectBooking, multiForm);
						}

						multiForm.SetView(view);
					}
				}
			}
			else
			{
				var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
				SetLastControllerForTest(controller);
				var form = (TransportBookingMultiForm)controller.ShowEditForm(consolidation);
				if (form != null) // may not have had security clearance
				{
					SelectBooking(selectBooking, form);
					form.SetView(view);
				}
			}
		}

		void SelectBooking(DtbBooking selectBooking, TransportBookingMultiForm form)
		{
			if (!form.Visible)
			{
				form.Shown += (s, e) =>
				{
					form.SelectBooking(selectBooking);
				};
			}
			else
			{
				form.SelectBooking(selectBooking);
			}
		}

		PublishToUniversalResult CreateBookingConsolidation(TransportBookingDocumentOptions options, bool overrideBookingConsolidation)
		{
			return CreateBookingConsolidation(options, overrideBookingConsolidation, Parent, TopLevelDO);
		}

		PublishToUniversalResult CreateBookingConsolidation(TransportBookingDocumentOptions options, bool overrideBookingConsolidation, IDtbBookingParent parent, Shipment topLevelDO)
		{
			PublishToUniversalResult result;
			var orgHeader = GlbCompany.CurrentCompany.OrgProxy;
			DtbBookingCreationErrorType? errorType = null;
			string errorMessage = null;
			IEnumerable<DtbBooking> bookingsFromResult = null;

			if (orgHeader == null)
			{
				result = PublishToUniversalResult.New("", Res.GetString("44BC1445-C6AB-479F-9FC7-14619401A8F6", "Recipient Organization not set."));
			}
			else
			{
				try
				{
#if DEBUG
					ThrowSaveExceptionForTest();
					BeforeMainProcessing();
#endif

					IEnumerable<UniversalDataBuss.DataObjects.Universal.Event> universalEvents;
					using (Factory.AddDisposableService())
					{
						universalEvents = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, orgHeader, new RecipientRoleType[] { RecipientRoleType.CTG }, (IWorkflowProvider)parent, (outboundSessionTracker) => new DtbBookingParentDataObjectWriter(options, topLevelDO, outboundSessionTracker));
#if DEBUG
						// This is simulating what we are seeing in the issue. We have not managed to replicate it in CW1 but from the issue logs we know it is occurring,
						// a duplicate DIM (Data Import) event on the Transport Booking is occurring (seems like one per container in the TB).
						if (SimulateDuplicateTransportBookingDataImportEventsForTest)
						{
							var transportBookingDataImportEvent = universalEvents.FirstOrDefault(e => e.EventType.Value == "DIM" && e.DataContext.DataSourceCollection.Any(s => s.Type.Value == "TransportBooking"));
							universalEvents = universalEvents.Append(transportBookingDataImportEvent).ToArray();
						}
#endif
						Factory.Save();
					}

					result = PublishToUniversalResult.New(universalEvents, DataContextType.TransportBookingConsolidation, GetFailedToCreateTransportBookingMessage(), true, DataContextType.TransportBooking);

#if DEBUG
					AfterMainProcessing();
					DoInsertInstructionForTest(Factory, parent);
#endif

					var queryLocalCache = new ZQuery() { FetchOnlyFromLocalCache = true };
					var instructionsAlreadyInCache = Factory.Load<DtbBookingInstruction>(queryLocalCache);
					var instructionsAlreadyInCacheByPk = instructionsAlreadyInCache.ToDictionary(i => i.PK, i => (beforeSync: true, instruction: i));

					Factory.SynchroniseCachedBusinessObjectsWithDB<DtbBookingInstruction>(false, true);

					var instructionsInCacheFromSync = Factory.Load<DtbBookingInstruction>(queryLocalCache).Where(i => !instructionsAlreadyInCacheByPk.Keys.Contains(i.PK)).Select(i => (beforeSync: false, instruction: i)).ToArray();
					foreach (var entry in instructionsInCacheFromSync)
					{
						instructionsAlreadyInCacheByPk[entry.instruction.PK] = entry;
					}

					var reportPostSyncInstructionClash = false;
					var instructionsClashPostSync = new Dictionary<ZGuid, DtbBookingInstruction>();

#if DEBUG
					// Although it should be technically impossible for an insert to happen in another factory between the Factory.Load() above and the FindChildJobsIfExists() below ,
					// inserting here does seem to replicate the issue, so the unit test will run with the new flag (for inserting after the check) on
					// If/When we figure out what is happening, this code may be removed
					DoInsertInstructionAfterCheckForTest(Factory, parent);
#endif

					bookingsFromResult = result.FindChildJobsIfExists().OfType<DtbBooking>().GroupBy(b => b.PK).Select(b => b.First());
					var instructionsFromResult = bookingsFromResult.SelectMany(b => b.Instructions);
					foreach (var instruction in instructionsFromResult)
					{
						if (!instructionsAlreadyInCacheByPk.Keys.Contains(instruction.PK))
						{
							try
							{
								Factory.ImportFromAnotherFactory(instruction);
							}
							catch (ConstraintException ex) when (ex.Message.StartsWith(InstructionPKConstraintMessageStart, StringComparison.OrdinalIgnoreCase))
							{
								reportPostSyncInstructionClash = true;
								instructionsClashPostSync[instruction.PK] = instruction;
							}
						}
					}

					if (reportPostSyncInstructionClash)
					{
						ReportPostSyncImportClash(instructionsClashPostSync, parent, result, bookingsFromResult, instructionsFromResult, Factory);
					}
				}
				catch (UniversalEventDeliveryFailureException ex)
				{
					errorType = DtbBookingCreationErrorType.UniversalEventDeliveryFailure;
					errorMessage = ex.Message;
					result = PublishToUniversalResult.New("", Res.GetString("0912404a-23dd-43d4-b7af-095d086c1a1a", "Failed to create Transport Booking: {0}", ex.Message));
				}
				catch (ZSaveConcurrencyException)
				{
					errorType = DtbBookingCreationErrorType.ZSaveConcurrencyError;
					errorMessage = ConcurrencyErrorSavingTransportBooking;
					result = PublishToUniversalResult.New("", ConcurrencyErrorSavingTransportBooking);
				}
				catch (ZCannotSaveException ex)
				{
					errorType = DtbBookingCreationErrorType.ZCannotSaveError;
					errorMessage = ex.Message;
					result = PublishToUniversalResult.New("", Res.GetString("DtbDeliveryManager|CannotSaveTransportBooking", "Cannot save error occurred: {0}.", ex.Message));
				}
				catch (Exception ex) when (DtbBookingConsolidationDuplicateErrorHelper.IsSqlDuplicateBookingConsolidationException(ex))
				{
					errorType = DtbBookingCreationErrorType.DuplicateBookingConsolidationError;
					errorMessage = DuplicateBookingConsolidationError;
					result = PublishToUniversalResult.New("", DuplicateBookingConsolidationError);
				}

				OperationalActionSectionLogLoggerWrapper?.BumpSectionProgress();
			}

#if DEBUG
			SetUniversalResultErrorMessageForTest(ref result);
#endif

			if (result == null)
			{
				result = PublishToUniversalResult.New("", Res.GetString("19EC56A3-75F8-4E83-868F-029DE379301F", "Failed to Save Transport Booking due to Unknown Error"));
			}

			if (result.ResultType == UniversalResult.HadErrors)
			{
				var resultErrorMessage = result.ErrorMessage.ToString();
				if (!SuppressDialogsAndUserInteractivity)
				{
					Globals.Message.Show(resultErrorMessage);
				}
				Logger?.Log(LogType.Error, resultErrorMessage);
				ErrorManager?.AddError(errorType ?? DtbBookingCreationErrorType.UniversalResultError, errorMessage ?? resultErrorMessage);
			}
			else
			{
				LinkConsolidationToParent(overrideBookingConsolidation, parent, result);
				parent.TransportBookingCreatedOrUpdated(bookingsFromResult);
				if (OperationalActionSectionLogLoggerWrapper != null)
				{
					var bookingsFromResultJobIDs = string.Empty;
					var bookingsFromResultCount = bookingsFromResult.Count();
					if (bookingsFromResultCount == 0)
					{
						bookingsFromResultJobIDs = Res.GetString("b852f397-1a47-4916-849e-04a7c8444c1c", "No Transport Bookings");
					}
					else
					{
						bookingsFromResultJobIDs =
							(bookingsFromResult.Count() > 1 ? Res.GetString("40d63ca2-8cbc-4a08-be1a-2d7712f18c8e", "Transport Bookings") : Res.GetString("a8790d0b-ae9d-470e-83da-333bd9f2fb8d", "Transport Booking")) + " " +
								string.Join(", ", bookingsFromResult.Select(b => b.KM_JobID).ToArray());
					}
					OperationalActionSectionLogLoggerWrapper.Log(LogType.Information, Res.GetString("a41bd0d0-6965-4bde-9f65-7149054f457b", "{0} created for {1}", bookingsFromResultJobIDs, parent.HumanReadableName));
				}
			}

			return result;
		}

		void ReportPostSyncImportClash(
			Dictionary<ZGuid, DtbBookingInstruction> instructionsClashedPostSync,
			IDtbBookingParent parent,
			PublishToUniversalResult result,
			IEnumerable<DtbBooking> bookingsFromResult,
			IEnumerable<DtbBookingInstruction> instructionsFromResult,
			BusinessObjectFactory factory)
		{
#if DEBUG
			DoDeleteBookingForTest(instructionsClashedPostSync);
#endif

			var consolidation = (DtbBookingConsolidation)result.FindJobIfExists();
			var messageBuilder = new StringBuilder();
			messageBuilder.Append(FormattableString.Invariant($@"Was attempting to import into factory booking instruction(s) which were already in the database. Details below:
Parent: PK:{parent.PK}, TablePrefix:{parent.TablePrefix}, HumanReadableName:{parent.HumanReadableName}
Consolidation: PK:{consolidation.PK}, JobID:{consolidation.KB_JobID}
"));
			var instructionPksFromResult = instructionsFromResult.Select(i => i.PK);

			messageBuilder
				.Append((NoResString)"Bookings:")
				.AppendLine();
			foreach (var booking in bookingsFromResult)
			{
				messageBuilder
					.Append(FormattableString.Invariant($@"\tPK:{booking.PK}, JobID:{booking.KM_JobID}, Active:{booking.KM_IsActive}"))
					.AppendLine();
			}
			messageBuilder
				.AppendLine()
				.AppendLine();

			messageBuilder
				.Append((NoResString)"Instructions that we considered importing from another factory:")
				.AppendLine();
			foreach (var instruction in instructionsFromResult)
			{
				messageBuilder
					.Append(InstructionDescription(instruction, instructionsClashedPostSync.Keys))
					.AppendLine();
			}
			messageBuilder
				.AppendLine()
				.AppendLine();

			messageBuilder
				.Append("Instructions that could not be imported due to constraint error on KN_PK:")
				.AppendLine();
			foreach (var instruction in instructionsClashedPostSync.Values.Select(i => i))
			{
				messageBuilder
					.Append(InstructionDescription(instruction, Enumerable.Empty<ZGuid>()))
					.AppendLine();
			}
			messageBuilder
				.AppendLine()
				.AppendLine();

			messageBuilder
				.Append(FormattableString.Invariant($@"Factory has {factory.ChildFactories.Count} child factories"))
				.AppendLine();

			ErrorReporter.ReportOnce(InstructionClashExceptionKey, messageBuilder.ToString());
		}

		string InstructionDescription(IDtbBookingInstruction instruction, IEnumerable<ZGuid> instructionPKs)
		{
			if (((BusinessObject)instruction).IsDeleted)
			{
				return FormattableString.Invariant($@"\tPK:{instruction.PK}, ***DELETED***, ***DELETED***, {(instructionPKs.Contains(instruction.PK) ? "***CLASH***" : string.Empty)}");
			}
			else
			{
				return FormattableString.Invariant($@"\tPK:{instruction.PK}, BookingPK:{instruction.KN_KM_BookingMovement}, JobID:{instruction.KN_Sequence}, Type:{instruction.KN_InstructionType} {(instructionPKs.Contains(instruction.PK) ? "***CLASH***" : string.Empty)}");
			}
		}

		void LinkConsolidationToParent(bool overrideBookingConsolidation, IDtbBookingParent parent, PublishToUniversalResult result)
		{
			var consolidation = (DtbBookingConsolidation)result.FindJobIfExists();
			if (consolidation != null)
			{
				consolidation.KB_ParentTableCode = parent.BookingParentTablePrefix;
				consolidation.KB_ParentID = parent.BookingParentPK;
				consolidation.KB_IsOverridden = overrideBookingConsolidation;
				consolidation.Factory.Save();

				// Refresh booking collection on parent if neccessary
				var query = new ZQuery(DtbBookingConsolidationSchema.PK, consolidation.PK) { FetchOnlyFromLocalCache = true };
				var consolidationInParentsFactory = parent.Factory.LoadTop1<DtbBookingConsolidation>(query);
				if (consolidationInParentsFactory != null)
				{
					consolidationInParentsFactory.Bookings.RefreshFromDb(); // Universal imports in a separate factory with RefreshEnabled false
				}
			}
		}

		string ConcurrencyErrorSavingTransportBooking =>
			IsAutomated ? ConcurrencyErrorSavingTransportBookingAutomated : ConcurrencyErrorSavingTransportBookingInteractive;

		static string ConcurrencyErrorSavingTransportBookingInteractive => Res.GetString("DtbDeliveryManager|ConcurrencyErrorSavingTransportBooking", "Another user has made changes while you were working on this job. Re-open the form and try the action again.");
		static string ConcurrencyErrorSavingTransportBookingAutomated => Res.GetString("233b4039-8d30-4f33-9e0e-7092ab502bdd", "Another user has made changes while this job was being worked on.");

		string DuplicateBookingConsolidationError =>
			IsAutomated ? DuplicateBookingConsolidationErrorAutomated : DuplicateBookingConsolidationErrorInteractive;

		static string DuplicateBookingConsolidationErrorInteractive => Res.GetString("cfe49351-ae2f-479d-a2f2-301b22c80ecd", "Another user created a booking while we were attempting to create it, booking creation canceled. Re-open the form and try the action again.");
		static string DuplicateBookingConsolidationErrorAutomated => Res.GetString("58bce54b-8b9b-4326-b58e-e0368deec08d", "Another user created a booking while we were attempting to create it, booking creation canceled.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an English Caption as a constant")]
		public const string CreateEnglishCaption = "Create";
		public const string CreateEnglishCaptionWhenHasExistingBooking = "Create/Override";
		public static ResourceStringData CreateText => Res.GetData("DtbDeliveryManager|CreateText", CreateEnglishCaption);
		public static ResourceStringData CreateTextWhenHasExistingBooking => Res.GetData("DtbDeliveryManager|CreateTextWhenHasExistingBooking", CreateEnglishCaptionWhenHasExistingBooking);

		public const string MultiContainerEnglishCaption = "Multi-Container";
		public static ResourceStringData MultiContainerText => Res.GetData("DtbDeliveryManager|MultiContainerText", MultiContainerEnglishCaption);

		public static ResourceStringData ViewText => Res.GetData("DtbDeliveryManager|ViewText", "View/Edit");
		public static ResourceStringData ViewTextWhenParentCancelled => Res.GetData("DtbDeliveryManager|ViewTextWhenParentCancelled", "View");

		const string InstructionPKConstraintMessageStart = "Column 'KN_PK' is constrained to be unique.";
		const string InstructionClashExceptionKey = "DtbDeliveryManager_CreateBookingConsolidation_InstructionClash";

		static string GetFailedToCreateTransportBookingMessage() => Res.GetString("d8a5d16e-a08b-40df-9db1-e532dbd354a1", "Failed to Create Transport Booking:");

		static string GetServiceCommencedMessage() => Res.GetString("9ed799c1-6084-48ed-9b16-0630f7e35300", "Transport Company has commenced work on transport related to this job.");

		string GetIsSubMessage(DtbBookingConsolidation consolidation)
		{
			var masterBookingJobID = GetFirstSubTransportBookingMasterJobID(consolidation);
			return Res.GetString("81b3aba4-1f53-4d34-b520-2dacf64857c9", "Booking is a sub booking under Master Transport Booking {0} and cannot be overwritten.", masterBookingJobID);
		}

		static string GetBookingConsolidationOverriddenMessage() => Res.GetString("88554047-d245-4a9d-8bb7-00c3cee40b1f", "Booking has been overridden and is now managed independently of the job.");

		static string GetInactiveParentMessage(IDtbBookingParent parent) => Res.GetString("597a5335-fd6e-4366-a0ab-85d9a982b511", "{0} is deactivated and cannot create new Transport Bookings.", parent.HumanReadableName);

		public static void CreateTransportBooking(BusinessObjectFactory factory, IDtbBookingParent parent, DtbBookingDirection direction, bool combineContainers, Action<DtbDeliveryManager> beforeCreateTB = null, Func<bool> callback = null)
		{
			if (Env.Security.DtbBookingNew.IsAllowed)
			{
				if (callback != null && !callback())
				{
					return;
				}

				var deliveryManager = new DtbDeliveryManager(factory, parent, direction, combineContainers);
				beforeCreateTB?.Invoke(deliveryManager);
				deliveryManager.CreateTransportBooking();
			}
			else
			{
				Env.Security.ShowError(Env.Security.DtbBookingNew);
			}
		}

		public static TransportBookingInstructionView DefaultBookingView
		{
			get
			{
				switch (TransportRegistry.Instance.DefaultTransportBookingTabView.Value)
				{
					case BookingViews.Codes.Standard:
						return TransportBookingInstructionView.Standard;
					case BookingViews.Codes.Instruction:
						return TransportBookingInstructionView.Instruction;
					default:
						return TransportBookingInstructionView.Standard;
				}
			}
		}

		public static bool HasExistingBooking(DtbBookingConsolidation consolidation, bool combineContainers)
		{
			bool hasExistingBooking = false;

			if (consolidation != null)
			{
				hasExistingBooking = combineContainers
					? consolidation.Bookings.Any(b => b.AssignedPackages.IsCountMoreThan(1, p => p.IsContainer))
					: consolidation.Bookings.Any(b => b.AssignedPackages.IsCountLessThan(2, p => p.IsContainer));
			}

			return hasExistingBooking;
		}

#if DEBUG
		public bool SimulateDuplicateTransportBookingDataImportEventsForTest;
#endif

		partial void SetLastControllerForTest(ZController controller);
		partial void SetUniversalResultErrorMessageForTest(ref PublishToUniversalResult result);
		partial void ThrowSaveExceptionForTest();
		partial void DoInsertInstructionForTest(BusinessObjectFactory factory, IDtbBookingParent parent);

		partial void DoInsertInstructionAfterCheckForTest(BusinessObjectFactory factory, IDtbBookingParent parent);

		partial void DoDeleteBookingForTest(Dictionary<ZGuid, DtbBookingInstruction> instructionsClashedPostSync);

		partial void BeforeMainProcessing();
		partial void AfterMainProcessing();
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.GUI.Options.QueryProvider
{
	public enum DtbDeliveryManagerErrorsToTestFor
	{
		ZSaveConcurrencyException,
		ZCannotSaveException,
		SqlDuplicateConsolidationException,
		OtherZSaveException,
		UniversalEventDeliveryFailureException
	}

	public partial class DtbDeliveryManager
	{
		partial void SetLastControllerForTest(ZController controller)
		{
			LastControllerForTest = controller;
		}

		partial void SetUniversalResultErrorMessageForTest(ref PublishToUniversalResult result)
		{
			if (SetErrorMessageForTest)
			{
				result = PublishToUniversalResult.New("", "");
			}
			else if (SetResultNullForTest)
			{
				result = null;
			}
		}

		partial void ThrowSaveExceptionForTest()
		{
			if (TypeOfExceptionToThrowDuringSave == null)
			{
				return;
			}
			else if (TypeOfExceptionToThrowDuringSave == DtbDeliveryManagerErrorsToTestFor.ZSaveConcurrencyException)
			{
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Concurrency error during save"), null, null), Factory);
			}
			else if (TypeOfExceptionToThrowDuringSave == DtbDeliveryManagerErrorsToTestFor.ZCannotSaveException)
			{
				throw new ZCannotSaveException("Test Cannot Save Exception", "Cannot Save");
			}
			else if (TypeOfExceptionToThrowDuringSave == DtbDeliveryManagerErrorsToTestFor.SqlDuplicateConsolidationException)
			{
				throw DtbBookingConsolidationDuplicateErrorHelper.CreateSqlDuplicateBookingConsolidationException(Factory);
			}
			else if (TypeOfExceptionToThrowDuringSave == DtbDeliveryManagerErrorsToTestFor.OtherZSaveException)
			{
				throw new ZSaveException(new ZDataException(new InvalidOperationException("Error during save"), null, null), Factory);
			}
			else if (TypeOfExceptionToThrowDuringSave == DtbDeliveryManagerErrorsToTestFor.UniversalEventDeliveryFailureException)
			{
				throw new UniversalEventDeliveryFailureException(Array.Empty<UniversalDataBuss.DataObjects.Universal.Event>(), "Test Universal Event Delivery Failure");
			}
		}

		partial void BeforeMainProcessing()
		{
			SavesBeforeMainProcessing = FactorySaves;
		}

		partial void AfterMainProcessing()
		{
			SavesAfterMainProcessing = FactorySaves;
		}

		partial void DoInsertInstructionForTest(BusinessObjectFactory factory, IDtbBookingParent parent)
		{
			if (InsertInstructionForTest)
			{
				DoInsertInstructionForTestCore(factory, parent);
			}
		}

		partial void DoInsertInstructionAfterCheckForTest(BusinessObjectFactory factory, IDtbBookingParent parent)
		{
			if (InsertInstructionForTestAfterCheck)
			{
				DoInsertInstructionForTestCore(factory, parent);
			}
		}

		void DoInsertInstructionForTestCore(BusinessObjectFactory factory, IDtbBookingParent parent)
		{
			var consolidation = factory.LoadTop1<DtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, parent.PK));
			var booking = factory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_KB_Booking, consolidation.PK));
			var addedInstruction = factory.New<DtbBookingInstruction>();
			addedInstruction.KN_KM_BookingMovement = booking.PK;
			factory.Save();
		}

		partial void DoDeleteBookingForTest(Dictionary<ZGuid, DtbBookingInstruction> instructionsClashedPostSync)
		{
			if (DeleteBookingForTest)
			{
				DoDeleteBookingForTestCore(instructionsClashedPostSync);
			}
		}

		void DoDeleteBookingForTestCore(Dictionary<ZGuid, DtbBookingInstruction> instructionsClashedPostSync)
		{
			instructionsClashedPostSync.Values.First().Booking.Delete();
			Factory.Save();
		}

		public bool SetErrorMessageForTest;
		public bool SetResultNullForTest;
		public DtbDeliveryManagerErrorsToTestFor? TypeOfExceptionToThrowDuringSave;
		public bool InsertInstructionForTest { get; set; }
		public bool InsertInstructionForTestAfterCheck { get; set; }
		public bool DeleteBookingForTest { get; set; }
		public int FactorySaves { get; private set; }
		public int SavesBeforeMainProcessing { get; private set; }
		public int SavesAfterMainProcessing { get; private set; }
		public int SavesDuringMainProcessing
		{
			get
			{
				return SavesAfterMainProcessing - SavesBeforeMainProcessing;
			}
		}

		// This should be reduced by 1 when ShipmentDataContextManager.ReadIntoBusinessObject() removes the call to factory.SaveAtEndOfImport()
		// This should be reduced by 1 when DtbDeliveryManager removes save in CreateBookingConsolidation - WI00405675 - Improve CreateTransportBooking
		public const int ExpectedSavesDuringMainProcessing = 2;
		public ZController LastControllerForTest { get; set; }
		public ZBool CombineContainersForTest { get { return CombineContainers; } }

		public TransportBookingDocumentOptions GetOptionsForTest(IDtbBookingParent parent, Shipment parentDO, Shipment consolDO, string template = "")
		{
			return GetOptions(parent, parentDO, consolDO, template);
		}

		public TransportBookingDocumentOptions GetOptionsForTest()
		{
			return GetOptions();
		}

		public void SelectBookingForTest(DtbBooking selectBooking, TransportBookingMultiForm form)
		{
			SelectBooking(selectBooking, form);
		}

		public void TurnOnSaveCounting()
		{
			BusinessObjectFactory.SetOnFactorySaveHookForTest((_) =>
			{
				FactorySaves++;
			});
		}
	}
}

#endif
