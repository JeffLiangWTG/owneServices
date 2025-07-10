using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Extensions
{
	public static class DocManagerInfoExtensions
	{
		public static void ForceToUseAnotherFactory(this DocManagerInfo docManagerInfo, BusinessObjectFactory factoryToBeUsed)
		{
			Argument.NotNull(docManagerInfo, nameof(docManagerInfo));
			Argument.NotNull(factoryToBeUsed, nameof(factoryToBeUsed));

			var docManagerInfoFactory = (BusinessObjectFactory)docManagerInfo.MasterFactory;

			DetachFactoryToBeUsedFromDocumentFactory();
			AttachDocumentFactoryToFactoryToBeUsed();

			void DetachFactoryToBeUsedFromDocumentFactory()
			{
				if (docManagerInfoFactory.ChildFactories.Contains(factoryToBeUsed))
				{
					docManagerInfoFactory.ChildFactories.Remove(factoryToBeUsed);
				}
			}

			void AttachDocumentFactoryToFactoryToBeUsed()
			{
				if (!factoryToBeUsed.ChildFactories.Contains(docManagerInfoFactory))
				{
					factoryToBeUsed.ChildFactories.Add(docManagerInfoFactory);
				}
			}
		}
	}
}
