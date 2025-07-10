using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class CusPermitHeader : BaseCusPermitHeader, Integration.Customs.ZA.ICusPermitHeader
	{
		public CusPermitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public new class Loader : BaseCusPermitHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new CusPermitHeader Load(ZString country, ZString permitNumber, ZGuid permitHolder, ZDateTime transactionDate, string qtyValIndicator = "", string type = "", string subType = "", Dictionary<ZString, ZString> rules = null, ZGuid? appliesTo = null)
			{
				return base.Load(country, permitNumber, permitHolder, transactionDate, qtyValIndicator, type, subType, rules, appliesTo) as CusPermitHeader;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusPermitHeader);
			}
		}

		#endregion

		#region Override Properties

		[MaxLength(32)]
		public override ZString CPH_Number
		{
			get { return base.CPH_Number; }
			set { base.CPH_Number = value; }
		}

		[List(nameof(Lookups) + "." + nameof(BaseCusPermitHeaderLookups.PermitTypes))]
		public override ZString CPH_Type
		{
			get { return base.CPH_Type; }
			set { base.CPH_Type = value; }
		}

		[List(nameof(Lookups) + "." + nameof(BaseCusPermitHeaderLookups.PermitSubTypes))]
		public override ZString CPH_SubType
		{
			get { return base.CPH_SubType; }
			set { base.CPH_SubType = value; }
		}

		protected override ZBool AllowNewLineTransactions => IsCUM;

		public override bool CPH_StartDate_ReadOnly => false;

		#endregion

		#region Implementation

		protected override CusPermitLineTransactionCollection CreateNewCusPermitLineTransactionCollection()
		{
			return new CusPermitLineTransactionCollection(this);
		}

		[ChildEditable(true)]
		public new CusPermitLineTransactionCollection CusPermitLineTransactions => base.CusPermitLineTransactions;

		protected override Customs.Business.CusPermitHeaderValidation GetNewValidation()
		{
			return new CusPermitHeaderValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CPH_EndDate = ZDateTime.MaxSmallDateTime.Date;
		}

		#endregion
	}
}
