using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class PhaseDependant : RegistryBusinessObjectTemplate, IPhaseDependant
	{
		public PhaseDependant()
			: base()
		{
		}

		public PhaseDependant(IPhaseDependant dependant)
			: base()
		{
			if (dependant != null)
			{
				Name = dependant.Name;
				Description = dependant.Description;
				DependantType = dependant.DependantType;
				IsMandatory = dependant.IsMandatory;
				IsReadOnly = dependant.IsReadOnly;
			}
		}

		#region Schema

		public abstract class Schema
		{
			public const string DependantType = "DependantType";
			public const string Name = "Name";
			public const string Description = "Description";
			public const string IsMandatory = "IsMandatory";
			public const string IsReadOnly = "IsReadOnly";
		}

		#endregion

		#region Properties

		#region DependantType

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString DependantType
		{
			get { return dependantType; }
			set
			{
				SetNonPersistentPropertyValue(DependantTypeInfo, ref dependantType, value);
			}
		}
		ZString dependantType;

		public ZPropertyInfo DependantTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DependantType); }
		}

		#endregion

		#region Name

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(200)]
		public ZString Name
		{
			get { return name; }
			set
			{
				SetNonPersistentPropertyValue(NameInfo, ref name, value);
			}
		}
		ZString name;

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(Schema.Name); }
		}

		#endregion

		#region Description

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(200)]
		public ZString Description
		{
			get { return description; }
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
			}
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region IsMandatory

		public ZBool IsMandatory
		{
			get { return isMandatory; }
			set { SetNonPersistentPropertyValue(IsMandatoryInfo, ref isMandatory, value); }
		}
		ZBool isMandatory;

		public ZPropertyInfo IsMandatoryInfo
		{
			get { return GetZPropertyInfo(Schema.IsMandatory); }
		}

		#endregion

		#region IsReadOnly

		public ZBool IsReadOnly
		{
			get { return isReadOnly; }
			set { SetNonPersistentPropertyValue(IsReadOnlyInfo, ref isReadOnly, value); }
		}
		ZBool isReadOnly;

		public ZPropertyInfo IsReadOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.IsReadOnly); }
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DependantType, DependantType);
			writer.WriteElementString(Schema.Name, Name);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.IsMandatory, IsMandatory.ToString());
			writer.WriteElementString(Schema.IsReadOnly, IsReadOnly.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			DependantType = Interner.InternValue(wrapper.ReadElementString(Schema.DependantType));
			Name = Interner.InternValue(wrapper.ReadElementString(Schema.Name));
			Description = Interner.InternValue(wrapper.ReadElementString(Schema.Description));

			var mandatory = wrapper.ReadElementString(Schema.IsMandatory);
			IsMandatory = string.IsNullOrEmpty(mandatory) ? ZBool.False : new ZBool(mandatory);

			var readOnly = wrapper.ReadElementString(Schema.IsReadOnly);
			IsReadOnly = string.IsNullOrEmpty(readOnly) ? ZBool.False : new ZBool(readOnly);
		}

		static StringInterner Interner
		{
			get { return interner ?? (interner = new StringInterner()); }
		}

		[ThreadStatic]
		static StringInterner interner;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PhaseDependant();
		}

		#endregion
	}
}
