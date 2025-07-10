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
	public class AdditionalBillAddInfoDataObjectReader : AddInfoDataObjectReader
	{
		public AdditionalBillAddInfoDataObjectReader(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger, helper, CusDecHouseBillSchema.CU_AddInfo)
		{
		}

		protected override void ReadCore(IAddInfoManager addInfoManager, IColumnIndexer row, IEnumerable<AddInfo> addInfoCollection, IOrganizationAddressCollectionParent organizationAddresContainer, IDictionary<ZString, AddInfoPropertyNameAndValueParser> infoMappings, Dictionary<string, UniversalDataBuss.DataObjects.Core.ValueSetter> delaySetters)
		{
			base.ReadCore(addInfoManager, row, GetAddInfoCollectionToRead(addInfoCollection), organizationAddresContainer, infoMappings, delaySetters);
		}

		protected virtual IEnumerable<AddInfo> GetAddInfoCollectionToRead(IEnumerable<AddInfo> addInfoCollection)
		{
			return addInfoCollection.Where(x => x.Key.GetValueOrDefault() != Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber);
		}
	}
}
