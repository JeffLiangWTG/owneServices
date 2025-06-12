using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.DataModel.Business
{
	public class ClientSystemRegistrationStatusFactory
	{
		public static Dictionary<int, string> GetClientSystemRegistrationStatus(string registrationType)
		{
			if (!CacheStatusList.ContainsKey(registrationType))
			{
				var codeAndDescription = new Dictionary<int, string>();
				var assemblies = GetAssemblies();

				foreach (Assembly assembly in assemblies)
				{
					foreach (Type type in assembly.GetTypes().Where(t => t.IsEnum))
					{
						var attributes = type.GetCustomAttributes(typeof(ClientSystemRegistrationStatusAttribute), false);

						foreach (ClientSystemRegistrationStatusAttribute attribute in attributes)
						{
							if (attribute.RegistrationType == registrationType)
							{
								foreach (var statusCode in Enum.GetValues(type))
								{
									codeAndDescription.Add((int) statusCode, statusCode.ToString());
								}
							}
						}
					}
						
				}
				CacheStatusList.Add(registrationType, codeAndDescription);
				return codeAndDescription;
			}
			
			return CacheStatusList[registrationType];
		}

		static IEnumerable<Assembly> GetAssemblies()
		{
			var assemblies = new List<Assembly>();
			const string microsoftProduct = "Microsoft";

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (AssemblyProductAttribute product in assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false))
				{
					if (!product.Product.Contains(microsoftProduct))
					{
						assemblies.Add(assembly);
					}
				}
			}

			return assemblies;
		}
		
		static readonly Dictionary<string, Dictionary<int, string>> CacheStatusList = new Dictionary<string, Dictionary<int, string>>();
	}
}
