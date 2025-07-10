using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Tracking.Web.Testing
{
	class DummyDocumentSupportableBizOCreator : DocumentsMenuHelper
	{
		public readonly static ZGuid DocumentSupportablePK = new ZGuid("13610335-2756-4895-B7D1-47FF27ECFEDD");

		public DummyDocumentSupportableBizOCreator(ZGuid pk)
		{
			if (pk == DocumentSupportablePK)
			{
				shipment = new BusinessObjectFactory().New<DummyShipmentBusinessObject>();
			}
		}

		readonly DummyShipmentBusinessObject shipment;

		public override IDocumentSupportable GetDocumentSupportable()
		{
			return shipment;
		}

		public override List<DocumentsMenuItem> GetAvailableDocuments()
		{
			return new List<DocumentsMenuItem>();
		}

		public override ZGuid PKForBizOCreation
		{
			get
			{
				return DocumentSupportablePK;
			}
		}
	}
}
