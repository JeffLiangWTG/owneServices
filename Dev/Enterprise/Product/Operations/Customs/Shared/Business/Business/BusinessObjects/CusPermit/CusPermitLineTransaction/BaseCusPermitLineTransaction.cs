using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(BaseCusPermitHeader), "CusPermitLineTransactions")]
	public class BaseCusPermitLineTransaction : SharedCusPermitLineTransaction, ICommonCusPermitLineTransaction
	{
		public BaseCusPermitLineTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BaseCusPermitLineTransaction[] FindPendingLines(ZString applicationID, ZString reference)
			{
				var query = new ZQuery(CusPermitLineTransactionSchema.CPL_Reference, reference);
				query.AddToFilter(CusPermitLineTransactionSchema.CPL_AppId, applicationID);
				query.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionStatus, PermitTransactionStatusList.Codes.Pending);
				return Factory.Load<BaseCusPermitLineTransaction>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(BaseCusPermitLineTransaction);
		}

		#region Override Properties

		[RelatedBusinessObject(nameof(BaseCusPermitLineTransaction.PermitHeader))]
		public override ZGuid CPL_CPH_PermitHeader
		{
			get => base.CPL_CPH_PermitHeader;
			set => base.CPL_CPH_PermitHeader = value;
		}

		protected override ZString ShortName => Res.GetString("56151FC1-62AB-434D-9242-C77ECEC20BB6", "Permit Transaction");

		#endregion

		#region New Properties

		public new BaseCusPermitHeader PermitHeader => Factory.Load<BaseCusPermitHeader>(CPL_CPH_PermitHeader);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPL_TransactionCategory = "VAL";
			CPL_TransactionType = "CUS";
			CPL_Reference = "ABC";
			CPL_TransactionDate = ZDate.BrettsBirthday;
		}
#endif

		#region Validation and Lookups

		public new BaseCusPermitLineTransactionLookups Lookups => (BaseCusPermitLineTransactionLookups)base.Lookups;

		protected override CusPermitLineTransactionLookups GetNewLookups() => new BaseCusPermitLineTransactionLookups(this);

		public new BaseCusPermitLineTransactionValidation Validation => (BaseCusPermitLineTransactionValidation)base.Validation;

		protected override CusPermitLineTransactionValidation GetNewValidation() => new BaseCusPermitLineTransactionValidation(this);

		#endregion Validation and Lookups
	}
}
