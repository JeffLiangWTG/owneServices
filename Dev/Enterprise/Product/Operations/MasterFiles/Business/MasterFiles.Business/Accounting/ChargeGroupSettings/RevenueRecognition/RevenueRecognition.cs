using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RevenueRecognition : ChargeGroupSetting, IRevenueRecognition
	{
		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string RecognitionDateOption = "RecognitionDateOptionCode";
			public const string Offset = "Offset";
			public const string OffsetType = "OffsetType";
			public const string BrokerCode = "BrokerCode";
		}

		#endregion

		public static RevenueRecognition CreateRevenueRecognitionForQSH()
		{
			var result = new RevenueRecognition();
			result.JobType = JobInvoicingConsumerTypes.QuotedBooking.Code;
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RevenueRecognition();
		}

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(RevenueRecognitionCollection));
				return parentCollection != null ? parentCollection.Cast<IRevenueRecognition>().ToArray() : System.Array.Empty<IRevenueRecognition>();
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateRecognitionDateOptionCode();
			ValidateOffset();
			ValidateOffsetType();
			ValidateBrokerCode();
		}

		public new RevenueRecognitionValidation Validation
		{
			get { return (RevenueRecognitionValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new RevenueRecognitionValidation(this);
		}

		#endregion

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
		}

		#endregion

		#region Properties

		#region JobType

		public override ZString JobType
		{
			get { return base.JobType; }
			set
			{
				base.JobType = value;
				UpdateBrokerCodeIfShouldBeReadonly();
			}
		}

		#endregion

		#region Direction

		public override ZString DirectionCode
		{
			get { return base.DirectionCode; }
			set
			{
				base.DirectionCode = value;
				UpdateBrokerCodeIfShouldBeReadonly();
			}
		}

		#endregion

		#region RecognitionDateOption

		[MaxLength(3)]
		[List("RecognitionDateOptionList")]
		public ZString RecognitionDateOptionCode
		{
			get { return fRecognitionDateOptionCode; }
			set
			{
				CheckMaximumLength(RecognitionDateOptionCodeInfo, value);
				SetNonPersistentPropertyValue(RecognitionDateOptionCodeInfo, ref fRecognitionDateOptionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateRecognitionDateOptionCode();
				}

				UpdateOffsetIfShouldBeReadonly();
				UpdateOffsetTypeIfShouldBeReadonly();
			}
		}

		public virtual ZPropertyInfo RecognitionDateOptionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.RecognitionDateOption); }
		}

		public void ValidateRecognitionDateOptionCode()
		{
			RecognitionDateOptionCodeInfo.ClearAllNotifications();

			Validation.ValidateRecognitionDateOptionCode();
		}

		ZString fRecognitionDateOptionCode;

		public ZString RecognitionDateOptionDescription
		{
			get
			{
				ICodeDescription recognitionDateOption = RevenueRecognitionLookups.CompleteRecognitionDateOptionList[RecognitionDateOptionCode];
				return recognitionDateOption != null ? recognitionDateOption.Description : "";
			}
		}

		public ZPropertyInfo RecognitionDateOptionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RecognitionDateOptionDescription)); }
		}

		public CodeDescriptionPairList RecognitionDateOptionList
		{
			get { return RevenueRecognitionLookups.RecognitionDateOptionList; }
		}

		#endregion

		#region Offset

		public ZInt Offset
		{
			get { return fOffset; }
			set
			{
				SetNonPersistentPropertyValue(OffsetInfo, ref fOffset, value);
				if (!IsValidationSuspended)
				{
					ValidateOffset();
				}
			}
		}

		public ZPropertyInfo OffsetInfo
		{
			get { return GetZPropertyInfo(Schema.Offset); }
		}

		public bool Offset_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.Offset_ReadOnly;
			}
		}

		public void ValidateOffset()
		{
			OffsetInfo.ClearAllNotifications();

			Validation.ValidateOffset();
		}

		void UpdateOffsetIfShouldBeReadonly()
		{
			if (Offset_ReadOnly)
			{
				if (Offset != 0)
				{
					offsetToRestore = Offset;
					Offset = 0;
				}
			}
			else if (offsetToRestore != 0 && Offset == 0)
			{
				Offset = offsetToRestore;
			}
		}

		ZInt fOffset;
		ZInt offsetToRestore;

		#endregion

		#region Offset Type

		[MaxLength(3)]
		[List("OffsetTypeList")]
		public ZString OffsetType
		{
			get { return fOffsetType; }
			set
			{
				CheckMaximumLength(OffsetTypeInfo, value);
				SetNonPersistentPropertyValue(OffsetTypeInfo, ref fOffsetType, value);
				if (!IsValidationSuspended)
				{
					ValidateOffsetType();
				}
			}
		}

		public ZPropertyInfo OffsetTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OffsetType); }
		}

		public bool OffsetType_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.OffsetType_ReadOnly;
			}
		}

		public CodeDescriptionPairList OffsetTypeList
		{
			get { return RevenueRecognitionLookups.OffsetTypeList; }
		}

		public void ValidateOffsetType()
		{
			OffsetTypeInfo.ClearAllNotifications();

			Validation.ValidateOffsetType();
		}

		void UpdateOffsetTypeIfShouldBeReadonly()
		{
			if (OffsetType_ReadOnly)
			{
				if (!OffsetType.IsEmpty)
				{
					offsetTypeToRestore = OffsetType;
					OffsetType = ZString.Empty;
				}
			}
			else if (!offsetTypeToRestore.IsEmpty && OffsetType.IsEmpty)
			{
				OffsetType = offsetTypeToRestore;
			}
		}

		ZString fOffsetType;
		ZString offsetTypeToRestore;

		#endregion

		#region Broker

		[MaxLength(3)]
		[List("BrokerList")]
		public ZString BrokerCode
		{
			get { return fBrokerCode; }
			set
			{
				CheckMaximumLength(BrokerCodeInfo, value);
				SetNonPersistentPropertyValue(BrokerCodeInfo, ref fBrokerCode, value);
				if (!IsValidationSuspended)
				{
					ValidateBrokerCode();
				}
			}
		}

		public virtual ZPropertyInfo BrokerCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BrokerCode); }
		}

		public bool BrokerCode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.BrokerCode_ReadOnly;
			}
		}

		public CodeDescriptionPairList BrokerList
		{
			get { return RevenueRecognitionLookups.BrokerList; }
		}

		public void ValidateBrokerCode()
		{
			BrokerCodeInfo.ClearAllNotifications();

			Validation.ValidateBrokerCode();
		}

		void UpdateBrokerCodeIfShouldBeReadonly()
		{
			if (BrokerCode_ReadOnly)
			{
				if (!BrokerCode.IsEmpty)
				{
					brokerCodeToRestore = BrokerCode;
					BrokerCode = ZString.Empty;
				}
			}
			else if (!brokerCodeToRestore.IsEmpty && BrokerCode.IsEmpty)
			{
				BrokerCode = brokerCodeToRestore;
			}
		}

		ZString fBrokerCode;
		ZString brokerCodeToRestore;

		#endregion

		#endregion

		#region RevenueRecognitionLookups

		public RevenueRecognitionLookups RevenueRecognitionLookups
		{
			get { return (RevenueRecognitionLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new RevenueRecognitionLookups(this);
		}

		#endregion

		#region Read Only Helper

		public new RevenueRecognitionReadOnly ReadOnlyHelper
		{
			get { return (RevenueRecognitionReadOnly)base.ReadOnlyHelper; }
		}

		protected override JobConfigurationSelectorReadOnly GetNewReadOnlyHelper()
		{
			return new RevenueRecognitionReadOnly(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.RecognitionDateOption, RecognitionDateOptionCode);
			writer.WriteElementString(Schema.Offset, Offset.ToString());
			writer.WriteElementString(Schema.OffsetType, OffsetType);
			writer.WriteElementString(Schema.BrokerCode, BrokerCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);

			string oldRecognitionDateOptionCodeName = "RecognitionDateOption";
			if (reader.Reader.Name == oldRecognitionDateOptionCodeName)
			{
				string oldFormatValue = reader.ReadElementString(oldRecognitionDateOptionCodeName);
				switch (oldFormatValue)
				{
					case "Actual/Estimated Arrival Date":
					case "Actual Arrival Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
						break;
					case "Actual/Estimated Departure Date":
					case "Actual Departure Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
						break;
					case "Customs Clearance Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
						break;
					case "Pickup Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
						break;
					case "Delivery Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
						break;
					case "Immediate":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
						break;
					case "Job Closure":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
						break;
					case "Job Open Date":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
						break;
					case "Post Date of First AR Transaction":
						RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
						break;
				}
			}
			else
			{
				RecognitionDateOptionCode = reader.ReadElementString(Schema.RecognitionDateOption);
			}

			string oldOffsetName = "DaysOffset";
			if (reader.Reader.Name == oldOffsetName)
			{
				Offset = reader.ReadElementStringAsZInt(oldOffsetName);
			}
			else
			{
				Offset = reader.ReadElementStringAsZInt(Schema.Offset);
				OffsetType = reader.ReadElementString(Schema.OffsetType);
			}

			string oldBrokerName = (NoResString)"Broker";
			if (reader.Reader.Name == oldBrokerName)
			{
				string oldFormatValue = reader.ReadElementString(oldBrokerName);
				switch (oldFormatValue)
				{
					case "All":
						BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
						break;
					case "Internal":
						BrokerCode = RevenueRecognitionLookups.BrokerCodes.Internal;
						break;
					case "External":
						BrokerCode = RevenueRecognitionLookups.BrokerCodes.External;
						break;
				}
			}
			else
			{
				BrokerCode = reader.ReadElementString(Schema.BrokerCode);
			}
			if (BrokerCode.IsEmpty && !BrokerCodeInfo.ReadOnly)
			{
				BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			}
		}

		#endregion
	}
}
