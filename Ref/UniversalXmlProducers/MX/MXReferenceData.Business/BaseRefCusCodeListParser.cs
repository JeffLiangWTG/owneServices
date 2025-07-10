using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public abstract class BaseRefCusCodeListParser<T> : BaseParser
	{
		protected BaseRefCusCodeListParser(string dataSource) : base(dataSource)
		{
		}

		protected abstract IEnumerable<RefCusCodeList> GetRefCusCodeLists(IEnumerable<T> dataSource);

		protected abstract IEnumerable<RefCusCodeType> GetRefCusCodeType();

		protected abstract string CodeType { get; }

		protected virtual XmlWriterConfiguration GetRefCusCodeListWriterConfiguration()
		{
			return Helper.GetRefCusCodeListWriterConfiguration(CodeType, true);
		}

		public void ExportToXMLFile(IEnumerable<T> dataSource, string outputFileName, string outputTypeFileName = null, DateTime? publicationDateTime = null)
		{
			var publicationTime = publicationDateTime ?? DateTime.Now;

			if (outputTypeFileName != null)
			{
				var refCusCodeType = GetRefCusCodeType();
				var writerType = Helper.GetRefCusCodeTypeWriterConfiguration();
				Helper.ExportToXMLFile(outputTypeFileName, "MX RefCusCodeType", publicationTime, writerType, refCusCodeType, UpdateType.Partial);
			}

			var refCusCodeLists = GetRefCusCodeLists(dataSource);
			var writer = GetRefCusCodeListWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, base.dataSource, publicationTime, writer, refCusCodeLists);
		}
	}
}
