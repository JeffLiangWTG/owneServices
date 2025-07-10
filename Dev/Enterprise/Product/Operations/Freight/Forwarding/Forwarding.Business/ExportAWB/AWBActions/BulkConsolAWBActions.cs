using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class BulkConsolAWBActions : NonPersistentBusinessObject, IObsoleteValidation, IPrintMAWB, ISendCIMP, IAWBActionsSerializable
	{
		#region Schema

		public static class Schema
		{
			public const string PrintMasterAirWaybill = "PrintMasterAirWaybill";
			public const string PrintConsignmentSecurityDeclaration = "PrintConsignmentSecurityDeclaration";
			public const string NeutralAWB = "NeutralAWB";
			public const string CarrierAWB = "CarrierAWB";
			public const string LaserAWB = "LaserAWB";
			public const string MAWBPrinter = "MAWBPrinter";
			public const string MAWBUseEPrint = "MAWBUseEPrint";
			public const string SendFWB = "SendFWB";
			public const string SendFHL = "SendFHL";
			public const string IncludeSecurityDeclaration = "IncludeSecurityDeclaration";
			public const string AllowPrintWithMessageErrors = "AllowPrintWithMessageErrors";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "All overriding methods verified safe")]
		public BulkConsolAWBActions(BusinessObjectFactory factory)
			: base(factory)
		{
			SetAdditionalDefaults();
		}

		void SetAdditionalDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				NeutralAWB = ZBool.True;
				LoadSettings();
			}
		}

		#region PrintMasterAirWaybill

		public virtual ZBool PrintMasterAirWaybill
		{
			get { return printMasterAirWaybill; }
			set
			{
				SetNonPersistentPropertyValue(PrintMasterAirWaybillInfo, ref printMasterAirWaybill, value);
				if (!IsValidationSuspended)
				{
					ValidatePrintMasterAirWaybill();
					ValidateSendFWB();
				}

				if (PrintMasterAirWaybill == ZBool.False)
				{
					AllowPrintWithMessageErrors = ZBool.False;
					MAWBPrinter = ZGuid.Empty;
				}
			}
		}
		ZBool printMasterAirWaybill = ZBool.True;

		public virtual ZPropertyInfo PrintMasterAirWaybillInfo
		{
			get { return this.GetZPropertyInfo(Schema.PrintMasterAirWaybill); }
		}

		public virtual void ValidatePrintMasterAirWaybill()
		{
			PrintMasterAirWaybillInfo.ClearAllNotifications();
			if (!PrintMasterAirWaybill && !SendFWB)
			{
				PrintMasterAirWaybillInfo.AddError(ErrorMustPrintMAWBOrSendFWB);
			}
		}

		#endregion

		#region PrintConsignmentSecurityDeclaration

		public ZBool PrintConsignmentSecurityDeclaration
		{
			get
			{
				return printConsignmentSecurityDeclaration;
			}
			set
			{
				SetNonPersistentPropertyValue(PrintConsignmentSecurityDeclarationInfo, ref printConsignmentSecurityDeclaration, value);
			}
		}
		ZBool printConsignmentSecurityDeclaration = ZBool.False;

		public virtual ZPropertyInfo PrintConsignmentSecurityDeclarationInfo
		{
			get { return this.GetZPropertyInfo(Schema.PrintConsignmentSecurityDeclaration); }
		}

		#endregion

		#region NeutralAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public virtual ZBool NeutralAWB
		{
			get { return neutralAWB; }
			set
			{
				SetNonPersistentPropertyValue(NeutralAWBInfo, ref neutralAWB, value);
				if (!IsValidationSuspended)
				{
					ValidateNeutralAWB();
				}
			}
		}
		ZBool neutralAWB;

		public virtual ZPropertyInfo NeutralAWBInfo
		{
			get { return this.GetZPropertyInfo(Schema.NeutralAWB); }
		}

		public virtual void ValidateNeutralAWB()
		{
		}

		#endregion

		#region CarrierAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public virtual ZBool CarrierAWB
		{
			get { return carrierAWB; }
			set
			{
				SetNonPersistentPropertyValue(CarrierAWBInfo, ref carrierAWB, value);
				if (!IsValidationSuspended)
				{
					ValidateCarrierAWB();
				}
			}
		}
		ZBool carrierAWB;

		public virtual ZPropertyInfo CarrierAWBInfo
		{
			get { return this.GetZPropertyInfo(Schema.CarrierAWB); }
		}

		public virtual void ValidateCarrierAWB()
		{
		}

		#endregion

		#region LaserAWB

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public virtual ZBool LaserAWB
		{
			get { return laserAWB; }
			set
			{
				SetNonPersistentPropertyValue(LaserAWBInfo, ref laserAWB, value);
				if (!IsValidationSuspended)
				{
					ValidateLaserAWB();
				}
			}
		}
		ZBool laserAWB;

		public virtual ZPropertyInfo LaserAWBInfo
		{
			get { return this.GetZPropertyInfo(Schema.LaserAWB); }
		}

		public virtual void ValidateLaserAWB()
		{
		}

		#endregion

		#region MAWBPrinter

		[List("PrinterNames")]
		public virtual ZGuid MAWBPrinter
		{
			get { return mawbPrinter; }
			set
			{
				SetNonPersistentPropertyValue(MAWBPrinterInfo, ref mawbPrinter, value);
				if (!IsValidationSuspended)
				{
					ValidateMAWBPrinter();
				}
			}
		}
		ZGuid mawbPrinter;

		public virtual ZPropertyInfo MAWBPrinterInfo
		{
			get { return this.GetZPropertyInfo(Schema.MAWBPrinter); }
		}

		protected bool MAWBPrinter_ReadOnly
		{
			get { return NotPrintingMasterAirWayBill || MAWBUseEPrint; }
		}

		public virtual void ValidateMAWBPrinter()
		{
			MAWBPrinterInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(MAWBPrinterInfo);

			if (!MAWBPrinterInfo.ReadOnly && PrintMasterAirWaybill)
			{
				MandatoryValidation.CheckEntered(MAWBPrinterInfo);
				PrintQueueValidation.ValidatePrintQueue(MAWBPrinterInfo);
			}
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

		protected bool NotPrintingMasterAirWayBill
		{
			get { return !PrintMasterAirWaybill; }
		}

		#endregion

		#region MAWBUseEPrint

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public virtual ZBool MAWBUseEPrint
		{
			get { return mawbUseEPrint; }
			set
			{
				SetNonPersistentPropertyValue(MAWBUseEPrintInfo, ref mawbUseEPrint, value);
				if (value)
				{
					MAWBPrinter = ZGuid.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateMAWBUseEPrint();
				}
				MAWBPrinterInfo.RefreshBinding();
			}
		}
		ZBool mawbUseEPrint;

		public virtual ZPropertyInfo MAWBUseEPrintInfo
		{
			get { return this.GetZPropertyInfo(Schema.MAWBUseEPrint); }
		}

		public virtual void ValidateMAWBUseEPrint()
		{
			MAWBUseEPrintInfo.ClearAllNotifications();
			if (!MAWBUseEPrintInfo.ReadOnly && MAWBUseEPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(MAWBUseEPrintInfo);
			}
		}

		#endregion

		#region DatePrinted

		public ZString DatePrinted
		{
			get { return ZString.Empty; }
			set { }
		}

		#endregion

		#region SendFWB

		public virtual ZBool SendFWB
		{
			get { return sendFWB; }
			set
			{
				SetNonPersistentPropertyValue(SendFWBInfo, ref sendFWB, value);
				if (!IsValidationSuspended)
				{
					ValidateSendFWB();
					ValidateSendFHL();
					ValidatePrintMasterAirWaybill();
				}

				if (!value)
				{
					IncludeSecurityDeclaration = false;
				}
			}
		}
		ZBool sendFWB;

		public virtual ZPropertyInfo SendFWBInfo
		{
			get { return this.GetZPropertyInfo(Schema.SendFWB); }
		}

		public virtual void ValidateSendFWB()
		{
			SendFWBInfo.ClearAllNotifications();
			if (!PrintMasterAirWaybill && !SendFWB)
			{
				SendFWBInfo.AddError(ErrorMustPrintMAWBOrSendFWB);
			}
		}

		#endregion

		#region SendFHL

		public virtual ZBool SendFHL
		{
			get { return sendFHL; }
			set
			{
				SetNonPersistentPropertyValue(SendFHLInfo, ref sendFHL, value);
				if (!IsValidationSuspended)
				{
					ValidateSendFHL();
				}
			}
		}
		ZBool sendFHL;

		public virtual ZPropertyInfo SendFHLInfo
		{
			get { return this.GetZPropertyInfo(Schema.SendFHL); }
		}

		public virtual void ValidateSendFHL()
		{
			SendFHLInfo.ClearAllNotifications();
			if (!SendFWB && SendFHL)
			{
				SendFHLInfo.AddError(ConsolAWBActions.ErrorFHLCannotBeSentWithoutFWB);
			}
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

		public virtual void ValidateIncludeSecurityDeclaration()
		{
			IncludeSecurityDeclarationInfo.ClearAllNotifications();
		}

		#endregion

		#region DateLastSent

		public ZString DateLastSent
		{
			get { return ZString.Empty; }
			set { }
		}

		#endregion

		#region AllowPrintWithMessageErrors

		[ReadOnlyMember(nameof(NotPrintingMasterAirWayBill))]
		public virtual ZBool AllowPrintWithMessageErrors
		{
			get { return allowPrintWithMessageErrors; }
			set
			{
				SetNonPersistentPropertyValue(AllowPrintWithMessageErrorsInfo, ref allowPrintWithMessageErrors, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowPrintWithMessageErrors();
				}
			}
		}
		ZBool allowPrintWithMessageErrors;

		public virtual ZPropertyInfo AllowPrintWithMessageErrorsInfo
		{
			get { return this.GetZPropertyInfo(Schema.AllowPrintWithMessageErrors); }
		}

		public virtual void ValidateAllowPrintWithMessageErrors()
		{
		}

		#endregion

		static string ErrorMustPrintMAWBOrSendFWB
		{
			get { return Res.GetString("37c4f0f1-1cae-4e49-b462-3ec91ac5da78", "Print Final Master or Send FWB must be selected"); }
		}

		#region ValidateAll

		public virtual void ValidateAll()
		{
			ValidatePrintMasterAirWaybill();
			ValidateMAWBPrinter();
			ValidateMAWBUseEPrint();
			ValidateSendFWB();
			ValidateSendFHL();
			ValidateIncludeSecurityDeclaration();
		}

		#endregion

		#region IAWBActionsSerializable

		AWBActionsSerializationHelper SerializationHelper
		{
			get { return serializationHelper ?? (serializationHelper = new AWBActionsSerializationHelper(this)); }
		}
		AWBActionsSerializationHelper serializationHelper;

		const string settingsCacheName = "BulkConsolAWBActions";
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
			SerializationHelper.ReadXml(xmlReader);
		}

		List<ZPropertyInfo> IAWBActionsSerializable.GetPropertiesToSerialize()
		{
			var result = new List<ZPropertyInfo>();
			result.Add(PrintMasterAirWaybillInfo);
			result.Add(MAWBPrinterInfo);
			result.Add(MAWBUseEPrintInfo);
			result.Add(NeutralAWBInfo);
			result.Add(CarrierAWBInfo);
			result.Add(LaserAWBInfo);
			result.Add(SendFWBInfo);
			result.Add(SendFHLInfo);
			return result;
		}

		#endregion
	}
}
