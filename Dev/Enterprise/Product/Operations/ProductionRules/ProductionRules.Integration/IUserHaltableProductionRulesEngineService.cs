using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Integration
{
	public interface IUserHaltableProductionRulesEngineService
	{
		void RunRulesEngine(BusinessObjectFactory factory, INotifications notifications, RulesContextType contextType, Func<IEnumerable<IInputFact>> getFacts, Func<ProductionRulesEngineResult, INotification> processResults);
		void RunRulesEngine(BusinessObjectFactory factory, INotifications notifications, RulesContextType contextType, ProductionRuleSetFilter filters, Func<IEnumerable<IInputFact>> getFacts, Func<ProductionRulesEngineResult, INotification> processResults);
		void RunRulesEngine(BusinessObjectFactory factory, INotifications notifications, RulesContextType contextType, ProductionRuleSetFilter filters, Func<IEnumerable<IEnumerable<IInputFact>>> getFactBatches, Func<ProductionRulesEngineResult, INotification> processResults);
	}
}
