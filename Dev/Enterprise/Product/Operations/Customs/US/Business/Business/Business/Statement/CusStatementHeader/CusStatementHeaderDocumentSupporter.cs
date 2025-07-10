using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	class CusStatementHeaderDocumentSupporter : DocumentSupporter
	{
		public CusStatementHeaderDocumentSupporter(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		CusStatementHeader StatementHeader
		{
			get { return (CusStatementHeader)base.BusinessObject; }
		}

		StatementMessageHeader MessageHeader
		{
			get
			{
				if (fMessageHeader == null)
				{
					var messages = StatementHeader.Messages.Cast<MQEDIMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc);
					if (StatementHeader.IsMonthlyStatement)
					{
						fMessageHeader = GetMonthlyStatement(messages);
					}
					else
					{
						fMessageHeader = GetDailylyStatement(messages);
					}
				}
				return fMessageHeader;
			}
		}

		StatementMessageHeader GetMonthlyStatement(IEnumerable<MQEDIMessage> messages)
		{
			var result = messages.Where(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement
							&& ((IABIControlMessageBlockB)x.MessageBlock.B).StatementStatus == "F").Select(x => new MonthlyStatementMessageHeader(StatementHeader, x)).FirstOrDefault() ?? messages.Where(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatement)
					.Select(x => new MonthlyStatementMessageHeader(StatementHeader, x)).FirstOrDefault();
			return result;
		}

		StatementMessageHeader GetDailylyStatement(IEnumerable<MQEDIMessage> messages)
		{
			var result = messages.Where(x => (x.EM_MessageType == ApplicationIdentifierCodeList.Codes.DailyStatement || x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.DailyStatement)
							&& ((IABIControlMessageBlockB)x.MessageBlock.B).StatementStatus == "F").Select(x => new DailyStatementMessageHeader(StatementHeader, x)).FirstOrDefault() ?? messages.Where(x => x.EM_MessageType == ApplicationIdentifierCodeList.Codes.DailyStatement || x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.DailyStatement)
					.Select(x => new DailyStatementMessageHeader(StatementHeader, x)).FirstOrDefault();
			return result;
		}

		StatementMessageHeader fMessageHeader;

		#region Overrides

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return System.Array.Empty<Core.Constants.DataContext>();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CustomsStatementHdr; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return Core.Constants.CountryCodes.UnitedStates;

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = base.GetContactOrganisation(menuName, contact, direction);
			if (contact == ContactType.Consignee)
			{
				result = new OrgHeaderContact(StatementHeader.Importer, null);
			}

			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			BusinessObject messageHeader = MessageHeader;
			return new IBODocDataProvider[] { BODocDataProvider.Get(messageHeader ?? StatementHeader) };
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion
	}
}
