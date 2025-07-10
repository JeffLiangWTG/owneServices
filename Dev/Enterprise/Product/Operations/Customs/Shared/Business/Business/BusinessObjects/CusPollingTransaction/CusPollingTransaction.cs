using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	public class CusPollingTransaction : AutoCusPollingTransaction,
		Integration.Customs.ICusPollingTransaction
	{
		public CusPollingTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class ApplicationCodes
		{
			public const string TRCustoms = "TRC";
			public const string KRCustoms = "KRC";
			public const string CHCustomsPassar = "CHP";
			public const string CHCustomsCharteraOutput = "CHO";
		}

		public static readonly CusPollingTransactionTypeDecider TypeDecider = new CusPollingTransactionTypeDecider();

		#region ParentObject

		public BusinessObject ParentObject
		{
			get
			{
				if (parentObject == null)
				{
					if (RegisteredLinkedObjectTypes.TryGetValue(CPT_ParentTableCode, out var typeFunc))
					{
						var bizoType = typeFunc.Invoke();
						parentObject = Factory.Load(bizoType, CPT_ParentID);
					}
				}
				return parentObject;
			}
		}

		BusinessObject parentObject;

		#endregion

		protected Dictionary<string, Func<Type>> RegisteredLinkedObjectTypes => registeredLinkedObjectTypes ??= GetRegisterLinkedObjectTypes();
		Dictionary<string, Func<Type>> registeredLinkedObjectTypes;

		Dictionary<string, Func<Type>> GetRegisterLinkedObjectTypes() => new()
		{
			{ CusEntryHeaderSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.ICusEntryHeader> },
			{ CusExitReportSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.EUExitControl.ICusExitReport> },
			{ EDIMessageSchema.Constants.Prefix, ObjectFactory.GetType<Messaging.Integration.IEDIMessage> },
			{ GlbCompanySchema.Constants.Prefix, ObjectFactory.GetType<MasterFiles.Integration.IGlbCompany> },
			{ CusInBondHeaderSchema.Constants.Prefix, ObjectFactory.GetType<Integration.Customs.EU.NCTS.ICusInBondHeader> },
			{ EDIInterchangeSchema.Constants.Prefix, ObjectFactory.GetType<Messaging.Integration.IEDIInterchange> },
		};
	}
}
