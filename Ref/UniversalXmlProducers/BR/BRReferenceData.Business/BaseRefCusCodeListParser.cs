using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public abstract class BaseRefCusCodeListParser<T> : BaseParser
	{
		protected BaseRefCusCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected abstract IEnumerable<RefCusCodeList> GetRefCusCodeLists(T dataSource);

		protected abstract IEnumerable<RefCusCodeType> GetRefCusCodeType();

		protected virtual IEnumerable<RefCusCodeListAttributeName> GetRefCusCodeListAttributeNames() => null;

		protected abstract string CodeType { get; }

		protected virtual bool HasAttributes => false;

		protected virtual XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			return Helper.GetRefCusCodeListWriterConfiguration(CodeType, HasAttributes, true);
		}

		public static Func<DateTime> GetPublicationTime => () => DateTime.Now;

		public void ExportToXMLFile(T inputFileName, string outputFileName, string outputTypeFileName = null, string outputAttributeFileName = null)
		{
			var publicationTime = GetPublicationTime?.Invoke() ?? DateTime.Now;

			if (outputTypeFileName != null)
			{
				var refCusCodeType = GetRefCusCodeType();
				var writerType = Helper.GetRefCusCodeTypeWriterConfiguration();
				Helper.ExportToXMLFile(outputTypeFileName, "BR RefCusCodeType", publicationTime, writerType, refCusCodeType, UpdateType.Partial);
			}

			// TODO: There's a bug on importing RefCusCodeListAttributeName when ZXE_ZZK_NKCodeTypeForValueList is empty
			//       Uncomment the following code after fixing the bug
			//if (HasAttributes && outputAttributeFileName != null)
			//{
			//	var refAttributeNames = GetRefCusCodeListAttributeNames();
			//	if (refAttributeNames?.Any() ?? false)
			//	{
			//		var writerAttributeName = Helper.GetRefCusCodeListAttributeNameWriterConfiguration();
			//		Helper.ExportToXMLFile(outputAttributeFileName, "BR RefCusCodeListAttributeName", publicationTime, writerAttributeName, refAttributeNames, UpdateType.Partial);
			//	}
			//}

			var refCusCodeLists = GetRefCusCodeLists(inputFileName);
			var writer = GetRefCusCodeListWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusCodeLists);
		}

		public void ExportToStream(T inputStream, Stream outputStream, Stream outputTypeStream = null, Stream outputAttributeStream = null, DateTime? publicationDateTime = null)
		{
			var publicationTime = publicationDateTime ?? DateTime.Now;

			if (outputTypeStream != null)
			{
				var refCusCodeType = GetRefCusCodeType();
				var writerType = Helper.GetRefCusCodeTypeWriterConfiguration();
				Helper.ExportToStream(outputTypeStream, "BR RefCusCodeType", publicationTime, writerType, refCusCodeType, UpdateType.Partial);
			}

			if (HasAttributes && outputAttributeStream != null)
			{
				var refAttributeNames = GetRefCusCodeListAttributeNames();
				if (refAttributeNames?.Any() ?? false)
				{
					var writerAttributeName = Helper.GetRefCusCodeListAttributeNameWriterConfiguration();
					Helper.ExportToStream(outputAttributeStream, "BR RefCusCodeListAttributeName", publicationTime, writerAttributeName, refAttributeNames, UpdateType.Partial);
				}
			}

			var refCusCodeLists = GetRefCusCodeLists(inputStream);
			var writerList = GetRefCusCodeListWriterConfiguration();
			Helper.ExportToStream(outputStream, dataSource, publicationTime, writerList, refCusCodeLists);
		}

		protected void addIfValueIsNotNull(RefCusCodeListAttribute attribute, List<RefCusCodeListAttribute> list)
		{
			Argument.NotNull(attribute, nameof(attribute));
			if (!string.IsNullOrEmpty(attribute.ZZE_Value))
			{
				list.Add(attribute);
			}
		}
	}
}
