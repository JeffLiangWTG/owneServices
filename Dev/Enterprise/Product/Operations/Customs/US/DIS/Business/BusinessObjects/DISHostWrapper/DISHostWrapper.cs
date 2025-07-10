using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISHostWrapper : DISHostWrapperBase<DISDocument>
	{
		public DISHostWrapper(IUSDISHost disHost)
			: base(disHost)
		{
			this.DefaultValues = DISHost.ValueProvider;
		}

		public new IUSDISHost DISHost => base.DISHost as IUSDISHost;
		internal readonly IUSDISDefaultValues DefaultValues;

		public override DISDocumentCollectionBase<DISDocument> DISDocuments
		{
			get
			{
				if (disDocuments == null)
				{
					disDocuments = new DISDocumentCollection(this);
					RegisterEditableChildObject(disDocuments);
				}
				return disDocuments;
			}
		}
		DISDocumentCollection disDocuments;

		public bool IsExport
		{
			get { return DefaultValues != null && DefaultValues.IsExport; }
		}

		public DISHostWrapperLookups Lookups
		{
			get { return new DISHostWrapperLookups(this); }
		}

		internal IeDoc GetEDoc(ZGuid pk)
		{
			return DISHost.EDocs.FirstOrDefault(eDoc => !eDoc.IsDeleted && eDoc.UniqueKey == pk);
		}

		internal IDISBondDataDefault GetDefaultBondData(string code)
		{
			return DefaultValues.DefaultBondData.FirstOrDefault(bondData => bondData.Code == code);
		}

		public IEnumerable<IDISInvoiceLineDefault> GetAllInvoiceLines(ZString invoiceNumber)
		{
			return HostDataRepository.GetAllInvoiceLines(invoiceNumber);
		}

		public IEnumerable<IDISInvoiceLineDefault> GetInvoiceLines(ZString invoiceNumber, ZInt invoiceLineNoFrom, ZInt invoiceLineNoTo)
		{
			return HostDataRepository.GetInvoiceLines(invoiceNumber, invoiceLineNoFrom, invoiceLineNoTo);
		}

		public IEnumerable<IDISInvoiceLineDefault> GetCommodityLines(ZString invoiceNumber, ZInt invoiceLineNo, ZInt vehicleLineNo)
		{
			return HostDataRepository.GetCommodityLines(invoiceNumber, invoiceLineNo, vehicleLineNo);
		}

		public IEnumerable<IDISInvoiceLineDefault> GetCommodityLinesWithRangesOfInvoiceLines(ZString invoiceNumber, ZInt invoiceLineNoFrom, ZInt invoiceLineNoTo)
		{
			return HostDataRepository.GetCommodityLinesWithRangesOfInvoiceLines(invoiceNumber, invoiceLineNoFrom, invoiceLineNoTo);
		}

		public IEnumerable<IDISInvoiceLineDefault> GetCommodityLinesWithRangesOfVNELines(ZString invoiceNumber, ZInt invoiceLineNo, ZInt vneLineFrom, ZInt vneLineTo)
		{
			return HostDataRepository.GetCommodityLinesWithRangesOfCommodityLineNumbers(invoiceNumber, invoiceLineNo, vneLineFrom, vneLineTo);
		}

		DISHostDataRepository HostDataRepository
		{
			get { return hostDataRepository ?? (hostDataRepository = new DISHostDataRepository(DefaultValues)); }
		}
		DISHostDataRepository hostDataRepository;

		public void ReleaseObjects()
		{
			hostDataRepository = null;
		}

		public ZString MessageSendingWarning
		{
			get { return DISHost.MessageSendingWarning; }
		}

		public ZString MessageSendingError
		{
			get { return DISHost.MessageSendingError; }
		}

		#region DISHostDataRepository

		class DISHostDataRepository
		{
			public DISHostDataRepository(IUSDISDefaultValues defaultValues)
			{
				this.defaultValues = defaultValues;
				this.invoiceLines = new Dictionary<LineID, List<IDISInvoiceLineDefault>>();
				PopulateInvoiceLines();
			}

			readonly IUSDISDefaultValues defaultValues;
			readonly Dictionary<LineID, List<IDISInvoiceLineDefault>> invoiceLines;

			public IEnumerable<IDISInvoiceLineDefault> GetCommodityLines(ZString invoiceNumber, ZInt invoiceLineNo, ZInt vehicleLineNo)
			{
				var lineID = new LineID() { InvoiceNumber = invoiceNumber, InvoiceLineNo = invoiceLineNo, VehicleLineNo = vehicleLineNo };

				List<IDISInvoiceLineDefault> result;

				if (!invoiceLines.TryGetValue(lineID, out result) && vehicleLineNo == 0)
				{
					result = new List<IDISInvoiceLineDefault>();
					foreach (LineID key in invoiceLines.Keys)
					{
						if (key.InvoiceNumber == invoiceNumber && key.InvoiceLineNo == invoiceLineNo)
						{
							List<IDISInvoiceLineDefault> result2;

							if (invoiceLines.TryGetValue(key, out result2))
							{
								result.AddRange(result2);
							}
						}
					}
				}

				if (result != null)
				{
					result.Sort(new DISInvoiceLineDefaultComparer());
				}

				return result;
			}

			public IEnumerable<IDISInvoiceLineDefault> GetCommodityLinesWithRangesOfInvoiceLines(ZString invoiceNumber, ZInt invoiceLineNoFrom, ZInt invoiceLineNoTo)
			{
				var result = new List<IDISInvoiceLineDefault>();

				foreach (LineID lineID in invoiceLines.Keys)
				{
					if (lineID.InvoiceNumber == invoiceNumber)
					{
						List<IDISInvoiceLineDefault> result2;

						if (invoiceLines.TryGetValue(lineID, out result2))
						{
							foreach (IDISInvoiceLineDefault line in result2)
							{
								if (line.InvoiceLineNumber >= invoiceLineNoFrom && line.InvoiceLineNumber <= invoiceLineNoTo)
								{
									result.Add(line);
								}
							}
						}
					}
				}

				result.Sort(new DISInvoiceLineDefaultComparer());
				return result;
			}

			public IEnumerable<IDISInvoiceLineDefault> GetCommodityLinesWithRangesOfCommodityLineNumbers(ZString invoiceNumber, ZInt invoiceLineNo, ZInt vneLineFrom, ZInt vneLineTo)
			{
				var result = new List<IDISInvoiceLineDefault>();

				foreach (LineID lineID in invoiceLines.Keys)
				{
					if (lineID.InvoiceNumber == invoiceNumber)
					{
						List<IDISInvoiceLineDefault> result2;

						if (invoiceLines.TryGetValue(lineID, out result2))
						{
							foreach (IDISInvoiceLineDefault line in result2)
							{
								if (line.InvoiceLineNumber == invoiceLineNo)
								{
									var vehicleData = line.VehicleData;

									if (vehicleData == null && vneLineFrom == 0 && vneLineTo == 0
										|| vehicleData != null && vehicleData.VNELineNumber >= vneLineFrom && vehicleData.VNELineNumber <= vneLineTo)
									{
										result.Add(line);
									}
								}
							}
						}
					}
				}

				result.Sort(new DISInvoiceLineDefaultComparer());
				return result;
			}

			public IEnumerable<IDISInvoiceLineDefault> GetAllInvoiceLines(ZString invoiceNumber)
			{
				var result = new List<IDISInvoiceLineDefault>();
				foreach (LineID lineID in invoiceLines.Keys)
				{
					if (lineID.InvoiceNumber == invoiceNumber)
					{
						List<IDISInvoiceLineDefault> result2;

						if (invoiceLines.TryGetValue(lineID, out result2))
						{
							result.AddRange(result2);
						}
					}
				}

				result.Sort(new DISInvoiceLineDefaultComparer());
				return result;
			}

			public IEnumerable<IDISInvoiceLineDefault> GetInvoiceLines(ZString invoiceNumber, ZInt invoiceLineNoFrom, ZInt invoiceLineNoTo)
			{
				if (invoiceLineNoFrom > 0)
				{
					var to = invoiceLineNoTo > invoiceLineNoFrom ? invoiceLineNoTo : invoiceLineNoFrom;

					for (int index = invoiceLineNoFrom; index <= to; index++)
					{
						foreach (var line in GetDefaultLines(invoiceNumber, index))
						{
							yield return line;
						}
					}
				}
			}

			IEnumerable<IDISInvoiceLineDefault> GetDefaultLines(ZString invoiceNumber, ZInt invoiceLineNo)
			{
				var lineID = new LineID() { InvoiceNumber = invoiceNumber, InvoiceLineNo = invoiceLineNo };

				List<IDISInvoiceLineDefault> list;

				return invoiceLines.TryGetValue(lineID, out list) ? list : Enumerable.Empty<IDISInvoiceLineDefault>();
			}

			void PopulateInvoiceLines()
			{
				foreach (ICommercialInvoiceDefault invoice in defaultValues.DefaultInvoiceData)
				{
					foreach (IDISInvoiceLineDefault invoiceLine in invoice.InvoiceLines)
					{
						var lineID = GetLineID(invoice, invoiceLine);

						List<IDISInvoiceLineDefault> list;

						if (!invoiceLines.TryGetValue(lineID, out list))
						{
							list = new List<IDISInvoiceLineDefault>();
							invoiceLines.Add(lineID, list);
						}

						list.Add(invoiceLine);
					}
				}
			}

			static LineID GetLineID(ICommercialInvoiceDefault invoice, IDISInvoiceLineDefault invoiceLine)
			{
				return new LineID()
				{
					InvoiceNumber = invoice.InvoiceNumber,
					InvoiceLineNo = invoiceLine.InvoiceLineNumber,
					VehicleLineNo = invoiceLine.CommodityDetails != null && invoiceLine.CommodityDetails.VehicleData != null ? invoiceLine.CommodityDetails.VehicleData.VNELineNumber : ZInt.Zero
				};
			}

			class LineID
			{
				public ZString InvoiceNumber;
				public ZInt InvoiceLineNo;
				public ZInt VehicleLineNo;

				public override bool Equals(object obj)
				{
					var another = (LineID)obj;

					return this.InvoiceNumber.EqualsIgnoringCase(another.InvoiceNumber)
						&& this.InvoiceLineNo == another.InvoiceLineNo
						&& this.VehicleLineNo == another.VehicleLineNo;
				}

				public override int GetHashCode()
				{
					return InvoiceNumber.GetHashCode() ^ InvoiceLineNo.GetHashCode() ^ VehicleLineNo.GetHashCode();
				}
			}

			class DISInvoiceLineDefaultComparer : IComparer<IDISInvoiceLineDefault>
			{
				int IComparer<IDISInvoiceLineDefault>.Compare(IDISInvoiceLineDefault x, IDISInvoiceLineDefault y)
				{
					if (x == y)
					{
						return 0;
					}

					var result = x.InvoiceLineNumber.CompareTo(y.InvoiceLineNumber);

					if (result == 0)
					{
						result = x.EntryLineNumber.CompareTo(y.EntryLineNumber);
					}

					if (result == 0)
					{
						var xVehicleLineNo = x.CommodityDetails != null && x.CommodityDetails.VehicleData != null ? x.CommodityDetails.VehicleData.VNELineNumber : ZInt.Zero;
						var yVehicleLineNo = y.CommodityDetails != null && y.CommodityDetails.VehicleData != null ? y.CommodityDetails.VehicleData.VNELineNumber : ZInt.Zero;

						result = xVehicleLineNo.CompareTo(yVehicleLineNo);
					}

					return result;
				}
			}
		}
		#endregion

	}
}
