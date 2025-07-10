using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class GLLocalNumberFormat : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CountryCode = "CountryCode";
			public const string Language = "Language";
			public const string NumberFormat = "NumberFormat";
			public const string IsFixedLength = "IsFixedLength";
		}

		#endregion

		#region Construction

		public GLLocalNumberFormat() { }

		public GLLocalNumberFormat(FallbackLevel fallbackLevel) : base(fallbackLevel) { }

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GLLocalNumberFormat(fallbackLevel);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				ValidateLanguage();
				ValidateCountryCode();
				ValidateNumberFormat();
			}
		}

		#region Bound Properties

		#region  Country  Code

		ZString fCountryCode = ZString.Empty;
		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(CurrentFactory)); }
		}

		RefCountryCollection countries;

		[MaxLength(2)]
		[List("Countries")]
		public ZString CountryCode
		{
			get { return fCountryCode; }
			set
			{
				CheckMaximumLength(CountryCodeInfo, value);
				SetNonPersistentPropertyValue(CountryCodeInfo, ref fCountryCode, value);
				if (!IsValidationSuspended)
				{
					ValidateCountryCode();
				}
			}
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}

		void ValidateCountryCode()
		{
			CountryCodeInfo.ClearAllNotifications();
			if (fCountryCode.Length > 0)
			{
				ListValidation.ErrorIfInvalidCode(CountryCodeInfo);
			}
			CheckRecordIsUnique();
		}
		#endregion

		#region  Language  Code

		ZString fLanguage = ZString.Empty;

		[MaxLength(7)]
		[List("Languages")]
		public ZString Language
		{
			get { return fLanguage; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(LanguageInfo, value);
				SetNonPersistentPropertyValue(LanguageInfo, ref fLanguage, value);
				LanguageInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateLanguage();
				}
			}
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetZPropertyInfo(Schema.Language); }
		}

		void ValidateLanguage()
		{
			LanguageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LanguageInfo, Res.GetString("F5E30DDD-C7C1-4319-ADB5-2FF58792A97B", "Language"));
			ListValidation.ErrorIfInvalidCode(LanguageInfo);
			CheckRecordIsUnique();
		}

		public CodeDescriptionPairList Languages
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Language); }
		}

		#endregion

		#region  Number Format

		ZString fNumberFormat = ZString.Empty;

		[MaxLength(30)]
		public ZString NumberFormat
		{
			get { return fNumberFormat; }
			set
			{
				CheckMaximumLength(NumberFormatInfo, value);
				SetNonPersistentPropertyValue(NumberFormatInfo, ref fNumberFormat, value);
				NumberFormatInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateNumberFormat();
				}
			}
		}

		public ZPropertyInfo NumberFormatInfo
		{
			get { return GetZPropertyInfo(Schema.NumberFormat); }
		}

		void ValidateNumberFormat()
		{
			NumberFormatInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NumberFormatInfo, Res.GetString("64fa024f-8a7b-4531-860a-a1a60945d09d", "Local GL Number Format"));

			Regex r = new Regex("^[1-9](-[1-9])+$");

			if (!r.IsMatch(fNumberFormat))
			{
				NumberFormatInfo.AddError(Res.GetString("828F0171-91BC-4C56-ABBF-D3A9938DFDE9", "Local GL Number Format only use numeric value (1 to 9) and “-“ as separator to identify the different levels."));
			}
		}

		#endregion

		#region  Number Length

		ZBool fIsFixedLength = false;

		public ZBool IsFixedLength
		{
			get { return fIsFixedLength; }
			set { SetNonPersistentPropertyValue(IsFixedLengthInfo, ref fIsFixedLength, value); }
		}

		public ZPropertyInfo IsFixedLengthInfo
		{
			get { return GetZPropertyInfo(Schema.IsFixedLength); }
		}

		#endregion

		void CheckRecordIsUnique()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (GLLocalNumberFormat item in collection)
				{
					item.ClearRowNotifications();
					if (item != this &&
						item.Language == Language && item.CountryCode == CountryCode)
					{
						ZString duplicateMessage = Res.GetString("241CA2A2-AA18-478C-934D-B1E5AD108B98", "Duplicate Country/Region Code and Language.");
						AddRowError(duplicateMessage);
						item.AddRowError(duplicateMessage);
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

			writer.WriteElementString(Schema.Language, Language);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			writer.WriteElementString(Schema.NumberFormat, NumberFormat);
			writer.WriteElementString(Schema.IsFixedLength, IsFixedLength.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Language = reader.ReadElementString(Schema.Language);
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			NumberFormat = reader.ReadElementString(Schema.NumberFormat);
			IsFixedLength = reader.ReadElementStringAsZBool(Schema.IsFixedLength);
		}

		#endregion
	}
}
