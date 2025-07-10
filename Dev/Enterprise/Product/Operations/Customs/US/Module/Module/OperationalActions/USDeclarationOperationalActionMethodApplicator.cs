using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public abstract class USDeclarationOperationalActionMethodApplicator : USOperationalActionMethodApplicator
	{
		public USDeclarationOperationalActionMethodApplicator(string description, BusinessObjectFactory factory)
			: base(description, factory)
		{
		}

		public new abstract class Schema : USOperationalActionMethodApplicator.Schema
		{
			public const string ApportionWeight = "ApportionWeight";
		}

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			if (!IsCancelled)
			{
				base.SummaryLogCore(log);

				if (jobsPK != null && jobsPK.Count > 0)
				{
					foreach (var declaration in Factory.Load<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, jobsPK.ToArray())))
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							"Message for declaration {0} has been sent.", // params must be injected by NotifyFormat, not Res.GetString
							new object[] { declaration.GetDeclarationIdLink() });
					}

					log.Notify(OperationalActionLogErrorLevel.Informational, "Click the above declaration(s) to see the message.\r\n");
				}
			}
		}

		public ZBool ApportionWeight
		{
			get => apportionWeight;
			set => SetNonPersistentPropertyValue(apportionWeightInfo, ref apportionWeight, value);
		}
		ZBool apportionWeight;

		public ZPropertyInfo apportionWeightInfo => GetZPropertyInfo(Schema.ApportionWeight);
	}
}
