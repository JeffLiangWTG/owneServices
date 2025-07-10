using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageDocManagerInfo))]
	public class CommonCartageDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CommonCartage>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var move2 = cartage.ContainerBookedMoves.AddNew();
			move.CartageLegs.AddNew();
			return cartage;
		}

		public void TestAccTransactionHeaderRetrievedAsRelatedObjects()
		{
			var cne = Factory.New<OrgHeader>();
			cne.OH_Code = "LCLCNE";
			cne.OH_IsConsignee = true;
			cne.OH_FullName = "Local Consignee ";
			cne.MainAddress.OA_Address1 = "Test Address Line";
			cne.OH_RL_NKClosestPort = "AUSYD";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			cartage.LocalClientPK = cne.PK;
			Factory.Save();
			var invoiceHelper = new InvoiceCreationTestHelper(Factory);
			var header = invoiceHelper.SetupTransaction(GlbBranch.CurrentBranch, "ANYNUMBER", cne, cartage.JJ_ConsignmentID, ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);
			var relatedObjects = ((IDocManagerSupport)cartage).DocManagerInfo.RelatedObjects;
			AssertEquals("Should have the AccTransactionHeader in the related business objects", true, relatedObjects.Contains(header));
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		public void TestAllRelatedObjectsRetrieved()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLCFStoCTO, null);
			var cartage1 = Factory.New<CartageForTest>();
			cartage1.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			var move = cartage1.ContainerBookedMoves.AddNew();
			var container1 = move.Container;
			var move2 = cartage1.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			var leg = move.CartageLegs.AddNew();
			var cfs = Factory.New<OrgHeader>();
			var cto = Factory.New<OrgHeader>();
			cartage1.FirstDocAddress.E2_OA_Address = cfs.MainAddress.PK;
			cartage1.SecondDocAddress.E2_OA_Address = cto.MainAddress.PK;
			IList<BusinessObject> relatedObjects = ((IDocManagerSupport)cartage1).DocManagerInfo.RelatedObjects;
			AssertEquals("Should have the Parent in the related business objects", true, relatedObjects.Contains((BusinessObject)cartage1.CartageParent));
			AssertEquals("Should have the Container1 in the related business objects", true, relatedObjects.Contains(container1));
			AssertEquals("Should have the Container2 in the related business objects", true, relatedObjects.Contains(container2));
			AssertEquals("Should have the Leg in the related business objects", true, relatedObjects.Contains(leg));
			AssertEquals("Should have the cfs in the related business objects", true, relatedObjects.Contains(cfs));
			AssertEquals("Should have the cto in the related business objects", true, relatedObjects.Contains(cto));
			var parentBooking = Factory.New<IDtbBooking>();
			cartage1.JJ_ParentID = parentBooking.PK;
			cartage1.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			var cartageParentBooking = cartage1.ParentBooking as BusinessObject;
			relatedObjects = ((IDocManagerSupport)cartage1).DocManagerInfo.RelatedObjects;
			AssertEquals("Should have the ParentBooking in the related business objects", true, relatedObjects.Contains(cartageParentBooking));
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			relatedObjects = ((IDocManagerSupport)cartage2).DocManagerInfo.RelatedObjects;
			AssertEquals("Should not have the ParentBooking in the related business objects", false, relatedObjects.Contains(cartageParentBooking));
			AssertEquals("Should not have the Parent in the related business objects", false, relatedObjects.Contains((BusinessObject)cartage2.CartageParent));
			AssertEquals("Should not have the Container1 in the related business objects", false, relatedObjects.Contains(container1));
			AssertEquals("Should not have the Container2 in the related business objects", false, relatedObjects.Contains(container2));
			AssertEquals("Should not have the Leg in the related business objects", false, relatedObjects.Contains(leg));
			AssertEquals("Should not have the cfs in the related business objects", false, relatedObjects.Contains(cfs));
			AssertEquals("Should not have the cto in the related business objects", false, relatedObjects.Contains(cto));
		}
	}
}
