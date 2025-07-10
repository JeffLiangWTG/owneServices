#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocCusContainer : DocBaseCusContainer
	{
		DocCusContainer(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocCusContainer New(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			if (cusContainer == null)
			{
				return null;
			}
			else
			{
				return new DocCusContainer(cusContainer, factoryToWrap);
			}
		}

		public static DocCusContainer New(CusContainer cusContainer, JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			var result = New(cusContainer, factoryToWrap);

			if (result != null)
			{
				result.SetDeclaration(declaration);
			}

			return result;
		}
	}
}
#endif
