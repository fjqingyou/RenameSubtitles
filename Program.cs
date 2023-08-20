using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace RenameSubtitles {
    class Program {
        static void Main(string[] args) {
            //配置文件 - 当前测试先写在这里，后续要做成读取外部配置文件的方式
            Config config = new Config();
            config.perfectMatch = new Config.PerfectMatch();
            config.perfectMatch.enabled = true;
            config.perfectMatch.minCharCount = 2;

            //视频文件忽略的部分
            config.videoFileIgnoreRegList.Add(" Vol.");
            config.videoFileIgnoreRegList.Add(" SP ");
            config.videoFileIgnoreRegList.Add(" NCOP ");
            config.videoFileIgnoreRegList.Add(" Menu[_]?[\\d]*? ");

            //视频文件类型
            config.videoFileTypeList.Add("mkv");
            config.videoFileTypeList.Add("mp4");

            //字幕文件
            config.subtitleFileTypeList.Add("ass");
            config.subtitleFileTypeList.Add("ssa");
            config.subtitleFileTypeList.Add("srt");

            //正式执行
            RenameSubtitle renameSubtitle = new RenameSubtitle(config);
            renameSubtitle.DoWork(args);
        }
    }
}
