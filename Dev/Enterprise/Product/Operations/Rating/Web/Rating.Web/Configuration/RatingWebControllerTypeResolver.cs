using System;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Enterprise.Rating.Web.Configuration
{
	public class RatingWebControllerTypeResolver : DefaultHttpControllerTypeResolver
	{
		public RatingWebControllerTypeResolver() : base(IsRatingControllerType) { }

		public static bool IsRatingControllerType(Type t)
		{
			if (
					t != null
					&& t.IsClass
					&& t.IsVisible
					&& !t.IsAbstract
					&& typeof(IHttpController).IsAssignableFrom(t)
					&& t.Namespace.StartsWith($"{nameof(Enterprise)}.{nameof(Rating)}.{nameof(Web)}") // namespace comparison
				)
			{
				return HasValidControllerName(t);
			}

			return false;
		}

		static bool HasValidControllerName(Type controllerType)
		{
			string controllerSuffix = DefaultHttpControllerSelector.ControllerSuffix;
			if (controllerType.Name.Length > controllerSuffix.Length)
			{
				return controllerType.Name.EndsWith(controllerSuffix, StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}
	}
}
