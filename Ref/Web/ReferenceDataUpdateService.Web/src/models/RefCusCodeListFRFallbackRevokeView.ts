export interface IRefCusCodeListFRFallbackRevokeView {
	ZZD_PK: string,
	ZZD_StartDate: string,
	ZZD_EndDate: string,
	ZZD_InvokeComments: string,
	ZZD_RevokeComments: string,
	ZZD_Application: string,
	ZZD_User: string,
	ZZD_Edit: boolean,
}

export class RefCusCodeListFRFallbackRevokeView implements IRefCusCodeListFRFallbackRevokeView {

	constructor(ZZD_PK: string, ZZD_StartDate: string, ZZD_EndDate: string
		, ZZD_InvokeComments: string, ZZD_RevokeComments: string
		, ZZD_Application: string, ZZD_User: string, ZZD_Code: string
		, ZZE_PK_RevokeComment: string
		, Entity: any
	) {
		this.ZZD_PK = ZZD_PK;
		this.ZZD_StartDate = ZZD_StartDate;
		this.ZZD_EndDate = ZZD_EndDate;
		this.ZZD_InvokeComments = ZZD_InvokeComments;
		this.ZZD_RevokeComments = ZZD_RevokeComments;
		this.ZZD_Application = ZZD_Application;
		this.ZZD_User = ZZD_User;
		this.ZZD_Code = ZZD_Code;
		this.ZZD_Edit = false;
		this.ZZE_PK_RevokeComment = ZZE_PK_RevokeComment;
		this.Entity = Entity;
	}

	ZZD_PK: string;
	ZZD_StartDate: string;
	ZZD_EndDate: string;
	ZZD_InvokeComments: string;
	ZZD_RevokeComments: string;
	ZZD_Application: string;
	ZZD_User: string;
	ZZD_Code: string;
	ZZD_Edit: boolean;
	ZZE_PK_RevokeComment: string;
	Entity: any
}
