using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CASCCode3 : CASCCode, IShortSequenceNumberLine
	{
		public CASCCode3(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string CusCodeDataType
		{
			get { return CusCodeDataTypeList.Codes.CASCode3; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CASCCode3Validation(this);
		}

		protected override void RenumberLine(ZShort oldValue)
		{
			if (InvoiceLine != null)
			{
				CASC3SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}

		protected override void DetachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC3SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		protected override void AttachedLine()
		{
			if (InvoiceLine != null)
			{
				CASC3SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return CY_Order; }
			set { CY_Order = value; }
		}

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		public ShortSequenceNumberGenerator CASC3SequenceGenerator => casc3SequenceGenerator ?? (casc3SequenceGenerator = new ShortSequenceNumberGenerator(() => InvoiceLine.CASCCode3s));
		ShortSequenceNumberGenerator casc3SequenceGenerator;
	}
}
