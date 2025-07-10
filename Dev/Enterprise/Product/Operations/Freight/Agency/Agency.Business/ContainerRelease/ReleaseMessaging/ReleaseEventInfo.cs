using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	class ReleaseEventInfo : IEventInfo
	{
		public ReleaseEventInfo(ZString eventReference)
		{
			this.eventReference = eventReference;
		}

		readonly ZString eventReference;

		IBranch IEventInfo.EventBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		IDepartment IEventInfo.EventDepartment
		{
			get { return GlbDepartment.CurrentDepartment; }
		}

		ZString IEventInfo.EventReference
		{
			get { return eventReference; }
		}

		IUser IEventInfo.EventUser
		{
			get { return GlbStaff.CurrentUser; }
		}
	}
}
