using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobRequiredDocumentAddInfoDependentCollection : DependentBusinessObjectCollection<JobRequiredDocumentAddInfo, JobRequiredDocument>
	{
		public JobRequiredDocumentAddInfoDependentCollection(JobRequiredDocument requiredDocument)
			: base(requiredDocument)
		{
		}

		public IEnumerable<GlbCompany> GetDistinctCompanies()
		{
			return (from JobRequiredDocumentAddInfo addInfo in this
					where addInfo.Company != null
					select addInfo.Company).Distinct();
		}

		public string GetInstructionsOnHowToDeleteAddInfos(GlbCompany company)
		{
			var result = new ZStringBuilder();

			var distinctApplicationCodes = (from JobRequiredDocumentAddInfo addInfo in this
											where addInfo.EX_GC_Company == company.PK
											select addInfo.EX_ApplicationCode).Distinct();

			foreach (string appID in distinctApplicationCodes)
			{
				var instruction = JobRequiredDocumentAddInfoObjectTypeDecider.GetInstructionsToDeleteDISMessagingRecords(company, appID);
				if (!string.IsNullOrEmpty(instruction))
				{
					result.Append(instruction);
				}
				else
				{
					ErrorReporter.ReportOnce("No instructions on how to delete dbo.JobRequiredDocumentAddInfo is specified for " + company.GC_RN_NKCountryCode + "/" + appID);
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument; }
		}
	}
}
