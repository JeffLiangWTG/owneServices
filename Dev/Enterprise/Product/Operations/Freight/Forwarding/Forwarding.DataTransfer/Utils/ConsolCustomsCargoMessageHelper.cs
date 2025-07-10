using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ConsolCustomsCargoMessageHelper
	{
		public ConsolCustomsCargoMessageHelper(ForwardingConsol consol, INotifications notifications)
			: this(consol, notifications, true)
		{
		}

		public ConsolCustomsCargoMessageHelper(ForwardingConsol consol, INotifications notifications, ZBool isGlobalRun)
		{
			Consol = Argument.NotNull(consol, "Forwarding Consol should not be null");
			Notifications = Argument.NotNull(notifications, "INotifications should not be null");

			IsGlobalRun = isGlobalRun;
		}

		#region Create Cargo Job

		public void CreateConsolCargoJobWithSACFlag()
		{
			var action = new Action(() =>
			{
				var poster = (ICusPoster<ForwardingConsol>)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICusPoster>());
				poster?.PostAndSetSACFlag(Consol);
			});

			if (!IsGlobalRun)
			{
				if (Consol.IsImport() && (AutomaticallyCreateAirCargoJob || AutomaticallyCreateSeaCargoJob))
				{
					action();
				}
			}
			else
			{
				CustomsCargoAction(action);
			}
		}

		bool AutomaticallyCreateAirCargoJob => Consol.IsAir && SystemDataRegistry.Instance.AutomaticallyCreateAirCargoJob.Value;
		bool AutomaticallyCreateSeaCargoJob => Consol.IsSea && SystemDataRegistry.Instance.AutomaticallyCreateSeaCargoJob.Value;

		#endregion

		#region Send Cargo Message

		public void SendConsolCargoMessage(bool saveFactory = true)
		{
			var action = new Action(() =>
			{
				ICargoMessageProcessor processor;
				ICargoMessageProcessorJob job;
				switch (Consol.JK_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						processor = GetCargoProcessor<Enterprise.Integration.Customs.AU.IHouseBillsCargoMessageProcessor>();
						job = GetCargoProcessorJob<Enterprise.Integration.Customs.AU.IAirCargoMessageProcessorJob>();
						break;
					case Core.Constants.TransportModes.Sea:
						processor = GetCargoProcessor<Enterprise.Integration.Customs.AU.ISeaCargoMessageProcessor>();
						job = GetCargoProcessorJob<Enterprise.Integration.Customs.AU.ISeaCargoMessageProcessorJob>();
						break;
					default:
						return;
				}

				try
				{
					Notifications.Notify(new InfoNotification(Res.GetString("0af31327-ea80-4ef0-ad84-73e816ed94c6", "Destination port clearance process")));
					processor.Process(job, saveFactory);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					Notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("adf970a9-b3b6-4eb3-a63d-7878731a8dfe", "Exception occurred while trying to send Cargo Message:\r\n{0}", ex.Message)));
				}
				finally
				{
					job.Dispose();
				}
			});

			if (!IsGlobalRun)
			{
				if (Consol.IsImport() && (AutomaticallySendAirCargoMessage || AutomaticallySendSeaCargoMessage))
				{
					action();
				}
			}
			else
			{
				CustomsCargoAction(action);
			}
		}

		bool AutomaticallySendAirCargoMessage => Consol.IsAir && SystemDataRegistry.Instance.AutomaticallySendAirCargoMessage.Value;
		bool AutomaticallySendSeaCargoMessage => Consol.IsSea && SystemDataRegistry.Instance.AutomaticallySendSeaCargoMessage.Value;

		#endregion

		#region Implementation

		#region Customs Cargo Action

		void CustomsCargoAction(Action action)
		{
			if (Consol.IsImport())
			{
				var branch = GetBranchToProcessCargo();
				if (branch != null)
				{
					using (branch.SetAsTemporaryContext())
					{
						action();
					}
				}
			}
		}

		#endregion

		#region Get Branch To Process Cargo

		GlbBranch GetBranchToProcessCargo()
		{
			ZGuid selectedCompanyPK = ZGuid.Empty;
			int companiesWithCertificatesCount = 0;

			var companies = GetActiveAustralianCompanies();
			if (companies.Length == 1)
			{
				selectedCompanyPK = companies[0].PK;
			}
			else
			{
				foreach (var australianCompany in companies)
				{
					var companyCustomsKeyData = Env.Registry.RawRegistry.AUCCompanyCertificateData.GetValueWithoutFallback(australianCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

					if (companyCustomsKeyData != null)
					{
						companiesWithCertificatesCount++;
						selectedCompanyPK = australianCompany.PK;
						if (australianCompany.PK == GlbCompany.CurrentCompany.PK)
						{
							break;
						}
					}
				}

				if (companiesWithCertificatesCount == 0)
				{
					Notifications.Notify(new InfoNotification(Res.GetString("643bf4e8-0294-46d7-bcfc-73ba4b8d4a2f", "No AU company is set up with CMR company key file - No message sent.")));
				}
				else if (companiesWithCertificatesCount > 1 && selectedCompanyPK != GlbCompany.CurrentCompany.PK)
				{
					selectedCompanyPK = ZGuid.Empty;
					Notifications.Notify(new InfoNotification(Res.GetString("08c63109-a093-4726-9fe1-7ebedf099753", "Multiple AU companies are set up with CMR company key files - No message sent.")));
				}
			}

			return selectedCompanyPK.IsEmpty ? null : FindClosestBranch(selectedCompanyPK, Consol.JK_RL_NKDischargePort);
		}

		#endregion

		#region Find Closest Branch

		GlbBranch FindClosestBranch(ZGuid companyPK, ZString closestPort)
		{
			var query = new ZDBOnlyQuery(typeof(GlbBranch));
			query.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			query.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, companyPK);
			query.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, closestPort);
			query.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, SQLComparisonOperator.NotEqual, null);
			var result = Factory.LoadTop1<GlbBranch>(query);

			if (result == null)
			{
				var extraPortsQuery = new ZDBOnlySubQuery(typeof(GlbBranchExtraPorts), GlbBranchExtraPortsSchema.GY_GB);
				extraPortsQuery.AddToFilter(JoinCondition.And, GlbBranchExtraPortsSchema.GY_RL_NKAdditionalBranchRelatedPort, closestPort);
				query.AddSubQuery(extraPortsQuery, JoinCondition.Or);
				result = Factory.LoadTop1<GlbBranch>(query);

				if (result == null)
				{
					var anyBranchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, companyPK);
					anyBranchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);
					result = Factory.LoadTop1<GlbBranch>(anyBranchQuery);
				}
			}

			return result;
		}

		#endregion

		GlbCompany[] GetActiveAustralianCompanies()
		{
			var loader = new GlbCompany.Loader(Factory);
			return loader.LoadCompanies(Core.Constants.CountryCodes.Australia, true);
		}

		ICargoMessageProcessor GetCargoProcessor<T>()
		{
			return ObjectFactory.New<T>(Notifications) as ICargoMessageProcessor;
		}

		ICargoMessageProcessorJob GetCargoProcessorJob<T>()
		{
			return ObjectFactory.New<T>(Consol) as ICargoMessageProcessorJob;
		}

		BusinessObjectFactory Factory => Consol.Factory;

		readonly ForwardingConsol Consol;
		readonly INotifications Notifications;
		readonly ZBool IsGlobalRun;

		#endregion
	}
}

