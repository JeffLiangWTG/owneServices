using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using Microsoft.BizTalk.Message.Interop;


namespace CargoWise.eHub.Core.Transforms.Helper
{
    public class ContextAccessor
    {
        public virtual string GetContextProperty(string contextItemName, string contextItemNamespace)
        {
            return GetContextProperty(contextItemName, contextItemNamespace, "");
        }

        public virtual string GetContextProperty(string contextItemName, string contextItemNamespace, string defaultValue)
        {
            try
            {
                IBaseMessageContext messageContext = GetMessageContext();
                if (messageContext == null)
                {
                    return defaultValue;
                }

                return (messageContext.Read(contextItemName, contextItemNamespace) ?? defaultValue).ToString();
            }
            catch
            {
                return defaultValue;
            }
        }

        public virtual void SetContextProperty(string contextItemName, string contextItemNamespace, string replacementValue)
        {
            IBaseMessageContext messageContext = GetMessageContext();
            if (messageContext != null)
            {
                messageContext.Write(contextItemName, contextItemNamespace, replacementValue);
            }
        }

        IBaseMessageContext GetMessageContext()
        {
            if (testingContext != null)
            {
                return testingContext;
            }

            return MultipleTransformationComponent.context;
        }

        private static TestingMessageContext testingContext;

        public void SetTestingMessageContext(TestingMessageContext ctx)
        {
            testingContext = ctx;
        }
    }
}

namespace CargoWise.eHub.Core.Transforms.Helper.Testing
{
    using System.Collections.Generic;

    public class TestingMessageContext : IBaseMessageContext
    {
        private List<ContextProperty> MessageContext;

        public TestingMessageContext()
        {
            MessageContext = new List<ContextProperty>();
        }

        public object Read(string strName, string strNamespace)
        {
            foreach (var contextItem in MessageContext)
            {
                if (HasContextItem(strName, strNamespace, contextItem))
                {
                    return contextItem.Value;
                }
            }

            return null;
        }

        public void Write(string strName, string strNameSpace, object obj)
        {
            foreach (var contextItem in MessageContext)
            {
                if (HasContextItem(strName, strNameSpace, contextItem))
                {
                    contextItem.Value = obj;
                    return;
                }
            }

            MessageContext.Add(new ContextProperty(strName, strNameSpace, obj));
        }

        bool HasContextItem(string contextItemName, string contextItemNamespace, ContextProperty contextItem)
        {
            return contextItem.Name == contextItemName && contextItem.Namespace == contextItemNamespace;
        }

        public void AddPredicate(string strName, string strNameSpace, object obj)
        {
            throw new System.NotImplementedException();
        }

        public uint CountProperties
        {
            get { throw new System.NotImplementedException(); }
        }

        public ContextPropertyType GetPropertyType(string strName, string strNameSpace)
        {
            throw new System.NotImplementedException();
        }

        public bool IsPromoted(string strName, string strNameSpace)
        {
            throw new System.NotImplementedException();
        }

        public void Promote(string strName, string strNameSpace, object obj)
        {
            throw new System.NotImplementedException();
        }

        public object ReadAt(int index, out string strName, out string strNamespace)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ContextProperty
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public object Value { get; set; }

        public ContextProperty(string contextName, string contextNamespace, object contextValue)
        {
            Name = contextName;
            Namespace = contextNamespace;
            Value = contextValue;
        }
    }
}