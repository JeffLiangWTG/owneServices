using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZModuleButtonGridForDesigner))]
	public class ZModuleButtonGridForDesignerTest : ZModuleButtonGridTestBase
	{
		[RequiresSTA]
		public void TestAttachedEvent()
		{
			var isAttachedCalled = false;
			using (var form = GetForm())
			{
				form.ZModuleButtonGrid.Attached += LinkedOrganizationsGrid_Attached;
				form.Show();

				AssertEquals("PreCondition", false, isAttachedCalled);
				form.ZModuleButtonGrid.AttachButtonForTest.PerformClick();

				form.BoundDataSource.FindBoxCollection.AddNew();
				form.BoundDataSource.FindBoxCollection.AddNew();
				form.HandleSelection(new BusinessObject[] { form.BoundDataSource.FindBoxCollection[0], form.BoundDataSource.FindBoxCollection[1] });

				AssertEquals("PreCondition", true, isAttachedCalled);
			}

			void LinkedOrganizationsGrid_Attached(object sender, ModuleButtonGridOnAttachEventArgs e)
			{
				isAttachedCalled = true;
			}
		}

		[RequiresSTA]
		public void TestBeforeAttached()
		{
			var isBeforeAttachedCalled = false;
			using (var form = GetForm())
			{
				form.ZModuleButtonGrid.BeforeAttached += LinkedOrganizationsGrid_BeforeAttached;
				form.Show();

				AssertEquals("PreCondition", false, isBeforeAttachedCalled);
				form.ZModuleButtonGrid.AttachButtonForTest.PerformClick();

				form.BoundDataSource.FindBoxCollection.AddNew();
				form.BoundDataSource.FindBoxCollection.AddNew();
				form.HandleSelection(new BusinessObject[] { form.BoundDataSource.FindBoxCollection[0], form.BoundDataSource.FindBoxCollection[1] });

				AssertEquals("PreCondition", true, isBeforeAttachedCalled);
			}

			void LinkedOrganizationsGrid_BeforeAttached(object sender, ModuleButtonGridOnAttachEventArgs e)
			{
				isBeforeAttachedCalled = true;
			}
		}

		#region Implementation

		DummyForm GetForm()
		{
			var bo = new DummyBO(Factory);
			return new DummyForm(bo) { CaptionRenderingEnabled = true };
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		class DummyForm : ZChildForm
		{
			public DummyForm(DummyBO businessEntity) : base(businessEntity)
			{
				BoundDataSource = businessEntity;
				ZModuleButtonGrid = new ZModuleButtonGridForDesigner()
				{
					Dock = DockStyle.Fill,
				};
				ZModuleButtonGrid.InnerGrid.ReadOnly = true;

				var zTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo.ColumnName = "OH_Code";
				ZModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);

				Controls.Add(ZModuleButtonGrid);
				BindingSource.SetBindingMember(ZModuleButtonGrid, "GridCollection");
				ZModuleButtonGrid.BindToFindBoxList = "FindBoxCollection";
			}

			public ZModuleButtonGridForDesigner ZModuleButtonGrid { get; }

			public DummyBO BoundDataSource { get; }

			public void HandleSelection(BusinessObject[] bizos)
			{
				var method = typeof(EmbeddedModulePopup).GetMethod("HandleSelection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(ZModuleButtonGrid.LastShownAttachPopupForTesting, new object[] { bizos });
			}
		}

		class DummyBO : NonPersistentBusinessObject
		{
			public DummyBO(BusinessObjectFactory factory) : base(factory)
			{
				GridCollection = new OrgHeaderCollection(Factory);
				FindBoxCollection = new OrgHeaderCollection(Factory);
			}

			public OrgHeaderCollection GridCollection { get; }

			public OrgHeaderCollection FindBoxCollection { get; }
		}

		#endregion
	}
}
