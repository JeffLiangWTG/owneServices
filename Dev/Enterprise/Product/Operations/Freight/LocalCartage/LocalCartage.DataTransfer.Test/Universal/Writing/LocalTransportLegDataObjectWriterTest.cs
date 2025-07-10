using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class LocalTransportLegDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulate()
		{
			var localTransport = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 1);
			var container = localTransport.Containers.First();
			var leg = localTransport.GetBookedMoves(localTransport.Containers.First())[0].CartageLegs[0];
			var runSheet = Factory.New<CommonWorkSheet>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			transportCo.OH_FullName = "TRANCO";
			runSheet.EY_OH_TransportCo = transportCo.PK;
			leg.JU_EY_RunSheet = runSheet.PK;
			Factory.Save();
			var writer = new LocalTransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, leg)));
			var legData = writer.GetDataObject(leg);
			AssertNotNull("Precondition: legData", legData);
			AssertEquals("LegType", LegType.LocalTransport, legData.LegType);
			AssertEquals("TransportMode", TransportMode.Road, legData.TransportMode);
			AssertEquals("LegOrder", ZByte.Zero, legData.LegOrder);
			AssertEquals("Carrier.CompanyName", "TRANCO", legData.Carrier.CompanyName);
		}

		public void TestGetUserDefinedValues()
		{
			var localTransport = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 1);
			var container = localTransport.Containers.First();
			var leg = localTransport.GetBookedMoves(localTransport.Containers.First())[0].CartageLegs[0];
			leg.SetUserDefinedValue("Are you Happy?", ZBool.True);
			leg.SetUserDefinedValue("What Makes You Happy?", new ZString("Lots Of Ice"));
			leg.SetUserDefinedValue("The Happy Number", new ZInt(42));
			leg.SetUserDefinedValue("The Happy Decimal", new ZDecimal(7.7));
			leg.SetUserDefinedValue("The Date You Are Happy", ZDateTime.BrettsBirthday);
			var writer = new LocalTransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, leg)));
			var legData = writer.GetDataObject(leg);
			AssertNotNull("Precondition: legData", legData);
			var customFields = legData.CustomizedFieldCollection;
			AssertNotNull(customFields);
			CombineAssertions(delegate
			{
				AssertEquals("customFields.Count", 5, customFields.Count);
				customFields.AssertCustomFieldWasExported(DataType.Boolean, "Are you Happy?", "true");
				customFields.AssertCustomFieldWasExported(DataType.String, "What Makes You Happy?", "Lots Of Ice");
				customFields.AssertCustomFieldWasExported(DataType.Integer, "The Happy Number", "42");
				customFields.AssertCustomFieldWasExported(DataType.Decimal, "The Happy Decimal", "7.7");
				customFields.AssertCustomFieldWasExported(DataType.DateTime, "The Date You Are Happy", ZDateTime.BrettsBirthday.ToISO8601String());
			});
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
