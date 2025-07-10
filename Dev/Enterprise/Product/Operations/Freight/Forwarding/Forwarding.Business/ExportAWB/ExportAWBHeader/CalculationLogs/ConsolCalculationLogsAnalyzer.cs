using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ConsolCalculationLogsAnalyzer : CalculationLogsAnalyzer
	{
		public ConsolCalculationLogsAnalyzer(ConsolExportAWBHeader exportAWBHeader)
			: base(exportAWBHeader)
		{
			Header = exportAWBHeader;
		}

		readonly ConsolExportAWBHeader Header;

		#region Logs Wrapper

		protected override IEnumerable<BusinessObject> GetAllBusinessObjectsWithCalculationLogs()
		{
			if (Header.Consol == null)
			{
				return null;
			}

			var logsWrapperFromConsol = CalculationLogsLoader.Load(Header.Consol);
			if (logsWrapperFromConsol != null && !logsWrapperFromConsol.IsDisabled)
			{
				return new List<BusinessObject>() { Header.Consol };
			}
			else if (Header.Consol.IsGateway())
			{
				return new List<BusinessObject>() { Header.Consol.GetConsolFreightCharge() };
			}
			else
			{
				return Header.Consol.GetConsolFreightCosts();
			}
		}

		#endregion

		#region Disable Logs

		protected override void DisableLogsCore()
		{
			if (Header.Consol != null)
			{
				CalculationLogsLoader.Disable(Header.Consol);

				var consolFreightCosts = Header.Consol.GetConsolFreightCosts();
				foreach (var consolFreightCost in consolFreightCosts)
				{
					if (consolFreightCost != null)
					{
						CalculationLogsLoader.Disable(consolFreightCost);
					}
				}
			}
		}

		#endregion

		#region Overrides

		protected override ZDecimal GetAmountInAWBCurrency(ZDecimal originalAmount, ZString originalCurrencyCode)
		{
			ZDecimal? result = null;

			if (originalCurrencyCode != Header.EH_Currency)
			{
				result = Header.Consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(originalAmount, originalCurrencyCode, Header.EH_Currency);
			}

			return result ?? originalAmount;
		}

		protected override ZInt GetNumberOfPieces()
		{
			return Header.Consol != null ? (ZInt)Header.Consol.JK_TotalShipmentQuantity : base.GetNumberOfPieces();
		}

		protected override ZInt GetNumberOfLoosePieces()
		{
			return Header.Consol.UnAllocatedPackLines.Cast<PackLine>().Sum(packline => packline.JL_PackageCount);
		}

		#endregion
	}
}
