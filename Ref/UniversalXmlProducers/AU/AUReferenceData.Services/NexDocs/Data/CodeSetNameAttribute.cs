namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public sealed class CodeSetNameAttribute : CsvHelper.Configuration.Attributes.NameAttribute
	{
		public CodeSetNameAttribute(string name, CodeSetValueType valueType)
			: base(name)
		{
			Name = name;
			ValueType = valueType;
		}

		public string Name { get; }
		public CodeSetValueType ValueType { get; }
	}
}
