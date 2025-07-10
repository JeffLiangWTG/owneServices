using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGDataItemFormGuid))]
	sealed class UNDGDataItemFormGuidTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new UNDGDataItemFormGuid(Factory.New<DummyWithUNDGs>());
		}

		class DummyWithUNDGs : DummyBusinessObject, IUNDGDataItemProvider
		{
			public DummyWithUNDGs(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public UNDGDataItemCollection UNDGs
			{
				get
				{
					if (fUNDGs == null)
					{
						fUNDGs = new UNDGDataItemCollection(this);
					}
					return fUNDGs;
				}
			}
			UNDGDataItemCollection fUNDGs;

			bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
		}

		public void TestZTextBoxColumnWithInvalidEmptyValue()
		{
			var dummy = Factory.New<DummyWithUNDGsForReadOnlyOverride>();
			var item = Factory.New<DummyUNDGDataItemForReadOnlyOverride>();
			dummy.UNDGs.Add(item);
			var errorMessageBuilder = new StringBuilder();

			using (var form = new UNDGDataItemFormGuid(dummy))
			{
				form.Load += (sender, e) =>
				{
					var grid = (ZGrid)form.Controls["zGrid1"];

					for (var i = 0; i < grid.Columns.Count; i++)
					{
						if (grid.Columns[i].ColumnStyle is ZTextBoxColumnStyle style && style.TextBox is DataGridTextBox dataGridTextBox)
						{
							try
							{
								var cell1 = new DataGridCell(0, i);
								var columnBeside = i == 0 ? i + 1 : i - 1;
								var cell2 = new DataGridCell(0, columnBeside);

								grid.CurrentCell = cell1;

								dataGridTextBox.Text = string.Empty;
								dataGridTextBox.IsInEditOrNavigateMode = false;

								grid.CurrentCell = cell2;
							}
							catch (Exception ex)
							{
								errorMessageBuilder.AppendLine($"{grid.Columns[i].ColumnName} Column with ZTextBoxColumnStyle may cause the following exception. Please consider using a proper type of ColumnStyle instead : {ex}");
							}
						}
					}
				};
				form.Show();
				AssertNullOrEmpty(errorMessageBuilder.ToString());
			}
		}

		[RequiresSTA]
		public void TestColumnGroups()
		{
			var dummy = Factory.New<DummyWithUNDGsForReadOnlyOverride>();
			using (var form = new UNDGDataItemFormGuid(dummy))
			{
				var allColumns = form.zGrid1.ColumnStyles.ToArray().OfType<ZGridColumnInfo>();
				var cfrColumns = allColumns
					.Where(col => col.GroupName.Caption == "Dangerous Goods 49 CFR")
					.Select(col => col.ColumnName).ToList();
				var radioactiveColumns = allColumns
					.Where(col => col.GroupName.Caption == "Radioactive")
					.Select(col => col.ColumnName).ToList();

				CombineAssertions(() =>
				{
					AssertSequencesEqual(new[]
					{
						"DI_SpecialPermitIssueDate",
						"DI_SpecialPermitNumber",
						"DI_HazardousWasteCode",
						"DI_IsSalvagePackaging",
						"DI_IsResidueLastContained"
					}, cfrColumns);

					AssertSequencesEqual(new[]
					{
						"DI_RadioactiveLabelCategory",
						"DI_RadioactiveTransportIndex",
						"DI_IsHighwayRouteControlledQuantity",
						"DI_IsExclusiveUse",
						"DI_MaterialFormDescription",
						"DI_IsFissileExcepted",
						"DI_RadionuclideElement",
						"DI_RadionuclideElementSuffix",
						"DI_RadioactiveMaximumActivity",
						"DI_RadioactiveMaximumActivityUnit",
					}, radioactiveColumns);
				});
			}
		}

		class DummyWithUNDGsForReadOnlyOverride : DummyBusinessObject, IUNDGDataItemProvider
		{
			public DummyWithUNDGsForReadOnlyOverride(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public UNDGDataItemCollection UNDGs
			{
				get
				{
					if (fUNDGs == null)
					{
						fUNDGs = new DummyUNDGDataCollectionForReadOnlyOverride(this);
					}
					return fUNDGs;
				}
			}
			UNDGDataItemCollection fUNDGs;

			bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;
		}

		class DummyUNDGDataCollectionForReadOnlyOverride : UNDGDataItemCollection
		{
			public DummyUNDGDataCollectionForReadOnlyOverride(IUNDGDataItemProvider master, params OrgHeader[] contactSourceOrganisations) : base(master, contactSourceOrganisations)
			{
			}

			public new DummyUNDGDataItemForReadOnlyOverride this[int index]
			{
				get
				{
					return (DummyUNDGDataItemForReadOnlyOverride)base[index];
				}
			}

			public new DummyUNDGDataItemForReadOnlyOverride AddNew()
			{
				return (DummyUNDGDataItemForReadOnlyOverride)base.AddNew();
			}

			protected override UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
			{
				return new DummyUNDGDataItemStandAloneCollection(Factory, typeof(DummyUNDGDataItemForReadOnlyOverride));
			}

			public class DummyUNDGDataItemStandAloneCollection : UNDGDataItemStandAloneCollection
			{
				public DummyUNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type)
					: base(factory, type)
				{
				}

				public new DummyUNDGDataItemForReadOnlyOverride this[int index] => (DummyUNDGDataItemForReadOnlyOverride)base[index];

				public new DummyUNDGDataItemForReadOnlyOverride AddNew() => (DummyUNDGDataItemForReadOnlyOverride)base.AddNew();
			}
		}

		class DummyUNDGDataItemForReadOnlyOverride : UNDGDataItem
		{
			public DummyUNDGDataItemForReadOnlyOverride(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool DI_RadioactiveTransportIndex_ReadOnly_Override { get; set; }

			protected override bool DI_RadioactiveTransportIndex_ReadOnly => DI_RadioactiveTransportIndex_ReadOnly_Override;

			public bool DI_RadioactiveMaximumActivity_ReadOnly_Override { get; set; }

			public override bool DI_RadioactiveMaximumActivity_ReadOnly => DI_RadioactiveMaximumActivity_ReadOnly_Override;
		}
	}
}
