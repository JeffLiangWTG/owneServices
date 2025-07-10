using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicationMaster : IDeduplicationGlowObject
	{
		IDeduplicatable Master { get; }
		ICollection<IOrgContact> OrgContacts { get; }
	}
}
