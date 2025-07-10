using System.Linq;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects
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
				case DataContext.CargoDuesBrokerage:
					return new CargoDuesBrokerageDocDataBuilder(declaration, parameters).Build();
				case DataContext.DA66DA63Document:
				{
					var header = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
					return header != null ? new DA66DA63DocumentWrapper(header) : null;
				}
				case DataContext.DA306Document:
					return new DA306DeclarationWrapper(declaration);
			}

			return null;
		}
	}
}
