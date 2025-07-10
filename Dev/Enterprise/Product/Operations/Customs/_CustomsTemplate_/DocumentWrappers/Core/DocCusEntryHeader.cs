#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryHeader == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
			}
		}
	}
}
#endif
