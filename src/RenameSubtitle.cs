using System;
using System.IO;
using System.Collections.Generic;

public class RenameSubtitle{
    private static List<string> videoFileTypeList = new List<string>();
    private static List<string> subtitleFileTypeList = new List<string>();
    private string targetDirectory;
    private List<AssetFile> videoFileList = new List<AssetFile>();
    private List<AssetFile> subtitleFileList = new List<AssetFile>();
    private List<AssetMatch> assetMatchList = new List<AssetMatch>();

    //范围的权重
    private List<int> videoFileRangeWeightList = new List<int>();
    private List<int> subtitleFileRangeWeightList = new List<int>();
    private Config config;
    static RenameSubtitle(){
        videoFileTypeList.Add("mp4");
        videoFileTypeList.Add("mkv");

        subtitleFileTypeList.Add("ass");
    }

    /// <summary>
    /// 重命名字幕文件构造函数
    /// </summary>
    /// <param name="config">配置</param>
    public RenameSubtitle(Config config){
        this.config = config;
    }

    public void doWork(string [] args){
        try{
            //等待调试器 - for debug
            // this.WaitDebugger();

            //收集资源文件
            CollectAssetFile(args);
            
            //执行简化名称功能
            this.DoSimplifiedDisplayName();
            
            //收集文件名中，出现数字的范围
            CollectAssetFileNumberRanage(this.videoFileList);//收集视频文件数字范围
            CollectAssetFileNumberRanage(this.subtitleFileList);//收集字幕文件数字范围


            //初始化范围权重
            this.InitAssetFileRangeWeight(this.videoFileList, this.videoFileRangeWeightList);
            this.InitAssetFileRangeWeight(this.subtitleFileList, this.subtitleFileRangeWeightList);
            
            //初始化资源文件的模板范围文本的数字
            this.initAssetsFileTargetRangeTextNumber(this.videoFileList, this.videoFileRangeWeightList);
            this.initAssetsFileTargetRangeTextNumber(this.subtitleFileList, this.subtitleFileRangeWeightList);

            //对目标视频文件进行排序
            this.SortVideoFile();

            //匹配资源
            this.DoMatchAsset();

            //处理重复引用字幕的情况
            this.DoRepeatReferenceSubtitle();

            //准备执行重命名操作
            this.DoRaname();
        }catch(Exception e){
            Console.WriteLine(e);
        }

        //等待按键退出
        this.WaitKeyExit();
    }

    /// <summary>
    /// 收集资源文件
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private void CollectAssetFile(string [] args){
        this.targetDirectory = null;
        if(args.Length < 1){
            throw new Exception("请指定目录，或者同时指定目标范围内的视频与字幕文件");
        }else{
            string targetDirectory = args[0];
            if(args.Length == 1 && Directory.Exists(targetDirectory)){//如果只指定了一个参数，而且它是一个文件夹
                this.targetDirectory = targetDirectory;
                
                string [] files = Directory.GetFiles(targetDirectory);

                //文件分类
                FileClassification(files, videoFileList, videoFileTypeList);
                FileClassification(files, subtitleFileList, subtitleFileTypeList);
            }else{
                bool allIsFile = true;
                //那么这里就是多选了
                for(int i = 0 ; i < args.Length; i++){
                    if(!File.Exists(args[i])){
                        allIsFile = false;
                        break;
                    }
                }
                if(!allIsFile){
                    throw new Exception("指定了多个参数，但不是所有选择都是文件!");
                }
                
                //如果全是文件，那么需要判断是不是在同一个文件夹下面
                bool sameFolder = true;
                targetDirectory = Path.GetDirectoryName(targetDirectory);
                for(int i = 1 ; i < args.Length; i++){
                    if(targetDirectory != Path.GetDirectoryName(args[i])){
                        sameFolder = false;
                        break;
                    }
                }

                if(!sameFolder){//如果不是相同文件夹
                    throw new Exception("选择的多个文件，不是全在同一个文件夹里面");
                }

                //成功校验
                this.targetDirectory = targetDirectory;
                
                //文件分类
                FileClassification(args, videoFileList, videoFileTypeList);
                FileClassification(args, subtitleFileList, subtitleFileTypeList);
            }
        }
    }

