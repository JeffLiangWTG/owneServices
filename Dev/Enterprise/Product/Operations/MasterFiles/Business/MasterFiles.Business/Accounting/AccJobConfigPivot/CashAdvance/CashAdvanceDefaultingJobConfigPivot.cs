using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class CashAdvanceDefaultingJobConfigPivot : AccJobConfigPivot
	{
		public CashAdvanceDefaultingJobConfigPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(JobConfiguration))]
		public override ZGuid JCT_JCF_JobConfig
		{
			get => base.JCT_JCF_JobConfig;
			set => base.JCT_JCF_JobConfig = value;
		}

		public AccCashAdvanceDefaultingConfiguration JobConfiguration => Factory.Load<AccCashAdvanceDefaultingConfiguration>(JCT_JCF_JobConfig);
	}
}
