using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class IDocketTypeExtensions
	{
		public static void ThrowImportFailureExceptionIfNotEmpty(this IDocketType docketType, ZStringBuilder builder)
		{
			if (!builder.IsEmpty)
			{
				builder.Prepend(Res.GetString("adfc0d5b-f0a9-4f6f-b122-c52014048e76", "Cannot Import {0}", docketType.DocketType));
				throw new DataObjectReadFailureException(builder.ToStringWithNewLineBetweenAppends());
			}
		}
	}
}
