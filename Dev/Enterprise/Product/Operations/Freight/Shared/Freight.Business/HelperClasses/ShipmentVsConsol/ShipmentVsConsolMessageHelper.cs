using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Business.ShipmentVsConsolHelper;
using static Enterprise.Freight.Integration.Forwarding;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Business
{
	public abstract class ShipmentVsConsolMessageHelper : IShipmentVsConsolMessageHelper
	{
		protected ShipmentVsConsolMessageHelper()
		{
		}

		#region Consol -> Shipments

		protected virtual IEnumerable<string> DisabledChecks => Array.Empty<string>();

		public bool IsAllowedToAddNewShipment(out string message, CommonConsol parentConsol)
		{
			var shipments = CreateEnumerable<CommonShipment>();
			var consols = CreateEnumerable(parentConsol);

			message = RunAttachDetachChecks(AttachDetachAction.New, shipments, consols, parentConsol);
			return string.IsNullOrEmpty(message);
		}

		public IShipmentConsolAttachRequest IsAllowedToAttachShipment(CommonConsol parentConsol, CommonShipment shipment)
		{
			var consols = CreateEnumerable(parentConsol);
			var shipments = CreateEnumerable(shipment);

			Func<ZString> errors = () => RunAttachDetachChecks(AttachDetachAction.Attach, shipments, consols, parentConsol);
			Func<ZString> warnings = () =>
			{
				if (shipment != null)
				{
					var checkRelatedReceivingAgents = CheckRelatedReceivingAgents(shipments, consols);
					if (!string.IsNullOrEmpty(checkRelatedReceivingAgents))
					{
						return checkRelatedReceivingAgents;
					}

					var checkRelatedSendingAgents = CheckRelatedSendingAgents(shipments, consols);
					if (!string.IsNullOrEmpty(checkRelatedSendingAgents))
					{
						return checkRelatedSendingAgents;
					}

					var checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge = CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge(AttachDetachAction.Attach, shipments, consols, parentConsol, isWarning: true);
					if (!string.IsNullOrEmpty(checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge))
					{
						return checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge;
					}

					if (parentConsol.IsDirect && shipment.IsAssemblyMaster)
					{
						return FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster;
					}

					if (parentConsol.IsAgent && shipment.IsDirectShipment)
					{
						if (ShipmentVsConsolHelper.AllowAttachingDirectShipmentsToAgentConsols(shipments, consols))
						{
							return GetMessage_AttachDirectShipmentsToAgentConsols(shipments);
						}
					}
				}

				return string.Empty;
			};

			return new ShipmentConsolAttachRequest(errors, warnings);
		}

		public IShipmentConsolDetachRequest IsAllowedToDetachShipments(CommonConsol parentConsol, IEnumerable<CommonShipment> shipments)
		{
			var consols = CreateEnumerable(parentConsol);
			string message = RunAttachDetachChecks(AttachDetachAction.Detach, shipments, consols, parentConsol);

			Func<ZString> cutOffDatePassedMessageGetter = () =>
			{
				if (Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed && ConsolHasPastCutOffDate(parentConsol))
				{
					return Res.GetString("4fa1c781-7230-4b69-8c19-590178f81b5a",
								"The Cut Off Date has passed for the {0}. Are you sure that you want to detach the selected {1}?",
								NameAndList(new[] { parentConsol }),
								NameAndList(shipments));
				}

				return string.Empty;
			};

			Func<ZString> detachSubShipmentsMessageGetter = () =>
			{
				return GetMessage_DetachSubShipmentsFromConsols(shipments, consols, parentConsol);
			};

			var exportNotification755MessageGetter = () =>
			{
				return GetDetachMessage_ExportNotification755Message(shipments, consols);
			};

			return new ShipmentConsolDetachRequest(message, cutOffDatePassedMessageGetter, detachSubShipmentsMessageGetter, exportNotification755MessageGetter);
		}

		#endregion

		#region Shipment -> Consols

		public bool IsAllowedToAddNewConsol(out string message, CommonShipment parentShipment)
		{
			var shipments = CreateEnumerable(parentShipment);
			var consols = CreateEnumerable<CommonConsol>();

			message = RunAttachDetachChecks(AttachDetachAction.New, shipments, consols, parentShipment);
			return string.IsNullOrEmpty(message);
		}

		public IShipmentConsolAttachRequest IsAllowedToAttachConsol(CommonShipment parentShipment, CommonConsol consol)
		{
			var shipments = CreateEnumerable(parentShipment);
			var consols = CreateEnumerable(consol);

			Func<ZString> errors = () => RunAttachDetachChecks(AttachDetachAction.Attach, shipments, consols, parentShipment);
			Func<ZString> warnings = () =>
			{
				if (consol != null)
				{
					var checkRelatedReceivingAgents = CheckRelatedReceivingAgents(shipments, consols);
					if (!string.IsNullOrEmpty(checkRelatedReceivingAgents))
					{
						return checkRelatedReceivingAgents;
					}

					var checkRelatedSendingAgents = CheckRelatedSendingAgents(shipments, consols);
					if (!string.IsNullOrEmpty(checkRelatedSendingAgents))
					{
						return checkRelatedSendingAgents;
					}

					var checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge = CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge(AttachDetachAction.Attach, shipments, consols, parentShipment, isWarning: true);
					if (!string.IsNullOrEmpty(checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge))
					{
						return checkShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge;
					}

					if (consol.IsDirect && parentShipment.IsAssemblyMaster)
					{
						return FreightConstants.ASMShipmentMessages.AssemblyMasterAsDirectMaster;
					}

					if (consol.IsAgent && parentShipment.IsDirectShipment)
					{
						if (ShipmentVsConsolHelper.AllowAttachingDirectShipmentsToAgentConsols(shipments, consols))
						{
							return GetMessage_AttachDirectShipmentsToAgentConsols(shipments);
						}
					}
				}

				return string.Empty;
			};

			return new ShipmentConsolAttachRequest(errors, warnings);
		}

		public IShipmentConsolDetachRequest IsAllowedToDetachConsols(CommonShipment parentShipment, IEnumerable<CommonConsol> consols)
		{
			var shipments = CreateEnumerable(parentShipment);
			string message = RunAttachDetachChecks(AttachDetachAction.Detach, shipments, consols, parentShipment);

			Func<ZString> cutOffDatePassedMessageGetter = () =>
			{
				if (Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed)
				{
					var pastCutOffDateConsols = consols.Where(consol => ConsolHasPastCutOffDate(consol));
					if (pastCutOffDateConsols.Any())
					{
						return Res.GetString("36e3e324-0ca1-41be-b8ea-6ab6bc94f666",
									"The Cut Off Date has passed for the {0}. Are you sure that you want to detach the {1} from the {2}?",
									NameAndList(pastCutOffDateConsols),
									LowerCaseName(pastCutOffDateConsols),
									NameAndList(new[] { parentShipment }));
					}
				}

				return string.Empty;
			};

			Func<ZString> detachSubShipmentsMessageGetter = () =>
			{
				return GetMessage_DetachSubShipmentsFromConsols(shipments, consols, parentShipment);
			};

			var exportNotification755MessageGetter = () =>
			{
				return GetDetachMessage_ExportNotification755Message(shipments, consols);
			};

			return new ShipmentConsolDetachRequest(message, cutOffDatePassedMessageGetter, detachSubShipmentsMessageGetter, exportNotification755MessageGetter);
		}

		#endregion

		#region Sub Shipments

		public bool IsAllowedToAddNewSubShipment(out string message, CommonShipment master)
		{
			message = string.Empty;
			if (master != null)
			{
				bool isDirectCheckSuppressed = master.IsAssemblyMaster && master.Consols.Any(c => ((CommonConsol)c).IsDirect);
				using (SuppressCheckIsDirect(isDirectCheckSuppressed))
				{
					message = RunAttachDetachChecksForSubShipment(AttachDetachAction.New, master, null);
				}
			}

			return string.IsNullOrEmpty(message);
		}

		public bool IsAllowedToAttachSubShipment(out string message, CommonShipment master, CommonShipment sub)
		{
			message = string.Empty;
			if (master != null)
			{
				bool isDirectCheckSuppressed = master.IsAssemblyMaster && (sub == null || sub.IsStandardHouse) && master.Consols.Any(c => ((CommonConsol)c).IsDirect);
				using (SuppressCheckIsDirect(isDirectCheckSuppressed))
				{
					message = RunAttachDetachChecksForSubShipment(AttachDetachAction.Attach, master, sub);
				}
			}

			return string.IsNullOrEmpty(message);
		}

		IDisposable SuppressCheckIsDirect(bool isDirectCheckSuppressed)
		{
			isCheckDirectSuppressed = isDirectCheckSuppressed;
			return new DisposableAction(() => isCheckDirectSuppressed = false);
		}

		bool isCheckDirectSuppressed;

		public CommonConsol[] GetConsolsToDetachFromSubShipments(out string message, CommonConsol parentConsol, CommonShipment oldMaster, CommonShipment newMaster, IEnumerable<CommonShipment> subShipments)
		{
			message = string.Empty;

			var consolsToDetach = GetConsolsToDetach(parentConsol, oldMaster, newMaster);
			if (subShipments != null && subShipments.Any() && consolsToDetach.Any())
			{
				var allSubShipments = new List<CommonShipment>();
				foreach (CommonShipment subShipment in subShipments)
				{
					GetSubShipments(subShipment, allSubShipments);
				}

				var allConsolsInSubShipments = new HashSet<CommonConsol>();
				foreach (var subShipment in allSubShipments)
				{
					foreach (CommonConsol consol in subShipment.Consols)
					{
						allConsolsInSubShipments.Add(consol);
					}
				}

				consolsToDetach = consolsToDetach.Intersect(allConsolsInSubShipments).ToList();

				message = Res.GetString("0b6bb598-9f70-4916-a345-ad8dbf0e05cf",
					"Do you also want to detach the {0} from the {1}?",
					NameAndList(consolsToDetach),
					NameAndList(allSubShipments));
			}

			return consolsToDetach.ToArray();
		}

		#endregion

		#region DetachShipmentsFromConsols

		public void DetachShipmentsFromConsols(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			if (shipments != null && consols != null)
			{
				foreach (CommonConsol consol in consols.Where(item => item != null))
				{
					using (consol.Shipments.SuspendListChanged())
					{
						using (consol.SuspendShipmentsCountChanged())
						{
							var shipmentsToDetach = new List<CommonShipment>();
							GetLinearListOfShipmentsToDetach(shipmentsToDetach, consol, shipments);
							foreach (CommonShipment shipment in shipmentsToDetach)
							{
								consol.Shipments.Remove(shipment);
							}
						}
					}
				}
			}
		}

		public string CheckRelatedReceivingAgents(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var receivingAgentShipmentList = new List<CommonShipment>();
			var receivingAgentConsolList = new List<CommonConsol>();

			foreach (CommonShipment shipment in shipments.Where(item => item != null))
			{
				foreach (CommonConsol consol in consols.Where(item => item != null))
				{
					if (!shipment.HasRelatedReceivingAgent(consol))
					{
						receivingAgentShipmentList.AddIfNotContains(shipment);
						receivingAgentConsolList.AddIfNotContains(consol);
					}
				}
			}

			if (receivingAgentShipmentList.Any())
			{
				return Res.GetString("085bc66a-8e20-4189-be6d-bafc646f692f", "The Consignee/Consignor Related Receiving Agent on {0} does not match the Receiving Forwarder on {1}.",
									 NameAndList(receivingAgentShipmentList),
									 NameAndList(receivingAgentConsolList));
			}

			return string.Empty;
		}

		public string CheckRelatedSendingAgents(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var sendingAgentShipmentList = new List<CommonShipment>();
			var sendingAgentConsolList = new List<CommonConsol>();

			foreach (CommonShipment shipment in shipments.Where(item => item != null))
			{
				foreach (CommonConsol consol in consols.Where(item => item != null))
				{
					if (!shipment.HasRelatedSendingAgent(consol))
					{
						sendingAgentShipmentList.AddIfNotContains(shipment);
						sendingAgentConsolList.AddIfNotContains(consol);
					}
				}
			}

			if (sendingAgentShipmentList.Any())
			{
				return Res.GetString("ef92261c-9d15-4626-b418-43e5ca7f0422", "The Consignee/Consignor Related Sending Agent on {0} does not match the Sending Forwarder on {1}.",
									 NameAndList(sendingAgentShipmentList),
									 NameAndList(sendingAgentConsolList));
			}

			return string.Empty;
		}

		void GetLinearListOfShipmentsToDetach(List<CommonShipment> result, CommonConsol consol, IEnumerable<CommonShipment> shipmentsToAdd)
		{
			foreach (CommonShipment shipment in shipmentsToAdd.Where(item => item != null))
			{
				if (consol.Shipments.Contains(shipment) && !result.Contains(shipment))
				{
					result.Add(shipment);
					GetLinearListOfShipmentsToDetach(result, consol, shipment.CoLoadShipments.Cast<CommonShipment>());
				}
			}
		}

		public IReadOnlyCollection<string> CheckDatesWithinRange(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var messages = new List<string>();
			foreach (var consol in consols?.Where(consol => consol.JK_JX_JB_E_ARV.IsValid || consol.JK_JX_JA_E_DEP.IsValid) ?? Enumerable.Empty<CommonConsol>())
			{
				foreach (var shipment in shipments ?? Enumerable.Empty<CommonShipment>())
				{
					if (IsETAOutsideRange(shipment, consol))
					{
						messages.Add(Res.GetString("fb1d866b-4b5f-4091-967a-ea76116f4b6c", "Shipment {0} has estimated arrival date before Consol {1} ETA.", shipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef));
					}

					if (IsETDOutsideRange(shipment, consol))
					{
						messages.Add(Res.GetString("4f20599e-c11c-4c70-93ba-d0122617c67f", "Shipment {0} has estimated departure date after Consol {1} ETD.", shipment.JS_UniqueConsignRef, consol.JK_UniqueConsignRef));
					}
				}
			}

			return messages;
		}

		public IReadOnlyCollection<string> CheckShipmentEstimatedDeliveryIsAfterConsolArrival(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var messages = new List<string>();
			foreach (var consol in consols?.Where(consol => consol.JK_JX_JB_E_ARV.IsValid) ?? Enumerable.Empty<CommonConsol>())
			{
				shipments?.Where(shipment => IsShipmentEstimatedDeliveryBeforeConsolEstimatedArrival(shipment, consol)).ForEach(shipment => messages.Add(Res.GetString("ec517553-2ab9-4849-a8a2-8f6a18d05628", "Shipment {0}", shipment.JS_UniqueConsignRef)));
			}

			return messages;
		}

		static bool IsETAOutsideRange(CommonShipment shipment, CommonConsol consol)
		{
			return consol.JK_JX_JB_E_ARV.IsValid && shipment.JS_E_ARV.IsValid && shipment.JS_E_ARV < consol.JK_JX_JB_E_ARV;
		}

		static bool IsETDOutsideRange(CommonShipment shipment, CommonConsol consol)
		{
			return consol.JK_JX_JA_E_DEP.IsValid && shipment.JS_E_DEP.IsValid && shipment.JS_E_DEP > consol.JK_JX_JA_E_DEP;
		}

		static bool IsShipmentEstimatedDeliveryBeforeConsolEstimatedArrival(CommonShipment shipment, CommonConsol consol)
		{
			return consol.JK_JX_JB_E_ARV.IsValid &&
				 shipment.DocsAndCartage.JP_EstimatedDelivery.IsValid &&
				 shipment.DocsAndCartage.JP_EstimatedDelivery < consol.JK_JX_JB_E_ARV;
		}

		#endregion

		#region Implementation

		#region Main Check Runners

		protected delegate string AttachDetachCheck(AttachDetachAction action, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent);

		IEnumerable<AttachDetachCheck> GetAttachDetachChecks(AttachDetachAction detachAttachAction)
		{
			var checkList = new List<AttachDetachCheck>();

			checkList.Add(CheckPastCutOffDate);

			switch (detachAttachAction)
			{
				case AttachDetachAction.New:
					checkList.Add(CheckIsDirectConsol);
					checkList.Add(CheckIsMultiAWBMasterConsol);
					checkList.Add(CheckForbiddenDGShipmentCannotAttachToNotCargoOnlyConsol);
					break;
				case AttachDetachAction.Attach:
					checkList.Add(CheckConsolTypeAccessRights);
					checkList.Add(CheckInactiveConsol);
					checkList.Add(CheckIsDirectShipment);
					checkList.Add(CheckIsDirectConsol);
					checkList.Add(CheckIsMultiAWBMasterConsol);
					checkList.Add(CheckShipmentCanHaveDirectConsol);
					checkList.Add(CheckIsAllowedToAttachShipmentWithApprovedForCargoOnlyInspectionType);
					checkList.Add(CheckOrganisationsAreApprovedForShippingOnPassengerFlights);
					checkList.Add(CheckUserRightsToAttachShipmentWithDifferentGatewayServiceLevel);
					checkList.Add(CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge);
					checkList.Add(CheckShipmentPacklinesMatchConsolTemperatureControl);
					checkList.Add(CheckConsolAllowsAttachingOfShipmentsDangerousGoods);
					checkList.Add(CheckConsolAllowsAttachingShipmentWithLithiumBatteries);
					checkList.Add(CheckForbiddenDGShipmentCannotAttachToNotCargoOnlyConsol);
					checkList.Add(CheckUNDGPermissableQuantities);
					checkList.Add(CheckConsolAllowsAttachingShipmentWithMaximumAllowablePacklineAndContainerDimensions);
					checkList.Add(CheckShipmentAndConsolBothHaveSentBookingRequestMessage);
					break;
				case AttachDetachAction.Detach:
					checkList.Add(CheckConsolTypeAccessRights);
					checkList.Add(CheckShipmentHasJobAndChanges);
					checkList.Add(CheckHasConsolApportionmentOnShipment);
					checkList.Add(CheckHasShipmentChargesOnCollectInvoice);
					checkList.Add(CheckShipmentHasMasterOnSameConsol);
					checkList.Add(CheckShipmentConsolPivotCanBeDeleted);
					checkList.Add(CheckStandAloneShipmentCannotDetachConsol);
					checkList.Add(PreventDetachingShipmentWhenCCTHouseManifestHasBeenSent);
					break;
			}

			checkList.AddRange(GetCustomAttachDetachChecks());

			return checkList;
		}

		IEnumerable<AttachDetachCheck> GetCustomAttachDetachChecks()
		{
			var types = (ArrayList)ObjectFactory.Get("ShipmentAttachDetachChecks");
			foreach (IShipmentAttachDetachCheck checker in types)
			{
				yield return (AttachDetachAction action, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent) =>
				{
					return checker.Check(action, shipments, consols, parent, NameAndList);
				};
			}
		}

		string RunAttachDetachChecks(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (parent == null)
			{
				throw new NullReferenceException("Parent cannot be NULL");
			}

			var checkList = GetAttachDetachChecks(userAction);

			string result = string.Empty;
			foreach (var checkMethod in checkList)
			{
				if (DisabledChecks != null && DisabledChecks.Contains(checkMethod.Method.Name))
				{
					continue;
				}

				result = checkMethod(userAction, shipments, consols, parent);
				if (!string.IsNullOrEmpty(result))
				{
					break;
				}
			}

			return result;
		}

		#region Runner For SubShipments

		string RunAttachDetachChecksForSubShipment(AttachDetachAction userAction, CommonShipment master, CommonShipment sub)
		{
			var consols = new List<BusinessObject>();
			switch (userAction)
			{
				case AttachDetachAction.New:
				case AttachDetachAction.Attach:
					foreach (var consol in master.Consols)
					{
						bool masterConsolShouldBeChecked = sub == null || !sub.Consols.Any(subConsol => subConsol.PK == consol.PK);
						if (masterConsolShouldBeChecked)
						{
							consols.Add(consol);
						}
					}

					break;
			}

			foreach (CommonConsol consol in consols.Where(item => item != null))
			{
				string message = string.Empty;

				switch (userAction)
				{
					case AttachDetachAction.New:
						IsAllowedToAddNewShipment(out message, consol);
						break;
					case AttachDetachAction.Attach:
						var isAllowedToAttachShipment = IsAllowedToAttachShipment(consol, sub);
						if (!isAllowedToAttachShipment.Errors.IsEmpty)
						{
							message = isAllowedToAttachShipment.Errors;
						}
						break;
				}

				if (!string.IsNullOrEmpty(message))
				{
					string subShipmentMessage = GetMessage_AttachSubShipmentToConsol(userAction, master, sub);
					message = string.Join(System.Environment.NewLine, new string[] { subShipmentMessage, message });

					return message;
				}
			}

			if (master.JS_ShipmentType == Core.Constants.ShipmentTypes.AssemblyMaster && MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(master, (NoResString)"Advanced Cargo Report") && MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(sub, (NoResString)"Advanced Cargo Report"))
			{
				return Res.GetString("5a6507cd-918a-4d06-8c93-db00f05a98ff", "Advanced Air Cargo Reporting has been sent from this shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from this shipment prior to attaching it to Assembly master.");
			}
			return string.Empty;
		}

		string GetMessage_AttachSubShipmentToConsol(AttachDetachAction userAction, CommonShipment master, CommonShipment sub)
		{
			string result = String.Empty;

			if (userAction == AttachDetachAction.New)
			{
				if (master != null)
				{
					result = Res.GetString("9f9a16e7-7b39-4df7-bd07-4cb879cbe529",
						"New sub-shipment cannot be added to the master-shipment {0} as the master has a {1} that cannot be attached to a new sub-shipment.",
						master.JS_UniqueConsignRef,
						this.ConsolName_SingularLower);
				}
			}
			else if (userAction == AttachDetachAction.Attach)
			{
				if (sub == null)
				{
					result = Res.GetString("af79cabf-4830-49aa-870a-fdf846aac603",
						"Sub-shipment cannot be attached to the master-shipment {0} as the master has a {1} that cannot be attached to a sub-shipment.",
						master.JS_UniqueConsignRef,
						this.ConsolName_SingularLower);
				}
				else if (master != null)
				{
					result = Res.GetString("db2cbafe-1df1-4138-aea6-f7c2b1105099",
						"The sub-shipment {0} cannot be attached to the master-shipment {1} as the master has a {2} that cannot be attached to the sub-shipment.",
						sub.JS_UniqueConsignRef,
						master.JS_UniqueConsignRef,
						this.ConsolName_SingularLower);
				}
			}

			if (String.IsNullOrEmpty(result))
			{
				throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
			}

			return result;
		}

		#endregion

		#endregion

		#region Check Methods

		public static class CheckMethodNames
		{
			public const string CheckShipmentHasJobAndChanges = nameof(ShipmentVsConsolMessageHelper.CheckShipmentHasJobAndChanges);
		}

		#region CheckConsolTypeAccessRights

		string CheckConsolTypeAccessRights(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (userAction == AttachDetachAction.Attach || userAction == AttachDetachAction.Detach)
			{
				var groups = consols
					.GroupBy(x => x.JK_AgentType)
					.Where(x => !x.First().Validation.GetConsolTypeSecurityCheckpoint().IsAllowed)
					.ToArray();

				if (groups.Any())
				{
					var errors = groups
						.Select(x =>
							string.Join(", ", x.Select(c => c.HumanReadableName)) +
							System.Environment.NewLine +
							x.First().Validation.GetConsolTypeSecurityCheckpoint().ErrorMessageForNotAllowed);

					return Res.GetString("28e7d5ec-4f1b-47da-afd3-57af8262f0ad",
							"Cannot {0}{1}{2}",
							userAction,
							System.Environment.NewLine,
							string.Join(System.Environment.NewLine, errors));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckPastCutOffDate

		string CheckPastCutOffDate(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (!Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed)
			{
				var pastCutOffDateConsols = consols.Where(ConsolHasPastCutOffDate);
				if (pastCutOffDateConsols.Any())
				{
					switch (userAction)
					{
						case AttachDetachAction.New:
							return Res.GetString("3d59385a-7eb1-4401-b78d-6552374cb943",
								"Cannot add a new shipment to the {0} as the consol's Cut Off Date has passed.\r\n\r\n{1}",
								NameAndList(pastCutOffDateConsols),
								Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate));

						case AttachDetachAction.Attach:
							return (parent is CommonShipment)
								? Res.GetString("c38afabb-bc0a-4df7-a8d4-9db4b6ef9b20",
										"Cannot attach the {0} to the {1} as the {2} Cut Off Date has passed.\r\n\r\n{3}",
										NameAndList(pastCutOffDateConsols),
										NameAndList(shipments),
										LowerCaseName(pastCutOffDateConsols),
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate))
								: Res.GetString("ec366d09-9ba3-4247-bf63-ad9d102780bf",
										"Cannot attach {0} to the {1} as the consol's Cut Off Date has passed.\r\n\r\n{2}",
										shipments.Any() ? NameAndList(shipments) : Res.GetString("0a480aca-31db-407d-b03b-49ab8011036a", "any shipment"),
										NameAndList(pastCutOffDateConsols),
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate));

						case AttachDetachAction.Detach:
							return (parent is CommonShipment)
								? Res.GetString("19407936-2e90-4dc1-8043-fbb898fe869d",
										"Cannot detach the {0} from the {1} as the consol's Cut Off Date has passed.\r\n\r\n{2}",
										NameAndList(pastCutOffDateConsols),
										NameAndList(shipments),
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate), System.Environment.NewLine)
								: Res.GetString("0d6779e7-2f62-4c62-a7b8-dff340f73a0c",
										"Cannot detach the {0} from the {1} as the consol's Cut Off Date has passed.\r\n\r\n{2}",
										NameAndList(shipments),
										NameAndList(pastCutOffDateConsols),
										Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate), System.Environment.NewLine);

						default:
							throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
					}
				}
			}

			return string.Empty;
		}

		bool ConsolHasPastCutOffDate(CommonConsol consol)
		{
			return consol != null && consol.JK_ConsolCutOffDate.IsValid && consol.JK_ConsolCutOffDate < ZDateTime.UtcNow && consol.IsInDatabase;
		}

		#endregion

		#region CheckIsDirectShipment

		string CheckIsDirectShipment(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (CommonShipment shipment in shipments.Where(item => item != null && item.IsDirectShipment))
			{
				foreach (CommonConsol consol in consols.Where(item => item != null && !item.IsDirect))
				{
					shipmentsList.AddIfNotContains(shipment);
					consolsList.AddIfNotContains(consol);
				}
			}

			if (shipmentsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return !ShipmentVsConsolHelper.AllowAttachingDirectShipmentsToAgentConsols(shipmentsList, consolsList)
							? GetMessage_AttachDirectShipmentsToAgentConsols(shipmentsList)
							: string.Empty;
					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckIsDirectConsol

		string CheckIsDirectConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (isCheckDirectSuppressed)
			{
				return string.Empty;
			}

			var directConsols = consols.Where(consol => consol != null
				&& consol.IsDirect
				&& consol.Shipments.Any()
				&& shipments.All(s => !CanShipmentBeAddedToDirectConsolWithoutError(consol, s)));

			if (directConsols.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.New:
						return Res.GetString("51e12a57-88b7-425d-803d-9319ca7e650d",
							"Cannot add a new shipment to the Direct {0} because it already has another shipment attached to it.",
							NameAndList(directConsols));

					case AttachDetachAction.Attach:
						return (parent is CommonShipment)
							? Res.GetString("469c0a26-b1aa-4c5e-a97e-09e70f1b6815",
									"Cannot attach the Direct {0} to the {1} because the {2} already has another shipment attached.",
									NameAndList(directConsols),
									NameAndList(shipments),
									LowerCaseName(directConsols))
							: (shipments.Any(item => item != null))
								? Res.GetString("181dabe0-bbb7-4b92-bc20-b5bf37daa25b",
										"Cannot attach the {0} to the Direct {1} because it already has another shipment attached to it.",
										NameAndList(shipments),
										NameAndList(directConsols))
								: Res.GetString("5388a0bb-4dab-4b35-b4f0-c64fd26f9b3b",
										"Cannot attach a shipment to the Direct {0} because it already has another shipment attached to it.",
										NameAndList(directConsols));

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		bool CanShipmentBeAddedToDirectConsolWithoutError(CommonConsol consol, CommonShipment newShipment)
		{
			Argument.NotNull(consol, "consol");
			Argument.NotNull(newShipment, "newShipment");
			CommonShipment currentMaster;

			if (consol.Shipments.Count > 1)
			{
				currentMaster = consol.Shipments.Cast<CommonShipment>().FirstOrDefault(s => s.IsAssemblyMaster);
			}
			else if (consol.Shipments.Count == 1)
			{
				currentMaster = consol.Shipments.Cast<CommonShipment>().First();
			}
			else
			{
				return true;
			}

			return currentMaster != null && (currentMaster.JS_JS_ColoadMasterShipment == newShipment.PK
				|| newShipment.JS_JS_ColoadMasterShipment == currentMaster.PK
				|| consol.Shipments.Contains(newShipment.PK));
		}

		#endregion

		#region CheckInactiveConsol

		string CheckInactiveConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var inactiveConsols = consols.Where(consol => consol != null && consol.IsCancelled);
			if (inactiveConsols.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return (parent is CommonShipment)
							? Res.GetString("61ea99c8-cea7-4729-981d-c6db8edd7d61",
									"Cannot attach the {0} to the {1} because the {2} is inactive.",
									NameAndList(inactiveConsols),
									NameAndList(shipments),
									LowerCaseName(inactiveConsols))
							: Res.GetString("cfb37f74-64c2-4815-a66b-9b5aececd019",
									"Cannot attach the {0} to the {1} because the {2} is inactive.",
									NameAndList(shipments),
									NameAndList(inactiveConsols),
									LowerCaseName(inactiveConsols));
					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckIsMultiAWBMasterConsol

		string CheckIsMultiAWBMasterConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var multiAWBMasterConsols = consols.Where(consol => consol != null && consol.IsMultiAWBMaster);
			if (multiAWBMasterConsols.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.New:
						return Res.GetString("365f0670-2520-49e3-b817-08c9350d2dc7",
							"Cannot add a new shipment to the Multi AWB Master {0}.",
							NameAndList(multiAWBMasterConsols));

					case AttachDetachAction.Attach:
						return (parent is CommonShipment)
							? Res.GetString("e2fe7d73-1f0e-4529-b3eb-cfe864de0028",
									"Cannot attach the Multi AWB Master {0} to the {1}.",
									NameAndList(multiAWBMasterConsols),
									NameAndList(shipments))
							: Res.GetString("5a9b73e4-0cfa-49c4-8067-262e96f67530",
									"Cannot attach the {0} to the Multi AWB Master {1}.",
									NameAndList(shipments),
									NameAndList(multiAWBMasterConsols));
					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentCanHaveDirectConsol

		string CheckShipmentCanHaveDirectConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (CommonShipment shipment in shipments.Where(item => item != null))
			{
				foreach (CommonConsol consol in consols.Where(item => item != null && item.IsDirect && !ShipmentCanHaveDirectConsol(shipment)))
				{
					shipmentsList.AddIfNotContains(shipment);
					consolsList.AddIfNotContains(consol);
				}
			}

			if (shipmentsList.Any())
			{
				var potentiallyDirectShipments = shipmentsList.Where(item => item.CanBeDirect());
				var otherShipments = shipmentsList.Except(potentiallyDirectShipments);

				StringBuilder builder = new StringBuilder();

				switch (userAction)
				{
					case AttachDetachAction.Attach:
						if (parent is CommonShipment)
						{
							if (potentiallyDirectShipments.Any())
							{
								builder.Append(Res.GetString("6827a100-5b73-49ad-9cd5-4a018a3bde2c",
										"Cannot attach the Direct {0} to the {1} because the {2} is already attached to at least one other Non-Direct Consol.",
										NameAndList(consolsList),
										NameAndList(potentiallyDirectShipments),
										LowerCaseName(potentiallyDirectShipments)));
							}

							if (otherShipments.Any())
							{
								if (builder.Length > 0)
								{
									builder.AppendLine("");
								}

								builder.Append(Res.GetString("d8bf338f-4236-4b1d-b705-3788a913fd21",
										"Cannot attach the Direct {0} to the {1} because the {2} is not a Standard House {2}, High Volume Low Value {2} or Assembly Master {2} with no master shipment.",
										NameAndList(consolsList),
										NameAndList(otherShipments),
										LowerCaseName(otherShipments)));
							}
						}
						else
						{
							if (potentiallyDirectShipments.Any())
							{
								builder.Append(Res.GetString("a5790c4b-2997-4f10-a386-35b04798560c",
									"Cannot attach the {0} to the Direct {1}, because the {2} is already attached to at least one other Non-Direct Consol.",
									NameAndList(potentiallyDirectShipments),
									NameAndList(consolsList),
									LowerCaseName(potentiallyDirectShipments)));
							}

							if (otherShipments.Any())
							{
								if (builder.Length > 0)
								{
									builder.AppendLine("");
								}

								builder.Append(Res.GetString("fe9da037-86d8-474a-b1f2-1ada08fa2f43",
										"Cannot attach the {0} to the Direct {1}, because the {2} is not a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no master shipment.",
										NameAndList(otherShipments),
										NameAndList(consolsList),
										LowerCaseName(otherShipments)));
							}
						}

						return builder.ToString();

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		bool ShipmentCanHaveDirectConsol(CommonShipment shipment)
		{
			return shipment.CanBeDirect() && (shipment.Consols.Count == 0 || shipment.ContainsDirectConsol());
		}

		#endregion

		#region CheckIsAllowedToAttachShipmentWithApprovedForCargoOnlyInspectionType

		string CheckIsAllowedToAttachShipmentWithApprovedForCargoOnlyInspectionType(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (SupplyChainSecurityConfiguration.IsEnabled)
			{
				var shipmentsList = new List<CommonShipment>();
				var consolsList = new List<CommonConsol>();

				foreach (var shipment in shipments.Where(shipment => shipment != null
					&& !shipment.AviationSecurity.IsAllowedOnPassengerFlights()))
				{
					foreach (var consol in consols.Where(consol => consol != null
						&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
						&& !consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType))
					{
						shipmentsList.AddIfNotContains(shipment);
						consolsList.AddIfNotContains(consol);
					}
				}

				if (shipmentsList.Any())
				{
					switch (userAction)
					{
						case AttachDetachAction.Attach:
							return (parent is CommonShipment)
								? Res.GetString("9d28e78d-aa6e-4b8b-921d-9634298ba35a",
										"Cannot attach the {0} to the {1} because the {2} or one of its sub-shipments is not Aviation Security Approved or Exempt and the consol has a voyage which is not Cargo Only.",
										NameAndList(consolsList),
										NameAndList(shipmentsList),
										LowerCaseName(shipmentsList))
								: Res.GetString("4022da88-8aab-49e3-aa6f-110b21434162", "For a voyage that is not Cargo Only, all Shipments must be Aviation Security Approved or Exempt");

						default:
							throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
					}
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckOrganisationsAreApprovedForShippingOnPassengerFlights

		string CheckOrganisationsAreApprovedForShippingOnPassengerFlights(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (SupplyChainSecurityConfiguration.IsEnabled)
			{
				var shipmentsList = new List<CommonShipment>();
				var consolsList = new List<CommonConsol>();

				foreach (var shipment in shipments.Where(shipment => shipment != null
					&& shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
					&& !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights))
				{
					foreach (var consol in consols.Where(consol => consol != null
						&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
						&& !consol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType))
					{
						shipmentsList.AddIfNotContains(shipment);
						consolsList.AddIfNotContains(consol);
					}
				}

				if (shipmentsList.Any())
				{
					switch (userAction)
					{
						case AttachDetachAction.Attach:
							return (parent is CommonShipment)
								? Res.GetString("8da84c0b-15cb-4406-82f2-5bed391f89c1",
										"Cannot attach the {0} to the {1} because the {2} or one of its sub-shipments has been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo.",
										NameAndList(consolsList),
										NameAndList(shipmentsList),
										LowerCaseName(shipmentsList))
								: Res.GetString("09a5ba49-4c94-4bde-a99d-58017d486221", "The Shipment has been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo.");

						default:
							throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
					}
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentHasJobAndChanges

		string CheckShipmentHasJobAndChanges(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (userAction == AttachDetachAction.Detach
							&& parent.TablePrefix == JobConsolSchema.Constants.Prefix
							&& HasJobAndChanges(shipments))
			{
				return Res.GetString("557f321c-92b2-410a-820f-564c70a77380", "Shipment contains important changes and should be saved before it can be detached from consol.");
			}

			return string.Empty;
		}

		bool HasJobAndChanges(IEnumerable<CommonShipment> shipments)
		{
			foreach (var shipment in shipments)
			{
				if (HasJobAndChanges(shipment)
								|| HasJobAndChanges(shipment.CoLoadShipments.Cast<CommonShipment>()))
				{
					return true;
				}
			}

			return false;
		}

		bool HasJobAndChanges(CommonShipment shipment)
		{
			return shipment != null && HasChangesExcludingJobConShipLink(shipment) && shipment.Job != null;
		}

		public bool HasChangesExcludingJobConShipLink(CommonShipment shipment)
		{
			if (!shipment.HasChanges)
			{
				return false;
			}

			IEnumerable<BusinessObject> links = shipment.Factory.Load<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipment.PK) { FetchOnlyFromLocalCache = true })
				.Where(t => t.HasChanges);
			IEnumerable<BusinessObject> logs = shipment.Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipment.PK) { FetchOnlyFromLocalCache = true })
				.Where(t => t.HasChanges);

			var bizosWithChanges = links.Concat(logs).ToList();

			foreach (var bizo in bizosWithChanges)
			{
				bizo.HasChanges = false;
			}

			var result = shipment.HasChanges;
			foreach (var bizo in bizosWithChanges)
			{
				bizo.HasChanges = true;
			}

			return result;
		}

		#endregion

		#region CheckHasConsolApportionmentOnShipment

		string CheckHasConsolApportionmentOnShipment(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (CommonShipment shipment in shipments.Where(item => item != null))
			{
				foreach (CommonConsol consol in consols.Where(item => item != null && HasConsolApportionmentOnShipment(shipment, item)))
				{
					shipmentsList.AddIfNotContains(shipment);
					consolsList.AddIfNotContains(consol);
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Detach:
						return (parent is CommonShipment)
							? Res.GetString("a9d493aa-352d-47d4-94fe-a0751a2bc0c4",
									"Cannot detach the {0} from the {1} as posted/unposted apportionments exist on the {2}.\r\nIf the apportionment is not posted, you can detach the {2} after you have deleted the apportionment.\r\nIf the apportionment is posted, you can detach the {2} after you have reversed the invoice with the apportioned charges and deleted the apportionment.",
									NameAndList(consolsList),
									NameAndList(shipmentsList),
									LowerCaseName(consolsList))
							: Res.GetString("c237e8ce-81dd-4f10-8f0a-9bc1d566d62d",
									"Cannot detach the {0} from the {1} as posted/unposted apportionments exist on the {2}.\r\nIf the apportionment is not posted, you can detach the {3} after you have deleted the apportionment.\r\nIf the apportionment is posted, you can detach the {3} after you have reversed the invoice with the apportioned charges and deleted the apportionment.",
									NameAndList(shipmentsList),
									NameAndList(consolsList),
									LowerCaseName(consolsList),
									LowerCaseName(shipmentsList));

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		bool HasConsolApportionmentOnShipment(CommonShipment shipment, CommonConsol consol)
		{
			return consol != null && shipment != null && consol.Shipments.Contains(shipment) && HasApportionmentOnShipment(consol, shipment);
		}

		bool HasApportionmentOnShipment(CommonConsol consol, CommonShipment shipment)
		{
			bool result = false;

			if (shipment.Job != null)
			{
				var findChargesForJobQuery = new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, DBNull.Value);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, 0m);
				findChargesForJobQuery.FetchOnlyFromLocalCache = true;
				var charges = consol.Factory.Load<JobCharge>(findChargesForJobQuery);

				if (charges.Length > 0)
				{
					var consolCostPKs = charges.ToArray().Select(x => x.JR_E6).Distinct();
					var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK);
					consolCostQuery.AddToFilter(JobConsolCostSchema.PK, consolCostPKs);

					result = consol.Factory.LoadTop1<Enterprise.Integration.Accounting.IJobConsolCost>(consolCostQuery) != null;
				}
			}

			if (!result && shipment.IsInDatabase)
			{
				string sql = string.Empty;

				if (shipment.IsLeadOrMaster)
				{
					sql = @"
					SELECT TOP 1 1 AS A FROM dbo.JobShipment
					INNER JOIN dbo.JobHeader ON JH_ParentID = JS_PK
					INNER JOIN dbo.JobCharge ON JR_JH = JH_PK
					INNER JOIN dbo.JobConsolCost ON JR_E6 = E6_PK
					WHERE E6_ParentID = @ConsolID
					AND E6_ParentTableCode = @ConsolType
					AND JS_PK = @ShipmentPK

					UNION ALL

					SELECT TOP 1 1 AS A FROM dbo.JobShipment
					INNER JOIN dbo.JobHeader ON JH_ParentID = JS_PK
					INNER JOIN dbo.JobCharge ON JR_JH = JH_PK
					INNER JOIN dbo.JobConsolCost ON JR_E6 = E6_PK
					WHERE E6_ParentID = @ConsolID
					AND E6_ParentTableCode = @ConsolType
					AND JS_JS_ColoadMasterShipment = @ShipmentPK";
				}
				else
				{
					sql = @"
					SELECT TOP 1 1 AS A FROM dbo.JobShipment
					INNER JOIN dbo.JobHeader ON JH_ParentID = JS_PK
					INNER JOIN dbo.JobCharge ON JR_JH = JH_PK
					INNER JOIN dbo.JobConsolCost ON JR_E6 = E6_PK
					WHERE E6_ParentID = @ConsolID
					AND E6_ParentTableCode = @ConsolType
					AND JS_PK = @ShipmentPK";
				}

				var collection = new DynamicBusinessObjectCollection(shipment.Factory);

				var parameters = new ZSqlParameterCollection();
				parameters.Add("@ConsolID", consol.PK.ToGuid(), JobConsolCostSchema.E6_ParentID);
				parameters.Add("@ConsolType", "JK", JobConsolCostSchema.E6_ParentTableCode);
				parameters.Add("@ShipmentPK", shipment.PK.ToGuid(), JobShipmentSchema.PK);
				parameters.Add("@MasterShipmentPK", shipment.PK.ToGuid(), JobShipmentSchema.PK);

				collection.Load(sql, parameters);
				result = collection.Count > 0;
			}

			return result;
		}

		#endregion

		#region CheckHasShipmentChargesOnCollectInvoice

		string CheckHasShipmentChargesOnCollectInvoice(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (var shipment in shipments.Where(item => item != null))
			{
				foreach (var consol in consols.Where(item => item != null && HasShipmentChargesOnCollectInvoice(shipment, item)))
				{
					shipmentsList.AddIfNotContains(shipment);
					consolsList.AddIfNotContains(consol);
				}
			}

			if (shipmentsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Detach:
						return (parent is CommonShipment)
							? Res.GetString("2c450dd3-cfd7-4960-b11e-9f00af8a8d01",
									"Cannot detach the {0} from the {1} as there are collect charges posted.",
									NameAndList(consolsList),
									NameAndList(shipmentsList))
							: Res.GetString("34d2405f-388c-48a3-a7dd-8126980542d7",
									"Cannot detach the {0} from the {1} as there are collect charges posted.",
									NameAndList(shipmentsList),
									NameAndList(consolsList));

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		bool HasShipmentChargesOnCollectInvoice(CommonShipment shipment, CommonConsol consol)
		{
			return consol != null && shipment != null && consol.Shipments.Contains(shipment) && HasChargesOnCollectInvoice(consol, shipment);
		}

		bool HasChargesOnCollectInvoice(CommonConsol consol, CommonShipment shipment)
		{
			if (consol.JK_UniqueConsignRef.IsEmpty)
			{
				return false;
			}

			string sQL =
				@"
				SELECT TOP 1
					*
				from
					dbo.JobHeader
					inner join dbo.AccTransactionLines on AL_JH = JH_PK
					inner join dbo.AccTransactionHeader on AL_AH = AH_PK
				where
					JH_ParentID = @ShipmentPK
					and AH_JH is null
					and AL_LineType = @RevenueLineType
					and AH_Ledger = @ARLedger
					and AH_TransactionType = @InvoiceType
					and AH_IsCancelled = 0
					and AH_JobNumber = @ConsolNumber";

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@ConsolNumber", consol.JK_UniqueConsignRef, AccTransactionHeaderSchema.AH_JobNumber);
			parameters.Add("@ShipmentPK", shipment.PK, JobShipmentSchema.PK);
			parameters.Add("@RevenueLineType", TransactionLineTypes.Revenue, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@ARLedger", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@InvoiceType", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);

			DynamicBusinessObjectCollection dynamicBizOs = new DynamicBusinessObjectCollection(consol.Factory);
			dynamicBizOs.Load(sQL, parameters);

			return dynamicBizOs.Count > 0;
		}

		#endregion

		#region CheckShipmentHasMasterOnSameConsol

		string CheckShipmentHasMasterOnSameConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var mastersList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (var shipment in shipments.Where(item => item != null && item.CoLoadMasterShipment != null && !shipments.Contains(item.CoLoadMasterShipment)))
			{
				var master = shipment.CoLoadMasterShipment;
				foreach (var consol in consols.Where(item => item != null && master.Consols.Contains(item)))
				{
					shipmentsList.AddIfNotContains(shipment);
					mastersList.AddIfNotContains(master);
					consolsList.AddIfNotContains(consol);
				}
			}

			if (shipmentsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Detach:
						return (parent is CommonShipment)
							? Res.GetString("34945769-3b5f-4bab-af72-4186771ff010",
									"Cannot detach the {0} from the {1} as the shipment is a sub-shipment of the master-shipment {2} that also belongs to the {3}. If you would like to detach the {3} then you must detach the shipment from its master-shipment first.",
									NameAndList(consolsList),
									NameAndList(shipmentsList),
									ListAsString(mastersList),
									LowerCaseName(consolsList))
							: Res.GetString("9e242d7b-dd16-4f3e-ac47-5fc2081c0ccd",
									"Cannot detach the {0} from the {1} as the {2} {3} sub-{2} of the master-{4} that also belongs to the {5}. If you would like to detach {6}, then you must detach the {2} from the master-{7} first.",
									NameAndList(shipmentsList),
									NameAndList(consolsList),
									LowerCaseName(shipmentsList),
									shipmentsList.Take(2).Count() == 1 ? IsA : Are,
									NameAndList(mastersList),
									LowerCaseName(consolsList),
									ListAsString(shipmentsList),
									LowerCaseName(mastersList));

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentConsolPivot

		string CheckShipmentConsolPivotCanBeDeleted(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();
			var messagesList = new List<string>();

			var subShipments = shipments.SelectMany(s => s.CoLoadShipments.Cast<CommonShipment>());

			foreach (var shipment in shipments.Concat(subShipments).Where(item => item != null))
			{
				foreach (var consol in consols.Where(item => item != null && shipment.Consols.Contains(item)))
				{
					BusinessObject shipmentConsolPivot = shipment.Consols.GetRelationshipBusinessObject(consol);
					try
					{
						shipmentConsolPivot.RunDeleteCheckers();
					}
					catch (CannotDeleteException ex)
					{
						shipmentsList.AddIfNotContains(shipment);
						consolsList.AddIfNotContains(consol);
						messagesList.AddIfNotContains(ex.Message);
					}
				}
			}

			if (shipmentsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Detach:
						return (parent is CommonShipment)
							? Res.GetString("a65a76c7-6120-4a07-aaa0-a04523d3d2c8",
									"Cannot detach the {0} from the {1}: {2}",
									NameAndList(consolsList),
									NameAndList(shipmentsList),
									string.Join(", ", messagesList))
							: Res.GetString("11c8cdc6-f586-487e-a3e9-a5861b8b8fd2",
									"Cannot detach the {0} from the {1}: {2}",
									NameAndList(shipmentsList),
									NameAndList(consolsList),
									string.Join(", ", messagesList));

					default:
						throw new NotSupportedException(string.Format("User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentPacklinesMatchConsolTemperatureControl

		string CheckShipmentPacklinesMatchConsolTemperatureControl(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (userAction != AttachDetachAction.Attach)
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
			}

			var consolList = new List<CommonConsol>();
			var shipmentList = new List<CommonShipment>();

			foreach (var shipment in shipments)
			{
				foreach (var consol in consols)
				{
					if (!consol.ShipmentTemperatureRangeIsValid(shipment))
					{
						consolList.Add(consol);
						shipmentList.Add(shipment);
					}
				}
			}

			if (consolList.Count > 0)
			{
				return Res.GetString("11b3d6f0-b314-58bf-44cb-ff1d2de7025c", "Cannot attach {0} as it has a temperature range not supported by the {1}.", NameAndList(shipments), NameAndList(consols));
			}

			return string.Empty;
		}

		#endregion

		#region CheckUserRightsToAttachShipmentWithDifferentGatewayServiceLevel

		string CheckUserRightsToAttachShipmentWithDifferentGatewayServiceLevel
		(
			AttachDetachAction userAction,
			IEnumerable<CommonShipment> shipments,
			IEnumerable<CommonConsol> consols,
			BusinessObject parent
		)
		{
			if (IsGatewayServiceLevelCheckSuspended)
			{
				return string.Empty;
			}

			var gatewayServiceLevelChecker = new ExclusiveGatewayServiceChecker();

			foreach (var consol in consols.WhereNotNull())
			{
				foreach (var shipment in shipments.WhereNotNull())
				{
					var errorMessage = gatewayServiceLevelChecker.CheckUserPermissionToAttachShipmentWithDifferentServiceLevel(
						userAction,
						shipment,
						consol);

					if (!string.IsNullOrWhiteSpace(errorMessage))
					{
						return errorMessage;
					}
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge

		string CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge(AttachDetachAction userAction,
			IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			return CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge(userAction, shipments, consols, parent, isWarning: false);
		}

		string CheckShipmentDoesNotHaveAnotherConsolWithSameLoadOrDischarge(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent, bool isWarning)
		{
			var isConsolParent = parent is CommonConsol;
			var consolsWithSameDischargePort = new List<CommonConsol>();
			var consolsWithSameLoadPort = new List<CommonConsol>();
			var consolsInOnePortWithTimeOverlap = new List<CommonConsol>();
			foreach (CommonShipment shipment in shipments.Where(x => x != null && (!isConsolParent || x.JS_ShipmentType != Core.Constants.ShipmentTypes.BuyersConsolLead)))
			{
				foreach (CommonConsol consol in consols.Where(x => x != null))
				{
					if (consol.Shipments.Any() && consol.Shipments.All(s => ((CommonShipment)s).IsBuyersConsolLead))
					{
						continue;
					}

					foreach (CommonConsol shipmentConsol in shipment.Consols)
					{
						var portCompareResult = CommonConsolComparisonHelper.CompareConsolsLoadAndDischargePorts(consol, shipmentConsol, ignoreDomesticRailOrRoad: !isWarning);

						if (portCompareResult == CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameLoadPort || portCompareResult == CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameDischargeAndLoadPort)
						{
							consolsWithSameLoadPort.Add(shipmentConsol);
						}

						if (portCompareResult == CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameDischargePort || portCompareResult == CommonConsolComparisonHelper.ConsolsPortComparisonResult.HasSameDischargeAndLoadPort)
						{
							consolsWithSameDischargePort.Add(shipmentConsol);
						}

						if (portCompareResult == CommonConsolComparisonHelper.ConsolsPortComparisonResult.InOnePortWithTimeOverlap)
						{
							consolsInOnePortWithTimeOverlap.Add(shipmentConsol);
						}
					}
				}
			}
			return CreateErrorMessageForShipmentWithSameLoadOrDischargeInConsol(shipments, consols, isConsolParent, consolsWithSameDischargePort, consolsWithSameLoadPort, consolsInOnePortWithTimeOverlap, isWarning);
		}

		string CreateErrorMessageForShipmentWithSameLoadOrDischargeInConsol(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, bool isConsolParent,
					List<CommonConsol> consolsWithSameDischargePort, List<CommonConsol> consolsWithSameLoadPort, List<CommonConsol> consolsInOnePortWithTimeOverlap, bool isWarning)
		{
			if (!consolsWithSameDischargePort.Any() && !consolsWithSameLoadPort.Any() && !consolsInOnePortWithTimeOverlap.Any())
			{
				return string.Empty;
			}

			var result = new StringBuilder();

			if (!isWarning)
			{
				result.Append(Res.GetString("099a389e-a5d3-4c0e-9b87-01428abb4339", @"Cannot attach {0} to {1}",
					isConsolParent ? NameAndList(shipments) : NameAndList(consols),
					isConsolParent ? NameAndList(consols) : NameAndList(shipments)));
			}

			if (consolsWithSameLoadPort.Any())
			{
				var duplicateLoadError = Res.GetString("9168b139-8f6d-45cc-837e-931c2cf0915b", @"{0} shipment is already attached to a consol with same load port", string.IsNullOrEmpty(result.ToString()) ? (NoResString)"This" : (NoResString)", this");
				result.Append(duplicateLoadError);
			}

			if (consolsWithSameDischargePort.Any())
			{
				var duplicateDischargeError = Res.GetString("14e2e2c6-47d8-4f4c-9f02-96ef625a8128", @"{0} shipment is already attached to a consol with same discharge port", string.IsNullOrEmpty(result.ToString()) ? (NoResString)"This" : (NoResString)", this");
				result.Append(duplicateDischargeError);
			}

			if (consolsInOnePortWithTimeOverlap.Any())
			{
				var duplicateOverlapError = Res.GetString("4145866c-551f-4ffe-aee3-8979413422b7", @"{0} shipment is already attached to a consol with same Load and Discharge port with ETD/ATD or ETA/ATA date overlap", string.IsNullOrEmpty(result.ToString()) ? (NoResString)"This" : (NoResString)", this");
				result.Append(duplicateOverlapError);
			}

			return result.ToString();
		}

		#endregion

		#region PreventDetachingShipmentWhenCCTHouseManifestHasBeenSent

		string PreventDetachingShipmentWhenCCTHouseManifestHasBeenSent(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			foreach (var shipment in shipments.Where(s => s.IsAir && s.IsImport() && s.JS_RL_NKDestination.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Brazil))
			{
				var detachmentStatus = CheckDetachmentStatus(shipment, consols);

				if (detachmentStatus.IsDetachmentDisallowed)
				{
					return Res.GetString("C23FBD1F-A5EE-4279-9871-09C812A216C8",
						"CCT House Manifest has been sent from Consol {0}. Please withdraw the message sent from above Consol prior to detaching shipment(s).",
						detachmentStatus.ConsolUniqueRef);
				}
			}

			return string.Empty;
		}

		(bool IsDetachmentDisallowed, ZString ConsolUniqueRef) CheckDetachmentStatus(CommonShipment shipment, IEnumerable<CommonConsol> consols)
		{
			var allowedEvents = new HashSet<string> { Events.InterchangeRejectedCode, Events.MessageRejectedCode, Events.MessageWithdrawCancelAcceptedCode };
			var disallowedEvents = new HashSet<string> { Events.MessageSentCode, Events.MessageAcceptedCode, Events.MessageWithdrawCancelRequestCode };

			var isShipmentDetachmentAllowed = IsDetachmentAllowedFromConsolOrShipment(shipment, allowedEvents, disallowedEvents, (NoResString)"AdvancedCargoReportBR", (NoResString)"Advanced Cargo Report");

			if (isShipmentDetachmentAllowed)
			{
				return (false, ZString.Empty);
			}

			var isConsolDetachmentAllowed = false;

			foreach (var consol in consols.Where(c => c.IsAir && c.IsImport() && c.JK_RL_NKDischargePort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Brazil))
			{
				isConsolDetachmentAllowed = IsDetachmentAllowedFromConsolOrShipment(consol, allowedEvents, disallowedEvents, (NoResString)"AdvancedManifestBR", (NoResString)"Advanced Manifest");

				if (!isConsolDetachmentAllowed)
				{
					return (true, consol.JK_UniqueConsignRef);
				}
			}

			return (false, ZString.Empty);
		}

		bool IsDetachmentAllowedFromConsolOrShipment(BusinessObject parent, HashSet<string> allowedEvents, HashSet<string> disallowedEvents, string dataStoreName, string documentName)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			var documentData = documentDataLoader.Load(parent, dataStoreName) as IStmALogParent;

			return CheckLatestLogForDetachment(documentData, allowedEvents, disallowedEvents, documentName);
		}

		bool CheckLatestLogForDetachment(IStmALogParent logParent, HashSet<string> allowedEvents, HashSet<string> disallowedEvents, string documentName)
		{
			if (logParent == null)
			{
				return true;
			}

			var latestLog = logParent.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.OrderByDescending(x => x.SL_PostedTimeUtc)
				.FirstOrDefault(log => IsLogApplicable(log, allowedEvents, disallowedEvents, documentName));

			return latestLog == null || !disallowedEvents.Contains(latestLog.SL_SE_NKEvent) && allowedEvents.Contains(latestLog.SL_SE_NKEvent);
		}

		bool IsLogApplicable(StmALog log, HashSet<string> allowedEvents, HashSet<string> disallowedEvents, string documentName)
		{
			if (!allowedEvents.Contains(log.SL_SE_NKEvent) && !disallowedEvents.Contains(log.SL_SE_NKEvent))
			{
				return false;
			}

			var location = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Location) ?? string.Empty;
			if (!location.StartsWith(Core.Constants.CountryCodes.Brazil, StringComparison.Ordinal))
			{
				return false;
			}

			var messageType = log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.MessageType);
			return string.Equals(messageType, documentName, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region CheckStandAloneShipmentCannotDetachConsol

		string CheckStandAloneShipmentCannotDetachConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var consolsList = new List<CommonConsol>();
			foreach (CommonConsol consol in consols.Where(consol => consol != null && consol.IsAttachedToStandAloneShipment))
			{
				consolsList.AddIfNotContains(consol);
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Detach:
						if (parent is CommonShipment)
						{
							return Res.GetString("7e44bc29-ce96-4d2a-a13e-947d65ded81b",
								"Cannot detach {0} as the shipment is a standalone shipment. Please save the form first or cancel the form.",
								NameAndList(consolsList));
						}

						break;

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckConsolAllowsAttachingOfShipmentsDangerousGoods

		string CheckConsolAllowsAttachingOfShipmentsDangerousGoods(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (!FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.Value)
			{
				return string.Empty;
			}

			var consolsList = new List<CommonConsol>();
			var shipmentsList = new List<CommonShipment>();

			foreach (var consol in consols)
			{
				foreach (var shipment in shipments)
				{
					if (!consol.AllowsForShipmentsDangerousGoods(shipment))
					{
						consolsList.AddIfNotContains(consol);
						shipmentsList.AddIfNotContains(shipment);
					}
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return Res.GetString("46729ffb-b7ae-b0a2-4001-067e564b16ca",
							"Cannot attach {0} as it contains dangerous cargo that is not accepted by the {1}", NameAndList(shipmentsList), NameAndList(consolsList));

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckConsolAllowsAttachingShipmentWithLithiumBatteries

		string CheckConsolAllowsAttachingShipmentWithLithiumBatteries(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var consolsList = new List<CommonConsol>();
			var shipmentsList = new List<CommonShipment>();

			foreach (var consol in consols)
			{
				foreach (var shipment in shipments)
				{
					if (!consol.AllowsForShipmentsLithiumBatteries(shipment))
					{
						consolsList.AddIfNotContains(consol);
						shipmentsList.AddIfNotContains(shipment);
					}
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return Res.GetString("ec621c74-1ebf-ce96-43f4-cdd93c82e545",
							"Cannot attach {0} as it contains lithium substances that are not accepted by the {1}.", NameAndList(shipmentsList), NameAndList(consolsList));

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		string CheckUNDGPermissableQuantities(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var consolsList = new List<CommonConsol>();

			foreach (var consol in consols)
			{
				if (!consol.AllShipmentsContainPermissibleQuantities(shipments))
				{
					consolsList.AddIfNotContains(consol);
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return Res.GetString("326a9a10-06cb-7fa5-4089-8040ff88d2d4",
							"Cannot attach {0} as it contains lithium substances which would cause the total allowable quantity for {1} to be exceeded.", NameAndList(shipments), NameAndList(consolsList));

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		string CheckForbiddenDGShipmentCannotAttachToNotCargoOnlyConsol(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			if (consols.Any(consol => consol.Transports.Cast<Transport>().Any(transport => transport.IsAir && !transport.JW_IsCargoOnly)))
			{
				if (shipments.Any(shipment => shipment.HasDangerousGoodsSubstancesForbiddenOnPassengerAircraft()))
				{
					switch (userAction)
					{
						case AttachDetachAction.Attach:
						case AttachDetachAction.New:
							return Res.GetString("c56a6316-2be2-49c0-9470-1fdf64197c13", "Cannot attach shipment to consol as shipment has dangerous goods substances that are forbidden for a passenger flight. Please ensure 'Is Cargo Only' flag is checked on Consol.");
					}
				}
			}

			return string.Empty;
		}

		#region CheckConsolAllowsAttachingShipmentWithMaximumAllowablePacklineAndContainerDimensions

		string CheckConsolAllowsAttachingShipmentWithMaximumAllowablePacklineAndContainerDimensions(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var consolsList = new List<CommonConsol>();
			var shipmentsList = new List<CommonShipment>();

			foreach (var consol in consols)
			{
				foreach (var shipment in shipments)
				{
					if (!consol.ShipmentContainsAllowableCargoDimensions(shipment, true))
					{
						consolsList.AddIfNotContains(consol);
						shipmentsList.AddIfNotContains(shipment);
					}
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return Res.GetString("054bea5a-97c0-e881-45da-a77160e5f71b",
							"Cannot attach {0} as it contains cargo dimensions that exceed the maximum dimensions set by the {1}.", NameAndList(shipmentsList), NameAndList(consolsList));

					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "User Action '{0}' is not suppported.", userAction));
				}
			}

			return string.Empty;
		}

		#endregion

		#region CheckShipmentAndConsolBothHaveSentBookingRequestMessage

		string CheckShipmentAndConsolBothHaveSentBookingRequestMessage(AttachDetachAction userAction, IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var consolsList = new List<CommonConsol>();
			var shipmentsList = new List<CommonShipment>();

			foreach (var consol in consols.Where(item => MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(item, (NoResString)"Booking Request")))
			{
				foreach (var shipment in shipments.Where(item => MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(item, (NoResString)"Booking Request")))
				{
					consolsList.AddIfNotContains(consol);
					shipmentsList.AddIfNotContains(shipment);
				}
			}

			if (consolsList.Any())
			{
				switch (userAction)
				{
					case AttachDetachAction.Attach:
						return Res.GetString("0d18fb2e-d46d-49d3-b6ce-ae9365e4d79b", @"A Booking Request was sent from this Consol and Shipment. To attach this Shipment to the Consol, either:
    1.	Withdraw/Cancel the Booking Request from either the Shipment or Consol or
    2.	Reset either the Shipment or Consol's Booking Request to Original and advise the NVOCC accordingly.");
				}
			}

			return string.Empty;
		}

		bool MessageHasBeenSentAndNoWithdrawAcceptedOrResetToOriginal(IStmALogParent logParent, string documentName)
		{
			var dialogs = logParent.GetDialogs(documentName, false);
			if (dialogs != null)
			{
				foreach (var dialog in dialogs.Reverse())
				{
					if (dialog.TransmissionCode == Events.MessageWithdrawCancelRequestCode && dialog.HasBeenAccepted())
					{
						return false;
					}

					if (dialog.HasBeenResetToOriginal())
					{
						return false;
					}

					if (dialog.TransmissionCode == Events.MessageSentCode)
					{
						return !dialog.HasBeenRejected();
					}
				}
			}

			return false;
		}

		#endregion

		#region CheckComplianceRisk

		public string CheckShipmentAndConsolComplianceRisk(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var result = new StringBuilder();

			CheckAttachingClearShipmentToRiskyConsolidation(shipments, consols, result);

			CheckAttachingRiskyShipmentToClearConsolidation(shipments, consols, result);

			return result.ToString();
		}

		static void CheckAttachingClearShipmentToRiskyConsolidation(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, StringBuilder result)
		{
			foreach (var consol in consols)
			{
				foreach (var shipment in shipments)
				{
					if (IsAttachingClearShipmentToRiskyConsolidation(shipment, consol))
					{
						result.Append(Res.GetString("82a2381c-e9cc-4830-972d-ec67481078da",
							@"Warning - Consol {0} has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab which may cause delays to the Shipment {1}.

Are you sure you want to proceed?

Click No – to cancel the operation.
Click Yes – to attach the shipment.", consol.JK_UniqueConsignRef, shipment.JS_UniqueConsignRef));
					}
				}
			}
		}

		void CheckAttachingRiskyShipmentToClearConsolidation(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, StringBuilder result)
		{
			foreach (var consol in consols)
			{
				var shipmentsList = new List<CommonShipment>();

				foreach (var shipment in shipments)
				{
					if (IsAttachingRiskyShipmentToClearConsol(shipment, consol))
					{
						shipmentsList.Add(shipment);
					}
				}

				if (shipmentsList.Any())
				{
					result.Append(Res.GetString("584d36cc-aadc-4fd4-a583-6f4b7f1c4508",
						@"Warning - Shipment ({0}) has a Job Compliance Status of ‘Risk’ or ‘Override Clear’ within their Compliance Risk tab.
Attaching Shipment(s) with that Risk Status to a Consolidation may put other Shipments linked to the Consolidation at risk.

Are you sure you want to proceed?

Click No – to cancel the operation and review the shipment.
Click Yes – to attach the shipment and flag the Consol as ‘Risk’.", ListAsString(shipmentsList)));
				}
			}
		}

		static bool IsAttachingClearShipmentToRiskyConsolidation(CommonShipment shipment, CommonConsol consolidation)
		{
			return (consolidation.ComplianceRiskStatus.JobRisk.HasOverallRisk() ||
				consolidation.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.OverrideClear)) &&
				shipment.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.Clear);
		}

		static bool IsAttachingRiskyShipmentToClearConsol(CommonShipment shipment, CommonConsol consolidation)
		{
			return
				IsAttachingRiskyShipmentToClearOrOverrideClearConsol(shipment, consolidation) ||
				IsAttachingOverrideClearShipmentToClearConsol(shipment, consolidation);
		}

		static bool IsAttachingOverrideClearShipmentToClearConsol(CommonShipment shipment, CommonConsol consolidation)
		{
			return consolidation.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.Clear) &&
				shipment.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.OverrideClear);
		}

		static bool IsAttachingRiskyShipmentToClearOrOverrideClearConsol(CommonShipment shipment, CommonConsol consolidation)
		{
			return (consolidation.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.Clear) ||
					consolidation.ComplianceRiskStatus.JobRisk.EqualsIgnoringCase(ComplianceRiskStatusCodeList.Codes.OverrideClear)) &&
					shipment.ComplianceRiskStatus.JobRisk.HasOverallRisk();
		}

		#endregion

		#endregion

		#region Common

		public bool IsGatewayServiceLevelCheckSuspended { get; set; }

		IEnumerable<T> CreateEnumerable<T>(T bizo = null) where T : BusinessObject
		{
			return bizo != null ? new[] { bizo } : Enumerable.Empty<T>();
		}

		#endregion

		#region Names and Grammar

		protected abstract string ConsolName_PluralLower { get; }
		protected abstract string ConsolName_SingularLower { get; }

		string ShipmentName_PluralLower
		{
			get { return Res.GetString("fc3d9048-ba79-495a-a8e9-d67b84e4619e", "shipments"); }
		}

		string ShipmentName_SingularLower
		{
			get { return Res.GetString("35c4db2b-c66e-4afb-bbfb-75c2238a0b6f", "shipment"); }
		}

		string IsA
		{
			get { return Res.GetString("194f3613-4f57-4db3-a3f8-f9b388d82f8f", "is a"); }
		}

		string Is
		{
			get { return Res.GetString("2cd111c8-cb18-4019-9b3a-64c451e426b4", "is"); }
		}

		string Are
		{
			get { return Res.GetString("9eb3dced-a028-450b-b4a4-7ce6836ee213", "are"); }
		}

		#endregion

		#region Names and Lists

		const int defaultItemsToList = 3;

		public delegate string ListFormatterDelegate(IEnumerable<BusinessObject> list, int itemsToList = defaultItemsToList);

		string LowerCaseName(IEnumerable<BusinessObject> list)
		{
			if (list == null || !list.Any(item => item != null))
			{
				return string.Empty;
			}

			int countSummary = list.Where(item => item != null).Take(2).Count();

			BusinessObject listItem = list.FirstOrDefault();
			if (listItem is CommonConsol)
			{
				return countSummary == 1 ? ConsolName_SingularLower : ConsolName_PluralLower;
			}
			else if (listItem is CommonShipment)
			{
				return countSummary == 1 ? ShipmentName_SingularLower : ShipmentName_PluralLower;
			}

			return string.Empty;
		}

		public string NameAndList(IEnumerable<BusinessObject> list, int itemsToList = defaultItemsToList)
		{
			if (list == null || !list.Any(item => item != null))
			{
				return string.Empty;
			}

			var builder = new StringBuilder();
			builder.Append(LowerCaseName(list));
			builder.Append(" ");
			builder.Append(ListAsString(list, itemsToList));

			return builder.ToString();
		}

		string ListAsString(IEnumerable<BusinessObject> list, int itemsToList = defaultItemsToList)
		{
			if (list == null || !list.Any(item => item != null))
			{
				return string.Empty;
			}

			var builder = new StringBuilder();
			int listCount = list.Count();

			if (listCount > 1)
			{
				builder.Append("(");
			}

			foreach (BusinessObject listItem in list.Take(itemsToList))
			{
				string uniqueRef = string.Empty;

				if (listItem is CommonConsol)
				{
					uniqueRef = ((CommonConsol)listItem).JK_UniqueConsignRef;

					if (string.IsNullOrEmpty(uniqueRef) && !listItem.IsInDatabase)
					{
						uniqueRef = Res.GetString("34cd002a-56dc-43d6-addf-8acf1a468ccf", "[new consol]");
					}
				}
				else if (listItem is CommonShipment)
				{
					CommonShipment shipment = (CommonShipment)listItem;
					uniqueRef = shipment.JS_UniqueConsignRef.IsEmpty ? shipment.JS_HouseBill : shipment.JS_UniqueConsignRef;

					if (string.IsNullOrEmpty(uniqueRef) && !shipment.IsInDatabase)
					{
						uniqueRef = Res.GetString("3b0545c0-1215-4baf-9e5c-896d6f6a99aa", "[new shipment]");
					}
				}

				if (!string.IsNullOrEmpty(uniqueRef))
				{
					if (builder.Length > 1)
					{
						builder.Append(", ");
					}

					builder.Append(uniqueRef);
				}
			}

			if (listCount == itemsToList + 1)
			{
				builder.Append(" " + Res.GetString("da808b0e-c269-4acd-9782-7b249f5234e6", "and 1 other"));
			}
			else if (listCount > itemsToList + 1)
			{
				builder.Append(" " + Res.GetString("7c4abea9-702f-4eb2-8525-a41181ee3962", "and {0} others", listCount - itemsToList));
			}

			if (listCount > 1)
			{
				builder.Append(")");
			}

			return builder.ToString();
		}

		string CapitalizeName(string name)
		{
			return string.IsNullOrEmpty(name)
				? string.Empty
				: char.ToUpper(name.First()) + name.Substring(1).ToLower();
		}

		#endregion

		#region GetMessage_DetachSubShipmentsFromConsols

		string GetMessage_DetachSubShipmentsFromConsols(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols, BusinessObject parent)
		{
			var shipmentsList = new List<CommonShipment>();
			var subShipmentsList = new List<CommonShipment>();
			var consolsList = new List<CommonConsol>();

			foreach (var shipment in shipments.Where(item => item != null))
			{
				var subShipments = new List<CommonShipment>();
				GetSubShipments(shipment, subShipments);

				foreach (CommonShipment subShipment in subShipments.Where(item => !shipments.Contains(item)))
				{
					foreach (var consol in consols.Where(item => item != null && subShipment.Consols.Contains(item)))
					{
						shipmentsList.AddIfNotContains(shipment);
						subShipmentsList.AddIfNotContains(subShipment);
						consolsList.AddIfNotContains(consol);
					}
				}
			}

			string message = string.Empty;
			if (shipmentsList.Any())
			{
				message = (parent is CommonShipment)
					 ? Res.GetString("7e08436f-7b17-4482-946e-325a99f827c2",
							"The {0} is a master / lead shipment of {1}.\r\n\r\nDo you also want to detach the {2} from the {3}?\r\n\r\nPress [Yes] to detach the {4} from the master / lead shipment & sub-{5}.\r\nPress [No] to detach the {4} from the master / lead shipment, but keep the sub-{5} attached to the {4}.\r\nPress [Cancel] to cancel the operation.",
							NameAndList(shipmentsList),
							ListAsString(subShipmentsList),
							NameAndList(consolsList),
							NameAndList(subShipmentsList),
							LowerCaseName(consolsList),
							LowerCaseName(subShipmentsList))
					: Res.GetString("cd4fe0ef-aae7-451e-9d13-40b319c7ae1f",
							"The {0} {1} master / lead {2} of {3}.\r\n\r\nDo you also want to detach the sub-{4} from the {5}?\r\n\r\nPress [Yes] to detach the master / lead {2} and sub-{4} from the {6}.\r\nPress [No] to detach the master / lead {2} from the {6}, but keep the sub-{4} attached to the {6}.\r\nPress [Cancel] to cancel the operation.",
							NameAndList(shipmentsList),
							shipmentsList.Take(2).Count() == 1 ? IsA : Are,
							LowerCaseName(shipmentsList),
							ListAsString(subShipmentsList),
							LowerCaseName(subShipmentsList),
							NameAndList(consolsList),
							LowerCaseName(consolsList));
			}

			return message;
		}

		#endregion

		#region GetDetachMessage_ExportNotification755Message

		(ZString, ZString, ZString) GetDetachMessage_ExportNotification755Message(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols)
		{
			var existReceivedFFMAndDepartureMessageShipment = false;
			var receivedAcceptedMessageShipmentList = new List<CommonShipment>();
			var existAwaitingResponseMessageShipment = false;

			foreach (var shipment in shipments)
			{
				if (shipment.IsAir && shipment.JS_RL_NKOrigin.SubstringSafe(0, 2) == Core.Constants.CountryCodes.France)
				{
					var documentName = (NoResString)"Export Notification (755)";
					var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
					var documentData = documentDataLoader.Load(shipment, "CINExportNotification") as IStmALogParent;

					var isDialogInitiatingEvent = (StmALog log) => MessageEventCodes.SentMessagesEventCodes.Contains(log.SL_SE_NKEvent.ToString())
						|| (log.SL_SE_NKEvent == Events.StatusUpdatedCode && log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type) == EventReferenceMessageTypes.ResetToOriginal);

					var dialogs = documentData?.GetDialogs(documentName, false, isDialogInitiatingEvent);
					if (dialogs != null && dialogs.Any(dialog => dialog.TransmissionCode == Events.MessageSentCode) && !dialogs.Last().IsWithdrawal())
					{
						var logs = documentData.Logs?.GetAllLogs().OfType<StmALog>()
							.Where(log => log.MatchesDocumentName(documentName) && !log.SL_IsCancelled)
							.OrderByDescending(log => log.SL_PostedTimeUtc) ?? Enumerable.Empty<StmALog>();

						if (logs.Any(log => log.SL_SE_NKEvent == Events.StatusUpdatedCode
								&& log.Parameters.GetValueSafe(EventConstants.EventReferenceParameters.Codes.Type) == (NoResString)"FFM and departure message received"))
						{
							existReceivedFFMAndDepartureMessageShipment = true;
						}
						else if (logs.Any(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode))
						{
							receivedAcceptedMessageShipmentList.Add(shipment);
						}
						else if ((logs.FirstOrDefault()?.SL_SE_NKEvent ?? ZString.Empty) == Events.InterchangeSentCode)
						{
							existAwaitingResponseMessageShipment = true;
						}
					}
				}
			}

			return (existReceivedFFMAndDepartureMessageShipment ?
					Res.GetString("89d6d6f3-b296-41d1-ba5a-59ee972965af", "This shipment has been linked to an Export Notification (755) at Cargo Information Network (CIN) France. To de-link any MRN after FFM and departure message received, please manually correct this with Customs Authorities if needed.") :
					ZString.Empty,

				receivedAcceptedMessageShipmentList.Any() ?
					Res.GetString("18470340-b842-4219-a9cb-57f678f470d9", @"This shipment's MRN Number should be de-linked from the Air Waybill registered at Cargo Information Network (CIN) France. Before detaching the Shipment, you should cancel the Export Notification (755) at Cargo Information Network (CIN) France. To do so, go to Shipment {0} > Electronic Messaging > Port Messaging > Export > CIN Export Notification 755 (FR) and run Cancel/Withdraw Message.
Click ""Yes"" if you first want to de-link the MRN Number and stop detaching the shipment from the Consol. Click ""No"" if you want to continue detaching the Shipment from the Consol."
						, ListAsString(receivedAcceptedMessageShipmentList)) :
					ZString.Empty,

				existAwaitingResponseMessageShipment ?
					Res.GetString("87934809-71cf-49e6-ba8d-03bce0b5047e", "A response has not yet been received from Cargo Information Network (CIN) France. Click \"Yes\" if you want to wait for a response. Click \"No\" if you want to continue detaching the Shipment from the Consol.") :
					ZString.Empty);
		}

		#endregion

		#region GetMessage_AttachDirectShipmentsToAgentConsols

		string GetMessage_AttachDirectShipmentsToAgentConsols(IEnumerable<CommonShipment> shipments)
		{
			var shipmentsList = shipments.ToList();

			if (shipmentsList.Any())
			{
				return Res.GetString("58a74001-4f68-4964-86fb-5ad4ad1063d6",
					"The {0} {1} already attached to at least one other Direct {2}. A shipment on a Direct {2} can only be attached to Direct {3} (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).",
					NameAndList(shipmentsList),
					shipmentsList.Count > 1 ? Are : Is,
					CapitalizeName(ConsolName_SingularLower),
					CapitalizeName(ConsolName_PluralLower));
			}

			return string.Empty;
		}

		#endregion

		#region SubShipment helpers

		List<CommonConsol> GetConsolsToDetach(CommonConsol parentConsol, CommonShipment oldMaster, CommonShipment newMaster)
		{
			var consolsToDetach = new List<CommonConsol>();
			if (oldMaster != null && oldMaster.Consols.Count > 0)
			{
				consolsToDetach = (newMaster != null)
					? oldMaster.Consols.Except(newMaster.Consols).Cast<CommonConsol>().ToList()
					: oldMaster.Consols.Cast<CommonConsol>().ToList();

				if (parentConsol != null)
				{
					consolsToDetach.Remove(parentConsol);
				}
			}

			return consolsToDetach;
		}

		void GetSubShipments(CommonShipment shipment, List<CommonShipment> subShipments)
		{
			subShipments.Add(shipment);
			foreach (CommonShipment subShipment in shipment.CoLoadShipments)
			{
				GetSubShipments(subShipment, subShipments);
			}
		}

		#endregion

		#region SupplyChainSecurityConfiguration

		ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get
			{
				if (supplyChainSecurityConfiguration == null || supplyChainSecurityConfigurationCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				{
					var helper = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>();
					supplyChainSecurityConfiguration = helper.GetConfiguration();
					supplyChainSecurityConfigurationCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}

				return supplyChainSecurityConfiguration;
			}
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;
		ZString supplyChainSecurityConfigurationCountryCode;

		#endregion

		#endregion
	}

	#region ListExtensions

	static class ListExtensions
	{
		public static void AddIfNotContains<T>(this List<T> list, T element)
		{
			if (list != null && !list.Contains(element))
			{
				list.Add(element);
			}
		}
	}

	#endregion
}
