using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode2 : CASCCode, IShortSequenceNumberLine
	{
		public CASCCode2(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string CusCodeDataType
		{
			get { return CusCodeDataTypeList.Codes.CASCode2; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CASCCode2Validation(this);
		}

		protected override void DetachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC2SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		protected override void AttachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC2SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		protected override void RenumberLine(ZShort oldValue)
		{
			if (InvoiceLine != null)
			{
				CASC2SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return CY_Order; }
			set { CY_Order = value; }
		}

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		public ShortSequenceNumberGenerator CASC2SequenceGenerator => casc2SequenceGenerator ?? (casc2SequenceGenerator = new ShortSequenceNumberGenerator(() => InvoiceLine.CASCCode2s));
		ShortSequenceNumberGenerator casc2SequenceGenerator;
	}
}
