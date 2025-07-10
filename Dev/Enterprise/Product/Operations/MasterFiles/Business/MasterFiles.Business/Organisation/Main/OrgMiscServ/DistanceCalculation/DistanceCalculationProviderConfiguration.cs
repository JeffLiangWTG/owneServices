using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DistanceCalculationProviderConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Provider = "Provider";
			public const string Version = "Version";
			public const string CalculationMethod = "CalculationMethod";
		}

		#endregion

		#region Properties

		#region Provider

		[List("ProviderList")]
		[MaxLength(3)]
		public ZString Provider
		{
			get { return fProvider; }
			set
			{
				SetNonPersistentPropertyValue(ProviderInfo, ref fProvider, value);
				if (!IsValidationSuspended)
				{
					ValidateProvider();
				}
				Version = VersionList.DefaultCode ?? "";
				CalculationMethod = CalculationMethodList.DefaultCode ?? "";
			}
		}
		ZString fProvider;

		public ZPropertyInfo ProviderInfo
		{
			get { return GetZPropertyInfo(Schema.Provider); }
		}

		public void ValidateProvider()
		{
			ProviderInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ProviderInfo);
			ListValidation.ErrorIfInvalidCode(ProviderInfo);
		}

		#endregion

		#region Version

		[List("VersionList")]
		[MaxLength(3)]
		public ZString Version
		{
			get { return fVersion; }
			set
			{
				SetNonPersistentPropertyValue(VersionInfo, ref fVersion, value);
				if (!IsValidationSuspended)
				{
					ValidateVersion();
				}
			}
		}
		ZString fVersion;

		public ZPropertyInfo VersionInfo
		{
			get { return GetZPropertyInfo(Schema.Version); }
		}

		public void ValidateVersion()
		{
			VersionInfo.ClearAllNotifications();
			if (Provider == DistanceCalculationConstants.Providers.PCMiler)
			{
				MandatoryValidation.CheckEntered(VersionInfo);
			}
		}

		protected bool Version_ReadOnly
		{
			get { return Provider != DistanceCalculationConstants.Providers.PCMiler; }
		}

		#endregion

		#region CalculationMethod

		[List("CalculationMethodList")]
		[MaxLength(3)]
		public ZString CalculationMethod
		{
			get { return fCalculationMethod; }
			set
			{
				SetNonPersistentPropertyValue(CalculationMethodInfo, ref fCalculationMethod, value);
				if (!IsValidationSuspended)
				{
					ValidateCalculationMethod();
				}
			}
		}
		ZString fCalculationMethod;

		public ZPropertyInfo CalculationMethodInfo
		{
			get { return GetZPropertyInfo(Schema.CalculationMethod); }
		}

		public void ValidateCalculationMethod()
		{
			CalculationMethodInfo.ClearAllNotifications();
			if (Provider == DistanceCalculationConstants.Providers.PCMiler)
			{
				MandatoryValidation.CheckEntered(CalculationMethodInfo);
			}
			ListValidation.ErrorIfInvalidCode(CalculationMethodInfo);
		}

		protected bool CalculationMethod_ReadOnly
		{
			get { return Provider != DistanceCalculationConstants.Providers.PCMiler; }
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList ProviderList
		{
			get { return DistanceCalculationLists.Instance.Providers; }
		}

		public CodeDescriptionPairList VersionList
		{
			get { return DistanceCalculationLists.Instance.Versions(Provider); }
		}

		public CodeDescriptionPairList CalculationMethodList
		{
			get { return DistanceCalculationLists.Instance.CalculationMethods(Provider); }
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DistanceCalculationProviderConfiguration();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateProvider();
			ValidateVersion();
			ValidateCalculationMethod();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Provider, Provider);
			writer.WriteElementString(Schema.Version, Version);
			writer.WriteElementString(Schema.CalculationMethod, CalculationMethod);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Provider = reader.ReadElementString(Schema.Provider);
			Version = reader.ReadElementString(Schema.Version);
			CalculationMethod = reader.ReadElementString(Schema.CalculationMethod);
		}

		#endregion

		public static DistanceCalculationProviderConfiguration GetDefault()
		{
			DistanceCalculationProviderConfiguration result = new DistanceCalculationProviderConfiguration();
			result.Provider = result.ProviderList.DefaultCode;
			return result;
		}
	}
}
