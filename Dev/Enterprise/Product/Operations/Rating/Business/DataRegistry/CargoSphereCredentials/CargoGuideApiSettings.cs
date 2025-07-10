using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class CargoGuideApiSettings : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ApiURL = nameof(ApiURL);
			public const string ApiVersion = nameof(ApiVersion);
		}

		#endregion

		public CargoGuideApiSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CargoGuideApiSettings()
		{
		}

		#region CargoGuideApiSettings Properties

		ZString fApiURL = "";
		public virtual ZString ApiURL
		{
			get
			{
				return fApiURL;
			}
			set
			{
				if (fApiURL != value)
				{
					ApiURLInfo.ClearAllNotifications();

					SetNonPersistentPropertyValue(ApiURLInfo, ref fApiURL, value);
					ValidateUrl(value);
				}
			}
		}

		bool ValidateUrl(string url)
		{
			var result = true;
			try
			{
				UriRegistryTypeValidator.ValidateUri(url, Uri.UriSchemeHttps, false);
			}
			catch (RegistryValidationException ex)
			{
				ApiURLInfo.AddError(ex.Message);
				result = false;
			}

			return result;
		}

		public ZPropertyInfo ApiURLInfo
		{
			get { return GetZPropertyInfo(Schema.ApiURL); }
		}

		ZString fApiVersion = "";
		public ZString ApiVersion
		{
			get
			{
				return fApiVersion;
			}
			set
			{
				if (fApiVersion != value)
				{
					SetNonPersistentPropertyValue(ApiVersionInfo, ref fApiVersion, value);
				}
			}
		}

		public ZPropertyInfo ApiVersionInfo
		{
			get { return GetZPropertyInfo(Schema.ApiVersion); }
		}

		#endregion

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ApiURL, ApiURL);
			writer.WriteElementString(Schema.ApiVersion, ApiVersion);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ApiURL = new ZString(reader.ReadElementString(Schema.ApiURL));
			ApiVersion = new ZString(reader.ReadElementString(Schema.ApiVersion));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoGuideApiSettings(fallbackLevel, factory);
		}
	}
}
