using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskTemplateLookups : AutoProcessTaskTemplateLookups
	{
		public ProcessTaskTemplateLookups(AutoProcessTaskTemplate parent)
			: base(parent)
		{
		}

		public new ProcessTaskTemplate Parent
		{
			get { return (ProcessTaskTemplate)base.Parent; }
		}

		#region Process Types

		public CodeDescriptionPairList WorkflowTypeList
		{
			get => Factory.GetCachedValue("ITemplateWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<ITemplateWorkflowDescriptorList>());
		}

		#endregion

		#region Clients

		public override OrgHeaderCollection Clients
		{
			get { return new OrganisationsFindBoxCollection(Factory); }
		}

		#endregion

		#region Warehouses

		public BusinessObjectCollection Warehouses
		{
			get
			{
				var warehouseType = Parent.WorkflowDescriptor != null ? Parent.WorkflowDescriptor.WarehouseType : WarehouseCollectionType.ProductWarehouse;
				return Factory.GetCachedValue("ProcessTaskTemplateLookups|WarehousesType|" + warehouseType, delegate
				{
					var whsWarehouseCollectionType = ObjectFactory.GetType<IWhsWarehouseCollection>();
					var result = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, Factory, warehouseType);
					result.Load();
					return result;
				});
			}
		}

		#endregion

		#region Buffer Management Systems

		public IBusinessObjectCollection BMSystems
		{
			get
			{
				return Factory.GetCachedValue("BMSystems", delegate
				{
					Type bmsCollectionType = ObjectFactory.GetType<BufferManagement.Integration.IBMSystemCollection>();
					IActiveBusinessObjectCollection result = (IActiveBusinessObjectCollection)Activator.CreateInstance(bmsCollectionType, Factory);
					return result;
				});
			}
		}

		#endregion

		#region Sub Type Lists

		[SuppressWeaklyTypedCollectionMessage]
		public IList List1
		{
			get { return GetList(1); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List2
		{
			get { return GetList(2); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List3
		{
			get { return GetList(3); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List4
		{
			get { return GetList(4); }
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList List5
		{
			get { return GetList(5); }
		}

		IList GetList(int listNumber)
		{
			var subtypeInformation = Parent.WorkflowSubTypeInformation;
			if (subtypeInformation.Length > listNumber - 1)
			{
				var info = subtypeInformation[listNumber - 1];
				if (info.UseCollection)
				{
					return info.Collection;
				}
				else
				{
					return info.List;
				}
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Fallback Types

		public CodeDescriptionPairList FallbackTypes
		{
			get { return Factory.GetCachedValue<FallbackTypeList>(); }
		}

		public CodeDescriptionPairList CustomFieldFallbackTypes
		{
			get { return Factory.GetCachedValue(nameof(CustomFieldFallbackTypes), InitCustomFieldFallbackTypes); }
		}

		CodeDescriptionPairList InitCustomFieldFallbackTypes()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FallbackTypeList.Codes.AlwaysFallback, FallbackTypeList.Descriptions.AlwaysFallback);
			list.AddPair(FallbackTypeList.Codes.NeverFallback, FallbackTypeList.Descriptions.NeverFallback);
			return list;
		}

		public ICodeDescriptionPairList ReleaseGroupFallbackMethods => Factory.GetCachedValue(nameof(ReleaseGroupFallbackMethods), () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(FallbackTypeList.Codes.EmptyFallback, Res.GetString("87810b25-2710-4e9e-9427-1cfa41349854", "Falls back if no Release Group is determined"));
			list.AddPair(FallbackTypeList.Codes.NeverFallback, Res.GetString("922d26db-815f-4e0f-bdda-f59ab9c57eab", "Does not fall back, even if no Release Group is determined"));

			return list;
		});

		#endregion

		#region Validation Rules

		public CodeDescriptionPairList ProcessTemplateValidationActionSourceList
		{
			get
			{
				var countryCode = Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty;
				return Factory.GetCachedValue($"ProcessTaskTemplateLookups.ProcessTemplateValidationActionSourceList_{Parent.P0_ProcessType}_{countryCode}", delegate {
						return Parent.WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationActionSourceList(countryCode) ?? new CodeDescriptionPairList();
				});
			}
		}

		#endregion
	}
}
