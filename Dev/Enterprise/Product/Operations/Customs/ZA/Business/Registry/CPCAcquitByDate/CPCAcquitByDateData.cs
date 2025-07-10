using CargoWise.Integration;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class CPCAcquitByDateData : ICodeDescription
	{
		public object PK { get; }
		public string Code { get; }
		public string Description { get; }

		public CPCAcquitByDateData(object pk, string code, string description)
		{
			PK = pk;
			Code = code;
			Description = description;
		}
	}
}
