using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public class JobContainerPackPivot : AutoJobContainerPackPivot, Integration.IJobContainerPackPivot
	{
		public JobContainerPackPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public override bool IsSavedByFactory
		{
			get { return !IsNewAndAllPropertiesExceptAuditColumnsAreDuplicatesOfExistingBusinessObject; }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobContainerPackPivotFetchStrategy(this);
		}

		[RelatedBusinessObject("PackLine")]
		public override ZGuid J6_JL
		{
			get { return base.J6_JL; }
			set
			{
				base.J6_JL = value;
				ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("JobContainerPackPivot.J6_JL_setter", J6_JL, J6_JC, Factory);
			}
		}

		public PackLine PackLine
		{
			get { return Factory.Load<PackLine>(J6_JL); }
		}

		[RelatedBusinessObject("Container")]
		public override ZGuid J6_JC
		{
			get { return base.J6_JC; }
			set
			{
				base.J6_JC = value;
				ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("JobContainerPackPivot.J6_JC_setter", J6_JL, J6_JC, Factory);
			}
		}

		public CommonContainer Container
		{
			get { return Factory.Load<CommonContainer>(J6_JC); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			ContainerPackLineRelationshipHelper.ReportAbsentConShipLink("JobContainerPackPivot.OnSaving()", J6_JL, J6_JC, Factory);
		}
	}
}
