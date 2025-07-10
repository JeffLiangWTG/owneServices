using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.IZType>;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(DGClassDGSubstanceFilter))]
	internal sealed class DGClassDGSubstanceFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Test DGClass Pre-Filling

		public void TestDGClass_PreFillsWhenNeccessary()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "9999";
			subs.DG_Variant = "b";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "3";

			Factory.Save();

			Filter.DGSubstance = "9999b";
			AssertEquals("DG Class should remain empty", "", Filter.DGClass);

			Filter.DGClass = "3";
			Filter.DGSubstance = "9999b";
			AssertEquals("DG Class should equal 3, as the substances DG class is 3", "3", Filter.DGClass);

			Filter.DGClass = "4";
			Filter.DGSubstance = "9999b";
			AssertEquals("DG Class should be updated to equal 3, as the substances DG class is 3, not 4", "3", Filter.DGClass);
		}

		#endregion

		#region Test DG Class / DG Substance Read Only

		public void TestDGClass_ReadOnly()
		{
			DGSubstanceTestHelper.Create("9999", "b", "IMO", additionalInitialisation: (subs) => subs.DG_Class = "3");
			Filter.ComparisonOperator = ComparisonConstants.Contains;
			Filter.DGClass = "1.1";

			AssertEquals("DG Class should not be readonly", Filter.DGClass_ReadOnly, false);
			AssertEquals("DG Class should not be blank", "1.1", Filter.DGClass);

			Filter.ComparisonOperator = ComparisonConstants.IsBlank;

			AssertEquals("DG Class should be readonly", true, Filter.DGClass_ReadOnly);
			AssertEquals("DG Class should be blank", ZString.Empty, Filter.DGClass);

			Filter.ComparisonOperator = ComparisonConstants.Exact;

			AssertEquals("DG Class should not be readonly", false, Filter.DGClass_ReadOnly);
			AssertEquals("DG Class should not be blank", "1.1", Filter.DGClass);

			Filter.ComparisonOperator = ComparisonConstants.IsNotBlank;

			AssertEquals("DG Class should be readonly", true, Filter.DGClass_ReadOnly);
			AssertEquals("DG Class should be blank", ZString.Empty, Filter.DGClass);
		}

		public void TestDGSubstance_ReadOnly()
		{
			DGSubstanceTestHelper.Create("9999", "b", "IMO", additionalInitialisation: (subs) => subs.DG_Class = "3");
			Filter.ComparisonOperator = ComparisonConstants.Contains;
			Filter.DGSubstance = "1100";

			AssertEquals("DG Substance should not be readonly", false, Filter.DGSubstance_ReadOnly);
			AssertEquals("DG Substance should not be blank", "1100", Filter.DGSubstance);

			Filter.ComparisonOperator = ComparisonConstants.IsBlank;

			AssertEquals("DG Substance should be readonly", true, Filter.DGSubstance_ReadOnly);
			AssertEquals("DG Substance should be blank", ZString.Empty, Filter.DGSubstance);

			Filter.ComparisonOperator = ComparisonConstants.Exact;

			AssertEquals("DG Substance should not be readonly", false, Filter.DGSubstance_ReadOnly);
			AssertEquals("DG Substance should not be blank", "1100", Filter.DGSubstance);

			Filter.ComparisonOperator = ComparisonConstants.IsNotBlank;

			AssertEquals("DG Substance should be readonly", true, Filter.DGSubstance_ReadOnly);
			AssertEquals("DG Substance should be blank", ZString.Empty, Filter.DGSubstance);
		}

		#endregion

		#region Test DG Class MaxLength

		public void TestDGClass_MaxLength()
		{
			using (var form = new ZForm())
			using (var jobShipmentModuleStrip = new JobShipmentModuleStrip())
			using (var dgClassDGSubstanceFilterControl = new DGClassDGSubstanceFilterControl(jobShipmentModuleStrip))
			{
				var dgClassDGSubstanceFilter = new DGClassDGSubstanceFilter("description", delegate
				{ return new ZQuery(); });
				dgClassDGSubstanceFilterControl.SetDataBinding(dgClassDGSubstanceFilter, string.Empty);
				form.Controls.Add(dgClassDGSubstanceFilterControl);
				form.Show();

				var dgClassDropEdit = dgClassDGSubstanceFilterControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "DGClassDropEdit");
				AssertNotNull(dgClassDropEdit);

				var codeBox = dgClassDropEdit.FindSingleOrDefault<ZDropCodeBox.Bare>(c => c.Name == "CodeBox");
				AssertNotNull(codeBox);

				for (var index = 0; index < 10; index++)
				{
					TestKeyStrokeHelper.SendKeyToControl(codeBox, Keys.D1, doEvents: false);
				}
				Application.DoEvents();

				AssertEquals("DGClass MaxLength: 4", "1111", codeBox.Text);
			}
		}

		#endregion

		#region Implementation

		DGClassDGSubstanceFilter Filter
		{
			get { return filter ?? (filter = (DGClassDGSubstanceFilter)GetNewBusinessObject()); }
		}
		DGClassDGSubstanceFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DGClassDGSubstanceFilter("yeet", (op, dgClass, dgSubstance) => new ZQuery());
		}

		#endregion
	}
}
