function WarehouseReceiveLineUpdate(source, docketRef, lineRef, productID, descriptionID, packsID, packsUQID, quantityID, quantityUQID, expectedQuantityID, attribute1ID, attribute2ID, attribute3ID, serialNumberID, expiryDateID) {
    var modifiedControl = $(source);
    if (modifiedControl) {
        ExecuteTrackingServiceMethod("WarehouseReceiveLineUpdate", JSON.encode
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
				ExpectedQuantityControlID: expectedQuantityID,
                Attribute1ControlID: attribute1ID,
                Attribute2ControlID: attribute2ID,
				Attribute3ControlID: attribute3ID,
				SerialNumberControlID: serialNumberID,
				ExpiryDateControlID: expiryDateID
            }));
    }
}
