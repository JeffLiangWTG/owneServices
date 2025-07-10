
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
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
			{ return new DocCusContainer(cusContainer, factoryToWrap); }
		}

		public static DocCusContainer New(CusContainer cusContainer, JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			DocCusContainer result = New(cusContainer, factoryToWrap);

			if (result != null)
			{
				result.SetDeclaration(declaration);
			}

			return result;
		}

		public ZBool IsContainerToBeAdvised
		{
			get { return CusContainer.IsContainerToBeAdvised; }
		}

		#region Implementation

		CusContainer CusContainer
		{
			get { return (CusContainer)WrappedObject; }
		}

		#endregion
	}
}
