using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class CustomLabelsCustomizedFieldDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ZDY!DS#@";
			org.MiscServ.OM_CustomAttrib1 = "Attrib1";
			org.MiscServ.OM_CustomDate1 = ZDateTime.BrettsBirthday;
			org.MiscServ.OM_CustomDecimal1 = 120.54m;
			org.MiscServ.OM_CustomFlag1 = true;
			var orgMiscServRow = (IColumnIndexer)((IBusinessObjectInternals)org.MiscServ).Row;

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			CustomLabelsCustomizedFieldDataObjectWriter.Write(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertNull(container.CustomizedFieldCollection);

			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomAttribute1, "STRING1");
			CustomLabelsCustomizedFieldDataObjectWriter.Write(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertNotNull(container.CustomizedFieldCollection);
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "Attrib1");

			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomDate1, "DATE1");
			CustomLabelsCustomizedFieldDataObjectWriter.Write(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertNotNull(container.CustomizedFieldCollection);
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "Attrib1");
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", ZDateTime.BrettsBirthday.ToISO8601String());

			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomDecimal1, "DECIMAL1");
			CustomLabelsCustomizedFieldDataObjectWriter.Write(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertNotNull(container.CustomizedFieldCollection);
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "Attrib1");
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", ZDateTime.BrettsBirthday.ToISO8601String());
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "120.54");

			org.AddCustomLabel(Constants.CustomLabels.Organisation.CustomFlag1, "FLAG1");
			CustomLabelsCustomizedFieldDataObjectWriter.Write(OrgMiscServSchema.Instance, orgMiscServRow, container, org);
			AssertNotNull(container.CustomizedFieldCollection);
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING1", "Attrib1");
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE1", ZDateTime.BrettsBirthday.ToISO8601String());
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL1", "120.54");
			container.CustomizedFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "FLAG1", "true");
		}
	}
}
