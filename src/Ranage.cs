public class Ranage {
    /// <summary>
    /// 范围的索引开始
    /// </summary>
    public int start;

    /// <summary>
    /// 范围的索引结束
    /// </summary>
    public int end;

    /// <summary>
    /// 范围的权重
    /// </summary>
    public int weight;

    /// <summary>
    /// 此范围的文本
    /// </summary>
    public string text;

    /// <summary>
    /// 这个范围的资源文件
    /// </summary>
    public AssetFile assetFile;

    /// <summary>
    /// 获取范围的长度
    /// </summary>
    public int Length => end - start;
}