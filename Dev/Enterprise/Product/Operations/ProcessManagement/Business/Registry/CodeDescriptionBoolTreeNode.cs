using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class CodeDescriptionBoolTreeNode : CodeDescriptionBool
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string ID = "PK";
			public const string ParentID = "ParentID";
		}

		#endregion

		public ZGuid ID
		{
			get
			{
				if (id.IsEmpty)
				{
					id = base.GetPK();
				}
				return id;
			}
			private set
			{
				id = value;
			}
		}
		ZGuid id;

#if DEBUG
		public void SetIdForTesting(ZGuid testId)
		{
			id = testId;
		}
#endif

		public ZGuid ParentID { get; set; }

		protected override ZGuid GetPK()
		{
			return ID;
		}

		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				base.Code = value;
				if (!IsSystemAll && CodeList != null)
				{
					Description = CodeList.GetMultilingualDescriptionFromCode(Code);
				}
			}
		}

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override MultilingualString Description
		{
			get { return !IsSystemAll ? base.Description : AllDescription; }
			set
			{
				if (!IsSystemAll)
				{
					base.Description = value;
				}
				else
				{
					base.Description = (NoResString)"";
					AllDescription = value;
				}
			}
		}

		[ReadOnlyMember(nameof(EnglishDescriptionReadOnly))]
		public override ZString EnglishDescription
		{
			get { return base.EnglishDescription; }
			set { base.EnglishDescription = value; }
		}

		public bool EnglishDescriptionReadOnly
		{
			get { return CodeAndDescriptionReadOnly || CodeList != null; }
		}

		public bool IsSystemAll
		{
			get { return SystemDefined && Code == AllCode; }
		}

		protected override bool IsCodeMandatory
		{
			get { return !IsSystemAll; }
		}

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		protected override void ValidateCodeCore()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				var tree = collection as CodeDescriptionBoolTreeNodeCollection;
				if (tree != null)
				{
					foreach (CodeDescriptionBoolTreeNode node in tree)
					{
						if (!node.IsDeleted && node.ID != ID && node.ParentID == ParentID && node.Code == Code)
						{
							CodeInfo.AddError(Res.GetString("2336EC91-3C96-4434-8B2B-7CB95524837C", "The {0} has been duplicated and must be unique.", CodeInfo.HumanReadableName));
							return;
						}
					}
				}
			}
		}

		public const string AllCode = ""; // persistent code not to be translated
		MultilingualString AllDescription { get; set; }

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			ID = new ZGuid(new Guid(reader.ReadElementString(Schema.ID)));
			ParentID = new ZGuid(new Guid(reader.ReadElementString(Schema.ParentID)));

			if (Code == AllCode)
			{
				SystemDefined = true;
				Description = (NoResString)"";
				// Actual description is set in CodeDescriptionBoolTreeRegistryDataType.DeserialiseCore
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(RegistryBusinessObject.Schema.CodeMaxLength, CodeMaxLength.ToString());
			writer.WriteElementString(RegistryBusinessObject.Schema.Code, Code);
			writer.WriteElementString(RegistryBusinessObject.Schema.Description, base.Description);
			writer.WriteElementString(CodeDescriptionBool.Schema.Bool, Bool.ToString());
			writer.WriteElementString(Schema.ID, ID.ToString());
			writer.WriteElementString(Schema.ParentID, ParentID.ToString());
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolTreeNode();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var node = (CodeDescriptionBoolTreeNode)clone;
			node.ParentID = ParentID;
			node.ID = ID;
		}

		#endregion

		public override string ToString()
		{
			return Code + " " + Description + " (" + ID + ", " + ParentID + ") bool=" + Bool + " system=" + SystemDefined;
		}
	}
}
