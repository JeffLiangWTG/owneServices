using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ZMultiFindBoxColumnStyleTest : TestCaseWithDummy
	{
		public void TestSelectMultipleValues()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "ABC";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "DEF";

			Factory.Save();

			using (TestForm = CreateTestForm(SuperDummy, true))
			{
				TestForm.Show();

				var guidMultiFindBox = ((ZChargeCodesFindBoxColumnStyle)Grid.Columns.First(column => column.ColumnStyle is ZChargeCodesFindBoxColumnStyle).ColumnStyle).MultiFindBox;
				guidMultiFindBox.ModuleID = DummyModuleIDs.Dummy;

				Grid.CurrentCell = new DataGridCell(0, 0);
				new PopupModuleDecisionProviderWithMultipleSelect(guidMultiFindBox).HandleFindBoxOKButton(new[] { dummy1, dummy2 });

				AssertEquals("ABC, DEF", guidMultiFindBox.CodeBox.Text);
			}
		}

		[RequiresSTA]
		public void TestReadOnly()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				SuperDummy.Collection.AddNew();
				SuperDummy.Collection[0].Z0_NVarCharMax = Child1.Z0_Code;
				((IZPropertyInfoObsolete)SuperDummy.Collection[0].Z0_NVarCharMaxInfo).ReadOnly = true;

				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Right);
				AssertEquals("Navigated Right", new DataGridCell(0, 1), Grid.CurrentCell);
				AssertEquals("Text while ReadOnly", "CH1", Grid.LastFocusedColumn.TextBox.Text);
				AssertEquals("All Text should be selected when entering readonly column", "CH1", Grid.LastFocusedColumn.TextBox.SelectedText);
			}
		}

		#region Test Classes

		protected class TestBusinessObject : BusinessObject, IObsoleteValidation
		{
			#region Property Constants

			public abstract class Schema
			{
				public const string TableName = "DUMMYBIZO";
				public const string SS_Name = "Z0_Description";
			}

			#endregion

			public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region Business Object Overrides

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				DummyEnterpriseBusinessObject.SetDataRowDefaultValues(((INeedRow)this).Row);
			}

			#endregion

			#region PK

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return DummyBizoSchema.PK; }
			}

			#endregion

			#region Dummy Collection

			public DummyEnterpriseBusinessObjectCollection Collection
			{
				get
				{
					if (fCollection == null)
					{
						fCollection = new DummyEnterpriseBusinessObjectCollection(Factory);
					}
					return fCollection;
				}
			}
			DummyEnterpriseBusinessObjectCollection fCollection;

			#endregion
		}

		protected class DummyEnterpriseBusinessObjectCollection : DummyBusinessObjectCollection
		{
			public DummyEnterpriseBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new DummyEnterpriseBusinessObject this[int i]
			{
				get { return (DummyEnterpriseBusinessObject)base[i]; }
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Child1 = Factory.New<DummyChildBusinessObject>();
			Child1.Z0_Code = "CH1";

			Child2 = Factory.New<DummyChildBusinessObject>();
			Child2.Z0_Code = "CH2";

			Child3 = Factory.New<DummyChildBusinessObject>();
			Child3.Z0_Code = "CH3";

			SuperDummy = Factory.New<TestBusinessObject>();
		}

		TestBusinessObject SuperDummy;
		DummyChildBusinessObject Child1;
		DummyChildBusinessObject Child2;
		DummyChildBusinessObject Child3;

		protected virtual void SendKeyToEditControl(Keys key)
		{
			ZGridFindBox findBox = Grid.LastFocusedColumn.EditControl as ZGridFindBox;
			KeySender.PostKeyDown(findBox.CodeBox, findBox.CodeBox.Handle, key);
			Application.DoEvents();
		}

		ZForm TestForm;

		protected virtual ZGrid Grid
		{
			get { return ((ZChargeCodesFindBoxColumnStyleTestForm)TestForm).zGrid1; }
		}

		void AssertPreconditions()
		{
			AssertNotNull(TestForm);
			AssertNotNull(Grid);
			AssertNotNull(Grid.LastFocusedColumn);
			AssertNotNull(Grid.LastFocusedColumn.EditControl);
			AssertEquals("Default Cell Position", new DataGridCell(0, 0), Grid.CurrentCell);
		}

		ZForm CreateTestForm(BusinessObject superDummy, bool addMultiFindColumnFirst)
		{
			return new ZChargeCodesFindBoxColumnStyleTestForm(SuperDummy, addMultiFindColumnFirst);
		}

		#endregion
	}
}
