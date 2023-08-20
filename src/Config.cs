using System.Collections;
using System.Collections.Generic;

public class Config {
    /// <summary>
    /// 完全匹配
    /// </summary>
    public PerfectMatch perfectMatch;

    /// <summary>
    /// 视频文件类型列表
    /// </summary>
    public readonly List<string> videoFileTypeList = new List<string>();

    /// <summary>
    /// 字幕文件类型列表
    /// </summary>
    public readonly List<string> subtitleFileTypeList = new List<string>();


    /// <summary>
    /// 视频文件忽略列表
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public readonly List<string> videoFileIgnoreRegList = new List<string>();

    /// <summary>
    /// 完全匹配
    /// </summary>
    public class PerfectMatch {
        /// <summary>
        /// 是否启用完全匹配
        /// </summary>
        public bool enabled;

        /// <summary>
        /// 最小要求的字符数
        /// </summary>
        public int minCharCount;
    }
}