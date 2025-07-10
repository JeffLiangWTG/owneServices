using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ClassificationCollectionReader<THarmonisedCode, IHarmonisedCodeProvider> : DataObjectCollectionReader<Classification, THarmonisedCode>
		where THarmonisedCode : BusinessObject, IHarmonisedCode
	{
		public ClassificationCollectionReader(DataObjectList<Classification> dataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, IHarmonisedCodesProvider parent)
			: base(dataObjects)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.parent = Argument.NotNull(parent, "parent");

			this.logger = logger;
			this.parent = parent;
		}
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly IHarmonisedCodesProvider parent;

		protected override THarmonisedCode[] BusinessObjects => parent.HarmonisedCodes.Cast<THarmonisedCode>().ToArray();

		protected override void AddToCollection(THarmonisedCode harmonisedCode)
		{
			var collection = (parent.HarmonisedCodes);
			collection.Add(harmonisedCode);
		}

		protected override void RemoveFromCollection(THarmonisedCode harmonisedCode)
		{
			var collection = (parent.HarmonisedCodes);
			collection.Delete(harmonisedCode);
		}

		protected override THarmonisedCode FindMatchingBusinessObject(Classification dataObject)
		{
			var countryCode = dataObject.Country?.Code.GetValueOrDefault() ?? ZString.Empty;
			var hsCode = dataObject.Code.GetValueOrDefault();
			return parent.HarmonisedCodes.Cast<THarmonisedCode>().FirstOrDefault(x => x.Country == countryCode && x.Code == hsCode);
		}

		protected override THarmonisedCode ReadIntoBusinessObject(Classification dataObject, THarmonisedCode harmonisedCodeBO)
		{
			var reader = new ClassificationDataObjectReader<THarmonisedCode>(dataObject, logger, factory, parent);
			return reader.ReadIntoBusinessObject();
		}

		protected override bool SkipEntity(Classification dataObject)
		{
			return dataObject.Type == null ||
				   dataObject.Type.Code.GetValueOrDefault() != FreightConstants.Classification.Codes.HarmonizedCode;
		}
	}
}
