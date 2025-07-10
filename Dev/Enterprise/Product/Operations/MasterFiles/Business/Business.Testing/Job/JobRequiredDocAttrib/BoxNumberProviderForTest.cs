using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BoxNumberProviderForTest : IBoxNumberProvider
	{
		ICodeDescriptionPairList IBoxNumberProvider.GetBoxNumberList(BusinessObjectFactory factory, ZString customsDistrict)
		{
			var result = new CodeDescriptionPairList();
			if (customsDistrict == "A")
			{
				result.AddPair("111");
				result.AddPair("222");
			}
			else if (customsDistrict == "B")
			{
				result.AddPair("333");
			}
			return result;
		}

		ZString IBoxNumberProvider.GetDefaultBoxNumber(BusinessObjectFactory factory, ZString customsDistrict)
		{
			if (customsDistrict == "A")
			{
				return "111";
			}
			else if (customsDistrict == "B")
			{
				return "333";
			}
			else
			{
				return "";
			}
		}
	}
}
