using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(GlbPortDeliveryTimeSchema.Constants.G1_RL_NKDischargePort), DescriptionProperty(GlbPortDeliveryTimeSchema.Constants.G1_RL_NKDestinationPort)]
	public class GlbPortDeliveryTime : AutoGlbPortDeliveryTime
	{
		public GlbPortDeliveryTime(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			G1_GC_Company = Env.CurrentCompany.PK;
		}

		#region Properties

		[List("Lookups.JobMode")]
		public override ZString G1_JobMode
		{
			get
			{
				return base.G1_JobMode;
			}
			set
			{
				base.G1_JobMode = value;
			}
		}

		[List("Lookups.FreightMode")]
		public override ZString G1_FreightMode
		{
			get
			{
				return base.G1_FreightMode;
			}
			set
			{
				base.G1_FreightMode = value;
			}
		}

		[List("Lookups.DischargePorts")]
		public override ZString G1_RL_NKDischargePort
		{
			get
			{
				return base.G1_RL_NKDischargePort;
			}
			set
			{
				base.G1_RL_NKDischargePort = value;
			}
		}

		[List("Lookups.DestinationPorts")]
		public override ZString G1_RL_NKDestinationPort
		{
			get
			{
				return base.G1_RL_NKDestinationPort;
			}
			set
			{
				base.G1_RL_NKDestinationPort = value;
			}
		}

		[List("Lookups.Consignees")]
		public override ZGuid G1_OH_ClientOverride
		{
			get
			{
				return base.G1_OH_ClientOverride;
			}
			set
			{
				base.G1_OH_ClientOverride = value;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("A7A25796-AAE5-4077-AF58-B99EB8EE9940", "Port Delivery Time - {0}", CalculateShortcutName());

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory, RefUNLOCO dischargePort, RefUNLOCO destinationPort, ZString freightMode, ZBool isForwarding) : this(factory, dischargePort, destinationPort, null, freightMode, isForwarding)
			{ }

			public Loader(BusinessObjectFactory factory, RefUNLOCO dischargePort, RefUNLOCO destinationPort, OrgHeader client, ZString freightMode, ZBool isForwarding) : base(factory)
			{
				fClient = client;
				fDischargePort = dischargePort != null ? dischargePort.RL_Code : ZString.Empty;
				fDestinationPort = destinationPort != null ? destinationPort.RL_Code : ZString.Empty;
				fFreightMode = freightMode;
				fIsForwarding = isForwarding;
			}

			public GlbPortDeliveryTime Load()
			{
				GlbPortDeliveryTime fGlbPortDeliveryTime = null;
				if (!fDischargePort.IsEmpty && !fDestinationPort.IsEmpty && !fFreightMode.IsEmpty)
				{
					ZString jobMode = fIsForwarding ? "FWD" : "CUS";

					if (fClient != null)
					{
						fGlbPortDeliveryTime = (GlbPortDeliveryTime)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetFilterWithClient(fFreightMode, jobMode));
						if (fGlbPortDeliveryTime == null && (fFreightMode == Enterprise.Core.Constants.ContainerModes.LCL
							|| fFreightMode == Enterprise.Core.Constants.ContainerModes.FCL))
						{
							fGlbPortDeliveryTime = (GlbPortDeliveryTime)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetFilterWithClient(Enterprise.Core.Constants.TransportModes.Sea, jobMode));
						}
					}
					if (fGlbPortDeliveryTime == null)
					{
						fGlbPortDeliveryTime = (GlbPortDeliveryTime)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetFilter(fFreightMode, jobMode));
						if (fGlbPortDeliveryTime == null)
						{
							if (fFreightMode == Enterprise.Core.Constants.ContainerModes.LCL
								|| fFreightMode == Enterprise.Core.Constants.ContainerModes.FCL)
							{
								fGlbPortDeliveryTime = (GlbPortDeliveryTime)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetFilter(Enterprise.Core.Constants.TransportModes.Sea, jobMode));
							}
						}
					}
				}
				return fGlbPortDeliveryTime;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(GlbPortDeliveryTime);
			}

			ZQuery GetFilter(ZString freightMode, ZString jobMode)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDischargePort, fDischargePort);
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDestinationPort, fDestinationPort);
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_FreightMode, freightMode);
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_OH_ClientOverride, null);

				ZQuery jobModeFilter = new ZQuery(GlbPortDeliveryTimeSchema.G1_JobMode, jobMode);
				jobModeFilter.AddToFilter(JoinCondition.Or, GlbPortDeliveryTimeSchema.G1_JobMode, SQLComparisonOperator.Equal, "ALL");
				jobModeFilter.AddToFilter(JoinCondition.Or, GlbPortDeliveryTimeSchema.G1_JobMode, SQLComparisonOperator.Equal, "");

				filter.AddToFilter(jobModeFilter);

				filter.OrderBy = GlbPortDeliveryTimeSchema.G1_JobMode.Name + OrderByClause.Descending;

				return filter;
			}

			ZQuery GetFilterWithClient(ZString freightMode, ZString jobMode)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDischargePort, fDischargePort);
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDestinationPort, fDestinationPort);
				filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_FreightMode, freightMode);

				ZQuery jobModeFilter = new ZQuery(GlbPortDeliveryTimeSchema.G1_JobMode, jobMode);
				jobModeFilter.AddToFilter(JoinCondition.Or, GlbPortDeliveryTimeSchema.G1_JobMode, SQLComparisonOperator.Equal, "ALL");
				jobModeFilter.AddToFilter(JoinCondition.Or, GlbPortDeliveryTimeSchema.G1_JobMode, SQLComparisonOperator.Equal, "");

				filter.AddToFilter(jobModeFilter);

				if (fClient != null)
				{
					filter.AddToFilter(GlbPortDeliveryTimeSchema.G1_OH_ClientOverride, fClient.PK);
				}
				filter.OrderBy = GlbPortDeliveryTimeSchema.G1_JobMode.Name + OrderByClause.Descending;

				return filter;
			}

			readonly ZString fDischargePort;
			readonly ZString fDestinationPort;
			readonly ZString fFreightMode;
			readonly ZBool fIsForwarding;
			readonly OrgHeader fClient;
		}

		#endregion
	}
}
