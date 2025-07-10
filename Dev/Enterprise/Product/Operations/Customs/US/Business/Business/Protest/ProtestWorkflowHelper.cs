using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class ProtestWorkflowHelper
	{
		public static ColumnValueRanker GetTemplateSelectionCriteria(JobDeclaration declaration)
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_GB, declaration.JE_GB, ZGuid.Empty);

			var clientList = new List<IZType>();
			var protest = declaration.Protest ?? new Protest.Protest(declaration);
			var protestant = protest.Protestant;
			if (protestant != null && protestant.Address != null && protestant.Address.OA_OH.IsValid)
			{
				clientList.Add(protestant.Address.OA_OH);
			}

			if (protest != null)
			{
				var job = new JobHeader.Loader(protest).Load();
				if (job != null)
				{
					clientList.Add(job.LocalChargesPK);
				}
			}

			clientList.Add(ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, clientList.ToArray());

			return result;
		}

		public static ZString WorkflowType
		{
			get { return WorkflowDescriptors.ProtestWorkflowDescriptorCode; }
		}
	}
}
