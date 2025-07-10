using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.Business
{
	public class JobDeclarationProcessTaskCollection<TJobDeclaration> : ProcessTaskCollection<BaseJobDeclarationProcessTask<TJobDeclaration>, TJobDeclaration>
		where TJobDeclaration : BaseJobDeclaration, IWorkflowProvider
	{
		readonly JobDeclarationProcessTaskConditionChecker conditionChecker;

		public JobDeclarationProcessTaskCollection(TJobDeclaration declaration)
			: base(declaration)
		{
			conditionChecker = new JobDeclarationProcessTaskConditionChecker(Parent);
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.Origin == null ? ZString.Empty : Parent.Origin.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.FinalDestination == null ? ZString.Empty : Parent.FinalDestination.RL_RN_NKCountryCode; }
		}

		#endregion

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode) => conditionChecker.IsCondition1Met(conditionCode);

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value) => conditionChecker.IsCondition2Met(conditionCode, value);

		#endregion
	}
}
