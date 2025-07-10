using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class CustomsEnclosureCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public CustomsEnclosureCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsEnclosure.Code;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var customsEnclosure = xml.Root?.Descendants("RecintoAduaneiro");

			Contract.Assume(customsEnclosure != null);

			var blockedList = LoadBlockedList(customsEnclosure);

			foreach (XElement element in customsEnclosure)
			{
				string code = element.GetElementValueAsString("codigo", 35);

				if (blockedList.Contains(code))
				{
					continue;
				}

				if (!string.IsNullOrEmpty(code))
				{
					var refCusCodeList = new RefCusCodeList
					{
						ZZD_Code = code,
						ZZD_Description = element.GetElementValueAsString("descricao", 2000),
						ZZD_StartDate = element.GetElementValueAsDateTime("inicioVigencia")
					};

					result.Add(refCusCodeList);
				}
			}
			return result;
		}

		static HashSet<string> LoadBlockedList(IEnumerable<XElement> elements)
		{
			HashSet<string> blockedList = new HashSet<string>();
			HashSet<string> validationList = new HashSet<string>();

			foreach (XElement element in elements)
			{
				string code = element.GetElementValueAsString("codigo", 35);
				if (!validationList.Add(code))
				{
					blockedList.Add(code);
				}
			}
			return blockedList;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.CustomsEnclosure.Description,
				ZZK_MaxLength = 7
			};
		}
	}
}
