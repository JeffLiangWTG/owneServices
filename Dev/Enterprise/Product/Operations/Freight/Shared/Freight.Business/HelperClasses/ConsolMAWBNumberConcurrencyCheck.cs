using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolMAWBNumberConcurrencyCheck : ConcurrencyChecker
	{
		readonly HashSet<(ZGuid consolPk, ConsolMAWBNumberConcurrencyCheckerParam param)> _mawbNumberToCheck = new();
		readonly int recycleMonths = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;

		ConsolMAWBNumberConcurrencyCheck(BusinessObjectFactory factory) : base(factory)
		{
		}

		public static void Register(BusinessObjectFactory factory, string mawbNumber, ZGuid consolPk, string transportMode, ZBool isCoLoad, ZDateTime createTime, bool isImportingData)
		{
			if (!consolPk.IsValid || isImportingData)
			{
				return;
			}

			var concurrencyCheck = GetRegisteredConsolConcurrencyCheck(factory);
			if (concurrencyCheck == null)
			{
				concurrencyCheck = new ConsolMAWBNumberConcurrencyCheck(factory);
				factory.SaveInTransactionActions.Add(concurrencyCheck);
			}

			if (consolPk.IsValid && concurrencyCheck._mawbNumberToCheck.Any(x => x.consolPk == consolPk))
			{
				concurrencyCheck._mawbNumberToCheck.RemoveWhere(x => x.consolPk == consolPk);
			}

			if (mawbNumber.IsNullOrEmpty() || mawbNumber.Length <= 3 || transportMode.IsNullOrEmpty() || !isCoLoad.IsValid)
			{
				return;
			}

			var param = new ConsolMAWBNumberConcurrencyCheckerParam
			{
				MawbNumber = mawbNumber, TransportMode = transportMode, IsCoLoad = isCoLoad, CreateTime = createTime
			};

			concurrencyCheck._mawbNumberToCheck.Add((consolPk, param));
		}

		static ConsolMAWBNumberConcurrencyCheck GetRegisteredConsolConcurrencyCheck(BusinessObjectFactory factory)
		{
			return factory?.SaveInTransactionActions.OfType<ConsolMAWBNumberConcurrencyCheck>().FirstOrDefault();
		}

		public static bool IsRegistered(BusinessObjectFactory factory, string mawbNumber, ZGuid consolPk, string transportMode, ZBool isCoLoad, ZDateTime createTime)
		{
			var param = new ConsolMAWBNumberConcurrencyCheckerParam
			{
				MawbNumber = mawbNumber, TransportMode = transportMode, IsCoLoad = isCoLoad, CreateTime = createTime
			};
			return !mawbNumber.IsNullOrEmpty()
					&& consolPk.IsValid
					&& !transportMode.IsNullOrEmpty()
					&& isCoLoad.IsValid
					&& GetRegisteredConsolConcurrencyCheck(factory) is ConsolMAWBNumberConcurrencyCheck concurrencyCheck
					&& concurrencyCheck._mawbNumberToCheck.Any(x => x.consolPk == consolPk && x.param.GetHashCode() == param.GetHashCode());
		}

		protected override void SaveInTransactionCore() => RunConcurrencyCheck();

		void RunConcurrencyCheck()
		{
			foreach (var (consolPk, param) in _mawbNumberToCheck)
			{
				var consol = Factory.Load<CommonConsol>(consolPk);
				if (consol == null || consol.IsDeleted)
				{
					continue;
				}

				var cutOffStart = param.CreateTime.IsValid
					? param.CreateTime.AddMonths(-recycleMonths)
					: ZDateTime.UtcNow.AddMonths(-recycleMonths);
				var cutOffEnd = param.CreateTime.IsValid ? param.CreateTime.AddMonths(recycleMonths) : ZDateTime.Empty;

				var sql = MasterBillValidator.GetSqlForCheckDuplicateConsolMAWB(param.IsCoLoad);

				var sqlParams = new ZSqlParameterCollection();

				sqlParams.Add("@BillNum", param.MawbNumber, JobShipmentSchema.JS_HouseBill);
				sqlParams.Add("@ExcludeConsolPK", consolPk, JobConsolSchema.PK);
				sqlParams.Add("@ConsolTransportMode", param.TransportMode, JobConsolSchema.JK_TransportMode);

				if (param.IsCoLoad)
				{
					sqlParams.Add("@Coload", Core.Constants.AgentType.CoLoad, JobConsolSchema.JK_AgentType);
				}

				sqlParams.Add("@CutOffStart",
					cutOffStart.IsValidSmallDateTime ? cutOffStart : ZDateTime.MinSmallDateTimeValue,
					JobConsolSchema.JK_SystemCreateTimeUtc);
				sqlParams.Add("@CutOffEnd",
					cutOffEnd.IsValidSmallDateTime ? cutOffEnd : ZDateTime.MaxSmallDateTimeValue,
					JobConsolSchema.JK_SystemCreateTimeUtc);

				var collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load(sql, sqlParams);

				if (collection.Count > 0)
				{
					var message = Res.GetString("5cdd3384-f2db-43df-b37a-899d22a0414b", "Another user has made changes that conflicts with your own changes. You must reopen this form again before saving.");
					var heading = Res.GetString("f1138cb5-52b5-452d-8581-4201ecec34ca", "MAWB number concurrency error, you must reopen this form again before saving");

					throw new ZCannotSaveException(message, heading, shouldReprocess: true);
				}
			}

			_mawbNumberToCheck.Clear();
		}
	}

	public class ConsolMAWBNumberConcurrencyCheckerParam
	{
		public string MawbNumber { get; set; }
		public string TransportMode { get; set; }
		public ZBool IsCoLoad { get; set; }
		public ZDateTime CreateTime { get; set; }

		public override int GetHashCode() => MawbNumber.GetHashCode() ^ TransportMode.GetHashCode() ^ IsCoLoad.GetHashCode() ^ CreateTime.GetHashCode();
	}
}
