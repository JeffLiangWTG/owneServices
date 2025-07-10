using CargoWise.Integration;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AirlineConfigProduct : ICodeDescription
	{
		public string Code { get; set; }
		public string Description { get; set; }

		#region ICodeDescription members

		object ICodeDescription.PK => null;
		string ICodeDescription.Code => Description;
		string ICodeDescription.Description => null;

		#endregion
	}
}

