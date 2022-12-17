# 字幕文件重命名

>主要用于重命名番剧的外挂字幕文件

>字幕下载网站下载的字幕文件经常跟下载到的生肉番剧的文件名不一致，需要手动指定视频字幕，下次打开又得重新指定。

>或者是直接修改字幕文件的名字跟视频文件保持一致。但是重命名操作太机械化，且麻烦。

>所以我就花了点时间做了这个工具，可以简单识别文件名中数字的顺序，将视频和字幕做匹配，然后有选择的进行批量重名。

## 使用前提条件
>依赖.netCore运行环境，您可以从这里下载：[https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

## 使用方法1 -> 拖住文件夹 - 重命名整个文件夹

>选择需要重命名的文件所在的文件夹

>鼠标按住不放，拖到 RenameSubtitles.bat 文件上面放开鼠标

## 使用方法2 -> 拖住文件 - 只重命名选择的部分

>选择需要重命名的视频文件列表，以及它们匹配的字幕文件

>鼠标按住不放，拖到 RenameSubtitles.bat 文件上面放开鼠标

## 编译
``` shell
dotnet build
```

## 参与开发

>欢迎提交任意有益的合并请求
