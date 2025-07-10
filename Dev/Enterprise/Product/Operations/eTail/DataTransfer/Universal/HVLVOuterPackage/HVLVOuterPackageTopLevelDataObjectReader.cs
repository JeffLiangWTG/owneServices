using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOuterPackageTopLevelDataObjectReader : ShipmentDataObjectReader<HVLVOuterPackage>
	{
		public HVLVOuterPackageTopLevelDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.HVLVOuterPackage;

		protected override IMatchingBusinessEntityFinder<HVLVOuterPackage> GetCombinedReferenceMatcher() => null;

		protected override HVLVOuterPackage GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override void PopulateBusinessObject(HVLVOuterPackage outerPackageBO)
		{
			if (dataObject.PackingLineCollection != null && dataObject.PackingLineCollection.Any())
			{
				var outerPackageDataObject = dataObject.PackingLineCollection.FirstOrDefault();
				var outerPackageReader = new HVLVOuterPackageReader(outerPackageDataObject, logger, factory, outerPackageBO);
				outerPackageReader.ReadIntoBusinessObject();

				SetValue(outerPackageBO, HVLVOuterPackageSchema.HVO_PL_NKLastMileCarrierServiceLevel, dataObject.CarrierServiceLevel?.Code);
			}
		}
	}
}
