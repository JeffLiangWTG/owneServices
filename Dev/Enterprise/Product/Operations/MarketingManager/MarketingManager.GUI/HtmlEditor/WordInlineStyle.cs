using System.Collections.Specialized;

namespace Enterprise.MarketingManager.GUI
{
	class WordInlineStyle
	{
		/// <summary>
		/// The restricted styles
		/// </summary>
		static readonly StringCollection RestrictedStyles = new StringCollection { "font-face" };

		/// <summary>
		/// Initializes a new instance of the <see cref="WordInlineStyle"/> class.
		/// </summary>
		/// <param name="fullName">The full name.</param>
		/// <param name="content">The content.</param>
		public WordInlineStyle(string fullName, string content)
		{
			FullName = fullName.Trim();
			Content = content.Trim();

			// Extract class name
			var nameParts = FullName.Split('.');
			Valid = nameParts.Length <= 2;
			if (Valid)
			{
				// Extract element name like p, table, span etc.
				TagName = nameParts[0].Trim();
			}

			Valid = !RestrictedStyles.Contains(TagName);
		}

		/// <summary>
		/// Gets the name of the tag.
		/// </summary>
		/// <value>The name of the tag.</value>
		string TagName { get; set; }

		/// <summary>
		/// Gets the content.
		/// </summary>
		/// <value>The content.</value>
		public string Content { get; private set; }

		/// <summary>
		/// Gets the full name.
		/// </summary>
		/// <value>The full name.</value>
		string FullName { get; set; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="WordInlineStyle"/> is valid.
		/// </summary>
		/// <value><c>true</c> if valid; otherwise, <c>false</c>.</value>
		public bool Valid { get; private set; }
	}
}
