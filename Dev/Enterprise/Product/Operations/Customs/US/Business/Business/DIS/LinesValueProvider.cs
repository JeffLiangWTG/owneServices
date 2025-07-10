using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.Business.DIS
{
	class LinesValueProvider
	{
		public LinesValueProvider(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			this.invoiceLines = invoiceLines;
		}

		readonly IEnumerable<JobComInvoiceLine> invoiceLines;

		public IEnumerable<IDISInvoiceLineDefault> Lines
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in invoiceLines)
				{
					var containers = from CusContainerInvoiceLinePivot pivot in invoiceLine.ContainersPivot
									 where pivot.Container != null
									 select pivot.Container;

					if (invoiceLine.HasVNEDetails)
					{
						foreach (Vehicle vehicle in invoiceLine.VehicleLines)
						{
							foreach (VehicleDetails vehicleDetailLine in vehicle.VehicleAndEngineDetails)
							{
								foreach (var wrapper in GetLineWrappers(invoiceLine, containers, vehicleDetailLine))
								{
									yield return wrapper;
								}
							}
						}
					}
					else
					{
						foreach (var wrapper in GetLineWrappers(invoiceLine, containers))
						{
							yield return wrapper;
						}
					}
				}
			}
		}

		IEnumerable<LineWrapper> GetLineWrappers(JobComInvoiceLine invoiceLine, IEnumerable<CusContainer> containers, VehicleDetails vehicleDetailLine = null)
		{
			if (containers.Any())
			{
				foreach (CusContainer container in containers)
				{
					var result = GetLineWrapper(invoiceLine, vehicleDetailLine);
					result.ContainerNumber = container.CO_ContainerNumber;
					result.SealNumber = container.CO_Seal;

					yield return result;
				}
			}
			else
			{
				yield return GetLineWrapper(invoiceLine, vehicleDetailLine);
			}
		}

		LineWrapper GetLineWrapper(JobComInvoiceLine invoiceLine, VehicleDetails vehicleDetailLine = null)
		{
			var result = LineWrapper.GetLineWrapper(invoiceLine);

			if (vehicleDetailLine != null)
			{
				result.VehicleData = new VehicleAndEngineDataWrapper(vehicleDetailLine);
			}

			return result;
		}

		class LineWrapper : IDISInvoiceLineDefault
		{
			public static LineWrapper GetLineWrapper(JobComInvoiceLine invoiceLine)
			{
				return new LineWrapper()
				{
					InvoiceLineNumber = invoiceLine.JI_LineNo,
					CommodityDescription = invoiceLine.JI_Description,
					CountryOfOrigin = invoiceLine.US_UC_NKCountryOfOrigin,
					EntryLineNumber = invoiceLine.CusEntryLine != null ? invoiceLine.CusEntryLine.CL_LineNumber.ToZInt() : ZInt.Zero,
					HTSNumber = invoiceLine.JI_Tariff,
					PortOfLoading = invoiceLine.US_SchDLoading,
					PortOfEntry = invoiceLine.Declaration.US_SchDEntry,
					PortOfUnlading = invoiceLine.Declaration.US_SchDArrival,
					ArrivalDate = invoiceLine.Declaration.JE_DateOfArrival,
					TradeParties = new TradePartyValueProvider(invoiceLine).TradeParties
				};
			}

			public IDISCommodityLine CommodityDetails
			{
				get { return this; }
			}

			public ZInt InvoiceLineNumber
			{
				get;
				internal set;
			}

			public ZString CommodityDescription
			{
				get;
				internal set;
			}

			public ZString ContainerNumber
			{
				get;
				internal set;
			}

			public ZString CountryOfOrigin
			{
				get;
				internal set;
			}

			public ZInt EntryLineNumber
			{
				get;
				internal set;
			}

			public ZString HTSNumber
			{
				get;
				internal set;
			}

			public ZString PortOfLoading
			{
				get;
				internal set;
			}

			public ZString SealNumber
			{
				get;
				internal set;
			}

			public ZDateTime ArrivalDate
			{
				get;
				internal set;
			}

			public ZString PortOfEntry
			{
				get;
				internal set;
			}

			public ZString PortOfUnlading
			{
				get;
				internal set;
			}

			public IEnumerable<IDISTradeParty> TradeParties
			{
				get { return tradeParties ?? Enumerable.Empty<IDISTradeParty>(); }
				internal set { tradeParties = value; }
			}
			IEnumerable<IDISTradeParty> tradeParties;

			public IDISVehicleAndEngineData VehicleData
			{
				get;
				internal set;
			}
		}
	}
}
