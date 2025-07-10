using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class JobDeclarationEventContextReaderTest : TestCaseWithFactory
	{
		public void TestFillCountrySpecificDetails()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders[0].EntryNumber = "00000006";
			var manager = declaration.GetUniversalDataContextManager() as IEventDataContextManager;
			var pair = manager.EventContextValues.FirstOrDefault(o => o.Key.ToString() == nameof(UniversalEvent.ContextTypes.InternalTransactionNumber));
			AssertEquals("00000006", pair.Value);
		}
	}
}
