using System.Collections;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IWebSecurityRightProvider : IEnumerable<WebSecurityRight>
	{
		int Count { get; }
		bool TryGetValue(string code, out WebSecurityRight right);
	}

	public abstract class WebSecurityRightsProvider : IWebSecurityRightProvider
	{
		readonly IDictionary<string, WebSecurityRight> rights = new Dictionary<string, WebSecurityRight>(50);

		protected void Add(WebSecurityRight securityRight) => rights.Add(securityRight.Code, securityRight);
		public bool TryGetValue(string code, out WebSecurityRight right) => rights.TryGetValue(code, out right);
		public int Count => rights.Count;
		public IEnumerator<WebSecurityRight> GetEnumerator() => rights.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
