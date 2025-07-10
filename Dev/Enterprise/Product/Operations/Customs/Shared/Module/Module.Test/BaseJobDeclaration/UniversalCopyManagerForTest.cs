using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalCopy.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	internal class UniversalCopyManagerForTest : UniversalCopyManager
	{
		public UniversalCopyManagerForTest(Type elementType, ZModule module) : base(elementType, module)
		{
		}

		public UniversalCopyManagerForTest(Type elementType, ModuleIdentifier moduleId) : base(elementType, moduleId)
		{
		}

		protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
		{
			throw new NotImplementedException();
		}

		protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
		{
			throw new NotImplementedException();
		}

		protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			scheduleTarget = null;
			return false;
		}

		protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
		{
			throw new NotImplementedException();
		}

		internal BusinessObjectFactory GetDefaultFactoryForCopyManagerExposed()
		{
			return GetDefaultFactoryForCopyManager();
		}

		internal BusinessObject ImportIntoAnotherFactoryExposed(BusinessObject selectedElement, Type elementType, BusinessObjectFactory otherFactory)
		{
			return ImportIntoAnotherFactory(selectedElement, elementType, otherFactory);
		}
	}
}
