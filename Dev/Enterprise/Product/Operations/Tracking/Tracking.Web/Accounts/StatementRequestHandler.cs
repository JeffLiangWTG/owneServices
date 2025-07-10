using System.Web;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class StatementRequestHandler : DataRequestHandler<StatementRequestHelper>
	{
		public TrackingTransactionFilterBusinessObject FilterBusinessObject
		{
			get { return BusinessObjects[0] as TrackingTransactionFilterBusinessObject; }
		}

		public override ZBlob GetBinaryData()
		{
			var language = HttpContext.Current.Session["Language"] as string;
			lock (FilterBusinessObject)
			{
#if DEBUG
				StopStopwatchForTesting();
#endif
				var statement = FilterBusinessObject.GetStatementForSelectedCompany();

				if (!string.IsNullOrEmpty(language))
				{
					ObjectFactory.Get<IResourceStrings>().CurrentLanguage = language;
				}
				return statement;
			}
		}

		public override string FileName
		{
			get { return "StatementOfAccount.pdf"; }
		}

		public override string ContentType
		{
			get { return DataContentTypes.Pdf; }
		}

		public override string NoDataErrorMessage
		{
			get { return Res.GetString("a64b002f-f131-4832-89f4-bdbc4eca8c93", "Cannot generate a statement because there are no transactions issued to your organization."); }
		}

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(Factory);
			TrackingTransactionFilterBusinessObject filterBusinessObject = filterFactory.Load<TrackingTransactionFilterBusinessObject>();
			filterBusinessObject.FilterOperator = SQLComparisonOperator.Contains;

			// Ignore all PKs except the first one
			filterBusinessObject.Company = PKs[0];
			filterBusinessObject.LoggedInUser = (OrgContact)AppInstance.SiteUser.LoggedInUser;

			return new BusinessObject[] { filterBusinessObject };
		}

		protected override string GetCacheKey(params ZGuid[] pKeys)
		{
			string cacheKey = base.GetCacheKey(pKeys);

			OrgContactWebUser user = AppInstance.SiteUser as OrgContactWebUser;

			if (user != null && user.LoggedInOrganisation != null)
			{
				return string.Format("{0},{1}", cacheKey, user.LoggedInOrganisation.PK);
			}

			return cacheKey;
		}
	}
}
