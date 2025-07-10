using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class ReferenceNumber
	{
		public static IReadOnlyCollection<ReferenceNumber> Create(IContext context, CusEntryNumAdditionalReferenceCollection numbers)
		{
			if (context == null
				|| numbers == null)
			{
				return System.Array.Empty<ReferenceNumber>();
			}

			var lookup = CreateNumberTypesLookup(numbers);

			return numbers?.OfType<CusEntryNumber>().Select(n => Create(context, n, lookup)).ToArray() ?? System.Array.Empty<ReferenceNumber>();
		}

		static ICodeDescriptionPairList CreateNumberTypesLookup(CusEntryNumAdditionalReferenceCollection numbers)
		{
			var list = new ZArchitecture.Core.CodeDescriptionPairList();

			if (numbers != null)
			{
				foreach (var number in numbers.OfType<CusEntryNumber>())
				{
					list.AddPairIfNotExist(number.CE_EntryType, number.Lookups.AdditionalReferenceNumberTypes.GetDescriptionFromCode(number.CE_EntryType));
				}
			}

			return list;
		}

		public static ReferenceNumber Create(IContext context, CusEntryNumber cusEntryNumber, ICodeDescriptionPairList numberTypes)
		{
			if (context == null
				|| cusEntryNumber == null)
			{
				return new ReferenceNumber();
			}

			return new ReferenceNumber
			{
				Value = cusEntryNumber.CE_EntryNum,
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = cusEntryNumber.CE_RN_NKCountryCode
				},
				Type = new CodeDescription(numberTypes)
				{
					Code = cusEntryNumber.CE_EntryType
				}
			};
		}
	}
}
