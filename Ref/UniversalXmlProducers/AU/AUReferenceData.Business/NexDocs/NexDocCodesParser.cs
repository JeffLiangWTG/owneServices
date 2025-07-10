using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class NexDocCodesParser : BaseNexDocCodeParser<RefCusAUNexdocECMCode>
	{
		public NexDocCodesParser(IEnumerable<IListCodeSet> codeSetList)
			: base(codeSetList)
		{
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration => Helper.GetRefCusAUNexdocECMCodeWriterConfiguration();

		protected override string XMLWriterDataSource => "NexDoc Codes";

		protected override string OutPutFileName => "RefCusAUNexdocECMCodeZZ_AU_ECM_CODES.xml";

		protected override void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Duplicate NexDoc Codes exists in downloaded List.");

			ErrorBuilder.AppendLine("NexDoc Code Details:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Position: {listCodeSet.Position}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
		}

		protected override void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet)
		{
			ErrorBuilder.AppendLine("Unable to import NexDoc Codes due to empty Code.");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"NexDoc Code Position: {listCodeSet.Position}");
		}

		protected override bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet) => !string.IsNullOrWhiteSpace(keyValues.Code.Replace(Separator, string.Empty));

		protected override IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet) => new ECMCodeKeyValues(listItemCodeSet);

		protected override void AddToRefList(List<RefCusAUNexdocECMCode> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet)
		{
			result.Add(new RefCusAUNexdocECMCode()
			{
				ZY5_CommodityCode = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CommodityCode),
				ZY5_PreservationCode = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.PreservationCode),
				ZY5_ProductTypeCode = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.ProductTypeCode),
				ZY5_PackTypeCode = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.PackTypeCode),
				ZY5_SupplementaryCode = listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.SupplementaryCode),
			});
		}

		const string Separator = "|";

		sealed class ECMCodeKeyValues : IKeyValues
		{
			public ECMCodeKeyValues(IItemCodeSet[] listItemCodeSet)
			{
				this.listItemCodeSet = listItemCodeSet;
			}

			readonly IItemCodeSet[] listItemCodeSet;

			string IKeyValues.Code
			{
				get
				{
					if (string.IsNullOrWhiteSpace(code))
					{
						var codes = new []
						{
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.CommodityCode) ?? string.Empty,
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.PreservationCode) ?? string.Empty,
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.ProductTypeCode) ?? string.Empty,
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.PackTypeCode) ?? string.Empty,
							listItemCodeSet.GetStringCodeValue(NexDocConstants.ItemCodeSetKeys.SupplementaryCode) ?? string.Empty,
						};

						code = string.Join(Separator, codes);
					}

					return code;
				}
			}
			string code;

			string IKeyValues.SecondaryCode => string.Empty;

			string IKeyValues.Description => string.Empty;

			bool IKeyValues.IsStartDateSuccesfullyParsed => false;

			DateTime IKeyValues.StartDate => DateTime.MinValue;

			string IKeyValues.UniqueCode => ((IKeyValues)this).Code;
		}
	}
}
