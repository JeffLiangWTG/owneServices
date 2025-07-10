using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class PackageVersionOverride : RegistryBusinessObject
	{
		public PackageVersionOverride() { }

		public PackageVersionOverride(PackageVersionOverrideCollection parent)
		{
			Parent = parent;
		}

		[XmlIgnore]
		[BusinessObjectTestExclude]
		internal PackageVersionOverrideCollection Parent { get; set; }

		#region CarrierCode

		public override ZString Code
		{
			get => base.Code;
			set
			{
				base.Code = value;
				ValidateUniqueKey();
			}
		}

		protected override string CodeDisplayName => Res.GetString("A2087739-E90E-4A8B-8F97-58D8B1DADFF1", "Carrier Code");

		protected override int CodeMaxLengthDefaultValue => 5;

		protected override bool IsCodeUniqueInCollection => false;

		#endregion

		#region AccountNumber

		[MaxLength(35)]
		public ZString AccountNumber
		{
			get { return accountNumber; }
			set
			{
				if (SetNonPersistentPropertyValue(AccountNumberInfo, ref accountNumber, value) && !IsValidationSuspended)
				{
					AccountNumberInfo.ClearAllNotifications();
					ValidateUniqueKey();
				}
			}
		}

		ZString accountNumber;

		public ZPropertyInfo AccountNumberInfo => GetZPropertyInfo(Schema.AccountNumber);

		#endregion

		#region UniqueKey

		public ZString UniqueKey => Code + AccountNumber;

		public ZPropertyInfo UniqueKeyInfo => GetZPropertyInfo(nameof(UniqueKey));

		void ValidateUniqueKey()
		{
			UniqueKeyInfo.ClearAllNotifications();

			if (Parent != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(UniqueKeyInfo, Parent,
					Res.GetString("6B4AA736-4680-4C26-ADEC-CE43C3C9A00F", "Only one package and version for a carrier can be overridden per account number."));
			}
		}

		#endregion

		#region PackageName

		[MaxLength(200)]
		[ResourceStringData("PackageVersionOverride|PackageName", Caption = "Package Name")]
		public ZString PackageName
		{
			get { return packageName; }
			set
			{
				if (SetNonPersistentPropertyValue(PackageNameInfo, ref packageName, value) && !IsValidationSuspended)
				{
					ValidatePackageName();
				}
			}
		}

		ZString packageName;

		public ZPropertyInfo PackageNameInfo => GetZPropertyInfo(Schema.PackageName);

		void ValidatePackageName()
		{
			PackageNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PackageNameInfo);
		}

		#endregion

		#region PackageVersion

		[MaxLength(200)]
		[ResourceStringData("PackageVersionOverride|PackageVersion", Caption = "Package Version")]
		public ZString PackageVersion
		{
			get { return packageVersion; }
			set
			{
				if (SetNonPersistentPropertyValue(PackageVersionInfo, ref packageVersion, value) && !IsValidationSuspended)
				{
					ValidatePackageVersion();
				}
			}
		}

		ZString packageVersion;

		public ZPropertyInfo PackageVersionInfo => GetZPropertyInfo(Schema.PackageVersion);

		void ValidatePackageVersion()
		{
			PackageVersionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PackageVersionInfo);
		}

		#endregion

		#region Schema/Constants

		public new class Schema : RegistryBusinessObject.Schema
		{
			public const string CarrierCode = "CarrierCode";
			public const string AccountNumber = "AccountNumber";
			public const string PackageName = "PackageName";
			public const string PackageVersion = "PackageVersion";
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.AccountNumber, AccountNumber);
			writer.WriteElementString(Schema.PackageName, PackageName);
			writer.WriteElementString(Schema.PackageVersion, PackageVersion);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			AccountNumber = reader.ReadElementString(Schema.AccountNumber);
			PackageName = reader.ReadElementString(Schema.PackageName);
			PackageVersion = reader.ReadElementString(Schema.PackageVersion);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PackageVersionOverride();
		}

		#endregion

		#region Save

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePackageName();
			ValidatePackageVersion();
		}

		#endregion
	}
}
