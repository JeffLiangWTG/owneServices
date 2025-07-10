using Enterprise.Integration.Freight;

namespace Enterprise.Packing.Business
{
	class ReferenceNumberType : ICustomsNumberTypeCodeDescription
	{
		public ReferenceNumberType(string code, string description, bool isUnique = true, bool isAutomation = false)
		{
			this.code = code;
			this.description = description;
			this.IsUnique = isUnique;
			this.IsAutomation = isAutomation;
		}

		public bool IsUnique { get; }

		public bool IsAutomation { get; }

		public string Code
		{
			get { return code; }
		}

		readonly string code;

		public string Description
		{
			get { return description; }
		}

		readonly string description;

		public object PK
		{
			get { return null; }
		}
	}
}
