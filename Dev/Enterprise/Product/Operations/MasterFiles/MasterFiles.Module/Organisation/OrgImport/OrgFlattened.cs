using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public class OrgFlattened : AutoOrgFlattened
	{
		public OrgFlattened()
		{
		}

		[BusinessObjectTestExclude]
		public override ZString WorkNotes
		{
			get { return base.WorkNotes; }
			set { base.WorkNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString ForwardingNotes
		{
			get { return base.ForwardingNotes; }
			set { base.ForwardingNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString DeliveryNotes
		{
			get { return base.DeliveryNotes; }
			set { base.DeliveryNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString ARNotes
		{
			get { return base.ARNotes; }
			set { base.ARNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString InvoiceNotes
		{
			get { return base.InvoiceNotes; }
			set { base.InvoiceNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString APNotes
		{
			get { return base.APNotes; }
			set { base.APNotes = value; }
		}

		[BusinessObjectTestExclude]
		public override ZDateTime OC_DetailsVerified
		{
			get { return base.OC_DetailsVerified; }
			set { base.OC_DetailsVerified = value; }
		}

		[EmailAddress]
		public override ZString OA_Email
		{
			get
			{
				return base.OA_Email;
			}

			set
			{
				base.OA_Email = value;
			}
		}

		[EmailAddress]
		public override ZString OC_Email
		{
			get
			{
				return base.OC_Email;
			}

			set
			{
				base.OC_Email = value;
			}
		}
	}
}
