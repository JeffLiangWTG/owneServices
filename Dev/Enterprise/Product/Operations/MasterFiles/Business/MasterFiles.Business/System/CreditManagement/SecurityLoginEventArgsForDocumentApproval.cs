using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public class SecurityLoginEventArgsForDocumentApproval : SecurityLoginEventArgs, ICustomMessageBox, ISecurityLoginEventArgsForDocumentApproval
	{
		public SecurityLoginEventArgsForDocumentApproval(
			MultilingualString loginPromptMessage,
			MultilingualString messageToShowWhenNotPrinting,
			Func<SecurityCore, SecurityCheckpoint> getSecurityCheckpoint,
			CustomMessageBoxCallback messageBoxCallback,
			BusinessObject parentBusinessObject,
			ZGuid menuItemPK,
			IReadOnlyList<int> authorizationLevel,
			ZString defaultApprovalRequestReason,
			ZBool isDPSFreightMovementRestricted,
			ZBool isAviationSecurityFreightMovementRestricted,
			ZBool isExternalAccountingSystemUsed,
			ZBool isAccountingRestricted
			)
			: base(loginPromptMessage, messageToShowWhenNotPrinting, getSecurityCheckpoint)
		{
			CustomMessageBox = messageBoxCallback;
			ParentBusinessObject = parentBusinessObject;
			MenuItemPK = menuItemPK;
			AuthorizationLevel = authorizationLevel;
			DefaultApprovalRequestReason = defaultApprovalRequestReason;
			IsDPSFreightMovementRestricted = isDPSFreightMovementRestricted;
			IsExternalAccountingSystemUsed = isExternalAccountingSystemUsed;
			IsAviationSecurityFreightMovementRestricted = isAviationSecurityFreightMovementRestricted;
			IsAccountingRestricted = isAccountingRestricted;
		}

		public BusinessObject ParentBusinessObject { get; }
		public CustomMessageBoxCallback CustomMessageBox { get; }
		public ZGuid MenuItemPK { get; }
		public IReadOnlyList<int> AuthorizationLevel { get; }
		public ZString DefaultApprovalRequestReason { get; }
		public ZBool IsCustomsSubmission { get; set; }
		public ZBool IsDPSFreightMovementRestricted { get; }
		public ZBool IsAviationSecurityFreightMovementRestricted { get; }
		public ZBool IsExternalAccountingSystemUsed { get; }
		public ZBool IsAccountingRestricted { get; }
	}
}
