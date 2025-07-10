using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader
{
	abstract class ISFReferenceDataObjectReader<T> : DataObjectReader<T, CusISFBill>, IReferenceDataObjectReader
		where T : IDataObject
	{
		public ISFReferenceDataObjectReader(T dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header)
			: base(dataObject, logger, factory)
		{
			this.header = header;
		}
		protected readonly CusISFHeader header;

		protected override CusISFBill GetNewBusinessObject()
		{
			return header.ReferenceDatas.AddNew();
		}

		protected override CusISFBill GetExistingBusinessObject()
		{
			CusISFBill result = null;
			var billNumber = BillNumber;
			var billType = BillType;
			if (!billType.IsEmpty && !billNumber.IsEmpty)
			{
				var query = new ZQuery();
				query.AddToFilter(CusISFBillSchema.BB_BF, header.PK);
				query.AddToFilter(CusISFBillSchema.BB_BillNum, billNumber);
				query.AddToFilter(CusISFBillSchema.BB_BillType, billType);
				result = factory.LoadTop1<CusISFBill>(query);
			}

			return result;
		}

		protected override void PopulateBusinessObject(CusISFBill targetBO)
		{
			var billBO = GetColumnIndexer(targetBO);
			SetValue(billBO, CusISFBillSchema.BB_BillType, BillType);
			SetValue(billBO, CusISFBillSchema.BB_BillNum, BillNumber);
		}

		public CusISFBill BillBO
		{
			get { return ReadIntoBusinessObject(); }
		}

		protected abstract ZString BillNumber { get; }
		public abstract ZString BillType { get; }
	}

	class ISFAdditionalBillDataObjectReader : ISFReferenceDataObjectReader<AdditionalBill>
	{
		public ISFAdditionalBillDataObjectReader(AdditionalBill dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header)
			: base(dataObject, logger, factory, header)
		{
		}

		protected override ZString BillNumber
		{
			get
			{
				if (!billNumber.HasValue)
				{
					billNumber = dataObject.BillNumber.GetValueOrDefault();
				}
				return billNumber.Value;
			}
		}
		ZString? billNumber;

		public override ZString BillType
		{
			get
			{
				if (!billType.HasValue)
				{
					billType = dataObject.BillType.GetCodeAsUpperCase();
				}
				return billType.Value;
			}
		}
		ZString? billType;
	}

	class ISFEntryNumberBillDataObjectReader : ISFReferenceDataObjectReader<EntryNumber>
	{
		public ISFEntryNumberBillDataObjectReader(EntryNumber dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header)
			: base(dataObject, logger, factory, header)
		{
		}

		protected override ZString BillNumber
		{
			get
			{
				if (!billNumber.HasValue)
				{
					billNumber = dataObject.Number.GetValueOrDefault();
				}
				return billNumber.Value;
			}
		}
		ZString? billNumber;

		public override ZString BillType
		{
			get { return BillTypeList.Codes.USCBPEntryNumber; }
		}
	}

	class ISFAdditionalReferenceBillDataObjectReader : ISFReferenceDataObjectReader<AdditionalReference>
	{
		public ISFAdditionalReferenceBillDataObjectReader(AdditionalReference dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header)
			: base(dataObject, logger, factory, header)
		{
		}

		protected override ZString BillNumber
		{
			get
			{
				if (!billNumber.HasValue)
				{
					billNumber = dataObject.ReferenceNumber.GetValueOrDefault();
				}
				return billNumber.Value;
			}
		}
		ZString? billNumber;

		public override ZString BillType
		{
			get
			{
				if (!billType.HasValue)
				{
					billType = dataObject.Type.GetCodeAsUpperCase();
				}
				return billType.Value;
			}
		}
		ZString? billType;
	}
}
