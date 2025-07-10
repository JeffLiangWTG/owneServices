using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class CargoSphereCredentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Login = nameof(Login);
			public const string EncryptedPassword = nameof(EncryptedPassword);
			public const string SystemCode = nameof(SystemCode);
		}

		#endregion

		public CargoSphereCredentials(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CargoSphereCredentials()
		{
		}

		#region CargoSphereCredentials Properties

		ZString fLogin = "";
		public ZString Login
		{
			get
			{
				return fLogin;
			}
			set
			{
				if (fLogin != value)
				{
					SetNonPersistentPropertyValue(LoginInfo, ref fLogin, value);
					Validation.ValidateCargoSphereCredentialsLogin();
				}
			}
		}

		public ZPropertyInfo LoginInfo
		{
			get { return GetZPropertyInfo(Schema.Login); }
		}

		ZString fPassword = "";
		[Password]
		public ZString Password
		{
			get
			{
				return fPassword;
			}
			set
			{
				if (fPassword != value)
				{
					SetNonPersistentPropertyValue(PasswordInfo, ref fPassword, value);
					Validation.ValidateCargoSphereCredentialsPassword();
				}
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(nameof(Password)); }
		}

		ZString fSystemCode = "";
		public ZString SystemCode
		{
			get
			{
				return fSystemCode;
			}
			set
			{
				if (fSystemCode != value)
				{
					SetNonPersistentPropertyValue(SystemCodeInfo, ref fSystemCode, value);
					Validation.ValidateCargoSphereCredentialsSystemCode();
				}
			}
		}

		public ZPropertyInfo SystemCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SystemCode); }
		}

		#endregion

		CargoSphereCredentialsValidation Validation => fValidation ?? (fValidation = new CargoSphereCredentialsValidation(this));
		CargoSphereCredentialsValidation fValidation;

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			var encryptedPassword = Encoder.Encrypt(Password);
			writer.WriteElementString(Schema.Login, Login);
			writer.WriteElementString(Schema.EncryptedPassword, encryptedPassword);
			writer.WriteElementString(Schema.SystemCode, SystemCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Login = new ZString(reader.ReadElementString(Schema.Login));
			var encryptedPassword = new ZString(reader.ReadElementString(Schema.EncryptedPassword));
			Password = Encoder.Decrypt(encryptedPassword);
			SystemCode = new ZString(reader.ReadElementString(Schema.SystemCode));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoSphereCredentials(fallbackLevel, factory);
		}

		#region Encoder

		// Please don't change the encryption key, because the same GUID has been used in the related data transformation
		TwoWayEncoder Encoder => encoder ?? (encoder = new TwoWayEncoder(new Guid("40AA44B5-8FC0-45B9-8FE9-4A05835209D8")));
		TwoWayEncoder encoder;

		#endregion
	}
}
