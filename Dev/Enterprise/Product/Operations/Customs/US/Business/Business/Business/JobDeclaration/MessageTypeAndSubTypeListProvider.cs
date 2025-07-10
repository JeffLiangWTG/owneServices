using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class MessageTypeAndSubTypeListProvider : Customs.Business.MessageTypeAndSubTypeListProvider
	{
		public MessageTypeAndSubTypeListProvider()
		{
		}

		protected override CodeDescriptionPairList GetMessageTypeListCore(BusinessObjectFactory factory)
		{
			return new Common.US.USJobMessageTypeList();
		}

		protected override CodeDescriptionPairList GetMessageSubTypeListCore(BusinessObjectFactory factory, ZString messageType)
		{
			var result = new CodeDescriptionPairList();

			if (messageType.IsEmpty || messageType == JobMessageTypeList.Codes.Import || messageType == JobMessageTypeList.Codes.ImportByExternalBroker || messageType == JobMessageTypeList.Codes.Miscellaneous)
			{
				result = EntryTypeList.GetACEList();
			}
			else if (messageType == JobMessageTypeList.Codes.Drawback)
			{
				result = ACEDrawbackProvisionsList.GetDrawbackProvisionList(factory);
			}

			return result;
		}
	}
}
