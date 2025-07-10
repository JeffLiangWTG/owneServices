using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeConfiguration : AutoAccSurchargeConfiguration, IBusinessObjectLogging
	{
		public new abstract class Schema : AutoAccSurchargeConfiguration.Schema
		{
			public const string IsApplicable = "IsApplicable";
		}

		public AccSurchargeConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region ASC_Type

		[List("Lookups.SurchargeTypeList")]
		public override ZString ASC_Type
		{
			get { return base.ASC_Type; }
			set { base.ASC_Type = value; }
		}

		#endregion

		#region ASC_BasisType

		[List("Lookups.SurchargeBasisTypeList")]
		public override ZString ASC_BasisType
		{
			get { return base.ASC_BasisType; }
			set
			{
				base.ASC_BasisType = value;
				SetAccSurchargeBasisesReadOnly();
			}
		}

		void SetAccSurchargeBasisesReadOnly()
		{
			var readOnly = ASC_BasisType == SurchargeBasisTypeList.Codes.ALL;
			if (readOnly)
			{
				AccSurchargeBasises.RemoveAndDeleteAll();
			}
			AccSurchargeBasises.SetReadOnlyIncludingChildren(readOnly);
			AccSurchargeBasises.RefreshBindingIncludingChildren();
		}

		#endregion

		protected bool ASC_Code_ReadOnly => !ASC_Code.IsEmpty && IsInDatabase && HasReferenceBySurchargeApplication;

		#region AccSurchargeBasises

		[ChildEditable(true)]
		public AccSurchargeBasisCollection AccSurchargeBasises
		{
			get
			{
				if (accSurchargeBasises == null)
				{
					accSurchargeBasises = new AccSurchargeBasisCollection(this);
					accSurchargeBasises.Load();
					RegisterEditableChildObject(accSurchargeBasises);
				}
				accSurchargeBasises.SetReadOnlyIncludingChildren(ASC_BasisType == InvoiceTypeChargeInclusionTypeList.Codes.ALL);

				return accSurchargeBasises;
			}
		}
		AccSurchargeBasisCollection accSurchargeBasises;

		#endregion

		#region IsApplicable

		public ZBool IsApplicable
		{
			get
			{
				return isApplicable;
			}
			set
			{
				SetNonPersistentPropertyValue(IsApplicableInfo, ref isApplicable, value);
			}
		}

		ZBool isApplicable;

		public ZPropertyInfo IsApplicableInfo
		{
			get { return GetZPropertyInfo(Schema.IsApplicable); }
		}
		#endregion

		#region Logging

		public ZString GetLogReference()
		{
			if (this.GetLogStatusCode() == Events.EditedARecord.Code)
			{
				var originalValues = $"Surcharge {BusinessObjectLoggingExtension.DeleteLogStatus}: Code: {ASC_CodeInfo.OriginalValue}, Surcharge Type: {ASC_TypeInfo.OriginalValue}, Percentage: {ASC_RateInfo.OriginalValue}%, Base Type: {ASC_BasisTypeInfo.OriginalValue}." + "\r\n";

				var currentValues = $"Surcharge {BusinessObjectLoggingExtension.AddedLogStatus}: Code: {ASC_Code}, Surcharge Type: {ASC_Type}, Percentage: {ASC_Rate}%, Base Type: {ASC_BasisType}.";

				return originalValues + currentValues;
			}
			else
			{
				return $"Surcharge {this.GetLogStatusShortDescription()}: Code: {ASC_Code}, Surcharge Type: {ASC_Type}, Percentage: {ASC_Rate}%, Base Type: {ASC_BasisType}.";
			}
		}

		#endregion

		public override void OnSaving()
		{
			ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper().AddLog(Company, this);
			base.OnSaving();
		}

		public bool HasReferenceBySurchargeApplication
		{
			get
			{
				var query = new ZQuery(AccSurchargeApplicationSchema.ASP_ASC_NKSurchargeCode, ASC_Code);
				query.AddToFilter(JoinCondition.And, AccSurchargeApplicationSchema.ASP_GC_Company, SQLComparisonOperator.Equal, ASC_GC_Company);
				return Factory.Exists(typeof(AccSurchargeApplication), query);
			}
		}

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !HasReferenceBySurchargeApplication;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = (MultilingualString)((NoResString)string.Empty);

				if (!CanDelete)
				{
					result = ResString.GetMultilingualString("A8F16DEB-A9C0-489E-8676-7A5DBF19659C", "This code is reference by a Surcharge Application rule, it cannot be deleted.");
				}

				return result;
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ASC_BasisType = "ALL";
			ASC_Type = "PER";
		}

#endif
	}
}
