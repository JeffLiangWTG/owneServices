using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : MessageSendingFormWithValidationDetailsAbstractTest
{
	public void TestGridLayout()
	{
		var expectedColumns = new List<(string ColumnName, Type InfoType)>
		{
			(DMOMessageSendingObject.Schema.CustomsLevel, typeof(ZTextBoxColumnStyle)),
			(DMOMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyle)),
			(DMOMessageSendingObject.Schema.Representative, typeof(ZTextBoxColumnStyle)),
			(DMOMessageSendingObject.Schema.Consignee, typeof(ZTextBoxColumnStyle))
		};

		AssertGridColumns("MessageSendingObjectsGrid", expectedColumns);
	}

	void AssertGridColumns(string gridName, List<(string ColumnName, Type InfoType)> expectedColumns)
	{
		using (var userControl = GetFormToBashCore())
		{
			userControl.Show();
			var grid = userControl.FindSingle<ZGrid>(gridName);
			CombineAssertions(() =>
			{
				foreach (var (columnName, columnStyleInfoType) in expectedColumns)
				{
					var column = grid.GetColumnStyle(columnName);
					AssertNotNull($"{columnName} Exists", column);
					AssertEquals($"{columnName} Type", columnStyleInfoType, column.ColumnStyleType);
					AssertEquals($"{columnName} is Visible", true, column.IsVisible);
				}
			});
		}
	}

	protected override Form GetFormToBashCore()
	{
		return new MessageSendingForm(MessageSendingObjectParent);
	}

	DMOMessageSendingObjectParent GetManifestMessageSendingObjectParent()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		return new DMOMessageSendingObjectParent(header);
	}

	DMOMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ??= GetManifestMessageSendingObjectParent();
	DMOMessageSendingObjectParent messageSendingObjectParent;
}
