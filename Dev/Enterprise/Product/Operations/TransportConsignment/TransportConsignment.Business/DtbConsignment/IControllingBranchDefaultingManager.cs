using CargoWise.Types;
using Enterprise.Integration.LandTransport;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportConsignment.Business
{
	public interface IControllingBranchDefaultingManager
	{
		IZType FetchDefaultValue(IDtbConsignment entity, ICompany loginCompany, IBranch loginBranch, IDepartment loginDepartment);
	}
}
