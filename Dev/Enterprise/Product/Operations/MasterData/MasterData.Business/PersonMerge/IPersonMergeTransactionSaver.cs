using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public interface IPersonMergeTransactionSaver
	{
		bool IsSuccessful { get; }
		List<IFactory> Factories { get; }
		IPersonMergeTransactionSaver AddParticipant(IFactory factory);
		void Save(GlbPerson retainedPerson, GlbPerson dissolvedPerson);
	}
}
