using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class ReconWorkflowHelper
	{
		public static ColumnValueRanker GetTemplateSelectionCriteria(JobDeclaration jobdeclaration)
		{
			var result = new ColumnValueRanker();
			if (jobdeclaration != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_GB, jobdeclaration.JE_GB, ZGuid.Empty);

				var clientList = new List<IZType>();
				if (jobdeclaration.JE_OH_Importer.IsValid)
				{
					clientList.Add(jobdeclaration.JE_OH_Importer);
				}
				var rec = jobdeclaration.ReconDeclaration;
				if (rec != null)
				{
					var job = new JobHeader.Loader(rec).Load();
					if (job != null)
					{
						clientList.Add(job.LocalChargesPK);
					}
				}
				clientList.Add(ZGuid.Empty);
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, clientList.ToArray());
			}

			return result;
		}

		public static ZString WorkflowType
		{
			get { return WorkflowDescriptors.ReconWorkflowDescriptorCode; }
		}
	}
}
