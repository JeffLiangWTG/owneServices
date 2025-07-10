using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common.Testing;

namespace Enterprise.MasterFiles.Business
{
	public static class DocumentAssemblyDataProxy
	{
		public static string GetReferenceTypeFromDocManagerCode(string code)
		{
			lock (codeDictionary)
			{
				string docManagerCode;
				if (!codeDictionary.TryGetValue(code, out docManagerCode))
				{
					docManagerCode = (string)ObjectFactory.GetType<DocumentScanning.Integration.IAssemblyDataLookup>().GetMethod("GetReferenceTypeFromDocManagerCode", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { code });
					codeDictionary.Add(code, docManagerCode);
				}

				return docManagerCode;
			}
		}

		[SuppressThreadStaticFieldMessage]
		static readonly Dictionary<string, string> codeDictionary = new Dictionary<string, string>();
	}
}

