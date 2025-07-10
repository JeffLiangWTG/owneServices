using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.Customs.TR.Business
{
	public interface IRegistrationNoEntryProvider
	{
		ZString RegistrationNumber { get; set; }
		ZDateTime RegistrationDate { get; set; }
		BusinessObject ParentBusinessObject { get; }
		SecurityCheckpoint CanModifyRegistrationNumbers { get; }
	}
}
