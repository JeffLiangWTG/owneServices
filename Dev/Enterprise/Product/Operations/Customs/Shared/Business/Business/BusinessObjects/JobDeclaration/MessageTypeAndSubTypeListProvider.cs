using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class MessageTypeAndSubTypeListProvider : Integration.Customs.Shared.IMessageTypeAndSubTypeListProvider
	{
		public MessageTypeAndSubTypeListProvider()
		{
		}

		public ICodeDescriptionPairList MessageTypeList(BusinessObjectFactory factory, ZString companyCode)
		{
			return factory.GetCachedValue($"MessageTypeListFor_{companyCode}", () =>
			{
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					return GetMessageTypeListCore(factory);
				}
			});
		}

		protected virtual CodeDescriptionPairList GetMessageTypeListCore(BusinessObjectFactory factory)
		{
			var declaration = factory.GetNull<BaseJobDeclaration>();
			return declaration.Lookups.MessageTypeList;
		}

		public ICodeDescriptionPairList MessageSubTypeList(BusinessObjectFactory factory, ZString companyCode, ZString messageType)
		{
			return factory.GetCachedValue($"MessageSubTypeListFor_{companyCode}_{messageType}", () =>
			{
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					return GetMessageSubTypeListCore(factory, messageType);
				}
			});
		}

		protected virtual CodeDescriptionPairList GetMessageSubTypeListCore(BusinessObjectFactory factory, ZString messageType)
		{
			var declaration = factory.GetNull<BaseJobDeclaration>();
			declaration.JE_MessageType = messageType;
			return declaration.Lookups.MessageSubTypeList;
		}
	}
}
