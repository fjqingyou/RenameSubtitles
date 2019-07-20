using System;
using System.IO;

namespace RenameSubtitles{
    class Program{
        static void Main(string[] args){
            RenameSubtitle renameSubtitle = new RenameSubtitle();
            renameSubtitle.doWork(args);
        }
    }
}
