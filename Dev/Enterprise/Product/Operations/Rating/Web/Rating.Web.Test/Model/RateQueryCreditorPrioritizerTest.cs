using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateQueryCreditorPrioritizerTest : RatingTestCase
	{
		public void TestGetCreditors()
		{
			var creditorOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			creditorOnRoute.OH_Code = OrganisationRole.Roles.COR;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = OrganisationRole.Roles.CCR;

			var arrCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			arrCtoOnRoute.OH_Code = OrganisationRole.Roles.ACTR;

			var arrCto = Factory.NewWithValidTestData<OrgHeader>();
			arrCto.OH_Code = OrganisationRole.Roles.ACTO;

			var arrCfs = Factory.NewWithValidTestData<OrgHeader>();
			arrCfs.OH_Code = OrganisationRole.Roles.ACFS;

			var arrCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			arrCfsTransport.OH_Code = OrganisationRole.Roles.ACFT;

			var depCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			depCtoOnRoute.OH_Code = OrganisationRole.Roles.DCTR;

			var depCto = Factory.NewWithValidTestData<OrgHeader>();
			depCto.OH_Code = OrganisationRole.Roles.DCTO;

			var depCfs = Factory.NewWithValidTestData<OrgHeader>();
			depCfs.OH_Code = OrganisationRole.Roles.DCFS;

			var depCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			depCfsTransport.OH_Code = OrganisationRole.Roles.DCFT;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = OrganisationRole.Roles.CAR;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_Code = OrganisationRole.Roles.SAG;

			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_Code = OrganisationRole.Roles.RAG;

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			importBroker.OH_Code = OrganisationRole.Roles.IB;

			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			exportBroker.OH_Code = OrganisationRole.Roles.EB;

			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_Code = OrganisationRole.Roles.DA;

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_Code = OrganisationRole.Roles.PA;

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = OrganisationRole.Roles.CA;

			var deliveryCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCartageCo.OH_Code = OrganisationRole.Roles.DTC;

			var pickupCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			pickupCartageCo.OH_Code = OrganisationRole.Roles.PTC;

			var importCFS = Factory.NewWithValidTestData<OrgHeader>();
			importCFS.OH_Code = OrganisationRole.Roles.ICFS;

			var exportCFS = Factory.NewWithValidTestData<OrgHeader>();
			exportCFS.OH_Code = OrganisationRole.Roles.ECFS;

			var rateParties = new List<OrganisationRole>()
			{
				new OrganisationRole() { Code = creditorOnRoute.OH_Code , Role = creditorOnRoute.OH_Code },
				new OrganisationRole() { Code = creditor.OH_Code , Role = creditor.OH_Code },
				new OrganisationRole() { Code = arrCtoOnRoute.OH_Code , Role = arrCtoOnRoute.OH_Code },
				new OrganisationRole() { Code = arrCto.OH_Code , Role = arrCto.OH_Code },
				new OrganisationRole() { Code = arrCfs.OH_Code , Role = arrCfs.OH_Code },
				new OrganisationRole() { Code = arrCfsTransport.OH_Code , Role = arrCfsTransport.OH_Code },
				new OrganisationRole() { Code = depCtoOnRoute.OH_Code , Role = depCtoOnRoute.OH_Code },
				new OrganisationRole() { Code = depCto.OH_Code , Role = depCto.OH_Code },
				new OrganisationRole() { Code = depCfs.OH_Code , Role = depCfs.OH_Code },
				new OrganisationRole() { Code = depCfsTransport.OH_Code , Role = depCfsTransport.OH_Code },
				new OrganisationRole() { Code = carrier.OH_Code , Role = carrier.OH_Code },
				new OrganisationRole() { Code = sendingAgent.OH_Code , Role = sendingAgent.OH_Code },
				new OrganisationRole() { Code = receivingAgent.OH_Code , Role = receivingAgent.OH_Code },
				new OrganisationRole() { Code = importBroker.OH_Code , Role = importBroker.OH_Code },
				new OrganisationRole() { Code = exportBroker.OH_Code , Role = exportBroker.OH_Code },
				new OrganisationRole() { Code = deliveryAgent.OH_Code , Role = deliveryAgent.OH_Code },
				new OrganisationRole() { Code = pickupAgent.OH_Code , Role = pickupAgent.OH_Code },
				new OrganisationRole() { Code = controllingAgent.OH_Code , Role = controllingAgent.OH_Code },
				new OrganisationRole() { Code = deliveryCartageCo.OH_Code , Role = deliveryCartageCo.OH_Code },
				new OrganisationRole() { Code = pickupCartageCo.OH_Code , Role = pickupCartageCo.OH_Code },
				new OrganisationRole() { Code = importCFS.OH_Code , Role = importCFS.OH_Code },
				new OrganisationRole() { Code = exportCFS.OH_Code , Role = exportCFS.OH_Code },
			};

			#region FRT

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Collect,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
					},
					rateParties);

			AssertCreditors(
					"FRT",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
					},
					rateParties);

			#endregion

			#region "ORG"

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Prepaid,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"ORG",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			#endregion

			#region "DST"

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Prepaid,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"DST",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			#endregion

			#region "UNL"

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Prepaid,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"UNL",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(arrCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCto.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCfs.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(arrCfsTransport.PK ,     new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(importBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryAgent.PK ,       new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(deliveryCartageCo.PK ,   new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(importCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			#endregion

			#region "LOD"

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 6 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Prepaid,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 4 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 5 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			AssertCreditors(
					"LOD",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(depCtoOnRoute.PK ,       new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCto.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCfs.PK ,              new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(depCfsTransport.PK ,     new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK ,        new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupAgent.PK ,         new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(pickupCartageCo.PK ,     new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(exportCFS.PK ,           new int[] { 1, 2, 3, 4, 5, 6 }),
						new KeyValuePair<ZGuid, int[]>(controllingAgent.PK ,    new int[] { 1, 2, 3, 4, 5, 6 }),
					},
					rateParties);

			#endregion

			#region "INS"

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 4 }),
					},
					rateParties);

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Collect,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
					},
					rateParties);

			AssertCreditors(
					"INS",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(creditorOnRoute.PK ,     new int[] { 1 }),
						new KeyValuePair<ZGuid, int[]>(creditor.PK ,            new int[] { 2 }),
						new KeyValuePair<ZGuid, int[]>(sendingAgent.PK ,        new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(receivingAgent.PK ,      new int[] { 3 }),
						new KeyValuePair<ZGuid, int[]>(carrier.PK ,             new int[] { 3 }),
					},
					rateParties);

			#endregion

			#region "CDS"

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(importBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(importBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			#endregion

			#region "BRK"

			AssertCreditors(
					"BRK",
					Core.Constants.PaymentType.Prepaid,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(importBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"BRK",
					Core.Constants.PaymentType.Collect,
					Directions.Import,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(importBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"BRK",
					Core.Constants.PaymentType.Collect,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			AssertCreditors(
					"BRK",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			#endregion

			#region "OBR"

			AssertCreditors(
					"OBR",
					Core.Constants.PaymentType.Prepaid,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"OBR",
					Core.Constants.PaymentType.Collect,
					Directions.Export,
					new List<KeyValuePair<ZGuid, int[]>>()
					{
						new KeyValuePair<ZGuid, int[]>(exportBroker.PK , new int[] { 1 }),
					},
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.Domestic,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			AssertCreditors(
					"CDS",
					Core.Constants.PaymentType.Collect,
					Directions.CrossTrade,
					new List<KeyValuePair<ZGuid, int[]>>(),
					rateParties);

			#endregion
		}

		void AssertCreditors(string chargeGroup, string carrierPayTerm, Directions direction, List<KeyValuePair<ZGuid, int[]>> expectedCreditorsWithPriority, IEnumerable<OrganisationRole> rateParties)
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.RateParties = rateParties.ToArray();
			rateQuery.TransportMode = "SEA";
			rateQuery.TransportMode = "FCL";
			rateQuery.CarrierPayTerm = carrierPayTerm;

			// to support old clients
			if (rateParties.Any(x => x.Role == OrganisationRole.Roles.CAR))
			{
				rateQuery.Carriers = rateParties
					.Where(x => x.Role == OrganisationRole.Roles.CAR)
					.Select(x => new Organisation { CWCode = x.Code })
					.ToArray();
			}

			switch (direction)
			{
				case Directions.Import:
					rateQuery.Origin = new Location { Type = Location.Types.UNLOCO, Value = "USLAX" };
					rateQuery.Destination = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" };
					break;
				case Directions.Export:
					rateQuery.Origin = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" };
					rateQuery.Destination = new Location { Type = Location.Types.UNLOCO, Value = "USLAX" };
					break;
				case Directions.Domestic:
					rateQuery.Origin = new Location { Type = Location.Types.UNLOCO, Value = "AUBNE" };
					rateQuery.Destination = new Location { Type = Location.Types.UNLOCO, Value = "AUSYD" };
					break;
				case Directions.CrossTrade:
					rateQuery.Origin = new Location { Type = Location.Types.UNLOCO, Value = "USLAX" };
					rateQuery.Destination = new Location { Type = Location.Types.UNLOCO, Value = "NLAMS" };
					break;
			}

			var rateQueryBusinessObject = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var creditors = rateQueryBusinessObject.GetCreditors();

			var message = $"'{chargeGroup}' Charge Group, '{carrierPayTerm}' PaymentTerm, '{direction.ToString()}' Direction";

			CombineAssertions(message,
			() =>
			{
				foreach (var item in expectedCreditorsWithPriority)
				{
					OrgHeader org = Factory.Load<OrgHeader>(item.Key);
					var ranks = creditors.GetRank(chargeGroup, OrgWithSource.New(org, new List<string>() { "Provider" }));
					AssertContainsExactElementsInAnyOrder(item.Value, ranks);
				}
			});
		}
	}
}
