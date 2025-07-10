using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.US.Business
{
	public class CustomsDocDataObjectProvider : ICustomsDocDataObjectProvider
	{
		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (parent is JobDeclaration declaration)
			{
				return GetFromDeclaration(declaration, dataContext, parameters);
			}

			return null;
		}

		object GetFromDeclaration(JobDeclaration declaration, string dataContext, IDocDataObjectParameters parameters)
		{
			switch (dataContext)
			{
				case DataContext.USATF6A:
					return new ATF6ADataBuilder(declaration, parameters).Build();
			}

			return null;
		}
	}
}
