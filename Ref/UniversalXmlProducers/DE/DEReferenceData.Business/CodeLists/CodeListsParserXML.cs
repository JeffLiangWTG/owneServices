using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public abstract class CodeListsParserXML<T, TKeyValues> : ManyToOneCodeListsParserXML<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected CodeListsParserXML(string[] downloadLinks) : base(downloadLinks)
		{
		}

		protected override List<T> ConvertAndCombineCodeLists(Dictionary<string, ParseResult<TKeyValues>> parseResults) => parseResults.Single().Value.List.Select(CreateRefList).ToList();

		protected virtual void AppendInvalidDataErrorDetails(TKeyValues keyValues, XElement entry)
		{
			base.AppendInvalidDataErrorDetails(CustomsCodeListIdentifier, keyValues, entry);
		}

		protected sealed override void AppendInvalidDataErrorDetails(string customsCodeListIdentifier, TKeyValues keyValues, XElement entry) => AppendInvalidDataErrorDetails(keyValues, entry);

		protected sealed override string[] CustomsCodeListIdentifiers => new[] { CustomsCodeListIdentifier };

		protected abstract string CustomsCodeListIdentifier { get; }

		protected abstract T CreateRefList(TKeyValues keyValues);
	}
}
