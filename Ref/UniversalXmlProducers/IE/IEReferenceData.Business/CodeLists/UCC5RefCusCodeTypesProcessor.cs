using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business
{
	public class UCC5RefCusCodeTypesProcessor
	{
		public UCC5RefCusCodeTypesProcessor()
		{
			GetExistingIE5RefCusCodeTypes = Task.Run(AsyncGetExistingIE5RefCusCodeTypes);
			GetExistingIERefCusCodeTypes = Task.Run(AsyncGetExistingIERefCusCodeTypes);
		}
		Task<List<RefCusCodeType>> GetExistingIE5RefCusCodeTypes;
		Task<List<RefCusCodeType>> GetExistingIERefCusCodeTypes;

		public void Process(
			IEnumerable<(string code, string dataGrouping, IRevenueExtractedCodeList listDetails)> extractedCodeLists,
			IRevenueCodeListDetails[] codeListDetails,
			string outputFilePath
		)
		{
			if (!string.IsNullOrWhiteSpace(outputFilePath))
			{
				var ieRefCusCodeTypeDictionary = GetExistingIERefCusCodeTypes.Result.ToDictionary(codeType => codeType.ZZK_CodeType);
				var codeTypeDictionary = codeListDetails
					.Where(codeListDetail => codeListDetail.ApplicationType == ApplicationType.AISUCC5)
					.ToDictionary(codeListDetail => codeListDetail.Code, codeListDetail => codeListDetail.NameInFile);

				var ucc5CodeLists = extractedCodeLists
					.Where(codeListItem => codeListItem.dataGrouping == Constants.DataGroupings.IEUCC5 && codeListItem.listDetails.CodeList.Any())
					.Select(codeListItem =>
					{
						var ieRefCusCodeType = ieRefCusCodeTypeDictionary.TryGetValue(codeListItem.code, out var ieItem) ? ieItem : null;
						return (
							CodeType: codeListItem.code,
							DataGrouping: codeListItem.dataGrouping,
							Description: ieRefCusCodeType?.ZZK_Description ?? codeTypeDictionary[codeListItem.code],
							MaxLength: ieRefCusCodeType?.ZZK_MaxLength ?? codeListItem.listDetails.CodeList.Max(code => code.Code.Length),
							PublicationDate: codeListItem.listDetails.VersionDate
						);
					});

				var existingIE5Types = GetExistingIE5RefCusCodeTypes.Result.Select(type => type.ZZK_CodeType).ToHashSet();
				var newUCC5CodeTypes = ucc5CodeLists.Where(item => !existingIE5Types.Contains(item.CodeType)).ToArray();

				if (newUCC5CodeTypes.Length > 0)
				{
					var newUCC5CodeTypesRef = newUCC5CodeTypes.Select(type => new RefCusCodeType
					{
						ZZK_CodeType = type.CodeType,
						ZZK_Description = type.Description,
						ZZK_MaxLength = (byte)type.MaxLength
					});

					Helper.ExportToXmlFile(
						dataSource: DataSourceName,
						outputFile: Path.Combine(outputFilePath, $"RefCusCodeTypeZZ_{Constants.DataGroupings.IEUCC5}.xml"),
						xmlWriterConfig: GetWriterConfiguration(),
						publicationDateTime: ucc5CodeLists.First().PublicationDate,
						updateType: UpdateType.Full,
						codeList: newUCC5CodeTypesRef
					);
				}
			}
		}

		public static string GetDependingOn(IRevenueExtractedCodeList listDetails) => listDetails.DataGrouping == Constants.DataGroupings.IEUCC5 ? DataSourceName : null;

		public static string DataSourceName => $"{Constants.DataGroupings.IEUCC5} {nameof(RefCusCodeType)}";

		protected virtual Task<List<RefCusCodeType>> AsyncGetExistingIE5RefCusCodeTypes() => RefDataLoader.GetRefDataAsync<RefCusCodeType>(
			RefDataLoader.RefDataActionNames.RefCusCodeTypeUpdate,
			(nameof(RefCusCodeType.ZZK_ZZZ_NKDataGrouping), RefDataLoader.RefDataComparisonOperator.Equal, Constants.DataGroupings.IEUCC5)
		);

		protected virtual Task<List<RefCusCodeType>> AsyncGetExistingIERefCusCodeTypes() => RefDataLoader.GetRefDataAsync<RefCusCodeType>(
			RefDataLoader.RefDataActionNames.RefCusCodeTypeUpdate,
			(nameof(RefCusCodeType.ZZK_ZZZ_NKDataGrouping), RefDataLoader.RefDataComparisonOperator.Equal, Constants.IECountryCode)
		);

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var refCusCodeType = new EntityTypeConfiguration<RefCusCodeType>(true);
			refCusCodeType.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, Constants.DataGroupings.IEUCC5);
			refCusCodeType.IncludeColumnWithConstantValue(x => x.ZZK_IsReadonly, true, true);
			refCusCodeType.IncludeColumn(x => x.ZZK_CodeType, true);
			refCusCodeType.IncludeColumn(x => x.ZZK_Description, false);
			refCusCodeType.IncludeColumn(x => x.ZZK_MaxLength, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeType);

			return writerConfiguration;
		}
	}
}
