using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISHostWrapperLookups : ZLookups
	{
		public DISHostWrapperLookups(DISHostWrapper disHost)
			: base(disHost)
		{
		}

		new DISHostWrapper Parent
		{
			get { return (DISHostWrapper)base.Parent; }
		}

		public IBusinessObjectCollection RequiredDocuments
		{
			get { return Parent.DISHost.RequiredDocumentsProvider.RequiredDocuments; }
		}

		public CodeDescriptionPairList EDocsList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("DIS_EDocsList"), delegate
				{
					var result = new CodeDescriptionPairList();
					foreach (var eDoc in Parent.DISHost.EDocs)
					{
						if (!eDoc.IsDeleted)
						{
							result.AddPair(eDoc.UniqueKey, eDoc.DocType.PadRight(3) + "-" + eDoc.FileName, "Added: " + eDoc.DateAdded.ToShortDateString() + " - " + eDoc.Description);
						}
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList InvoiceList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("InvoiceList"), delegate
				{
					var result = new CodeDescriptionPairList();

					foreach (var invoiceData in Parent.DefaultValues.DefaultInvoiceData)
					{
						result.AddPair(invoiceData.InvoiceNumber, invoiceData.Description);
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList BondDataList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("BondDataList"), delegate
				{
					var result = new CodeDescriptionPairList();

					foreach (var bondData in Parent.DefaultValues.DefaultBondData)
					{
						result.AddPair(bondData.Code, bondData.Description);
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList DefaultCBPRequestList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("DefaultCBPRequestList"), delegate
				{
					var result = new CodeDescriptionPairList();

					foreach (var request in Parent.DefaultValues.DefaultCBPRequests)
					{
						result.AddPair(request.ID, request.Description);
					}

					result.AddRange(new MiscCBPRequestIDList());
					return result;
				});
			}
		}

		public CodeDescriptionPairList ShipmentList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("ShipmentList"), delegate
				{
					var result = new CodeDescriptionPairList();
					foreach (var tradeTransactions in Parent.DefaultValues.DefaultTradeTransactions)
					{
						result.AddPair(tradeTransactions.ShipmentNo, tradeTransactions.XTN);
					}
					return result;
				});
			}
		}

		string GetCachedKey(string listName)
		{
			return Parent.PK.ToStringKey() + listName;
		}
	}
}
