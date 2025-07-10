using System;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPackingLineDataObjectReader : PackingLineWithContainerDataObjectReader<ForwardingPackLine, ForwardingShipment, ForwardingConsol>
	{
		public ForwardingPackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment parentShipment, IContainerLinkManager<ForwardingConsol> linkManager, Func<PackingLine, ForwardingPackLine> packLineBizObjProvider = null)
			: base(packingLineDataObject, logger, factory, parentShipment, linkManager, packLineBizObjProvider)
		{
		}

		public ForwardingPackingLineDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment parentShipment, IContainerLinkManager<ForwardingConsol> containerlinkManager, IOrderLineLinkManager orderLineLinkManager, Func<PackingLine, ForwardingPackLine> packLineBizObjProvider = null)
			: this(packingLineDataObject, logger, factory, parentShipment, containerlinkManager, packLineBizObjProvider)
		{
			this.orderLineLinkManager = orderLineLinkManager;
		}

		readonly IOrderLineLinkManager orderLineLinkManager;

		protected override void PopulateBusinessObject(ForwardingPackLine packingLineBO)
		{
			base.PopulateBusinessObject(packingLineBO);

			packingLineBO.JL_InspectionTypeCode = PackLine.GetCalculatedInspectionTypeCode(packingLineBO, dataObject);
			packingLineBO.JL_IsHighRisk = PackLine.GetPackageIsHighRisk(packingLineBO, dataObject);
			packingLineBO.JL_AdditionalInspectionTypeCode = PackLine.GetCalculatedAdditionalInspectionTypeCode(packingLineBO, dataObject);

			if (dataObject.PackedItemCollection != null && dataObject.PackedItemCollection.Any())
			{
				packingLineBO.Products.DeleteAll();

				foreach (var packedItem in dataObject.PackedItemCollection)
				{
					var productBO = packingLineBO.Products.AddNew();

					if (orderLineLinkManager != null)
					{
						orderLineLinkManager.SetPackProductLink(productBO, packedItem);
					}

					SetValue(productBO, JobPackProductSchema.D2_ProductQuantity, packedItem.PackedQuantity);

					if (packedItem.UnitOfQuantity != null)
					{
						SetValue(productBO, JobPackProductSchema.D2_ProductUnitOfQty, packedItem.UnitOfQuantity.Code);
					}

					if (packedItem.Product != null)
					{
						SetValue(productBO, JobPackProductSchema.D2_ProductCode, packedItem.Product.Code);
					}
				}
			}

			if (packingLineBO.JL_FreightMode == FreightConstants.OuterPackType && dataObject.HasInnerPackingLines())
			{
				packingLineBO.InnerPackLines.DeleteAll();
				foreach (var innerPackingLine in dataObject.PackingLineCollection)
				{
					var reader = new PackingLineDataObjectReader<ForwardingPackLine, ForwardingShipment>(innerPackingLine, logger, factory, parentShipment);
					var innerPackLineBO = reader.ReadIntoBusinessObject();
					innerPackLineBO.JL_JL_OuterPackLine = packingLineBO.PK;
					innerPackLineBO.JL_FreightMode = FreightConstants.InnerPackType;
					parentShipment.InnerPackLines.Add(innerPackLineBO);
				}
			}

			base.PopulateUNDGCollection(packingLineBO);
		}

		protected override bool ShouldPopulateUNDGCollection
		{
			get { return false; }
		}
	}
}

