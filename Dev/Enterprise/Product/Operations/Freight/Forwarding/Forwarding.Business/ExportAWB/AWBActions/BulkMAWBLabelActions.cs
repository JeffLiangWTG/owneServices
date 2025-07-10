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
	public class BulkMAWBLabelActions : NonPersistentBusinessObject, IObsoleteValidation, IAWBActionsSerializable
	{
		#region Schema

		public static class Schema
		{
			public const string PrintOptionalInformation = "PrintOptionalInformation";
			public const string FiveInchLabel = "FiveInchLabel";
			public const string SixInchLabel = "SixInchLabel";
			public const string PackagesFromAWB = "PackagesFromAWB";
			public const string PackagesFromConsol = "PackagesFromConsol";
			public const string NumberOfCopies = "NumberOfCopies";
			public const string AWBLabelPrinter = "AWBLabelPrinter";
			public const string AWBLabelUseEPrint = "AWBLabelUseEPrint";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public BulkMAWBLabelActions(BusinessObjectFactory factory)
			: base(factory)
		{
			SetAdditionalDefaults();
		}

		void SetAdditionalDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				FiveInchLabel = ZBool.True;
				PackagesFromAWB = ZBool.True;
				NumberOfCopies = 1;

				LoadSettings();
			}
		}

		#region PrintOptionalInformation

		public virtual ZBool PrintOptionalInformation
		{
			get { return printOptionalInformation; }
			set
			{
				SetNonPersistentPropertyValue(PrintOptionalInformationInfo, ref printOptionalInformation, value);
				if (!IsValidationSuspended)
				{
					ValidatePrintOptionalInformation();
				}
			}
		}
		ZBool printOptionalInformation;

		public virtual ZPropertyInfo PrintOptionalInformationInfo
		{
			get { return this.GetZPropertyInfo(Schema.PrintOptionalInformation); }
		}

		public virtual void ValidatePrintOptionalInformation()
		{
		}

		#endregion

		#region FiveInchLabel

		public virtual ZBool FiveInchLabel
		{
			get { return fiveInchLabel; }
			set
			{
				SetNonPersistentPropertyValue(FiveInchLabelInfo, ref fiveInchLabel, value);
				if (!IsValidationSuspended)
				{
					ValidateFiveInchLabel();
					ValidateSixInchLabel();
				}
			}
		}
		ZBool fiveInchLabel;

		public virtual ZPropertyInfo FiveInchLabelInfo
		{
			get { return this.GetZPropertyInfo(Schema.FiveInchLabel); }
		}

		public void ValidateFiveInchLabel()
		{
			FiveInchLabelInfo.ClearAllNotifications();
			if (!FiveInchLabel && !SixInchLabel)
			{
				FiveInchLabelInfo.AddError(ErrorFiveOrSixInchMandatory);
			}
		}

		#endregion

		#region SixInchLabel

		public virtual ZBool SixInchLabel
		{
			get { return sixInchLabel; }
			set
			{
				SetNonPersistentPropertyValue(SixInchLabelInfo, ref sixInchLabel, value);
				if (!IsValidationSuspended)
				{
					ValidateFiveInchLabel();
					ValidateSixInchLabel();
				}
			}
		}
		ZBool sixInchLabel;

		public virtual ZPropertyInfo SixInchLabelInfo
		{
			get { return this.GetZPropertyInfo(Schema.SixInchLabel); }
		}

		public virtual void ValidateSixInchLabel()
		{
			SixInchLabelInfo.ClearAllNotifications();
			if (!FiveInchLabel && !SixInchLabel)
			{
				SixInchLabelInfo.AddError(ErrorFiveOrSixInchMandatory);
			}
		}

		#endregion

		static string ErrorFiveOrSixInchMandatory
		{
			get { return Res.GetString("d8c22578-6289-401d-8e7c-4210c8af3323", "Either 'Five Inch' or 'Six Inch' label size must be selected."); }
		}

		#region PackagesFromAWB

		public virtual ZBool PackagesFromAWB
		{
			get { return packagesFromAWB; }
			set
			{
				SetNonPersistentPropertyValue(PackagesFromAWBInfo, ref packagesFromAWB, value);
				if (!IsValidationSuspended)
				{
					ValidatePackagesFromAWB();
					ValidatePackagesFromConsol();
				}
			}
		}
		ZBool packagesFromAWB;

		public virtual ZPropertyInfo PackagesFromAWBInfo
		{
			get { return this.GetZPropertyInfo(Schema.PackagesFromAWB); }
		}

		public virtual void ValidatePackagesFromAWB()
		{
			PackagesFromAWBInfo.ClearAllNotifications();
			if (!PackagesFromAWB && !PackagesFromConsol)
			{
				PackagesFromAWBInfo.AddError(ErrorPackagesFromAWBOrConsolMandatory);
			}
		}

		#endregion

		#region PackagesFromConsol

		public virtual ZBool PackagesFromConsol
		{
			get { return packagesFromConsol; }
			set
			{
				SetNonPersistentPropertyValue(PackagesFromConsolInfo, ref packagesFromConsol, value);
				if (!IsValidationSuspended)
				{
					ValidatePackagesFromConsol();
					ValidatePackagesFromAWB();
				}
			}
		}
		ZBool packagesFromConsol;

		public virtual ZPropertyInfo PackagesFromConsolInfo
		{
			get { return this.GetZPropertyInfo(Schema.PackagesFromConsol); }
		}

		public virtual void ValidatePackagesFromConsol()
		{
			PackagesFromConsolInfo.ClearAllNotifications();
			if (!PackagesFromAWB && !PackagesFromConsol)
			{
				PackagesFromConsolInfo.AddError(ErrorPackagesFromAWBOrConsolMandatory);
			}
		}

		#endregion

		static string ErrorPackagesFromAWBOrConsolMandatory
		{
			get { return Res.GetString("3b4c05fd-842a-4256-8467-13d5885a8189", "Either 'AWB' or 'Consol' must be selected."); }
		}

		#region NumberOfCopies

		public virtual ZInt NumberOfCopies
		{
			get { return numberOfCopies; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfCopiesInfo, ref numberOfCopies, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberOfCopies();
				}
			}
		}
		ZInt numberOfCopies = 1;

		public virtual ZPropertyInfo NumberOfCopiesInfo
		{
			get { return this.GetZPropertyInfo(Schema.NumberOfCopies); }
		}

		public virtual void ValidateNumberOfCopies()
		{
			NumberOfCopiesInfo.ClearAllNotifications();
			if (NumberOfCopies < 1)
			{
				NumberOfCopiesInfo.AddError(Res.GetString("3b12e8bc-7d6c-4258-a278-1d1def560301", "Number of copies needs to be at least 1"));
			}
		}

		#endregion

		#region AWBLabelPrinter

		[List("PrinterNames")]
		public virtual ZGuid AWBLabelPrinter
		{
			get { return awbLabelPrinter; }
			set
			{
				SetNonPersistentPropertyValue(AWBLabelPrinterInfo, ref awbLabelPrinter, value);
				if (!IsValidationSuspended)
				{
					ValidateAWBLabelPrinter();
				}
			}
		}
		ZGuid awbLabelPrinter;

		public virtual ZPropertyInfo AWBLabelPrinterInfo
		{
			get { return this.GetZPropertyInfo(Schema.AWBLabelPrinter); }
		}

		protected bool AWBLabelPrinter_ReadOnly
		{
			get { return AWBLabelUseEPrint; }
		}

		public virtual void ValidateAWBLabelPrinter()
		{
			AWBLabelPrinterInfo.ClearAllNotifications();

			TypeValidation.CheckValidGuid(AWBLabelPrinterInfo);

			if (!AWBLabelPrinterInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(AWBLabelPrinterInfo);
				PrintQueueValidation.ValidatePrintQueue(AWBLabelPrinterInfo);
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

		#endregion

		#region AWBLabelUseEPrint

		public virtual ZBool AWBLabelUseEPrint
		{
			get { return awbLabelUseEPrint; }
			set
			{
				SetNonPersistentPropertyValue(AWBLabelUseEPrintInfo, ref awbLabelUseEPrint, value);
				if (value)
				{
					AWBLabelPrinter = ZGuid.Empty;
				}
				if (!IsValidationSuspended)
				{
					ValidateAWBLabelUseEPrint();
				}
				AWBLabelPrinterInfo.RefreshBinding();
			}
		}
		ZBool awbLabelUseEPrint;

		public virtual ZPropertyInfo AWBLabelUseEPrintInfo
		{
			get { return this.GetZPropertyInfo(Schema.AWBLabelUseEPrint); }
		}

		public virtual void ValidateAWBLabelUseEPrint()
		{
			AWBLabelUseEPrintInfo.ClearAllNotifications();
			if (!AWBLabelUseEPrintInfo.ReadOnly && AWBLabelUseEPrint)
			{
				DeliveryMethodValidation.ValidateEPrint(AWBLabelUseEPrintInfo);
			}
		}

		#endregion

		#region ValidateAll

		public virtual void ValidateAll()
		{
			ValidateFiveInchLabel();
			ValidateSixInchLabel();
			ValidatePackagesFromAWB();
			ValidatePackagesFromConsol();
			ValidateNumberOfCopies();
			ValidateAWBLabelPrinter();
			ValidateAWBLabelUseEPrint();
		}

		#endregion

		#region IAWBActionsSerializable

		AWBActionsSerializationHelper SerializationHelper
		{
			get { return serializationHelper ?? (serializationHelper = new AWBActionsSerializationHelper(this)); }
		}
		AWBActionsSerializationHelper serializationHelper;

		const string settingsCacheName = "BulkMAWBLabelActions";
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
			result.Add(PrintOptionalInformationInfo);
			result.Add(FiveInchLabelInfo);
			result.Add(SixInchLabelInfo);
			result.Add(PackagesFromAWBInfo);
			result.Add(PackagesFromConsolInfo);
			result.Add(AWBLabelPrinterInfo);
			result.Add(AWBLabelUseEPrintInfo);
			result.Add(NumberOfCopiesInfo);
			return result;
		}

		#endregion
	}
}
