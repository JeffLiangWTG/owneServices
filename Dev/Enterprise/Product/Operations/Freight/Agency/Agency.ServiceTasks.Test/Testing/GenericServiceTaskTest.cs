using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class GenericServiceTaskTest : TestCase
	{
		public void TestAssertNoDuplicateTypes()
		{
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (var pair in GetGroupedHostedServiceAtts((a) => a.TypeName))
			{
				if (pair.Value.Count > 1)
				{
					result.Add(pair.Key, pair.Value.ConvertAll((a) => a.Code + " - " + a.Description));
				}
			}

			AssertGroupedErrorList("These types are used by multiple service tasks.", result);
		}

		public void TestAssertNoDuplicateCodes()
		{
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (var pair in GetGroupedHostedServiceAtts((a) => a.Code))
			{
				if (pair.Value.Count > 1)
				{
					result.Add(pair.Key, pair.Value.ConvertAll((a) => a.TypeName));
				}
			}

			AssertGroupedErrorList("These codes are used by multiple service tasks.", result);
		}

		public void TestAssertNoDuplicateDescriptions()
		{
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			foreach (var pair in GetGroupedHostedServiceAtts((a) => a.Description))
			{
				if (pair.Value.Count > 1)
				{
					result.Add(pair.Key, pair.Value.ConvertAll((a) => a.TypeName));
				}
			}

			AssertGroupedErrorList("These codes are used by multiple service tasks.", result);
		}

		#region Implementation
		static Dictionary<T, List<HostedServiceAttribute>> GetGroupedHostedServiceAtts<T>(Converter<HostedServiceAttribute, T> keyGetter)
		{
			Dictionary<T, List<HostedServiceAttribute>> result = new Dictionary<T, List<HostedServiceAttribute>>();
			foreach (HostedServiceAttribute att in Attribute.GetCustomAttributes(Assembly.GetExecutingAssembly(), typeof(HostedServiceAttribute)))
			{
				T key = keyGetter(att);
				List<HostedServiceAttribute> list;
				if (!result.TryGetValue(keyGetter(att), out list))
				{
					list = new List<HostedServiceAttribute>();
					result.Add(key, list);
				}

				list.Add(att);
			}

			return result;
		}
		#endregion
	}
}
