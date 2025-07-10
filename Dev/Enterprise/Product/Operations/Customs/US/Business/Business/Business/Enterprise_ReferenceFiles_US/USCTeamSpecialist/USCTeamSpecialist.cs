using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCTeamSpecialist : AutoUSCTeamSpecialist
	{
		public USCTeamSpecialist(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
