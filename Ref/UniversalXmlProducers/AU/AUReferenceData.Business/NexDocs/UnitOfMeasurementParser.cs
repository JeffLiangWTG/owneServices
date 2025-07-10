using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class UnitOfMeasurementParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public UnitOfMeasurementParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.UnitOfMeasure);

		protected override string XMLWriterDataSource => "NexDoc Unit of Measurement";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_UNIT_OF_MEASUREMENT.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Unit Of Measurement exists in downloaded List.");
			AppendUnitOfMeasurementErrorDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Unit Of Measurement due to empty Symbol Code, Description or an invalid Start Date.");
			AppendUnitOfMeasurementErrorDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new UnitOfMeasurementKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeDescriptionAndStartDateIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet) => result.Add(Helper.BaseRefCusCodeList(keyValues, listItemCodeSet));

		void AppendUnitOfMeasurementErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine("Unit Of Measurement Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Code)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Symbol Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"System Code: {listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.SystemCode)}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.StartDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.EndDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}

		class UnitOfMeasurementKeyValues : IKeyValues
		{
			public UnitOfMeasurementKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				ListItemCodeSet = listItemCodeSet;
				StartDateDetails = ListItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.StartDate);
			}
			readonly IItemCodeSet[] ListItemCodeSet;
			readonly (bool SuccessfullyParsed, DateTime DateTime) StartDateDetails;

			public string Code => code ?? (code = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.SymbolCode));
			string code;

			public string SecondaryCode => null;

			public string Description => description ?? (description = ListItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.Description).ToUpperInvariant());
			string description;

			public bool IsStartDateSuccesfullyParsed => StartDateDetails.SuccessfullyParsed;

			public DateTime StartDate => StartDateDetails.DateTime;

			public string UniqueCode => Code;
		}
	}
}
