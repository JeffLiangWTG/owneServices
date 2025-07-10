namespace Enterprise.Customs.US.Business
{
	partial class USContainerCodeList
	{
		public static USContainerCodeList GetEquipmentDescriptionCodeList()
		{
			var result = new USContainerCodeList();
			result.RemoveCode(USContainerCodeList.Codes._20T0);
			result.RemoveCode(USContainerCodeList.Codes._20T1);
			result.RemoveCode(USContainerCodeList.Codes._20T2);
			result.RemoveCode(USContainerCodeList.Codes._20T3);
			result.RemoveCode(USContainerCodeList.Codes._20T4);
			result.RemoveCode(USContainerCodeList.Codes._20T5);
			result.RemoveCode(USContainerCodeList.Codes._20T6);
			result.RemoveCode(USContainerCodeList.Codes._20T7);
			result.RemoveCode(USContainerCodeList.Codes._20T8);
			result.RemoveCode(USContainerCodeList.Codes._20T9);

			result.RemoveCode(USContainerCodeList.Codes._25T2);
			result.RemoveCode(USContainerCodeList.Codes._22T0);
			result.RemoveCode(USContainerCodeList.Codes._22T5);
			result.RemoveCode(USContainerCodeList.Codes._22T7);
			result.RemoveCode(USContainerCodeList.Codes._22T8);
			result.RemoveCode(USContainerCodeList.Codes._42T0);
			return result;
		}
	}
}
