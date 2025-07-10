using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode1 : CASCCode, IShortSequenceNumberLine
	{
		public CASCCode1(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string CusCodeDataType
		{
			get { return CusCodeDataTypeList.Codes.CASCode1; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CASCCode1Validation(this);
		}

		protected override void DetachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC1SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		protected override void AttachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC1SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		protected override void RenumberLine(ZShort oldValue)
		{
			if (InvoiceLine != null)
			{
				CASC1SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return CY_Order; }
			set { CY_Order = value; }
		}

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		public ShortSequenceNumberGenerator CASC1SequenceGenerator => casc1SequenceGenerator ?? (casc1SequenceGenerator = new ShortSequenceNumberGenerator(() => InvoiceLine.CASCCode1s));
		ShortSequenceNumberGenerator casc1SequenceGenerator;
	}
}
