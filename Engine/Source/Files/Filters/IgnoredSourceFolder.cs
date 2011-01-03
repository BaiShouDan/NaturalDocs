/* 
 * Class: GregValure.NaturalDocs.Engine.Files.Filters.IgnoredSourceFolder
 * ____________________________________________________________________________
 * 
 * An ignored source folder.  If a folder matches this one or is contained within it, it will be ignored.
 */

// This file is part of Natural Docs, which is Copyright © 2003-2011 Greg Valure.
// Natural Docs is licensed under version 3 of the GNU Affero General Public License (AGPL)
// Refer to License.txt for the complete details


using System;


namespace GregValure.NaturalDocs.Engine.Files.Filters
	{
	public class IgnoredSourceFolder : Filter
		{
		
		// Group: Functions
		// __________________________________________________________________________
		
		
		/* Constructor: IgnoredSourceFolder
		 * Creates a new filter from the passed <Path>.
		 */
		public IgnoredSourceFolder (Path path)
			{
			ignoredPath = path;
			}

		public override bool IgnoreSourceFolder (Path path)
			{
			return (ignoredPath == path || ignoredPath.Contains(path));
			}

			
		
		// Group: Variables
		// __________________________________________________________________________
		
		
		/* var: ignoredPath
		 * The <Path> to be ignored.  Must be absolute.
		 */
		protected Path ignoredPath;
		
		}
	}