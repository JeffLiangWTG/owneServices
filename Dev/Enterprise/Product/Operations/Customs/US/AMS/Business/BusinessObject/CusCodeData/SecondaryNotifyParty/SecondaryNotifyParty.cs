using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class SecondaryNotifyParty : CusCodeData,
		IShortSequenceNumberLine,
		ICanDelete,
		ISynchroniserReadOnlyMembersProvider
	{
		public SecondaryNotifyParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public override ZString CY_Type
		{
			get { return base.CY_Type; }
			set
			{
				if (value != SNPType)
				{
					ErrorReporter.ReportOnce(GetType().FullName + ".CY_Type Invalid Setting", "CY_Type should be '" + SNPType + "'"); // Column names are in a string, which is okay
				}
				var oldValue = CY_Type;
				base.CY_Type = value;
				if (!IsCopying && oldValue != CY_Type)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(SecondaryNotifyPartyLookups.SCACOrFIRMSList))]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != CY_Data)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				var oldBill = Bill;
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID)
				{
					SetOrderOnSettingCY_ParentID(oldBill);
					MarkParentAsNeedingValidation();
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get { return base.CY_ParentTableCode; }
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode)
				{
					MarkParentAsNeedingValidation();
				}
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsol))]
		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				if (value > 0)
				{
					var oldValue = CY_Order;

					base.CY_Order = value;

					var bill = Bill;
					if (!IsCopying && bill != null)
					{
						bill.SecondaryNotifyPartyOrderGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		public bool ShouldSynchroniseWithConsol
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.ShouldSynchronise;
			}
		}

		public bool IsInventoryRecordValidationMode
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.IsInventoryRecordValidationMode;
			}
		}

		public override void Delete()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.SecondaryNotifyPartyOrderGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		public CusInBondBill Bill
		{
			get { return Factory.Load<CusInBondBill>(CY_ParentID); }
		}

		public CusInBondHeader Header
		{
			get
			{
				var bill = Bill;
				return bill == null ? null : bill.Header;
			}
		}

		public new SecondaryNotifyPartyLookups Lookups
		{
			get { return (SecondaryNotifyPartyLookups)base.Lookups; }
		}

		public new SecondaryNotifyPartyValidation Validation
		{
			get { return (SecondaryNotifyPartyValidation)base.Validation; }
		}

		#region Implementation

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new SecondaryNotifyPartyLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new SecondaryNotifyPartyValidation(this);
		}

		void SetOrderOnSettingCY_ParentID(CusInBondBill oldBill)
		{
			if (!IsCopying)
			{
				if (oldBill != null)
				{
					oldBill.SecondaryNotifyPartyOrderGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}

				var bill = Bill;
				if (bill != null)
				{
					bill.SecondaryNotifyPartyOrderGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = SNPType;
		}

		public const string SNPType = "SNP";

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusInBondBill)); }
		}

		#endregion

		#region IShortSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CY_Order;
			set => CY_Order = value;
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !(ShouldSynchroniseWithConsol && (CY_Order == 1 || CY_Order == 2)); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ReasonForCannotDeleteStillBeingSynchronise; }
		}

		internal static MultilingualString ReasonForCannotDeleteStillBeingSynchronise
		{
			get { return ResString.GetMultilingualString("AMS|SecondaryNotifyParty|4E2143C1-136A-4C81-8A85-488F460A24BE", "Secondary Notify Party values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'."); }
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(System.ComponentModel.PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion
	}
}
