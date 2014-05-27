using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HighlighterLib.Templating
{
    public class SolutionFolder
    {
        public string FolderName { get; private set; }
        public bool IsProject { get; private set; }
        public IEnumerable<SolutionFolder> SubFolders { get; private set; }
        public IEnumerable<SolutionFile> SolutionFiles { get; private set; }

        public SolutionFolder(string folderName, bool isProject, IEnumerable<SolutionFolder> subFolders, IEnumerable<SolutionFile> solutionFiles)
        {
            FolderName = folderName;
            IsProject = isProject;
            SubFolders = subFolders;
            SolutionFiles = solutionFiles;
        }
    }
}