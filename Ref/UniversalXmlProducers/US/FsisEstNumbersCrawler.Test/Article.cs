using System;

namespace FsisEstNumbersCrawler.Test
{
	public class Article
	{
		public int Id { get; set; }

		public string Author { get; set; }

		public string Title { get; set; }

		public string Content { get; set; }

		public DateTime Time { get; set; }

		public DateTime UnformattedTime { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Article other && other.Id == Id && other.Author == Author && other.Title == Title && other.Content == Content && other.Time == Time && other.UnformattedTime == UnformattedTime;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
