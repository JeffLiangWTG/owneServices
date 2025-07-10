using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public abstract class RefCusCodeTypeGenerator : BaseUniversalReferenceDataFileGenerator<RefCusCodeType>
	{
		protected sealed override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeType = new EntityTypeConfiguration<RefCusCodeType>(true);
			codeType.IncludeColumn(x => x.ZZK_CodeType, true);
			codeType.IncludeColumn(x => x.ZZK_Description, false);
			codeType.IncludeColumn(x => x.ZZK_IsReadonly, false);
			codeType.IncludeColumn(x => x.ZZK_MaxLength, false);
			codeType.IncludeColumnWithConstantValue(x => x.ZZK_ZZZ_NKDataGrouping, true, DataGrouping);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeType);

			return xmlWriterConfiguration;
		}

		protected override List<RefCusCodeType> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			return new RefCusCodeType[]
				{
					new RefCusCodeType
					{
						ZZK_CodeType = CodeType,
						ZZK_Description = Description,
						ZZK_IsReadonly = ReadOnly,
						ZZK_MaxLength = MaxLength
					}
				}.ToList();
		}

		public sealed override string OutputFile => $"{DataSource}.xml";

		public sealed override string DataSource => $"{DataGrouping} {Description} Code Type";

		internal protected abstract string DataGrouping { get; }

		protected abstract string CodeType { get; }

		internal protected abstract string Description { get; }

		protected abstract bool ReadOnly { get; }

		protected abstract byte MaxLength { get; }

		protected override UpdateType updateType => UpdateType.Full;
	}
}
