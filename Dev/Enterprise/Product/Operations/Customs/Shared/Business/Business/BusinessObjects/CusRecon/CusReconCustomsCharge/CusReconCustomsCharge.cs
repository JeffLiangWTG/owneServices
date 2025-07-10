using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusReconCustomsCharge : AutoCusReconCustomsCharge, Integration.Customs.ICusReconCustomsCharge
	{
		public CusReconCustomsCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusReconCustomsChargeTypeDecider TypeDecider = new CusReconCustomsChargeTypeDecider();

		public CusReconEntryLine ReconEntryLine
		{
			get
			{
				if (!IsDeleted && (reconEntryLine == null || reconEntryLine.PK != CRC_CRL_Line))
				{
					reconEntryLine = Factory.Load<CusReconEntryLine>(CRC_CRL_Line);
				}

				return reconEntryLine != null && !reconEntryLine.IsDeleted ? reconEntryLine : null;
			}
		}

		CusReconEntryLine reconEntryLine;

		[RelatedBusinessObject(nameof(ReconEntryLine))]
		public override ZGuid CRC_CRL_Line
		{
			get => base.CRC_CRL_Line; 
			set => base.CRC_CRL_Line = value;
		}
	}
}
