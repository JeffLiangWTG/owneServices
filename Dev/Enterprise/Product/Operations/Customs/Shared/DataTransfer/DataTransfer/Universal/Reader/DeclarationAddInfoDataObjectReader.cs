using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class DeclarationAddInfoDataObjectReader : AddInfoDataObjectReader
	{
		public DeclarationAddInfoDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, JobDeclarationSchema.JE_AddInfo)
		{
		}

		protected override void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, UniversalDataBuss.DataObjects.Core.ValueSetter> delaySetters)
		{
			base.ReadCore(addInfoManager, row, RemoveMasterWayBillRelatedData(addInfoCollection), organizationAddresContainer, infoMappings, delaySetters);
		}

		protected virtual IEnumerable<AddInfo> RemoveMasterWayBillRelatedData(IEnumerable<AddInfo> addInfoCollection)
		{
			return addInfoCollection.Where(x => x.Key.GetValueOrDefault() != Constants.AddInfoKeys.Declaration.MasterWayBillNumber);
		}
	}
}
