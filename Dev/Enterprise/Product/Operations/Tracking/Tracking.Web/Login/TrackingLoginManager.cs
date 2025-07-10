using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public class TrackingLoginManager : LoginManager
	{
		#region Schema

		public abstract new class Schema : LoginManager.Schema
		{
			public const string QuickViewNumber = "QuickViewNumber";
			public const string ContainerQuickViewNumber = "ContainerQuickViewNumber";
			public const string QuickViewErrorMsg = "QuickViewErrorMsg";
			public const string LoginErrorMsg = "LoginErrorMsg";
		}

		#endregion

		#region ClearSaved

		public bool ClearSaved;

		#endregion

		#region QuickViewNumber

		protected ZString fQuickViewNumber;
		public ZString QuickViewNumber
		{
			get
			{
				return fQuickViewNumber;
			}
			set
			{
				if (fQuickViewNumber != value)
				{
					CheckMaximumLength(QuickViewNumberInfo, value);
					fQuickViewNumber = value;
					QuickViewNumberInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo QuickViewNumberInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.QuickViewNumber);
			}
		}

		#endregion

		#region ContainerQuickViewNumber

		protected ZString containerQuickViewNumber;
		[MaxLength(AutoJobContainer.Schema.JC_ContainerNumMaxLength)]
		public ZString ContainerQuickViewNumber
		{
			get
			{
				return containerQuickViewNumber;
			}
			set
			{
				if (containerQuickViewNumber != value)
				{
					containerQuickViewNumber = value.SubstringSafe(0, ContainerQuickViewNumberInfo.MaxLength);
					ContainerQuickViewNumberInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo ContainerQuickViewNumberInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ContainerQuickViewNumber);
			}
		}

		#endregion

		#region LoginErrorMsg

		[MaxLength(128)]
		public ZString LoginErrorMsg
		{
			get
			{
				return fLoginErrorMsg;
			}
			set
			{
				if (fLoginErrorMsg != value)
				{
					CheckMaximumLength(LoginErrorMsgInfo, value);
					fLoginErrorMsg = value;
					LoginErrorMsgInfo.RefreshBinding();
				}
			}
		}
		ZString fLoginErrorMsg;

		public virtual ZPropertyInfo LoginErrorMsgInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.LoginErrorMsg); }
		}

		#endregion

		#region QuickViewErrorMsg

		[MaxLength(255)]
		public ZString QuickViewErrorMsg
		{
			get { return fQuickViewErrorMsg; }
			set
			{
				if (fQuickViewErrorMsg != value)
				{
					CheckMaximumLength(QuickViewErrorMsgInfo, value);
					fQuickViewErrorMsg = value;
					QuickViewErrorMsgInfo.RefreshBinding();
				}
			}
		}
		ZString fQuickViewErrorMsg;

		public virtual ZPropertyInfo QuickViewErrorMsgInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.QuickViewErrorMsg); }
		}

		#endregion

		internal QueryString CustomLoginPageQueryString
		{
			get
			{
				var result = new QueryString();
				result.UrlEncodeNameAndValue = false;
				if (!CompanyCode.IsEmpty)
				{
					result.Add("CompanyCode", Uri.EscapeDataString(CompanyCode));
				}
				if (!UserName.IsEmpty)
				{
					result.Add("UserEmail", Uri.EscapeDataString(UserName));
				}
				if (!LoginErrorMsg.IsEmpty)
				{
					result.Add("LoginErrorMsg", Uri.EscapeDataString(LoginErrorMsg));
				}
				if (ClearSaved)
				{
					result.Add("ClearSaved", "Y");
				}
				if (RememberMe)
				{
					result.Add("RememberMe", (NoResString)"on"); // non-semantic text
				}
				result.Add(CustomQuickViewPageQueryString);

				return result;
			}
		}

		internal QueryString CustomQuickViewPageQueryString
		{
			get
			{
				var result = new QueryString();
				result.UrlEncodeNameAndValue = false;
				if (!QuickViewErrorMsg.IsEmpty)
				{
					result.Add("QuickViewErrorMsg", Uri.EscapeDataString(QuickViewErrorMsg));
				}
				if (!QuickViewNumber.IsEmpty)
				{
					result.Add("QuickViewNumber", Uri.EscapeDataString(QuickViewNumber));
				}

				return result;
			}
		}
	}
}
