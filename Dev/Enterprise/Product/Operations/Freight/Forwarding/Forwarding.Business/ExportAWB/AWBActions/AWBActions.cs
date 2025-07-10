using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public abstract class AWBActions : AutoAWBActions, IAWBActionsSerializable
	{
		#region Constants

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document Menu Name")]
		public static class DocumentNames
		{
			public const string CarrierMAWB = "Carrier MAWB";
			public const string NeutralMAWB = "Neutral MAWB";
			public const string LaserMAWB = "Laser MAWB";
			public const string AWBBarcodeLabels = "AWB Barcode Label";
			public const string HAWBBarcodeLabels = "HAWB Barcode Label 5 Inch";
			public const string ConsignmentSecurityDeclaration = "Consignment Security Declaration";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special Code")]
		public static class DocumentSizes
		{
			public const string FiveInch = "5 Inch";
			public const string SixInch = "6 Inch";
		}

		public virtual string MenuName
		{
			get { return "AWB"; } // Document Menu Name
		}

		#endregion

		#region Ctor

		protected AWBActions(ActionsModeType actionsMode, BusinessObjectFactory factory)
			: base(factory)
		{
			this.ActionsMode = actionsMode;
		}

		#endregion

		readonly protected ActionsModeType ActionsMode;

		#region Actions

		public enum ActionsModeType
		{
			None,
			All,
			LabelsOnly
		}

		public virtual void PrintAWBBarcodeLabel()
		{
			AWB.DocumentSize = FiveInchLabel ? DocumentSizes.FiveInch : DocumentSizes.SixInch;
			DoPrintBarcodeLabel(DocumentNames.AWBBarcodeLabels);
		}

		internal void SetDocumentSettings()
		{
			SetDocumentSettingsCore();
			AWB.DocumentSettingsPopulated = true;
		}

		protected virtual void SetDocumentSettingsCore()
		{
			AWB.DocumentSize = FiveInchLabel ? DocumentSizes.FiveInch : DocumentSizes.SixInch;
			AWB.LabelStartRange = AWB.MAWBLabelStartRange = LabelRangeFrom;
			AWB.LabelEndRange = LabelRangeTo;
			AWB.LabelTotalPacks = AWB.MAWBLabelTotalPacks = TotalPacks;
			AWB.PrintOptionalInformation = PrintOptionalInformation;
		}

		protected virtual void DoPrintBarcodeLabel(string documentName)
		{
			SetDocumentSettings();

			PrintDocument(documentName, LabelPrinter, LabelUseEPrint);
		}

		#endregion

		#region IAWBActionsSerializable

		AWBActionsSerializationHelper SerializationHelper
		{
			get { return serializationHelper ?? (serializationHelper = new AWBActionsSerializationHelper(this)); }
		}
		AWBActionsSerializationHelper serializationHelper;

		const string settingsCacheName = "AWBActionsSettings";

		[SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces", Justification = "All overriding methods verified safe")]
		public virtual string SettingsCacheName
		{
			get { return settingsCacheName; }
		}

		public void LoadSettings()
		{
			SerializationHelper.LoadSettings();
		}

		public void SaveSettings()
		{
			if (NeutralAWB)
			{
				PrintNeutralAWBsAsLaser = false;
			}
			else if (LaserAWB)
			{
				PrintNeutralAWBsAsLaser = true;
			}

			SerializationHelper.SaveSettings();
		}

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.WriteXml(XmlWriter xmlWriter)
		{
			SerializationHelper.WriteXml(xmlWriter);
		}

		void IXmlSerializable.ReadXml(XmlReader xmlReader)
		{
			IsDeserialising = true;

			try
			{
				SerializationHelper.ReadXml(xmlReader);
			}
			finally
			{
				IsDeserialising = false;
			}
		}

		protected bool IsDeserialising { get; private set; }

		List<ZPropertyInfo> IAWBActionsSerializable.GetPropertiesToSerialize()
		{
			return GetPropertiesToSerializeCore();
		}

		protected virtual List<ZPropertyInfo> GetPropertiesToSerializeCore()
		{
			var result = new List<ZPropertyInfo>();
			result.Add(PrintBarcodeLabelInfo);
			result.Add(FiveInchLabelInfo);
			result.Add(PrintOptionalInformationInfo);
			result.Add(LabelPrinterInfo);
			result.Add(LabelUseEPrintInfo);
			result.Add(PrintNeutralAWBsAsLaserInfo);
			result.Add(MAWBPrinterInfo);
			result.Add(MAWBUseEPrintInfo);
			result.Add(PrintHAWBBarcodeLabelsInfo);
			result.Add(HAWBLabelPrinterInfo);
			result.Add(HAWBLabelUseEPrintInfo);
			result.Add(AWBPackagesLabelInfo);
			return result;
		}

		#endregion

		#region Properties

		public string OverrideMenuPath { get; set; }

		#region PrintBarcodeLabel

		public override ZBool PrintBarcodeLabel
		{
			get { return base.PrintBarcodeLabel; }
			set
			{
				base.PrintBarcodeLabel = value;
				if (!IsValidationSuspended)
				{
					ValidateTotalPacks();
					ValidateLabelPrinter();
					ValidateLabelUseEPrint();
					ValidateLabelRangeFrom();
					ValidateLabelRangeTo();
					ValidateAWBLabelCopies();
				}
				FiveInchLabelInfo.RefreshBinding();
				SixInchLabelInfo.RefreshBinding();
				LabelPrinterInfo.RefreshBinding();
				LabelUseEPrintInfo.RefreshBinding();
				LabelRangeFromInfo.RefreshBinding();
				LabelRangeToInfo.RefreshBinding();
				AWBLabelCopiesInfo.RefreshBinding();
			}
		}

		protected bool PrintBarcodeLabel_ReadOnly
		{
			get { return ActionsMode == ActionsModeType.LabelsOnly; }
		}

		protected bool NotPrintingBarcodeLabel
		{
			get { return !PrintBarcodeLabel; }
		}

		#endregion

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZBool FiveInchLabel
		{
			get { return base.FiveInchLabel; }
			set { base.FiveInchLabel = value; }
		}

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZBool SixInchLabel
		{
			get { return base.SixInchLabel; }
			set { base.SixInchLabel = value; }
		}

		public override ZBool AWBPackagesLabel
		{
			get { return base.AWBPackagesLabel; }
			set
			{
				base.AWBPackagesLabel = value;
				if (value)
				{
					LabelRangeFrom = 1;
					TotalPacks = LabelRangeTo = AWB.EH_TotalNoOfPieces;
				}
			}
		}
		public override ZBool ParentPackagesLabel
		{
			get { return base.ParentPackagesLabel; }
			set
			{
				base.ParentPackagesLabel = value;
				if (value)
				{
					LabelRangeFrom = 1;
					TotalPacks = LabelRangeTo = PackagesNumber;
				}
			}
		}

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZInt LabelRangeFrom
		{
			get { return base.LabelRangeFrom; }
			set
			{
				base.LabelRangeFrom = value;
				if (!IsValidationSuspended)
				{
					ValidateLabelRangeTo();
				}
			}
		}

		public override void ValidateLabelRangeFrom()
		{
			base.ValidateLabelRangeFrom();
			if (PrintBarcodeLabel)
			{
				if (LabelRangeFrom < 1)
				{
					LabelRangeFromInfo.AddError(Res.GetString("1b129190-a239-42af-9483-dfc33bd0f42a", "Start range needs to be greater or equal to 1"));
				}
				else if (LabelRangeFrom > TotalPacks)
				{
					LabelRangeFromInfo.AddError(Res.GetString("a55d1853-64af-4e44-866d-7957ff7805d9", "Start range needs to be less or equal to the total number of  labels: {0}", TotalPacks));
				}
			}
		}

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZInt LabelRangeTo
		{
			get { return base.LabelRangeTo; }
			set { base.LabelRangeTo = value; }
		}

		public override void ValidateLabelRangeTo()
		{
			base.ValidateLabelRangeTo();
			if (PrintBarcodeLabel)
			{
				if (LabelRangeTo < 1)
				{
					LabelRangeToInfo.AddError(Res.GetString("f7e76c68-4673-4a8c-933d-004fe86dac0b", "End range needs to be greater or equal to 1"));
				}
				else if (LabelRangeTo > TotalPacks)
				{
					LabelRangeToInfo.AddError(Res.GetString("879f0101-e4ab-4601-95fa-c64fb3ca0131", "End range needs to be less or equal to the total number of labels: {0}", TotalPacks));
				}
				else if (LabelRangeTo < LabelRangeFrom)
				{
					LabelRangeToInfo.AddError(Res.GetString("96b2ed24-bc7e-4efb-847c-4812dd67676a", "End range needs to be greater or equal to the start range"));
				}
			}
		}

		public override void ValidateTotalPacks()
		{
			base.ValidateTotalPacks();
			if (PrintBarcodeLabel)
			{
				if (TotalPacks < 1)
				{
					TotalPacksInfo.AddError(Res.GetString("52fcb177-d37c-48ed-ab4d-263221ddfcdc", "Total Packs cannot be less than 1"));
				}
				else if (AWBPackagesLabel && TotalPacks > AWB.EH_TotalNoOfPieces)
				{
					TotalPacksInfo.AddError(Res.GetString("9c396bd6-12b9-4bd3-89f6-f14db756a8f9", "Total Packs should be less than total Packs on the AWB: {0}", AWB.EH_TotalNoOfPieces));
				}
				else if (ParentPackagesLabel && TotalPacks > PackagesNumber)
				{
					TotalPacksInfo.AddError(Res.GetString("9a9b80e5-4111-459d-99c6-9faa8568dfa5", "Total Packs should be less than total Outer Packs: {0}", PackagesNumber));
				}
			}
			ValidateLabelRangeFrom();
			ValidateLabelRangeTo();
		}

		[List("PrinterNames")]
		public override ZGuid LabelPrinter
		{
			get { return base.LabelPrinter; }
			set
			{
				if (!IsDeserialising || IsPrinterQueuePKValid(value))
				{
					base.LabelPrinter = value;
				}
			}
		}

		protected bool LabelPrinter_ReadOnly
		{
			get { return NotPrintingBarcodeLabel || LabelUseEPrint; }
		}

		public override void ValidateLabelPrinter()
		{
			base.ValidateLabelPrinter();
			if (!LabelPrinterInfo.ReadOnly && PrintBarcodeLabel)
			{
				MandatoryValidation.CheckEntered(LabelPrinterInfo);
				PrintQueueValidation.ValidatePrintQueue(LabelPrinterInfo);
			}
		}

		#region LabelUseEPrint

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZBool LabelUseEPrint
		{
			get { return base.LabelUseEPrint; }
			set
			{
				base.LabelUseEPrint = value;
				if (value)
				{
					LabelPrinter = ZGuid.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateLabelPrinter();
				}
				LabelPrinterInfo.RefreshBinding();
			}
		}

		public override void ValidateLabelUseEPrint()
		{
			base.ValidateLabelUseEPrint();
			if (!LabelUseEPrintInfo.ReadOnly && LabelUseEPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(LabelUseEPrintInfo);
			}
		}

		#endregion

		[ReadOnlyMember(nameof(NotPrintingBarcodeLabel))]
		public override ZBool PrintOptionalInformation
		{
			get { return base.PrintOptionalInformation; }
			set { base.PrintOptionalInformation = value; }
		}

		public CodeDescriptionPairList PrinterNames
		{
			get
			{
				if (fPrinters == null)
				{
					fPrinters = new DocDeliveryPrintDetails(Factory);
				}
				return fPrinters.PrinterNames;
			}
		}
		DocDeliveryPrintDetails fPrinters;

		#endregion

		#region Implementation

		protected string[] GetDocumentNamesToPrint()
		{
			List<string> documentNames = new List<string>();

			if (PrintMasterAirWaybill)
			{
				if (CarrierAWB)
				{
					documentNames.Add(DocumentNames.CarrierMAWB);
				}
				else if (LaserAWB)
				{
					documentNames.Add(DocumentNames.LaserMAWB);
				}
				else
				{
					documentNames.Add(DocumentNames.NeutralMAWB);
				}
			}

			if (PrintConsignmentSecurityDeclaration)
			{
				documentNames.Add(DocumentNames.ConsignmentSecurityDeclaration);
			}

			if (PrintBarcodeLabel)
			{
				documentNames.Add(DocumentNames.AWBBarcodeLabels);
			}

			if (PrintHAWBBarcodeLabels)
			{
				documentNames.Add(DocumentNames.HAWBBarcodeLabels);
			}

			return documentNames.ToArray();
		}

		protected override void SetDefaultValues()
		{
			PrintBarcodeLabel = true;
			FiveInchLabel = true;
			PrintOptionalInformation = true;

			base.SetDefaultValues();
		}

		protected virtual void SetAdditionalDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				LoadSettings();

				ParentPackagesLabel = !AWBPackagesLabel;

				if (ActionsMode == ActionsModeType.LabelsOnly)
				{
					PrintBarcodeLabel = true;
				}

				SixInchLabel = !FiveInchLabel;

				LabelRangeFrom = 1;

				ValidateHAWBLabelPrinter();
			}
		}

		protected bool IsPrinterQueuePKValid(ZGuid printerQueuePK)
		{
			return printerQueuePK.IsValid && PrinterNames.Cast<CodeElement>().Any(element => (ZGuid)element.PK == printerQueuePK);
		}

		protected void PrintDocument(string documentName, ZGuid printer, ZBool useEPrint, DocDeliveryContact overrideContact = null)
		{
			DocumentCommand documentCommand = CreateDocumentCommand(documentName);

			using (var set = new DocumentPrintSet(documentCommand, new UserControlProviderList()))
			{
				var instructions = new DeliveryInstructions();
				instructions.AllowAutoDelivery = false;

				var destination = DeliveryInstructionDestination.Print;

				if (useEPrint)
				{
					destination = DeliveryInstructionDestination.TakenFromContact;
					var contact = new DocDeliveryContact(Factory);
					contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
					contact.AttachmentType = AttachmentTypeList.Codes.Tif;
					instructions.Recipients.Add(contact);
				}
				else
				{
					if (overrideContact != null)
					{
						instructions.Recipients.Add(overrideContact);
					}

					instructions.PrinterDelivery.PrintQueuePK = printer;
				}

#if DEBUG
				instructions.Destination = Globals.IsTest ? destination : DeliveryInstructionDestination.Preview;
#else
				instructions.Destination = destination;
#endif
				set.Run(instructions);
			}
		}

		protected DocumentCommand CreateDocumentCommand(string documentName)
		{
			DocumentZQuery filter;
			DocumentCommand documentCommand = null;

			if (OverrideMenuPath != null)
			{
				filter = new DocumentZQuery(true);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuName, documentName);
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, DocumentSupportable.DocumentSupporter.BusinessContext);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, OverrideMenuPath);

				documentCommand = FindFirstApplicableDocumentCommand(filter);
			}

			if (documentCommand == null)
			{
				// Fallback to catch-all MenuName if OverrideMenuName doesn't return a document
				filter = new DocumentZQuery(true);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuName, documentName);
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, DocumentSupportable.DocumentSupporter.BusinessContext);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, MenuName);

				documentCommand = FindFirstApplicableDocumentCommand(filter);
			}

			if (documentCommand == null)
			{
				ErrorReporter.ReportOnce("WI00449720-ObjectRefereceNull",
					$"Can't create document command. Parameters: documentName:{documentName}, OverrideMenuPath:{OverrideMenuPath}, SU_BusinessContext:{DocumentSupportable.DocumentSupporter.BusinessContext}.");
			}

			return documentCommand;
		}

		DocumentCommand FindFirstApplicableDocumentCommand(DocumentZQuery filter)
		{
			var docCommands = Factory.Load<DocumentCommand>(filter);
			return docCommands.FirstOrDefault(d =>
			{
				d.Parent = DocumentSupportable;
				return d.IsApplicable;
			});
		}

		protected abstract ExportAWBHeader AWB { get; }
		protected abstract IDocumentSupportable DocumentSupportable { get; }
		protected abstract ZInt PackagesNumber { get; }

		#endregion
	}
}
