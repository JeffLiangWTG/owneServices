using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class AssignProductToPickFaceActionMethodApplicator : OperationalActionMethodApplicator, IDisposable
	{
		public AssignProductToPickFaceActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("aaba0a12-3a6f-4dbf-ae45-ce314663b4a4", "Product To Pick Face"), factory) // text used for logging
		{
			Products = new OrgSupplierPartCollection(factory, new ZQuery(OrgSupplierPartSchema.OP_IsActive, true));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (productForm != null)
				{
					productForm.Dispose();
				}
			}
		}

		#region Settings and Validation

		ZGuid productPK;

		public abstract class Schema
		{
			public const string ProductPK = "ProductPK";
		}

		[ResourceStringData("AssignProductToPickFaceActionMethodApplicator|Product", Caption = "Product")]
		public ZGuid ProductPK
		{
			get { return productPK; }
			set { SetNonPersistentPropertyValue(ProductPKInfo, ref productPK, value); }
		}

		public ZPropertyInfo ProductPKInfo => GetZPropertyInfo(
			Schema.ProductPK,
			Res.GetString("9a70145e-c768-4843-bd4e-8c6ebdf88678", "Product"));

		public OrgSupplierPartCollection Products { get; }

		#endregion

		OrgSupplierPartForm productForm;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] bizoList)
		{
			log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("3e2622ea-c3ae-48c7-b470-1c449e806410", "Product form will open and populate pick faces. Please modify and then save the form"));

			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.PK, ProductPK));
			var assignableLocations = new HashSet<ZGuid>();
			if (ValidateSupplierPart(part, log) && (assignableLocations = GetAssignableSelectedLocations(bizoList, log)).Count > 0)
			{
				var ownerRelationships = part.RelatedOrganisations.Cast<OrgPartRelation>().Where(ro => ro.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner || ro.OU_Relationship == OrgPartRelation.RelationshipTypes.Both);
				var ownerPKs = ownerRelationships.Select(ro => ro.OU_OH).Distinct().ToArray();
				var singleOwnerPK = ownerPKs.Length == 1 ? ownerPKs.Single() : ZGuid.Empty;

				if (singleOwnerPK.IsEmpty)
				{
					log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("fa28233a-98b5-4e4c-b07d-f30231fbbdd5", "Client was not defaulted as there are multiple owner relationships for this product"));
				}

				productForm = new OrgSupplierPartForm(part);
				productForm.Show();
				productForm.BeginInvoke(new MethodInvoker(() => { productForm.BringToFront(); productForm.Focus(); productForm.Activate(); productForm.TopMost = true; }));

				var zgrid1 = productForm.SelectAndReturnPickFaceGrid();
				foreach (var location in assignableLocations)
				{
					var pickFace = (WhsPickFace)zgrid1.List.AddNew();
					pickFace.WF_WL = location;
					pickFace.WF_OH_Client = singleOwnerPK;
					((ICancelAddNew)zgrid1.List).EndNew(zgrid1.List.Count - 1);
					Application.DoEvents();
				}
			}
		}

		bool ValidateSupplierPart(OrgSupplierPart part, IOperationalActionSectionLog log)
		{
			if (part == null || !part.OP_IsActive)
			{
				var errorMessage = part == null
					? Res.GetString("fd73c73b-2bdd-43f4-a3fe-fcd7d608d481", "The product code is invalid. Please select a valid product.")
					: Res.GetString("0a69330a-06de-4228-9c55-f9dcbfc5dead", "The selected product is inactive");
				log.Notify(OperationalActionLogErrorLevel.Error, errorMessage);
				return false;
			}
			return true;
		}

		HashSet<ZGuid> GetAssignableSelectedLocations(BusinessObject[] selectedBusinessObjects, IOperationalActionSectionLog log)
		{
			var selectedLocations = selectedBusinessObjects.Cast<WhsPickFaceView>();
			var locationsAlreadyAssignedOrWithStock = Factory.Load<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_OP, productPK)).Where(pf => pf.HasStockOrPendingTransactions || !pf.WPV_WF.IsEmpty);
			var assignableLocations = new HashSet<ZGuid>();
			foreach (var selectedLocation in selectedLocations)
			{
				var location = Factory.LoadTop1<WhsLocation>(new ZQuery(WhsLocationViewSchema.PK, selectedLocation.WPV_WL));
				if (locationsAlreadyAssignedOrWithStock.Any(pf => pf.WPV_WL == selectedLocation.WPV_WL && !pf.WPV_WF.IsEmpty))
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("ccd39901-aad4-4e3f-936c-06ddd8d568b1", "A pick face was already assigned for that product in Location {0}."), location.WLV_LocationString);
				}
				else if (locationsAlreadyAssignedOrWithStock.Any(pf => pf.WPV_WL == selectedLocation.WPV_WL && pf.HasStockOrPendingTransactions))
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("afbdbd7e-d116-4c78-80d8-119506c5635d", "Location {0} is not empty for that product and client, please clear the location before assigning it."), location.WLV_LocationString);
				}
				else
				{
					assignableLocations.Add(selectedLocation.WPV_WL);
				}
			}
			return assignableLocations;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class AssignProductToPickFaceActionMethodApplicator
	{
		public OrgSupplierPartForm ProductForm
		{
			get { return productForm; }
		}
	}
}
#endif
#endregion
