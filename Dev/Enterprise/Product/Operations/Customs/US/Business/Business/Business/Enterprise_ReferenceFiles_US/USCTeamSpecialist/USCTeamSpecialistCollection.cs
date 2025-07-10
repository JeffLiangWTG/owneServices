using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCTeamSpecialistCollection : BusinessObjectCollection<USCTeamSpecialist>
	{
		public USCTeamSpecialistCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
