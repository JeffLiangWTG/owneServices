using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public sealed class OAuth2ScopesCollection : NonPersistentBusinessObjectCollection<OAuth2Scope>
	{
		public OAuth2ScopesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public string[] ToStringArray()
		{
			List<string> result = new List<string>();
			foreach (OAuth2Scope element in this)
			{
				result.Add(element.ScopeName);
			}
			return result.ToArray();
		}

		public override int GetHashCode()
		{
			int num = 0;
			foreach (OAuth2Scope item in base.Elements.Cast<OAuth2Scope>())
			{
				num ^= item.GetHashCode();
			}

			return num;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OAuth2Scope();
		}
	}
}
