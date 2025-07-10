function PackLineUpdate(source, shipmentRef, lineRef, quantityID, lengthID, widthID, heightID, packUDID, volumeID, volumeUQID) {
	var modifiedControl = $(source);
	if (modifiedControl) {
		ExecuteTrackingServiceMethod("PackLineUpdate", JSON.encode
			({
				ModifiedControlID: modifiedControl.id,
				NewValue: modifiedControl.get('value'),
				ShipmentRef: shipmentRef,
				LineRef: lineRef,
				QuantityControlID: quantityID,
				LengthControlID: lengthID,
				WidthControlID: widthID,
				HeightControlID: heightID,
				PackUDControlID: packUDID,
				VolumeControlID: volumeID,
				VolumeUQControlID: volumeUQID
			}));
	}
}
