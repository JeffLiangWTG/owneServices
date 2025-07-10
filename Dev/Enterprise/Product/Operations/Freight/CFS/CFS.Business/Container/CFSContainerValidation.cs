using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerValidation : CommonContainerValidation
	{
		public CFSContainerValidation(CFSContainer container)
			: base(container)
		{
		}

		public new CFSContainer Parent
		{
			get { return (CFSContainer)base.Parent; }
		}

		#region JC_OH_CFSClient

		protected override void CheckJC_OH_CFSClient()
		{
			base.CheckJC_OH_CFSClient();
			MandatoryValidation.CheckEntered(Parent.JC_OH_CFSClientInfo);
			ListValidation.ErrorIfInvalidPK(Parent.JC_OH_CFSClientInfo, Container.ClientOrgHeaderList);
		}

		#endregion

		#region JC_Purpose

		protected override void CheckJC_Purpose()
		{
			base.CheckJC_Purpose();
			ListValidation.ErrorIfInvalidCode(Container.JC_PurposeInfo, Container.JC_Purpose_List);
		}

		#endregion

		#region JC_ContainerMode

		protected override void CheckJC_ContainerMode()
		{
			base.CheckJC_ContainerMode();
			if (!SkipContainerModeListCheck)
			{
				ListValidation.ErrorIfInvalidCode(Container.JC_ContainerModeInfo, Container.JC_ContainerMode_List);
			}
		}

		#endregion

		#region JC_DeliveryMode

		protected override void CheckJC_DeliveryMode()
		{
		}

		#endregion

		#region JC_ContainerNum

		protected override void CheckJC_ContainerNum()
		{
			CommonConsol consol = Container.Consol;
			JobSailing sailing = Container.Sailing;

			if (!Container.JC_ContainerNum.IsEmpty)
			{
				ContainerNumberValidation.WarnIfInvalid(Container.JC_ContainerNumInfo);
				base.CheckContainerNumberAgainstRelatedContainers();
			}
			else
			{
				if ((consol != null && consol.IsImport()) || (sailing != null && sailing.Origin != null && GlbBranch.CurrentBranch.Country != null && sailing.Destination.JB_RL_NKPortOfDischarge.StartsWith(GlbBranch.CurrentBranch.Country.RN_Code, StringComparison.Ordinal)))
				{
					Container.JC_ContainerNumInfo.AddError(Res.GetString("4e603286-48fe-4f78-914c-6e4e95057f73", "Container number is required for imports"));
				}

				if (Parent.IsGrossWeightVerified)
				{
					Parent.JC_ContainerNumInfo.AddError(Res.GetString("61e90b01-0f2c-499f-92f7-0d4ff44d3581", "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON."));
				}
			}
		}

		#endregion

		#region JC_RH_NKContainerCommodityCode

		protected override void CheckJC_RH_NKContainerCommodityCode()
		{
		}

		#endregion

		#region JC_FCLAvailable

		protected override void CheckJC_FCLAvailable()
		{
			base.CheckJC_FCLAvailable();
			if (Parent.JC_FCLAvailable.IsValid &&
				Parent.JC_ArrivalTime.IsValid &&
				Parent.JC_FCLAvailable.Date > Parent.JC_ArrivalTime.Date)
			{
				Parent.JC_FCLAvailableInfo.AddWarning(Res.GetString("10a4942f-57c6-4d0a-a599-37ef69462156", "FCL Available Date should be before Arrival Date"));
			}
		}

		#endregion

		#region JC_LCLUnpack

		protected override void CheckJC_LCLUnpack()
		{
			base.CheckJC_LCLUnpack();

			DateRangeValidation.ValidateDateValueHasChanged(Parent.JC_LCLUnpackInfo);

			if (!Parent.JC_LCLUnpackInfo.HasErrors())
			{
				if (Parent.JC_LCLUnpack.IsEmpty && AtLeastOnePackLineHasOutturnOrOutturnNotes())
				{
					Parent.JC_LCLUnpackInfo.AddWarning(Res.GetString("420ac92c-d93a-48ca-a719-12276faa8d17", "Outturned packages and/or Outturn Notes have been entered but Unpack Date is empty."));
				}
			}
		}

		protected bool AtLeastOnePackLineHasOutturnOrOutturnNotes()
		{
			foreach (PackLine pack in Parent.PackLines)
			{
				if (pack.JL_Outturn > 0 || !pack.JL_OutturnComment.IsEmpty)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region JC_LCLAvailable

		protected override void CheckJC_LCLAvailable()
		{
			base.CheckJC_LCLAvailable();

			DateRangeValidation.ValidateDateValueHasChanged(Parent.JC_LCLAvailableInfo);

			if (!Parent.JC_LCLAvailableInfo.HasErrors())
			{
				if (Parent.JC_LCLUnpack.IsValid
					&& !Parent.JC_LCLAvailable.IsEmpty && !Parent.JC_LCLUnpack.IsEmpty
					&& Parent.JC_LCLAvailable < Parent.JC_LCLUnpack)
				{
					Parent.JC_LCLAvailableInfo.AddError(Res.GetString("b8db89fd-291b-432f-9f88-d2f182103423", "Please enter a date later than or equal to the Unpack date or remove the Unpack date."));
				}
			}

			ValidateJC_LCLStorageCommences();
		}

		#endregion

		#region JC_LCLStorageCommences

		protected override void CheckJC_LCLStorageCommences()
		{
			base.CheckJC_LCLStorageCommences();

			DateRangeValidation.ValidateDateValueHasChanged(Parent.JC_LCLStorageCommencesInfo);

			if (!Parent.JC_LCLStorageCommencesInfo.HasErrors())
			{
				if ((Parent.JC_LCLStorageCommences.IsEmpty && !Parent.JC_LCLAvailable.IsEmpty) ||
					(!Parent.JC_LCLStorageCommences.IsEmpty && !Parent.JC_LCLAvailable.IsEmpty && Parent.JC_LCLStorageCommences <= Parent.JC_LCLAvailable))
				{
					Parent.JC_LCLStorageCommencesInfo.AddError(Res.GetString("f247edea-63c9-4503-a48c-572ee26cae5a", "Please enter a date later than the Available From date"));
				}
			}
		}

		#endregion

		#region Calculated Property Validation

		#region JC_TransportMode

		public void ValidateJC_TransportMode()
		{
			ValidateCalculatedProperty(Container.JC_TransportModeInfo);
		}

		protected virtual void CheckJC_TransportMode()
		{
			ListValidation.ErrorIfInvalidCode(Container.JC_TransportModeInfo, Container.JC_TransportMode_List);
		}

		#endregion

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateJC_TransportMode();
			ValidateJC_JA_NKPortOfLoading();
			ValidateJC_JB_NKPortOfDischarge();
		}

		#endregion

		#region Implementation

		CFSContainer Container
		{
			get { return Parent; }
		}

		#region DateRangeValidation

		DateRangeValidation DateRangeValidation
		{
			get { return dateRangeValidation ?? (dateRangeValidation = new DateRangeValidation()); }
		}
		DateRangeValidation dateRangeValidation;

		#endregion

		#region HasContainerDepartureDetails

		public bool HasContainerDepartureDetails()
		{
			int nonEmptyCounter = 0;

			if (!Container.JC_DepartureTruckRegistration.IsEmpty)
			{
				nonEmptyCounter++;
			}

			if (Container.JC_OA_DepartureTransportAddress.IsValid)
			{
				nonEmptyCounter++;
			}

			if (!Container.JC_DepartureTruckDriversLicense.IsEmpty)
			{
				nonEmptyCounter++;
			}

			if (!Container.JC_DepartureTime.IsEmpty)
			{
				nonEmptyCounter++;
			}

			return nonEmptyCounter > 0;
		}

		#endregion

		#region JC_JX

		protected override void CheckJC_JX()
		{
			base.CheckJC_JX();
			ValidatePackOrUnpackStatus(Container.JC_JXInfo);
		}

		#endregion

		#region JC_JA_NKPortOfLoading

		public void ValidateJC_JA_NKPortOfLoading()
		{
			ValidateCalculatedProperty(Container.JC_JA_NKPortOfLoadingInfo);
		}

		protected virtual void CheckJC_JA_NKPortOfLoading()
		{
			ValidatePackOrUnpackStatus(Container.JC_JA_NKPortOfLoadingInfo);
		}

		#endregion

		#region JC_JB_NKPortOfDischarge

		public void ValidateJC_JB_NKPortOfDischarge()
		{
			ValidateCalculatedProperty(Container.JC_JB_NKPortOfDischargeInfo);
		}

		protected virtual void CheckJC_JB_NKPortOfDischarge()
		{
			ValidatePackOrUnpackStatus(Container.JC_JB_NKPortOfDischargeInfo);
		}

		#endregion

		#region PackOrUnpackStatus

		void ValidatePackOrUnpackStatus(ZPropertyInfo info)
		{
			PackUnpackStatusHelper.PackUnpackStatus status = Container.PackOrUnpackStatus;
			bool isError = true;
			ZString error = "";

			switch (status)
			{
				case PackUnpackStatusHelper.PackUnpackStatus.InvalidBranchHomePort:
					error = Res.GetString("6bbc1562-eb0d-4c0b-999f-f7635c1ce932", "Please edit your branch details to add a home port for your branch.");
					break;
				case PackUnpackStatusHelper.PackUnpackStatus.InvalidSamePort:
					error = Res.GetString("2e95bbfa-9954-4b70-bfe9-14cefc2bac39", "The load and discharge ports cannot be the same.");
					break;
				default:
					isError = false;
					break;
			}

			if (isError)
			{
				info.AddError(error);
			}
		}

		#endregion

		#endregion
	}
}
