using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CombinedNomenclatureParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public CombinedNomenclatureParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfigurationWithAttributes(NexDocConstants.CodeTypes.CombinedNomenclatureCode, true);

		protected override string XMLWriterDataSource => "NexDoc Combined Nomenclature Code Types";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_CNCode.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendDuplicateDetails(ErrorBuilder, "CN Code", keyValues, listCodeSet);

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.AppendErrorDetails(ErrorBuilder, "CN Code", keyValues, listCodeSet);

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new CombinedNomenclatureKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			var refcusCodeList = Helper.BaseRefCusCodeList(keyValues, listItemCodeSet);
			result.Add(refcusCodeList);

			var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
			AddAttribute(refCusCodeListAttributes, NexDocConstants.AttributeNames.Commodity, keyValues.SecondaryCode, (value) => !string.IsNullOrWhiteSpace(value), true);
			refcusCodeList.RefCusCodeListAttributes = refCusCodeListAttributes.ToArray();
		}

		class CombinedNomenclatureKeyValues : IKeyValues
		{
			public CombinedNomenclatureKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				ListItemCodeSet = listItemCodeSet;
				StartDateDetails = ListItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			}
			readonly IItemCodeSet[] ListItemCodeSet;
			readonly (bool SuccessfullyParsed, DateTime DateTime) StartDateDetails;

			public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code));
			string code;

			public string SecondaryCode => secondaryCode ?? (secondaryCode = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.ComTypeCode));
			string secondaryCode;

			public string Description => description ?? (description = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description).ToUpperInvariant());
			string description;

			public bool IsStartDateSuccesfullyParsed => StartDateDetails.SuccessfullyParsed;

			public DateTime StartDate => StartDateDetails.DateTime;

			public string UniqueCode => Code + SecondaryCode;
		}
	}
}
