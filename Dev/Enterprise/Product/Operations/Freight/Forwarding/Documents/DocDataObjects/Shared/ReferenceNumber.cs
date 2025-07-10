using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ReferenceNumber : DocDataObject, IReferenceNumber
	{
		#region Create

		public static IReadOnlyCollection<ReferenceNumber> Create(IContext context, CusEntryNumAdditionalReferenceCollection numbers)
		{
			if (context == null
				|| numbers == null)
			{
				return System.Array.Empty<ReferenceNumber>();
			}

			var lookup = CreateNumberTypesLookup(numbers);

			return numbers
				?.OfType<CusEntryNumber>()
				.Select(n => Create(context, n, lookup))
				.ToArray()
				?? System.Array.Empty<ReferenceNumber>();
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

		#endregion

		#region Value

		public ZString Value
		{
			get => _value;
			set
			{
				if (SetNonPersistentPropertyValue(ValueInfo, ref _value, value))
				{
					Validate(ValueInfo);
				}
			}
		}

		ZString _value;

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));

		#endregion

		#region Type

		public DocumentVisualizer.DocDataObjects.ICodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		DocumentVisualizer.DocDataObjects.ICodeDescription type;

		#endregion

		#region CountryOfIssue

		public ICountry CountryOfIssue
		{
			get => countryOfIssue;
			set => countryOfIssue = SetChild(countryOfIssue, value);
		}

		ICountry countryOfIssue;

		#endregion
	}
}
