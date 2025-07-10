using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public sealed class CO2eUniversalCodeMapper : IUniversalCodeMapper
	{
		public static IUniversalCodeMapper Create(ITopLevelDataObject dataObject, BusinessObjectFactory factory)
		{
			if (dataObject == null || factory == null)
			{
				return null;
			}

			return new CO2eUniversalCodeMapper();
		}

		CO2eUniversalCodeMapper()
		{
		}

		public string GetMappedOrInput(string input, string codeMappingRelationshipCode, int? currentLineNumber = null) => string.Empty;

		public string GetMappedOrEmpty(string input, string codeMappingRelationshipCode, int? currentLineNumber = null) => string.Empty;

		public IOrgPatternMatchOverride CreateOrUpdateCodeMapping(string codeMappingRelationshipCode, string foreignCode, string localCode, ZGuid localGuid) => null;

		public IOrgHeader SourceOrganisation => null;
	}
}
