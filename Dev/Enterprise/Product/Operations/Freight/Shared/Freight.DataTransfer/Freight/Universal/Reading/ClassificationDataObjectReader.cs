using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ClassificationDataObjectReader<THarmonisedCode> : DataObjectReader<Classification, THarmonisedCode>
		where THarmonisedCode : BusinessObject, IHarmonisedCode
	{
		public ClassificationDataObjectReader(Classification dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IHarmonisedCodesProvider parent) : base(dataObject, logger, factory)
		{
			this.parent = parent;
		}
		readonly IHarmonisedCodesProvider parent;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(THarmonisedCode targetBO)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO));

			if (builder.IsEmpty)
			{
				if (dataObject.Code.GetValueOrDefault().IsEmpty)
				{
					builder.AppendLine(Res.GetString("fd43bffb-a41d-4ef9-b2d6-3de9f8642bc3", "Data object does not contain Code."));
				}

				if (dataObject.Country == null || dataObject.Country.Code.GetValueOrDefault().IsEmpty)
				{
					builder.AppendLine(Res.GetString("c3cceb8a-3064-42fe-aa8d-012373662441", "Data object does not contain Country/Region Code."));
				}
			}

			return builder.ToString();
		}

		protected override THarmonisedCode GetExistingBusinessObject()
		{
			var countryCode = dataObject.Country?.Code.GetValueOrDefault() ?? ZString.Empty;
			var hsCode = dataObject.Code.GetValueOrDefault();
			return parent.HarmonisedCodes.Cast<THarmonisedCode>().FirstOrDefault(x => !countryCode.IsEmpty && x.Country == countryCode && x.Code == hsCode);
		}

		protected override void PopulateBusinessObject(THarmonisedCode harmonisedCodeBO)
		{
			if (harmonisedCodeBO != null)
			{
				harmonisedCodeBO.Country = (ZString)dataObject.Country.Code;
				harmonisedCodeBO.Code = (ZString)dataObject.Code;
			}
		}
	}
}
