using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Warehouse.Transactions.Business
{
	class PalletIDFromSSCCGenerator : IPalletIDFromSSCCGenerator
	{
		public IEnumerable<ZString> GenerateIDs(ZString ssccPrefix, int numberOfIDs)
		{
			Argument.NotNullOrEmpty(ssccPrefix, nameof(ssccPrefix));
			Argument.GreaterThanZero(numberOfIDs, nameof(numberOfIDs));

			var ids = new List<ZString>(numberOfIDs);

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.RunTransactioned(() =>
				{
					var idsArr = Env.NumberFountains.SSCCBarCode(ssccPrefix).GetNextsFormatted(connection, numberOfIDs);
					ids.AddRange(idsArr.Select(i => (ZString)i));
				});
			}

			return ids;
		}
	}
}
