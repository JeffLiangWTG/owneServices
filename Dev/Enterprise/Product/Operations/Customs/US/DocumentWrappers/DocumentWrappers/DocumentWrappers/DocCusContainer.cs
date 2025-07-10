using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.US.DocumentWrappers
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

		#region Overrides

		public override ZInt TotalAllocatedJobPackages
		{
			get
			{
				ZInt result = 0;
				foreach (BasePackage package in CusContainer.Packages)
				{
					result += package.CW_PackQty;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		CusContainer CusContainer
		{
			get { return (CusContainer)WrappedObject; }
		}

		#endregion
	}
}
