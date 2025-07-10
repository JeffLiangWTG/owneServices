using CargoWise.Common.Testing;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Messaging.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class OriginalMessageLinker
	{
		public static T Link<T>(CBPEDIMessage message)
			where T : BusinessObject
		{
			var bizObj = Link(message) as T;
			if (bizObj == null)
			{
				message.EM_LinkedObject = null;
			}
			return bizObj;
		}

		public static BusinessObject Link(CBPEDIMessage message)
		{
			BusinessObject result = null;
			if (message.EM_LinkUniqueID.IsValid)
			{
				result = message.EM_LinkedObject;
			}
			if (result == null && message.OriginalMessage is CBPEDIMessage originalMessage && originalMessage.EM_LinkedObject is BusinessObject linkedObject)
			{
				result = linkedObject;
				message.EM_LinkedObject = linkedObject;
			}

			return result;
		}
	}
}
