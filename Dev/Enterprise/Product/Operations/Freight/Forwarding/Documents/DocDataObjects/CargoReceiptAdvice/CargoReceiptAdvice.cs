using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class CargoReceiptAdvice : DocDataObject, IDataSourceProvider
	{
		public CargoReceiptAdvice(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProviderMembers

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region HIR Reference

		public ZString HIRReference
		{
			get => hirReference;
			set
			{
				if (SetNonPersistentPropertyValue(HIRReferenceInfo, ref hirReference, value))
				{
					Validate(HIRReferenceInfo);
				}
			}
		}
		ZString hirReference;

		public ZPropertyInfo HIRReferenceInfo => GetZPropertyInfo(nameof(HIRReference));

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get => marksAndNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersInfo, ref marksAndNumbers, value))
				{
					Validate(MarksAndNumbersInfo);
				}
			}
		}

		ZString marksAndNumbers;

		public ZPropertyInfo MarksAndNumbersInfo => GetZPropertyInfo(nameof(MarksAndNumbers));

		#endregion

		#region BookingParty

		public Address BookingParty
		{
			get => bookingParty;
			set => bookingParty = SetChild(bookingParty, value);
		}

		Address bookingParty;

		#endregion

		#region DepartureCFSAddress

		public Address DepartureCFSAddress
		{
			get => departureCFSAddress;
			set => departureCFSAddress = SetChild(departureCFSAddress, value);
		}

		Address departureCFSAddress;

		#endregion

		#region PackingLines

		public IReadOnlyCollection<PackingLine> PackingLines
		{
			get => packingLines;
			set => packingLines = SetChildCollection(packingLines, value);
		}

		IReadOnlyCollection<PackingLine> packingLines;

		#endregion

		#region Interim Receipt

		public ZString InterimReceipt
		{
			get => interimReceipt;
			set
			{
				if (SetNonPersistentPropertyValue(InterimReceiptInfo, ref interimReceipt, value))
				{
					Validate(InterimReceiptInfo);
				}
			}
		}

		ZString interimReceipt;

		public ZPropertyInfo InterimReceiptInfo => GetZPropertyInfo(nameof(InterimReceipt));

		public ZDateTime InterimReceiptDate
		{
			get => interimReceiptDate;
			set
			{
				if (SetNonPersistentPropertyValue(InterimReceiptDateInfo, ref interimReceiptDate, value))
				{
					Validate(InterimReceiptDateInfo);
				}
			}
		}

		ZDateTime interimReceiptDate;

		public ZPropertyInfo InterimReceiptDateInfo => GetZPropertyInfo(nameof(InterimReceiptDate));

		#endregion

	}
}
