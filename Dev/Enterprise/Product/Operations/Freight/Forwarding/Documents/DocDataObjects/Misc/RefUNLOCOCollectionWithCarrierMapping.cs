using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class RefUNLOCOCollectionWithCarrierMapping : RefUNLOCOCollection, ICodeMapper
	{
		public RefUNLOCOCollectionWithCarrierMapping(BusinessObjectFactory factory, ZGuid carrierPK, bool showForeignCode = true)
			: base(factory)
		{
			lazyCarrierUnlocoMapping = new Lazy<CarrierUNLOCOMapping>(() => new CarrierUNLOCOMapping(Factory, carrierPK));
			this.showForeignCode = showForeignCode;
		}

		readonly Lazy<CarrierUNLOCOMapping> lazyCarrierUnlocoMapping;
		readonly bool showForeignCode;

		protected override IFindBoxListProvider FindBoxListProvider => new RefUNLOCOFindBoxListProviderWithCarrierMapping(this, lazyCarrierUnlocoMapping.Value, base.FindBoxListProvider);

		public bool ShowForeignCode => showForeignCode;

		string ICodeMapper.GetForeignCode(string localCode) => lazyCarrierUnlocoMapping.Value.GetForeignCode(localCode);
		string ICodeMapper.GetLocalCode(string foreignCode) => lazyCarrierUnlocoMapping.Value.GetLocalCode(foreignCode);
	}
}
