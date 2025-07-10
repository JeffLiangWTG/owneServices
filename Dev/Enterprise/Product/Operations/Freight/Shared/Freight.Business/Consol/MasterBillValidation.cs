using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Validates the master bill number to ensure it passes the following algorithm:
	///  - Value must be 11 digits (not including spaces which are removed prior to validation)
	///  - Ingore the first 3 digits, the next 7 digits % 7 should equal the last digit (11)
	/// </summary>
	public static class MasterBillValidator
	{
		public static class Schema
		{
			public const string DuplicateName = "DuplicateName";
			public const string PK = "PK";
			public const string Result = "Result";
		}

		public static string DuplicateMAWB(BusinessObjectFactory factory, string startMAWB, string endMAWB, string transportMode)
		{
			var result = string.Empty;

			if (startMAWB.Length > 3 && endMAWB.Length > 3)
			{
				const string sql = @"
SELECT JK_PK AS PK, JK_MasterBillNum AS MAWB FROM dbo.JobConsol 
	WHERE
		JK_TransportMode = @ConsolTransportMode AND
		JK_IsCancelled = 0 AND
		JK_MasterBillNum >= @startBillNum1 AND
		JK_MasterBillNum <= @endBillNum1 AND
		JK_SystemCreateTimeUtc >= @maxJobConsolCreateTime

UNION ALL
SELECT JS_PK AS PK, JS_HouseBill AS MAWB FROM dbo.JobShipment
	WHERE
		JS_TransportMode = @ShipmentTransportMode AND
		JS_IsBooking = 1 AND
		JS_IsCancelled = 0 AND
		JS_IsForwardRegistered = 0 AND
		JS_HouseBill >= @startBillNum2 AND
		JS_HouseBill <= @endBillNum2 AND
		JS_SystemCreateTimeUtc >= @maxJobShipmentCreateTime
";

				var sqlParameters = new ZSqlParameterCollection();
				sqlParameters.Add("@ConsolTransportMode", transportMode, JobConsolSchema.JK_TransportMode);
				sqlParameters.Add("@ShipmentTransportMode", transportMode, JobShipmentSchema.JS_TransportMode);
				sqlParameters.Add("@startBillNum1", startMAWB, JobConsolSchema.JK_MasterBillNum);
				sqlParameters.Add("@endBillNum1", endMAWB, JobConsolSchema.JK_MasterBillNum);
				sqlParameters.Add("@startBillNum2", startMAWB, JobShipmentSchema.JS_HouseBill);
				sqlParameters.Add("@endBillNum2", endMAWB, JobShipmentSchema.JS_HouseBill);
				sqlParameters.Add("@maxJobConsolCreateTime", ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value), JobConsolSchema.JK_SystemCreateTimeUtc);
				sqlParameters.Add("@maxJobShipmentCreateTime", ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value), JobShipmentSchema.JS_SystemCreateTimeUtc);

				var collection = new DynamicBusinessObjectCollection(factory);
				collection.Load(sql, sqlParameters);

				if (collection.Count > 0)
				{
					result = collection[0]["MAWB"].ToString();
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL statement, i'm a constant")]
		public static bool IsDuplicate(CommonConsol consol, ZDateTime cutOffStart, ZDateTime cutOffEnd, string mawb = "")
		{
			bool result = false;
			var mawbValue = string.IsNullOrEmpty(mawb) ? consol.JK_MasterBillNum.ToString() : mawb;

			if (consol != null && mawbValue.Length > 3)
			{
				var sql = string.Format(Culture.Invariant, @"
DECLARE @ConsolDuplicate as uniqueidentifier

SET @ConsolDuplicate = ({0})

IF (@ConsolDuplicate IS NOT NULL)
(
	SELECT @ConsolName AS DuplicateName, @ConsolDuplicate AS PK
)
ELSE
(
	SELECT @ShipmentName AS DuplicateName, JS_PK AS PK FROM dbo.JobShipment
		WHERE
		JS_TransportMode = @ShipmentTransportMode AND
		JS_IsBooking = 1 AND
		JS_IsCancelled = 0 AND
		JS_IsForwardRegistered = 0 AND
		JS_HouseBill = @BillNum AND
		JS_SystemCreateTimeUtc > @CutOffStart AND
		JS_SystemCreateTimeUtc < @CutOffEnd
)", GetSqlForCheckDuplicateConsolMAWB(consol.IsCoLoad));

				const string consolNameToken = "Consol";
				const string shipmentNameToken = "Shipment";

				var sqlParameters = new ZSqlParameterCollection();
				sqlParameters.Add("@ExcludeConsolPK", consol.PK, JobConsolSchema.PK);
				sqlParameters.Add("@ConsolTransportMode", consol.JK_TransportMode, JobConsolSchema.JK_TransportMode);
				sqlParameters.Add("@ShipmentTransportMode", consol.JK_TransportMode, JobShipmentSchema.JS_TransportMode);
				sqlParameters.Add("@BillNum", mawbValue, JobShipmentSchema.JS_HouseBill);

				if (consol.IsCoLoad)
				{
					sqlParameters.Add("@Coload", Core.Constants.AgentType.CoLoad, JobConsolSchema.JK_AgentType);
				}

				sqlParameters.Add("@CutOffStart", cutOffStart.IsValidSmallDateTime ? cutOffStart : ZDateTime.MinSmallDateTimeValue, JobConsolSchema.JK_SystemCreateTimeUtc);
				sqlParameters.Add("@CutOffEnd", cutOffEnd.IsValidSmallDateTime ? cutOffEnd : ZDateTime.MaxSmallDateTimeValue, JobConsolSchema.JK_SystemCreateTimeUtc);
				sqlParameters.Add("@ConsolName", consolNameToken, JobShipmentSchema.JS_HouseBill);
				sqlParameters.Add("@ShipmentName", shipmentNameToken, JobShipmentSchema.JS_HouseBill);

				var collection = new DynamicBusinessObjectCollection(consol.Factory);
				collection.Load(sql, sqlParameters);

				if (collection.Count == 1 && (new ZString(collection[0][Schema.DuplicateName]) == consolNameToken))
				{
					result = true;
				}
				else
				{
					foreach (DynamicBusinessObject elem in collection)
					{
						var pk = new ZGuid(elem[Schema.PK]);

						if (!consol.Shipments.Contains(pk))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}

		public static bool IsDuplicate(CommonShipment booking, ZDateTime cutOffStart, ZDateTime cutOffEnd)
		{
			bool result = false;

			if (booking != null && booking.JS_HouseBill.Length > 3)
			{
				const string sql = @"
SELECT Result = 
(
	CASE WHEN 
		EXISTS
		(
			SELECT 
				JK_PK FROM dbo.JobConsol
			WHERE
				JK_TransportMode = @ConsolTransportMode AND
				JK_IsCancelled = 0 AND
				JK_MasterBillNum = @BillNum AND
				JK_SystemCreateTimeUtc > @CutOffStart AND
				JK_SystemCreateTimeUtc < @CutOffEnd
		) 
		OR
		EXISTS
		(
			SELECT 
				JS_PK FROM dbo.JobShipment
			WHERE
				JS_TransportMode = @ShipmentTransportMode AND
				JS_PK != @ExcludeBookingPK AND
				JS_IsBooking = 1 AND
				JS_IsCancelled = 0 AND
				JS_IsForwardRegistered = 0 AND
				JS_HouseBill = @BillNum AND
				JS_SystemCreateTimeUtc > @CutOffStart AND
				JS_SystemCreateTimeUtc < @CutOffEnd
		)
	THEN 1
	ELSE 0
	END
)";
				var sqlParameters = new ZSqlParameterCollection();
				sqlParameters.Add("@ConsolTransportMode", booking.JS_TransportMode, JobConsolSchema.JK_TransportMode);
				sqlParameters.Add("@ShipmentTransportMode", booking.JS_TransportMode, JobShipmentSchema.JS_TransportMode);
				sqlParameters.Add("@ExcludeBookingPK", booking.PK, JobConsolSchema.PK);
				sqlParameters.Add("@BillNum", booking.JS_HouseBill, JobShipmentSchema.JS_HouseBill);
				sqlParameters.Add("@CutOffStart", cutOffStart.IsValidSmallDateTime ? cutOffStart : ZDateTime.MinSmallDateTimeValue, JobConsolSchema.JK_SystemCreateTimeUtc);
				sqlParameters.Add("@CutOffEnd", cutOffEnd.IsValidSmallDateTime ? cutOffEnd : ZDateTime.MaxSmallDateTimeValue, JobConsolSchema.JK_SystemCreateTimeUtc);

				var collection = new DynamicBusinessObjectCollection(booking.Factory);
				collection.Load(sql, sqlParameters);
				result = new ZInt(collection[0][Schema.Result]) == 1;
			}

			return result;
		}

		public static ZString GetMAWBFormatValidMessage(ZString masterBillNumber, BusinessObjectFactory factory)
		{
			var result = "";

			if (masterBillNumber.Length != 11)
			{
				return Res.GetString("4b888a81-5c26-4432-82c9-942468e2c982", "The MAWB should contain 11 digits.");
			}

			var airline = RefAirline.LoadFromAirlinePrefix((factory ?? new BusinessObjectFactory()), masterBillNumber.SubstringSafe(0, 3));

			if (MasterBillNumberContainsInvalidAlphaNumericCharacters(masterBillNumber, airline))
			{
				result = Res.GetString("75cb5445-16b2-459a-9cca-a93856d6c4f4", "The MAWB can only contain numbers.");
			}
			else if (MasterBillNumberContainsInvalidNumericCharacters(masterBillNumber, airline))
			{
				result = Res.GetString("9ecccd5f-06f3-4487-9b74-352d9490cf49", "The MAWB can only contain western Arabic numerals (0-9).");
			}
			else
			{
				var expectedCheckDigit = Convert.ToInt32(masterBillNumber.Substring(3, 7), CultureInfo.InvariantCulture) % 7;
				var actualCheckDigit = Convert.ToInt32(masterBillNumber.Substring(masterBillNumber.Length - 1, 1), CultureInfo.InvariantCulture);

				if (actualCheckDigit != expectedCheckDigit)
				{
					result = Res.GetString("7af97111-7090-479d-9ade-df98a530b6eb", "Invalid check digit. The last digit should be '{0}'", expectedCheckDigit);
				}
			}

			return result;
		}

		static bool MasterBillNumberContainsInvalidAlphaNumericCharacters(ZString masterBillNumber, RefAirline airlineFromMasterBillPrefix)
		{
			if (airlineFromMasterBillPrefix == null || !airlineFromMasterBillPrefix.RM_MembershipFlagIATA)
			{
				return !Regex.IsMatch(masterBillNumber, @"^\d{11}$");
			}

			return !Regex.IsMatch(masterBillNumber.SubstringSafe(3, masterBillNumber.Length), @"^\d{8}$");
		}

		static bool MasterBillNumberContainsInvalidNumericCharacters(ZString masterBillNumber, RefAirline airlineFromMasterBillPrefix)
		{
			if (airlineFromMasterBillPrefix == null || !airlineFromMasterBillPrefix.RM_MembershipFlagIATA)
			{
				return !Regex.IsMatch(masterBillNumber, @"^[0-9]{11}$");
			}

			return !Regex.IsMatch(masterBillNumber.SubstringSafe(3, masterBillNumber.Length), @"^[0-9]{8}$");
		}

		public static void ValidateMAWB(IMAWBAllocationParent parent, Func<bool> duplicateCheck)
		{
			if (parent == null)
			{
				return;
			}

			if (duplicateCheck != null && duplicateCheck())
			{
				parent.MasterBillMAWBInfo.AddError(Res.GetString("e6ff0109-c9f0-4a09-bb73-54463aad6d2b", "This Master Bill Number already exists on another Consol, Shipment or Booking.\r\nPlease select another number."));
				return;
			}

			var mawbInStock = parent.MAWBAllocation.FreightJobMawbLink.LoadByAirlineAndMawbNo(parent.MasterBillAirlinePrefix, parent.MasterBillMAWB, ZDateTime.UtcNow.AddMonths(-FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));

			if (mawbInStock != null)
			{
				//if carrier mawb is borrowed out then error
				if (!mawbInStock.JM_OH_AllocatedTo.IsEmpty && mawbInStock.JM_OH_AllocatedTo.IsValid)
				{
					parent.MasterBillMAWBInfo.AddError(Res.GetString("f2ac1840-0ce0-46cd-87ea-5f4ecc2e6423", "MAWB has been 'borrowed out' to a customer. Please enter another number."));
					return;
				}

				//if carrier number entered and in stock as a neutral - then error
				if (!parent.IsNeutralMaster && !mawbInStock.JM_IsPaper)
				{
					parent.MasterBillMAWBInfo.AddError(Res.GetString("3713fe44-9b1c-4277-bc45-7799ad9df6e9", "This Master Bill Number is already in stock.\r\nIt is flagged as a Neutral Number.\r\nPlease enter another number."));
					return;
				}
			}

			var warning = GetMAWBFormatValidMessage(String.Concat(parent.MasterBillAirlinePrefix, parent.MasterBillMAWB), parent.Factory);
			if (warning != ZString.Empty)
			{
				parent.MasterBillMAWBInfo.AddWarning(warning);
			}
		}

		public static void ValidateNeutralMAWB(IMAWBAllocationParent parent)
		{
			if (parent.IsNeutralMaster)
			{
				var stockThreshold = parent.MAWBAllocation.MAWBStockManagementStrategy.GetBranchStrategy(GlbBranch.CurrentBranch, parent.MasterBillAirlinePrefix).StockThreshold;
				int noOfMawbsAvailable = parent.MAWBAllocation.FreightJobMawbLink.NoJobMawbsAvailable(parent.MasterBillAirlinePrefix, parent.MasterBillMAWB, new ZString[] { parent.AWBServiceLevel, OrgCarrierServiceLevel.AllCode });

				if (noOfMawbsAvailable == 0)
				{
					parent.MasterBillNeutralMAWBInfo.AddWarning(Res.GetString("69b55a1d-a8cc-4e41-9f59-a6bd2a212180", "There are no MAWBs left for this Airline. Please add more numbers to your stock."));
				}
				else if (noOfMawbsAvailable == 1 && stockThreshold > 1)
				{
					parent.MasterBillNeutralMAWBInfo.AddWarning(Res.GetString("c8377d85-9da2-4703-a39d-ab2f984b60be", "There is only 1 MAWB left for this Airline. Please add more numbers to your stock."));
				}
				else if (noOfMawbsAvailable < stockThreshold)
				{
					parent.MasterBillNeutralMAWBInfo.AddWarning(Res.GetString("ffe7aad4-aae6-487e-806a-b13023b3a4f6", "There are only {0} MAWBs left for this Airline. Please add more numbers to your stock.", noOfMawbsAvailable));
				}
			}
		}

		public static string GetSqlForCheckDuplicateConsolMAWB(ZBool isCoLoad)
		{
			var agentTypeCriteria = isCoLoad ? "JK_AgentType <> @Coload" : "JK_AgentType <> ''";

			return string.Format(Culture.Invariant, @"
					SELECT Top 1 JK_PK FROM dbo.JobConsol 
					WHERE
					JK_PK != @ExcludeConsolPK AND
					JK_TransportMode = @ConsolTransportMode AND
					JK_IsCancelled = 0 AND
					JK_MasterBillNum = @BillNum AND
					{0} AND
					JK_SystemCreateTimeUtc > @CutOffStart AND
					JK_SystemCreateTimeUtc < @CutOffEnd", agentTypeCriteria);
		}
	}
}
