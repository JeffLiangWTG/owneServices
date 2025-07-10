using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DocAddressTypes : AutoDocAddressTypes
	{
		public const string OrgAddressTypeCode = "OrgAddress";

		public static DocAddressType GetDocAddressTypeFromCode(BusinessObjectFactory factory, string docAddressTypeCode)
		{
			foreach (DocAddressType docAddress in Enum.GetValues(typeof(DocAddressType)))
			{
				if (docAddressTypeCode == DocAddressTypes.GetCode(factory, docAddress))
				{
					return docAddress;
				}
			}

			return DocAddressType.None;
		}

		public static CodeDescriptionPairList GetAddressTypesCodeDescriptionPairList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AddressTypesCodeDescriptionPairList", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgAddressTypeCode, Res.GetString("8fbd767c-5b2c-4f30-b583-c0b04fa48fd5", "Organization Address"));
				var codes = GetCodes(factory);
				foreach (var code in codes)
				{
					if (code == DocAddressTypes.None)
					{
						var noneString = Res.GetString("DocAddressTypes|GetAddressTypesCodeDescriptionPairList|StringForNone", "None");
						list.AddPair(noneString, noneString);
					}
					else
					{
						list.AddPair(code, GetDescription(factory, GetDocAddressTypeFromCode(factory, code)));
					}
				}
				return list;
			});
		}

		public static ZString Unspecified { get { return Res.GetString("ee07f1a6-bdaf-4713-a35e-33cb722234b4", "Unspecified"); } }
		public static readonly ZString None = "?";

		public static ZString GetCode(BusinessObjectFactory factory, DocAddressType docAddressType)
		{
			return (docAddressType == DocAddressType.None) ? DocAddressTypes.None : factory.GetCachedValue<DocAddressTypes>().GetCodeCore(docAddressType);
		}

		public static CodeDescriptionPair GetPair(BusinessObjectFactory factory, DocAddressType docAddressType)
		{
			return factory.GetCachedValue<DocAddressTypes>().GetPairCore(docAddressType);
		}

		public static ZString GetDescription(BusinessObjectFactory factory, DocAddressType docAddressType)
		{
			return (docAddressType == DocAddressType.None) ? DocAddressTypes.None.ToString() : factory.GetCachedValue<DocAddressTypes>().GetPairCore(docAddressType).Description;
		}

		public static IEnumerable<ZString> GetCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<DocAddressTypes>().GetCodesCore(factory);
		}

		public ZString GetCodeCore(DocAddressType addressType)
		{
			ZString code = "";
			if (addressType != DocAddressType.None)
			{
				if (!CodeCache.TryGetValue(addressType, out code))
				{
					var codeField = typeof(Codes).GetField(addressType.ToString(), BindingFlags.Public | BindingFlags.Static)
						?? throw new ArgumentException(addressType.ToString() + " does not have a corresponding entry in AutoDocAddressTypes.xml", nameof(addressType));

					code = new ZString(codeField.GetValue(null));
					CodeCache.Add(addressType, code);
				}
			}

			return code;
		}

		Dictionary<DocAddressType, ZString> CodeCache
		{
			get { return codeCache ?? (codeCache = new Dictionary<DocAddressType, ZString>()); }
		}

		Dictionary<DocAddressType, ZString> codeCache;

		public CodeDescriptionPair GetPairCore(DocAddressType docAddressType)
		{
			return (CodeDescriptionPair)this[GetCodeCore(docAddressType)];
		}

		IEnumerable<ZString> GetCodesCore(BusinessObjectFactory factory)
		{
			if (codes == null)
			{
				var docAddressEnumTypes = Enum.GetValues(typeof(DocAddressType));
				var codesList = new List<ZString>(docAddressEnumTypes.Length);

				foreach (DocAddressType docAddress in docAddressEnumTypes)
				{
					codesList.Add(DocAddressTypes.GetCode(factory, docAddress));
				}

				codes = codesList;
			}
			return codes;
		}

		IEnumerable<ZString> codes;
	}
}
