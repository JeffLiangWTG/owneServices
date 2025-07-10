using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class ABLEntryNum : ASYCUDA.Business.ABLEntryNum, ICustomsNumber
	{
		public ABLEntryNum(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ABLEntryNumLookups Lookups => (ABLEntryNumLookups)base.Lookups;
		protected override CusEntryNumLookups GetNewLookups() => new ABLEntryNumLookups(this);

		[ResourceStringData("ZA.Manifest.Business.ABLEntryNum.CusEntryNum|CE_EntryType", Caption = "LRN Type", FullDescription = "Indicates the Type of Reference Number.", ShortCaption = "Type")]
		public override ZString CE_EntryType
		{
			get => base.CE_EntryType;
			set
			{
				base.CE_EntryType = value;
				Bill?.CustomsEntryNumbers?.MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("ZA.Manifest.Business.ABLEntryNum.CusEntryNum|CE_EntryNum", Caption = "Local Reference No / LRN", FullDescription = "Number", ShortCaption = "Number")]
		public override ZString CE_EntryNum
		{
			get => base.CE_EntryNum;
			set
			{
				base.CE_EntryNum = value;
				Bill?.CustomsEntryNumbers?.MarkAsNeedingValidation();
			}
		}

		public override ZGuid CE_ParentID
		{
			get => base.CE_ParentID;
			set
			{
				base.CE_ParentID = value;
				Bill?.CustomsEntryNumbers?.MarkAsNeedingValidation();
			}
		}

		public override ZString CE_RN_NKCountryCode
		{
			get => base.CE_RN_NKCountryCode;
			set
			{
				base.CE_RN_NKCountryCode = value;
				Bill?.CustomsEntryNumbers?.MarkAsNeedingValidation();
			}
		}

		public override ZString CE_Category
		{
			get => base.CE_Category;
			set
			{
				base.CE_Category = value;
				Bill?.CustomsEntryNumbers?.MarkAsNeedingValidation();
			}
		}

		public ZString Type => CE_EntryType;
		public ZString Number => CE_EntryNum;

		protected override CusEntryNumValidation GetNewValidation() => Provider.GetNewValidation(this);
	}
}
