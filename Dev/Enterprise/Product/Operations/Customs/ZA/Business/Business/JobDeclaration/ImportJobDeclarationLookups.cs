using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class ImportJobDeclarationLookups : JobDeclarationLookups
	{
		public ImportJobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		protected override ZQuery DischargePortFilter()
		{
			return PortQuery(Declaration.JE_TransportMode, PortLocation.All);
		}
	}
}
