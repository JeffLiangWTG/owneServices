using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business
{
	class JobRequiredDocumentConcurrencyChecker : SaveInTransactionActionWithFactory
	{
		JobRequiredDocumentConcurrencyChecker(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static void Register(BusinessObjectFactory factory)
		{
			if (!IsRegistered(factory))
			{
				var participant = new JobRequiredDocumentConcurrencyChecker(factory);
				factory.SaveInTransactionActions.Add(participant);
			}
		}

		public static bool IsRegistered(BusinessObjectFactory factory)
		{
			return factory != null && factory.SaveInTransactionActions.OfType<JobRequiredDocumentConcurrencyChecker>().Any();
		}

		protected override bool AllowTransactionWithOtherParticipant => true;

		protected override IChangedTableNames SaveInTransaction()
		{
			var query = new ZQuery { FetchOnlyFromLocalCache = true };

			var requiredDocuments = Factory.Load<JobRequiredDocument>(query)
				.Where(r => !r.IsInDatabase && !r.IsDeleted)
				.ToArray();

			if (requiredDocuments.Any())
			{
				RunConcurrencyCheck(requiredDocuments);
			}

			return ChangedTableNames.Empty;
		}

		void RunConcurrencyCheck(IEnumerable<JobRequiredDocument> requiredDocuments)
		{
			foreach (var requiredDocument in requiredDocuments)
			{
				if (requiredDocument.IsDocTypeDuplicate)
				{
					requiredDocument.Delete();
				}
			}
		}
	}
}
