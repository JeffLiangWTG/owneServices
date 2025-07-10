using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.AWB;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolAWBActions : AWBActions, ILicensedComponent, IPrintMAWB, ISendCIMP
	{
		#region Schema

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new sealed class Schema : AWBActions.Schema
		{
			Schema() { }
			public const string ConsolOuterPacksCount = "ConsolOuterPacksCount";
			public const string IncludeSecurityDeclaration = "IncludeSecurityDeclaration";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "All overriding methods verified safe")]
		public ConsolAWBActions(ForwardingConsol consol, ActionsModeType actionsMode)
			: base(actionsMode, consol.Factory)
		{
			this.consol = consol;
			SetAdditionalDefaults();
		}

		public ConsolAWBActions(ForwardingConsol consol, ActionsModeType actionsMode, string menuPath)
			: this(consol, actionsMode)
		{
			OverrideMenuPath = menuPath;
		}

		readonly ForwardingConsol consol;

		public override string SettingsCacheName
		{
			get { return "ConsolAWBActionsSettings"; }
		}

		public bool PerformAllActions()
		{
			var messageSentOk = true;

			if (PrintMasterAirWaybill)
			{
				DoPrintMasterAirWaybill();
			}

			if (PrintConsignmentSecurityDeclaration)
			{
				DoPrintConsignmentSecurityDeclaration();
			}

			if (PrintBarcodeLabel)
			{
				for (ZInt i = 0; i < AWBLabelCopies; i++)
				{
					PrintAWBBarcodeLabel();
				}
			}

			if (PrintHAWBBarcodeLabels)
			{
				for (ZInt i = 0; i < HAWBLabelCopies; i++)
				{
					PrintHAWBBarcodeLabel();
				}
			}

			if (SendFWB && FreightDataRegistry.Instance.MessagingViaPelicanServer.Value)
			{
				messageSentOk = DoSendAirlineMessage();
			}
			else
			{
				if (SendFWB)
				{
					DoSendFWB();
				}

				if (SendFHL)
				{
					DoSendFHL();
				}
			}

			SaveSettings();

			return messageSentOk;
		}

		protected virtual bool DoSendAirlineMessage()
		{
			var airlineMsgManager = ObjectFactory.Get<IAirlineMessagingManager>();
			var messageSentOk = airlineMsgManager.Send(consol, PrepareAirlineMessagesAddInfo(), out var failureReason);

			if (!messageSentOk)
			{
				Globals.Message.ShowError(
					Res.GetString("61aec23e-9d0f-4ad5-b511-b047d5ecf6b8", "Airline Messaging Engine has returned the following validation error/rejection during data processing. Please review and resubmit your airline messages for this consol as the data has not been sent to the Airline.\r\n\r\n{0}", failureReason),
					Res.GetString("58c21fe2-09ca-420f-af63-5aae3da12065", "Airline Messaging Error"));
			}
			SaveFactory();
			return messageSentOk;
		}

		KeyValuePair<string, string>[] PrepareAirlineMessagesAddInfo()
		{
			return new KeyValuePair<string, string>[]
			{
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.SendFWBOrFHLToAirlineBasedOnMAWBPrefix, ForwardingConfigurationRegistry.Instance.SendFWBOrFHLToAirlineBasedOnMAWBPrefix.Value.ToString()),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.SendFWBNatureAndQuantityOfGoodsType, FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.Value.ToString()),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.SendFWB, ((bool)SendFWB).ToString()),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.SendFHL, ((bool)SendFHL).ToString()),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.IncludeECSD, ((bool)includeSecurityDeclaration).ToString()),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.CargoIMPVersion, consol.AWBHeader.AirCargoImpVersion),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.DefaultIdentifierForCneNfyName, consol.AWBHeader.EH_AirlineDefaultIdentifierForCneNfyName),
				new(ExportAWBHeader.Constants.AirlineMessagesAddInfoKeys.DefaultIdentifierForCneNfyPhone, consol.AWBHeader.EH_AirlineDefaultIdentifierForCneNfyPhone)
			};
		}

		void AddMessageSentEventWithParameters(Logs logs, ZString messageType, ZString referenceNumber)
		{
			var eventParameters = new KeyValuePair<string, string>[] {
				new KeyValuePair<string,string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, messageType),
				new KeyValuePair<string,string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, (NoResString)"Airline"),			// Message for event reference
				new KeyValuePair<string,string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, referenceNumber)
			};

			bool isWorkflowAndTemplateApplicationDeferred = WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.Value;
			using (Logs.DeferFiringWorkflow(isWorkflowAndTemplateApplicationDeferred))
			{
				logs.AddNew(Events.MessageSent, eventParameters);
			}
		}

		protected virtual void DoPrintMasterAirWaybill()
		{
			string documentName;

			if (CarrierAWB)
			{
				documentName = DocumentNames.CarrierMAWB;
			}
			else if (LaserAWB)
			{
				documentName = DocumentNames.LaserMAWB;
			}
			else
			{
				documentName = DocumentNames.NeutralMAWB;
			}

			if (consol != null && consol.IsAWBHeaderAccessible)
			{
				ConsolExportAWBHeader consolAWB = consol.AWBHeader as ConsolExportAWBHeader;
				if (consolAWB != null)
				{
					consolAWB.IsPrintingFinalNeutralMAWB = consol.JK_IsNeutralMaster && !consol.IsNeutralMAWBPrinted;
				}
			}

			PrintDocument(documentName, MAWBPrinter, MAWBUseEPrint);
		}

		protected void DoPrintConsignmentSecurityDeclaration()
		{
			if (ShouldUpdateSecurityStatusIssueDate)
			{
				consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Now;
			}

			var printContact = new DocDeliveryContact(Factory);
			PrintDocument(DocumentNames.ConsignmentSecurityDeclaration, MAWBPrinter, MAWBUseEPrint, MAWBUseEPrint ? null : printContact);
		}

		public void PrintHAWBBarcodeLabel()
		{
			PrintDocument(DocumentNames.HAWBBarcodeLabels, HAWBLabelPrinter, HAWBLabelUseEPrint);
		}

		DocumentSupporterDataState GetDocumentDataState(string documentName, string menuPath)
		{
			var filter = new DocumentZQuery(true);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, documentName);

			if (menuPath != null)
			{
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
			}

			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, DocumentSupportable.DocumentSupporter.BusinessContext);

			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			if (menuItem == null)
			{
				string menuPathAndName = menuPath != null ? menuPath + "/" + documentName : documentName;
				string fallbackMenuPathAndName = MenuName != null ? MenuName + "/" + documentName : documentName;
				DocumentSupporterDataState result = new DocumentSupporterDataState();
				result.IsValid = false;
				result.ErrorMessage = Res.GetString("08d730e0-6ae9-43ee-ba91-ed2b03e66fc8",
					"Expected document ({0}) does not exist. Default document ({1}) will be used instead.",
					menuPathAndName, fallbackMenuPathAndName);
				return result;
			}

			return DocumentSupportable.DocumentSupporter.GetDataStateBeforeRun(menuItem);
		}

		#region Properties

		#region PrintMasterAirWaybill

		public override ZBool PrintMasterAirWaybill
		{
			get { return base.PrintMasterAirWaybill; }
			set
			{
				base.PrintMasterAirWaybill = value;
				if (!IsValidationSuspended)
				{
					ValidateMAWBPrinter();
					ValidateMAWBUseEPrint();
				}

				MAWBPrinterInfo.RefreshBinding();
				MAWBUseEPrintInfo.RefreshBinding();
				LaserAWBInfo.RefreshBinding();
				NeutralAWBInfo.RefreshBinding();
				CarrierAWBInfo.RefreshBinding();
			}
		}

		public override void ValidatePrintMasterAirWaybill()
		{
			base.ValidatePrintMasterAirWaybill();
			if (PrintMasterAirWaybill)
			{
				if (DatePrinted.Length > 0)
				{
					PrintMasterAirWaybillInfo.AddWarning(Res.GetString("1fdb20f1-b04e-4c36-8808-bd8d0b9669ba", "This document has already been printed."));
				}

				string docName = string.Empty;

				if (CarrierAWB)
				{
					docName = DocumentNames.CarrierMAWB;
				}
				else if (LaserAWB)
				{
					docName = DocumentNames.LaserMAWB;
				}
				else
				{
					docName = DocumentNames.NeutralMAWB;
				}

				DocumentSupporterDataState docDataState = GetDocumentDataState(docName, string.IsNullOrEmpty(OverrideMenuPath) ? MenuName : OverrideMenuPath);
				if (docDataState != null && !docDataState.IsValid)
				{
					PrintMasterAirWaybillInfo.AddWarning(docDataState.ErrorMessage);
				}
			}
		}

		protected bool NotPrintingMasterAirWayBill
		{
			get { return !PrintMasterAirWaybill; }
		}

		#endregion

		#region DatePrinted

		public override ZString DatePrinted
		{
			get { return base.DatePrinted; }
			set
			{
				base.DatePrinted = value;
				if (!IsValidationSuspended)
				{
					ValidatePrintMasterAirWaybill();
				}
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("c1e878ef-c95b-46e3-a058-8b6614a07129", "Consol"); }
		}

		#region NeutralAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public override ZBool NeutralAWB
		{
			get { return base.NeutralAWB; }
			set { base.NeutralAWB = value; }
		}

		public override void ValidateNeutralAWB()
		{
			base.ValidateNeutralAWB();

			//allow coload to print
			if (NeutralAWB && !consol.JK_IsNeutralMaster && (consol.IsAgentOrDirect || consol.IsAWBCoload))
			{
				NeutralAWBInfo.AddError(Res.GetString("033c77d5-29f8-4147-89b5-883fc6dbbbb2", "The MAWB that you are trying to print is a Carrier MAWB.\r\nPrinting of the Neutral MAWB is not allowed in this case."));
			}

			ValidatePrintMasterAirWaybill();
		}

		#endregion

		#region CarrierAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public override ZBool CarrierAWB
		{
			get { return base.CarrierAWB; }
			set { base.CarrierAWB = value; }
		}

		public override void ValidateCarrierAWB()
		{
			base.ValidateCarrierAWB();

			if (CarrierAWB && (consol.JK_IsNeutralMaster || consol.IsCoLoad))
			{
				CarrierAWBInfo.AddError(Res.GetString("78d8c14d-fb41-4af6-8b04-c0ec2dfdeaba", "The MAWB that you are trying to print is a Neutral \\ Master House MAWB.\r\n Printing of the Carrier MAWB is not allowed in this case."));
			}

			ValidatePrintMasterAirWaybill();
		}

		#endregion

		#region LaserAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public override ZBool LaserAWB
		{
			get { return base.LaserAWB; }
			set { base.LaserAWB = value; }
		}

		public override void ValidateLaserAWB()
		{
			base.ValidateLaserAWB();

			if (LaserAWB && !consol.JK_IsNeutralMaster && (consol.IsAgentOrDirect || consol.IsAWBCoload))
			{
				LaserAWBInfo.AddError(Res.GetString("bb5c244a-46e1-4ea3-841b-d1bd6836f3e6", "The MAWB that you are trying to print is a Carrier MAWB.\r\nPrinting of the Laser MAWB is not allowed in this case."));
			}

			ValidatePrintMasterAirWaybill();
		}

		#endregion

		#region MAWBPrinter

		[List("PrinterNames")]
		public override ZGuid MAWBPrinter
		{
			get { return base.MAWBPrinter; }
			set
			{
				if (!IsDeserialising || IsPrinterQueuePKValid(value))
				{
					base.MAWBPrinter = value;
				}
			}
		}

		protected bool MAWBPrinter_ReadOnly
		{
			get { return NotPrintingMasterAirWayBill || MAWBUseEPrint; }
		}

		public override void ValidateMAWBPrinter()
		{
			base.ValidateMAWBPrinter();

			if (!MAWBPrinterInfo.ReadOnly && PrintMasterAirWaybill)
			{
				MandatoryValidation.CheckEntered(MAWBPrinterInfo);
				PrintQueueValidation.ValidatePrintQueue(MAWBPrinterInfo);
			}
		}

		#endregion

		#region MAWBUseEPrint

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public override ZBool MAWBUseEPrint
		{
			get { return base.MAWBUseEPrint; }
			set
			{
				base.MAWBUseEPrint = value;
				if (value)
				{
					MAWBPrinter = ZGuid.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateMAWBPrinter();
				}
				MAWBPrinterInfo.RefreshBinding();
			}
		}

		public override void ValidateMAWBUseEPrint()
		{
			base.ValidateMAWBUseEPrint();
			if (!MAWBUseEPrintInfo.ReadOnly && MAWBUseEPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(MAWBUseEPrintInfo);
			}
		}

		#endregion

		#region ConsolOuterPacksCount

		public virtual ZString ConsolOuterPacksCount
		{
			get { return consol.JK_TotalShipmentQuantity.ToString(0); }
		}

		public ZPropertyInfo ConsolOuterPacksCountInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ConsolOuterPacksCount); }
		}

		#endregion

		#region SendFWB

		public override ZBool SendFWB
		{
			get { return base.SendFWB; }
			set
			{
				base.SendFWB = value;
				if (!IsValidationSuspended)
				{
					ValidateSendFHL();
				}

				IncludeSecurityDeclaration = value && ShouldSendECSDByDefault;
			}
		}

		public override void ValidateSendFWB()
		{
			base.ValidateSendFWB();

			if (SendFWB)
			{
				if (consol.JK_MasterBillNum.Length != 11)
				{
					SendFWBInfo.AddError(ErrorFWBCannotBeSentWithoutMAWBNumber);
				}

				if (consol.IsCoLoad)
				{
					SendFWBInfo.AddError(ErrorFWBCannotBeSentForColoadConsols);
				}

				if (GlbBranch.CurrentBranch.HomePort == null)
				{
					SendFWBInfo.AddError(ErrorFWBCannotBeSentWithoutBranchHomePort);
				}

				if (GlbBranch.CurrentBranch.HomePort != null && GlbBranch.CurrentBranch.HomePort.RL_IATA.IsEmpty)
				{
					SendFWBInfo.AddError(ErrorFWBCannotBeSentWithoutBranchHomePortIATACode);
				}

				if (DateLastSent.Length != 0)
				{
					SendFWBInfo.AddWarning(WarningCargoImpMessagesHaveAlreadyBeenSent);
				}

				if (CargoIMPMessageSenderServiceTaskIsInActiveOrTurnOff)
				{
					if (Env.CurrentUser.IsSupportUser || Env.CurrentUser.IsDeveloper)
					{
						SendFWBInfo.AddWarning(ErrorCargoImpMessageServiceTaskIsNotActive);
					}
					else
					{
						SendFWBInfo.AddError(ErrorCargoImpMessageServiceTaskIsNotActive);
					}
				}

				ValidateConsolExportAWBHeader();
				consol.SupplyChainSecurityConfiguration.ValidateSendFWBPackLineInspectionTypes(consol, SendFWBInfo);
			}
			else
			{
				if (consol.AnyGoodsTravelToOrThroughFHLCountry)
				{
					SendFWBInfo.AddWarning(WarningFHLRegulationsMessage);
				}
			}
		}

		UsaACASCountryHandler usaACASCountryHandler;

		public void ValidateACASForFWB()
		{
			usaACASCountryHandler = consol.AWBHeader.GetACASCountryHandler() as UsaACASCountryHandler;
			if (usaACASCountryHandler.HasWarningForFWB(out var fwbWarnings))
			{
				fwbWarnings.ForEach(SendFWBInfo.AddWarning);
			}
		}

		public void ValidateACASForFHL()
		{
			usaACASCountryHandler = consol.AWBHeader.GetACASCountryHandler() as UsaACASCountryHandler;
			if (usaACASCountryHandler.HasWarningsForFHL(out var fhlWarnings))
			{
				fhlWarnings.ForEach(SendFHLInfo.AddWarning);
			}
		}

		public void ValidateConsolExportAWBHeader()
		{
			var awbHeaderError = Res.GetString("a47a6ce4-0504-412b-8e32-f3405a878609", "FWB Message cannot be sent as current consol does not have a valid AWB.");

			if (!consol.IsAWBHeaderAccessible)
			{
				SendFWBInfo.AddError(awbHeaderError);
				return;
			}

			var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;

			if (awbHeader == null)
			{
				SendFWBInfo.AddError(awbHeaderError);
				return;
			}

			using (awbHeader.TemporarilySuspendCargoSecurityValidation())
			using (awbHeader.ResumeValidationTemporarily())
			{
				awbHeader.MarkAsNeedingValidationIncludingChildren();
				awbHeader.RunPreSaveValidation();

				bool canSendMessagesWithErrors = ForwardingConfigurationRegistry.Instance.AllowUsersToSendFWBsWithErrors.Value;

				if (awbHeader.HasErrors || (!canSendMessagesWithErrors && awbHeader.HasMessageErrors))
				{
					SendFWBInfo.AddError(ErrorFWBCannotBeSentWithMessageErrors);
				}

				if (!awbHeader.HasErrors && awbHeader.HasMessageErrors && canSendMessagesWithErrors)
				{
					SendFWBInfo.AddWarning(WarningFWBContainsMessageErrors);
				}
			}
		}

		public static string ErrorFWBCannotBeSentWithMessageErrors
		{
			get { return Res.GetString("74162a76-3917-46b1-ae8b-79aa041193b7", "FWB Message Cannot Be Sent while there are Message Errors on the AWB."); }
		}

		public static string WarningFWBContainsMessageErrors
		{
			get { return Res.GetString("337ff7a8-1a34-4b4d-9444-cd851b841271", "FWB Message contains Message Errors on the AWB and may not be accepted by receiver."); }
		}

		public static string ErrorFWBCannotBeSentWithoutMAWBNumber
		{
			get { return Res.GetString("857fc89d-8d4a-484c-8d03-ce1d9bfa7057", "FWB Message Cannot Be Sent Without a Complete MAWB Number."); }
		}

		public static string ErrorFWBCannotBeSentForColoadConsols
		{
			get { return Res.GetString("c22cc7ed-0b3d-4b19-ae13-1d9ac07d59b1", "FWB message Cannot be Sent for Coload Consols."); }
		}

		public static string ErrorFWBCannotBeSentWithoutBranchHomePort
		{
			get { return Res.GetString("d0203360-a6cf-454e-91ae-758936d16c1a", "The logged in branch has no home port configured and is required for FWB."); }
		}

		public static string ErrorFWBCannotBeSentWithoutBranchHomePortIATACode
		{
			get { return Res.GetString("e5c54ea6-6317-4ef0-90a5-62f9920a2fc5", "The Home Port of your logged in Branch does not have an IATA code, which is required for the FWB message. Please add a valid IATA code (nearest airport code) to the Home Port UNLOCO of your branch."); }
		}

		#endregion

		#region SendFHL

		public override void ValidateSendFHL()
		{
			base.ValidateSendFHL();

			if (!SendFWB)
			{
				if (SendFHL)
				{
					SendFHLInfo.AddError(ErrorFHLCannotBeSentWithoutFWB);
				}
			}
			else
			{
				if (!SendFHL)
				{
					if (!consol.IsDirect && !consol.IsCoLoad && consol.AnyGoodsTravelToOrThroughFHLCountry)
					{
						SendFHLInfo.AddWarning(WarningFHLRegulationsMessage);
					}
				}
				else
				{
					if (consol.JK_MasterBillNum.Length != 11)
					{
						SendFHLInfo.AddError(ErrorFHLCannotBeSentWithoutMAWBNumber);
					}
					if (consol.IsDirect || consol.IsCoLoad)
					{
						SendFHLInfo.AddError(ErrorFHLCannotBeSentForDirectOrColoadConsols);
					}
				}
			}
		}

		public void ValidateShipmentsToSend()
		{
			ConsolAWBFetchHelper.AddFetchHintsForShipments(ShipmentsToSend);

			foreach (ForwardingShipment shipment in ShipmentsToSend)
			{
				if (!ShouldSendFHLMessageForHVLVConsignments(shipment))
				{
					shipment.PopulateAWB();

					var awbHeader = shipment.AWBHeader as ShipmentExportAWBHeader;

					using (awbHeader.ResumeValidationTemporarily())
					{
						awbHeader.MarkAsNeedingValidationIncludingChildren();
						awbHeader.RunPreSaveValidation();

						if (awbHeader.HasErrors || awbHeader.HasMessageErrors)
						{
							SendFHLInfo.AddError(ErrorFHLCannotBeSentWithMessageErrors);
							break;
						}
					}

					if (shipment.JS_HouseBill.IsEmpty)
					{
						SendFHLInfo.AddError(Res.GetString("05ebc5cf-103d-4273-83f6-8c2013f8ba9f", "FHL Message Cannot Be Sent Without a House Bill Number. Shipment ID: {0}", shipment.JS_UniqueConsignRef));
					}
				}
			}
		}

		#region RunPreSaveValidation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (SendFWB && SendFHL)
			{
				ValidateShipmentsToSend();
			}

			if (SendFWB)
			{
				ValidateACASForFWB();
			}

			if (SendFHL && !(consol.IsDirect || consol.IsCoLoad))
			{
				ValidateACASForFHL();
			}
		}

		#endregion

		public static string ErrorFHLCannotBeSentWithoutFWB
		{
			get { return Res.GetString("c4e93c96-a3f7-4ce8-ac51-891133568175", "Cannot send FHL without sending FWB."); }
		}

		public static string ErrorFHLCannotBeSentForDirectOrColoadConsols
		{
			get { return Res.GetString("a288cd95-a89a-4a7d-9fcb-a693cf2de565", "FHL message cannot be sent for Direct or Coload Consols."); }
		}

		public static string ErrorFHLCannotBeSentWithoutMAWBNumber
		{
			get { return Res.GetString("cb6afe34-4908-42fd-94c5-53201cebba71", "FHL Message Cannot Be Sent Without a Complete MAWB Number."); }
		}

		public static string WarningCargoImpMessagesHaveAlreadyBeenSent
		{
			get { return Res.GetString("5bd7d137-b522-426a-a9a3-ac98f5ca0a5e", "Cargo Interchange Messages have already been sent."); }
		}

		public static string ErrorCargoImpMessageServiceTaskIsNotActive
		{
			get { return Res.GetString("848b850a-e843-424b-8e75-5875cae7d1f2", "Cannot send FWB while CargoIMP Message Sender service task is not active."); }
		}

		public static string ErrorFHLCannotBeSentWithMessageErrors
		{
			get { return Res.GetString("70f35a04-de97-478e-b591-2d00b2ef1909", "FHL Message Cannot Be Sent while there are Message Errors on the AWB."); }
		}

		#endregion

		#region PrintConsignmentSecurityDeclaration

		public override void ValidatePrintConsignmentSecurityDeclaration()
		{
			base.ValidatePrintConsignmentSecurityDeclaration();

			if (PrintConsignmentSecurityDeclaration)
			{
				if (!consol.SupplyChainSecurityConfiguration.UseConsignmentSecurityDeclaration)
				{
					PrintConsignmentSecurityDeclarationInfo.AddError(ErrorCSDNotApplicableForUS);
				}
				else if (!SecurityStatusIsSecured())
				{
					PrintConsignmentSecurityDeclarationInfo.AddError(ErrorCSDNotApplicableForUnsecuredConsolidation);
				}
			}
		}

		static string ErrorCSDNotApplicableForUS
		{
			get { return Res.GetString("3ab06556-a790-4f5a-ae3d-cd576e6d8dac", "CSD is not applicable for United States."); }
		}

		#endregion

		#region IncludeSecurityDeclaration

		[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IncludeSecurityDeclarationReadOnly")]
		[ReadOnlyMember("IncludeSecurityDeclarationReadOnly")]
		public ZBool IncludeSecurityDeclaration
		{
			get { return includeSecurityDeclaration; }
			set
			{
				SetNonPersistentPropertyValue(IncludeSecurityDeclarationInfo, ref includeSecurityDeclaration, value);

				if (!IsValidationSuspended)
				{
					ValidateIncludeSecurityDeclaration();
				}
			}
		}

		ZBool includeSecurityDeclaration;

		public ZPropertyInfo IncludeSecurityDeclarationInfo
		{
			get { return GetZPropertyInfo(Schema.IncludeSecurityDeclaration); }
		}

		public void ValidateIncludeSecurityDeclaration()
		{
			IncludeSecurityDeclarationInfo.ClearAllNotifications();

			if (IncludeSecurityDeclaration)
			{
				var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;
				if (awbHeader != null)
				{
					if (!awbHeader.SupplyChainSecurityConfiguration.AllowIncludeECSD)
					{
						IncludeSecurityDeclarationInfo.AddError(Res.GetString("5b18e1bb-1625-4be9-b26e-79001cc7d101", "eCSD is not applicable for {0}.", GlbCompany.CurrentCompany.Country.Description));
					}
					else if (!SecurityStatusIsSecured())
					{
						IncludeSecurityDeclarationInfo.AddError(ErrorCSDNotApplicableForUnsecuredConsolidation);
					}
					else if (SecurityDeclarationHasErrors())
					{
						IncludeSecurityDeclarationInfo.AddError(Res.GetString("31a1af93-e25f-40d5-95fc-1aed8a71200f", "eCSD cannot be included while there are Message Errors on the Security Declaration."));
					}
				}
			}
		}

		public bool SecurityStatusIsSecured()
		{
			var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;
			if (awbHeader != null)
			{
				string[] securityStatuses = new string[]
				{
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
					AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly,
					AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements
				};

				return securityStatuses.Contains((string)awbHeader.EH_SecurityStatus);
			}

			return false;
		}

		bool SecurityDeclarationHasErrors()
		{
			var awbHeader = consol.AWBHeader as ConsolExportAWBHeader;
			if (awbHeader != null)
			{
				var securityValidation = awbHeader.CargoSecurityValidation;
				securityValidation.ValidateAll();
				return securityValidation.AWBHeaderSecurityProperties.Any(x => x.HasErrors() || x.HasMessageErrors())
					|| CargoSecurityLinesHasErrorsOrMessageErrors(awbHeader.CargoSecurityKnownShippers)
					|| CargoSecurityLinesHasErrorsOrMessageErrors(awbHeader.CargoSecurityScreeningMethods)
					|| CargoSecurityLinesHasErrorsOrMessageErrors(awbHeader.CargoSecurityExemptionGrounds);
			}

			return false;
		}

		bool CargoSecurityLinesHasErrorsOrMessageErrors(ExportAWBSecurityStatusLineView lines)
		{
			foreach (ExportAWBSecurityStatusLine line in lines)
			{
				line.Validation.ValidateAll();

				if (line.HasErrors || line.HasMessageErrors)
				{
					return true;
				}
			}

			return false;
		}

		public bool ShouldSendECSDByDefault
		{
			get
			{
				var supplyChainConfiguration = SupplyChainSecurityConfiguration.New(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				return supplyChainConfiguration.IncludeECSDByDefault;
			}
		}

		public static string ErrorCSDNotApplicableForUnsecuredConsolidation => Res.GetString("81fdedbc-d0d3-43cf-929f-9789a3ca0a9f", "The eCSD can only be issued for secured Consolidations – those with Security Status \"SPX\", \"SCO\" or \"SHR\".");

		#endregion

		#region DateLastSent

		public override ZString DateLastSent
		{
			get { return base.DateLastSent; }
			set
			{
				base.DateLastSent = value;

				if (!IsValidationSuspended)
				{
					ValidateSendFHL();
					ValidateSendFWB();
				}
			}
		}

		#endregion

		#region PrintBarcodeLabel

		public override void ValidatePrintBarcodeLabel()
		{
			base.ValidatePrintBarcodeLabel();
			if (PrintBarcodeLabel)
			{
				DocumentSupporterDataState docDataState = GetDocumentDataState(DocumentNames.AWBBarcodeLabels, MenuName);
				if (docDataState != null && !docDataState.IsValid)
				{
					PrintBarcodeLabelInfo.AddWarning(docDataState.ErrorMessage);
				}
			}
		}

		#endregion

		#region AWBLabelCopies

		[SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member NotPrintingBarcodeLabel")]
		[ReadOnlyMember("NotPrintingBarcodeLabel")]
		public override ZInt AWBLabelCopies
		{
			get { return base.AWBLabelCopies; }
			set { base.AWBLabelCopies = value; }
		}

		public override void ValidateAWBLabelCopies()
		{
			base.ValidateAWBLabelCopies();
			if (PrintBarcodeLabel)
			{
				if (AWBLabelCopies < 1)
				{
					AWBLabelCopiesInfo.AddError(Res.GetString("d58989d5-5fb4-4acc-a29e-c8dac51e26be", "Number of copies needs to be at least 1"));
				}
			}
		}

		#endregion

		#region PrintHAWBBarcodeLabels

		public override ZBool PrintHAWBBarcodeLabels
		{
			get { return base.PrintHAWBBarcodeLabels; }
			set
			{
				base.PrintHAWBBarcodeLabels = value;
				if (!IsValidationSuspended)
				{
					ValidateHAWBLabelPrinter();
					ValidateHAWBLabelUseEPrint();
					ValidateHAWBLabelCopies();
				}
				HAWBLabelPrinterInfo.RefreshBinding();
				HAWBLabelUseEPrintInfo.RefreshBinding();
				HAWBLabelCopiesInfo.RefreshBinding();
			}
		}

		protected bool NotPrintingHAWBBarcodeLabels
		{
			get { return !PrintHAWBBarcodeLabels; }
		}

		#endregion

		#region ValidatePrintHAWBBarcodeLabels

		public override void ValidatePrintHAWBBarcodeLabels()
		{
			base.ValidatePrintHAWBBarcodeLabels();
			if (PrintHAWBBarcodeLabels)
			{
				if (consol.IsDirect)
				{
					PrintHAWBBarcodeLabelsInfo.AddError(Res.GetString("d498df89-a8a2-4b6d-bd96-d0992fbf699f", "HAWB Barcode Labels cannot be printed for Direct Consols."));
				}
				else
				{
					DocumentSupporterDataState docDataState = GetDocumentDataState(DocumentNames.HAWBBarcodeLabels, MenuName);
					if (docDataState != null && !docDataState.IsValid)
					{
						PrintHAWBBarcodeLabelsInfo.AddWarning(docDataState.ErrorMessage);
					}
				}
			}
		}

		#endregion

		#region HAWBLabelPrinter

		[List("PrinterNames")]
		public override ZGuid HAWBLabelPrinter
		{
			get { return base.HAWBLabelPrinter; }
			set
			{
				if (!IsDeserialising || IsPrinterQueuePKValid(value))
				{
					base.HAWBLabelPrinter = value;
				}
			}
		}

		protected bool HAWBLabelPrinter_ReadOnly
		{
			get { return NotPrintingHAWBBarcodeLabels || HAWBLabelUseEPrint; }
		}

		public override void ValidateHAWBLabelPrinter()
		{
			base.ValidateHAWBLabelPrinter();
			if (!HAWBLabelPrinterInfo.ReadOnly && PrintHAWBBarcodeLabels)
			{
				MandatoryValidation.CheckEntered(HAWBLabelPrinterInfo);
				PrintQueueValidation.ValidatePrintQueue(HAWBLabelPrinterInfo);
			}
		}

		#endregion

		#region HAWBLabelUseEPrint

		[ReadOnlyMember(nameof(NotPrintingHAWBBarcodeLabels))]
		public override ZBool HAWBLabelUseEPrint
		{
			get { return base.HAWBLabelUseEPrint; }
			set
			{
				base.HAWBLabelUseEPrint = value;
				if (value)
				{
					HAWBLabelPrinter = ZGuid.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateHAWBLabelPrinter();
				}
				HAWBLabelPrinterInfo.RefreshBinding();
			}
		}

		public override void ValidateHAWBLabelUseEPrint()
		{
			base.ValidateHAWBLabelUseEPrint();
			if (!HAWBLabelUseEPrintInfo.ReadOnly && HAWBLabelUseEPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(HAWBLabelUseEPrintInfo);
			}
		}

		#endregion

		#region HAWBLabelCopies

		[ReadOnlyMember(nameof(NotPrintingHAWBBarcodeLabels))]
		public override ZInt HAWBLabelCopies
		{
			get { return base.HAWBLabelCopies; }
			set { base.HAWBLabelCopies = value; }
		}

		public override void ValidateHAWBLabelCopies()
		{
			base.ValidateHAWBLabelCopies();
			if (PrintHAWBBarcodeLabels)
			{
				if (HAWBLabelCopies < 1)
				{
					HAWBLabelCopiesInfo.AddError(Res.GetString("4ef36f11-8c6c-49da-9f88-2a88c17da0a3", "Number of copies needs to be at least 1"));
				}
			}
		}

		#endregion

		#endregion

		#region Message Processing

		public event EventHandler<ShowMessageOnGUIEventArgs> ShowMessageOnGUI;

		protected virtual void DoSendFWB()
		{
			ExecuteInEzycargoInterfaceLicence(delegate ()
			{
				if (ShouldUpdateSecurityStatusIssueDate)
				{
					consol.AWBHeader.EH_SecurityStatusIssueDate = ZDateTime.Now;
				}

				var consolDetails = new FWBMessageDetails(consol.AWBHeader as ConsolExportAWBHeader);
				var version = ShouldSendNewFWBAndFHLVersions ? FWB.Version.No16 : FWB.Version.No10;
				var fwb = new FWB(consolDetails, version, IncludeSecurityDeclaration);
				CIMEDIMessage message = consol.CIMEDIMessages.AddNew();
				message.EM_MessageText = fwb.ToString();
				message.EM_MessageType = fwb.StandardMessageIdentifier;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_ApplicationReference = consol.JK_MasterBillNum;

				CreateInterchange(message);

				AddMessageSentEventWithParameters(consol.Logs, messageType: CargoIMP.MessageTypes.FWB, referenceNumber: consol.JK_MasterBillNum);

				if (consol.JK_MasterBillIssueDate.IsEmpty)
				{
					consol.JK_MasterBillIssueDate = consol.GetAWBIssueDate();
				}

				SaveFactory();
			}, Res.GetString("bfbb1c95-a12d-45af-9782-7782d68ce425", "FWB Message"));
		}

		bool ShouldSendNewFWBAndFHLVersions
		{
			get
			{
				switch (ForwardingConfigurationRegistry.Instance.CargoImpSentMessageVersions.Value)
				{
					case CargoIMPSentMessageVersionsList.Codes.FWBv16_FHLv4:
						return true;

					case CargoIMPSentMessageVersionsList.Codes.FWBv10_FHLv2:
						return false;

					default:
						var consolAWBHeader = consol.AWBHeader as ConsolExportAWBHeader;
						return consolAWBHeader.Booking1stFlightDate >= new ZDateTime(2010, 12, 29);
				}
			}
		}

		bool ShouldSendFHLMessageForHVLVConsignments(ForwardingShipment shipment)
		{
			return shipment.IsHighVolumeLowValue && HVLVDataRegistry.Instance.EnableHVLVFHLMessaging.Value;
		}

		bool ShouldUpdateSecurityStatusIssueDate
		{
			get
			{
				return (PrintConsignmentSecurityDeclaration || IncludeSecurityDeclaration) && !consol.IsCSDValuesOverriddenProperty;
			}
		}

		protected virtual void DoSendFHL()
		{
			ExecuteInEzycargoInterfaceLicence(delegate ()
			{
				var consolDetails = new FWBMessageDetails(consol.AWBHeader as ConsolExportAWBHeader);
				foreach (ForwardingShipment shipment in ShipmentsToSend)
				{
					if (ShouldSendFHLMessageForHVLVConsignments(shipment))
					{
						SendFHLMessageForHVLVConsignments(consolDetails, shipment.HVLVConsignments);
					}
					else
					{
						var shipmentDetails = new FHLMessageDetails(shipment.AWBHeader as ShipmentExportAWBHeader);
						SendFHLMessage(consolDetails, shipmentDetails);
					}

					AddMessageSentEventWithParameters(shipment.Logs, messageType: CargoIMP.MessageTypes.FHL, referenceNumber: shipment.JS_HouseBill);
				}

				if (consol.JK_MasterBillIssueDate.IsEmpty)
				{
					consol.JK_MasterBillIssueDate = consol.GetAWBIssueDate();
				}

				SaveFactory();
			}, Res.GetString("1f4716a5-f4eb-49aa-bd16-98bdb70b0544", "FHL Message"));
		}

		void SendFHLMessage(IFWBMessageDetailsProvider fwbMessageDetails, IFHLMessageDetailsProvider fhlMessageDetails)
		{
			var version = ShouldSendNewFWBAndFHLVersions ? FHL.Version.No4 : FHL.Version.No2;
			var fhl = new FHL(fwbMessageDetails, fhlMessageDetails, version);

			var message = consol.CIMEDIMessages.AddNew();
			message.EM_MessageText = fhl.ToString();
			message.EM_MessageType = fhl.StandardMessageIdentifier;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = !fhlMessageDetails.UniqueReference.IsEmpty ? fhlMessageDetails.UniqueReference : fhlMessageDetails.HouseBill;

			if (!fhlMessageDetails.ParentTable.IsEmpty && !fhlMessageDetails.ParentID.IsEmpty)
			{
				message.EM_LinkTable = fhlMessageDetails.ParentTable;
				message.EM_LinkUniqueID = fhlMessageDetails.ParentID;
			}

			CreateInterchange(message);
		}

		void SendFHLMessageForHVLVConsignments(IFWBMessageDetailsProvider fwbMessageDetails, IEnumerable<IHVLVConsignment> consignments)
		{
			consignments.ForEach(consignment =>
			{
				var consignmentFHLMessageDetailsProvider = consignment as IHVLVConsignmentFHLMessageDetailsProvider;
				SendFHLMessage(fwbMessageDetails, consignmentFHLMessageDetailsProvider.GetFHLMessageDetailsProvider());
				consignmentFHLMessageDetailsProvider.SetLastUsageCodeForAllItems(UsageCodes.FHLAirlineMessaging);
			});
		}

		List<ForwardingShipment> ShipmentsToSend
		{
			get { return consol.Shipments.Cast<ForwardingShipment>().Where(ForwardingShipmentExtensions.IsFHLShipment).ToList(); }
		}

		void ExecuteInEzycargoInterfaceLicence(Action action, string name)
		{
			if (ServiceProviderIsHUB)
			{
				action();
				return;
			}

			if (Env.Licence.EzycargoInterface.Login(this) != LicenceLoginResponse.Denied)
			{
				try
				{
					action();
				}
				finally
				{
					Env.Licence.EzycargoInterface.Logout(this);
				}
			}
			else
			{
				if (ShowMessageOnGUI != null)
				{
					ShowMessageOnGUI(this, new ShowMessageOnGUIEventArgs(name, Env.Licence.EzycargoInterface.LastReasonForNotAllowing));
				}
			}
		}

		void SaveFactory()
		{
			try
			{
				if (!consol.JK_OverrideWaybillDefaults)
				{
					AWB.ForceSavingByFactory = true;
				}

				bool isWorkflowAndTemplateApplicationDeferred = WorkflowDataRegistry.Instance.DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave.Value;

				using (Logs.DeferFiringWorkflow(isWorkflowAndTemplateApplicationDeferred))
				using (ProcessTask.Loader.SuppressTemplateApplication(isWorkflowAndTemplateApplicationDeferred))
				{
					consol.Factory.Save();
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AWB.ForceSavingByFactory = false;
			}
		}

		void CreateInterchange(CIMEDIMessage message)
		{
			CIMEDIInterchange interchange = consol.Factory.New<CIMEDIInterchange>();

			interchange.EI_HeaderText = ZString.Empty;      // this is dependent on the transmission method (SMTP/FTP) so is set in the sender processor
			interchange.EI_BodyText = message.EM_MessageText;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = CIMEDIInterchange.GetERouterFromCode();
			interchange.EI_To = CIMEDIInterchange.GetERouterToCode();

			message.EM_EI = interchange.PK;
		}

		#endregion

		#region Implementation

		public ForwardingConsol Consol { get { return consol; } }

		public override ZString ParentName
		{
			get { return Res.GetString("4d82456c-99a6-45f6-b0f3-9c197b4d5b92", "Consol"); }
		}

		internal string WarningFHLRegulationsMessage
		{
			get
			{
				var result = new StringBuilder(Res.GetString("901058bb-ba70-401a-ae6e-32c54b421381", "Regulations in the following countries/regions specify that it is mandatory to send all Cargo Interchange Messages when goods are routed through the listed countries/regions:"));
				result.Append(System.Environment.NewLine);
				foreach (CodeDescriptionPair country in consol.FHLCountries)
				{
					result.Append(" - ").Append(country.Description).Append(System.Environment.NewLine);
				}

				result.Append(Res.GetString("c1df6724-5600-40ad-9b3d-91d1a9ad899f", "- European Union countries/regions")).Append(System.Environment.NewLine);
				return result.ToString();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PrintNeutralAWBsAsLaser = true;
		}

		protected override void SetAdditionalDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				LoadSettings();

				if (consol.JK_IsNeutralMaster || consol.IsCoLoad)
				{
					LaserAWB = PrintNeutralAWBsAsLaser;
					NeutralAWB = !LaserAWB;
					CarrierAWB = false;
				}
				else
				{
					CarrierAWB = true;
					NeutralAWB = false;
					LaserAWB = false;
				}

				SixInchLabel = !FiveInchLabel;

				ParentPackagesLabel = !AWBPackagesLabel;

				if (ActionsMode == ActionsModeType.LabelsOnly)
				{
					PrintMasterAirWaybill = false;
					SendFHL = false;
					SendFWB = false;
					PrintBarcodeLabel = true;
					PrintHAWBBarcodeLabels = false;
				}
				else if (ActionsMode == ActionsModeType.All)
				{
					PrintMasterAirWaybill = true;

					DatePrinted = consol.FinalMAWBPrintedDate.IsEmpty ? "" : consol.FinalMAWBPrintedDate.ToLongTimeString();
					CIMEDIMessage message = consol.CIMEDIMessages.GetLatestTransmittedMessage();
					DateLastSent = (message == null) ? "" : message.EM_StatusDateTime.ToLongTimeString();
					SendFWB = (DateLastSent.Length == 0) && !consol.IsCoLoad && !CargoIMPMessageSenderServiceTaskIsInActiveOrTurnOff;

					if (Env.Registry.Freight.AirWaybill.SelectFHLByDefault)
					{
						SendFHL = !consol.IsDirect && SendFWB;
					}
					else
					{
						SendFHL = !consol.IsDirect && SendFWB && consol.AnyGoodsTravelToOrThroughFHLCountry;
					}
				}

				LabelRangeFrom = 1;

				AWBLabelCopies = 1;
				HAWBLabelCopies = 1;

				ValidatePrintBarcodeLabel();
				ValidatePrintHAWBBarcodeLabels();

				ValidateLabelPrinter();
				ValidateMAWBPrinter();
				ValidateHAWBLabelPrinter();
			}
		}

		protected override ExportAWBHeader AWB
		{
			get { return consol.AWBHeader; }
		}

		protected override IDocumentSupportable DocumentSupportable
		{
			get { return consol; }
		}

		protected override ZInt PackagesNumber
		{
			get { return (ZInt)consol.JK_TotalShipmentQuantity; }
		}

		public string FindFirstInvalidCreditControlledDocumentName()
		{
			return GetDocumentNamesToPrint().FirstOrDefault(doc =>
			{
				var dataState = GetDocumentDataState(doc, MenuName);
				return dataState != null && !dataState.IsValid;
			});
		}

		public DocumentCommand GetDocumentCommand(string documentName)
		{
			return CreateDocumentCommand(documentName);
		}

		protected virtual bool ServiceProviderIsHUB
		{
			get
			{
				return
					ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty) ==
					Core.Constants.AWB.CargoIMPServiceProviderConstants.HUB;
			}
		}

		#endregion

		#region ILicensedComponent Members

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get
			{
				return this.licensedComponentManager ?? (this.licensedComponentManager = new LicensedComponentManager(this));
			}
		}
		LicensedComponentManager licensedComponentManager;

		#endregion

		#region CargoIMP Message Sender Service Task

		protected virtual bool CargoIMPMessageSenderServiceTaskIsInActiveOrTurnOff
		{
			get
			{
				return CargoIMPMessageSenderServiceTaskStatus == ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb || CargoIMPMessageSenderServiceTaskStatus == ServiceTaskStatus.ServiceTaskIsInactive;
			}
		}

		ServiceTaskStatus? CargoIMPMessageSenderServiceTaskStatus
		{
			get
			{
				if (cargoIMPMessageSenderServiceTaskStatus == null)
				{
					cargoIMPMessageSenderServiceTaskStatus = ObjectFactory.Get<IServiceManagerQuerier>().CheckStateOfNamedServiceTask(FreightConstants.ServiceTask.CargoIMPMessageSender);
				}
				return cargoIMPMessageSenderServiceTaskStatus;
			}
		}
		ServiceTaskStatus? cargoIMPMessageSenderServiceTaskStatus;

		#endregion
	}
}
