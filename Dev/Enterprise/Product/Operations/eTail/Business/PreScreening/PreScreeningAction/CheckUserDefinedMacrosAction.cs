using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Macros;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.Business
{
	class CheckUserDefinedMacrosAction : PreScreeningAction
	{
		public CheckUserDefinedMacrosAction(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}

		protected override void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result)
		{
			var expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(field.MacrosScript, new BusinessObject[] { consignment });

			try
			{
				if (expression.EvaluateDocEngineExpression(RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value))
				{
					AddPreScreeningDetailsCore(field.MessageText, result);
				}
			}
			catch (Exception e)
			{
				AddPreScreeningDetailsCore(e.Message, result, messageLevelOverride: MessageLevel.NotifyOnlyWarning);
			}
		}
	}
}
