using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class RateCommodityDefaultingRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string RateCommodityCode = nameof(RateCommodityCode);
			public const string Origin = nameof(Origin);
			public const string Destination = nameof(Destination);
			public const string TransportMode = nameof(TransportMode);
			public const string ContainerMode = nameof(ContainerMode);
			public const string Direction = nameof(Direction);
			public const string ServiceLevel = nameof(ServiceLevel);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return
				new RateCommodityDefaultingRule().InitialiseValues
				(
					RateCommodityCode,
					Origin,
					Destination,
					TransportMode,
					ContainerMode,
					Direction,
					ServiceLevel
				);
		}

		#region RateCommodity

		ZString rateCommodityCode;

		public ZPropertyInfo RateCommodityCodeInfo => GetZPropertyInfo(RateCommodityDefaultingRule.Schema.RateCommodityCode);

		[ResourceStringData("RateCommodityControl|4e997b45-9763-4ed3-908d-65580c1336e2", Caption = "Commodity")]
		[List("Lookups.CommodityCodes")] 
		[MaxLength(4)]
		public ZString RateCommodityCode
		{
			get => rateCommodityCode;
			set
			{
				CheckMaximumLength(RateCommodityCodeInfo, value);
				rateCommodityCode = value;
				RateCommodityCodeInfo.RefreshBinding();
				Validate();
			}
		}

		#endregion

		#region Origin

		ZString origin;
		public ZPropertyInfo OriginInfo => GetZPropertyInfo(Schema.Origin);

		[ResourceStringData("RateCommodityControl|bb4807b5-84bc-42b3-84ae-69b0b5ce40bc", Caption = "Origin")]
		[List("Lookups.Locations")]
		[MaxLength(5)]
		public ZString Origin
		{
			get => origin;
			set
			{
				CheckMaximumLength(OriginInfo, value);
				origin = value;
				OriginInfo.RefreshBinding();
				Validate();
			}
		}

		#endregion

		#region Destination

		ZString destination;
		public ZPropertyInfo DestinationInfo => GetZPropertyInfo(Schema.Destination);

		[ResourceStringData("RateCommodityControl|7649df37-2386-47e7-94fd-b8d6f702bddd", Caption = "Destination")]
		[List("Lookups.Locations")]
		[MaxLength(5)]
		public ZString Destination
		{
			get => destination;
			set
			{
				CheckMaximumLength(DestinationInfo, value);
				destination = value;
				DestinationInfo.RefreshBinding();
				Validate();
			}
		}

		#endregion

		#region TransportMode

		ZString transportMode;
		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		[ResourceStringData("RateCommodityControl|0bc17e39-921e-44ba-9c5d-b89c96cd9525", Caption = "Transport Mode")]
		[List("Lookups.TransportModeList")]
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

		#endregion

		#region ContainerMode

		ZString containerMode;
		public ZPropertyInfo ContainerModeInfo => GetZPropertyInfo(FallbackSubjectToCharges.Schema.ContainerMode);

		[ResourceStringData("RateCommodityControl|aa4c6de5-affc-4a06-a77e-b5cf71f33451", Caption = "Container Mode")]
		[List("Lookups.ContainerModeList")]
		[MaxLength(3)]
		public ZString ContainerMode
		{
			get => containerMode;
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				containerMode = value;
				ContainerModeInfo.RefreshBinding();
				Validate();
			}
		}

		#endregion

		#region Direction

		ZString direction;
		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(Schema.Direction);

		[ResourceStringData("RateCommodityControl|43d08bd1-2655-47a1-96cb-879ba7df60c0", Caption = "Direction")]
		[List("Lookups.DirectionList")]
		[MaxLength(5)]
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

		#endregion

		#region ServiceLevel

		ZString serviceLevel;
		public ZPropertyInfo ServiceLevelInfo => GetZPropertyInfo(Schema.ServiceLevel);

		[ResourceStringData("RateCommodityControl|6dc74752-5ac4-4fa2-9c5f-6eda9fdb58f0", Caption = "Service Level")]
		[List("Lookups.ServiceLevels")]
		[MaxLength(3)]
		public ZString ServiceLevel
		{
			get => serviceLevel;
			set
			{
				CheckMaximumLength(ServiceLevelInfo, value);
				serviceLevel = value;
				ServiceLevelInfo.RefreshBinding();
				Validate();
			}
		}

		#endregion

		RateCommodityDefaultingRule InitialiseValues(string rateCommodityCode, string origin, string destination, string transportMode, string containerMode, string direction, string serviceLevel)
		{
			this.rateCommodityCode = rateCommodityCode;
			this.origin = origin;
			this.destination = destination;
			this.transportMode = transportMode;
			this.containerMode = containerMode;
			this.direction = direction;
			this.serviceLevel = serviceLevel;

			RateCommodityCodeInfo.RefreshBinding();
			OriginInfo.RefreshBinding();
			DestinationInfo.RefreshBinding();
			TransportModeInfo.RefreshBinding();
			ContainerModeInfo.RefreshBinding();
			DirectionInfo.RefreshBinding();
			ServiceLevelInfo.RefreshBinding();

			return this;
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.RateCommodityCode, RateCommodityCode);
			writer.WriteElementString(Schema.Origin, Origin);
			writer.WriteElementString(Schema.Destination, Destination);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.Direction, Direction);
			writer.WriteElementString(Schema.ServiceLevel, ServiceLevel);
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			string rateCommodityCode = null;
			string origin = null;
			string destination = null;
			string transportMode = null;
			string containerMode = null;
			string direction = null;
			string serviceLevel = null;

			XmlReader reader = wrapper.Reader;
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				switch (reader.LocalName)
				{
					case Schema.RateCommodityCode:
						rateCommodityCode = reader.ReadElementString();
						break;
					case Schema.Origin:
						origin = reader.ReadElementString();
						break;
					case Schema.Destination:
						destination = reader.ReadElementString();
						break;
					case Schema.TransportMode:
						transportMode = reader.ReadElementString();
						break;
					case Schema.ContainerMode:
						containerMode = reader.ReadElementString();
						break;
					case Schema.Direction:
						direction = reader.ReadElementString();
						break;
					case Schema.ServiceLevel:
						serviceLevel = reader.ReadElementString();
						break;
					default:
						reader.ReadElementString();
						break;
				}
			}

			InitialiseValues(rateCommodityCode: rateCommodityCode, origin: origin, destination: destination, transportMode: transportMode, containerMode: containerMode, direction: direction, serviceLevel: serviceLevel);
		}

		#endregion

		#region Validation

		void Validate()
		{
			if (!IsValidationSuspended)
			{
				ValidateRateCommodityCode();
				ValidateField(DestinationInfo);
				ValidateField(OriginInfo);
				ValidateField(TransportModeInfo);
				ValidateField(ContainerModeInfo);
				ValidateField(DirectionInfo);
				ValidateField(ServiceLevelInfo);
				CheckUniqueConfigurations();
			}
		}

		void ValidateRateCommodityCode()
		{
			RateCommodityCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RateCommodityCodeInfo);
			ListValidation.ErrorIfInvalidCode(RateCommodityCodeInfo);
		}

		void ValidateField(ZPropertyInfo info)
		{
			info.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(info);
		}

		public static readonly MultilingualString IdenticalCommodityDefaultConfigurationExists = ResString.GetMultilingualString("7f338126-0da0-4428-a4d5-3fd65300a1db", "Commodity default with identical values already exists");

		void CheckUniqueConfigurations()
		{
			if (!IsValidationSuspended)
			{
				foreach (var collection in ParentCollections.OfType<RateCommodityDefaultingRuleCollection>()) // Parent Collection where!
				{
					foreach (RateCommodityDefaultingRule rateCommodity in collection)
					{
						if (PK != rateCommodity.PK
							&& Origin == rateCommodity.Origin
							&& Destination == rateCommodity.Destination
							&& TransportMode == rateCommodity.TransportMode
							&& ContainerMode == rateCommodity.ContainerMode
							&& Direction == rateCommodity.Direction
							&& ServiceLevel == rateCommodity.ServiceLevel)
						{
							RateCommodityCodeInfo.AddError(IdenticalCommodityDefaultConfigurationExists);
							rateCommodity.RateCommodityCodeInfo.AddError(IdenticalCommodityDefaultConfigurationExists);
						}
					}
				}
			}
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validate();
		}

		public RateCommodityDefaultingRuleLookups Lookups
		{
			get
			{
				return lookups ?? (lookups = new RateCommodityDefaultingRuleLookups(this));
			}
		}
		RateCommodityDefaultingRuleLookups lookups;
	}
}
