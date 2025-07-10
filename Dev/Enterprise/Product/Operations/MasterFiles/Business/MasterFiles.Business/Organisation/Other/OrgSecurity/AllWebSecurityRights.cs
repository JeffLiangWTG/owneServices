using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	public class AllWebSecurityRights : IWebSecurityRightProvider
	{
		readonly IList<IWebSecurityRightProvider> providers = new List<IWebSecurityRightProvider>();

		protected AllWebSecurityRights() { }

		public int Count => providers.Sum(p => p.Count);

		protected void Add(IWebSecurityRightProvider provider) => providers.Add(provider);

		public WebSecurityRight FindByCode(string code)
		{
			WebSecurityRight right;
			foreach (var provider in providers)
			{
				if (provider.TryGetValue(code, out right))
				{
					return right;
				}
			}

			return null;
		}

		public bool ContainsCode(string code)
		{
			WebSecurityRight ignored;
			return providers.Any(provider => provider.TryGetValue(code, out ignored));
		}

		public WebSecurityRight[] FindByApplication(WebSecurityApplication application)
		{
			return this.Where(element => element.WebApplication == application).ToArray();
		}

		#region IEnumerable Members

		public IEnumerator<WebSecurityRight> GetEnumerator() => providers.SelectMany(p => p).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		bool IWebSecurityRightProvider.TryGetValue(string code, out WebSecurityRight right)
		{
			right = FindByCode(code);
			return right != null;
		}

		#endregion

		protected delegate AllWebSecurityRights NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public static AllWebSecurityRights New(BusinessObjectFactory factory)
		{
			AllWebSecurityRights result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new AllWebSecurityRights();

				result.Add(WebSecurityRightsList.New());
				result.Add(ReportsWebSecurityRights.New(factory));
				result.Add(new DocumentWebSecurityRights(factory));
			}
			return result;
		}
	}
}
