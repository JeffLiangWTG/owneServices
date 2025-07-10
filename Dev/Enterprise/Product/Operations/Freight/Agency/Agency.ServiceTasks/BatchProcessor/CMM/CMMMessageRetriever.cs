using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;

[assembly: MailSubscriber(typeof(Enterprise.Freight.Agency.ServiceTasks.CMMMessageRetriever))]

namespace Enterprise.Freight.Agency.ServiceTasks
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal sealed class CMMMessageRetriever : ShippingManagerMessageRetriever
	{
		protected override string GetApplicationCode(MailItem item)
		{
			return EDIInterchange.ApplicationCodes.ContainerManagement;
		}

		protected override ZString MailFailureMessage(MailItem item)
		{
			if (AgencyRegistry.Instance.AllowDirectCODECOCOARRICMMMessaging.Value)
			{
				return ZString.Empty;
			}
			else
			{
				return Res.GetString("f0f1cd7b-ff9a-402d-8f8d-3ed357a0b9f6", "Failed to process as registry does not allow processing of CODECO / COARRI messages (from: {0}, date: {1}, subject: {2})", item.MI_From, item.MI_ReceivedDateTime, item.MI_Subject);
			}
		}

		protected override IMailFilter MailFilter { get; } = CreateFilter();

		[MailFilter(MailFilterCodes.CMMMessage)]
		public static IMailFilter CreateFilter()
			=> new QueryMailFilter(MailFilterCodes.CMMMessage, subjectComparison: SQLComparisonOperator.Contains, subjects: new[] { "CODECO", "COARRI" });
	}
}







