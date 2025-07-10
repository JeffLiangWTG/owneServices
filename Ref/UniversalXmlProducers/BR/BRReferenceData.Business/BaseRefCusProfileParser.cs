using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.BRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public abstract class BaseRefCusProfileParser<T> : BaseParser
	{
		protected BaseRefCusProfileParser(string dataSource) : base(dataSource)
		{
		}

		protected abstract string ProfileType { get; }

		protected abstract string TariffType { get; }

		protected virtual bool AddTariffAttributes => false;

		protected abstract IEnumerable<RefCusProfile> GetRefCusProfile(T data);

		protected abstract IEnumerable<RefCusProfileQuestion> GetRefCusProfileQuestion(T data);

		protected virtual IEnumerable<RefCusTariff> GetRefCusTariff(T data) => null;

		public virtual IEnumerable<RefCusProfileQuestionPathway> GetRefCusProfileQuestionPathway(T data) => null;

		protected virtual IEnumerable<RefCusTariffType> GetRefCusTariffType()
		{
			var list = new List<RefCusTariffType>
			{
				new RefCusTariffType() { ZZI_TariffType = TariffType, ZZI_Description = TariffTypes.GetDescritionFromCode(TariffType)}
			};
			return list;
		}

		protected virtual IEnumerable<RefCusProfileType> GetRefCusProfileType()
		{
			var list = new List<RefCusProfileType>
			{
				new RefCusProfileType() { XXX_ProfileType = ProfileType, XXX_Description = ProfileTypes.GetDescritionFromCode(ProfileType) }
			};
			return list;
		}

		XmlWriterConfiguration GetRefCusProfileTypeWriterConfiguration() => Helper.GetRefCusProfileTypeWriterConfiguration(TariffType);

		protected virtual XmlWriterConfiguration GetRefCusProfileWriterConfiguration() => Helper.GetRefCusProfileWriterConfiguration(ProfileType, TariffType);

		protected virtual XmlWriterConfiguration GetRefCusProfileQuestionWriterConfiguration() => Helper.GetRefCusProfileQuestionWriterConfiguration(ProfileType, TariffType);

		XmlWriterConfiguration GetRefCusProfileQuestionPathwayWriterConfiguration() => Helper.GetRefCusProfileQuestionPathwayWriterConfiguration(ProfileType, TariffType);

		static XmlWriterConfiguration GetRefCusTariffTypeWriterConfiguration() => Helper.GetRefCusTariffTypeWriterConfiguration();

		XmlWriterConfiguration GetRefCusTariffWriterConfiguration() => Helper.GetRefCusTariffWriterConfiguration(TariffType, addAttributes: AddTariffAttributes, addDefaultEndDate: true, hasCompositeKey: false);

		protected virtual Func<DateTime> GetPublicationTime() => () => PublicationTimeCore;

		protected virtual Func<DateTime> GetProfilePublicationTime() => () => PublicationTimeCore;

		protected virtual Func<DateTime> GetQuestionPublicationTime() => () => PublicationTimeCore;

		protected virtual DateTime PublicationTimeCore { get; set; } = DateTime.Now;

		public void ExportToXMLFile(T data, string outputFileName, string outputTypeFileName = null, string outputQuestionFileName = null, string outputTariffFileName = null, string outputTariffTypeFileName = null, string outputQuestionPathwayFileName = null)
		{
			if (outputTariffTypeFileName != null)
			{
				var writerType = GetRefCusTariffTypeWriterConfiguration();
				Helper.ExportToXMLFile(outputTariffTypeFileName, $"BR RefCusTariffType {TariffType}", GetPublicationTime().Invoke(), writerType, GetRefCusTariffType(), UpdateType.Partial);
			}

			if (outputTariffFileName != null)
			{
				var writerType = GetRefCusTariffWriterConfiguration();
				Helper.ExportToXMLFile(outputTariffFileName, $"BR RefCusTariff {TariffType}", GetPublicationTime().Invoke(), writerType, GetRefCusTariff(data), UpdateType.Full);
			}

			if (outputTypeFileName != null)
			{
				var writerType = GetRefCusProfileTypeWriterConfiguration();
				Helper.ExportToXMLFile(outputTypeFileName, $"BR RefCusProfileType {ProfileType}", GetPublicationTime().Invoke(), writerType, GetRefCusProfileType(), UpdateType.Full);
			}

			if (outputQuestionFileName != null)
			{
				var writerType = GetRefCusProfileQuestionWriterConfiguration();
				Helper.ExportToXMLFile(outputQuestionFileName, $"BR RefCusProfileQuestion {ProfileType}", GetQuestionPublicationTime().Invoke(), writerType, GetRefCusProfileQuestion(data), UpdateType.Full);
			}

			var refCusProfileList = GetRefCusProfile(data);
			var writer = GetRefCusProfileWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, $"BR RefCusProfile {ProfileType}", GetProfilePublicationTime().Invoke(), writer, refCusProfileList);

			if (outputQuestionPathwayFileName != null)
			{
				Thread.Sleep(5000);
				var writerType = GetRefCusProfileQuestionPathwayWriterConfiguration();
				Helper.ExportToXMLFile(outputQuestionPathwayFileName, $"BR RefCusProfileQuestionPathway {ProfileType}", GetQuestionPublicationTime().Invoke().AddSeconds(5), writerType, GetRefCusProfileQuestionPathway(data), UpdateType.Full);
			}
		}

		protected static string GetDataType(string externalDataType)
		{
			switch (externalDataType)
			{
				case VariableStyleConstants.Date:
				case VariableStyleConstants.DateTime:
					return StyleConstants.Date;
				case VariableStyleConstants.List:
				case VariableStyleConstants.StaticList:
					return StyleConstants.List;
				case VariableStyleConstants.Boolean:
					return StyleConstants.Boolean;
				case VariableStyleConstants.Text:
					return StyleConstants.Text;
				case VariableStyleConstants.NumberInteger:
				case VariableStyleConstants.NumberFloat:
					return StyleConstants.Number;
				case VariableStyleConstants.Compound:
					return StyleConstants.Compound;
				default:
					ParserErrorCollector.Instance.AppendLine($"Style not found: {externalDataType}");
					return string.Empty;
			}
		}
	}
}
