using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingTmpl))]
	sealed class DtbBookingTmplBizOTest : DtbTransportBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestInstructions()
		{
			var template = (DtbBookingTmpl)GetNewBusinessObject();
			AssertEquals("Instructions should be registered editable on Transport Booking Template", true, template.IsRegisteredEditableChildObject(template.Instructions));
			AssertEquals(typeof(DtbBookingInstructionTmplCollection), template.Instructions.GetType());
		}

		protected override Type ExpectedLookupsType => typeof(DtbBookingTmplLookups);

		protected override Type ExpectedValidationType => typeof(DtbBookingTmplValidation);
	}

	public class DtbBookingTmplTest : DtbBookingTestCaseWithFactory
	{
		public void TestKT_RatingFreightMode()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import, RatingFreightModes.Codes.Both);
			var picInstruction = template.Instructions.AddNew();
			var dlvInstruction = template.Instructions.AddNew();

			picInstruction.K2_IsContainerRateable = true;
			dlvInstruction.K2_IsContainerRateable = true;
			picInstruction.K2_IsLooseRateable = true;
			dlvInstruction.K2_IsLooseRateable = true;

			template.KT_RatingFreightMode = RatingFreightModes.Codes.Containerised;
			AssertEquals(true, picInstruction.K2_IsContainerRateable);
			AssertEquals(true, dlvInstruction.K2_IsContainerRateable);
			AssertEquals(false, picInstruction.K2_IsLooseRateable);
			AssertEquals(false, dlvInstruction.K2_IsLooseRateable);

			template.KT_RatingFreightMode = RatingFreightModes.Codes.Loose;
			AssertEquals(false, picInstruction.K2_IsContainerRateable);
			AssertEquals(false, dlvInstruction.K2_IsContainerRateable);
			AssertEquals(false, picInstruction.K2_IsLooseRateable);
			AssertEquals(false, dlvInstruction.K2_IsLooseRateable);

			picInstruction.K2_IsContainerRateable = true;
			dlvInstruction.K2_IsContainerRateable = true;
			picInstruction.K2_IsLooseRateable = true;
			dlvInstruction.K2_IsLooseRateable = true;

			template.KT_RatingFreightMode = RatingFreightModes.Codes.Both;
			AssertEquals(true, picInstruction.K2_IsContainerRateable);
			AssertEquals(true, dlvInstruction.K2_IsContainerRateable);
			AssertEquals(true, picInstruction.K2_IsLooseRateable);
			AssertEquals(true, dlvInstruction.K2_IsLooseRateable);
		}

		public void TestReadOnly()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			AssertEquals(true, template.KT_IsSystemInfo.ReadOnly);
			AssertEquals(false, template.ReadOnly);
			AssertEquals(false, template.KT_CodeInfo.ReadOnly);
			AssertEquals(false, template.KT_DescriptionInfo.ReadOnly);
			AssertEquals(false, template.KT_DirectionInfo.ReadOnly);
			AssertEquals(false, template.KT_RatingFreightModeInfo.ReadOnly);
			AssertEquals(false, template.KT_IsActiveInfo.ReadOnly);

			template.KT_IsSystem = true;
			AssertEquals(true, template.KT_IsSystemInfo.ReadOnly);
			AssertEquals(false, template.ReadOnly);
			AssertEquals(true, template.KT_CodeInfo.ReadOnly);
			AssertEquals(true, template.KT_DescriptionInfo.ReadOnly);
			AssertEquals(true, template.KT_DirectionInfo.ReadOnly);
			AssertEquals(true, template.KT_RatingFreightModeInfo.ReadOnly);
			AssertEquals(false, template.KT_IsActiveInfo.ReadOnly);
		}

		public void TestCanCancel_TemplateUsedInRegistry()
		{
			var template = Factory.LoadTop1<DtbBookingTmpl>(new ZQuery(DtbBookingTmplSchema.KT_Code, SQLComparisonOperator.Equal, TransportRegistry.Instance.JobTemplateDefault.Value[0].BookingTemplate));

			var expectedMessage = "This Template is set as a default template in Registry setting 'Job Template Defaults' and therefore cannot be made Inactive.";
			AssertEquals("CanCancel should provide a string indicating why it cannot be cancelled", expectedMessage, template.CanCancel());
		}

		public void TestCanCancel_TemplateNotUsedInRegistry()
		{
			var template = Factory.NewWithValidTestData<DtbBookingTmpl>();
			template.KT_Code = "ABCD";

			var expectedMessage = (string)null;
			AssertEquals("CanCancel should indicate this template can be cancelled by providing a null string", expectedMessage, template.CanCancel());
		}

		public void TestFetchStrategy()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			AssertNotNull(template.FetchStrategy);
			AssertEquals(typeof(DtbBookingTmplFetchStrategy), template.FetchStrategy.GetType());
		}

		public void TestKT_Description_Translatable()
		{
			var bizO = Factory.New<DtbBookingTmpl>();
			bizO.KT_Description = "Boom";
			string resKey = bizO.KT_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", bizO.KT_DescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "咚"));
				AssertEquals("咚", bizO.KT_DescriptionMultilingual);
			}
		}

		public void TestHumanReadableName()
		{
			var bizO = Factory.New<DtbBookingTmpl>();
			AssertEquals("Transport Booking Template", bizO.HumanReadableName);

			bizO.KT_Code = "Boo";
			AssertEquals("Transport Booking Template Boo", bizO.HumanReadableName);
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingTmplSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Should supply correct type based on the table prefix", typeof(DtbBookingTmpl), businessObjectType);
			AssertNotEquals("Should not supply deprecated abstract type", typeof(DtbTransportTmpl), businessObjectType);
		}

		// No interface called IDtbTransportTmpl, therefore no need for TestGetTypeFromObjectFactory or TestCreateFromInterface
	}
}