    /// <summary>
    /// 文件分类
    /// </summary>
    private void FileClassification(string [] files, List<AssetFile> assetFileList, List<string> fileTypeList){
        for(int i = 0 ; i < files.Length; i++){
            string fileName = Path.GetFileName(files[i]);
            int dotIndex = fileName.LastIndexOf(".");
            if(dotIndex > -1){
                string fileType = fileName.Substring(dotIndex + 1).ToLower();
                if(fileTypeList.Contains(fileType)){//如果目标资源文件
                    AssetFile assetFile = new AssetFile();
                    assetFile.fileName = fileName;
                    assetFileList.Add(assetFile);
                }
            }
        }
    }

    /// <summary>
    /// 收集资源文件数字范围
    /// </summary>
    private void CollectAssetFileNumberRanage(List<AssetFile> assetFileList){
        //收集文件中数字开始和结束的索引列表
        for (int i = 0; i < assetFileList.Count; i++){
            AssetFile assetFile = assetFileList[i];
            Ranage ranage = null;
            string displayName = assetFile.displayName;

            List<Ranage> numberRanageList = assetFile.numberRanageList;
            numberRanageList.Clear();

            for(int j = 0; j < displayName.Length; j++){
                char c = displayName[j];
                if(c >= '0' && c <= '9'){//如果这个字符是数字
                    if(ranage == null){
                        ranage = new Ranage();
                        ranage.assetFile = assetFile;
                        ranage.start = j;

                    }
                }else{//如果这个字符不是数字
                    if(ranage != null){
                        ranage.end = j;
                        ranage.text = displayName.Substring(ranage.start, ranage.end - ranage.start);
                        numberRanageList.Add(ranage);
                        ranage = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 做重命名动作
    /// </summary>
    private void DoRaname(){
        if(this.assetMatchList.Count < 1){
            Console.WriteLine("没有找到匹配项");
        }else{

            //输出匹配信息
            this.PrintMatchList();

            Console.Write("请确认上方的文件映射关系，确认无误输入Y确定重命名，其他字符取消本次操作，您的选择是？：");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine("\n");//输出一下空白行
            if(keyInfo.KeyChar != 'y' && keyInfo.KeyChar != 'Y'){
                Console.WriteLine("用户放弃本次操作！");
            }else{
                for(int i = 0 ; i < this.assetMatchList.Count; i++){
                    AssetMatch matchAsset = this.assetMatchList[i];
                    AssetFile videoAssetFile = matchAsset.video;
                    AssetFile subtitleAssetFile = matchAsset.subtitle;

                    //字幕文件扩展名
                    string subtitleExtFileName = Path.GetExtension(subtitleAssetFile.fileName);

                    //获取没有扩展名的视频文件名
                    string targetFileName = Path.GetFileNameWithoutExtension(videoAssetFile.fileName);
                    targetFileName += subtitleExtFileName;
                    
                    //原始文件路径
                    string originFilePath = Path.Combine(this.targetDirectory, subtitleAssetFile.fileName);

                    //目标文件路径
                    string targetFilePath = Path.Combine(this.targetDirectory, targetFileName);

                    //重命名字幕文件
                    if(!originFilePath.Equals(targetFilePath, StringComparison.OrdinalIgnoreCase)){//如果确实发生了改变
                        if(!File.Exists(originFilePath)){
                            Console.WriteLine("文件未重命名就已经不存在了：\n视频：{0}\n字幕：{1}\n", videoAssetFile.fileName, subtitleAssetFile.fileName);
                        }else{
                            File.Move(originFilePath, targetFilePath);
                        }
                    }
                }
                Console.WriteLine("任务完成！");
            }
        }
    }

    /// <summary>
    /// 等待按键退出
    /// </summary>
    private void WaitKeyExit(){
        Console.WriteLine("按任意键退出");
        Console.ReadKey();
    }

    /// <summary>
    /// 匹配资源
    /// </summary>
    private void DoMatchAsset(){
        assetMatchList.Clear();

        for(int i = 0 ; i < this.videoFileList.Count; i++){
            AssetFile videoAssetFile = this.videoFileList[i];
            
            //先按照数字匹配
            List<AssetFile> subtitleAssetFileList = null;

            int ? numPtr = videoAssetFile.targetRangeTextNumber;

            //先按照数字匹配
            if(numPtr.HasValue){
                int num = numPtr.Value;
                subtitleAssetFileList = this.subtitleFileList.FindAll( x => x.targetRangeTextNumber == num);
            }
            
            if(subtitleAssetFileList == null || subtitleAssetFileList.Count == 0){//如果数字没有匹配项
                subtitleAssetFileList = new List<AssetFile>();
                AssetFile templateAssetFile = this.videoFileList.Find(x => x.targetRangeTextNumber == 1);//至少有一个 1 吧，用它作为参照
                if(templateAssetFile != null){//如果找到模板了
                    int videoWeightRangeIndex = this.getMaxWeightRangeIndex(this.videoFileRangeWeightList);
                    int subtitleWeightRangeIndex = this.getMaxWeightRangeIndex(this.subtitleFileRangeWeightList);
                    Ranage videoRange = templateAssetFile.numberRanageList[videoWeightRangeIndex];
                    string str1 = videoAssetFile.fileName.Substring(videoRange.start, videoRange.length);
                    for(int j = 0 ; j < this.subtitleFileList.Count; j++){
                        AssetFile subtitle = this.subtitleFileList[j];
                        Ranage subtitleRange = templateAssetFile.numberRanageList[videoWeightRangeIndex];
                        string str2 = subtitle.fileName.Substring(subtitleRange.start, subtitleRange.length);
                        if(str2.Contains(str1)){
                            subtitleAssetFileList.Add(subtitle);
                        }
                    }
                }
            }

            //如果匹配成功才记录匹配数据
            if(subtitleAssetFileList.Count > 0){//如果找到匹配项了
                AssetMatch assetMatch = new AssetMatch();
                assetMatch.video = videoAssetFile;
                assetMatch.subtitleList = subtitleAssetFileList;

                //处理完全匹配
                if(config.perfectMatch.enabled){//如果启用了完全匹配
                    if(subtitleAssetFileList.Count > 1){//如果有多个匹配项
                        this.DoPerfectMatch(assetMatch.video.displayNameSimplified, subtitleAssetFileList);
                    }
                }

                //单匹配 和 多匹配处理
                if(subtitleAssetFileList.Count == 1){//单匹配
                    assetMatch.subtitle = subtitleAssetFileList[0];//直接取得就行了
                }else{//多匹配
                    int index;
                    for(;;){
                        Console.WriteLine("视频：" + videoAssetFile.displayName);
                        for(int j = 0 ; j < subtitleAssetFileList.Count; j++){
                            Console.WriteLine("编号：{0} -> 字幕：{1}", j, subtitleAssetFileList[j].displayName);
                        }
                        Console.Write("视频出现同时多个字幕匹配，请选择目标字幕文件的编号：");
                        
                        Console.Write("请输入您的选择，并按回车键确认，您的选择：");
                        string line = Console.ReadLine();
                        Console.WriteLine();

                        if(int.TryParse(line, out index)){
                            if(index < 0 || index > subtitleAssetFileList.Count){
                                Console.WriteLine("输出错误，请输入正确的编号！");
                            }else{
                                assetMatch.subtitle = subtitleAssetFileList[index];
                                break;
                            }
                        }
                    }
                }
                assetMatchList.Add(assetMatch);
            }
        }
    }

    /// <summary>
    /// 处理重复引用字幕的情况
    /// </summary>
    private void DoRepeatReferenceSubtitle(){
        //收集重复引用了相同字幕文件的视频
        Dictionary<string, List<AssetMatch>> map = new Dictionary<string, List<AssetMatch>>();
        for(int i = 0 ; i < assetMatchList.Count - 1; i++){
            AssetMatch match1 = assetMatchList[i];
            List<AssetMatch> list = null;
            string displayNameSimplified = match1.subtitle.displayNameSimplified;
            if(map.ContainsKey(displayNameSimplified)){
                list = map[displayNameSimplified];
                if(list.Contains(match1)){//如果当前文件已经被排除掉了
                    continue;//那么直接开始下一个文件
                }
            }
            for(int j = i + 1 ; j < assetMatchList.Count; j++){
                AssetMatch match2 = assetMatchList[j];
                if(displayNameSimplified.Equals(match2.subtitle.displayNameSimplified)){
                    if(!map.ContainsKey(displayNameSimplified)){
                        list = new List<AssetMatch>();
                        list.Add(match1);
                        map[displayNameSimplified] = list;
                    }
                    list.Add(match2);
                }
            }
        }

        //完全匹配处理
        if(config.perfectMatch.enabled){//如果启用了完全匹配
            if(map.Count > 0){//如果确实出现重复引用的情况了
                List<string> removeKeyList = new List<string>();
                //开始处理
                foreach(KeyValuePair<string,List<AssetMatch>> entry in map){
                    List<AssetMatch> matchList = entry.Value;
                    List<AssetFile> matchAssetFileList = matchList.ConvertAll(x => x.video);

                    this.DoPerfectMatch(entry.Key, matchAssetFileList);

                    //排除掉不要的东西
                    for(int i = 0 ; i < matchList.Count; i++){
                        AssetMatch assetMatch = matchList[i];
                        if(!matchAssetFileList.Contains(assetMatch.video)){//如果这个匹配已经不存在了
                            assetMatchList.Remove(assetMatch);
                            matchList.RemoveAt(i);
                        }
                    }

                    if(matchList.Count == 1){//如只剩下一个匹配了
                        removeKeyList.Add(entry.Key);
                    }
                }

                //移除掉只剩下一个匹配的项
                for(int i = 0 ; i < removeKeyList.Count; i++){
                    map.Remove(removeKeyList[i]);
                }
            }
        }


        //单匹配 和 多匹配处理
        if(map.Count > 0){//如果确实出现重复引用的情况了
            //通知用户进入处理程序
            for(;;){
                Console.WriteLine("重复引用了字幕文件，即将进入处理程序，请按 y 键进入该程序！");
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine("\n");//输出一下空白行
                if(keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'Y'){
                    break;
                }
            }

            //开始处理
            foreach(KeyValuePair<string,List<AssetMatch>> entry in map){
                int index;
                for(;;){
                    Console.WriteLine("重复引用了字幕文件，请选择字幕匹配的视频文件编号。");
                    Console.WriteLine("当前字幕文件：{0}", entry.Key);
                    List<AssetMatch> matchList = entry.Value;
                    for(int i = 0 ; i < matchList.Count; i++){
                        AssetMatch match = matchList[i];
                        Console.WriteLine("编号：{0} -> 字幕：{1}", i,  match.video.displayName);
                    }

                    Console.Write("请输入您的选择，并按回车键确认，您的选择：");
                    string line = Console.ReadLine();
                    Console.WriteLine();

                    if(int.TryParse(line, out index)){
                        if(index < 0 || index > matchList.Count){
                            Console.WriteLine("输出错误，请输入正确的编号！");
                        }else{
                            AssetMatch match = matchList[index];
                            
                            //待移除的列表中排除用户要保留的设置
                            matchList.Remove(match);

                            //移除掉不要的
                            for(int i = 0 ; i < matchList.Count; i++){
                                assetMatchList.Remove(matchList[i]);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 处理完全匹配
    /// </summary>
    /// <param name="displayNameSimplified"></param>
    /// <param name="matchAssetFileList"></param>
    private void DoPerfectMatch(string displayNameSimplified, List<AssetFile> matchAssetFileList){
        if(displayNameSimplified.Length >= config.perfectMatch.count){//如果显示名称大于等于要求的字符数
            int count = 0;
            AssetFile assetFile = null;
            for(int j = 0 ; j < matchAssetFileList.Count; j++){
                AssetFile item = matchAssetFileList[j];
                if(item.displayNameSimplified == displayNameSimplified){
                    assetFile = item;
                    count++;
                }
            }

            if(count == 1){//如果只有一个完全匹配项
                matchAssetFileList.Clear();//排除掉其他可能
                matchAssetFileList.Add(assetFile);//记录当前的
            }
        }
    }

    /// <summary>
    /// 输出匹配信息
    /// </summary>
    private void PrintMatchList(){
        //输出顺序
        for(int i = 0 ; i < this.assetMatchList.Count; i++){
            //取得显示名称
            string videoDisplayName = Path.GetFileName(this.assetMatchList[i].video.displayName);
            string subtitleDisplayName = Path.GetFileName(this.assetMatchList[i].subtitle.displayName);

            //输出名字
            Console.WriteLine("{0} -> {1}", videoDisplayName, subtitleDisplayName);
        }
    }

    /// <summary>
    /// 等待附加 - 当前这个是一个模拟的
    /// </summary>
    private void WaitDebugger(){
        Console.WriteLine("请在调试器附加后，输入按键 k 进入程序！");
        while('k' != Console.ReadKey().KeyChar){

        }
        Console.WriteLine();//输出一个空白行
    }

    /// <summary>
    /// 执行名称简化操作
    /// </summary>
    private void DoSimplifiedDisplayName(){
        int videoLeft = 0, videoRight = 0;
        int subtitleLeft = 0, subtitleRight = 0;

        //转化数据类型
        List<string> videoFileNameList = this.videoFileList.ConvertAll(x => x.fileName);
        List<string> subtitleFileNameList = this.subtitleFileList.ConvertAll(x => x.fileName);

        //收集简化的显示名称
        this.CollectSimplifiedDisplayName(videoFileNameList, out videoLeft, out videoRight);
        this.CollectSimplifiedDisplayName(subtitleFileNameList, out subtitleLeft, out subtitleRight);
        
        //视频
        for(int i = 0 ; i < this.videoFileList.Count; i++){
            AssetFile assetFile = this.videoFileList[i];
            assetFile.displayNameSimplified = this.SimplifiedDisplayName(assetFile.fileName, videoLeft, videoRight);
            assetFile.CalcDisplay();
        }
        
        //字幕
        for(int i = 0 ; i < this.subtitleFileList.Count; i++){
            AssetFile assetFile = this.subtitleFileList[i];
            assetFile.displayNameSimplified = this.SimplifiedDisplayName(assetFile.fileName, subtitleLeft, subtitleRight);
            assetFile.CalcDisplay();
        }
    }

    /// <summary>
    /// 简化名称
    /// </summary>
    /// <param name="fileName">原始名称</param>
    /// <param name="left">左边移除的数量</param>
    /// <param name="right">右边简化的数量</param>
    /// <returns></returns>
    private string SimplifiedDisplayName(string fileName, int left, int right){
        //计算出短名字
        string fileNameShort = fileName;

        //简化后占位显示的部分
        string simplifiedSymal = "~~~";

        if(left > simplifiedSymal.Length){
            fileNameShort = fileNameShort.Substring(left);
        }

        if(right > simplifiedSymal.Length){
            fileNameShort = fileNameShort.Substring(0, fileNameShort.Length - right);
        }

        if(left > 0){
            fileNameShort = simplifiedSymal +  fileNameShort;
        }
        if(right > 0){
            fileNameShort = fileNameShort + simplifiedSymal;
        }
        return fileNameShort;
    }

    /// <summary>
    /// 收集简化的显示名称
    /// </summary>
    /// <param name="list"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private void CollectSimplifiedDisplayName(List<string> list, out int left, out int right){
        left = 0;
        right = 0;

        if(list.Count > 1){//布置一个文件的时候才有缩减的条件
            //获得最大的字符串长度
            int length = 0;
            for(int i = 0 ; i < list.Count; i++){
                if(list[i].Length > length){
                    length = list[i].Length;
                }
            }

            if(length > 0){//如果有内容，那么才有必要继续执行
                //查找完成的标志
                bool leftEQFinshed = false;
                bool rightEQFinshed = false;

                for(int n = 0 ; n < length; n++){
                    //left
                    if(!leftEQFinshed){
                        for(int i = 0 ; i < list.Count - 1; i++){
                            string item1 = list[i];
                            if(item1.Length <= left){//如果长度小于目标
                                break;//那么就是已经是结束了
                            }

                            char c1 = item1[left];
                            for(int j = i + 1 ; j < list.Count; j++){
                                string item2 = list[j];
                                if(item1.Length <= left){//如果长度小于目标
                                    leftEQFinshed = true;
                                    break;//那么就是已经是结束了
                                }

                                char c2 = item2[left];
                                if(c1 != c2){
                                    leftEQFinshed = true;
                                    break;
                                }
                            }

                            if(leftEQFinshed){
                                break;
                            }
                        }

                        //索引项
                        if(!leftEQFinshed){
                            left++;
                        }
                    }

                    //right
                    if(!rightEQFinshed){
                        for(int i = 0 ; i < list.Count - 1; i++){
                            string item1 = list[i];
                            if(item1.Length <= right){//如果长度小于目标
                                break;//那么就是已经是结束了
                            }

                            char c1 = item1[item1.Length - right - 1];
                            for(int j = i + 1 ; j < list.Count; j++){
                                string item2 = list[j];
                                if(item1.Length <= right){//如果长度小于目标
                                    rightEQFinshed = true;
                                    break;//那么就是已经是结束了
                                }

                                char c2 = item2[item2.Length - right - 1];
                                if(c1 != c2){
                                    rightEQFinshed = true;
                                    break;
                                }
                            }

                            if(rightEQFinshed){
                                break;
                            }

                        }

                        //索引项
                        if(!rightEQFinshed){
                            right++;
                        }
                    }

                    
                    //判定任务是否已经完成了
                    if(leftEQFinshed && rightEQFinshed){//如果两个任务都完成了
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 初始化资源文件的模板范围文本的数字
    /// </summary>
    private void initAssetsFileTargetRangeTextNumber(List<AssetFile> assetFileList, List<int> rangeWeightList){
        //取得目标权重索引
        int videoFileRangeIndex = this.getMaxWeightRangeIndex(rangeWeightList);

        //提取目标数字
        for(int i = 0 ; i < assetFileList.Count; i++){
            AssetFile assetFile = assetFileList[i];
            int ? targetRangeTextNumber = null;//不能识别的都放置在最后

            if(assetFile.numberRanageList.Count > videoFileRangeIndex){
                Ranage ranage = assetFile.numberRanageList[videoFileRangeIndex];
                targetRangeTextNumber = Convert.ToInt32(ranage.text);
            }

            assetFile.targetRangeTextNumber = targetRangeTextNumber;   
        }
    }

    /// <summary>
    /// 对目标视频文件进行排序
    /// </summary>
    private void SortVideoFile(){
        //根据数字对视频文件进行排序
        this.videoFileList.Sort((x, y) =>{
            if(x.targetRangeTextNumber == null){
                return 1;
            } else if(y.targetRangeTextNumber == null){
                return -1;
            }else{
                return x.targetRangeTextNumber.Value.CompareTo(y.targetRangeTextNumber.Value);
            }
        });
    }

    /// <summary>
    /// 获取权重最大的范围索引
    /// </summary>
    /// <returns></returns>
    private int getMaxWeightRangeIndex(List<int> rangeWeightList){
        //选取权重最大的范围的索引
        int count = 0;
        int index = 0;
        for(int i = 0 ; i < rangeWeightList.Count; i++){
            int c = rangeWeightList[i];
            if(c > count){
                count = c;
                index = i;
            }
        }
        return index;
    }

    /// <summary>
    /// 初始化范围权重
    /// </summary>
    private void InitAssetFileRangeWeight(List<AssetFile> assetFileList, List<int> rangeWeightList){
        //取得权重个数最大的值
        //ranage list 索引相同，但是 start end 之间数字不同的，权重值+1，数字相同的，权重-1
        int maxWeightCount = 0;//计算出最大权重
        for(int i = 0; i < assetFileList.Count; i++){
            AssetFile videoFile = assetFileList[i];
            int weight = videoFile.numberRanageList.Count;
            if(weight > maxWeightCount){
                maxWeightCount = weight;
            }
        }

        //初始化权重
        rangeWeightList.Clear();
        for(int i = 0; i < maxWeightCount; i++){
            rangeWeightList.Add(0);
        }

        //便利视频文件中包含数字的权重列表
        for(int i = 0; i < assetFileList.Count - 1; i++){
            AssetFile assetFile = assetFileList[i];
            for(int j = 0; j < assetFileList.Count; j++){
                AssetFile assetFile2 = assetFileList[j];

                int count = assetFile.numberRanageList.Count;
                int count2 = assetFile2.numberRanageList.Count;

                List<Ranage> listMin, listMax;
                if(count > count2){
                    listMax = assetFile.numberRanageList;
                    listMin = assetFile2.numberRanageList;
                }else{
                    listMin = assetFile.numberRanageList;
                    listMax = assetFile2.numberRanageList;
                }

                //遍历相同索引的范围
                for(int k = 0; k < listMin.Count; k++){
                    Ranage ranage1 = listMin[k];
                    Ranage ranage2 = listMax[k];
                    if(ranage1.text != ranage2.text){//如果这个索引项中的内容不一样
                        rangeWeightList[k]++;//那么权重加1
                    }
                }
            }
        }
    }
}