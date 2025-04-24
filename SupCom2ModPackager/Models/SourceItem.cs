using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupCom2ModPackager.Models
{
    public enum SourceType
    {
        GoogleDrive,
        LocalFolder,
    }

    public class SourceItem
    {
        public SourceType SourceType { get; set; } = SourceType.GoogleDrive;
        public string FriendlyName { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
    }
}
