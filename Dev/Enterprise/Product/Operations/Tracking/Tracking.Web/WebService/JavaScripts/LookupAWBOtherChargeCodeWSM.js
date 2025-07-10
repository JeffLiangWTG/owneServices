function LookupAWBOtherChargeCode(codeControlID, descriptionControlID, entitlementCodeControlID, sessionIndex, chargePK, prepaidCollectFlagID) {
    if ($(codeControlID)) {
        ExecuteTrackingServiceMethod("LookupAWBOtherChargeCode", JSON.encode({ CodeControlID: codeControlID, CodeValue: $(codeControlID).get('value'), DescriptionControlID: descriptionControlID, EntitlementCodeControlID: entitlementCodeControlID, SessionIndex: sessionIndex, ChargePK: chargePK, PrepaidCollectFlagID: prepaidCollectFlagID }));
    }
}