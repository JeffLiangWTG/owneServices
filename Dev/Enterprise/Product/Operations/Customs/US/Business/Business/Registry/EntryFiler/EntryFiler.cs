using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class EntryFiler : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string EntryFilerCode = "EntryFilerCode";
			public const string IsABICertified = "IsABICertified";
			public const string CompanyPK = "CompanyPK";
		}
		#endregion

		internal Guid companyPK;

		public BusinessObject Company
		{
			get
			{
				if (company == null || company.PK != companyPK)
				{
					company = (BusinessObject)CurrentFactory.Load<IGlbCompany>(companyPK);
				}
				return company;
			}
		}
		BusinessObject company;

		#region Bound Properties

		#region EntryFilerCode
		[MaxLength(3)]
		public ZString EntryFilerCode
		{
			get { return entryFilerCode; }
			set
			{
				SetNonPersistentPropertyValue(EntryFilerCodeInfo, ref entryFilerCode, value);
			}
		}
		ZString entryFilerCode;

		public ZPropertyInfo EntryFilerCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.EntryFilerCode);
			}
		}
		#endregion

		#region IsABICertified
		public ZBool IsABICertified
		{
			get { return isABICertified; }
			set
			{
				SetNonPersistentPropertyValue(IsABICertifiedInfo, ref isABICertified, value);
			}
		}
		ZBool isABICertified;

		public ZPropertyInfo IsABICertifiedInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.IsABICertified);
				return result;
			}
		}
		#endregion

		#endregion

		#region Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			IsABICertified = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntryFiler
			{
				companyPK = companyPK,
				EntryFilerCode = EntryFilerCode,
				IsABICertified = IsABICertified
			};
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CompanyPK, companyPK.ToString());
			writer.WriteElementString(Schema.EntryFilerCode, EntryFilerCode);
			writer.WriteElementString(Schema.IsABICertified, IsABICertified.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			companyPK = new Guid(reader.ReadElementString(Schema.CompanyPK));
			EntryFilerCode = reader.ReadElementString(Schema.EntryFilerCode);
			IsABICertified = reader.ReadElementStringAsZBool(Schema.IsABICertified);
		}

		public override bool Equals(object obj)
		{
			var other = obj as EntryFiler;
			return other != null &&
					other.companyPK == companyPK &&
					other.EntryFilerCode == EntryFilerCode &&
					other.isABICertified == isABICertified;
		}

		public override int GetHashCode()
		{
			return companyPK.GetHashCode();
		}

		#endregion
	}
}
