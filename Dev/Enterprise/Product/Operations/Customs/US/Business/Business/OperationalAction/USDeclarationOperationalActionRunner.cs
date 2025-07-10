using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public abstract class USDeclarationOperationalActionRunner : USOperationalActionRunner<JobDeclaration>
	{
		public USDeclarationOperationalActionRunner(IOperationalActionSectionLog log)
			: base(log)
		{
		}

		public IEnumerable<ZGuid> PerformFunctionOperationalAction(bool sendWithMessageErrors, BusinessObject[] targets, bool apportionWeight)
		{
			this.apportionWeight = apportionWeight;
			return PerformFunctionOperationalAction(sendWithMessageErrors, targets);
		}

		bool apportionWeight;

		protected override string TypeOfJob => "declaration";

		protected sealed override OperationalActionBulkMessageSender<JobDeclaration> GetMessageSender(JobDeclaration job)
		{
			if (apportionWeight)
			{
				job.JE_AutoWeightApportion = true;
			}

			return GetMessageSenderCore(job);
		}

		protected abstract OperationalActionBulkMessageSender<JobDeclaration> GetMessageSenderCore(JobDeclaration job);

		protected override void UnlockMergeMutexIfNecessary(JobDeclaration job)
		{
			job.UnlockDoMergeMutex();
		}

		protected override LogControllerLink GetLogControllerLink(JobDeclaration job)
		{
			return job.GetDeclarationIdLink();
		}
	}
}
