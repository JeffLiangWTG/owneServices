using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class TransportReferenceNumberType : RegistryBusinessObject, ICodeDescription, ICanDelete
	{
		public TransportReferenceNumberType()
		{
		}

		public TransportReferenceNumberType(TransportReferenceNumberTypeCollection parent)
			: this()
		{
			Parent = parent;
		}

		[XmlIgnore]
		[BusinessObjectTestExclude]
		internal TransportReferenceNumberTypeCollection Parent { get; set; }

		#region Schema/Constants

		public new class Schema : RegistryBusinessObject.Schema
		{
			public const string IsUnique = "IsUnique";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			isUnique = true;
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#region IsUnique

		[ReadOnlyMember(nameof(AllFieldsReadOnly))]
		public ZBool IsUnique
		{
			get { return isUnique; }
			set
			{
				if (SetNonPersistentPropertyValue(IsUniqueInfo, ref isUnique, value) && !IsValidationSuspended)
				{
					ValidateIsUnique();
				}
			}
		}
		ZBool isUnique;

		public ZPropertyInfo IsUniqueInfo
		{
			get { return GetZPropertyInfo(Schema.IsUnique); }
		}

		public void ValidateIsUnique()
		{
			IsUniqueInfo.ClearAllNotifications();
		}

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return SystemDefined; }
		}

		public bool Description_ReadOnly
		{
			get { return AllFieldsReadOnly; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return AllFieldsReadOnly; }
		}

		string[] SystemDefinedCodesMakingAllFieldsReadOnly => new string[]
		{
			TransportCommonAdditionalReferenceTypes.Codes.HouseBill,
			TransportCommonAdditionalReferenceTypes.Codes.MasterBill,
			TransportCommonAdditionalReferenceTypes.Codes.OrderNumber,
			TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber,
			TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference,
			TransportCommonAdditionalReferenceTypes.Codes.TransportReference,
			TransportCommonAdditionalReferenceTypes.Codes.CarrierBookingReference,
		};

		bool CodeMakesAllFieldsReadOnly => SystemDefinedCodesMakingAllFieldsReadOnly.Contains<string>(Code);
		bool AllFieldsReadOnly => SystemDefined && CodeMakesAllFieldsReadOnly;

		#endregion

		#region System Defined

		public bool SystemDefined
		{
			get; set;
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIsUnique();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var transportReferenceNumberType = new TransportReferenceNumberType();
			transportReferenceNumberType.Code = Code;
			transportReferenceNumberType.Description = Description;
			transportReferenceNumberType.IsUnique = IsUnique;
			transportReferenceNumberType.SystemDefined = SystemDefined;
			return transportReferenceNumberType;
		}

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, EnglishDescription);
			writer.WriteElementString(Schema.IsUnique, IsUnique.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			EnglishDescription = reader.ReadElementString(Schema.Description);
			isUnique = new ZBool(reader.ReadElementString(Schema.IsUnique));
		}

		#endregion

		#region ICanDelete members

		bool ICanDelete.CanDelete
		{
			get { return !SystemDefined; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("7A8CB90F-6A9C-449E-8060-FF068F6A620E", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region ITransportNumberType members

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		object ICodeDescription.PK
		{
			get { return base.PK; }
		}

		#endregion
	}
}
