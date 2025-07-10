using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

[assembly: ClusterKeyMetaData(typeof(CusEntryInstruction), ParentTableName = JobDeclarationSchema.Constants.TableName, ParentFkColumnName = nameof(CusEntryInstruction.CEI_JE))]

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow()]
	[CodeProperty(Schema.CEI_Style), DescriptionProperty(Schema.CEI_Description)]
	public class CusEntryInstruction : AutoCusEntryInstruction,
		Integration.Customs.ICusEntryInstruction,
		ICustomsCustomLabelsConfigOrgProvider,
		IClusterKeyWorker,
		IDocAddresses,
		IAddInfoChildSupporter,
		ITypeDeciderContext,
		ICusGoodsLocationTypeSupporter,
		IDataModelSupporter
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			CEI_OH_OwnerInfo.ValueChanged += CEI_OH_OwnerInfo_ValueChanged;
		}

		void CEI_OH_OwnerInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsDeleted)
			{
				JobDeclaration?.OnNewOwnerPartAttributeCaptionDetailsChanged(sender, e);
			}
		}

		public static readonly CusEntryInstructionTypeDecider TypeDecider = new CusEntryInstructionTypeDecider();

		#region Related Business Object

		public BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (!IsDeleted && (jobDeclaration == null || jobDeclaration.PK != CEI_JE))
				{
					var reference = CEI_JE.IsEmpty ? cEI_JECachedOnRelationshipResetByCore : CEI_JE;
					jobDeclaration = Factory.Load<BaseJobDeclaration>(reference);
				}

				return jobDeclaration != null && !jobDeclaration.IsDeleted ? jobDeclaration : null;
			}
		}

		BaseJobDeclaration jobDeclaration;

		public ZString CountryCode => JobDeclaration?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code;

		public CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeaderCached == null)
				{
					entryHeaderCached = new CachedProperty<CusEntryHeader>(Factory, () =>
					{
						var header = JobDeclaration?.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_CEI_Instruction == this.PK).OrderBy(x => x.CH_SystemCreateTimeUtc).FirstOrDefault();
						return header ?? InvoiceLines.FirstOrDefault()?.CusEntryLine?.Header;
					});
				}
				return entryHeaderCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> entryHeaderCached;

		public bool HasAnEntryWithEntryStatus => Factory.GetValue(ref hasAnEntryWithEntryStatusCached, () => JobDeclaration?.CustomsEntryHeaders.OfType<CusEntryHeader>().Any(x => x.CH_CEI_Instruction == this.PK && !x.CH_EntryStatus.IsEmpty) ?? false);
		CachedProperty<bool> hasAnEntryWithEntryStatusCached;

		public BaseJobComInvoiceLine[] InvoiceLines => Factory.GetValue(ref invoiceLinesCached, () =>
		{
			var parentDeclaration = JobDeclaration;
			if (parentDeclaration == null)
			{
				return Array.Empty<BaseJobComInvoiceLine>();
			}
			else
			{
				return parentDeclaration.InvoiceLines.Where(x => x.JI_CEI == PK).ToArray();
			}
		});
		CachedProperty<BaseJobComInvoiceLine[]> invoiceLinesCached;

		public bool HasInvoiceLineWithPPC(string pPC)
		{
			return InvoiceLines.Cast<BaseJobComInvoiceLine>().FirstOrDefault(line => line.JI_Calc_PreviousProcedure == pPC) != null;
		}

		#endregion

		#region Override Properties

		public override ZString CEI_DataModel
		{
			get { return base.CEI_DataModel; }
			set
			{
				this.ReportDataModelErrorIfNeeded(CEI_DataModelInfo, value);
				base.CEI_DataModel = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.Business.CusEntryInstruction|CEI_DateForDuty", Caption = "Date of Valuation")]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set => base.CEI_DateForDuty = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.StyleList))]
		public override ZString CEI_Style
		{
			get { return base.CEI_Style; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					var oldValue = CEI_Style;
					base.CEI_Style = value;
					var newValue = CEI_Style;
					if (!IsCopying && oldValue != newValue)
					{
						var parentDeclaration = JobDeclaration;
						if (parentDeclaration != null)
						{
							parentDeclaration.CustomsEntryInstructionProvider.RefreshSortedEntryInstructionList();
							foreach (var invoiceLine in InvoiceLines)
							{
								UpdateAndRefreshWhenStyleIsChanged(invoiceLine, oldValue, newValue);
							}
						}

						if (IsDescriptionDefaultedFromStyle)
						{
							CEI_Description = ((ZString)Lookups.StyleList.GetDescriptionFromCode(CEI_Style)).Left(CEI_DescriptionInfo.MaxLength);
						}
					}
				}
			}
		}

		protected virtual bool IsDescriptionDefaultedFromStyle => false;

		[RelatedBusinessObject("JobDeclaration")]
		public override ZGuid CEI_JE
		{
			get { return base.CEI_JE; }
			set
			{
				if (value.IsEmpty && !IsCopying)
				{
					cEI_JECachedOnRelationshipResetByCore = base.CEI_JE;
				}

				base.CEI_JE = value;
			}
		}
		ZGuid cEI_JECachedOnRelationshipResetByCore;
		public override ZGuid CEI_OA_Warehouse
		{
			get => base.CEI_OA_Warehouse;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_OA_Warehouse))
				{
					base.CEI_OA_Warehouse = value;
				}
			}
		}

		public override ZGuid CEI_OA_Warehouse2
		{
			get => base.CEI_OA_Warehouse2;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_OA_Warehouse2))
				{
					base.CEI_OA_Warehouse2 = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.MergeByList))]
		public override ZString CEI_MergeBy
		{
			get { return base.CEI_MergeBy; }
			set { base.CEI_MergeBy = value; }
		}

		public override ZString CEI_Description
		{
			get { return base.CEI_Description; }
			set
			{
				var oldValue = CEI_Description;
				base.CEI_Description = value;
				var parentDeclaration = JobDeclaration;
				var newValue = CEI_Description;
				if (oldValue != newValue && parentDeclaration != null)
				{
					parentDeclaration.CustomsEntryInstructionProvider.RefreshSortedEntryInstructionList();
					foreach (var invoiceLine in InvoiceLines)
					{
						UpdateAndRefreshWhenDescriptionIsChanged(invoiceLine, oldValue, newValue);
					}
				}
			}
		}

		public override ZString CEI_SubStyle
		{
			get => base.CEI_SubStyle;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_SubStyle))
				{
					base.CEI_SubStyle = value;
				}
			}
		}

		public override bool CanDelete => base.CanDelete && InvoiceLines.All(line => line.CusEntryLine == null);

		public override MultilingualString ReasonForNotAbleToDelete => base.ReasonForNotAbleToDelete.IsEmpty
			? ResString.GetMultilingualString(
				"152EAB64-2F4E-4E1A-861B-C3C93E006AC1",
				"Entry Instruction with {0} {1} is being used by an Entry Line and cannot be deleted.",
				CEI_StyleInfo.HumanReadableName,
				CEI_Style
			)
			: base.ReasonForNotAbleToDelete;

		#endregion

		#region Override Methods

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateDataModelIfNeeded();
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			ContainersPivot.DeleteAll();
			var parentDeclaration = JobDeclaration;
			foreach (BaseJobComInvoiceLine invLine in InvoiceLines)
			{
				invLine.JI_CEI = ZGuid.Empty;
			}
			base.Delete();
			if (parentDeclaration != null)
			{
				parentDeclaration.CustomsEntryInstructionProvider.RefreshSortedEntryInstructionList();
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.CusEntryInstructionFetchStrategy(this);
		}

		protected override ZAddress GetNewCEI_OA_Warehouse_ZAddress()
		{
			var zAddress = base.GetNewCEI_OA_Warehouse_ZAddress();
			zAddress.GetDefaultAddress = GetDefaultAddress;
			return zAddress;
		}

		protected override ZAddress GetNewCEI_OA_Warehouse2_ZAddress()
		{
			var zAddress = base.GetNewCEI_OA_Warehouse2_ZAddress();
			zAddress.GetDefaultAddress = GetDefaultAddress;
			return zAddress;
		}

		ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			var result = ZGuid.Empty;
			var organisation = orgHeader as OrgHeader;
			if (organisation != null)
			{
				var addresses = organisation.Addresses;
				if (addresses.Count == 1)
				{
					result = addresses[0].PK;
				}
				else
				{
					var whsAddresses = addresses.Cast<OrgAddress>().Where(x => x.GetWhsWarehouse() != null).Take(2).ToArray();
					if (whsAddresses.Length == 1)
					{
						result = whsAddresses[0].PK;
					}
				}
			}

			return result;
		}

		#endregion

		#region New Properties

		public virtual ZString FromWarehouseCode => Factory.GetValue(ref fromWarehouseCodeCached, GetFromWarehouseCode);
		CachedProperty<ZString> fromWarehouseCodeCached;

		protected virtual ZString GetFromWarehouseCode() => Warehouse?.CustomsCodes?.GetCustomsRegNo(OrgCusCodeTypeForWarehouse, JobDeclaration?.CountryCode ?? Env.CurrentCompany.Country.Code) ?? ZString.Empty;

		public virtual ZString ToWarehouseCode => Factory.GetValue(ref toWarehouseCodeCached, GetToWarehouseCode);
		CachedProperty<ZString> toWarehouseCodeCached;

		protected virtual ZString GetToWarehouseCode() => Warehouse2?.CustomsCodes?.GetCustomsRegNo(OrgCusCodeTypeForWarehouse, JobDeclaration?.CountryCode ?? Env.CurrentCompany.Country.Code) ?? ZString.Empty;

		protected virtual ZString OrgCusCodeTypeForWarehouse => OrgCusCode.CodeTypes.WarehouseControlledPremisesID;

		public InventoryManagementSetting OwnerInventoryManagementSetting => Factory.GetValue(ref ownerInventoryManagementSettingCached, () => InventoryManagementSetting.New(Owner));
		CachedProperty<InventoryManagementSetting> ownerInventoryManagementSettingCached;
		public bool OwnerIsBondedWarehousing => OwnerInventoryManagementSetting.SupportBondedWarehouse;
		public bool OwnerIsInwardProcessing => OwnerInventoryManagementSetting.SupportInwardProcessing;
		public bool OwnerIsOutwardProcessing => OwnerInventoryManagementSetting.SupportOutwardProcessing;
		public bool OwnerIsInventoryManagementOn => OwnerIsBondedWarehousing || OwnerIsInwardProcessing || OwnerIsOutwardProcessing;

		public InventoryManagementSetting ClientInventoryManagementSetting => Factory.GetValue(ref clientInventoryManagementSettingCached, () => JobDeclaration is BaseJobDeclaration declaration ? new InventoryManagementSetting(declaration.ClientIsBondedWarehousing, declaration.ClientIsInwardProcessing, declaration.ClientIsOutwardProcessing) : new InventoryManagementSetting(false, false, false));
		CachedProperty<InventoryManagementSetting> clientInventoryManagementSettingCached;
		public bool ClientIsBondedWarehousing => ClientInventoryManagementSetting.SupportBondedWarehouse;
		public bool ClientIsInwardProcessing => ClientInventoryManagementSetting.SupportInwardProcessing;
		public bool ClientIsOutwardProcessing => ClientInventoryManagementSetting.SupportOutwardProcessing;
		public bool ClientIsInventoryManagementOn => JobDeclaration?.ClientIsInventoryManagementOn ?? false;

		public InventoryManagementSetting Warehouse2InventoryManagementSetting => Factory.GetValue(ref warehouse2InventoryManagementSettingCached, () => InventoryManagementSetting.New(Warehouse2?.Header));
		CachedProperty<InventoryManagementSetting> warehouse2InventoryManagementSettingCached;
		public bool Warehouse2IsBondedWarehousing => Warehouse2InventoryManagementSetting.SupportBondedWarehouse;
		public bool Warehouse2IsInwardProcessing => Warehouse2InventoryManagementSetting.SupportInwardProcessing;
		public bool Warehouse2IsOutwardProcessing => Warehouse2InventoryManagementSetting.SupportOutwardProcessing;
		public bool Warehouse2IsInventoryManagementOn => Warehouse2IsInventoryManagementOnCore;
		protected virtual bool Warehouse2IsInventoryManagementOnCore => Warehouse2IsBondedWarehousing || Warehouse2IsInwardProcessing || Warehouse2IsOutwardProcessing;

		public InventoryManagementSetting WarehouseInventoryManagementSetting => Factory.GetValue(ref warehouseInventoryManagementSettingCached, () => InventoryManagementSetting.New(Warehouse?.Header));
		CachedProperty<InventoryManagementSetting> warehouseInventoryManagementSettingCached;
		public bool WarehouseIsBondedWarehousing => WarehouseInventoryManagementSetting.SupportBondedWarehouse;
		public bool WarehouseIsInwardProcessing => WarehouseInventoryManagementSetting.SupportInwardProcessing;
		public bool WarehouseIsOutwardProcessing => WarehouseInventoryManagementSetting.SupportOutwardProcessing;
		public bool WarehouseIsInventoryManagementOn => WarehouseIsInventoryManagementOnCore;
		protected virtual bool WarehouseIsInventoryManagementOnCore => WarehouseIsBondedWarehousing || WarehouseIsInwardProcessing || WarehouseIsOutwardProcessing;

		public bool HasBothOutOfAndIntoRegimeProcedure
		{
			get
			{
				if (hasChangeOfOwnershipProcedureCached == null)
				{
					hasChangeOfOwnershipProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasBothOutOfAndIntoRegimeProcedure);
					});
				}
				return hasChangeOfOwnershipProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasChangeOfOwnershipProcedureCached;

		public bool IsChangeOfOwnershipWarehousing
		{
			get
			{
				if (isChangeOfOwnershipWarehousingCached == null)
				{
					isChangeOfOwnershipWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						return IsChangeOfOwnershipWarehousingEnabled && InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing);
					});
				}
				return isChangeOfOwnershipWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfOwnershipWarehousingCached;

		public bool IsChangeOfOwnershipWarehousingEnabled => IsChangeOfOwnershipWarehousingEnabledCore;

		protected virtual bool IsChangeOfOwnershipWarehousingEnabledCore => CEI_OH_Owner.IsValid;

		public bool IsChangeOfRegimeWarehousing
		{
			get
			{
				if (isChangeOfRegimeWarehousingCached == null)
				{
					isChangeOfRegimeWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						return IsChangeOfRegimeWarehousingEnabled && InvoiceLines.Any(x => x.IsChangeOfRegimeWarehousing);
					});
				}
				return isChangeOfRegimeWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isChangeOfRegimeWarehousingCached;

		public bool HasChangeOfRegimeWarehousing => Factory.GetValue(ref hasChangeOfRegimeWarehousingCached, () => ((WarehouseIsBondedWarehousing && Warehouse2IsInwardProcessing)
				|| (WarehouseIsInwardProcessing && Warehouse2IsBondedWarehousing))
			&& ClientIsBondedWarehousing
			&& ClientIsInwardProcessing); // TODO: Confirm whether we really need ClientIsBondedWarehousing & ClientIsInwardProcessing to be true
		CachedProperty<bool> hasChangeOfRegimeWarehousingCached;

		public bool IsChangeOfRegimeWarehousingEnabled => CEI_OH_Owner.IsEmpty && IsChangeOfRegimeWarehousingEnabledCore;

		protected virtual bool IsChangeOfRegimeWarehousingEnabledCore => false;

		public bool HasOutOfRegimeProcedure => Factory.GetValue(ref hasOutOfRegimeProcedureCached, () => InvoiceLines.Any(x => x.HasOutOfRegimeProcedure));
		CachedProperty<bool> hasOutOfRegimeProcedureCached;

		public bool HasIntoRegimeProcedure => Factory.GetValue(ref hasIntoRegimeProcedureCached, () => InvoiceLines.Any(x => x.HasIntoRegimeProcedure));
		CachedProperty<bool> hasIntoRegimeProcedureCached;

		public bool HasOutOfWarehouseProcedure => HasOutOfWarehouseProcedureCore;

		protected virtual bool HasOutOfWarehouseProcedureCore
		{
			get
			{
				if (hasOutOfWarehouseProcedureCached == null)
				{
					hasOutOfWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasOutOfWarehouseProcedure);
					});
				}
				return hasOutOfWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfWarehouseProcedureCached;

		public bool IsOutOfWarehouseWarehousing
		{
			get
			{
				if (isOutOfWarehouseWarehousingCached == null)
				{
					isOutOfWarehouseWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsOutOfWarehouseWarehousing);
					});
				}
				return isOutOfWarehouseWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfWarehouseWarehousingCached;

		public bool HasIntoWarehouseProcedure => HasIntoWarehouseProcedureCore;

		protected virtual bool HasIntoWarehouseProcedureCore
		{
			get
			{
				if (hasIntoWarehouseProcedureCached == null)
				{
					hasIntoWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasIntoWarehouseProcedure);
					});
				}
				return hasIntoWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoWarehouseProcedureCached;

		public bool HasIntoVATWarehouseProcedure => HasIntoVATWarehouseProcedureCore;

		protected virtual bool HasIntoVATWarehouseProcedureCore
		{
			get
			{
				if (hasIntoVATWarehouseProcedureCached == null)
				{
					hasIntoVATWarehouseProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasIntoVATWarehouseProcedure);
					});
				}
				return hasIntoVATWarehouseProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoVATWarehouseProcedureCached;

		public bool IsIntoWarehouseWarehousing
		{
			get
			{
				if (isIntoWarehouseWarehousingCached == null)
				{
					isIntoWarehouseWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsIntoWarehouseWarehousing);
					});
				}
				return isIntoWarehouseWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isIntoWarehouseWarehousingCached;

		public bool IsNonWarehousing
		{
			get
			{
				if (isNonWarehousingCached == null)
				{
					isNonWarehousingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => !IsWarehousing(x.CusProcedure));
					});
				}
				return isNonWarehousingCached.Value;
			}
		}
		CachedProperty<bool> isNonWarehousingCached;

		bool IsWarehousing(RefCusProcedure procedure)
		{
			var result = false;
			if (procedure != null)
			{
				var decider = GetNewProcedureRegimeDecider();
				result = decider.IsIntoRegime(procedure) || decider.IsOutOfRegime(procedure);
			}
			return result;
		}

		public bool IsInventorySelectionEnabled
		{
			get
			{
				if (isInventorySelectionEnabledCached == null)
				{
					isInventorySelectionEnabledCached = new CachedProperty<bool>(Factory, () =>
					{
						return (JobDeclaration?.IsWHSUniversalXMLActive ?? false) && IsInventorySelectionEnabledCore();
					});
				}
				return isInventorySelectionEnabledCached.Value;
			}
		}
		CachedProperty<bool> isInventorySelectionEnabledCached;

		protected virtual bool IsInventorySelectionEnabledCore()
		{
			return HasOutOfRegimeProcedure;
		}

		#region For CusEntryHeader IWarehouseIntegrationSupporter Members

		public bool HasIntoTemporaryImportProcedure => HasIntoTemporaryImportProcedureCore;

		protected virtual bool HasIntoTemporaryImportProcedureCore
		{
			get
			{
				if (hasIntoTemporaryImportProcedureCached == null)
				{
					hasIntoTemporaryImportProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => IsIntoTemporaryImport(x.CusProcedure));
					});
				}
				return hasIntoTemporaryImportProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoTemporaryImportProcedureCached;
		bool IsIntoTemporaryImport(RefCusProcedure procedure) => procedure != null && procedure.IsIntoTemporaryImport();

		public bool HasOutOfTemporaryImportProcedure => HasOutOfTemporaryImportProcedureCore;

		protected virtual bool HasOutOfTemporaryImportProcedureCore
		{
			get
			{
				if (hasOutOfTemporaryImportProcedureCached == null)
				{
					hasOutOfTemporaryImportProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => IsOutOfTemporaryImport(x.CusProcedure));
					});
				}
				return hasOutOfTemporaryImportProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfTemporaryImportProcedureCached;
		bool IsOutOfTemporaryImport(RefCusProcedure procedure) => procedure != null && procedure.IsOutOfTemporaryImport();

		public bool HasIntoTemporaryExportProcedure => HasIntoTemporaryExportProcedureCore;

		protected virtual bool HasIntoTemporaryExportProcedureCore
		{
			get
			{
				if (hasIntoTemporaryExportProcedureCached == null)
				{
					hasIntoTemporaryExportProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => IsIntoTemporaryExport(x.CusProcedure));
					});
				}
				return hasIntoTemporaryExportProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoTemporaryExportProcedureCached;
		bool IsIntoTemporaryExport(RefCusProcedure procedure) => procedure != null && procedure.IsIntoTemporaryExport();

		public bool HasOutOfTemporaryExportProcedure
		{
			get
			{
				if (hasOutOfTemporaryExportProcedureCached == null)
				{
					hasOutOfTemporaryExportProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => IsOutOfTemporaryExport(x.CusProcedure));
					});
				}
				return hasOutOfTemporaryExportProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfTemporaryExportProcedureCached;
		bool IsOutOfTemporaryExport(RefCusProcedure procedure) => procedure != null && procedure.IsOutOfTemporaryExport();

		public bool HasIntoInwardProcessingProcedure => HasIntoInwardProcessingProcedureCore;

		protected virtual bool HasIntoInwardProcessingProcedureCore
		{
			get
			{
				if (hasIntoInwardProcessingProcedureCached == null)
				{
					hasIntoInwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasIntoInwardProcessingProcedure);
					});
				}
				return hasIntoInwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoInwardProcessingProcedureCached;

		public bool IsIntoInwardProcessing
		{
			get
			{
				if (isIntoInwardProcessingCached == null)
				{
					isIntoInwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsIntoInwardProcessing);
					});
				}
				return isIntoInwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isIntoInwardProcessingCached;

		public bool HasOutOfInwardProcessingProcedure => HasOutOfInwardProcessingProcedureCore;

		protected virtual bool HasOutOfInwardProcessingProcedureCore
		{
			get
			{
				if (hasOutOfInwardProcessingProcedureCached == null)
				{
					hasOutOfInwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasOutOfInwardProcessingProcedure);
					});
				}
				return hasOutOfInwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfInwardProcessingProcedureCached;

		public bool IsOutOfInwardProcessing
		{
			get
			{
				if (isOutOfInwardProcessingCached == null)
				{
					isOutOfInwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsOutOfInwardProcessing);
					});
				}
				return isOutOfInwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfInwardProcessingCached;

		public bool HasIntoOutwardProcessingProcedure => HasIntoOutwardProcessingProcedureCore;

		protected virtual bool HasIntoOutwardProcessingProcedureCore
		{
			get
			{
				if (hasIntoOutwardProcessingProcedureCached == null)
				{
					hasIntoOutwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasIntoOutwardProcessingProcedure);
					});
				}
				return hasIntoOutwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasIntoOutwardProcessingProcedureCached;

		public bool IsIntoOutwardProcessing
		{
			get
			{
				if (isIntoOutwardProcessingCached == null)
				{
					isIntoOutwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsIntoOutwardProcessing);
					});
				}
				return isIntoOutwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isIntoOutwardProcessingCached;

		public bool HasOutOfOutwardProcessingProcedure => HasOutOfOutwardProcessingProcedureCore;

		protected virtual bool HasOutOfOutwardProcessingProcedureCore
		{
			get
			{
				if (hasOutOfOutwardProcessingProcedureCached == null)
				{
					hasOutOfOutwardProcessingProcedureCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.HasOutOfOutwardProcessingProcedure);
					});
				}
				return hasOutOfOutwardProcessingProcedureCached.Value;
			}
		}
		CachedProperty<bool> hasOutOfOutwardProcessingProcedureCached;

		public bool IsOutOfOutwardProcessing
		{
			get
			{
				if (isOutOfOutwardProcessingCached == null)
				{
					isOutOfOutwardProcessingCached = new CachedProperty<bool>(Factory, () =>
					{
						return InvoiceLines.Any(x => x.IsOutOfOutwardProcessing);
					});
				}
				return isOutOfOutwardProcessingCached.Value;
			}
		}
		CachedProperty<bool> isOutOfOutwardProcessingCached;
		#endregion

		public bool IsIntoRegime => IsIntoRegimeCore;
		protected virtual bool IsIntoRegimeCore => IsIntoWarehouseWarehousing || IsIntoInwardProcessing || IsIntoOutwardProcessing;

		public bool IsOutOfRegime => IsOutOfWarehouseWarehousing || IsOutOfInwardProcessing || IsOutOfOutwardProcessing;

		protected virtual ProcedureRegimeDecider GetNewProcedureRegimeDecider() => new ProcedureRegimeDecider();

		public ZString UpdateOutwardLinesWithInventoryDetails() => CreateNewDeclarationEntryInstructionInventorySelectionHeader().UpdateOutwardLinesWithInventoryDetails(InvoiceLines);

		protected virtual DeclarationInventorySelectionHeader CreateNewDeclarationEntryInstructionInventorySelectionHeader() => new DeclarationEntryInstructionInventorySelectionHeader(this);

		public bool IsWarehouseRequiredForWarehouseValidation
		{
			get { return HasOutOfRegimeProcedure; }
		}

		public bool IsWarehouse2RequiredForWarehouseValidation
		{
			get
			{
				if (HasIntoRegimeProcedure && JobDeclaration.Importer?.CompanyData is OrgCompanyData importerCompanyData)
				{
					return (HasIntoInwardProcessingProcedure && importerCompanyData.OB_CusInventoryForInwardProcessing)
						|| (HasIntoOutwardProcessingProcedure && importerCompanyData.OB_CusInventoryForOutwardProcessing)
						|| (HasIntoWarehouseProcedure && importerCompanyData.OB_IMUsedBondedWhs);
				}
				return false;
			}
		}

		public ZBool AllowedToBeLinkedToMultipleEntryHeaders => AllowedToBeLinkedToMultipleEntryHeadersCore;

		protected virtual ZBool AllowedToBeLinkedToMultipleEntryHeadersCore => !(JobDeclaration?.AreMultipleEntryInstructionsAllowed ?? true);

		public virtual ZBool AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError => false;

		#region JobDocAddress

		[ChildEditable(false)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return GetCanOverrideCheckpointCore(docAddress);
		}

		protected virtual SecurityCheckpoint GetCanOverrideCheckpointCore(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZString IDocAddresses.HumanReadableName => HumanReadableNameCore;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return GetJobDocAddressValidationCore(addressToValidate);
		}

		protected virtual JobDocAddressValidation GetJobDocAddressValidationCore(JobDocAddress addressToValidate)
		{
			return new CusEntryInstructionJobDocAddressValidation(addressToValidate, this);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => GetSupportedAddressTypesCore();

		protected virtual DocAddressType[] GetSupportedAddressTypesCore()
		{
			return Array.Empty<DocAddressType>();
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirementCore(addressType);
		}

		protected virtual JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			DocAddressChangedCore(docAddress);
		}

		protected virtual void DocAddressChangedCore(JobDocAddress docAddress) { }

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
			OnBeforeDocAddressDeletedCore(docAddress);
		}

		protected virtual void OnBeforeDocAddressDeletedCore(JobDocAddress docAddress) { }

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
			AnyAddressFieldBeforeChangeCore(docAddress);
		}

		protected virtual void AnyAddressFieldBeforeChangeCore(JobDocAddress docAddress) { }

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			OrgAddressBeforeChangeCore(docAddress);
		}

		protected virtual void OrgAddressBeforeChangeCore(JobDocAddress docAddress) { }

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			OrgHeaderAfterChangeCore(docAddress);
		}

		protected virtual void OrgHeaderAfterChangeCore(JobDocAddress docAddress) { }

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return CanDeleteAddressCore(docAddress);
		}

		protected virtual bool CanDeleteAddressCore(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return OrgHeaderCollectionCore(addressType);
		}

		protected virtual OrgHeaderCollection OrgHeaderCollectionCore(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion

		#endregion

		#region ICustomsCustomLabelsConfigOrgProvider Members

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute1 => PartAttribute1Core;
		protected virtual string PartAttribute1Core
		{
			get
			{
				ErrorReporter.ReportOnce(GetType().FullName + ".PartAttribute1Core is missing implementation");
				return string.Empty;
			}
		}

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute2 => PartAttribute2Core;
		protected virtual string PartAttribute2Core
		{
			get
			{
				ErrorReporter.ReportOnce(GetType().FullName + ".PartAttribute2Core is missing implementation");
				return string.Empty;
			}
		}

		ZString ICustomsCustomLabelsConfigOrgProvider.PartAttribute3 => PartAttribute3Core;
		protected virtual string PartAttribute3Core
		{
			get
			{
				ErrorReporter.ReportOnce(GetType().FullName + ".PartAttribute3Core is missing implementation");
				return string.Empty;
			}
		}

		ZString ICustomsCustomLabelsConfigOrgProvider.SerialNumber => SerialNumberCore;
		protected virtual string SerialNumberCore
		{
			get
			{
				ErrorReporter.ReportOnce(GetType().FullName + ".SerialNumberCore is missing implementation");
				return string.Empty;
			}
		}

		#region ICustomLabelsConfigOrgProvider Members
		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				CEI_OH_OwnerInfo.ValueChanged -= value;
				CEI_OH_OwnerInfo.ValueChanged += value;
			}
			remove { CEI_OH_OwnerInfo.ValueChanged -= value; }
		}

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg => Owner;

		#region Stuff for handling non-persistent decs (e.g. those temporarily set on a standalone invoice)
		public override bool IsSavedByFactory
		{
			get { return fIsPersistent && base.IsSavedByFactory; }
		}

		public bool IsPersistent => fIsPersistent;
		bool fIsPersistent = true;

		public virtual void MakeNonPersistent()
		{
			fIsPersistent = false;
		}
		#endregion

		#endregion

		#endregion

		#region Implementation

		protected virtual void UpdateAndRefreshWhenStyleIsChanged(BaseJobComInvoiceLine invoiceLine, ZString oldValue, ZString newValue)
		{
			invoiceLine.JI_CEIInfo.RefreshBinding();
		}

		protected virtual void UpdateAndRefreshWhenDescriptionIsChanged(BaseJobComInvoiceLine invoiceLine, ZString oldValue, ZString newValue)
		{
			invoiceLine.JI_CEIInfo.RefreshBinding();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusEntryInstruction)base.CloneInternal(args);
			result.ResetValuesAfterClone();
			return result;
		}

		public void ResetValuesAfterClone()
		{
			ResetValuesAfterCloneCore();
		}

		protected virtual void ResetValuesAfterCloneCore()
		{
		}

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CEI_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CEI_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.CN.ICusCNEntryInstruction>(), CusCNEntryInstructionSchema.CNE_CEI);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.EU.ICusAuthorizationUsage>(), CusAuthorizationUsageSchema.AGC_ParentID);
			}
		}

		#endregion

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();

		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();

		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => RegisterListChangedCalledRefreshBinding(element);

		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(System.ComponentModel.IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (JobDeclaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region ICusGoodsLocationTypeSupporter

		Type ICusGoodsLocationTypeSupporter.GoodsLocationType => GoodsLocationTypeCore;

		protected virtual Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		#endregion

		#region IDataModelSupporter

		public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromParentIfNeeded(JobDeclaration);

		ZString IDataModelSupporter.DataModel { get => CEI_DataModel; set => CEI_DataModel = value; }

		#endregion

		#region Containers Pivot

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public CusContainerEntryInstructionPiovtCollection ContainersPivot
		{
			get
			{
				if (fContainersPivot == null)
				{
					fContainersPivot = new CusContainerEntryInstructionPiovtCollection(this);
					if (JobDeclaration?.SupportContainerEntryInstructionPivot ?? false)
					{
						fContainersPivot.Load();
						fContainersPivot.IsManagedForDataRefresh = true;
						RegisterEditableChildObject(fContainersPivot);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)fContainersPivot).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						fContainersPivot.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return fContainersPivot;
			}
		}
		CusContainerEntryInstructionPiovtCollection fContainersPivot;

		public ICusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction> ContainersForInstructionForBindingOnly => containersForInstruction ??= GetNewCusContainerOnEntryInstructionCollection();
		ICusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction> containersForInstruction;

		protected virtual ICusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction> GetNewCusContainerOnEntryInstructionCollection()
		{
			return new CusContainerOnEntryInstructionCollection<CusContainerOnEntryInstruction>(this);
		}

		internal void ToggleLinkageWithContainer(BaseCusContainer container, bool value)
		{
			if (container != null)
			{
				if (value)
				{
					ContainersPivot.AddPivotFor(container);
				}
				else
				{
					ContainersPivot.DeletePivotFor(container);
				}
			}
		}
		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CEI_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		}
#endif
	}
}
