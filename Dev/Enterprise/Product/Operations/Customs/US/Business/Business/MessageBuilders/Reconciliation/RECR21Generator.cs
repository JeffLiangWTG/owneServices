using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class RECR21Generator
	{
		public IEnumerable<RECR21> Generate(IEnumerable<IReconciliationImportEntryFee> fees)
		{
			int trailerNumber = 0;

			List<IReconciliationImportEntryFee> threeFees = new List<IReconciliationImportEntryFee>();

			List<IReconciliationImportEntryFee> allFees = new List<IReconciliationImportEntryFee>();
			allFees.AddRange(fees);
			int countTo = allFees.Count - 1;
			for (int i = 0; i <= countTo; i++)
			{
				threeFees.Add(allFees[i]);
				if (threeFees.Count == 3 || i == countTo)
				{
					trailerNumber++;
					yield return RECR21Populator.Populate(trailerNumber, threeFees);
					threeFees.Clear();
				}
			}
		}
	}
}
