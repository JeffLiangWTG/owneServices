using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;

[assembly: MailSubscriber(typeof(Enterprise.Freight.Agency.ServiceTasks.EIDOMessageRetriever))]
namespace Enterprise.Freight.Agency.ServiceTasks
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal sealed class EIDOMessageRetriever : ShippingManagerMessageRetriever
	{
		protected override string GetApplicationCode(MailItem item)
		{
			return EDIInterchange.ApplicationCodes.EIDO;
		}

		protected override IMailFilter MailFilter { get; } = CreateMailFilter();

		[MailFilter(MailFilterCodes.EIDOMessageRetriever)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.EIDOMessageRetriever, subjectComparison: SQLComparisonOperator.Contains, subjects: new[] { "E-IDO", "EIDO" });
	}
}


