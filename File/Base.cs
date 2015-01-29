using System;

namespace WSD.Collection
{
	// This interface need to be implemented in each platform.
	// But I'll implement it on each an see how to distribute it.
	// This will read a file from URL or local file,
	// for android must detect witch type is, content, file path, etc
	// Each type of file (Image, Binary, Text, Video) will ask for a param
	// with this interface in the constructor
	public interface Base
	{
	}
}

