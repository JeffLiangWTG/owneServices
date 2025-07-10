using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ManifestToOpenHeader : CusEntryNumber
	{
		public ManifestToOpenHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZInt ClusterKey
		{
			get
			{
				var result = 0;
				var parent = Parent;

				if (parent != null && parent is JobDeclaration declaration)
				{
					result = declaration.JE_ClusterKey;
				}

				return result;
			}
		}

		public bool IsSea
		{
			get
			{
				var result = false;
				var parent = Parent;

				if (parent != null && parent is JobDeclaration declaration)
				{
					result = declaration.IsSea;
				}

				return result;
			}
		}

		public bool HasBillNumber => Bills.Any(b => !b.TPD_DocumentNumber.IsEmpty);

		[ChildEditable(true)]
		public ManifestToOpenBillCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = new ManifestToOpenBillCollection(this);
					RegisterEditableChildObject(bills);
				}
				return bills;
			}
		}
		ManifestToOpenBillCollection bills;

		protected override CusEntryNumValidation GetNewValidation() => new ManifestToOpenHeaderValidation(this);

		public new ManifestToOpenHeaderValidation Validation => (ManifestToOpenHeaderValidation)base.Validation;

		[MaxLength(20)]
		[ResourceStringData("c9af12c2-86f5-4412-ac2f-e7122af48c35", ShortCaption = "G.Man.Reg.No", Caption = "Global Manifest Registration No")]
		public override ZString CE_EntryNum { get => base.CE_EntryNum; set => base.CE_EntryNum = value; }

		[ResourceStringData("ddf0f923-9d61-4aa6-9efd-f91a7673b84b", ShortCaption = "G.Man.Reg.Date", Caption = "Global Manifest Registration Date")]
		public override ZDateTime CE_IssueDate
		{
			get => base.CE_IssueDate;
			set
			{
				base.CE_IssueDate = value;
				if (!IsCopying && CE_ExpiryDate.IsEmpty && !CE_IssueDate.IsEmpty && Parent != null)
				{
					if (IsSea)
					{
						CE_ExpiryDate = CE_IssueDate.AddDays(45);
					}
					else
					{
						CE_ExpiryDate = CE_IssueDate.AddDays(20);
					}
				}
			}
		}

		[ResourceStringData("100f3184-fedc-4fc0-bb6e-5288331ffa25", ShortCaption = "G.Man.Exp.Date", Caption = "Global Manifest Expiration Date")]
		public override ZDateTime CE_ExpiryDate { get => base.CE_ExpiryDate; set => base.CE_ExpiryDate = value; }

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Bills.DeleteAll();
				base.Delete();
			}
		}
	}
}
