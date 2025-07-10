using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class RefUNLOCOFindBoxListProviderWithCarrierMapping : IFindBoxListProvider
	{
		public RefUNLOCOFindBoxListProviderWithCarrierMapping(ActiveBusinessObjectCollection<RefUNLOCO> list, CarrierUNLOCOMapping carrierUnlocoMapping, IFindBoxListProvider provider)
		{
			List = list;
			this.carrierUnlocoMapping = carrierUnlocoMapping;
			this.provider = provider;
		}

		readonly IFindBoxListProvider provider;
		readonly CarrierUNLOCOMapping carrierUnlocoMapping;

		public IBusinessObjectCollection List { get; }

		public bool AutoCompleteOnCommit => provider.AutoCompleteOnCommit;

		public (string, bool) NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code, explicitAutoComplete);
			var match = provider.NearestMatch(localCode, explicitAutoComplete, cursor);
			var matchedCode = carrierUnlocoMapping.GetForeignCode(match.Item1);

			return (matchedCode, match.Item2);
		}

		public (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code, explicitAutoComplete);
			var match = provider.NearestMatchCore(localCode, explicitAutoComplete);
			var matchedCode = carrierUnlocoMapping.GetForeignCode(match.Item1);

			return (matchedCode, match.Item2);
		}

		public string DescriptionFromCode(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.DescriptionFromCode(localCode);
		}

		public string DescriptionFromPrimaryKey(ZGuid pk)
		{
			return provider.DescriptionFromPrimaryKey(pk);
		}

		public ZGuid PrimaryKeyFromCode(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.PrimaryKeyFromCode(localCode);
		}

		public BusinessObject GetBusinessObjectFromCode(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.GetBusinessObjectFromCode(localCode);
		}

		public BusinessObject GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.GetBusinessObjectFromCodeWithoutFilter(localCode);
		}

		public IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.GetBusinessObjectsFromCode(localCode);
		}

		public IEnumerable<BusinessObject> GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			var localCode = carrierUnlocoMapping.GetLocalCode(code);
			return provider.GetBusinessObjectsFromCodeWithoutFilter(localCode);
		}

		public string CodeFromPrimaryKey(ZGuid pk)
		{
			var localCode = provider.CodeFromPrimaryKey(pk);
			return carrierUnlocoMapping.GetForeignCode(localCode);
		}

		public ICodeDescription GetCustomCodeDescription(BusinessObject bizo)
		{
			if (bizo is ICodeDescription codeDescritpion)
			{
				var code = carrierUnlocoMapping.GetForeignCode(codeDescritpion.Code);

				return new CodeDescriptionPair(code, codeDescritpion.Description);
			}

			return provider.GetCustomCodeDescription(bizo);
		}
	}
}
