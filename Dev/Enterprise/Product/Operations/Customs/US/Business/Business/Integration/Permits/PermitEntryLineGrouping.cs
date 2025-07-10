
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.US.Business
{
	public interface IEntryLineGroup
	{
		ZString TariffNumber { get; }
		ZString FirmRegNo { get; }
		ZString ProductCode { get; }
		ZString CountryOfOrigin { get; }
		ZString ZoneStatus { get; }

		ZString EntryNumber { get; }
		ZDecimal FTZCustomsValue { get; }
		ZDecimal FTZCustomsQty { get; }
		ZGuid ManufacturerAddress { get; }
		ZString UnitOfMeasure { get; }
	}

	class EntryLineGroup : IEntryLineGroup
	{
		public ZString TariffNumber { get; set; }
		public ZString FirmRegNo { get; set; }
		public ZString ProductCode { get; set; }
		public ZString CountryOfOrigin { get; set; }
		public ZString ZoneStatus { get; set; }

		public ZString EntryNumber { get; set; }
		public ZDecimal FTZCustomsValue { get; set; }
		public ZDecimal FTZCustomsQty { get; set; }
		public ZGuid ManufacturerAddress { get; set; }
		public ZString UnitOfMeasure { get; set; }
	}

	public class PermitEntryLineGrouping
	{
		public PermitEntryLineGrouping(JobDeclaration declaration)
		{
			this.declaration = declaration;
			invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.US_ZoneStatus != ZoneStatusList.Codes.Domestic);
			outwardEntryNumber = GetOutwardEntryNumber(declaration);
		}
		readonly ZString outwardEntryNumber;
		readonly IEnumerable<JobComInvoiceLine> invoiceLines;
		readonly JobDeclaration declaration;

		public static ZString GetOutwardEntryNumber(JobDeclaration declaration)
		{
			var result = ZString.Empty;
			if (declaration != null)
			{
				result = ZString.Format("{0}-{1}", declaration.EntryFilerCode, declaration.ImportEntryNumber);
			}
			return result;
		}

		virtual public bool IsDetailedTrackingEnabled   // virtual for Mock
		{
			get
			{
				var warehouseAddress = declaration.WarehouseAddress;
				return warehouseAddress != null && ObjectFactory.Get<IFTZWarehouseDataProvider>().IsFTZWarehouseDetailedTrackingEnabled(warehouseAddress);
			}
		}

		public IEnumerable<IEntryLineGroup> GetEntryLineGroups()
		{
			if (IsDetailedTrackingEnabled)
			{
				return GetDetailedTrackingLineGroups();
			}
			else
			{
				return GetSimpleTrackingLineGroups();
			}
		}

		IEnumerable<IEntryLineGroup> GetDetailedTrackingLineGroups()
		{
			var singleTariffLines = GetSingleTariffLinesForDetailedTracking();
			var multiTariffLines = GetMultiTariffLinesForDetailedTracking();

			return singleTariffLines.Concat(multiTariffLines);
		}

		IEnumerable<IEntryLineGroup> GetSimpleTrackingLineGroups()
		{
			var singleTariffLines = GetSingleTariffLinesForSimpleTracking();
			var multiTariffLines = GetMultiTariffLinesForSimpleTracking();

			return singleTariffLines.Concat(multiTariffLines);
		}

		public IEnumerable<IEntryLineGroup> GetSingleTariffLinesForDetailedTracking()
		{
			var singleTariffLines = invoiceLines
					.Where(x => IsSingleTariffLine(x))
					.GroupBy(x => new { x.JI_Tariff, x.US_UC_NKCountryOfOrigin, x.JI_OA_ManufacturerAddress, x.US_ZoneStatus, x.US_ManifestUQ })
					.Select(x => new EntryLineGroup
					{
						TariffNumber = x.Key.JI_Tariff,
						ProductCode = ZString.Empty,
						CountryOfOrigin = x.Key.US_UC_NKCountryOfOrigin,
						ZoneStatus = x.Key.US_ZoneStatus,
						EntryNumber = outwardEntryNumber,
						FTZCustomsValue = x.Sum(value => value.US_CustomsValue),
						FTZCustomsQty = x.Sum(qty => qty.US_ManifestQty),
						ManufacturerAddress = x.Key.JI_OA_ManufacturerAddress,
						UnitOfMeasure = x.Key.US_ManifestUQ,
						FirmRegNo = GetWarehouseFirmsCode()
					});
			return singleTariffLines;
		}

		public IEnumerable<IEntryLineGroup> GetMultiTariffLinesForDetailedTracking()
		{
			var multiTariffLines = invoiceLines
					.Where(x => IsMultiTariffLine(x))
					.GroupBy(x => new { x.JI_Tariff, x.US_UC_NKCountryOfOrigin, x.JI_OA_ManufacturerAddress, x.US_ZoneStatus, x.US_ManifestUQ, x.JI_PartNo })
					.Select(x => new EntryLineGroup
					{
						TariffNumber = x.Key.JI_Tariff,
						UnitOfMeasure = x.Key.US_ManifestUQ,
						ProductCode = x.Key.JI_PartNo,
						CountryOfOrigin = x.Key.US_UC_NKCountryOfOrigin,
						ZoneStatus = x.Key.US_ZoneStatus,
						EntryNumber = outwardEntryNumber,
						FTZCustomsValue = x.Sum(value => value.US_CustomsValue),
						FTZCustomsQty = x.Sum(qty => qty.US_ManifestQty),
						ManufacturerAddress = x.Key.JI_OA_ManufacturerAddress,
						FirmRegNo = GetWarehouseFirmsCode()
					});
			return multiTariffLines;
		}

		public IEnumerable<IEntryLineGroup> GetSingleTariffLinesForSimpleTracking()
		{
			var singleTariffLines = invoiceLines
					.Where(x => IsSingleTariffLine(x))
					.GroupBy(x => new { x.JI_Tariff, x.US_ManifestUQ })
					.Select(x => new EntryLineGroup
					{
						TariffNumber = x.Key.JI_Tariff,
						UnitOfMeasure = x.Key.US_ManifestUQ,
						ProductCode = ZString.Empty,
						CountryOfOrigin = ZString.Empty,
						ZoneStatus = ZString.Empty,
						ManufacturerAddress = ZGuid.Empty,
						EntryNumber = outwardEntryNumber,
						FTZCustomsValue = x.Sum(value => value.US_CustomsValue),
						FTZCustomsQty = x.Sum(qty => qty.US_ManifestQty),
						FirmRegNo = GetWarehouseFirmsCode()
					});
			return singleTariffLines;
		}

		public IEnumerable<IEntryLineGroup> GetMultiTariffLinesForSimpleTracking()
		{
			var multiTariffLines = invoiceLines
					.Where(x => IsMultiTariffLine(x))
					.GroupBy(x => new { x.JI_Tariff, x.US_ManifestUQ, x.JI_PartNo })
					.Select(x => new EntryLineGroup
					{
						TariffNumber = x.Key.JI_Tariff,
						UnitOfMeasure = x.Key.US_ManifestUQ,
						ProductCode = x.Key.JI_PartNo,
						CountryOfOrigin = ZString.Empty,
						ZoneStatus = ZString.Empty,
						ManufacturerAddress = ZGuid.Empty,
						EntryNumber = outwardEntryNumber,
						FTZCustomsValue = x.Sum(value => value.US_CustomsValue),
						FTZCustomsQty = x.Sum(qty => qty.US_ManifestQty),
						FirmRegNo = GetWarehouseFirmsCode()
					});
			return multiTariffLines;
		}

		ZString GetWarehouseFirmsCode()
		{
			return declaration.WarehouseAddress?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
		}

		static bool IsSingleTariffLine(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.HasEmptySupTariff && !invoiceLine.IsParentLine && !invoiceLine.IsChildLine;
		}

		static bool IsMultiTariffLine(JobComInvoiceLine invoiceLine)
		{
			return !invoiceLine.HasEmptySupTariff && !invoiceLine.IsChildLine || invoiceLine.IsParentLine;
		}
	}
}
