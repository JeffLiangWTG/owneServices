using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerValidation : CommonContainerValidation
	{
		public AgencyShipmentContainerValidation(AgencyShipmentContainer container)
			: base(container) { }

		protected static string notEmptyContainerHasNoPacklines
		{
			get { return Res.GetString("4c3425cd-2561-4db7-8fce-382ff7369db2", "This container has no pack lines packed into it and yet is not marked as empty"); }
		}
		protected static string emptyContainerHasPacklines
		{
			get { return Res.GetString("573926ae-4c2c-42c7-9923-202fa262386d", "This container has pack lines packed into it and yet is marked as empty"); }
		}

		#region JC_IsEmptyContainer

		protected override void CheckJC_IsEmptyContainer()
		{
			base.CheckJC_IsEmptyContainer();

			if (Parent.JC_IsEmptyContainer && Parent.SupportsPackLines && Parent.PackLines.Count != 0)
			{
				Parent.JC_IsEmptyContainerInfo.AddError(emptyContainerHasPacklines);
			}
		}

		#endregion

		#region JC_ContainerNum

		protected override void CheckContainerNumberAgainstRelatedContainers()
		{
			if (!Parent.JC_ContainerNum.IsEmpty)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobContainerSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(JobContainerSchema.JC_JS_FCLBookingOnlyLink, Parent.JC_JS_FCLBookingOnlyLink);
				filter.AddToFilter(JobContainerSchema.JC_Purpose, Parent.JC_Purpose);
				filter.AddToFilter(JobContainerSchema.JC_ContainerNum, Parent.JC_ContainerNum);

				if (Parent.Factory.LoadTop1(Parent.GetType(), filter) != null)
				{
					Parent.JC_ContainerNumInfo.AddError(MessageForDuplicatedContainers);
				}
			}
		}

		protected virtual string MessageForDuplicatedContainers
		{
			get { return Res.GetString("7fab3907-8eeb-4b09-bc1e-a7082c6c9ea8", "Duplicate Container Number is entered."); }
		}

		protected override void CheckJC_ContainerNum()
		{
			base.CheckJC_ContainerNum();

			AgencyShipment booking;
			JobSailing sailing;
			JobVoyage voyage;
			if (!Parent.JC_ContainerNum.IsEmpty && Parent.JC_ContainerMode == Constants.ContainerModes.FCL
				&& (booking = Parent.Booking) != null && (sailing = booking.Sailing) != null && (voyage = sailing.Voyage) != null)
			{
				var billName = Res.GetString("6163fb0c-814d-48fb-961e-62974698d051", "Bill Of Lading");
				var bookingName = Res.GetString("d688dfeb-2c23-497f-adb3-65978626dab7", "Booking");

				ZString shipments = GetShipments(Parent.JC_JS_FCLBookingOnlyLink, voyage.PK, Parent.JC_ContainerNum, billName, bookingName);
				if (!shipments.IsEmpty)
				{
					Parent.JC_ContainerNumInfo.AddWarning(Res.GetString("1a58513b-b2d0-4560-9721-3643209b5d2d",
						"Container number {0} is flagged as FCL, but it is also registered against the following shipment(s) on the same vessel-voyage:\r\n{1}\r\nThe Container Mode on this {2} {3} should not be FCL.",
						Parent.JC_ContainerNum,
						shipments,
						booking.IsBillOfLadingStage ? billName : bookingName,
						booking.JS_UniqueConsignRef));
				}
			}
		}

		ZString GetShipments(ZGuid shipment, ZGuid voyage, ZString containerNum, ZString billName, ZString bookingName)
		{
			var modeName = Res.GetString("f6923e3a-ec9e-4060-a11d-41fb8520213f", "Container Mode");
			var delimiter1 = " ";
			var delimiter2 = ", ";

			var query = string.Format(CultureInfo.InvariantCulture, (NoResString)@"
				SELECT
					Bill = CASE WHEN {0} in (@CNF, @WFI) THEN @BillName ELSE @BookingName END + @Delimiter1 + {1} + @Delimiter2 + @ModeName + @Delimiter1 + {2}
				FROM {3}
				JOIN {4} ON {5} = {6}
				WHERE {6} <> @Shipment
					AND {7} = @Y
					AND {8} = @N
					AND {9} in (SELECT {10} FROM {11} WHERE {12} in (SELECT {13} FROM {14} WHERE {15} = @Voyage))
					AND {16} = @ContainerNum AND {16} <> ''
					AND
					(
						{17} = @REL AND {0} in (@CNF, @WFI)
						OR {17} = @BKD AND {0} NOT in (@CNF, @WFI)
					)
				", // SQL Query Statement
				/* 00 */ JobShipmentSchema.Constants.JS_ShipmentStatus,
				/* 01 */ JobShipmentSchema.Constants.JS_UniqueConsignRef,
				/* 02 */ JobContainerSchema.Constants.JC_ContainerMode,
				/* 03 */ JobShipmentSchema.Constants.TableName,
				/* 04 */ JobContainerSchema.Constants.TableName,
				/* 05 */ JobContainerSchema.Constants.JC_JS_FCLBookingOnlyLink,
				/* 06 */ JobShipmentSchema.Constants.PK,
				/* 07 */ JobShipmentSchema.Constants.JS_IsShipping,
				/* 08 */ JobShipmentSchema.Constants.JS_IsCancelled,
				/* 09 */ JobShipmentSchema.Constants.JS_JX,
				/* 10 */ JobSailingSchema.Constants.PK,
				/* 11 */ JobSailingSchema.Constants.TableName,
				/* 12 */ JobSailingSchema.Constants.JX_JA,
				/* 13 */ JobVoyOriginSchema.Constants.PK,
				/* 14 */ JobVoyOriginSchema.Constants.TableName,
				/* 15 */ JobVoyOriginSchema.Constants.JA_JV,
				/* 16 */ JobContainerSchema.Constants.JC_ContainerNum,
				/* 17 */ JobContainerSchema.Constants.JC_Purpose
				);

			var paremeters = new ZSqlParameterCollection();
			paremeters.Add("@CNF", ShipmentStatusList.Codes.Confirmed, JobShipmentSchema.JS_ShipmentStatus);
			paremeters.Add("@WFI", ShipmentStatusList.Codes.WebFwdInstruction, JobShipmentSchema.JS_ShipmentStatus);
			paremeters.Add("@BillName", billName, JobShipmentSchema.JS_GoodsDescription);
			paremeters.Add("@BookingName", bookingName, JobShipmentSchema.JS_GoodsDescription);
			paremeters.Add("@Delimiter1", delimiter1, JobShipmentSchema.JS_GoodsDescription);
			paremeters.Add("@Delimiter2", delimiter2, JobShipmentSchema.JS_GoodsDescription);
			paremeters.Add("@ModeName", modeName, JobShipmentSchema.JS_GoodsDescription);
			paremeters.Add("@Shipment", shipment, JobShipmentSchema.PK);
			paremeters.Add("@Y", true, JobShipmentSchema.JS_IsShipping);
			paremeters.Add("@N", false, JobShipmentSchema.JS_IsCancelled);
			paremeters.Add("@Voyage", voyage, JobVoyOriginSchema.JA_JV);
			paremeters.Add("@ContainerNum", containerNum, JobContainerSchema.JC_ContainerNum);
			paremeters.Add("@REL", ContainerBookedStatus.Codes.Real, JobContainerSchema.JC_Purpose);
			paremeters.Add("@BKD", ContainerBookedStatus.Codes.Booked, JobContainerSchema.JC_Purpose);

			var collection = new DynamicBusinessObjectCollection(Parent.Factory);
			collection.Load(query, paremeters);

			var shipments = new List<ZString>();
			foreach (DynamicBusinessObject row in collection)
			{
				shipments.Add((ZString)row["Bill"]);
			}

			shipments.Sort();

			return ZString.Join(System.Environment.NewLine, shipments.ToArray());
		}

		#endregion

		#region JC_ContainerMode

		protected override void CheckJC_ContainerMode()
		{
			base.CheckJC_ContainerMode();

			MandatoryValidation.CheckEntered(Parent.JC_ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JC_ContainerModeInfo);
		}

		#endregion

		#region JC_SetPointTemp

		protected override void CheckJC_SetPointTemp()
		{
			base.CheckJC_SetPointTemp();
			if (!Parent.JC_IsNonOperativeReefer && Parent.JC_SetPointTemp == 0 && Parent.JC_IsRefrigerated)
			{
				Parent.JC_SetPointTempInfo.AddWarning(Res.GetString("8d8edefc-6c67-4d1d-9b07-6e6e726074f3", "Please enter a temperature."));
			}
		}

		#endregion

		#region JC_ImportReleaseOrderStatus

		public void ValidateJC_ImportReleaseOrderStatus()
		{
			ValidateCalculatedProperty(Parent.JC_ImportReleaseOrderStatusInfo);
		}

		protected virtual void CheckJC_ImportReleaseOrderStatus()
		{
			if (Parent.JC_ImportReleaseOrderStatus == ReleaseImportOrderMessageStatusList.Codes.Rejected)
			{
				Parent.JC_ImportReleaseOrderStatusInfo.AddWarning(Res.GetString("86c90d89-f80e-44d8-a2a5-4e0025c36122", "The last message sent for this container was rejected."));
			}
		}

		#endregion

		#region JC_HumidityPercent

		protected override void CheckJC_HumidityPercent()
		{
			base.CheckJC_HumidityPercent();
			if (!Parent.JC_IsNonOperativeReefer && Parent.JC_IsRefrigerated && Parent.JC_HumidityPercent == 0)
			{
				Parent.JC_HumidityPercentInfo.AddWarning(Res.GetString("4b171415-b9fd-4f94-a2a5-f9aaa733bc2f", "Please enter Humidity Percent."));
			}
		}

		#endregion

		#region JC_AirVentFlow

		protected override void CheckJC_AirVentFlow()
		{
			base.CheckJC_AirVentFlow();
			if (!Parent.JC_IsNonOperativeReefer && Parent.JC_IsRefrigerated && Parent.JC_AirVentFlow == 0)
			{
				Parent.JC_AirVentFlowInfo.AddWarning(Res.GetString("da87ab11-72e3-4ace-84d4-9f757c87fb3e", "Please enter Air Ventilation Setting."));
			}
		}

		#endregion

		#region JC_HarmonisedCode

		protected override void CheckJC_HarmonisedCode()
		{
			base.CheckJC_HarmonisedCode();
			HarmonisedCodeValidator.Validate(Parent.JC_HarmonisedCodeInfo, Parent.IsInDatabase);
		}

		#endregion

		#region Customs Entry Numbers

		public void ValidateCustomsEntryNumberType()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberTypeInfo);
		}

		protected virtual void CheckCustomsEntryNumberType()
		{
		}

		public void ValidateCustomsEntryNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsEntryNumberInfo);
		}

		protected virtual void CheckCustomsEntryNumber()
		{
		}

		#endregion

		#region JC_StowagePosition

		protected override void CheckJC_StowagePosition()
		{
			base.CheckJC_StowagePosition();

			if (!Parent.JC_StowagePosition.IsEmpty)
			{
				if (!Parent.IsTopLevelPack && !Regex.IsMatch(Parent.JC_StowagePosition, "^[0-9]{7,7}$"))
				{
					Parent.JC_StowagePositionInfo.AddWarning(Res.GetString("d6235823-10d4-4712-b597-7e463fafefca", "Stowage Position format should be BBBRRTT where BBB is Bay, RR is Row and TT is Tier. All values should be numeric."));
				}
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJC_ImportReleaseOrderStatus();
			ValidateCustomsEntryNumberType();
			ValidateCustomsEntryNumber();
		}

		#endregion

		#region Implementation

		protected new AgencyShipmentContainer Parent
		{
			get { return (AgencyShipmentContainer)base.Parent; }
		}

		protected override bool IsFcxContainterModeAllowed
		{
			get { return false; }
		}

		#endregion
	}
}



