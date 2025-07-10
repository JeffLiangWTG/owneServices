using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingSource<T> : IPatternMatchingMaintenance where T : BusinessObject
	{
		protected PatternMatchingSource(T bizO)
		{
			this.bizO = bizO;
		}

		protected readonly T bizO;

		protected virtual bool CreateOrUpdateAddress()
		{
			return false;
		}

		protected virtual bool CreateOrUpdateDomain()
		{
			return false;
		}

		protected virtual bool CreateOrUpdateEmail()
		{
			return false;
		}

		protected virtual bool CreateOrUpdateName()
		{
			return false;
		}

		protected virtual bool CreateOrUpdatePhone()
		{
			return false;
		}

		protected virtual bool CreateOrUpdateRegCode()
		{
			return false;
		}

		protected IDuplicateDetectorProvider DuplicateDetectorProvider
		{
			get { return duplicateDetectorProvider = duplicateDetectorProvider ?? (duplicateDetectorProvider = new DuplicateDetectorProvider()); }
		}
		IDuplicateDetectorProvider duplicateDetectorProvider;

		protected ZInt GetHash(string valueToHash)
		{
			return !string.IsNullOrEmpty(valueToHash) ? TextStandardizerHelper.ComputeStringHashFast(valueToHash) : 0;
		}

		public bool CreateOrUpdatePatternMatchingTables()
		{
			var shouldSave = CreateOrUpdateAddress() |
							CreateOrUpdateDomain() |
							CreateOrUpdateEmail() |
							CreateOrUpdateName() |
							CreateOrUpdatePhone() |
							CreateOrUpdateRegCode();

			if (shouldSave)
			{
				QueueMasterForProcessing();
			}

			return shouldSave;
		}

		protected abstract void QueueMasterForProcessing();

		protected void QueueOrgForDeduplicationProcessing(OrgHeader header)
		{
			PatternMatchingResultsEnqueuer.QueueOrgHeaderForDeduplicationProcessing(header, header.Factory);
		}

		protected void QueuePersonForDeduplicationProcessing(GlbPerson person)
		{
			if (person != null)
			{
				PatternMatchingResultsEnqueuer.QueueGlbPersonForDeduplicationProcessing(person, person.Factory);
			}
		}
	}
}
