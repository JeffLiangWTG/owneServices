using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class JobConfigurationSelectorReadOnly
	{
		public JobConfigurationSelectorReadOnly(IJobConfigurationSelector parent)
		{
			fParent = parent;
		}

		protected IJobConfigurationSelector Parent => fParent;
		readonly IJobConfigurationSelector fParent;

		#region Direction

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI007:CustomizableDataTranslationRule", Justification = "Debug only code")]
		public bool DirectionCode_ReadOnly
		{
			get
			{
				var result = JobConfigurationSelectorReadOnlyHelper.DirectionCode_ReadOnly(Parent.JobType);
#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region Mode

		public bool Mode_ReadOnly
		{
			get
			{
				var result = JobConfigurationSelectorReadOnlyHelper.Mode_ReadOnly(Parent.JobType);

#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion
	}
}
