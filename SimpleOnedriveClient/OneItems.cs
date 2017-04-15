using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOnedriveClient
{
    public class MyFile
    {
        public string name { get; set; }
        // 以下のプロパティは今回使用しませんが、デバッグ時に値を見ることをお勧めします。
        public string webUrl { get; set; }
        public string createdDateTime { get; set; }
        public string lastModifiedDateTime { get; set; }
    }

    public class MyFiles
    {
        public List<MyFile> value;
    }

    // ファイル移動時に使います。
    public class MyParentFolder
    {
        public string path { get; set; }
    }

    public class MyFileModify
    {
        public string name { get; set; }
        // ファイル移動時に使います。
        public MyParentFolder parentReference { get; set; }
    }

    
}
