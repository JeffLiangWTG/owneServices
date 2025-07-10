using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class CertificateCusCodeData : CusCodeData, IShortSequenceNumberLine
	{
		public CertificateCusCodeData(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryInstruction)); }
		}

		public CusEntryInstruction EntryInstruction
		{
			get { return (CusEntryInstruction)Parent; }
		}

		public CusPermitHeader PermitHeader
		{
			get
			{
				CusPermitHeader result = null;
				if (!CY_Code.IsEmpty)
				{
					var importer = EntryInstruction?.JobDeclaration?.Importer;
					if (importer != null)
					{
						var countryCode = Core.Constants.CountryCodes.SouthAfrica;
						var permitNumber = CY_Code;
						var assessmentDate = EntryInstruction.AssessmentDate.Date;
						var qtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
						var key = string.Format(CultureInfo.InvariantCulture, "CusPermitHeader_{0}_{1}_{2}_{3}_{4}", countryCode, permitNumber, importer.PK, assessmentDate, qtyValIndicator);
						result = Factory.GetCachedValue(key, () =>
						{
							return new CusPermitHeader.Loader(Factory).Load(countryCode, permitNumber, importer.PK, assessmentDate, qtyValIndicator, "");
						});
					}
				}
				return result;
			}
		}

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(CertificateLookups.Certificates))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set { base.CY_Code = value; }
		}

		public override ZShort CY_Order
		{
			get { return base.CY_Order; }
			set
			{
				if (value > 0)
				{
					ZShort oldValue = CY_Order;
					base.CY_Order = value;
					if (!IsCopying)
					{
						RecalculateWhenRenumbered(oldValue);
					}
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get { return base.CY_ParentID; }
			set
			{
				var oldEntryInstruction = EntryInstruction;
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (oldValue != CY_ParentID && !IsCopying)
				{
					RecalculateWhenAboutToBeDetachedOrDeleted(oldEntryInstruction);
					RecalculateWhenAdded();
				}
			}
		}

		protected abstract void RecalculateWhenAboutToBeDetachedOrDeleted(CusEntryInstruction instruction);

		protected abstract void RecalculateWhenAdded();

		protected abstract void RecalculateWhenRenumbered(ZShort oldValue);

		public IEnumerable<CertificateCusCodeData> GetCertificateCollection(CusEntryInstruction instruction) => instruction != null ? GetCertificateCollectionCore(instruction) : System.Array.Empty<CertificateCusCodeData>();

		protected abstract IEnumerable<CertificateCusCodeData> GetCertificateCollectionCore(CusEntryInstruction instruction);

		#endregion

		#region New Properties

		public ZDate ExpiryDate => PermitHeader?.CPH_EndDate ?? ZDate.Empty;

		public ZDecimal RemainingValue => PermitHeader?.ValueBalance ?? ZDecimal.Zero;

		public ZDecimal RemainingValueExcludingThisDeclaration
		{
			get
			{
				var result = ZDecimal.Zero;
				var permitHeader = PermitHeader;
				if (permitHeader != null)
				{
					result = new ZDecimal(permitHeader.CusPermitLineTransactions.OfType<BaseCusPermitLineTransaction>().Where(trans =>
					{
						return !EntryInstruction.LinkedEntryHeaders.Any(entry => trans.CPL_Reference == PermitHelper.GetPermitReferenceForEntry(entry)) && trans.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Deleted;
					}).Sum(x => x.CPL_TranValue));
				}
				return result;
			}
		}

		protected abstract ZString CusCodeDataTypeCode { get; }

		public ZString PermitType => PermitHeader?.CPH_Type ?? ZString.Empty;

		internal ZDecimal ValueUsedByThisDeclaration { get; set; }

		public new CertificateLookups Lookups
		{
			get { return (CertificateLookups)base.Lookups; }
		}

		#endregion

		#region Override Methods

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeCode;
		}

		#endregion

		#region IShortSequenceNumberLine

		public ZShort SequenceNumber
		{
			get { return CY_Order; }
			set { CY_Order = value; }
		}

		public ZGuid FKToHeader => EntryInstruction?.PK ?? ZGuid.Empty;

		#endregion

		#region IEqualityComparer

		public class EqualityComparer : IEqualityComparer<CertificateCusCodeData>
		{
			public bool Equals(CertificateCusCodeData x, CertificateCusCodeData y)
			{
				return x.CY_Code == y.CY_Code;
			}

			public int GetHashCode(CertificateCusCodeData obj)
			{
				return obj.CY_Code.GetHashCode();
			}
		}

		#endregion
	}
}
