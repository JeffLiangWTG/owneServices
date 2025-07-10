using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(OrgAddressEmbeddedModulePopup))]
	sealed class OrgAddressEmbeddedModulePopupTest : EmbeddModulePopupBasherTest
	{
		[RequiresSTA]
		public void TestAddressSearchScreenPopupWithPADTypeInFilterWhenTypeIsPicOrDLV()
		{
			var assertWithAddressType = new Action<ZArchitecture.Business.AddressType>(type =>
			{
				using (var popup = GetPopup(type))
				{
					popup.Show();
					var filterStrips = popup.Module_ForTest.FilterBusinessObject.FilterStrips.OfType<FilterStrip>().ToList();
					CombineAssertions(() =>
					{
						AssertEquals("Should have 2 filter strips", 2, filterStrips.Count);
						Assert("2 filter strips both with description \"Type\"", filterStrips.All(f => f.FilterDescription == "Type"));
						Assert("2 filter strips have same Or Category: Red", filterStrips.All(f => f.OrCategory == FilterOrCategory.Red));
						AssertContains("Query should also filter with PAD", string.Format(@"SELECT PZ_OA FROM dbo.OrgAddressCapability WHERE PZ_AddressType = '{0}' 
		OR
		PZ_AddressType = 'PAD'", type), popup.Module_ForTest.FilterBusinessObject.Filter.LiteralTextSqlFormatted);
					});
				}
			});

			assertWithAddressType(ZArchitecture.Business.AddressType.PIC);
			assertWithAddressType(ZArchitecture.Business.AddressType.DLV);
		}

		[RequiresSTA]
		public void TestPreSpecifyOrganisationFilter()
		{
			using (Popup)
			{
				popup.Show();

				var orgFilter = Popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Organisation"] as ModuleGuidFilter;
				AssertNotNull("Organisation Filter exists", orgFilter);
				AssertEquals("Organisation Filter is always applied and hidden", FilterVisibility.AlwaysAppliedAndHidden, orgFilter.Visibility);
				AssertEquals("Organisation Filter's property is parent organisation's PK", lastOrgPK, orgFilter.Property);
			}
		}

		[RequiresSTA]
		public void TestIEmbeddedModulePopupOKButtonStrategy()
		{
			using (Popup)
			{
				Assert(Popup is IEmbeddedModulePopupOKButtonStrategy);
			}
		}

		[RequiresSTA]
		public void TestDefaultAddressType()
		{
			using (var popup = GetPopup(ZArchitecture.Business.AddressType.PIC))
			{
				popup.Show();

				var typeFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Type"] as ModuleTextFilter;
				AssertNotNull("Address Type Filter exists", typeFilter);
				AssertEquals("Address Type Filter is always visible", FilterVisibility.AlwaysVisible, typeFilter.Visibility);
				AssertEquals("Address Type Filter's property is \"PIC\"", "PIC", typeFilter.Property);
			}
		}

		[RequiresSTA]
		public void TestEmptyAddressType()
		{
			using (var popup = GetPopup(null))
			{
				popup.Show();

				var typeFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Type"] as ModuleTextFilter;
				AssertNotNull("Address Type Filter exists", typeFilter);
				AssertEquals("Address Type Filter is always visible", FilterVisibility.AlwaysVisible, typeFilter.Visibility);
				AssertEquals("Address Type Filter's property is \"\"", string.Empty, typeFilter.Property);
			}
		}

		[RequiresSTA]
		public void TestHandleFindBoxOKButton()
		{
			var address = Factory.New<OrgAddress>();
			using (Popup)
			{
				Popup.Show();
				Popup.Selected += (_, e) =>
				{
					AssertEquals("One address selected", 1, e.SelectedBusinessObjects.Length);
					AssertEquals("Address selected", address.PK, e.SelectedBusinessObjects[0].PK);
				};
				Popup.HandleFindBoxOKButton(new[] { address });
			}
		}

		[RequiresSTA]
		public void TestWithUserDefinedLayout()
		{
			var module = ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses);
			var filter = module.FilterBusinessObject.Layouts.AddNew();
			filter.S9_FilterName = "This one ";

			using (var popup = GetPopup(ZArchitecture.Business.AddressType.OFC, module))
			{
				popup.Show();

				var typeFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Type"] as ModuleTextFilter;
				AssertNotNull("Address Type Filter exists", typeFilter);
				AssertEquals("Address Type Filter is always visible", FilterVisibility.AlwaysVisible, typeFilter.Visibility);
				AssertEquals("Address Type Filter's property is \"OFC\"", "OFC", typeFilter.Property);

				var orgFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Organisation"] as ModuleGuidFilter;
				AssertNotNull("Organisation Filter exists", orgFilter);
				AssertEquals("Organisation Filter is always applied and hidden", FilterVisibility.AlwaysAppliedAndHidden, orgFilter.Visibility);
				AssertEquals("Organisation Filter's property is parent organisation's PK", lastOrgPK, orgFilter.Property);
			}
		}

		[RequiresSTA]
		public void TestWithUserDefinedLayout_TypeIsNull()
		{
			var module = ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses);
			var filter = module.FilterBusinessObject.Layouts.AddNew();
			filter.S9_FilterName = "This one ";

			using (var popup = GetPopup(null, module))
			{
				popup.Show();

				var typeFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Type"] as ModuleTextFilter;
				AssertNotNull("Address Type Filter exists", typeFilter);
				AssertEquals("Address Type Filter is always visible", FilterVisibility.AlwaysVisible, typeFilter.Visibility);
				AssertEquals("Address Type Filter's property is \"\"", string.Empty, typeFilter.Property);

				var orgFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Organisation"] as ModuleGuidFilter;
				AssertNotNull("Organisation Filter exists", orgFilter);
				AssertEquals("Organisation Filter is always applied and hidden", FilterVisibility.AlwaysAppliedAndHidden, orgFilter.Visibility);
				AssertEquals("Organisation Filter's property is parent organisation's PK", lastOrgPK, orgFilter.Property);
			}
		}

		[RequiresSTA]
		public void TestWithUserDefinedLayout_TypeIsPicOrDLV()
		{
			var assertWithAddressType = new Action<ZArchitecture.Business.AddressType>(type =>
			{
				var module = ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses);
				var filter = module.FilterBusinessObject.Layouts.AddNew();
				filter.S9_FilterName = "This one " + type.ToString();

				using (var popup = GetPopup(type, module))
				{
					popup.Show();

					var orgFilter = popup.Module_ForTest.FilterBusinessObject.ModuleFilters["Organisation"] as ModuleGuidFilter;
					CombineAssertions(() =>
					{
						AssertNotNull("Organisation Filter exists", orgFilter);
						AssertEquals("Organisation Filter is always applied and hidden", FilterVisibility.AlwaysAppliedAndHidden, orgFilter.Visibility);
						AssertEquals("Organisation Filter's property is parent organisation's PK", lastOrgPK, orgFilter.Property);
						AssertContains($"OA_OH = '{lastOrgPK}'", popup.Module_ForTest.FilterBusinessObject.Filter.LiteralTextSqlFormatted);
					});

					var filterStrips = popup.Module_ForTest.FilterBusinessObject.FilterStrips.OfType<FilterStrip>().ToList();
					CombineAssertions(() =>
					{
						AssertEquals("Should have 2 filter strips", 2, filterStrips.Count);
						Assert("2 filter strips both with description \"Type\"", filterStrips.All(f => f.FilterDescription == "Type"));
						Assert("2 filter strips have same Or Category: Red", filterStrips.All(f => f.OrCategory == FilterOrCategory.Red));
						AssertContains("Query should also filter with PAD", string.Format(@"SELECT PZ_OA FROM dbo.OrgAddressCapability WHERE PZ_AddressType = '{0}' 
		OR
		PZ_AddressType = 'PAD'", type), popup.Module_ForTest.FilterBusinessObject.Filter.LiteralTextSqlFormatted);
					});
				}
			});

			assertWithAddressType(ZArchitecture.Business.AddressType.PIC);
			assertWithAddressType(ZArchitecture.Business.AddressType.DLV);
		}

		OrgAddressEmbeddedModulePopup GetPopup(ZArchitecture.Business.AddressType? defaultAddressType, ZFilterModule filterModule = null)
		{
			var org = Factory.New<OrgHeader>();
			return new OrgAddressEmbeddedModulePopup(filterModule ?? ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses), (lastOrgPK = org.PK), defaultAddressType);
		}

		ZGuid lastOrgPK;

		OrgAddressEmbeddedModulePopup Popup => popup ?? (popup = GetPopup(ZArchitecture.Business.AddressType.OFC));

		OrgAddressEmbeddedModulePopup popup;

		protected override Form GetFormToBashCore() => Popup;
	}
}
