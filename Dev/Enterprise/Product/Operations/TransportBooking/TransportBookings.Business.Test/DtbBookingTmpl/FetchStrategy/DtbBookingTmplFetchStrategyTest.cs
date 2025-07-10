using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	internal sealed class DtbBookingTmplFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var freshFactory = new BusinessObjectFactory();
			freshFactory.ResetDatabaseLoadCount();

			var templates = freshFactory.Load<DtbBookingTmpl>(new ZQuery(DtbBookingTmplSchema.PK, CreateTransportBookingTemplates()));

			string hit = "";
			foreach (DtbBookingTmpl template in templates)
			{
				hit = template.KT_Code;
				hit = template.Instructions[0].K2_InstructionType;
				hit = template.Instructions[1].K2_InstructionType;
				hit = template.Instructions[2].K2_InstructionType;
			}

			// DtbBookingInstructionTmpl: 1
			// DtbBookingTmpl: 1

			// Hits: 2/1

			AssertMaxDbHits(2, freshFactory);
			AssertEquals("Should hit the db exactly 2 times, why are there less?", 2, freshFactory.DatabaseLoadCount);
		}

		List<ZGuid> CreateTransportBookingTemplates()
		{
			var result = new List<ZGuid>();

			for (int i = 0; i < 10; i++)
			{
				var template = Helper.CreateTransportBookingTemplate("IFC" + i.ToString(), "FCL Import", Constants.CartageDirection.Import);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
				Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

				result.Add(template.PK);
			}

			Factory.Save();

			return result;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
