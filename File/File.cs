using System;

namespace WSD.Data
{
	/**
	 * Monkey patch, dunno why cannot reference rest if data is referenced in other projects 
	 */
	public class File : WSD.Rest.File
	{
		public File (string name, string type, byte[] data) : base (name, type, data)
		{

		}
	}
}

