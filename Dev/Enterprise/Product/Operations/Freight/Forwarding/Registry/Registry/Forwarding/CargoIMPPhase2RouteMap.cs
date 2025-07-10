using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class CargoIMPPhase2RouteMap : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string AirlineTwoCharacterCode = "AirlineTwoCharacterCode";
		}

		#endregion

		public CargoIMPPhase2RouteMap()
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoIMPPhase2RouteMap();
		}

		#endregion

		#region Properties

		#region Origin

		[List("Locations")]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString Origin
		{
			get { return this.origin; }
			set
			{
				SetNonPersistentPropertyValue(OriginInfo, ref this.origin, value);
				if (!IsValidationSuspended)
				{
					ValidateOrigin();
					ValidateOriginDestinationAirline();
				}
			}
		}
		ZString origin;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(Schema.Origin); }
		}

		public void ValidateOrigin()
		{
			OriginInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OriginInfo);
			ListValidation.ErrorIfInvalidCode(OriginInfo, Locations);
		}

		#endregion

		#region Destination

		[List("Locations")]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public ZString Destination
		{
			get { return this.destination; }
			set
			{
				SetNonPersistentPropertyValue(DestinationInfo, ref this.destination, value);
				if (!IsValidationSuspended)
				{
					ValidateDestination();
					ValidateOriginDestinationAirline();
				}
			}
		}
		ZString destination;

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.Destination); }
		}

		public void ValidateDestination()
		{
			DestinationInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DestinationInfo);
			ListValidation.ErrorIfInvalidCode(DestinationInfo, Locations);
		}

		#endregion

		#region AirlineTwoCharacterCode

		[List("Airlines")]
		[MaxLength(RefAirline.Schema.RM_TwoCharacterCodeMaxLength)]
		public ZString AirlineTwoCharacterCode
		{
			get { return this.airlineTwoCharacterCode; }
			set
			{
				SetNonPersistentPropertyValue(AirlineTwoCharacterCodeInfo, ref this.airlineTwoCharacterCode, value);
				if (!IsValidationSuspended)
				{
					ValidateAirlineTwoCharacterCode();
					ValidateOriginDestinationAirline();
				}
			}
		}
		ZString airlineTwoCharacterCode;

		public ZPropertyInfo AirlineTwoCharacterCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AirlineTwoCharacterCode); }
		}

		public void ValidateAirlineTwoCharacterCode()
		{
			AirlineTwoCharacterCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AirlineTwoCharacterCodeInfo);
			ListValidation.ErrorIfInvalidCode(AirlineTwoCharacterCodeInfo, Airlines);
		}

		#endregion

		#region OriginDestinationAirline

		public ZString OriginDestinationAirline
		{
			get { return Origin + "/" + Destination + "/" + AirlineTwoCharacterCode; }
		}

		public ZPropertyInfo OriginDestinationAirlineInfo
		{
			get { return GetZPropertyInfo(nameof(OriginDestinationAirline), "Origin/Destination/Airline"); }
		}

		public void ValidateOriginDestinationAirline()
		{
			OriginDestinationAirlineInfo.ClearAllNotifications();
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OriginDestinationAirlineInfo, Res.GetString("b1a85ed7-30b7-485d-9882-28140bec0586", "There must be only one line for each Origin/Destination/Airline combination."));
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrigin();
			ValidateDestination();
			ValidateAirlineTwoCharacterCode();
			ValidateOriginDestinationAirline();
		}

		#endregion

		#region Lookups

		public LocationCollection Locations
		{
			get { return RegistryFactory.Instance.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(RegistryFactory.Instance, false)); }
		}

		public RefAirlineCollection Airlines
		{
			get { return this.airlines ?? (this.airlines = new RefAirlineCollection(RegistryFactory.Instance)); }
		}
		RefAirlineCollection airlines;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Origin, origin);
			writer.WriteElementString(Schema.Destination, destination);
			writer.WriteElementString(Schema.AirlineTwoCharacterCode, AirlineTwoCharacterCode);
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			Origin = wrapper.ReadElementString(Schema.Origin);
			Destination = wrapper.ReadElementString(Schema.Destination);
			AirlineTwoCharacterCode = wrapper.ReadElementString(Schema.AirlineTwoCharacterCode);
		}

		#endregion
	}
}
