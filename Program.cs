using System;
using System.IO;

namespace RenameSubtitles{
    class Program{
        static void Main(string[] args){
            //配置文件 - 当前测试先写在这里，后续要做成读取外部配置文件的方式
            Config config = new Config();
            config.perfectMatch = new Config.PerfectMatch();
            config.perfectMatch.enabled = true;
            config.perfectMatch.count = 2;

            //正式执行
            RenameSubtitle renameSubtitle = new RenameSubtitle(config);
            renameSubtitle.doWork(args);
        }
    }
}
