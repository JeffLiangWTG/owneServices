using System;
using CargoWise.Types;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public partial class MessageSubTypeList
	{
		public static ZString GetMessageSubTypeFromActionType(ZString actionType)
		{
			switch (actionType)
			{
				case USExportBillOfLadingActionCodeType.Codes.A:
					return MessageSubTypeList.Codes.BillOfLadingAdd;
				case USExportBillOfLadingActionCodeType.Codes.R:
					return MessageSubTypeList.Codes.BillOfLadingReplace;
				case USExportBillOfLadingActionCodeType.Codes.D:
					return MessageSubTypeList.Codes.BillOfLadingDelete;
				default:
					throw new NotSupportedException($"This action type {actionType} is not supported in export manifest yet.");
			}
		}
	}
}
