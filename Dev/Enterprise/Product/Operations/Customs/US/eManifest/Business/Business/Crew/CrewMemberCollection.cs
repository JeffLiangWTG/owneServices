using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CrewMemberCollection : ActiveBusinessObjectCollection<CrewMember>
	{
		internal CrewMemberCollection(Trip master)
			: base(master.Factory, master, new ZQuery(), CusInBondPersonSchema.CP_BH_Header)
		{
		}

		protected override void SetDefaultsForNewElementCore(CrewMember newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Count == 0)
			{
				newElement.CP_Type = CrewTypes.Codes.ResponsibleParty;
			}
		}
	}
}
