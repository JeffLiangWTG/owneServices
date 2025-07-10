using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public sealed class SYSTBL_NG_9000_MSG_SystemTableRequestWrapper : ISYSTBL_NG_9000_MSG_SystemTableRequest
	{
		public SYSTBL_NG_9000_MSG_SystemTableRequestWrapper(string tableName, DateTime transmissionDateTime)
		{
			this.tableName = Argument.NotNullOrEmpty(tableName, nameof(tableName));
			this.transmissionDateTime = transmissionDateTime;
		}

		public IRequestContentHeader RequestContentHeader => new RequestContentHeaderWrapper(transmissionDateTime);

		public ISYSTBL_NG_9000_MSG_SystemTableRequestSelectOptions SelectOptions => new SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptionsWrapper();

		public string TableName => tableName;

		readonly string tableName;
		readonly DateTime transmissionDateTime;
	}
}
