using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	public abstract class NewExportAWBHeaderTest<T> : TestCaseWithFactory where T : ExportAWBHeader
	{
		public void TestAgentDetailsAreDefaultedFromRegistryButAreModifiableWhenAGTSegmentIsNotMandatory()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "7654321";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "FRED FLINTSTONE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "BEDROCK";

			AWBParent.IsAWBValuesOverriddenProperty = true;
			var awbHeader = AWBHeader;
			awbHeader.EH_AirportOfDestinationText = "ALBEQUERQUE";

			CombineAssertions(delegate
			{
				AssertEquals("awbHeader.EH_GB_UserBranch should be filled in when the registry is on", GlbBranch.CurrentBranch.PK, awbHeader.EH_GB_UserBranch);
				AssertEquals("awbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", awbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("awbHeader.EH_AgentAccountNo", "7654321", awbHeader.EH_AgentAccountNo);
				AssertEquals("awbHeader.EH_AgentName", "FRED FLINTSTONE", awbHeader.EH_AgentName);
				AssertEquals("awbHeader.EH_AgentPlace", "BEDROCK", awbHeader.EH_AgentPlace);

				AssertEquals("awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", false, awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentAccountNoInfo.ReadOnly", false, awbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentNameInfo.ReadOnly", false, awbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentPlaceInfo.ReadOnly", false, awbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			Factory.Save();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "7654328";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "1357924";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "BART SIMPSON";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "SPRINGFIELD";

			var reloadedAwbHeader = new BusinessObjectFactory().Load<T>(awbHeader.PK);
			CombineAssertions(delegate
			{
				AssertEquals("reloadedAwbHeader.EH_GB_UserBranch still filled in", GlbBranch.CurrentBranch.PK, reloadedAwbHeader.EH_GB_UserBranch);
				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", reloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNo", "7654321", reloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("reloadedAwbHeader.EH_AgentName", "FRED FLINTSTONE", reloadedAwbHeader.EH_AgentName);
				AssertEquals("reloadedAwbHeader.EH_AgentPlace", "BEDROCK", reloadedAwbHeader.EH_AgentPlace);

				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", false, reloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly", false, reloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentNameInfo.ReadOnly", false, reloadedAwbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly", false, reloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			var rereloadedAwbHeader = new BusinessObjectFactory().Load<T>(awbHeader.PK);

			CombineAssertions(delegate
			{
				AssertEquals("rereloadedAwbHeader.EH_GB_UserBranch STILL filled in", GlbBranch.CurrentBranch.PK, rereloadedAwbHeader.EH_GB_UserBranch);
				AssertEquals("rereloadedAwbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", rereloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("rereloadedAwbHeader.EH_AgentAccountNo", "7654321", rereloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("rereloadedAwbHeader.EH_AgentName", "FRED FLINTSTONE", rereloadedAwbHeader.EH_AgentName);
				AssertEquals("rereloadedAwbHeader.EH_AgentPlace", "BEDROCK", rereloadedAwbHeader.EH_AgentPlace);

				AssertEquals("rereloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", false, rereloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly", false, rereloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentNameInfo.ReadOnly", false, rereloadedAwbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly", false, rereloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			awbHeader.Populate();
			AssertEquals("awbHeader.EH_AgentIATACodeFormatted", "76-5 4328", awbHeader.EH_AgentIATACodeFormatted);
			AssertEquals("awbHeader.EH_AgentAccountNo", "1357924", awbHeader.EH_AgentAccountNo);
			AssertEquals("awbHeader.EH_AgentName", "BART SIMPSON", awbHeader.EH_AgentName);
			AssertEquals("awbHeader.EH_AgentPlace", "SPRINGFIELD", awbHeader.EH_AgentPlace);

			awbHeader.EH_GB_UserBranch = ZGuid.Empty;
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "666";
			awbHeader.Populate();
			AssertEquals("awbHeader.EH_AgentIATACode", "7654328", awbHeader.EH_AgentIATACode);

			awbHeader.EH_GB_UserBranch = GlbBranch.CurrentBranch.PK;
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "777";
			awbHeader.Populate();
			AssertEquals("awbHeader.EH_AgentIATACode", "777", awbHeader.EH_AgentIATACode);

			ZString currentUserCode = GlbStaff.CurrentUser.GS_Code;

			try
			{
				GlbStaff.CurrentUser.GS_Code = User.ServiceUserCode;
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "888";
				awbHeader.Populate();
				AssertEquals("awbHeader.EH_AgentIATACode", "777", awbHeader.EH_AgentIATACode);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_Code = currentUserCode;
			}
		}

		public void TestAgentDetailsComeFromRegistryWhenAGTSegmentIsMandatory()
		{
			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "7654321";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "FRED FLINTSTONE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "BEDROCK";

			AWBParent.IsAWBValuesOverriddenProperty = true;
			var awbHeader = AWBHeader;
			awbHeader.EH_AirportOfDestinationText = "ALBEQUERQUE";

			CombineAssertions(delegate
			{
				AssertEquals("awbHeader.EH_GB_UserBranch should be empty by default", ZGuid.Empty, awbHeader.EH_GB_UserBranch);
				AssertEquals("awbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", awbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("awbHeader.EH_AgentAccountNo", "7654321", awbHeader.EH_AgentAccountNo);
				AssertEquals("awbHeader.EH_AgentName", "FRED FLINTSTONE", awbHeader.EH_AgentName);
				AssertEquals("awbHeader.EH_AgentPlace", "BEDROCK", awbHeader.EH_AgentPlace);

				AssertEquals("awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", true, awbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentAccountNoInfo.ReadOnly", true, awbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentNameInfo.ReadOnly", true, awbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("awbHeader.EH_AgentPlaceInfo.ReadOnly", true, awbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			Factory.Save();

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "7654328";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "1357924";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "BART SIMPSON";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "SPRINGFIELD";

			var reloadedAwbHeader = new BusinessObjectFactory().Load<T>(awbHeader.PK);
			CombineAssertions(delegate
			{
				AssertEquals("reloadedAwbHeader.EH_GB_UserBranch still empty", ZGuid.Empty, reloadedAwbHeader.EH_GB_UserBranch);
				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormatted", "76-5 4328", reloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNo", "1357924", reloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("reloadedAwbHeader.EH_AgentName", "BART SIMPSON", reloadedAwbHeader.EH_AgentName);
				AssertEquals("reloadedAwbHeader.EH_AgentPlace", "SPRINGFIELD", reloadedAwbHeader.EH_AgentPlace);

				AssertEquals("reloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", true, reloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly", true, reloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentNameInfo.ReadOnly", true, reloadedAwbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("reloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly", true, reloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly);
			});

			ForwardingConfigurationRegistry.Instance.AllowFWBWithoutAGTSegment.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);

			var rereloadedAwbHeader = new BusinessObjectFactory().Load<T>(awbHeader.PK);

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "1234567/0000";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "7654321";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "FRED FLINTSTONE";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "BEDROCK";

			CombineAssertions(delegate
			{
				AssertEquals("rereloadedAwbHeader.EH_GB_UserBranch STILL empty", ZGuid.Empty, rereloadedAwbHeader.EH_GB_UserBranch);
				AssertEquals("rereloadedAwbHeader.EH_AgentIATACodeFormatted", "12-3 4567/0000", rereloadedAwbHeader.EH_AgentIATACodeFormatted);
				AssertEquals("rereloadedAwbHeader.EH_AgentAccountNo", "7654321", rereloadedAwbHeader.EH_AgentAccountNo);
				AssertEquals("rereloadedAwbHeader.EH_AgentName", "FRED FLINTSTONE", rereloadedAwbHeader.EH_AgentName);
				AssertEquals("rereloadedAwbHeader.EH_AgentPlace", "BEDROCK", rereloadedAwbHeader.EH_AgentPlace);

				AssertEquals("rereloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly", true, rereloadedAwbHeader.EH_AgentIATACodeFormattedInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly", true, rereloadedAwbHeader.EH_AgentAccountNoInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentNameInfo.ReadOnly", true, rereloadedAwbHeader.EH_AgentNameInfo.ReadOnly);
				AssertEquals("rereloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly", true, rereloadedAwbHeader.EH_AgentPlaceInfo.ReadOnly);
			});
		}

		public void TestEH_ParentIDIsNotPresetInForwardingExportAWBHeaderSubClasses()
		{
			var header = GetNewAWBHeader();
			AssertEquals("header.EH_ParentID should be blank upon instantiation", ZGuid.Empty, header.EH_ParentID);
		}

		public void TestSourceIdentifier()
		{
			var header = GetNewAWBHeader();
			Assert(header is ISourceIdentifierProvider);

			var bizO = header as BusinessObject;
			AssertEquals(false, bizO.IsInDatabase);
			AssertEquals((header as ISourceIdentifierProvider).SourceIdentifier, header.EH_ParentID);

			bizO.FillWithValidTestData();
			bizO.Factory.Save();
			AssertEquals(true, bizO.IsInDatabase);
			AssertEquals((header as ISourceIdentifierProvider).SourceIdentifier, header.PK);
		}

		#region Other Charges

		public abstract void TestIncludeCharges();
		public abstract void TestSplitCharges();
		public abstract void TestGroupExcessiveCharges();
		public abstract void TestMaxOtherChargesOnGroupExcessiveCharges();
		public abstract void TestMergeWithExistingOtherCharges();

		#endregion

		#region Job & Freight Charges

		public void TestGetFreightCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;

			var freightChargeCode = Factory.New<AccChargeCode>();
			freightChargeCode.AC_IATA_ChargeCodeMap = Constants.AWB.ChargeCodes.AC;

			var notAFreightChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			notAFreightChargeCode.AC_Code = "NOT";

			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			var freightCharge1 = CreateShipmentCharge(shipment, null, freightChargeCode, 11.0m, 0m);
			var freightCharge2 = CreateShipmentCharge(shipment, null, freightChargeCode, 1252.0m, 0m);
			var notAFreightCharge = CreateShipmentCharge(shipment, null, notAFreightChargeCode, 11.0m, 0m);

			var freightCharges = AWBHeader.GetFreightCharges(jobHeader);

			CombineAssertions("Only freight charges should be in this list", () =>
			{
				AssertNotNull(freightCharges);
				AssertEquals(2, freightCharges.Length);

				AssertCollectionContains(freightCharge1, freightCharges);
				AssertCollectionContains(freightCharge2, freightCharges);
				AssertCollectionNotContains(notAFreightCharge, freightCharges);
			});
		}

		#endregion

		#region Implementation

		protected abstract BooleanRegistryItem GroupOtherChargesByIataCodeRegistry { get; }

		protected string[] GetChargesDescriptions()
		{
			return AWBHeader.AWBOtherCharges
				.Cast<ExportAWBOtherCharges>()
				.Select(charge => string.Format("{0}|{1}|{2}|{3}", charge.EO_ChargeCode, charge.EO_EntitlementCode, charge.EO_PPDCLT, charge.EO_Amount))
				.ToArray();
		}

		protected JobCharge CreateShipmentCharge(ForwardingShipment shipment, BusinessObject consolCost, AccChargeCode chargeCode, ZDecimal sell, ZDecimal cost, OrgHeader sellAccount = null, AccTaxRate rate = null)
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AC = chargeCode != null ? chargeCode.PK : ZGuid.Empty;
			charge.JR_JH = shipment.Job.PK;
			charge.JR_E6 = consolCost != null ? (ZGuid)consolCost[JobConsolCostSchema.PK] : ZGuid.Empty;
			charge.JR_OH_SellAccount = sellAccount != null ? sellAccount.PK : ZGuid.Empty;
			charge.JR_LocalSellAmt = sell;
			charge.JR_OSSellAmt = sell;

			if (rate != null)
			{
				charge.JR_AT_SellGSTRate = rate.PK;
			}

			charge.JR_LocalCostAmt = cost;
			charge.JR_OSCostAmt = cost;

			return charge;
		}

		protected JobHeader CreateShipmentHeader(ForwardingShipment shipment)
		{
			if (shipment.Job == null)
			{
				JobHeader jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
				jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
				var localCharges = shipment.Factory.NewWithValidTestData<OrgHeader>();
				localCharges.OH_Code = "CLIENTORG";
				jobHeader.LocalChargesPK = localCharges.PK;

				return jobHeader;
			}
			return null;
		}

		protected JobCharge[] GetShipmentCharges(ForwardingShipment shipment)
		{
			return Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
		}

		protected AccChargeCode ChargeCodeAS
		{
			get { return chargeCodeAS ?? (chargeCodeAS = CreateChargeCode(Core.Constants.AWB.ChargeCodes.AS)); }
		}
		AccChargeCode chargeCodeAS;

		protected AccChargeCode ChargeCodeAC
		{
			get { return chargeCodeAC ?? (chargeCodeAC = CreateChargeCode(Core.Constants.AWB.ChargeCodes.AC)); }
		}
		AccChargeCode chargeCodeAC;

		protected T AWBHeader
		{
			get
			{
				if (awbHeader == null)
				{
					SetupHeaderAndParent();
				}

				return awbHeader;
			}
		}
		T awbHeader;

		protected IAWBParent AWBParent
		{
			get
			{
				if (awbParent == null)
				{
					SetupHeaderAndParent();
				}

				return awbParent;
			}
		}
		IAWBParent awbParent;

		void SetupHeaderAndParent()
		{
			var parent = GetNewParent();

			awbHeader = GetNewAWBHeader();
			awbHeader.EH_ParentID = parent.PK;
			awbHeader.EH_Table = parent.TableName;

			awbParent = (IAWBParent)parent;
		}

		protected void ResetHeaderAndParent()
		{
			awbHeader = null;
			awbParent = null;
		}

		T GetNewAWBHeader()
		{
			return Factory.New<T>();
		}

		protected abstract BusinessObject GetNewParent();

		protected AccChargeCode CreateChargeCode(ZString iataChargeCode)
		{
			AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_Code = "X" + iataChargeCode;
			charge.AC_IATA_ChargeCodeMap = iataChargeCode;
			return charge;
		}

		protected void LoadOrCreateNewTariff(string dataGrouping, string tariffCode, string nkTariffType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, nkTariffType);
			var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		#endregion
	}
}
