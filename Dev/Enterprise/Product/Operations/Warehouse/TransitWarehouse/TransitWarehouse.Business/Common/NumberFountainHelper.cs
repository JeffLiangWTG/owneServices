using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class NumberFountainHelper
	{
		public static ZString GetNextReferenceNumber(BusinessObjectFactory objectFactory, INumberFountainProxy numberFountain)
		{
			Argument.NotNull(objectFactory, nameof(objectFactory));
			Argument.NotNull(numberFountain, nameof(numberFountain));

			ZString referenceNumber;

			using (var manager = ((IDbConnected)objectFactory).Connection.BeginTransactionWithManager())
			{
				referenceNumber = numberFountain.GetNextFormatted(objectFactory);
				manager.CommitTransaction();
			}

			return referenceNumber;
		}
	}
}
