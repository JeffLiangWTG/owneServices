using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ClassificationDataObjectWriter<THarmonisedCode> : DataObjectWriter<THarmonisedCode, Classification>
		where THarmonisedCode : BusinessObject, IHarmonisedCode
	{
		public ClassificationDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override Classification PopulateDataObject(THarmonisedCode harmonisedCode)
		{
			return new Classification
			{
				Country = ListHelper.GetWithName<Country>(harmonisedCode.Country, new RefCountryCollection(harmonisedCode.Factory)),
				Type = new CodeDescriptionPair
				{
					Code = FreightConstants.Classification.Codes.HarmonizedCode,
					Description = FreightConstants.Classification.Description.HarmonizedCode
				},
				Code = harmonisedCode.Code
			};
		}
	}
}
