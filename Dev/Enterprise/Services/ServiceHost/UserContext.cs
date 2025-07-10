using System;
using System.ServiceModel;

using Enterprise.Environment;

namespace Enterprise.Services.ServiceHost
{
	public abstract class UserContext : IDisposable
	{
		protected void GenerateContext(Guid branch, Guid department, Guid? staff = null)
		{
			if (userContext == null)
			{
				if (staff == null)
				{
					var name = GetPrimaryIdentityName();
					userContext = Env.SetTemporaryUserContext(name, branch, department);
				}
				else
				{
					userContext = Env.SetTemporaryUserContext(staff.Value, branch, department);
				}
			}
			else
			{
				throw new InvalidOperationException("Context has already been established");
			}
		}

		protected virtual string GetPrimaryIdentityName()
		{
			return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (userContext != null)
			{
				userContext.Dispose();
				userContext = null;
			}
		}

		IDisposable userContext;

		#endregion
	}
}
