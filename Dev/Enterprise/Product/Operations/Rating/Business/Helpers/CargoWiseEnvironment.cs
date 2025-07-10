using Enterprise.Environment;

namespace Enterprise.Rating.Business
{
	public interface ICargoWiseEnvironment
	{
		bool IsCargoWiseDomain(string domain);
	}

	public class CargoWiseEnvironment : ICargoWiseEnvironment
	{
		public bool IsCargoWiseDomain(string domain) => Env.IsCargoWiseDomain(domain);
	}
}