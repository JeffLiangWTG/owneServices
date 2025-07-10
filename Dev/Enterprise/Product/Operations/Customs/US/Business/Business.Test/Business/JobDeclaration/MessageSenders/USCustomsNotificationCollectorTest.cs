using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	class USCustomsNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestDoNotIncludeNotificationsFromExcludedChildren()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			header.BH_ParentID = dec.PK;
			header.BH_ParentTableCode = dec.TablePrefix;
			dec.RegisterEditableChildObject(header);
			var provider = dec as IMessageNotificationsProvider;
			provider.ExcludeChildAndItsDescendentsFromMessageNotifications(header);
			header.AddRowMessageError("There is an inbond message error");
			var invoice = dec.Invoices.AddNew();
			invoice.AddRowMessageError("There is an invoice message error");

			var messageErrors = new USCustomsNotificationCollector(dec, true, false).GetMessageErrors();
			Assert(messageErrors.Any(x => x.Message.Contains("There is an invoice message error")));
			Assert(!messageErrors.Any(x => x.Message.Contains("There is an inbond message error")));
		}
	}
}
