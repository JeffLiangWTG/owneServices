using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.TransportBooking;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class UniversalJobLinksCreatorForBusinessFactoryTest : TestCaseWithFactory
	{
		public void TestTryCreateJobLink_DispatchConsignment()
		{
			TryCreateJobLinkCore(DataContextType.TransitDispatch);
		}

		public void TestTryCreateJobLink_TransportBooking()
		{
			TryCreateJobLinkCore(DataContextType.TransportBooking);
		}

		void TryCreateJobLinkCore(DataContextType dataContextType)
		{
			var whs = Helper.CreateWarehouse("WHS");
			var dcn = Helper.CreateDispatchConsignment("DC1", whs.PK);
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var transportBooking = Factory.New<IDtbBooking>();
			transportBooking.KM_KB_Booking = consolidation.PK;
			transportBooking.KM_JobID = "TB001";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = FormattableString.Invariant($"{registrationKey.EnterpriseCode}");
			var serverCode = FormattableString.Invariant($"{registrationKey.ServerCode}");
			BusinessObject sourceBO = null;
			BusinessObject targetBO = null;
			Factory.Save();

			if (dataContextType == DataContextType.TransitDispatch)
			{
				sourceBO = transportBooking as BusinessObject;
				targetBO = dcn;
			}
			else
			{
				sourceBO = dcn;
				targetBO = transportBooking as BusinessObject;
			}
			var universalJobLinkCreator = new UniversalJobLinksCreatorForBusinessFactory(Factory, sourceBO, dataContextType);
			universalJobLinkCreator.CreateUniversalJobLink(targetBO);
			Factory.Save();

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, sourceBO.PK));
			AssertEquals(1, links.Length);
			var link = links.FirstOrDefault();
			AssertEquals(dataContextType == DataContextType.TransitDispatch ? "KM" : "WDC", link.UCL_ParentTableCode);
			AssertEquals(dataContextType == DataContextType.TransitDispatch ? transportBooking.PK : dcn.PK, link.UCL_ParentID);
			AssertEquals(dataContextType == DataContextType.TransitDispatch ? nameof(DataContextType.TransitDispatch) : nameof(DataContextType.TransportBooking), link.UCL_SourceType);
			AssertEquals(dataContextType == DataContextType.TransitDispatch ? dcn.WDC_JobID : transportBooking.KM_JobID, link.UCL_SourceKey);
			AssertEquals(link.UCL_EnterpriseCode, enterpriseCode);
			AssertEquals(link.UCL_ServerCode, serverCode);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
