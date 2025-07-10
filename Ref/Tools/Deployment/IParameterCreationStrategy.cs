using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Deployment
{
	interface IParameterCreationStrategy
	{
		List<(string paramName, object paramValue)> CreateParameters(TestRig testRig);
	}
}
