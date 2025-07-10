using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Messaging
{
	public partial class EDIMessages : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			var typeName = GetStringFromParameter("ParentType");

			if (!string.IsNullOrEmpty(typeName))
			{
				var sourceType = Type.GetType(typeName);
				if (sourceType != null && typeof(IMessagingSupport).IsAssignableFrom(sourceType))
				{
					var getByPKMethod = sourceType.GetMethod("FromPKFilteredBySiteUser", BindingFlags.Static | BindingFlags.Public);
					if (getByPKMethod != null)
					{
						return getByPKMethod.Invoke(null, new object[] { Factory, GetGuidFromParameter("Ref"), SiteUser }) as BusinessObject;
					}
				}
			}

			return null;
		}

		public IMessagingSupport MessagingParent
		{
			get { return DataSource as IMessagingSupport; }
		}

		#endregion

		#region Overrides

		protected override bool CanAccessAuthorisedContent
		{
			get { return true; }
		}

		protected override void SetupGrids()
		{
			base.SetupGrids();

			EDIMessagesGrid.ColumnProvider = new TrackingEDIMessageColumnProvider();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.EDIMessages;
		}

		#endregion
	}
}