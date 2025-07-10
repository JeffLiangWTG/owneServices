using CargoWise.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class CustomizedFieldCodeDescription : ICodeDescription
	{
		public CustomizedFieldCodeDescription(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; set; }

		public string Description { get; set; }

		public object PK => throw new System.NotImplementedException();
	}
}
