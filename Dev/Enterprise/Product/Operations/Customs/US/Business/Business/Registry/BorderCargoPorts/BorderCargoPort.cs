using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BorderCargoPort : RegistryBusinessObjectTemplate
	{
		public BorderCargoPort()
		{
		}

		public BorderCargoPort(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string PortCode = "PortCode";
			public const string CRProcess = "CRProcess";
			public const string Location = "Location";
		}

		#region Port Code

		[List(nameof(Ports))]
		[MaxLength(4)]
		public ZString PortCode
		{
			get { return portCode; }
			set
			{
				SetNonPersistentPropertyValue(PortCodeInfo, ref portCode, value);

				if (!IsValidationSuspended)
				{
					ValidatePortCode();
				}
			}
		}
		ZString portCode;

		public IBusinessObjectCollection Ports
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(CurrentFactory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZPropertyInfo PortCodeInfo
		{
			get { return GetZPropertyInfo(Schema.PortCode); }
		}

		void ValidatePortCode()
		{
			PortCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PortCodeInfo);
			ListValidation.ErrorIfInvalidCode(PortCodeInfo);
		}

		#endregion

		#region CR Process

		[List(nameof(ProcessList))]
		public ZString CRProcess
		{
			get { return crProcess; }
			set
			{
				SetNonPersistentPropertyValue(CRProcessInfo, ref crProcess, value);

				if (!IsValidationSuspended)
				{
					ValidateCRProcess();
				}
			}
		}
		ZString crProcess;

		public CRProcessList ProcessList => CurrentFactory.GetCachedValue<CRProcessList>();

		public ZPropertyInfo CRProcessInfo
		{
			get { return GetZPropertyInfo(Schema.CRProcess); }
		}

		void ValidateCRProcess()
		{
			CRProcessInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CRProcessInfo);
			ListValidation.ErrorIfInvalidCode(CRProcessInfo);
		}

		#endregion

		#region Location

		[List(nameof(Locations))]
		[MaxLength(1)]
		public ZString Location
		{
			get { return location; }
			set
			{
				SetNonPersistentPropertyValue(LocationInfo, ref location, value);

				if (!IsValidationSuspended)
				{
					ValidateLocation();
				}
			}
		}
		ZString location;

		public LocationList Locations => CurrentFactory.GetCachedValue<LocationList>();

		public ZPropertyInfo LocationInfo
		{
			get { return GetZPropertyInfo(Schema.Location); }
		}

		void ValidateLocation()
		{
			LocationInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(LocationInfo);
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePortCode();
			ValidateCRProcess();
			ValidateLocation();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BorderCargoPort(fallbackLevel, factory);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.PortCode, PortCode);
			writer.WriteElementString(Schema.CRProcess, CRProcess);
			writer.WriteElementString(Schema.Location, Location);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			PortCode = reader.ReadElementString(Schema.PortCode);
			CRProcess = reader.ReadElementString(Schema.CRProcess);
			Location = reader.ReadElementString(Schema.Location);
		}

		#endregion
	}
}
