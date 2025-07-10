
namespace Enterprise.Customs.US.Business
{
	public partial class ImporterTypeList
	{
		public static bool IsGovernmentImporter(string type)
		{
			return type == Codes.ForeignGovernment ||
					type == Codes.StateGovernment ||
					type == Codes.USGovernment;
		}
	}
}
