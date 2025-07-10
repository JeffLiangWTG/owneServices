using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business
{
	public class ProcessEstimateLog : AutoProcessEstimateLog, IProcessEstimateLog
	{
		public ProcessEstimateLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
