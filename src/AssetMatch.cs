using System.Collections.Generic;
/// <summary>
/// 资源匹配
/// </summary>
public class AssetMatch {
    /// <summary>
    /// 视频
    /// </summary>
    public AssetFile video;

    /// <summary>
    /// 目标字幕
    /// </summary>
    public AssetFile subtitle;

    /// <summary>
    /// 可选字幕列表
    /// </summary>
    public List<AssetFile> subtitleList;
}