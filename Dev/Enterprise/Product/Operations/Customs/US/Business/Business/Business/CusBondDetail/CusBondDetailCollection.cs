using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusBondDetailCollection : MasterFiles.Business.CusBondDetailCollection
	{
		public CusBondDetailCollection(OrgHeader organisation)
			: base(organisation)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.UsaInBond);
			return query;
		}

		public new CusBondDetail this[int index]
		{
			get { return (CusBondDetail)base[index]; }
		}

		public new CusBondDetail AddNew()
		{
			return (CusBondDetail)base.AddNew();
		}

		protected override BusinessObject AddNewCore(System.Type bizOType)
		{
			return base.AddNewCore(typeof(CusBondDetail));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CusBondDetail)child).PW_ApplicationCode = ApplicationCodeList.Codes.UsaInBond;
		}

		public CusBondDetail GetActiveBondDetailDataFor(List<ZString> activityCodes, ZString bondType, ZDateTime effectiveDate)
		{
			CusBondDetail result = null;

			foreach (CusBondDetail bondData in this)
			{
				if (bondData.IsBondActive(effectiveDate))
				{
					bool passesBondTypeCheck = bondType.IsEmpty || bondData.PW_BondType == bondType;
					bool passesActivityCodeCheck = activityCodes == null || activityCodes.Count == 0 || activityCodes.Contains(bondData.PW_ActivityCode);

					if (passesBondTypeCheck && passesActivityCodeCheck)
					{
						if (result == null)
						{
							result = bondData;
						}
						else if (bondData.IsContinuousBond && !result.IsContinuousBond)//continuous bond takes precedence
						{
							result = bondData;
						}
						else if (!bondData.PW_BondEffectiveDate.IsEmpty
							&& (result.PW_BondEffectiveDate.IsEmpty || bondData.PW_BondEffectiveDate > result.PW_BondEffectiveDate))//bond that has the latest effective date is the one that is defaulted
						{
							result = bondData;
						}
					}
				}
			}

			return result;
		}

		public CusBondDetail GetBondDetailForAccountNo(ZString accountNo)
		{
			CusBondDetail result = null;

			foreach (CusBondDetail bondData in this)
			{
				if (bondData.PW_BondNumber == accountNo)
				{
					result = bondData;
					break;
				}
			}

			return result;
		}

		public CusBondDetail GetBondDetailForSuretyCode(ZString suretyCode)
		{
			CusBondDetail result = null;
			foreach (CusBondDetail bondData in this)
			{
				if (bondData.PW_SuretyCode == suretyCode)
				{
					result = bondData;
					break;
				}
			}
			return result;
		}

		public bool HasContinuousBond
		{
			get
			{
				foreach (CusBondDetail bondData in this)
				{
					if (bondData.IsContinuousBond)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
