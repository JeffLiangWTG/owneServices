using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface INexDocCodeParser
	{
		bool HasData { get; }
		string ConvertCodesToXMLFile(string outPutFilePath, IDateTimeProvider dateTimeProvider);
	}

	public abstract class BaseNexDocCodeParser<T> : INexDocCodeParser
		where T : RefDataRepoModelEntityType
	{
		protected BaseNexDocCodeParser(IEnumerable<IListCodeSet> codeSetList)
		{
			CodeSetList = codeSetList;
		}
		readonly IEnumerable<IListCodeSet> CodeSetList;

		public bool HasData => CodeSetList.Any();

		public string ConvertCodesToXMLFile(string outPutFilePath, IDateTimeProvider dateTimeProvider)
		{
			ErrorBuilder.Clear();

			var writerConfiguration = XmlWriterConfiguration;
			var codesToImport = new HashSet<string>();
			var result = new List<T>();

			IEnumerable<IListCodeSet> codeSetList = GetFilteredCodeSetList();
			foreach (var codeSet in codeSetList)
			{
				var listItemCodeSet = codeSet.Items;
				var keyValues = GetKeyValues(listItemCodeSet);
				var dataIsValid = CheckCodeSetDataIsValid(keyValues, codeSet);
				var uniqueCode = keyValues.UniqueCode;
				var (updatedDateSuccessfullyParsed, updatedDateTime) = listItemCodeSet.GetDateTimeCodeValue(NexDocConstants.ItemCodeSetKeys.UpdatedDate);
				SetPublicationTime(updatedDateSuccessfullyParsed, updatedDateTime);

				if (dataIsValid)
				{
					if (!codesToImport.Contains(uniqueCode))
					{
						AddToRefList(result, keyValues, listItemCodeSet);
						codesToImport.Add(uniqueCode);
					}
					else
					{
						AppendDuplicateErrorDetails(keyValues, codeSet);
					}
				}
				else
				{
					AppendInvalidDataErrorDetails(keyValues, codeSet);
				}
			}
			AppendAdditionalCodes(result, codesToImport);

			if (PublicationTime.Equals(DateTime.MinValue))
			{
				PublicationTime = dateTimeProvider.CurrentLocalDateTime;
			}

			if (codesToImport.Any())
			{
				Helper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outPutFilePath, OutPutFileName), writerConfiguration, PublicationTime, UpdateType.Full, result);
			}
			else
			{
				ErrorBuilder.AppendLine("There is no filtered data for importing.");
			}

			return ErrorBuilder.ToString();
		}

		protected virtual IEnumerable<IListCodeSet> GetFilteredCodeSetList() => CodeSetList;

		protected virtual void AppendAdditionalCodes(List<T> result, HashSet<string> codesToImport)
		{
		}

		protected void AddAttribute(List<RefCusCodeListAttribute> refCusCodeListAttributes, string name, string value, Func<string, bool> isValid, bool includeValue)
		{
			if (isValid.Invoke(value))
			{
				var refCusCodeListAttribute = new RefCusCodeListAttribute() { ZZE_ZXE_NKName = name };
				if (includeValue)
				{
					refCusCodeListAttribute.ZZE_Value = value;
				}
				refCusCodeListAttributes.Add(refCusCodeListAttribute);
			}
		}

		protected StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		protected abstract XmlWriterConfiguration XmlWriterConfiguration { get; }

		protected abstract IKeyValues GetKeyValues(IItemCodeSet[] listItemCodeSet);

		protected abstract bool CheckCodeSetDataIsValid(IKeyValues keyValues, IListCodeSet listCodeSet);

		protected abstract void AddToRefList(List<T> result, IKeyValues keyValues, IItemCodeSet[] listItemCodeSet);

		protected abstract void AppendDuplicateErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet);

		protected abstract void AppendInvalidDataErrorDetails(IKeyValues keyValues, IListCodeSet listCodeSet);

		protected abstract string XMLWriterDataSource { get; }

		protected abstract string OutPutFileName { get; }

		DateTime PublicationTime { get; set; } = DateTime.MinValue;

		void SetPublicationTime(bool successfullyParsed, DateTime dateTime)
		{
			if (successfullyParsed && PublicationTime < dateTime)
			{
				PublicationTime = dateTime;
			}
		}
	}
}
