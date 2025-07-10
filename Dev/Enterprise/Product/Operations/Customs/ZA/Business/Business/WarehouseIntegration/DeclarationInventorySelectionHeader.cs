using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Business
{
	public class DeclarationInventorySelectionHeader : Customs.Business.DeclarationInventorySelectionHeader
	{
		public DeclarationInventorySelectionHeader(JobDeclaration declaration)
			: this(declaration, () => declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().OrderBy(x => Invariant($"{x.CEI_Style}|{x.CEI_Description}")).FirstOrDefault())
		{
		}

		public DeclarationInventorySelectionHeader(CusEntryInstruction entryInstruction)
			: this(entryInstruction.JobDeclaration, () => entryInstruction)
		{
		}

		DeclarationInventorySelectionHeader(JobDeclaration declaration, Func<CusEntryInstruction> getFirstEntryInstruction)
			: base(declaration)
		{
			if (getFirstEntryInstruction != null)
			{
				entryInstruction = getFirstEntryInstruction();
			}
		}

		readonly CusEntryInstruction entryInstruction;

		protected override BaseJobComInvoiceHeader GetFirstOrCreateNewInvoiceHeader(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var invoice = entryInstruction?.InvoiceLines?.FirstOrDefault()?.InvoiceHeader;
			return invoice ?? base.GetFirstOrCreateNewInvoiceHeader(whsBondedWarehouseAttribute);
		}

		protected override OrgAddress GetWarehouseAddress()
		{
			return entryInstruction?.Warehouse;
		}

		protected override void UpdateParentData()
		{
			base.UpdateParentData();
			if (entryInstruction != null && entryInstruction.CEI_OA_Warehouse.IsEmpty)
			{
				var warehouseAddress = SelectionLines.GetFirstWarehouseAddress();
				if (warehouseAddress != null)
				{
					entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
				}
			}
		}

		protected override ZString InwardEntryNumberHumanReadable => Res.GetString("43B4DC6A-4409-431E-B631-8111ABB7C08E", "MRN");

		protected override ZString InwardEntryLineNumberHumanReadable => Res.GetString("F0AE12A2-1738-4D1F-9CE1-D6D858D7F12E", "MRL");

		protected override ZBool SupportsVINLookup => ZBool.True;

		protected override ZString GetVIN(BaseJobComInvoiceLine invoiceLine) => ((JobComInvoiceLine)invoiceLine).JI_VIN;

		protected override ZString GetVINAddInfoExpression(BaseJobComInvoiceLine invoiceLine)
		{
			return new ZString(Invariant($"{Constants.VIN}={GetVIN(invoiceLine)}"));
		}

		protected override ZString GetVIN(IWhsBondedWarehouseAttribute attr)
		{
			var result = ZString.Empty;
			if (attr != null)
			{
				var dict = PopulateAddInfoData(attr);
				result = dict.TryGetValue(Constants.VIN, out var vin) ? vin : ZString.Empty;
			}

			return result;
		}

		protected override void UpdateOutwardLineWithInventoryDetailCore(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			base.UpdateOutwardLineWithInventoryDetailCore(invoiceLine, inventory, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
			var addInfos = PopulateAddInfoData(whsBondedWarehouseAttribute);
			if (whsBondedWarehouseAttribute != null)
			{
				var primaryPreference = whsBondedWarehouseAttribute.WB_PrimaryPreference;
				if (!primaryPreference.IsEmpty && invoiceLine.JI_PrimaryPreference != primaryPreference)
				{
					invoiceLine.JI_PrimaryPreference = primaryPreference;
				}

				var previousEntryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
				if (!previousEntryNumber.IsEmpty && invoiceLine.JI_PreviousEntryNumber != previousEntryNumber)
				{
					invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				}

				var previousEntryLineNumber = whsBondedWarehouseAttribute.WB_EntryLineNo;
				if (!previousEntryLineNumber.IsEmpty && invoiceLine.JI_PreviousEntryLineNumber != previousEntryLineNumber)
				{
					invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
				}

				PopulateCustomsThirdQuantityAndUnitIfNeeded(invoiceLine, addInfos, ratio);
			}

			PopulateCountrySpecificInvoiceLineData((JobComInvoiceLine)invoiceLine, whsBondedWarehouseAttribute, addInfos);
		}

		protected override void SetHeaderData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var zaInvoiceLine = (JobComInvoiceLine)invoiceLine;
			if (entryInstruction != null)
			{
				zaInvoiceLine.JI_CEI = entryInstruction.PK;
			}
		}

		protected override void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			base.PopulateCountrySpecificInvoiceLineData(invoiceLine, inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, invoiceQuantity, ratio);
			var zaInvoiceLine = (JobComInvoiceLine)invoiceLine;
			PopulateCountrySpecificInvoiceLineData(zaInvoiceLine, whsBondedWarehouseAttribute, addInfos);
		}

		protected override void PopulatePrimaryPreference(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
		}

		static void PopulateCountrySpecificInvoiceLineData(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos)
		{
			if (whsBondedWarehouseAttribute != null)
			{
				if (addInfos.TryGetValue(BondedWarehousingHelper.Constants.OriginalProcedureCode, out var ppc))
				{
					addInfos.Remove(BondedWarehousingHelper.Constants.OriginalProcedureCode);
				}
				if (addInfos.Count > 0)
				{
					var addInfo = invoiceLine.GetAddInfoString();
					var existingAddInfos = AddInfoParser.CreateDictionaryWithAddInfoString(addInfo);
					foreach (var pair in addInfos)
					{
						var invoiceLinePropertyName = $"{ZAJobComInvoiceLineSchema.Constants.Prefix}_{pair.Key}";
						if (JobComInvoiceLine.IsMigratedAddInfoProperties(invoiceLinePropertyName))
						{
							invoiceLine[invoiceLinePropertyName] = pair.Value;
						}
						else
						{
							existingAddInfos[pair.Key] = pair.Value;
						}
					}
					invoiceLine.JI_AddInfo = AddInfoParser.Serialise(existingAddInfos);
				}
				if (!ppc.IsEmpty)
				{
					invoiceLine.JI_Procedure = invoiceLine.JI_Procedure.Left(2).PadRight(2) + ppc;
				}
				else
				{
					var wbInwardProcedure = whsBondedWarehouseAttribute.WB_InwardProcedure;
					var instruction = invoiceLine.EntryInstruction;
					if (!wbInwardProcedure.IsEmpty && instruction != null && !instruction.CEI_Style.IsEmpty)
					{
						var newProcedure = instruction.CEI_Style + wbInwardProcedure.Left(2);
						if (((CodeDescriptionPairList)invoiceLine.Lookups.Procedures).CodesAsString.Contains(newProcedure))
						{
							invoiceLine.JI_Procedure = newProcedure;
						}
					}
				}
			}
		}

		static class Constants
		{
			public const string VIN = "VIN";
		}
	}
}
