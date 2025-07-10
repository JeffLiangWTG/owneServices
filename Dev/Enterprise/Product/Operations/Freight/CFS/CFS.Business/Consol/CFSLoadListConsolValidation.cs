using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolValidation : CommonConsolValidation
	{
		public CFSLoadListConsolValidation(CFSLoadListConsol parent)
			: base(parent)
		{
		}

		protected override void CheckJK_RL_NKLastForeignPort()
		{
		}

		protected override void CheckJK_OA_SendingForwarderAddress()
		{
		}

		protected override void CheckJK_OA_ReceivingForwarderAddress()
		{
		}

		#region Calculated

		#region JK_OH_Forwarder

		public void ValidateJK_OH_Forwarder()
		{
			ValidateCalculatedProperty(Parent.JK_OH_ForwarderInfo);
		}

		protected void CheckJK_OH_Forwarder()
		{
			TypeValidation.CheckValidGuid(Parent.JK_OH_ForwarderInfo);
			MandatoryValidation.CheckEntered(Parent.JK_OH_ForwarderInfo, Res.GetString("cc09045c-608a-4392-9030-7b5e98c53288", "Client"));
			if (!Parent.JK_OH_ForwarderInfo.HasErrors() && Parent.Forwarder != null)
			{
				if (!Parent.Forwarder.OH_IsDebtor && !CompanyIsAnInternalCompany(Parent.Forwarder))
				{
					Parent.JK_OH_ForwarderInfo.AddError(Res.GetString("78c978cd-6329-4aaa-a32e-cdd81b01b497", "This client is not checked as receivables."));
				}
			}
		}

		bool CompanyIsAnInternalCompany(OrgHeader forwarder)
		{
			GlbCompanyCollection result = new GlbCompanyCollection(Parent.Factory);

			foreach (GlbCompany myCompany in result)
			{
				if (myCompany.OrgProxy != null && myCompany.OrgProxy.PK == forwarder.PK)
				{
					return true;
				}

				foreach (GlbBranch possibleBranch in myCompany.Branches)
				{
					if (possibleBranch.OrgProxy != null && possibleBranch.OrgProxy.PK == forwarder.PK)
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region JK_OA_CTOAddress

		public void ValidateJK_OA_CTOAddress()
		{
			ValidateCalculatedProperty(Parent.JK_OA_CTOAddressInfo);
		}

		protected void CheckJK_OA_CTOAddress()
		{
			ValidateJK_OA_DepartureCTOAddress();
			ValidateJK_OA_ArrivalCTOAddress();

			TypeValidation.CheckValidGuid(Parent.JK_OA_CTOAddressInfo);
			Parent.JK_OA_CTOAddressInfo.RunAdditionalValidation();
		}

		#endregion

		#region JK_OA_EmptyContainerYard

		public void ValidateJK_OA_EmptyContainerYard()
		{
			ValidateCalculatedProperty(Parent.JK_OA_EmptyContainerYardInfo);
		}

		protected void CheckJK_OA_EmptyContainerYard()
		{
			ValidateJK_OA_ContainerYardEmptyPickupAddress();
			ValidateJK_OA_ContainerYardEmptyReturnAddress();

			TypeValidation.CheckValidGuid(Parent.JK_OA_EmptyContainerYardInfo);
			Parent.JK_OA_EmptyContainerYardInfo.RunAdditionalValidation();
		}

		#endregion

		#region ValidateJK_OA_CartageCoAddress

		public void ValidateJK_OA_CartageCoAddress()
		{
			ValidateCalculatedProperty(Parent.JK_OA_CartageCoAddressInfo);
		}

		protected void CheckJK_OA_CartageCoAddress()
		{
			TypeValidation.CheckValidGuid(Parent.JK_OA_CartageCoAddressInfo);
		}

		public void ValidateCartageCoPK()
		{
			ValidateCalculatedProperty(Parent.CartageCoPKInfo);
		}

		protected void CheckCartageCoPK()
		{
			TypeValidation.CheckValidGuid(Parent.CartageCoPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.CartageCoPKInfo, Parent.LocalTransport_List);
		}

		#endregion

		#region ValidateDepotAddress

		public void ValidateJK_OA_DepotAddress()
		{
			ValidateCalculatedProperty(Parent.JK_OA_DepotAddressInfo);
		}

		protected void CheckJK_OA_DepotAddress()
		{
			TypeValidation.CheckValidGuid(Parent.JK_OA_DepotAddressInfo);
			MandatoryValidation.CheckEntered(Parent.JK_OA_DepotAddressInfo, Res.GetString("9ddac882-0435-44b9-b1d7-34ff0dce4fcd", "Depot Address"));

			if (!Parent.JK_OA_DepotAddressInfo.HasErrors())
			{
				ValidateDepotPK();
				Parent.JK_OA_DepotAddressInfo.AddAllNotificationsFrom(Parent.DepotPKInfo);
			}
		}

		public void ValidateDepotPK()
		{
			ValidateCalculatedProperty(Parent.DepotPKInfo);
		}

		protected void CheckDepotPK()
		{
			TypeValidation.CheckValidGuid(Parent.DepotPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.DepotPKInfo, Parent.Depot_List);
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJK_OH_Forwarder();
			ValidateJK_OA_CTOAddress();
			ValidateJK_OA_EmptyContainerYard();
			ValidateJK_OA_DepotAddress();
			ValidateJK_OA_CartageCoAddress();
			ValidateDepotPK();
			ValidateCartageCoPK();
		}

		#endregion

		#region Implementation

		public new CFSLoadListConsol Parent
		{
			get { return (CFSLoadListConsol)base.Parent; }
		}

		#endregion
	}
}
