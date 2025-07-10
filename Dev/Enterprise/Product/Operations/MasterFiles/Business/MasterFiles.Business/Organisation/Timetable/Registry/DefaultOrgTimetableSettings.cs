using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultOrgTimetableSettings : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string CountryCode = "CountryCode";
		}

		#endregion

		#region Country

		RefCountryCollection countries;
		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(CurrentFactory)); }
		}

		ZString fCountryCode = ZString.Empty;
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

		public bool CountryCode_ReadOnly => ParentCollections.Any(collection => collection.FirstOrDefault() == this);

		protected override void RunPreSaveValidationCore()
		{
			if (!IsValidationSuspended)
			{
				ValidateCountryCode();
			}

			base.RunPreSaveValidationCore();
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}

		public ZString CountryName
		{
			get
			{
				return CountryCode.IsEmpty ? (ZString)(NoResString)"Default" : CurrentFactory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode).RN_Desc;
			}
		}

		public void ValidateCountryCode()
		{
			CountryCodeInfo.ClearAllNotifications();
			if (fCountryCode.Length > 0)
			{
				ListValidation.ErrorIfInvalidCode(CountryCodeInfo);
			}
			CheckRecordIsUnique();
			CheckDefaultSettingsExist();
		}

		void CheckRecordIsUnique()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (DefaultOrgTimetableSettings item in collection)
				{
					item.ClearRowNotifications();
					if (item != this && item.CountryCode == CountryCode)
					{
						ZString duplicateMessage = Res.GetString("194e4715-1b2a-41a8-b8b6-09cd41fd09b2", "Duplicate Country/Region Code.");
						AddRowError(duplicateMessage);
						item.AddRowError(duplicateMessage);
						break;
					}
				}
			}
		}

		void CheckDefaultSettingsExist()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				var defaultSettingsExist = collection.Cast<DefaultOrgTimetableSettings>().Any(x => x.CountryCode == ZString.Empty);
				if (!defaultSettingsExist)
				{
					ZString message = Res.GetString("3fd3e585-a033-4cc2-80e0-f8eb54e0840e", "No settings for default countries");
					AddRowError(message);
				}
			}
		}

		#endregion

		#region DefaultTimetable

		DefaultOrgTimetableCollection timetables;

		[ChildEditable]
		public DefaultOrgTimetableCollection Timetables
		{
			get
			{
				if (timetables  == null)
				{
					timetables = new DefaultOrgTimetableCollection(CurrentFallbackLevel, CurrentFactory);
					timetables.ParentSettings = this;
					RegisterEditableChildObject(timetables);
				}

				return timetables;
			}
		}

		void SetNewTimetables(DefaultOrgTimetableCollection value)
		{
			if (timetables != null)
			{
				timetables.ParentSettings = null;
				UnRegisterEditableChildObject(timetables);
			}

			timetables = value;
			if (timetables != null)
			{
				timetables.ParentSettings = this;
				RegisterEditableChildObject(timetables);
			}

			RefreshBindingIncludingChildren();
		}

		#endregion

		public DefaultOrgTimetableSettings()
			: base()
		{
		}

		public DefaultOrgTimetableSettings(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DefaultOrgTimetableSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultOrgTimetableSettings(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((DefaultOrgTimetableSettings)clone).SetNewTimetables((DefaultOrgTimetableCollection)Timetables.Clone(CurrentFallbackLevel, CurrentFactory));
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CountryCode, CountryCode);
			CollectionSerializer.Serialize(writer, Timetables);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			SetNewTimetables(((DefaultOrgTimetableCollection)CollectionSerializer.Deserialize(reader)));
		}

		ZXmlSerializer CollectionSerializer
		{
			get { return collectionSerializer ?? (collectionSerializer = ZXmlSerializer.New(typeof(DefaultOrgTimetableCollection))); }
		}

		ZXmlSerializer collectionSerializer;

		#endregion
	}
}
