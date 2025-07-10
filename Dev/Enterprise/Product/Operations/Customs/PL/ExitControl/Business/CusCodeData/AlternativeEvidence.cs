using System.Data;
using CargoWise.EntityFramework;
namespace Enterprise.Customs.PL.ExitControl.Business;

using EU = EU.ExitControl.Business;

public class AlternativeEvidence(BusinessObjectFactory factory, DataRow row) : EU.AlternativeEvidence(factory, row)
{
	protected override EU.IAdditionalInfoCollection<EU.AdditionalInfo> CreateNewAdditionalInfoCollection()
		=> new EU.AdditionalInfoCollection<AdditionalInfo>(this);
}
