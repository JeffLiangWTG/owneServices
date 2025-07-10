using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPackingLineDataObjectWriter : PackingLineWithContainerDataObjectWriter<ForwardingPackLine, ForwardingConsol>
	{
		public ForwardingPackingLineDataObjectWriter(IContainerLinkManager<ForwardingConsol> containerLinkManager, IOrderLineLinkManager orderLineLinkManager, PackLineLinkManager packLineLinkManager, BindToLists listCache, IDataWritingManager manager, LineRelatedDataWriterHelper lineRelatedDataWriterHelper = null)
			: base(containerLinkManager, listCache, manager)
		{
			this.manager = manager;
			this.listCache = listCache;
			this.orderLineLinkManager = orderLineLinkManager;
			this.lineRelatedDataWriterHelper = lineRelatedDataWriterHelper;
			PackLineLinkManager = packLineLinkManager;
		}

		readonly IDataWritingManager manager;
		readonly BindToLists listCache;
		readonly IOrderLineLinkManager orderLineLinkManager;
		readonly LineRelatedDataWriterHelper lineRelatedDataWriterHelper;
		internal readonly PackLineLinkManager PackLineLinkManager;

		protected override PackingLine PopulateDataObject(ForwardingPackLine packLineBO)
		{
			var packLineDataObject = base.PopulateDataObject(packLineBO);
			if (packLineBO.Products != null)
			{
				packLineDataObject.SetPackedItemCollection(() =>
				{
					var result = packLineDataObject.PackedItemCollection ?? new List<PackedItem>();

					foreach (var productBO in packLineBO.Products)
					{
						var packedItem = new PackedItem();
						packedItem.PackedQuantity = productBO.D2_ProductQuantity;
						packedItem.UnitOfQuantity = ListHelper.GetWithDescription<PackageType>(productBO.D2_ProductUnitOfQty, productBO.Lookups.UnitOfQuantity);
						packedItem.Product = !productBO.D2_ProductCode.IsEmpty
							? new Product { Code = productBO.D2_ProductCode }
							: null;

						result.Add(packedItem);

						orderLineLinkManager.AllocatePackedItemLink(productBO, packedItem);
					}
					return result;
				});
			}

			packLineDataObject.SetPortReferenceCollection(() => ProcessCollection(packLineBO.PortReferences, new PortReferenceDataObjectWriter(writeManager)));
			PopulateReferenceNumbers(packLineBO, packLineDataObject);

			if (PackLineLinkManager != null)
			{
				PackLineLinkManager.AllocateLink(packLineBO, packLineDataObject);
			}
			lineRelatedDataWriterHelper?.SetRelatedEntityCollection(writeManager.WriterStrategy, packLineBO, packLineDataObject);
			PopulateInnerPackLines(packLineBO, packLineDataObject);

			return packLineDataObject;
		}

		void PopulateInnerPackLines(ForwardingPackLine packLineBO, PackingLine packLineDataObject)
		{
			if (packLineBO.JL_FreightMode == FreightConstants.OuterPackType)
			{
				var innerPackLines = packLineBO.Shipment?.InnerPackLines?.OfType<ForwardingPackLine>()?.Where(line => line != null && line.JL_JL_OuterPackLine == packLineBO.PK).ToArray() ?? System.Array.Empty<ForwardingPackLine>();
				if (innerPackLines?.Length > 0)
				{
					packLineDataObject.SetPackingLineCollection(() => ProcessCollection(innerPackLines, new PackingLineDataObjectWriter<ForwardingPackLine>(listCache, manager)));
				}
			}
		}

		internal PackingLine PopulateInnerPackLine(ForwardingPackLine innerPackLineBO, PackingLine packLineDataObject)
		{
			if (packLineDataObject == null || innerPackLineBO == null || innerPackLineBO.JL_FreightMode != FreightConstants.InnerPackType)
			{
				return null;
			}

			var innerPackLineDataObject = ProcessCollection(new[] { innerPackLineBO }, new PackingLineDataObjectWriter<ForwardingPackLine>(listCache, manager))?.SingleOrDefault();
			if (innerPackLineDataObject == null)
			{
				return null;
			}

			if (packLineDataObject.PackingLineCollection == null)
			{
				packLineDataObject.SetPackingLineCollection(() => new List<PackingLine> { innerPackLineDataObject });
			}
			else
			{
				packLineDataObject.PackingLineCollection.Add(innerPackLineDataObject);
			}
			return innerPackLineDataObject;
		}

		void PopulateReferenceNumbers(ForwardingPackLine packLineBO, PackingLine packLineDataObject)
		{
			var referenceNumbers = new List<Reference>();
			foreach (CusEntryNumber cusEntryNum in packLineBO.AdditionalReferenceNumbers)
			{
				var refNumber = new Reference
				{
					Type = new EntryType { Code = cusEntryNum.CE_EntryType },
					ReferenceNumber = cusEntryNum.CE_EntryNum
				};

				referenceNumbers.Add(refNumber);
			}

			if (referenceNumbers.Count > 0)
			{
				packLineDataObject.SetReferenceNumberCollection(() => new List<Reference>());
				packLineDataObject.ReferenceNumberCollection.AddRange(referenceNumbers);
			}
		}
	}
}
