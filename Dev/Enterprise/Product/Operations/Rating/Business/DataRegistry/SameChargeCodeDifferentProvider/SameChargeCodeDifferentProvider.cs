using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class SameChargeCodeDifferentProvider : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobType = nameof(JobType);
			public const string TransportMode = nameof(TransportMode);
			public const string Direction = nameof(Direction);
			public const string IsEnabled = nameof(IsEnabled);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return
				new SameChargeCodeDifferentProvider().InitialiseValues
				(
					JobType,
					TransportMode,
					Direction,
					IsEnabled
				);
		}

		#region JobType

		ZString jobType;

		public ZPropertyInfo JobTypeInfo => GetZPropertyInfo(Schema.JobType);

		[ResourceStringData("SameChargeCodeDifferentProviderControl|cbb5dc47-702d-49ca-8e94-c33b1a52cada", Caption = "Job Type")]
		[List("JobTypeList")]
		[MaxLength(3)]
		public ZString JobType
		{
			get
			{
				return jobType;
			}
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				jobType = value;
				JobTypeInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				if (jobTypeList == null)
				{
					jobTypeList = new CodeDescriptionPairList();
					jobTypeList.AddPair("ALL", Res.GetString("339592b4-bcc4-4819-91d7-150a5771b742", "All"));
					jobTypeList.AddPair(JobInvoicingConsumerTypes.Shipment.Code, JobInvoicingConsumerTypes.Shipment.MultilingualDescription);
					jobTypeList.AddPair(JobInvoicingConsumerTypes.QuotedBooking.Code, JobInvoicingConsumerTypes.QuotedBooking.MultilingualDescription);
					jobTypeList.AddPair(JobInvoicingConsumerTypes.ForwardingConsol.Code, JobInvoicingConsumerTypes.ForwardingConsol.MultilingualDescription);
					jobTypeList.AddPair(JobInvoicingConsumerTypes.GatewayConsol.Code, JobInvoicingConsumerTypes.GatewayConsol.MultilingualDescription);
				}
				return jobTypeList;
			}
		}

		CodeDescriptionPairList jobTypeList;

		#endregion

		#region TransportMode

		ZString transportMode;
		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		[ResourceStringData("SameChargeCodeDifferentProviderControl|f544a451-bdeb-4b79-87c4-1615e86e4d4f", Caption = "Transport Mode")]
		[List("TransportModeList")]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get => transportMode;
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				transportMode = value;
				TransportModeInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList TransportModeList => new CodeDescriptionPairList(OLookUpEditType.TransportType);

		#endregion

		#region Direction

		ZString direction;
		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(Schema.Direction);

		[ResourceStringData("SameChargeCodeDifferentProviderControl|26761256-d8bf-44fc-88ab-102bf39daa0d", Caption = "Direction")]
		[List("DirectionList")]
		[MaxLength(3)]
		public ZString Direction
		{
			get => direction;
			set
			{
				CheckMaximumLength(DirectionInfo, value);
				direction = value;
				DirectionInfo.RefreshBinding();
				Validate();
			}
		}

		public CodeDescriptionPairList DirectionList => JobConfigurationSelectorLookups.GetBaseDirectionList();

		#endregion

		#region IsEnabled

		[ResourceStringData("SameChargeCodeDifferentProviderControl|93225e0d-b347-4c40-a945-5c773cbecdbf", Caption = "Enabled")]
		public ZBool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				isEnabled = value;
				IsEnabledInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(Schema.IsEnabled);

		ZBool isEnabled = false;

		#endregion

		SameChargeCodeDifferentProvider InitialiseValues(string jobType, string transportMode, string direction, bool enabled)
		{
			this.jobType = jobType;
			this.transportMode = transportMode;
			this.direction = direction;

			IsEnabled = enabled;

			JobTypeInfo.RefreshBinding();
			TransportModeInfo.RefreshBinding();
			DirectionInfo.RefreshBinding();

			return this;
		}

		#region Validation

		internal void Validate()
		{
			ValidateTransportMode();
			ValidateDirection();
			CheckUniqueConfigurations();
		}

		void BasicValidation(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info);
		}

		void ValidateTransportMode()
		{
			if (!IsValidationSuspended)
			{
				BasicValidation(TransportModeInfo);
			}
		}

		void ValidateDirection()
		{
			if (!IsValidationSuspended)
			{
				BasicValidation(DirectionInfo);
			}
		}

		void CheckUniqueConfigurations()
		{
			if (IsValidationSuspended)
			{
				return;
			}

			BasicValidation(JobTypeInfo);

			MultilingualString identicalRowExistsMessage = ResString.GetMultilingualString("640c286f-8cd4-478b-85e8-5cb189d5dbd4", "Entry with identical values already exists.");

			foreach (var collection in ParentCollections.OfType<SameChargeCodeDifferentProviderCollection>())
			{
				foreach (var fallbackCharge in collection.Cast<SameChargeCodeDifferentProvider>())
				{
					if (PK != fallbackCharge.PK
						&& JobType == fallbackCharge.JobType
						&& TransportMode == fallbackCharge.TransportMode
						&& Direction == fallbackCharge.Direction)
					{
						JobTypeInfo.AddError(identicalRowExistsMessage);
						fallbackCharge.JobTypeInfo.AddError(identicalRowExistsMessage);
						break;
					}
				}
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.Direction, Direction);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			string xJobType = null;
			string xTransportMode = null;
			string xDirection = null;
			bool xIsEnabled = false;
			bool isEnablednessParsed = false;

			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.JobType:
						xJobType = reader.ReadElementString();
						break;

					case Schema.TransportMode:
						xTransportMode = reader.ReadElementString();
						break;

					case Schema.Direction:
						xDirection = reader.ReadElementString();
						break;

					case Schema.IsEnabled:
						xIsEnabled = ZBool.ParseSafe(reader.ReadElementString(), false);
						isEnablednessParsed = true;
						break;

					default:
						reader.ReadElementString();
						break;
				}

				var allValuesParsed =
					isEnablednessParsed &&
					xJobType != null &&
					xTransportMode != null &&
					xDirection != null;

				if (allValuesParsed)
				{
					break;
				}
			}

			InitialiseValues(xJobType, xTransportMode, xDirection, xIsEnabled);
			DirectionInfo.RefreshBinding();
			TransportModeInfo.RefreshBinding();
			JobTypeInfo.RefreshBinding();
		}

		#endregion
	}
}
