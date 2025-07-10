using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class DeclarationStatementsParser : BaseNexDocCodeParser<RefCusCodeList>
	{
		public DeclarationStatementsParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusCodeListWriterConfiguration(NexDocConstants.CodeTypes.DeclarationStatement);

		protected override string XMLWriterDataSource => "NexDoc Declaration Statements";

		protected override string OutPutFileName => "RefCusCodeListZZ_AU_ECM_DECLARATION.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate Declaration Statement exists in downloaded List.");
			AppendListCodeSetDetails(keyValues, listCodeSet);
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import Declaration Statement due to empty Code or Description.");
			AppendListCodeSetDetails(keyValues, listCodeSet);
		}

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new BaseKeyValues(listItemCodeSet);

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => NexDocHelper.CheckCodeAndDescriptionIsValid(keyValues);

		protected override void AddToRefList(List<RefCusCodeList> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			result.Add(new RefCusCodeList()
			{
				ZZD_Code = keyValues.Code,
				ZZD_Description = keyValues.Description
			});
		}

		protected override void AppendAdditionalCodes(List<RefCusCodeList> result, HashSet<string> codesToImport)
		{
			if (!codesToImport.Contains("CDD03"))
			{
				result.Add(new RefCusCodeList
				{
					ZZD_Code = "CDD03",
					ZZD_Description = @"IS THE EXPORTER IN POSSESSION OF EITHER:
 • A DECLARATION THAT COMPLIES WITH CLAUSE 6 OF SCHEDULE 9 OF THE RELEVANT ORDERS; OR
 • A WRITTEN VERIFICATION BY AN AUTHORISED OFFICER MADE UNDER CLAUSE OF SCHEDULE 9 OF THE RELEVANT ORDER?"
				});
			}
			if (!codesToImport.Contains("CDD04"))
			{
				result.Add(new RefCusCodeList
				{
					ZZD_Code = "CDD04",
					ZZD_Description = @"I DECLARE THAT:
 • THE CONDITIONS OR RESTRICTIONS PRESCRIBED IN REGULATIONS OR ORDERS UNDER THE EXPORT CONTROL LAWS AND APPLICABLE TO THE GOODS BEEN COMPLIED WITH; AND
 • THE INFORMATION SUPPLIED ON THIS FORM IS TRUE AND CORRECT IN EVERY PARTICULAR.
NOTE: FOR CRIMINAL PENALTIES APPLYING TO PERSONS WHO MAKE FALSE MISLEADING STATEMENTS TO A COMMONWEALTH ENTITY SEE THE CRIMINAL CODE ACT 1995 PART 7.4 (FALSE OR MISLEADING STATEMENTS)."
				});
			}
			if (!codesToImport.Contains("CDD05"))
			{
				result.Add(new RefCusCodeList
				{
					ZZD_Code = "CDD05",
					ZZD_Description = @"I DECLARE THAT:
 • I HAVE EFFECTIVE MEASURES IN PLACE TO ENSURE THERE IS A SOUND BASIS FOR THE INFORMATION PROVIDED IN THIS PERMIT APPLICATION.
NOTE: FOR CRIMINAL PENALTIES APPLYING TO PERSONS WHO MAKE FALSE MISLEADING STATEMENTS TO A COMMONWEALTH ENTITY SEE THE CRIMINAL CODE ACT 1995, PART 7.4 (FALSE OR MISLEADING STATEMENTS)."
				});
			}
		}

		void AppendListCodeSetDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			var listItemCodeSet = listCodeSet.Items;
			ErrorBuilder.AppendLine($"Declaration Statement Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Updated Date: {listItemCodeSet.FirstOrDefault(x => x.Key == NexDocConstants.ItemCodeSetKeys.UpdatedDate && x.ValueType == CodeSetValueType.dateTime)?.Value}");
		}
	}
}
