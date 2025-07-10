using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	static class CusEntryHeaderLinker
	{
		public static CusEntryHeader Link(MQEDIMessage message)
		{
			return OriginalMessageLinker.Link<CusEntryHeader>(message);
		}

		/// <summary>
		/// If there are more than one finds, it does not link it up
		/// </summary>
		public static CusEntryHeader Link(ZString entryNumber, ZString filerCode, MQEDIMessage message, params ZString[] messageCodeTypes)
		{
			var result = new CusEntryHeader.Loader(message.Factory).FindByEntryNumberAndFilerCode(message.Branch.GB_GC, entryNumber, filerCode, messageCodeTypes);
			if (result != null)
			{
				message.EM_LinkedObject = result;
			}

			return result;
		}
	}
}
