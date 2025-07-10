using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	partial class DataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCustomLabelsCustomizedFieldDataObjectReader()
		{
			var reader = new CustomLabelsCustomizedFieldDataObjectReader(logger);

			var org = Factory.New<OrgHeader>();
			var orgMiscServRow = (IColumnIndexer)((IBusinessObjectInternals)org.MiscServ).Row;
			org.OH_Code = "ZDY!DS#@";
			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomAttribute1, "STRING1");
			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomAttribute2, "STRING2");
			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomDate1, "DaTE1"); // Should be a case insensitive match
			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomDecimal1, "DeCIMAL1");
			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomFlag1, "FLAG1");

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2, "", org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.Empty, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2, "", org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.Empty, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(CustomizedField.New("BOB", ZBool.True));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2, "", org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.Empty, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(CustomizedField.New("STRiNG1", new ZString("Attrib1")));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1 + " - case insensitive match", "Attrib1",
				org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2, "", org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.Empty, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(
				CustomizedField.New("STRiNG2", new ZString("Attrib2-123456789012over20now")));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1 + " - case insensitive match", "Attrib1",
				org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2 + " - max length is 20.", "Attrib2-123456789012",
				org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.Empty, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(CustomizedField.New("DATE1", ZDateTime.BrettsBirthday));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "Attrib1", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2 + " - max length is 20.", "Attrib2-123456789012",
				org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1 + " - case insensitive match", ZDateTime.BrettsBirthday,
				org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, ZDecimal.Zero, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(CustomizedField.New("DECIMAL1", (ZDecimal)120.54m));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "Attrib1", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2 + " - max length is 20.", "Attrib2-123456789012",
				org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.BrettsBirthday, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1 + " - case insensitive match", 120.54m,
				org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.False, org.MiscServ.OM_CustomFlag1);

			container.CustomizedFieldCollection.Add(CustomizedField.New("FLAG1", ZBool.True));
			reader.PopulateCustomFields(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib1, "Attrib1", org.MiscServ.OM_CustomAttrib1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomAttrib2 + " - max length is 20.", "Attrib2-123456789012",
				org.MiscServ.OM_CustomAttrib2);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDate1, ZDateTime.BrettsBirthday, org.MiscServ.OM_CustomDate1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomDecimal1, 120.54m, org.MiscServ.OM_CustomDecimal1);
			AssertEquals(OrgMiscServSchema.Constants.OM_CustomFlag1, ZBool.True, org.MiscServ.OM_CustomFlag1);
		}

		[ExpectNoExceptions]
		public void TestGarbageShortDataDoesNotBlowUp()
		{
			var reader = new CustomLabelsCustomizedFieldDataObjectReader(logger);

			var org = Factory.New<OrgHeader>();
			var dummy = Factory.New<DummyBusinessObject>();
			var provider = new CustomLabelsProvider(org);
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			org.AddCustomLabel("Short", "Short");

			container.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			container.CustomizedFieldCollection.Add(new CustomizedField
			{
				Value = "garbageData",
				DataType = DataType.Short,
				Key = "Short"
			});
			reader.PopulateCustomFields(DummyBizoSchema.Instance, dummy.Row(), container, provider);
		}

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider fConfigOrgProvider)
			{
				ConfigOrgProvider = fConfigOrgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider { get; }

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(DummyBusinessObject), configOrg, (NoResString)string.Empty, factory)
				{
					{
						"Short", DummyBizoSchema.Constants.Z0_Short,
						Constants.CustomLabels.Descriptions.CustomAttribute(1)
					}
				};

				return result;
			}
		}
	}
}
