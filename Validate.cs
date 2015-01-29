using System;
using System.Text.RegularExpressions;

namespace WSD.Data
{
	public class Validate
	{
		object value;

		public Validate(object value) {
			this.value = value;
		}

		public Validate NotNull(string message)
		{
			if (String.IsNullOrEmpty (string.Format("{0}", value)))
				throw new Exception (message);

			return this;
		}

		public Validate Equal(object value, string message)
		{
			if (String.IsNullOrEmpty (string.Format("{0}", value)))
				throw new Exception (message);

			if (!this.value.Equals(value))
				throw new Exception (message);

			return this;
		}

		public Validate Email(string message)
		{
			if (String.IsNullOrEmpty (string.Format("{0}", value)))
				throw new Exception (message);

			try {
				if (Regex.IsMatch(string.Format("{0}", value),
					@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
					@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
					RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))) {

					return this;
				}
			}
			catch (RegexMatchTimeoutException) {
			}

			throw new Exception (message);
		}

		public Validate MinLength (int minLen, string message)
		{
			if (String.IsNullOrEmpty (string.Format("{0}", value)))
				throw new Exception (message);

			if (value.ToString().Length < minLen)
				throw new Exception (message);

			return this;
		}

		public Validate MaxLength (int maxLen, string message)
		{
			if (String.IsNullOrEmpty (string.Format("{0}", value)))
				throw new Exception (message);

			if (value.ToString().Length > maxLen)
				throw new Exception (message);

			return this;
		}
	}
}

