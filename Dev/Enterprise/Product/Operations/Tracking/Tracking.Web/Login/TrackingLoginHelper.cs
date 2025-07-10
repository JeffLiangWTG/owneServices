using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingLoginHelper : OrgContactLoginHelper
	{
		enum QuickViewType
		{
			Shipment,
			Container
		}

		public TrackingLoginHelper(ZPage page)
			: base(page)
		{
		}

		public TrackingLoginHelper(ZPage page, bool isAutoMaticallyHookupOnLoad)
			: base(page, isAutoMaticallyHookupOnLoad)
		{
		}

		protected override string DefaultUrl
		{
			get
			{
				return RemoveUserSpecificQueryParameters(base.DefaultUrl);
			}
		}

		string RemoveUserSpecificQueryParameters(string url)
		{
			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
			{
				var uriInfo = new UriDeconstructor(uri);
				if (!string.IsNullOrEmpty(uriInfo.Query))
				{
					var parameters = HttpUtility.ParseQueryString(uriInfo.Query);
					var userSpecificKeys = GetUserSpecificQueryParameterKeys();
					var validKeys = parameters.AllKeys.Where(k => !string.IsNullOrEmpty(k) && !userSpecificKeys.Any(u => k.Equals(u, StringComparison.OrdinalIgnoreCase)));
					var newQuery = string.Join("&", validKeys.Select(k => $"{WebUtility.UrlEncode(k)}={WebUtility.UrlEncode(parameters[k])}"));

					if (string.IsNullOrEmpty(newQuery))
					{
						return uriInfo.BaseUrl;
					}

					return uriInfo.GetUriPathAndQuery(newQuery);
				}
			}

			return url;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static IEnumerable<string> GetUserSpecificQueryParameterKeys() => new[] { "Mode" };

		protected override bool SetUpLoginDataFromParams()
		{
			if (TrackingLoginMan != null)
			{
				TrackingLoginMan.LoginErrorMsg = ZString.Empty;
				TrackingLoginMan.QuickViewErrorMsg = ZString.Empty;
			}

			var result = base.SetUpLoginDataFromParams();

			if (!GetParamValue("ClearSaved").IsEmpty)
			{
				TrackingLoginMan.ClearSaved = true;
				TrackingLoginMan.RememberMe = false;
			}

			var returnUrl = GetParamValue("ReturnUrl");
			if (returnUrl.IsEmpty)
			{
				var quickViewNumber = GetParamValue("QuickViewNumber");
				var containerQuickViewNumber = GetParamValue("ContainerQuickViewNumber");
				if (WebDataRegistry.Instance.WebTrackerShipmentQuickView.Value && !quickViewNumber.IsEmpty)
				{
					TrackingLoginMan.QuickViewNumber = quickViewNumber;
					result = true;
				}
				else if (WebDataRegistry.Instance.WebTrackerContainerQuickView.Value && !containerQuickViewNumber.IsEmpty)
				{
					TrackingLoginMan.ContainerQuickViewNumber = containerQuickViewNumber;
					result = true;
				}
			}

			return result;
		}

		public void RedirectToDefault()
		{
			RedirectToPage(DefaultUrl);
		}

		protected override void OnLoginSucceed()
		{
			base.OnLoginSucceed();

			EventLogHelper eventLogHelper = new EventLogHelper();
			eventLogHelper.CreateLogForUserLoggedIn(Page.SiteUser as TrackingSiteUser);

			if (WebEnv.AppInstance is ILicenceUsageLogWriter writer)
			{
				writer.WriteLicenceUsageLog(Environment.Env.Licence.WebTracker);
			}
		}

		protected override void OnLoginFailure()
		{
			base.OnLoginFailure();

			if (TrackingLoginMan != null)
			{
				var quickViewFailed = false;
				var shipmentQuickViewFailed = TrackingLoginMan.QuickViewNumber.IsEmpty
					|| TrackingLoginMan.QuickViewNumberInfo.HasErrors()
					|| TrackingLoginMan.QuickViewNumberInfo.HasMessageErrors()
					|| !TryShipmentQuickView();

				if (shipmentQuickViewFailed)
				{
					var containerQuickViewFailed = TrackingLoginMan.ContainerQuickViewNumber.IsEmpty
						|| TrackingLoginMan.ContainerQuickViewNumberInfo.HasErrors()
						|| TrackingLoginMan.ContainerQuickViewNumberInfo.HasMessageErrors()
						|| !TryContainertQuickView();

					if (containerQuickViewFailed)
					{
						quickViewFailed = true;
					}
				}

				Page.NotificationFlags.DisplayErrors = !quickViewFailed;

				if (CurrentUser?.IsLockedOut ?? false)
				{
					TrackingLoginMan.LoginErrorMsg = Res.GetString("965C0772-BCBC-4A0E-98D3-570D0B0C78CC", "Account Locked");

					AddLoginFailedLog(true);
				}
				else if (!TrackingLoginMan.UserName.IsEmpty)
				{
					TrackingLoginMan.LoginErrorMsg = Res.GetString("ac99a8d0-ab0e-4fa3-ab49-153bf9120983", "Login Failed!");

					AddLoginFailedLog(false);
				}
			}
		}

		void AddLoginFailedLog(bool isLocked)
		{
			if (WebDataRegistry.Instance.StoreFailedLogins.Value)
			{
				var branch = GetWebBranch();
				if (branch != null)
				{
					var encoder = new TwoWayEncoder(branch.PK.ToGuid());
					var details = encoder.Encrypt(string.Format(CultureInfo.InvariantCulture, (NoResString)"Company Code: {0}, E-mail: {1}, IsLocked: {2}", TrackingLoginMan.CompanyCode, TrackingLoginMan.UserName, isLocked)); // internal text for WTG support/issues investigation.

					var factory = branch.Factory;
					var log = branch.Logs.AddNew(Events.MiscellaneousEvent);
					var data = factory.New<StmData>();
					data.SD_Owner = log.PK;
					data.SD_Name = log.SL_EventDescription;
					data.SD_BinaryValue = ZBlob.FromUTF8(details);
					factory.Save();
				}
			}
		}

		GlbBranch GetWebBranch()
		{
			var branchPK = Env.Registry.WebBranch;

			return branchPK != Guid.Empty ? Page.Factory.Load<GlbBranch>(branchPK) : null;
		}

		protected override void TryToRedirectToCustomLoginPage()
		{
			base.TryToRedirectToCustomLoginPage();
			ZString customShipmentQuickViewPageUrl = GetCustomShipmentQuickViewPageUrl();
			if (!customShipmentQuickViewPageUrl.IsEmpty)
			{
				RedirectToPage(customShipmentQuickViewPageUrl);
			}
			ZString customLoginPageUrl = GetCustomLoginPageUrl();
			if (!customLoginPageUrl.IsEmpty)
			{
				RedirectToPage(customLoginPageUrl);
			}
		}

		public ZString GetCustomLoginPageUrl()
		{
			ZString result = ZString.Empty;

			if (!string.IsNullOrEmpty(WebDataRegistry.Instance.CustomLoginPageURL.Value))
			{
				if (TrackingLoginMan != null && TrackingLoginMan.CustomLoginPageQueryString.Count > 0)
				{
					result = ZString.Format("{0}?{1}", WebDataRegistry.Instance.CustomLoginPageURL.Value, TrackingLoginMan.CustomLoginPageQueryString);
				}
				else
				{
					result = WebDataRegistry.Instance.CustomLoginPageURL.Value;
				}
			}

			return result;
		}

		public ZString GetCustomShipmentQuickViewPageUrl()
		{
			ZString result = ZString.Empty;

			if (!string.IsNullOrEmpty(WebDataRegistry.Instance.CustomQuickViewPageURL.Value) &&
				TrackingLoginMan != null && TrackingLoginMan.CustomQuickViewPageQueryString.Count > 0)
			{
				result = ZString.Format("{0}?{1}", WebDataRegistry.Instance.CustomQuickViewPageURL.Value, TrackingLoginMan.CustomQuickViewPageQueryString);
			}

			return result;
		}

		#region ShipmentQuickView

		public bool TryShipmentQuickView(bool doQuickViewRedirect = true)
		{
			return TryQuickView(doQuickViewRedirect, QuickViewType.Shipment);
		}

		bool TryQuickView(bool doQuickViewRedirect, QuickViewType quickViewType)
		{
			bool result = false;
			if (TrackingLoginMan == null)
			{
				return result;
			}
			string redirectURL = string.Empty;
			string notFoundMessage = string.Empty;
			string quickViewNumber = string.Empty;
			switch (quickViewType)
			{
				case QuickViewType.Shipment:
					quickViewNumber = TrackingLoginMan.QuickViewNumber.Trim();
					redirectURL = GetShipmentQuickViewURL(quickViewNumber);
					notFoundMessage = Res.GetString("38e6567f-0cef-47ab-a5c8-65237a54b937", "Shipment not Found!");
					break;
				case QuickViewType.Container:
					quickViewNumber = TrackingLoginMan.ContainerQuickViewNumber.Trim();
					redirectURL = GetContainerQuickViewURL(quickViewNumber);
					notFoundMessage = Res.GetString("61FEBE7E-DD5D-4D99-9FFA-E8EC6C0BD03F", "Container not Found!");
					break;
			}

			if (!string.IsNullOrEmpty(redirectURL))
			{
				var siteUser = Page.SiteUser;
				var currentCompany = GlbCompany.CurrentCompany;
				if (siteUser == null || currentCompany == null || currentCompany.OrgProxy == null)
				{
					return result;
				}
				if (doQuickViewRedirect)
				{
					var orgCode = currentCompany.OrgProxy.OH_Code;
					siteUser.Login(orgCode, User.WebUserName, User.WebTransientPassword);
					if (siteUser.IsLoggedIn)
					{
						FormsAuthentication.SetAuthCookie(orgCode, false);
						HttpContext.Current.Response.Redirect(redirectURL, true);
					}
				}

				result = true;
			}
			else if (!string.IsNullOrEmpty(quickViewNumber))
			{
				TrackingLoginMan.QuickViewErrorMsg = notFoundMessage;
			}

			return result;
		}

		ZQuery GetMainShipmentQuery(string shipmentHouseBillNumber)
		{
			var mainShipmentQuery = new ZQuery(
				new ZQuery(JobShipmentSchema.JS_HouseBill, shipmentHouseBillNumber),
				JoinCondition.Or,
				new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentHouseBillNumber))
			{
				OrderBy = JobShipmentSchema.JS_SystemCreateTimeUtc.Name + OrderByClause.Descending
			};

			return mainShipmentQuery;
		}

		#region GetShipmentQueryByMasterBillNumber

		/* The query looks like this:
		 * 
				SELECT *
				FROM dbo.JobShipment
				WHERE JS_PK IN (
					SELECT JN_JS
					FROM dbo.JobConShipLink
					WHERE JN_JK IN (
						SELECT JK_PK
						FROM dbo.JobConsol
						where JK_AgentType = 'DRT'
						AND (
							JK_MasterBillNum = @masterBillNumber -- DIRECT MATCH
							OR (
								JK_TransportMode <> 'AIR'
								AND
								(
									(JK_MasterBillNumNoSCAC = @masterBillNumber) -- User searches without SCAC but SCAC in DB
									or (@masterBillNumberNoSCAC = JK_MasterBillNum) -- User searches with SCAC but SCAC not in DB
								)
							)
						)
					)
				)
		 * */
		static ZQuery GetShipmentQueryByMasterBillNumber(string masterBillNumber)
		{
			var consolQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			consolQuery.AddToFilter(JobConsolSchema.JK_AgentType, Core.Constants.AgentType.Direct);

			var consolQueryDirectMatch = new ZQuery(JobConsolSchema.JK_MasterBillNum, masterBillNumber); // Direct match against user provided value

			var consolQueryNonDirectMatch = new ZQuery(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.NotEqual, Core.Constants.TransportModes.Air);
			var consolQueryNonDirectMatchSub = new ZQuery();

			consolQueryNonDirectMatchSub.AddFilterAndZSQLParameterCollection(
				"JK_MasterBillNumNoSCAC = @masterBillNumber",  // Computed column not in schema

				new ZSqlParameterCollection() { ZSqlParameter.New("@masterBillNumber", masterBillNumber, JobConsolSchema.JK_MasterBillNum) }
			);

			if (masterBillNumber.Length > 4 && Regex.IsMatch(masterBillNumber, "[a-zA-Z]{4}"))
			{
				var masterBillNumberNoSCAC = masterBillNumber.Substring(4);
				consolQueryNonDirectMatchSub.AddFilterAndZSQLParameterCollection(
					string.Format(CultureInfo.InvariantCulture, "@masterBillNumberNoSCAC = {0}", JobConsolSchema.Constants.JK_MasterBillNum),
					new ZSqlParameterCollection() { ZSqlParameter.New("@masterBillNumberNoSCAC", masterBillNumberNoSCAC, JobConsolSchema.JK_MasterBillNum) },
					JoinCondition.Or
				);
			}

			consolQueryNonDirectMatch.AddToFilter(consolQueryNonDirectMatchSub);

			consolQuery.AddToFilter(new ZQuery(consolQueryDirectMatch, JoinCondition.Or, consolQueryNonDirectMatch));

			var jobConShipLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			jobConShipLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolQuery, JoinCondition.And);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddSubQuery(JobShipmentSchema.PK, jobConShipLinkQuery, JoinCondition.And);

			return shipmentQuery;
		}

		#endregion

		static ZQuery GetDeclarationQueryByMasterBillNumber(ZString masterBillNumber, ZString billNumberLessScac)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MasterBill, masterBillNumber);
			if (!billNumberLessScac.IsEmpty)
			{
				var additionalQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, billNumberLessScac);
				declarationQuery.AddToFilter(additionalQuery, JoinCondition.Or);
			}
			return declarationQuery;
		}

		string GetShipmentQuickViewPage(TrackingShipment shipment)
		{
			var appInstance = (Global)Page.AppInstance;

			if (shipment.IsShippingBillOfLading)
			{
				return appInstance.LinerAndAgencyBillOfLadingDetailsPage;
			}

			if (shipment.IsShippingBooking)
			{
				return appInstance.LinerAndAgencyBookingDetailsPage;
			}

			if (shipment.JS_IsCFSRegistered && !shipment.JS_IsForwardRegistered)
			{
				return appInstance.CFSShipmentDetailsPage;
			}

			if (shipment.JS_IsBooking && !shipment.JS_IsForwardRegistered)
			{
				return appInstance.BookingDetailsPage;
			}

			return appInstance.ShipmentDetailsPage;
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public string GetShipmentQuickViewURL(string shipmentHousebillNumber)
		{
			if (string.IsNullOrEmpty(shipmentHousebillNumber))
			{
				return string.Empty;
			}

			var shipment = Page.Factory.LoadTop1<TrackingShipment>(GetMainShipmentQuery(shipmentHousebillNumber));
			if (shipment != null)
			{
				var redirectPage = GetShipmentQuickViewPage(shipment);
				return string.Format((NoResString)"{0}?Ref={1}", redirectPage, shipment.PK); // URL for Redirection
			}

			if (WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.Value)
			{
				var canadianShipment = Page.Factory.LoadTop1<TrackingShipment>(GetCanadianShipmentReferencesQuery(shipmentHousebillNumber));
				if (canadianShipment != null)
				{
					var redirectPage = GetShipmentQuickViewPage(canadianShipment);
					return string.Format((NoResString)"{0}?Ref={1}", redirectPage, canadianShipment.PK); // URL for Redirection
				}
			}

			var queryShipmentByMasterBillNumber = GetShipmentQueryByMasterBillNumber(shipmentHousebillNumber);
			var count = Page.Factory.GetDatabaseCount(typeof(ForwardingShipment), queryShipmentByMasterBillNumber);
			if (count == 1) //We don't want to return a shipment if more that one match for security reason
			{
				var shipmentByMasterBillNumber = Page.Factory.LoadTop1<TrackingShipment>(queryShipmentByMasterBillNumber);
				if (shipmentByMasterBillNumber != null)
				{
					return string.Format((NoResString)"{0}?Ref={1}", ((Global)Page.AppInstance).ShipmentDetailsPage, shipmentByMasterBillNumber.PK); // URL for Redirection
				}
			}

			var queryDeclaration = new ZQuery(new ZQuery(JobDeclarationSchema.JE_HouseBill, shipmentHousebillNumber),
				JoinCondition.Or,
				new ZQuery(JobDeclarationSchema.JE_DeclarationReference, shipmentHousebillNumber));
			var declaration = Page.Factory.LoadTop1<BaseJobDeclaration>(queryDeclaration);
			if (declaration != null)
			{
				return string.Format((NoResString)"{0}?Ref={1}", ((Global)Page.AppInstance).DeclarationDetailsPage, declaration.PK); // URL for Redirection
			}

			if (WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.Value)
			{
				var canadianDeclaration = Page.Factory.LoadTop1<BaseJobDeclaration>(GetCanadianDeclarationReferencesQuery(shipmentHousebillNumber));
				if (canadianDeclaration != null)
				{
					return string.Format((NoResString)"{0}?Ref={1}", ((Global)Page.AppInstance).DeclarationDetailsPage, canadianDeclaration.PK); // URL for Redirection
				}
			}

			var billNumber = new ZString(shipmentHousebillNumber);
			var billNumberLessScac = billNumber.SubstringSafe(4);
			var scacFromBillNumber = billNumber.SubstringSafe(0, 4);
			var queryDeclarationByMasterBillNumber = GetDeclarationQueryByMasterBillNumber(billNumber, billNumberLessScac);
			var candidateDeclarations = Page.Factory.Load<BaseJobDeclaration>(queryDeclarationByMasterBillNumber);
			var filteredDeclarations = candidateDeclarations.Where(dec =>
			{
				var matched = false;
				var usDeclaration = dec as Customs.US.Business.JobDeclaration;
				if (!billNumberLessScac.IsEmpty
					&& usDeclaration != null
					&& (dec.IsSea || dec.IsRail || dec.IsRoad))
				{
					matched = dec.JE_MasterBill == billNumberLessScac;
					matched = matched && string.Equals(usDeclaration.JE_MasterBillIssuerSCAC, scacFromBillNumber, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					matched = dec.JE_MasterBill == shipmentHousebillNumber;
				}
				return matched;
			}).Take(2);

			if (filteredDeclarations.Count() == 1) //We don't want to return a declaration if more than one match for security reason
			{
				var declarationByMasterBillNumber = filteredDeclarations.FirstOrDefault();
				return string.Format((NoResString)"{0}?Ref={1}", ((Global)Page.AppInstance).DeclarationDetailsPage, declarationByMasterBillNumber.PK); // URL for Redirection
			}

			if (WebDataRegistry.Instance.WebTrackerQuickViewByAdditionalReferences.Value)
			{
				var additionalRefShipment = Page.Factory.LoadTop1<TrackingShipment>(GetAdditionalReferenceQuery(shipmentHousebillNumber));
				if (additionalRefShipment != null)
				{
					return string.Format((NoResString)"{0}?Ref={1}", ((Global)Page.AppInstance).ShipmentDetailsPage, additionalRefShipment.PK); // URL for Redirection
				}
			}

			return string.Empty;
		}

		#region Reference Number Query

		ZQuery GetAdditionalReferenceQuery(string reference)
		{
			var result = new ZDBOnlyQuery(typeof(TrackingShipment));
			var moduleFilterBO = new TrackingShipmentFilterBusinessObject();
			var comparisonOperator = SQLComparisonOperator.Equal;

			result.AddSubQuery(moduleFilterBO.GetOrderRefSubQuery(comparisonOperator, reference), JoinCondition.Or);
			result.AddSubQuery(moduleFilterBO.GetOrderRefCartageSubQuery(comparisonOperator, reference), JoinCondition.Or);
			result.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_BookingReference, comparisonOperator, reference);
			result.OrderBy = string.Format("{0}{1}", TrackingShipment.Schema.JS_SystemCreateTimeUtc, OrderByClause.Descending); // QuickView SQL

			return result;
		}

		#endregion

		#region Canadian References Query

		ZQuery GetCanadianShipmentReferencesQuery(string reference)
		{
			var result = new ZDBOnlyQuery(typeof(TrackingShipment));
			var moduleFilterBO = new TrackingShipmentFilterBusinessObject();
			result.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobShipmentSchema.Constants.TableName, SQLComparisonOperator.Equal, reference), JoinCondition.Or);
			return result;
		}

		ZQuery GetCanadianDeclarationReferencesQuery(string reference)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			var moduleFilterBO = new TrackingShipmentFilterBusinessObject();
			var comparisonOperator = SQLComparisonOperator.Equal;

			result.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclarationSchema.Constants.TableName, SQLComparisonOperator.Equal, reference), JoinCondition.Or);
			result.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(SQLComparisonOperator.Equal, reference), JoinCondition.Or);
			result.AddSubQuery(moduleFilterBO.GetTransactionNumberQuery(Core.Constants.CountryCodes.Canada, comparisonOperator, reference), JoinCondition.Or);

			return result;
		}

		#endregion

		#endregion

		#region Shipment Container Related Query

		public ZGuid GetShipmentPk(string shipmentHousebillNumber)
		{
			TrackingShipment shipment = Page.Factory.LoadTop1<TrackingShipment>(GetMainShipmentQuery(shipmentHousebillNumber));
			return shipment != null ? shipment.PK : ZGuid.Empty;
		}

		public bool AreShipmentAndContainerRelated(string containerNumber, ZGuid shipmentPk)
		{
			return Page.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(TrackingContainer)), GetShipmentRelatedContainers(containerNumber, shipmentPk));
		}

		ZQuery GetShipmentRelatedContainers(string containerNumber, ZGuid shipmentPk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(TrackingContainer));
			query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
			var subQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
			subQuery.AddToFilter(JobConShipLinkSchema.JN_JS, shipmentPk);
			query.AddSubQuery(JobContainerSchema.JC_JK, subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region ContainerQuickView

		public bool TryContainertQuickView(bool doQuickViewRedirect = true)
		{
			return TryQuickView(doQuickViewRedirect, QuickViewType.Container);
		}

		string GetContainerQuickViewURL(string containerNumber)
		{
			string result = "";

			if (!string.IsNullOrEmpty(containerNumber))
			{
				string redirectPage = string.Empty;
				var query = new ZDBOnlyQuery(typeof(LinerAndAgencyContainer));
				var subquery = new ZDBOnlySubQuery(typeof(AgencyShipment), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				subquery.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				query.AddSubQuery(subquery, JoinCondition.And);
				query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerNumber);
				query.OrderBy = JobContainerSchema.JC_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var container = Page.Factory.LoadTop1<LinerAndAgencyContainer>(query);
				if (container != null)
				{
					redirectPage = ((Global)Page.AppInstance).LinerAndAgencyContainerDetailsPage;
					result = string.Format((NoResString)"{0}?Ref={1}", redirectPage, container.PK);// URL for Redirection
				}
				else
				{
					var queryContainer = new ZQuery(JobContainerSchema.JC_ContainerNum, containerNumber);
					queryContainer.OrderBy = JobContainerSchema.JC_SystemCreateTimeUtc.Name + OrderByClause.Descending;
					var forwardingContainer = Page.Factory.LoadTop1<TrackingContainer>(queryContainer);
					if (forwardingContainer != null)
					{
						redirectPage = ((Global)Page.AppInstance).ContainerDetailsPage;
						result = string.Format((NoResString)"{0}?Ref={1}", redirectPage, forwardingContainer.PK); // URL for Redirection
					}
				}
			}

			return result;
		}

		#endregion

		#region TermsAndConditions

		//bool ShouldAgreeToTermsAndConditions
		//{
		//	get
		//	{
		//		if (string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value))
		//		{
		//			return false;
		//		}

		//		bool firstSign = CurrentUser != null && CurrentUser.IsLoggedIn &&
		//			CurrentUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty;

		//		bool termsUpdated = false;
		//		if (!firstSign)
		//		{
		//			ZQuery logQuery = new ZQuery(StmDataSchema.SD_Name, "WebTrackerSiteTermsAndConditions");
		//			StmData stmData = Page.Factory.LoadTop1<StmData>(logQuery);

		//			if (stmData.Logs.GetAllLogs().Any(x => ((StmALog)x).SL_SE_NKEvent == "EDT"))
		//			{
		//				StmALog edtLog = (StmALog)stmData.Logs.GetAllLogs().First(x => ((StmALog)x).SL_SE_NKEvent == "EDT");

		//				termsUpdated = CurrentUser != null && CurrentUser.IsLoggedIn &&
		//					!CurrentUser.LoggedInWebContact.OC_WebContractSignedDate.IsEmpty &&
		//					(Env.Time.GetUtcFromLocalTime(CurrentUser.LoggedInWebContact.OC_WebContractSignedDate.ToDateTime()) < edtLog.SL_PostedTimeUtc);
		//			}
		//		}

		//		return firstSign || termsUpdated;
		//	}
		//}

		#endregion

		public override void RedirectViaLoginRouter(OrgContact contact)
		{
			var router = new TrackingLoginRouter(new Uri(DefaultUrl, UriKind.RelativeOrAbsolute), contact);
			var redirectUri = router.GetRoutingUrl();
			SetSwitchCompanyFunc(LoginMan.Password);
			SetRememberMe();
			RedirectToPage(redirectUri.IsAbsoluteUri ? redirectUri.AbsoluteUri : redirectUri.OriginalString);
		}

		TrackingLoginManager TrackingLoginMan
		{
			get { return base.LoginMan as TrackingLoginManager; }
		}

		TrackingSiteUser CurrentUser
		{
			get { return Page.SiteUser as TrackingSiteUser; }
		}
	}
}
