function LookupAWBHandlingCode(codeControlID, descriptionControlID) {
    if ($(codeControlID)) {
        ExecuteTrackingServiceMethod("LookupAWBHandlingCode", JSON.encode({ CodeControlID: codeControlID, CodeValue: $(codeControlID).get('value'), DescriptionControlID: descriptionControlID }));
    }
}
