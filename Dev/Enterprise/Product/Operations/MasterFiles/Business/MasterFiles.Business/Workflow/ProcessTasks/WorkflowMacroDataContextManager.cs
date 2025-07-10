using System;
using CargoWise.EntityFramework;
using Castle.DynamicProxy;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowMacroDataContextManager
	{
		public interface IBusinessObjectEventDataModel<TTriggerSource, TSource>
			where TTriggerSource : BusinessObject
			where TSource : BusinessObject
		{
			TTriggerSource TriggerSource { get; }
			TSource Source { get; }
		}

		class TypedDataModelInterceptor : IInterceptor
		{
			public void Intercept(IInvocation invocation)
			{
				if (invocation.Method.Name == $"get_{nameof(BusinessObjectEventDataModel.TriggerSource)}")
				{
					invocation.ReturnValue = ((BusinessObjectEventDataModel)invocation.Proxy).TriggerSource;
					return;
				}

				if (invocation.Method.Name == $"get_{nameof(BusinessObjectEventDataModel.Source)}")
				{
					invocation.ReturnValue = ((BusinessObjectEventDataModel)invocation.Proxy).Source;
					return;
				}

				invocation.Proceed();
			}
		}

		public static BusinessObjectEventDataModel GetProxyModel(BusinessObjectEventDataModel dataModel)
		{
			Type typeT = dataModel.TriggerSource.GetType();
			Type typeF = dataModel.Source.GetType();
			Type genericInterfaceType = typeof(IBusinessObjectEventDataModel<,>);
			Type specificInterfaceType = genericInterfaceType.MakeGenericType(typeT, typeF);

			var modelProxy = (BusinessObjectEventDataModel)proxyGen.CreateClassProxy(dataModel.GetType(), new[] { specificInterfaceType }, new ProxyGenerationOptions(), new[] { dataModel.TriggerSource }, new TypedDataModelInterceptor());
			foreach (var property in dataModel.GetType().GetProperties())
			{
				if (!property.CanWrite)
				{ continue; }
				property.SetValue(modelProxy, property.GetValue(dataModel));
			}

			return modelProxy;
		}

		public static BusinessObjectEventDataModel GetProxyModel(BusinessObjectEventDataModel dataModel, Type sourceType, Type triggerSourceType)
		{
			Type genericInterfaceType = typeof(IBusinessObjectEventDataModel<,>);
			Type specificInterfaceType = genericInterfaceType.MakeGenericType(triggerSourceType, sourceType);

			return (BusinessObjectEventDataModel)proxyGen.CreateClassProxy(dataModel.GetType(), new[] { specificInterfaceType }, new ProxyGenerationOptions(), new[] { dataModel.TriggerSource }, new TypedDataModelInterceptor());
		}

		[ThreadSafe]
		static readonly ProxyGenerator proxyGen = new ProxyGenerator();
	}
}
