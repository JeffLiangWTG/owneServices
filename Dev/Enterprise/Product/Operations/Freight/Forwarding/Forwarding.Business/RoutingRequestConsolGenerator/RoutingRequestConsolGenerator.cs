using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	/// Given a list of Routes, generates a number of Consols for each.
	/// </summary>
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public class RoutingRequestConsolGenerator : AutoRoutingRequestConsolGenerator, IDefaultNumberOfDecimalsSupporter
	{
		public RoutingRequestConsolGenerator(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection.Factory)
		{
			this.multiDaysSelection = multiDaysSelection;
			this.multiDaysSelection.OnFlightNumberChanged += MultiDaysSelection_OnFlightNumberChanged;

			FlightNumber = this.multiDaysSelection.GetFlightNumber();
		}

		public MultiDaysSelection MultiDaysSelection => multiDaysSelection;
		readonly MultiDaysSelection multiDaysSelection;

		#region Properties

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Accessors have attributes")]
		public override ZBool AllocateNeutralMaster
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.AllocateNeutralMaster; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.AllocateNeutralMaster = value; }
		}

		[MeasureUnit(Schema.WeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal Weight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Weight; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Weight = this.GetRoundedValue(WeightInfo, value); }
		}

		[List("Lookups.WeightUnitList")]
		public override ZString WeightUnit
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.WeightUnit; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.WeightUnit = value;
				this.SetRoundedValue(WeightInfo);
			}
		}

		[MeasureUnit(Schema.VolumeUnit, MeasureUnitType.Volume)]
		public override ZDecimal Volume
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Volume; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Volume = this.GetRoundedValue(VolumeInfo, value); }
		}

		[List("Lookups.VolumeUnitList")]
		public override ZString VolumeUnit
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.VolumeUnit; }
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.VolumeUnit = value;
				this.SetRoundedValue(VolumeInfo);
			}
		}

		#region FlightNumber

		int FlightNumber { get; set; }

		void MultiDaysSelection_OnFlightNumberChanged(object sender, FlightNumberEventArgs e)
		{
			FlightNumber = e.FlightNumber;
			TotalConsolsInfo.RefreshBinding();
		}

		#endregion

		#region TotalConsols

		public override ZInt TotalConsols => FlightNumber * ConsolsPerFlight;

		#endregion

		#region ServiceLevel

		[List("NeutralAirWaybillServiceLevelList")]
		public override ZString ServiceLevel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.ServiceLevel;
			}
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.ServiceLevel = value;
			}
		}

		public OrgCarrierServiceLevelCollection NeutralAirWaybillServiceLevelList
		{
			get
			{
				if (fCarrierServiceLevels == null)
				{
					ResetCarrierServiceLevels();
				}

				var lCarrierServiceLevels = fCarrierServiceLevels;
				lCarrierServiceLevels.Load();
				fCarrierServiceLevels = lCarrierServiceLevels;
				return fCarrierServiceLevels;
			}
		}

		void ResetCarrierServiceLevels()
		{
			fCarrierServiceLevels = JobMawb.GetNeutralAirWaybillServiceLevelsFrom2LetterCode(Factory, GlbBranch.CurrentBranch,
				AirlinePrefix.IsEmpty
				? MultiDaysSelection.FirstCarrier
				: AirlinePrefix);
		}

		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#region AirlinePrefix

		public override ZString AirlinePrefix
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.AirlinePrefix;
			}
			set
			{
				base.AirlinePrefix = value;
				ResetCarrierServiceLevels();
			}
		}

		#endregion

		#endregion

		#region Lookups

		public RoutingRequestConsolGeneratorLookups Lookups
		{
			get { return lookups ?? (lookups = new RoutingRequestConsolGeneratorLookups(this)); }
		}
		RoutingRequestConsolGeneratorLookups lookups;

		#endregion

		public void GenerateConsols(IReadOnlyList<JobSailingCollection> sailingList, Action<ICommonConsol, IJobSailing> action = null)
		{
			if (sailingList == null)
			{
				return;
			}

			foreach (JobSailingCollection sailings in sailingList)
			{
				if (sailings == null)
				{
					continue;
				}

				var origin = GetSailingsOrigin(sailings);
				var destination = GetSailingsDestination(sailings);
				if (CTOCutOff.IsValid)
				{
					sailings[0].Origin.JA_CutOff = sailings[0].Origin.JA_E_DEP - CTOCutOff.ToTimeSpan();
				}
				if (CFSCutOff.IsValid)
				{
					sailings[0].JX_DepotCutOff = sailings[0].Origin.JA_E_DEP - CFSCutOff.ToTimeSpan();
				}

				// Each route (header) has a corresponding consol.
				// Each consol has a number of corresponding transport legs (lines).
				for (int i = 0; i < ConsolsPerFlight; i++)
				{
					var consol = GetNewConsolWithTransports(sailings);
					using (consol.GetValidationSuspender())
					{
						consol.JK_RL_NKLoadPort = origin;
						consol.JK_RL_NKDischargePort = destination;
						consol.JK_IsNeutralMaster = AllocateNeutralMaster;
						consol.JK_TransportMode = Constants.TransportModes.Air;
						consol.JK_AWBServiceLevel = ServiceLevel;

						var airlinePrefix = AirlinePrefix.IsEmpty
							? sailings[0].JX_JV_VoyageFlight.SubstringSafe(0, AutoRoutingRequestConsolGenerator.Schema.AirlinePrefixMaxLength)
							: AirlinePrefix;

						var airline = RefAirline.LoadFromAirline2LetterCode(Factory, airlinePrefix);
						if (airline != null)
						{
							consol.JK_MasterBillNum = airline.RM_EagleAddedAirlinePrefixOrAccountingCode;
							consol.SetShippingLineBaseOnAirline(airline);
						}

						if (sailings.Any())
						{
							action?.Invoke(consol, sailings[0]);
						}
					}

					consol.JK_TotalShipmentChargeableUnit = WeightUnit;
					consol.JK_TotalShipmentActWeightCheck = Weight;

					consol.JK_TotalShipmentActOtherUnit = VolumeUnit;
					consol.JK_TotalShipmentActVolumeCheck = Volume;

					if (Shipments > 0)
					{
						consol.JK_TotalShipmentCountCheck = Shipments;
					}

					if (Chargeable > 0m)
					{
						consol.JK_TotalShipmentChargableCheck = Chargeable;
					}

					consol.Validation.ValidateJK_MasterBillNum();
				}
			}
		}

		ZString GetSailingsOrigin(JobSailingCollection sailings)
		{
			return sailings[0].JX_JA_RL_NKPortOfLoading;
		}

		ZString GetSailingsDestination(JobSailingCollection sailings)
		{
			var lastSailingIndex = sailings.Count - 1;
			return sailings[lastSailingIndex].JX_JB_RL_NKPortOfDischarge;
		}

		CommonConsol GetNewConsolWithTransports(JobSailingCollection sailings)
		{
			var consol = GetNewConsol();

			for (int index = 0; index < sailings.Count; index++)
			{
				var sailing = sailings[index];
				if ((consol.Transports.Count == 1) && (index == 0))
				{
					CopySailingToTransport(consol.Transports[0], sailing);
					continue;
				}

				var newTransport = consol.Transports.AddNew();
				CopySailingToTransport(newTransport, sailing);
			}

			return consol;
		}

		void CopySailingToTransport(Transport transport, JobSailing sailing)
		{
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
		}

		#region IDefaultNumberOfDecimalsSupporter Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return Constants.TransportModes.Air; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.Weight:
					unitOfMeasure = WeightUnit;
					break;

				case Schema.Volume:
					unitOfMeasure = VolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		protected ZDecimal GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			// not required, transport mode is always AIR
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WeightUnit = Env.Registry.FreightWeightUnit;
			VolumeUnit = Env.Registry.FreightVolumeUnit;
		}

		protected CommonConsol GetNewConsol()
		{
			var result = CreateNewConsol();

			multiDaysSelection.CreatedConsols.Add(result);

			return result;
		}

		protected CommonConsol CreateNewConsol()
		{
			CommonConsol consol;
			Factory.SuspendValidation();

			try
			{
				consol = Factory.New<IForwardingConsol>() as CommonConsol;
			}
			finally
			{
				Factory.ResumeValidation();
			}

			return consol;
		}

		#endregion
	}
}

