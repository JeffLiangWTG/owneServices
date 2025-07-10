using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusSeaManTranHead), "Arrivals")]
	public class CusSeaManArrivalPort : AutoCusSeaManArrivalPort, IStatusNeedsRecalculationProvider, ISendersMessageReferenceProvider
	{
		public CusSeaManArrivalPort(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusSeaManArrivalPortTypeDecider TypeDecider = new CusSeaManArrivalPortTypeDecider();

		public static CusSeaManArrivalPort LoadFromSendersReference(BusinessObjectFactory factory, ZString sendersReference)
		{
			return factory.LoadTop1<CusSeaManArrivalPort>(new ZQuery(Enterprise.ZArchitecture.Schema.CusSeaManArrivalPortSchema.BA_SendersMessageReference, sendersReference));
		}

		public bool IsAnyCargoDischargingHere
		{
			get
			{
				if (!BA_RL_NKArrivalPort.IsEmpty)
				{
					foreach (CusSeaManOBLHeader oceanBill in Header.OceanBills)
					{
						if (oceanBill.BO_RL_NKDischargePort == BA_RL_NKArrivalPort)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		#region Fetch Strategy

		protected class CusSeaManArrivalPortFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public CusSeaManArrivalPortFetchStrategy(CusSeaManArrivalPort port)
				: base(port)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();

				Factory.AddFetchHint(OrgAddressSchema.PK, port.BA_OA_CTOAddress);
				Factory.AddFetchHint(RefUNLOCOSchema.RL_Code, port.BA_RL_NKArrivalPort);
			}

			protected CusSeaManArrivalPort port
			{
				get { return (CusSeaManArrivalPort)base.BusinessObject; }
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusSeaManArrivalPortFetchStrategy(this);
		}

		#endregion

		#region Business Object Overrides

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				BA_SendersMessageReference = ZString.Empty;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("9c21542c-d0a6-47ea-85d9-ca3fe2216d21", "Arrival Port {0}", BA_RL_NKArrivalPort).Trim(); }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Business Objects

		public CusSeaManTranHead Header
		{
			get
			{
				return Factory.Load<CusSeaManTranHead>(BA_BT);
			}
		}

		EDIMessageCollection messages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}

		#endregion

		#region IStatusNeedsRecalculationProvider Members

		public bool StatusNeedsRecalculation
		{
			get
			{
				return Messages.HasChanges;
			}
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(BA_SendersMessageReferenceInfo, Env.NumberFountains.CusSeaManArrivalPortNumber, ignoreInDatabaseCheck: true);
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get { return BA_SendersMessageReference; }
		}

		#endregion
	}
}
