using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business
{
	public interface IISFDocAddress : IDocAddress
	{
		ZString E2_Contact { get; }
		ZString E2_SocialSecurityNumber { get; }
		ZDateTime E2_SocialSecurityNumberDateOfBirth { get; }
	}
}
