using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.MXReferenceData.Services;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public class CustomsFacilitiesParser : BaseRefCusCodeListParser<CustomsSectionDTO>
	{
		public CustomsFacilitiesParser(string dataSource) : base(dataSource)
		{
		}

		protected override string CodeType => Constants.CodeTypes.Codes.CustomsFacilities;

		protected override IEnumerable<RefCusCodeList> GetRefCusCodeLists(IEnumerable<CustomsSectionDTO> dataSource)
		{
			Argument.NotNull(dataSource, nameof(dataSource));

			var result = new List<RefCusCodeList>();

			foreach (var element in dataSource)
			{
				var refCusCodeList = new RefCusCodeList
				{
					ZZD_Code = element.CustomCode + element.SectionCode,
					ZZD_Description = element.SectionDescription
				};

				yield return refCusCodeList;
			}
		}

		protected override IEnumerable<RefCusCodeType> GetRefCusCodeType()
		{
			yield return new RefCusCodeType()
			{
				ZZK_CodeType = CodeType,
				ZZK_Description = Constants.CodeTypes.Codes.CustomsFacilitiesDescription,
				ZZK_MaxLength = 3
			};
		}
	}
}
