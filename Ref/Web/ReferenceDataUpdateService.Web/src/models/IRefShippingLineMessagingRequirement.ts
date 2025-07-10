import { IEntity } from "./IEntity";

interface IRefShippingLineMessagingRequirement extends IEntity {
    RSR_PK: string,
    RSR_RSL_ShippingLine: string,
    RSR_RST_NKType: string,
    RSR_IsBookingRequest: boolean,
    RSR_IsShippingInstruction: boolean,
    RSR_IsShippingOrder: boolean,
    RSR_IsEManifest: boolean,
    RSR_IsVerifiedGrossContainerWeight: boolean
}


export default IRefShippingLineMessagingRequirement;
