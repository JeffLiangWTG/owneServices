using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class LinkedEntry : CusAddInfo<USLinkedEntryAddInfo>
	{
		public LinkedEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : CusAddInfo.Schema
		{
			public const int LE_EntryNumberMaxLength = 11;
		}

		#endregion

		public JobDeclaration Declaration
		{
			get { return Factory.Load<JobDeclaration>(B7_ParentID); }
		}

		#region Validation and Lookups

		public USLinkedEntryAddInfoValidation AddInfoValidation
		{
			get { return Data.Validation; }
		}

		public USLinkedEntryAddInfoLookups AddInfoLookups
		{
			get { return Data.Lookups; }
		}

		#endregion

		#region US_LE_CH_EntryHeader

		public ZGuid US_LE_CH_EntryHeader
		{
			get { return Data.US_LE_CH_EntryHeader; }
		}

		void SetEntryHeaderAndDefaultsFromEntryNumber()
		{
			ResetEntryHeader();

			var entry = EntryHeader;
			Data.US_LE_CH_EntryHeader = (entry == null) ? ZGuid.Empty : Data.US_LE_CH_EntryHeader = entry.PK;

			US_LE_PortCode = entry != null ? entry.US_SchDEntry : ZString.Empty;
			US_LE_EntryDate = entry != null ? entry.ArrivalDate : ZDateTime.Empty;
			US_LE_LiquidationDate = entry != null ? entry.LiquidationDate : ZDateTime.Empty;
		}

		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					if (US_LE_EntryNumber.Length == 11)
					{
						var entryFilerCode = US_LE_EntryNumber.Left(3);
						var entryNumber = US_LE_EntryNumber.SubstringSafe(3);
						entryHeader = new CusEntryHeader.Loader(Factory).FindByEntryNumberAndFilerCode(GlbCompany.CurrentCompany.PK, entryNumber, entryFilerCode, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					}
				}
				return entryHeader;
			}
		}
		CusEntryHeader entryHeader;

		void ResetEntryHeader()
		{
			entryHeader = null;
		}

		#endregion

		#region US_LE_EntryDate

		public ZDateTime US_LE_EntryDate
		{
			get { return Data.US_LE_EntryDate; }
			set
			{
				Data.US_LE_EntryDate = value;
				US_LE_EntryDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_LE_EntryDateInfo
		{
			get { return GetZPropertyInfo(USLinkedEntryAddInfoSchema.Constants.US_LE_EntryDate); }
		}

		#endregion

		#region US_LE_EntryNumber

		[List(nameof(AddInfoLookups) + "." + nameof(USLinkedEntryAddInfoLookups.Entries))]
		public ZString US_LE_EntryNumber
		{
			get { return Data.US_LE_EntryNumber; }
			set
			{
				if (Data.US_LE_EntryNumber != value)
				{
					Data.US_LE_EntryNumber = value;
					SetEntryHeaderAndDefaultsFromEntryNumber();
					US_LE_EntryNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo US_LE_EntryNumberInfo
		{
			get { return GetWrappedZPropertyInfo(USLinkedEntryAddInfoSchema.Constants.US_LE_EntryNumber, x => Data.US_LE_EntryNumberInfo); }
		}

		#endregion

		#region US_LE_LiquidationDate

		public ZDateTime US_LE_LiquidationDate
		{
			get { return Data.US_LE_LiquidationDate; }
			set
			{
				Data.US_LE_LiquidationDate = value;
				US_LE_LiquidationDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_LE_LiquidationDateInfo
		{
			get { return GetWrappedZPropertyInfo(USLinkedEntryAddInfoSchema.Constants.US_LE_LiquidationDate, x => Data.US_LE_LiquidationDateInfo); }
		}

		#endregion

		#region US_LE_PortCode

		[List(nameof(AddInfoLookups) + "." + nameof(USLinkedEntryAddInfoLookups.SchDPortList))]
		public ZString US_LE_PortCode
		{
			get { return Data.US_LE_PortCode; }
			set
			{
				Data.US_LE_PortCode = value;
				US_LE_PortCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_LE_PortCodeInfo
		{
			get { return GetWrappedZPropertyInfo(USLinkedEntryAddInfoSchema.Constants.US_LE_PortCode, x => Data.US_LE_PortCodeInfo); }
		}

		#endregion

		#region US_LE_Withdraw

		public ZBool US_LE_Withdraw
		{
			get { return Data.US_LE_Withdraw; }
			set
			{
				Data.US_LE_Withdraw = value;
				US_LE_WithdrawInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_LE_WithdrawInfo
		{
			get { return GetWrappedZPropertyInfo(USLinkedEntryAddInfoSchema.Constants.US_LE_Withdraw, x => Data.US_LE_WithdrawInfo); }
		}

		#endregion

		#region Properties For Document Printing

		public ZString EntryFilerCode
		{
			get
			{
				var result = ZString.Empty;
				var entryFilerCandidate = US_LE_EntryNumber.SubstringSafe(0, 3);
				if (US_LE_EntryNumber.Length == Schema.LE_EntryNumberMaxLength)
				{
					result = entryFilerCandidate;
				}
				return result;
			}
		}

		public ZString EntryNumberCheckDigit
		{
			get { return US_LE_EntryNumber.Length == Schema.LE_EntryNumberMaxLength ? US_LE_EntryNumber.Right(1) : ZString.Empty; }
		}

		public ZString EntryNumber
		{
			get { return US_LE_EntryNumber.Length == Schema.LE_EntryNumberMaxLength ? US_LE_EntryNumber.SubstringSafe(3).SubstringSafe(0, 7) : ZString.Empty; }
		}

		#endregion
	}
}
