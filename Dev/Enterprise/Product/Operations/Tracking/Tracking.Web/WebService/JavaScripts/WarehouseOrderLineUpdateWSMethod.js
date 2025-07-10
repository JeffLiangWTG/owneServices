function WarehouseOrderLineUpdate(source, docketRef, lineRef, productID, descriptionID, packsID, packsUQID, quantityID, quantityUQID, shortfallID, attribute1ID, attribute2ID, attribute3ID, serialNumberID) {
    var modifiedControl = $(source);
    if (modifiedControl) {
        ExecuteTrackingServiceMethod("WarehouseOrderLineUpdate", JSON.encode
            ({
                ModifiedControlID: modifiedControl.id,
                NewValue: modifiedControl.get('value'),
				DocketRef: docketRef,
                LineRef: lineRef,
                ProductControlID: productID,
                DescriptionControlID: descriptionID,
                PacksControlID: packsID,
                PacksUQControlID: packsUQID,
                QuantityControlID: quantityID,
                ProductUQControlID: quantityUQID,
                ShortfallControlID: shortfallID,
                Attribute1ControlID: attribute1ID,
                Attribute2ControlID: attribute2ID,
				Attribute3ControlID: attribute3ID,
				SerialNumberControlID: serialNumberID
            }));
    }
}
