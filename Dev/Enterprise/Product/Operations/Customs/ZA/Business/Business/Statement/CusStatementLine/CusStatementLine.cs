using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	[DependentBusinessObject(typeof(CusStatementHeader), "StatementLines")]
	public class CusStatementLine : BaseCusStatementLine
	{
		public CusStatementLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		[ChildEditable(true)]
		public CusStatementLineChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new CusStatementLineChargeCollection(this);
					RegisterEditableChildObject(fCharges);
				}
				return fCharges;
			}
		}
		CusStatementLineChargeCollection fCharges;

		public CusStatementHeader Header
		{
			get
			{
				if (IsNull || B3_B2.IsEmpty)
				{
					return (CusStatementHeader)Factory.GetNull(GetCusStatementHeaderType());
				}
				else
				{
					ZGuid foreignKey = B3_B2;
					if (fHeader == null || fHeader.PK != foreignKey)
					{
						fHeader = (CusStatementHeader)Factory.Load(GetCusStatementHeaderType(), foreignKey);
					}
					return fHeader != null && !fHeader.IsDeleted ? fHeader : (CusStatementHeader)Factory.GetNull(GetCusStatementHeaderType());
				}
			}
		}
		CusStatementHeader fHeader;

		#endregion

		#region Implementation

		protected virtual Type GetCusStatementHeaderType()
		{
			return typeof(CusStatementHeader);
		}

		#endregion
	}
}
