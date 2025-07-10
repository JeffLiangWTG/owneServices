using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz.Xml.JobSchedulingData20;

namespace CargoWise.RefDbRepo.ApplicationConfigurationGenerator
{
	interface IJobDetailReader
	{
		Task<List<jobdetailType>> GetJobDetails();
	}
}
