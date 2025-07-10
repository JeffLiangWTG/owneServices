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
	public class CargoGuideCredentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Login = nameof(Login);
			public const string EncryptedPassword = nameof(EncryptedPassword);
		}

		#endregion

		public CargoGuideCredentials(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public CargoGuideCredentials()
		{
		}

		#region CargoGuideCredentials Properties

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
					Validation.ValidateCargoGuideCredentialsLogin();
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
					Validation.ValidateCargoGuideCredentialsPassword();
				}
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(nameof(Password)); }
		}

		#endregion

		CargoGuideCredentialsValidation Validation => fValidation ?? (fValidation = new CargoGuideCredentialsValidation(this));
		CargoGuideCredentialsValidation fValidation;

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			var encryptedPassword = Encoder.Encrypt(Password);
			writer.WriteElementString(Schema.Login, Login);
			writer.WriteElementString(Schema.EncryptedPassword, encryptedPassword);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Login = new ZString(reader.ReadElementString(Schema.Login));
			var encryptedPassword = new ZString(reader.ReadElementString(Schema.EncryptedPassword));
			Password = Encoder.Decrypt(encryptedPassword);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoGuideCredentials(fallbackLevel, factory);
		}

		#region Encoder

		// Please don't change the encryption key, because the same GUID has been used in the related data transformation
		TwoWayEncoder Encoder => encoder ?? (encoder = new TwoWayEncoder(new Guid("15252FAD-528D-436E-9822-00903719E6D5")));
		TwoWayEncoder encoder;

		#endregion
	}
}
