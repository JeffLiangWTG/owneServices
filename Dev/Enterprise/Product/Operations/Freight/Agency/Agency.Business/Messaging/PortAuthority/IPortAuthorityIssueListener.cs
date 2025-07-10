using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public interface IPortAuthorityIssueListener
	{
		void Notify(ZGuid businessObjectPK, ZString tablePrefix, ZString errorText, ZString detail);
	}
}
