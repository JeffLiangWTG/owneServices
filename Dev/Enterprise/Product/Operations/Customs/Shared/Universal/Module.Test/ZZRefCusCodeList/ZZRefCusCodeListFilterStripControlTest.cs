using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public class ZZRefCusCodeListFilterStripControlTest : ZFilterStripControlTest
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestInitializeMandatoryAttributeColumns__ValueDateTypeCaseInsensitive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			const string dataGrouping = Core.Constants.CountryCodes.China;
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", dataGrouping);
			attributeName.ZXE_ColumnCaption = "Attribute Name 1";
			const string stringType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attributeName.ZXE_ValueDataType = stringType.Substring(0, 1).ToLower() + stringType.Substring(1);
			attributeName.ZXE_IsValueMandatory = true;
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, dataGrouping, "TP1", ZDateTime.Today);
			using (var control = new ZZRefCusCodeListFilterStripControl(collection, new ZZRefCusCodeListFilterStripBusinessObject()))
			{
				control.Dispose();
			}
		}

		[RequiresSTA]
		public void TestInitializeMandatoryAttributeColumns()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TP1", "Ref Code Type 1");
			var attrName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", Core.Constants.CountryCodes.China);
			attrName1.ZXE_ColumnCaption = "Attribute Name 1";
			attrName1.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			attrName1.ZXE_IsValueMandatory = true;
			var attrName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute Name 2", "TP1", Core.Constants.CountryCodes.China);
			attrName2.ZXE_ColumnCaption = "Attribute Name 2";
			attrName2.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer;
			attrName2.ZXE_IsValueMandatory = true;
			var collection = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			using (var form = new ZForm())
			using (var control = new ZZRefCusCodeListFilterStripControl(collection, new ZZRefCusCodeListFilterStripBusinessObject()))
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.Grid;
				var customColumn1 = grid.GetColumnStyle("ATT1");
				AssertNotNull("Should have added a columnStyle for ATT1.", customColumn1);
				Assert("ColumnStyle for ATT1 should be visible by default.", customColumn1.IsVisible);
				AssertNullOrEmpty("Should not have a GroupName.", customColumn1.GroupName.Caption);
				AssertType("Should created a TextBoxColumnStyle for a STRING AttrName.", typeof(ZTextBoxColumnStyleInfo), customColumn1);
				var customColumn2 = grid.GetColumnStyle("ATT2");
				AssertNotNull("Should have added a columnStyle for ATT2.", customColumn2);
				Assert("ColumnStyle for ATT2 should be visible by default.", customColumn2.IsVisible);
				AssertNullOrEmpty("Should not have a GroupName.", customColumn2.GroupName.Caption);
				AssertType("Should created a CalcEditColumnStyle for an INTEGER AttrName.", typeof(ZCalcEditColumnStyleInfo), customColumn2);
			}
		}
	}
}
