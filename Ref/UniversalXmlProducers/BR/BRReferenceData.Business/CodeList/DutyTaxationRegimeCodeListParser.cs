using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class DutyTaxationRegimeCodeListParser : BaseRefCusCodeListParser<Stream>
	{
		public DutyTaxationRegimeCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.RefCusCodeTypes.CustomsTaxationRegimeDuty.Code;

		protected override bool HasAttributes => false;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(Stream dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			var xml = XDocument.Load(dataSource);

			Contract.Assume(xml != null);
			var taxationRegimes = xml.Root.Descendants(DutyTaxationRegimeCodeListConstants.TagTaxationRegime);

			foreach (XElement element in taxationRegimes)
			{
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = element.GetElementValueAsString(DutyTaxationRegimeCodeListConstants.TagTaxationRegimeCode, 35),
					ZZD_Description = element.GetElementValueAsString(DutyTaxationRegimeCodeListConstants.TagTaxationRegimeDescription, 2000)
				};

				refCusCodeList.ZZD_StartDate = element.GetElementValueAsDateTime(DutyTaxationRegimeCodeListConstants.TagTaxationRegimeStartDate);

				result.Add(refCusCodeList);
			}
			return result;
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.RefCusCodeTypes.CustomsTaxationRegimeDuty.Description
			};
		}
	}
}
