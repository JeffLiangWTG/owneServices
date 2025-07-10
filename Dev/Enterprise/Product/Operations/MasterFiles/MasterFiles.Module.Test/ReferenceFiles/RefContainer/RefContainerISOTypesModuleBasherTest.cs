using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefContainerISOTypesModule))]
	sealed class RefContainerISOTypesModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefContainerISOTypes;
		}

		[RequiresSTA]
		public void TestFilterControl()
		{
			using (var controlForTest = containerIsoTypeModule.GetNewFilterControlForTest())
			{
				Assert(controlForTest is RefContainerISOTypesFilterControl);
			}
		}

		public void TestGridCollection()
		{
			var collectionForTest = containerIsoTypeModule.GetNewGridCollectionForTest();
			Assert(collectionForTest is NonPersistentBusinessObjectCollection<ContainerISOType>);
		}

		public void TestFilterBusinessObject()
		{
			var businessForTest = containerIsoTypeModule.GetNewFilterBusinessObjectForTest();
			Assert(businessForTest is FilterBusinessObject);
		}

		public void TestActions()
		{
			Assert(!containerIsoTypeModule.AllowNew);
			Assert(!containerIsoTypeModule.AllowEdit);
			Assert(!containerIsoTypeModule.AllowDelete);
			Assert(!containerIsoTypeModule.AllowCopyFilterGridHyperlinkToClipboard);
			Assert(!containerIsoTypeModule.AllowView);
		}

		public void TestHasNoAdditionalMenuItems()
		{
			Assert(containerIsoTypeModule.GetNewAdditionalMenuItems().Length == 0);
		}

		public void TestCanReloadWithFilterIsTrue()
		{
			Assert(containerIsoTypeModule.CanReloadWithFilter(null, null, null));
		}

		[RequiresSTA]
		public void TestPerformSearch()
		{
			using (var form = new ZForm())
			{
				containerIsoTypeModule.FilterBusinessObject.SetInitialCodeForSearch("ANY5", "P1");
				var collection = new RefContainerISOTypesCollection(new BusinessObjectFactory());
				collection.AddRange(new[]
				{
					new ContainerISOType() { ISOCode = "ANY" },
					new ContainerISOType() { ISOCode = "ANY2" },
					new ContainerISOType() { ISOCode = "ANY3" },
					new ContainerISOType() { ISOCode = "ANY4" },
					new ContainerISOType() { ISOCode = "ANY5" },
					new ContainerISOType() { ISOCode = "ANY6" },
				});
				containerIsoTypeModule.GridCollection.AddRange(collection);
				form.Controls.Add(containerIsoTypeModule.EmbeddedControl);
				form.Show();

				containerIsoTypeModule.PerformSearch();

				Assert(containerIsoTypeModule.DisplayGrid.SelectedElements.Length == 1);
				Assert(((ContainerISOType)containerIsoTypeModule.DisplayGrid.SelectedElements[0]).ISOCode == "ANY5");
			}
		}

		[RequiresSTA]
		public void TestPerformSearchBeforeGridProperLoading()
		{
			using (var form = new ZForm())
			{
				containerIsoTypeModule.FilterBusinessObject.SetInitialCodeForSearch("ANY5", "P1");
				var collection = new RefContainerISOTypesCollection(new BusinessObjectFactory());
				collection.AddRange(new[]
				{
					new ContainerISOType() { ISOCode = "ANY" },
					new ContainerISOType() { ISOCode = "ANY2" },
					new ContainerISOType() { ISOCode = "ANY3" },
					new ContainerISOType() { ISOCode = "ANY4" },
					new ContainerISOType() { ISOCode = "ANY5" },
					new ContainerISOType() { ISOCode = "ANY6" },
				});
				containerIsoTypeModule.GridCollection.AddRange(collection);
				form.Controls.Add(containerIsoTypeModule.EmbeddedControl);

				AssertNull(containerIsoTypeModule.DisplayGrid.List);
				AssertNoExceptionThrown(() => containerIsoTypeModule.PerformSearch());
				Assert(containerIsoTypeModule.DisplayGrid.SelectedElements.Length == 0);

				form.Show();
				AssertNotNull(containerIsoTypeModule.DisplayGrid.List);
				containerIsoTypeModule.PerformSearch();

				Assert(containerIsoTypeModule.DisplayGrid.SelectedElements.Length == 1);
				Assert(((ContainerISOType)containerIsoTypeModule.DisplayGrid.SelectedElements[0]).ISOCode == "ANY5");
			}
		}

		public override void TestModuleShowsAndCanSearch()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			containerIsoTypeModule = new RefContainerISOTypesModuleForTest();
			base.SetUp();
		}

		protected override void TearDown()
		{
			containerIsoTypeModule.Dispose();
			base.TearDown();
		}
		RefContainerISOTypesModuleForTest containerIsoTypeModule;
	}
}
