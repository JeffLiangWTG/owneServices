
namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IEnterpriseExeDetailAccessor
	{
		EnterpriseExeDetail GetExeDetail(string senderID);
		string GetLicenceType(string senderID);
	}
}
