using System.Web.UI;

namespace Enterprise.MarketingManager.WebVoting
{
	public delegate void InstantiateTemplateDelegate(Control container);

	public class GenericTemplateImplementation : ITemplate
	{
		public GenericTemplateImplementation(InstantiateTemplateDelegate instantiateTemplate)
		{
			InstantiateTemplate = instantiateTemplate;
		}

		public void InstantiateIn(Control container)
		{
			if (InstantiateTemplate != null)
			{
				InstantiateTemplate(container);
			}
		}

		public InstantiateTemplateDelegate InstantiateTemplate
		{
			get;
			set;
		}
	}
}
