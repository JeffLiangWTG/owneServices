using System;
using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class AntlrMacroContext : IAntlrMacroContext
	{
		public AntlrMacroContext(object parent, Type parentType, IMacroLibrary[] libraries, Dictionary<string, (object, Type)> variables, Func<string, string> errorMessageExtender = null)
		{
			if (parent == null && parentType == null)
			{
				throw new ArgumentNullException(nameof(parent), "Parent and ParentType cannot both be null");
			}
			Parent = parent;
			ParentType = parentType ?? parent.GetType();
			Libraries = libraries ?? Array.Empty<IMacroLibrary>();
			Variables = variables;
			ErrorMessageExtender = errorMessageExtender;
		}

		public object Parent { get; }
		public Type ParentType { get; }
		public IMacroLibrary[] Libraries { get; }
		public Dictionary<string, (object, Type)> Variables { get; }

		IMacroScope scope;

		public IMacroScope Scope {
			get
			{
				if (scope == null)
				{
					scope = new MacroScope(Parent);
					if (Variables != null)
					{
						foreach (var entry in Variables)
						{
							scope.SetVariable(entry.Key, entry.Value.Item1);
						}
					}
				}
				return scope;
			}
		}

		public Func<string, string> ErrorMessageExtender { get; }

		#region IDisposable

		public void Dispose()
		{
			scope?.Dispose();
		}

		#endregion
	}
}
