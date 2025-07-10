using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection : DocumentWrapperCollection<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>
	{
		public NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection(NX101ControllingMessageHeaderDocumentWrapper parent, ZString certificateType, BusinessObjectFactory factory)
			: base(factory)
		{
			CertificateType = certificateType;
			this.parent = parent;
		}

		ZString CertificateType { get; }

		readonly NX101ControllingMessageHeaderDocumentWrapper parent;

		public void AddLinesFrom(IEnumerable governmentAgencyGoodsItems)
		{
			foreach (NX101GoodsShipmentGovernmentAgencyGoodsItem governmentAgencyGoodsItem in governmentAgencyGoodsItems)
			{
				Add(new NX101ControllingMessageHeaderSectionBodyDocumentWrapper(parent, governmentAgencyGoodsItem, CertificateType, Factory));
			}
		}
	}
}
