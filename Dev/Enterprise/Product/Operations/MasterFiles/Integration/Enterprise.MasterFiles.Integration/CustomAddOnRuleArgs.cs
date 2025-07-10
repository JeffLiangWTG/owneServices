using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public struct CustomAddOnRuleArgs
	{
		public IZType OldValue { get; set; }
		public IZType OldDescription { get; set; }
		public IZType NewValue { get; set; }
		public IZType NewDescription { get; set; }
		public string Type { get; set; }
		public ICustomAddOnRule[] Rules { get; set; }

		public string FieldName { get; set; }

		public bool HasChanges => $"{OldValue}|{OldDescription}" != $"{NewValue}|{NewDescription}";
	}
}
