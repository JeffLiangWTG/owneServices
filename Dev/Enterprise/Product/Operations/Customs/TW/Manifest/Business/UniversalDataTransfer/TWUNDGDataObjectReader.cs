using System;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer
{
	sealed class TWUNDGDataObjectReader : UNDGDataObjectReader
	{
		public TWUNDGDataObjectReader(UNDG dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, Func<MasterFiles.Business.UNDGDataItem> undgDataItemBizObjProvider = null)
			: base(dataObject, logger, factory, undgDataItemBizObjProvider)
		{
		}

		protected override void PopulateBusinessObject(MasterFiles.Business.UNDGDataItem undg)
		{
			base.PopulateBusinessObject(undg);

			if (undg.DI_DG_NKSubs.IsDefault)
			{
				var undgDataRow = GetColumnIndexer(undg);
				SetValue(undgDataRow, UNDGDataItemSchema.DI_DG_NKSubs, dataObject.UNDGCode);
			}
		}
	}
}
