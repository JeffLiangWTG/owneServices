using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	[XmlRoot("EntityConfiguration")]
	public class EntityConfiguration
	{
		public string DataFileExtensions { get; set; }
		public EntityType[] EntityTypes { get; set; }
		public EntityTypeExcelColumnMapping EntityTypeExcelColumnMapping { get; set; }

		public Rule[] Rules { get; set; }
		public string ValuesToTreatEmpty { get; set; }
	}

	public class EntityType
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public bool Data { get; set; }
		public PropertyRef[] Key { get; set; }
		public Property[] Properties { get; set; }
	}

	public class PropertyRef
	{
		[XmlAttribute]
		public string Name { get; set; }
	}

	public class Property
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public string Type { get; set; }
		[XmlAttribute]
		public int MaxLength { get; set; }
		[XmlAttribute]
		public string ConstantValue { get; set; }
		[XmlAttribute]
		public string DefaultValue { get; set; }
		[XmlAttribute]
		public int ExcelColumn { get; set; }
		[XmlAttribute]
		public string DateTimeFormat { get; set; }
		[XmlAttribute]
		public bool IsEndDate { get; set; }
		[XmlAttribute]
		public bool IsNonPersistent { get; set; }
		[XmlAttribute]
		public bool IsCompositeKey { get; set; }
	}

	public class EntityTypeExcelColumnMapping
	{
		public int SheetIndex { get; set; }
		public int StartRow { get; set; }
		public bool IsCompositeKeyNeeded { get; set; }
		public MappingEntityType[] EntityTypes { get; set; }
	}

	public class MappingEntityType : EntityType
	{
		[XmlAttribute]
		public int Sequence { get; set; }
	}

	public class Rule
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public string Relationship { get; set; }
		public RuleValue[] RuleValues { get; set; }
	}

	public class RuleValue
	{
		[XmlAttribute]
		public string Value { get; set; }
		[XmlAttribute]
		public int TaxReductionAmountPerUnit { get; set; }
		[XmlAttribute]
		public string QuantityUnit { get; set; }
		[XmlAttribute]
		public string DocumentType { get; set; }
		[XmlAttribute]
		public string Formula { get; set; }
	}
}
