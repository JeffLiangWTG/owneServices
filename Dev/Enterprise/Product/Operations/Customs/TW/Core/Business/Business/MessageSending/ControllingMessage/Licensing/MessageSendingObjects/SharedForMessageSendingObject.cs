using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	partial class LicensingMessageSendingObject
	{
		protected CodeDescriptionPairList GetNX201TypeListCore(BusinessObjectFactory factory, ZString businessType)
		{
			var typeListCacheKey = GetNX201TypeListCacheKey(businessType);
			return factory.GetCachedValue($"Enterprise.Customs.TW.Business.NX201MessageSendingObject.GetTypeListCore|{typeListCacheKey}", () =>
			{
				var result = new CodeDescriptionPairList();
				if (typeListCacheKey == NX201TypeListA)
				{
					result.AddRange(new NX201DocumentTypeList().Cast<CodeDescriptionPair>().Where(x => x.Code != NX201DocumentTypeList.Codes._99).ToList());
				}
				else if (typeListCacheKey == NX201TypeListB)
				{
					result.AddPair(NX201DocumentTypeList.Codes._99, NX201DocumentTypeList.Descriptions._99);
				}
				return result;
			});
		}

		string GetNX201TypeListCacheKey(ZString businessType)
		{
			var result = string.Empty;
			switch (businessType)
			{
				case NX902_TypeOfApplicationCodeList.Codes._1:
				case NX902_TypeOfApplicationCodeList.Codes._2:
					result = NX201TypeListA;
					break;
				case NX902_TypeOfApplicationCodeList.Codes._0:
				case NX902_TypeOfApplicationCodeList.Codes._3:
					result = NX201TypeListB;
					break;
				default:
					break;
			}
			return result;
		}

		const string NX201TypeListA = "TypeA";
		const string NX201TypeListB = "TypeB";
	}
}
