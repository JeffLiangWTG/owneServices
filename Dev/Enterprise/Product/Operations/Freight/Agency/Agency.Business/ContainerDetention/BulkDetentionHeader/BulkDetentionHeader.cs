using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionHeader : AutoBulkDetentionHeader
	{
		public BulkDetentionHeader(BusinessObjectFactory factory)
			: base(factory) { }

		public Boolean HasSelectedChildren()
		{
			foreach (BulkDetentionChild child in Children)
			{
				if (child.IsSelected)
				{
					return true;
				}
			}
			return false;
		}

		[List("Lookups.DetentionTypes")]
		public override ZString DetentionType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.DetentionType; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.DetentionType = value; }
		}

		[List("Lookups.Clients")]
		public override ZGuid ClientPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.ClientPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ClientPK = value; }
		}

		[List("Lookups.Principals")]
		public override ZGuid PrincipalPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PrincipalPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PrincipalPK = value; }
		}

		[List("Lookups.Countries")]
		public override ZString CountryCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.CountryCode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.CountryCode = value; }
		}

		public BulkDetentionChildCollection Children
		{
			get { return children ?? (children = new BulkDetentionChildCollection(Factory)); }
		}
		BulkDetentionChildCollection children;

		public BulkDetentionHeaderLookups Lookups
		{
			get { return lookups ?? (lookups = new BulkDetentionHeaderLookups(this)); }
		}
		BulkDetentionHeaderLookups lookups;

		public void SelectUnselectAll(bool isSelected)
		{
			foreach (BulkDetentionChild child in Children)
			{
				child.IsSelected = isSelected;
			}
		}

		public void CreateDetentionInvoice(BusinessObjectFactory createFactory)
		{
			int generated = 0;

			foreach (BulkDetentionChild child in Children)
			{
				if (child.IsSelected)
				{
					child.IsLocked = true;
					child.CreateDetentionInvoice(createFactory);
					child.IsSelected = false;

					if (!child.DetentionPK.IsEmpty)
					{
						generated++;
					}
				}
			}

			GeneratedJobs = generated;

			bool postSaveFired = false;

			createFactory.Saved += delegate(BusinessObjectFactory savedFactory, bool savedSuccessfully)
			{
				if (savedSuccessfully && !postSaveFired)
				{
					postSaveFired = true;

					foreach (BulkDetentionChild child in Children)
					{
						ContainerDetention detention = savedFactory.Load<ContainerDetention>(child.DetentionPK);

						if (detention != null)
						{
							child.JobNumber = detention.NC_JobNumber;
						}
					}
				}
			};
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public void Find()
		{
			Children.RemoveAll();
			Children.AddRange(DetentionableLoader.LoadClientsWithDetentionableContainers(Factory, GlbCompany.CurrentCompany.PK, ClientPK, PrincipalPK, CountryCode, DetentionType));
		}
	}
}


