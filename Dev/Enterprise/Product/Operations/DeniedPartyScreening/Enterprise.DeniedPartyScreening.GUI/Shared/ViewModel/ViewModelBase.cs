using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public abstract class ViewModelBase<TViewModel> : INotifyPropertyChanged where TViewModel : class
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void SetValue<T>(Expression<Func<T>> property, T value)
		{
			var propertyName = GetPropertyName(property);
			typeof(TViewModel).GetProperty(propertyName)?.SetValue(this, value);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void NotifyPropertyChanged<T>(Expression<Func<T>> property)
		{
			var propertyName = GetPropertyName(property);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void NotifyPropertyChanged([CallerMemberName] string property = null)
		{
			if (!string.IsNullOrEmpty(property))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}
		}

		string GetPropertyName(LambdaExpression property)
		{
			Argument.NotNull(property, nameof(property));

			if (property.Body is MemberExpression memberExpression)
			{
				if (memberExpression.Member is PropertyInfo propertyInfo)
				{
					if (propertyInfo.GetMethod != null && !propertyInfo.GetMethod.IsStatic)
					{
						return memberExpression.Member.Name;
					}
				}
			}

			throw new MemberAccessException("member not found");
		}
	}
}
