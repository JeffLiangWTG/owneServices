using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public class BillSynchronisationDataCalculator
	{
		public BillSynchronisationDataCalculator(JobDeclaration declaration, IBillDetails source)
		{
			this.source = source;
			this.declaration = declaration;
		}

		readonly IBillDetails source;
		readonly JobDeclaration declaration;

		ZString AMSBillNumber
		{
			get { return source.AMSBillNumberInfo != null ? (ZString)source.AMSBillNumberInfo.Value : ZString.Empty; }
		}

		public ZString BillNumber
		{
			get
			{
				Calculate();
				return billNumber;
			}
		}
		ZString billNumber;

		public ZString IssuerCode
		{
			get
			{
				Calculate();
				return issuerCode;
			}
		}
		ZString issuerCode;

		public ZBool SyncFromAMSBillNumber
		{
			get
			{
				Calculate();
				return syncFromAMSBillNumber;
			}
		}
		ZBool syncFromAMSBillNumber;

		void Calculate()
		{
			billNumber = ((ZString)source.BillNumberInfo.Value).KeepValidBillNumberCharacters();
			issuerCode = "";
			syncFromAMSBillNumber = false;

			if (declaration.IncludeSCACInBillNum)
			{
				if (!AMSBillNumber.IsEmpty)
				{
					billNumber = AMSBillNumber;
					syncFromAMSBillNumber = true;
					TrimBillNumber();
				}
				else
				{
					var validSCACs = source.GetValidSCACIssuerCodes(declaration.TransportMode);
					if (billNumber.ShouldTrimSCACFromBills(validSCACs))
					{
						issuerCode = billNumber.Left(4);
						billNumber = billNumber.GetBillNumberTrimSCAC();
					}
					else
					{
						issuerCode = validSCACs.FirstOrDefault(); // at this stage we don't need to consider IsTruck as IncludeSCACInBillNum is only true for Sea or Rail
					}
				}
			}
		}

		void TrimBillNumber()
		{
			if (!billNumber.SubstringSafe(0, 4).IsNumbersOnlyOrEmpty && billNumber.Length > 4)
			{
				issuerCode = billNumber.SubstringSafe(0, 4);
				billNumber = billNumber.SubstringSafe(4);
			}
		}
	}
}
